using System.Text.Json;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using Infin8.Coapp.Utility.Enums;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>
    /// ECS Recurring Deposit Loan Receipt, acc 25 (spec 18). Loads the member's open loans on RD with principal outstanding and
    /// interest due (simple, segment-wise, <see cref="EcsInterestCalculator"/>, capped at Last_DueDate) and, at approval,
    /// rechecks the staged figures and builds the loan_trn 'R' and fin_voucher_trn rows. The split is the shared
    /// <see cref="EcsRdLoanReceiptAppropriator"/>.
    /// </summary>
    public class EcsRdLoanReceiptHandler : IEcsRdLoanReceiptHandler
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
        private const string SchemeNotSetUp = "Loan on RD scheme is not set up";
        private const double Tolerance = EcsRdLoanReceiptAppropriator.Tolerance;

        readonly IUnitOfWork _unitOfWork;

        public EcsRdLoanReceiptHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =====================================================================
        // Load (spec 18 §4, §5)
        // =====================================================================

        public async Task<EcsRdLoanReceiptSetupVM> GetLoansAsync(decimal memId, string brCode, DateTime currentDate)
        {
            var openDay = await _unitOfWork.EcsLoanDisbursement.GetOpenBusinessDayAsync(brCode);
            if (openDay?.Date != currentDate.Date)
            {
                return new EcsRdLoanReceiptSetupVM
                {
                    ApplicationDate = currentDate.Date,
                    SetupError = "The business day has changed since you logged in (open business day: " +
                                 (openDay.HasValue ? openDay.Value.ToString("dd-MM-yyyy") : "none") + "). Log in again."
                };
            }
            return await GetPreviewAsync(memId, brCode, currentDate);
        }

        public async Task<EcsRdLoanReceiptSetupVM> GetPreviewAsync(decimal memId, string brCode, DateTime businessDay,
            decimal excludeStagingId = 0)
        {
            var day = businessDay.Date;
            var result = new EcsRdLoanReceiptSetupVM { ApplicationDate = day };

            var loans = await _unitOfWork.EcsRdLoanReceipt.GetOpenRdLoansAsync(memId, brCode);
            if (loans.Count == 0)
            {
                result.Message = "The member has no open loan on RD";
                return result;
            }

            // §8 rule 7: the scheme of every loan must carry its principal and interest ledgers.
            var schemes = (await _unitOfWork.EcsRdLoan.GetRdLoanSchemesAsync(brCode)).ToDictionary(s => s.Scheme_Id);
            if (loans.Any(l => !schemes.TryGetValue(l.Scheme_Id, out var s) || !LedgerSet(s.PrlLed_Id) || !LedgerSet(s.IntLed_Id)))
                result.SetupError = SchemeNotSetUp;

            var ids = loans.Select(l => l.Loan_Id).ToList();
            var trnRows = await _unitOfWork.EcsLoanRecovery.GetLoanTrnRowsAsync(ids, brCode, day);
            var roiRows = await _unitOfWork.EcsLoanRecovery.GetRoiRowsAsync(ids, brCode, day);
            var pledged = await _unitOfWork.EcsRdLoanReceipt.GetPledgedRdNosAsync(ids, brCode);
            var locked = await GetPendingLoanIdsAsync(brCode, excludeStagingId);

            var trnByLoan = trnRows.GroupBy(t => t.Loan_Id).ToDictionary(g => g.Key, g => g.ToList());
            var roiByLoan = roiRows.GroupBy(r => r.Loan_Id)
                .ToDictionary(g => g.Key, g => (IReadOnlyCollection<EcsRateEvent>)g.Select(r => new EcsRateEvent(r.Roi_Wef, r.Roi, r.Pi)).ToList());

            foreach (var loan in loans)
            {
                if (!trnByLoan.TryGetValue(loan.Loan_Id, out var rows) || rows.Count == 0) continue;   // not disbursed yet
                var rates = roiByLoan.TryGetValue(loan.Loan_Id, out var r) ? r : Array.Empty<EcsRateEvent>();

                var line = BuildLine(loan, rows, rates, day);
                line.PledgedRdNos = pledged.TryGetValue(loan.Loan_Id, out var nos) ? string.Join(", ", nos) : "";
                if (locked.Contains(loan.Loan_Id))
                    line.BlockReason = $"Loan No {line.Loan_No} is in another pending transaction";
                result.Lines.Add(line);
            }

            if (result.Lines.Count == 0) result.Message = "The member has no open loan on RD";
            EcsRdLoanReceiptAppropriator.Refresh(result.Lines);
            return result;
        }

        /// <summary>§5.1 principal outstanding and §5.2 interest of one loan, from its loan_trn rows.</summary>
        private static DtoEcsRdLoanReceiptLine BuildLine(Loan_Master loan, List<Loan_Trn> rows, IReadOnlyCollection<EcsRateEvent> rates,
            DateTime day)
        {
            double disb = rows.Sum(t => t.Disb_Amt);
            double prlColl = rows.Sum(t => t.PrlColl_Amt);
            double intCalc = rows.Sum(t => t.IntCalc_Amt);
            double intColl = rows.Sum(t => t.IntColl_Amt);

            DateTime disbursedOn = (rows.Where(t => t.Trn_Status == "I" && t.Disb_Date.HasValue).Min(t => t.Disb_Date)
                                    ?? rows.Where(t => t.Disb_Amt > 0).Select(t => (DateTime?)t.Trn_Date).Min()
                                    ?? loan.San_Date).Date;
            DateTime from = (rows.Max(t => t.IntCalc_Date) ?? disbursedOn).Date;
            DateTime lastDue = (loan.Last_DueDate ?? day).Date;

            // §5.2: interest runs to the business day, capped at the latest maturity of the pledged RDs (§15.2). The calculator's
            // end date is exclusive, so 20-Jun → 20-Dec is 183 days; IntCalc_Date is stamped with the same end date.
            DateTime to = day < lastDue ? day : lastDue;
            if (to < from) to = from;

            var deltas = rows.Select(t => new EcsTrnDelta(t.Trn_Date, t.Disb_Amt, t.Prl_Dem, t.PrlColl_Amt, t.IntCalc_Amt, t.IntColl_Amt)).ToList();
            // int_application 2 = on the principal outstanding; no penal (pi 0).
            var acc = EcsInterestCalculator.Calculate(EcsAccrualHead.Interest, 2, deltas, rates, loan.Roi, 0, from, to);
            var (roiNow, _) = EcsInterestCalculator.RateOn(rates, loan.Roi, 0, day);

            var line = new DtoEcsRdLoanReceiptLine
            {
                Loan_Id = loan.Loan_Id,
                Loan_No = loan.Loan_No?.Trim(),
                Scheme_Id = loan.Scheme_Id,
                DisbursedOn = disbursedOn,
                Disbursed = R2(disb),
                Roi = roiNow,
                LastDueDate = lastDue,
                PrincipalOutstanding = R2(Math.Max(0, disb - prlColl)),
                InterestFrom = from,
                InterestTo = to,
                InterestArrear = R2(Math.Max(0, intCalc - intColl)),
                InterestCurrent = acc.Amount
            };
            line.InterestDue = EcsRdLoanReceiptAppropriator.InterestDue(line);
            return line;
        }

        /// <summary>§2.2 of spec 17: a ledger id ending in 0000000 (the branch's zero id) counts as "not set".</summary>
        private static bool LedgerSet(decimal ledId) => ledId > 0 && ledId % 10000000m != 0;

        /// <summary>§9: the Loan_Ids in the branch's pending (I/M) acc 25 lines, except <paramref name="excludeStagingId"/>.</summary>
        private async Task<HashSet<decimal>> GetPendingLoanIdsAsync(string brCode, decimal excludeStagingId)
        {
            var set = new HashSet<decimal>();
            foreach (var json in await _unitOfWork.EcsRdLoanReceipt.GetPendingReceiptDataAsync(brCode, excludeStagingId))
            {
                try
                {
                    var staged = JsonSerializer.Deserialize<DtoEcsRdLoanReceipt>(json, JsonOptions);
                    if (staged != null) set.UnionWith(staged.Lines.Select(l => l.Loan_Id));
                }
                catch (JsonException)
                {
                    // A malformed staging row cannot lock loans; ignore it.
                }
            }
            return set;
        }

        // =====================================================================
        // Approval posting (spec 18 §9, §11)
        // =====================================================================

        public async Task<EcsRdLoanReceiptPosting> BuildPostingAsync(DtoEcsRdLoanReceipt staged, decimal stagingId, DateTime stagingDate,
            string brCode, decimal yrId, decimal usrId, decimal vocId, int trnType, string status)
        {
            const string Retry = ". Reject it and re-initiate.";
            var appDate = staged.ApplicationDate.Date;

            // --- context checks (§9) ---
            var society = await _unitOfWork.General.GetSocietyData(brCode);
            if ((society?.Bank_Type ?? 0).ToInstitutionType() != InstitutionType.ECS)
                throw new EcsRecoveryException("RD loan receipt can be approved only in an ECS society.");

            var openDay = await _unitOfWork.EcsLoanDisbursement.GetOpenBusinessDayAsync(brCode);
            if (appDate != stagingDate.Date || openDay?.Date != appDate)
                throw new EcsRecoveryException(
                    $"RD loan receipt staged on {appDate:dd-MM-yyyy} must be approved on the same business day " +
                    $"(open business day: {(openDay.HasValue ? openDay.Value.ToString("dd-MM-yyyy") : "none")}). Reject it and re-initiate.");

            if (staged.Lines.Count == 0)
                throw new EcsRecoveryException("RD loan receipt has no loans" + Retry);
            if (staged.Lines.Select(l => l.Loan_Id).Distinct().Count() != staged.Lines.Count)
                throw new EcsRecoveryException("RD loan receipt lists a loan twice" + Retry);

            // --- recompute (§5) and compare with the staged figures (§9) ---
            var preview = await GetPreviewAsync(staged.MemId, brCode, appDate, stagingId);
            if (!string.IsNullOrEmpty(preview.SetupError))
                throw new EcsRecoveryException(preview.SetupError + Retry);

            var byId = preview.Lines.ToDictionary(l => l.Loan_Id);
            var freshLines = new List<DtoEcsRdLoanReceiptLine>();
            foreach (var s in staged.Lines)
            {
                if (!byId.TryGetValue(s.Loan_Id, out var f))
                    throw new EcsRecoveryException($"Loan No {s.Loan_No?.Trim()} is not open" + Retry);
                if (!string.IsNullOrEmpty(f.BlockReason))
                    throw new EcsRecoveryException(f.BlockReason + Retry);

                string? changed =
                    !EcsRdLoanReceiptAppropriator.Near(s.PrincipalOutstanding, f.PrincipalOutstanding) ? "the principal outstanding" :
                    s.InterestFrom.Date != f.InterestFrom.Date ? "the interest-from date" :
                    s.InterestTo.Date != f.InterestTo.Date ? "the interest-to date" :
                    !EcsRdLoanReceiptAppropriator.Near(s.InterestArrear, f.InterestArrear) ? "the interest arrear" :
                    !EcsRdLoanReceiptAppropriator.Near(s.InterestCurrent, f.InterestCurrent) ? "the current interest" :
                    !EcsRdLoanReceiptAppropriator.Near(s.InterestDue, f.InterestDue) ? "the interest due" : null;
                if (changed != null)
                    throw new EcsRecoveryException($"Loan No {f.Loan_No}: {changed} has changed since this receipt was initiated" + Retry);

                f.InterestReceived = s.InterestReceived;
                f.PrincipalReceived = s.PrincipalReceived;
                freshLines.Add(f);
            }

            var fresh = new DtoEcsRdLoanReceipt
            {
                MemId = staged.MemId,
                MemberNo = staged.MemberNo,
                MemberName = staged.MemberName,
                ApplicationDate = appDate,
                CashOrAdjustment = staged.CashOrAdjustment
            };
            EcsRdLoanReceiptAppropriator.Totals(fresh, freshLines);
            if (fresh.Lines.Count != staged.Lines.Count)
                throw new EcsRecoveryException("RD loan receipt has a loan with nothing received" + Retry);
            var invalid = EcsRdLoanReceiptAppropriator.Validate(fresh);
            if (invalid != null) throw new EcsRecoveryException(invalid + Retry);

            string? total =
                !EcsRdLoanReceiptAppropriator.Near(staged.TotalReceived, fresh.TotalReceived) ? "the total received" :
                !EcsRdLoanReceiptAppropriator.Near(staged.TotalInterest, fresh.TotalInterest) ? "the interest received" :
                !EcsRdLoanReceiptAppropriator.Near(staged.TotalPrincipal, fresh.TotalPrincipal) ? "the principal received" : null;
            if (total != null)
                throw new EcsRecoveryException($"RD loan receipt: {total} does not match the loans" + Retry);
            foreach (var s in staged.Lines)
            {
                var f = fresh.Lines.First(l => l.Loan_Id == s.Loan_Id);
                if (s.ClosesLoan != f.ClosesLoan)
                    throw new EcsRecoveryException($"Loan No {f.Loan_No}: the closure flag does not match the amounts" + Retry);
            }

            // --- build rows (§11) ---
            var schemes = (await _unitOfWork.EcsRdLoan.GetRdLoanSchemesAsync(brCode)).ToDictionary(s => s.Scheme_Id);
            decimal zeroId = decimal.TryParse(brCode + "0000000", out var z) ? z : 0;
            string member = $"{staged.MemberNo?.Trim()} {staged.MemberName?.Trim()}".Trim();
            var posting = new EcsRdLoanReceiptPosting();

            foreach (var l in fresh.Lines)
            {
                if (!schemes.TryGetValue(l.Scheme_Id, out var scheme) || !LedgerSet(scheme.PrlLed_Id) || !LedgerSet(scheme.IntLed_Id))
                    throw new EcsRecoveryException(SchemeNotSetUp + Retry);

                // §11.1 loan_trn 'R'
                posting.LoanTrns.Add(new Loan_Trn
                {
                    Trn_Id = 0,                       // assigned by LoanTrnRepository.AddLoanTrnListAsync (MAX+1)
                    Loan_Id = l.Loan_Id,
                    Demand_Id = zeroId,
                    Trn_Status = "R",
                    Trn_Date = appDate,
                    IntCalc_Amt = l.InterestCurrent,
                    IntCalc_Date = l.InterestTo.Date,
                    IntColl_Amt = l.InterestReceived,
                    PrlColl_Amt = l.PrincipalReceived,
                    Prl_Dem = 0,
                    PICalc_Amt = 0,
                    PIColl_Amt = 0,
                    Soc_Roi = l.Roi,
                    SocPI_Rate = 0,
                    TrnTr_Oe = false,
                    TrnTr_Delete = false,
                    Voc_Id = vocId,
                    Usr_Id = usrId,
                    Yr_Id = yrId,
                    Trn_SlNo = 0,                     // assigned by AddLoanTrnListAsync (MAX per loan + 1)
                    BrCode = brCode,
                    Voc_Status = "V"
                });

                // §11.2 fin_voucher_trn: Cr interest, Cr principal (receipt side)
                string desc = "Loan No " + l.Loan_No;
                string narr = (member + " " + desc).Trim();
                if (l.InterestReceived > Tolerance)
                    posting.VoucherTrns.Add(GetModalObject.GetFinVoucherTrObject(vocId, scheme.IntLed_Id, l.InterestReceived, 0, trnType,
                        narr, false, usrId, yrId, status, desc, staged.MemId, brCode, l.Loan_Id, l.InterestDue,
                        R2(l.InterestDue - l.InterestReceived)));
                if (l.PrincipalReceived > Tolerance)
                    posting.VoucherTrns.Add(GetModalObject.GetFinVoucherTrObject(vocId, scheme.PrlLed_Id, l.PrincipalReceived, 0, trnType,
                        narr, false, usrId, yrId, status, desc, staged.MemId, brCode, l.Loan_Id, l.PrincipalOutstanding,
                        R2(l.PrincipalOutstanding - l.PrincipalReceived)));

                // §11.3
                if (l.ClosesLoan) posting.ClosedLoanIds.Add(l.Loan_Id);
            }

            double posted = R2(posting.VoucherTrns.Sum(v => v.Voc_Rpt));
            if (Math.Abs(posted - R2(staged.TotalReceived)) > Tolerance)
                throw new EcsRecoveryException($"RD loan receipt amount ₹{staged.TotalReceived:N2} does not match the appropriated ₹{posted:N2}" + Retry);

            return posting;
        }

        private static double R2(double v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
    }
}
