using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using Infin8.Coapp.Utility.Enums;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>
    /// ECS Loan Disbursement (spec 10). One run = the ticked status-E sanctions → one voc_type 14 voucher,
    /// a new loan per sanction, every recoverable posted (loan_trn R / mem_trn / deposit_trn / ledger lines),
    /// one fin_voucher_bank row per member and the sanctions moved to status C — in one transaction.
    /// </summary>
    public class EcsLoanDisbursementHandler : IEcsLoanDisbursementHandler
    {
        private const double Tolerance = 0.005;
        private const int VocTypeBatchDisbursement = 14;
        private const int VocTrnTypeAdjustment = 2;

        readonly IUnitOfWork _unitOfWork;

        public EcsLoanDisbursementHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<List<EcsBankLedgerVM>> GetBankLedgersAsync(string brCode) =>
            _unitOfWork.EcsLoanDisbursement.GetBankLedgersAsync(brCode);

        public async Task<EcsDisbursementPreviewVM> PreviewAsync(EcsDisbursementRequestVM request, EcsUserContextVM ctx)
        {
            var build = await BuildAsync(request, ctx, null);
            return build.Preview;
        }

        public async Task<EcsDisbursementResultVM> DisburseAsync(EcsDisbursementRequestVM request, EcsUserContextVM ctx)
        {
            _unitOfWork.BeginTransaction();
            try
            {
                // Ids are read inside the transaction; sanctions are re-read and re-validated here too.
                var alloc = await _unitOfWork.EcsLoanDisbursement.CreateIdAllocatorAsync(ctx.BrCode);
                var build = await BuildAsync(request, ctx, alloc);
                if (!build.Preview.IsValid || build.Posting == null)
                {
                    _unitOfWork.RollBack();
                    return new EcsDisbursementResultVM
                    {
                        Success = false,
                        Message = build.Preview.Messages.FirstOrDefault() ?? "Disbursement not valid.",
                        Reasons = build.Preview.Messages
                    };
                }

                await _unitOfWork.EcsLoanDisbursement.AddPostingAsync(build.Posting, ctx.BrCode);
                await _unitOfWork.CompleteAsync();
                _unitOfWork.CommitTransaction();

                return new EcsDisbursementResultVM
                {
                    Success = true,
                    Message = $"{build.Loans.Count} loan(s) disbursed. Voucher {build.Posting.Voucher.Voc_No}.",
                    VocId = build.Posting.VocId,
                    VocNo = build.Posting.Voucher.Voc_No,
                    Loans = build.Loans
                };
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                var baseMsg = ex.GetBaseException().Message;
                if (baseMsg.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) || baseMsg.Contains("23505"))
                    throw new InvalidOperationException(
                        "Another user posted at the same time. Nothing was disbursed — please try again.");
                throw new InvalidOperationException(ex.Message + " Something went wrong! Nothing was disbursed.");
            }
        }

        // =====================================================================
        // Build (shared by Preview and Disburse)
        // =====================================================================

        private sealed class BuildResult
        {
            public EcsDisbursementPreviewVM Preview { get; } = new();
            public EcsDisbursementPosting? Posting { get; set; }
            public List<EcsDisbursedLoanVM> Loans { get; } = new();
        }

        /// <param name="alloc">null for a preview — ids stay 0 and nothing is tracked.</param>
        private async Task<BuildResult> BuildAsync(EcsDisbursementRequestVM req, EcsUserContextVM ctx,
            EcsDisbursementIdAllocator? alloc)
        {
            var result = new BuildResult();
            var pv = result.Preview;
            var msg = pv.Messages;
            var repo = _unitOfWork.EcsLoanDisbursement;
            var brCode = ctx.BrCode;
            var date = ctx.CurrentDate.Date;
            pv.VoucherDate = date;

            // --- 1. Context (§7.1–7.2, §8.4) ---
            if (string.IsNullOrEmpty(brCode)) { msg.Add("Session has no branch. Please log in again."); return result; }

            var openDay = await repo.GetOpenBusinessDayAsync(brCode);
            if (openDay == null)
                msg.Add("Day Begin has not been done for this branch.");
            else if (openDay.Value.Date != date)
                msg.Add($"The business day is now {openDay.Value:dd-MM-yyyy} but this session is on {date:dd-MM-yyyy}. Log out and log in again.");
            if (ctx.YrBeginningDate > DateTime.MinValue && ctx.YrEndDate > DateTime.MinValue
                && (date < ctx.YrBeginningDate.Date || date > ctx.YrEndDate.Date))
                msg.Add("The business day is outside the current financial year.");

            var ids = (req.SanctionIds ?? new()).Where(i => i > 0).Distinct().ToList();
            if (ids.Count == 0) { msg.Add("Select at least one sanction."); return result; }

            // --- 2. Payment (§7.6) ---
            var mode = (req.Mode ?? "").Trim().ToUpperInvariant();
            if (mode != "C" && mode != "N") msg.Add("Choose Cheque or NEFT.");
            bool isCheque = mode == "C";
            var instrumentNo = (req.InstrumentNo ?? "").Trim();
            if (instrumentNo.Length == 0) msg.Add(isCheque ? "Enter the cheque no." : "Enter the NEFT reference.");
            if (instrumentNo.Length > 50) msg.Add("Cheque no. / NEFT reference is too long.");
            var instrumentDate = req.InstrumentDate.Date;
            if (req.InstrumentDate == DateTime.MinValue) msg.Add("Enter the cheque / NEFT date.");
            else if (instrumentDate > date) msg.Add("Cheque / NEFT date cannot be after the business day.");

            var bankLedgers = await repo.GetBankLedgersAsync(brCode);
            var bank = bankLedgers.FirstOrDefault(b => b.LedId == req.BankLedId);
            if (bank == null) msg.Add("Select a bank ledger of this branch.");
            pv.BankLedgerName = bank?.LedName;
            if (bank != null && isCheque && instrumentNo.Length > 0
                && await repo.IsChequeNoUsedAsync(bank.LedId, instrumentNo, brCode))
                pv.Warnings.Add($"Cheque no. {instrumentNo} is already used on another voucher of {bank.LedName}.");

            // --- 3. Load ---
            var sanctions = await repo.GetSanctionsAsync(ids, brCode);
            foreach (var id in ids.Where(i => sanctions.All(s => s.LoanSanction_Id != i)))
                msg.Add($"Sanction {id} was not found.");
            foreach (var s in sanctions.Where(s => s.LoanSanction_Status != "E"))
                msg.Add($"Sanction {s.LoanSanction_Id} is not awaiting disbursement (already disbursed?).");
            sanctions = sanctions.Where(s => s.LoanSanction_Status == "E")
                                 .OrderBy(s => ids.IndexOf(s.LoanSanction_Id)).ToList();
            if (sanctions.Count == 0) return result;

            var lines = await repo.GetRecoverableLinesAsync(sanctions.Select(s => s.LoanSanction_Id), brCode);
            var schemes = await repo.GetSchemesAsync(sanctions.Select(s => s.Scheme_Id), brCode);
            var members = await repo.GetMembersAsync(
                sanctions.SelectMany(s => new[] { s.Mem_Id, s.SuretyMem_Id }).Concat(lines.Select(l => l.Mem_Id)), brCode);
            var ledgerIds = lines.Select(l => l.Led_Id)
                .Concat(schemes.Values.Select(s => s.PrlLed_Id))
                .Append(req.BankLedId);
            var ledgers = await repo.GetLedgersAsync(ledgerIds, brCode);
            var recoveredLoanIds = lines.Where(l => l.Loan_Id > 0 && IsLoanStatus(l.Status)).Select(l => l.Loan_Id).Distinct().ToList();
            var loanNos = await repo.GetLoanNumbersAsync(recoveredLoanIds, brCode);
            var prlOs = await repo.GetPrincipalOutstandingAsync(recoveredLoanIds, brCode);
            var depositLedgers = await repo.GetDepositLedgersAsync(brCode);
            var shareCapitalLedId = await repo.GetShareCapitalLedgerAsync(brCode);

            // --- 4. Per-sanction validation (§7.3–7.5, §7.7, Q7) ---
            var loanOwners = new Dictionary<decimal, decimal>();   // recovered loan → sanction
            foreach (var s in sanctions)
            {
                var sid = s.LoanSanction_Id;
                var sLines = lines.Where(l => l.LoanSanction_Id == sid).ToList();
                var sDate = s.SanctionDate?.Date;

                if (sDate == null || sDate > date)
                    msg.Add($"Sanction {sid}: sanction date is after the business day.");
                if (sDate != null && sDate != date && sLines.Any(l => IsLoanStatus(l.Status)))
                    msg.Add($"Sanction {sid} was made on {sDate:dd-MM-yyyy}; the interest it recovers is stale. " +
                            "Modify the sanction (it re-dates to today) and disburse.");

                foreach (var m in await _unitOfWork.EcsLoanSanction.GetSubsequentTransactionMessagesAsync(sid, brCode))
                    msg.Add($"Sanction {sid}: {m} — modify the sanction first.");

                if (s.LoanAmount <= 0) msg.Add($"Sanction {sid}: loan amount is zero.");
                var sumLines = Math.Round(sLines.Sum(l => l.ReceiptItem), 2);
                if (Math.Abs(sumLines - Math.Round(s.TotalReceiptAmount, 2)) > Tolerance)
                    msg.Add($"Sanction {sid}: recoverable lines ({sumLines:N2}) do not match the total recoverable ({s.TotalReceiptAmount:N2}).");
                if (Math.Round(s.LoanAmount - s.TotalReceiptAmount, 2) <= 0)
                    msg.Add($"Sanction {sid}: nothing is left to pay after recoveries.");

                if (!schemes.TryGetValue(s.Scheme_Id, out var scheme))
                    msg.Add($"Sanction {sid}: loan scheme {s.Scheme_Id} was not found.");
                else
                {
                    if (scheme.Loan_Type != (int)LoanCategory.ThriftCreditLoans
                        || !LoanCategoryPolicy.IsAllowed((LoanCategory)scheme.Loan_Type, InstitutionType.ECS))
                        msg.Add($"Sanction {sid}: scheme {scheme.Scheme_Name} is not an ECS loan scheme.");
                    if (scheme.PrlLed_Id <= 0 || !ledgers.ContainsKey(scheme.PrlLed_Id))
                        msg.Add($"Sanction {sid}: scheme {scheme.Scheme_Name} has no principal ledger.");
                }

                foreach (var l in sLines)
                {
                    if (l.Led_Id <= 0 || !ledgers.ContainsKey(l.Led_Id))
                        msg.Add($"Sanction {sid}: recoverable '{l.ReceiptDescription}' has no valid ledger.");
                    if (IsLoanStatus(l.Status))
                    {
                        if (l.Loan_Id <= 0) msg.Add($"Sanction {sid}: recoverable '{l.ReceiptDescription}' has no loan.");
                        else if (loanOwners.TryGetValue(l.Loan_Id, out var other) && other != sid)
                            msg.Add($"Loan {loanNos.GetValueOrDefault(l.Loan_Id)} is recovered by both sanction {other} and {sid}; disburse them separately.");
                        else loanOwners[l.Loan_Id] = sid;
                    }
                    if (l.Status == "D" && (l.DM_Id <= 0 || !depositLedgers.ContainsKey(l.DM_Id)))
                        msg.Add($"Sanction {sid}: deposit recoverable '{l.ReceiptDescription}' has no deposit.");
                    if ((l.Status == "MS" || l.Status == "SS") && l.Mem_Id <= 0)
                        msg.Add($"Sanction {sid}: share-capital recoverable has no member.");
                }
            }

            // --- 5. Build (§5) ---
            decimal zeroId = decimal.TryParse(brCode + "0000000", out var z) ? z : 0;
            string Narr(decimal memId)
            {
                if (!members.TryGetValue(memId, out var m)) return "";
                return $"{(string.IsNullOrWhiteSpace(m.PerNo) ? m.MemberNo : m.PerNo)} {m.Name}".Trim();
            }
            string? LedName(decimal ledId) => ledgers.TryGetValue(ledId, out var l) ? l.Name : null;
            (string? no, string? name) Mem(decimal memId) =>
                members.TryGetValue(memId, out var m) ? (m.MemberNo, m.Name) : (null, null);

            var posting = new EcsDisbursementPosting { Date = date };
            int rptSlNo = 0, pmtSlNo = 0;
            if (alloc != null) (rptSlNo, pmtSlNo) = await repo.GetNextVoucherSlNosAsync(ctx.YrId, brCode);
            var vocId = alloc?.NextVocId() ?? 0;
            var rptNo = alloc != null ? $"AR/{rptSlNo}" : "AR/—";
            var pmtNo = alloc != null ? $"VR/{pmtSlNo}" : "VR/—";
            var instrumentText = isCheque ? $"Cheque No {instrumentNo}" : $"NEFT Ref {instrumentNo}";
            var narration = string.IsNullOrWhiteSpace(req.Narration) ? "ECS Loan Disbursement" : req.Narration.Trim();

            posting.Voucher = new Fin_Voucher
            {
                Voc_Id = vocId,
                Voc_SlNo = 0,
                Voc_Date = date,
                Voc_Type = VocTypeBatchDisbursement,
                Voc_Mode = "",
                Voc_Rpt_SlNo = rptSlNo,
                Voc_Rpt_No = rptNo,
                Voc_Rpt_Mode = isCheque ? "QR" : "AR",
                Voc_Pmt_SlNo = pmtSlNo,
                Voc_Pmt_No = pmtNo,
                Voc_Pmt_Mode = isCheque ? "PQ" : "VR",
                Voc_No = $"{pmtNo}-{rptNo}",
                Voc_Amt = Math.Round(sanctions.Sum(s => s.LoanAmount), 2),
                Voc_Narration = $"{narration} {instrumentText}".Trim(),
                Mem_Id = sanctions.Count == 1 ? sanctions[0].Mem_Id : zeroId,
                Voc_Bus = true,
                Voc_Delete = false,
                Voc_Status = "V",
                Usr_Id = ctx.UsrId,
                Yr_Id = ctx.YrId,
                BrCode = brCode
            };

            void AddVoucherLine(decimal sid, decimal ledId, double rpt, double pmt, string status, decimal memId,
                decimal loanId, string narr)
            {
                posting.VoucherTrns.Add(new Fin_Voucher_Trn
                {
                    Voc_Trn_Id = alloc?.NextVocTrnId() ?? 0,
                    Voc_Id = vocId,
                    Led_Id = ledId,
                    Voc_Rpt = Math.Round(rpt, 2),
                    Voc_Pmt = Math.Round(pmt, 2),
                    Voc_Trn_Type = VocTrnTypeAdjustment,
                    Voc_Narr = narr,
                    Voc_Cash_Adj_Id = 0,
                    FinVocTr_Delete = false,
                    Status = status,
                    Description = "",
                    Mem_Id = memId,
                    Loan_Id = loanId,
                    Usr_Id = ctx.UsrId,
                    Yr_Id = ctx.YrId,
                    BrCode = brCode,
                    Voc_Status = "V"
                });
                var (mNo, mName) = Mem(memId);
                pv.Lines.Add(new EcsVoucherLineVM
                {
                    SanctionId = sid, Status = status, LedId = ledId, LedgerName = LedName(ledId),
                    MemberNo = mNo, MemberName = mName, Narration = narr,
                    Debit = Math.Round(pmt, 2), Credit = Math.Round(rpt, 2)
                });
            }

            foreach (var s in sanctions)
            {
                var sid = s.LoanSanction_Id;
                schemes.TryGetValue(s.Scheme_Id, out var scheme);
                if (scheme == null) continue;   // already reported
                var sLines = lines.Where(l => l.LoanSanction_Id == sid).ToList();
                var loanAmount = Math.Round(s.LoanAmount, 2);
                var net = Math.Round(s.LoanAmount - s.TotalReceiptAmount, 2);

                // 5.2 — the new loan
                decimal loanId = 0;
                string loanNo = "(new)";
                if (alloc != null && msg.Count == 0)
                {
                    loanId = alloc.NextLoanId();
                    loanNo = await alloc.NextLoanNoAsync(scheme);
                }
                posting.Loans.Add(new Loan_Master
                {
                    Loan_Id = loanId,
                    Scheme_Id = s.Scheme_Id,
                    Loan_No = loanNo,
                    Mem_Id = s.Mem_Id,
                    App_Id = 0,
                    Res_Date = s.SanctionDate?.Date,
                    San_Amt = loanAmount,
                    San_Date = s.SanctionDate?.Date ?? date,
                    Loan_Type = scheme.Loan_Type,
                    Loan_Status = 1,
                    Pur_Id = 1,
                    Prl_Prd = s.PeriodOfLoan,
                    Int_Prd = 0,
                    FirstPrl_DueDate = s.FirstDueDate?.Date,
                    FirstInt_DueDate = s.FirstDueDate?.Date,
                    Roi = s.RateOfInterest,
                    Pi = s.PenalRate,
                    Inst_Amt = s.InstalmentAmount,
                    LastIntApplication_Date = date,
                    Loan_Oe = false,
                    Loan_Delete = false,
                    IsAccountClosed = false,
                    Voc_Id = vocId,
                    Usr_Id = ctx.UsrId,
                    Yr_Id = ctx.YrId,
                    BrCode = brCode,
                    Voc_Status = "V"
                });
                posting.Disbursements.Add(new Loan_Disb
                {
                    Disb_Id = alloc?.NextDisbId() ?? 0,
                    Loan_Id = loanId,
                    Disb_Date = date,
                    SocDisb_No = 1,
                    SocDisb_Amt = loanAmount,
                    SocDisb_Date = date,
                    SocDisbCheque_No = instrumentNo,
                    SocDisbCheque_Date = instrumentDate,
                    Reim_Amt = 0,
                    Disb_Oe = false,
                    Disb_SlNo = 1,
                    Is_FinalDisbursement = true,
                    LoanDisb_Delete = false,
                    Voc_Id = vocId,
                    Usr_Id = ctx.UsrId,
                    Yr_Id = ctx.YrId,
                    BrCode = brCode,
                    Voc_Status = "V"
                });
                posting.Instalments.Add(new Loan_Inst
                {
                    Inst_Id = alloc?.NextInstId() ?? 0,
                    Loan_Id = loanId,
                    Inst_Wef = date,
                    Inst_Amt = s.InstalmentAmount,
                    Inst_Oe = false,
                    Inst_Delete = false,
                    Voc_Id = vocId,
                    Usr_Id = ctx.UsrId,
                    Yr_Id = ctx.YrId,
                    BrCode = brCode,
                    Voc_Status = "V"
                });
                posting.Rois.Add(new Loan_Roi
                {
                    Roi_Id = alloc?.NextRoiId() ?? 0,
                    Loan_Id = loanId,
                    Agency = "S",
                    Roi_Wef = date,
                    Roi = s.RateOfInterest,
                    Pi = s.PenalRate,
                    Prd = 0,
                    InstallmentAmount = 0,
                    Loanroi_Delete = false,
                    LoanRoi_Oe = false,
                    Voc_Id = vocId,
                    Usr_Id = ctx.UsrId,
                    Yr_Id = ctx.YrId,
                    BrCode = brCode,
                    Voc_Status = "V"
                });
                posting.LoanTrns.Add(new Loan_Trn
                {
                    Trn_Id = alloc?.NextLoanTrnId() ?? 0,
                    Loan_Id = loanId,
                    Demand_Id = zeroId,
                    Trn_Status = "I",
                    Trn_Date = date,
                    Disb_Date = date,
                    Disb_Amt = loanAmount,
                    IntCalc_Date = date,
                    Soc_Roi = s.RateOfInterest,
                    SocPI_Rate = s.PenalRate,
                    Trn_SlNo = alloc != null && loanId > 0 ? await alloc.NextLoanTrnSlNoAsync(loanId) : 1,
                    TrnTr_Oe = false,
                    TrnTr_Delete = false,
                    Voc_Id = vocId,
                    Usr_Id = ctx.UsrId,
                    Yr_Id = ctx.YrId,
                    BrCode = brCode,
                    Voc_Status = "V"
                });
                AddVoucherLine(sid, scheme.PrlLed_Id, 0, loanAmount, "LA", s.Mem_Id, loanId,
                    $"{Narr(s.Mem_Id)} Loan No {loanNo}");

                // 5.3 — recoverables. Loan lines first, merged per recovered loan into one loan_trn R row.
                foreach (var g in sLines.Where(l => IsLoanStatus(l.Status) && l.Loan_Id > 0).GroupBy(l => l.Loan_Id))
                {
                    var oldLoanId = g.Key;
                    var oldLoanNo = loanNos.GetValueOrDefault(oldLoanId) ?? oldLoanId.ToString();
                    double p = Math.Round(g.Where(l => l.Status == "P").Sum(l => l.ReceiptItem), 2);
                    double i = Math.Round(g.Where(l => l.Status == "I").Sum(l => l.ReceiptItem), 2);
                    double pi = Math.Round(g.Where(l => l.Status == "PI").Sum(l => l.ReceiptItem), 2);
                    posting.LoanTrns.Add(new Loan_Trn
                    {
                        Trn_Id = alloc?.NextLoanTrnId() ?? 0,
                        Loan_Id = oldLoanId,
                        Demand_Id = zeroId,
                        Trn_Status = "R",
                        Trn_Date = date,
                        PrlColl_Amt = p,
                        IntColl_Amt = i,
                        PIColl_Amt = pi,
                        Trn_SlNo = alloc != null ? await alloc.NextLoanTrnSlNoAsync(oldLoanId) : 0,
                        TrnTr_Oe = false,
                        TrnTr_Delete = false,
                        Voc_Id = vocId,
                        Usr_Id = ctx.UsrId,
                        Yr_Id = ctx.YrId,
                        BrCode = brCode,
                        Voc_Status = "V"
                    });
                    if (p > 0 && prlOs.TryGetValue(oldLoanId, out var os) && os - p <= Tolerance)
                        posting.LoansToClose.Add(oldLoanId);

                    foreach (var l in g)
                        AddVoucherLine(sid, l.Led_Id, l.ReceiptItem, 0, l.Status!, s.Mem_Id, oldLoanId,
                            $"{Narr(s.Mem_Id)} Loan No {oldLoanNo}");
                }

                foreach (var l in sLines.Where(l => !IsLoanStatus(l.Status)))
                {
                    var amount = Math.Round(l.ReceiptItem, 2);
                    var lineMemId = l.Mem_Id > 0 ? l.Mem_Id : s.Mem_Id;
                    var status = string.IsNullOrWhiteSpace(l.Status) ? "G" : l.Status!;

                    switch (status)
                    {
                        case "MS":
                        case "SS":
                            await AddMemTrnAsync(posting, alloc, ctx, vocId, date, lineMemId, 3,
                                shareCapitalLedId > 0 ? shareCapitalLedId : l.Led_Id, amount);
                            break;
                        case "DTo":
                        case "DBy":
                            // Q3: sundry recoveries also reduce the member's sundry balance (mem_trn trn_type 1 / 2).
                            await AddMemTrnAsync(posting, alloc, ctx, vocId, date, lineMemId,
                                (byte)(status == "DTo" ? 1 : 2), l.Led_Id, amount);
                            break;
                        case "D":
                            var demandId = await repo.GetOldestUnpaidDepositDemandIdAsync(lineMemId, l.DM_Id, brCode) ?? zeroId;
                            posting.DepositTrns.Add(new Deposit_Trn
                            {
                                Trn_Id = alloc?.NextDepositTrnId() ?? 0,
                                Deposit_Id = l.DM_Id,
                                Mem_Id = lineMemId,
                                Demand_Id = demandId,
                                Trn_Date = date,
                                Receipt_Amount = amount,
                                Is_Opening_Entry = false,
                                Is_Active = true,
                                Voc_Id = vocId,
                                Usr_Id = ctx.UsrId,
                                Yr_Id = ctx.YrId,
                                BrCode = brCode,
                                Trn_SlNo = alloc != null ? await alloc.NextDepositTrnSlNoAsync(lineMemId, l.DM_Id) : 0
                            });
                            break;
                    }

                    var ledId = status is "MS" or "SS" && shareCapitalLedId > 0 ? shareCapitalLedId
                              : status == "D" && depositLedgers.TryGetValue(l.DM_Id, out var dl) && dl > 0 ? dl
                              : l.Led_Id;
                    AddVoucherLine(sid, ledId, amount, 0, status, lineMemId, zeroId, Narr(lineMemId));
                }

                // 5.4 — one bank row per member
                posting.BankRows.Add(new Fin_Voucher_Bank
                {
                    Fvb_Id = alloc?.NextFvbId() ?? 0,
                    Fvb_Date = date,
                    Mem_Id = s.Mem_Id,
                    Fvb_Bank_Mode = isCheque ? "C" : "O",
                    Fvb_Bank_Name = isCheque || string.IsNullOrWhiteSpace(req.BankName) ? bank?.LedName : req.BankName!.Trim(),
                    Voc_Id = vocId,
                    Led_Id = req.BankLedId,
                    Fvb_Amount = net,
                    Fvb_Cheque_No = instrumentNo,
                    Fvb_Cheque_Date = instrumentDate,
                    Fvb_Real_Amount = 0,
                    Fvb_Cheque_Return = false,
                    Fvb_Return_Voc_Id = zeroId,
                    Fvb_Delete = false,
                    Usr_Id = ctx.UsrId,
                    Yr_Id = ctx.YrId,
                    BrCode = brCode,
                    Voc_Status = "V"
                });
                var (memNo, memName) = Mem(s.Mem_Id);
                pv.BankRows.Add(new EcsBankRowVM
                {
                    SanctionId = sid, MemberNo = memNo, MemberName = memName, Amount = net,
                    Instrument = $"{(isCheque ? "Cheque" : "NEFT")} {instrumentNo}"
                });

                // 5.5 — close the sanction
                posting.SanctionUpdates.Add((sid, loanId, loanAmount, s.LoanElig_Id));
                result.Loans.Add(new EcsDisbursedLoanVM
                {
                    SanctionId = sid, LoanId = loanId, LoanNo = loanNo, MemberNo = memNo, MemberName = memName,
                    LoanAmount = loanAmount, Net = net
                });
            }

            // 5.4 — the single bank credit line for the run
            var totalNet = Math.Round(result.Loans.Sum(l => l.Net), 2);
            AddVoucherLine(0, req.BankLedId, totalNet, 0, isCheque ? "Q" : "NEFT", zeroId, zeroId,
                $"ECS Loan Disbursement {instrumentText}");

            // --- 6. Totals and balance (§7.8) ---
            pv.TotalDebit = Math.Round(pv.Lines.Sum(l => l.Debit), 2);
            pv.TotalCredit = Math.Round(pv.Lines.Sum(l => l.Credit), 2);
            pv.TotalLoan = Math.Round(result.Loans.Sum(l => l.LoanAmount), 2);
            pv.TotalNet = totalNet;
            pv.TotalRecoverable = Math.Round(pv.TotalLoan - totalNet, 2);
            if (Math.Abs(pv.TotalDebit - pv.TotalCredit) > Tolerance
                || Math.Abs(pv.TotalDebit - posting.Voucher.Voc_Amt) > Tolerance)
                msg.Add($"The voucher does not balance (debit {pv.TotalDebit:N2}, credit {pv.TotalCredit:N2}).");

            pv.IsValid = msg.Count == 0;
            if (pv.IsValid) result.Posting = posting;
            return result;
        }

        private static bool IsLoanStatus(string? status) => status is "P" or "I" or "PI";

        private static async Task AddMemTrnAsync(EcsDisbursementPosting posting, EcsDisbursementIdAllocator? alloc,
            EcsUserContextVM ctx, decimal vocId, DateTime date, decimal memId, byte trnType, decimal ledId, double amount)
        {
            posting.MemTrns.Add(new Mem_Trn
            {
                Mem_Trn_Id = alloc?.NextMemTrnId() ?? 0,
                Trn_Type = trnType,
                Mem_Id = memId,
                Led_Id = ledId,
                Trn_Date = date,
                Rpt_Amt = amount,
                Pmt_Amt = 0,
                MemTrn_OE = false,
                MemTrn_Delete = false,
                Voc_Id = vocId,
                Usr_Id = ctx.UsrId,
                Yr_Id = ctx.YrId,
                Trn_SlNo = alloc != null ? await alloc.NextMemTrnSlNoAsync(memId, trnType, ledId) : 0,
                Status = "",
                BrCode = ctx.BrCode,
                Voc_Status = "V"
            });
        }
    }
}
