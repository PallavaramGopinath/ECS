using System.Text.Json;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using Infin8.Coapp.Utility.Enums;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>
    /// ECS Loan on Recurring Deposit, acc 14 (spec 17). Loads the member's pledgeable RDs with the eligibility figures
    /// (<see cref="EcsRdLoanCalculator"/>) and, at approval, rechecks the staged figures and builds the loan_master,
    /// loan_trn, loan_disb, loan_roi, lien, lien_trn and fin_voucher_trn rows.
    /// </summary>
    public class EcsRdLoanHandler : IEcsRdLoanHandler
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
        private const string SchemeNotSetUp = "Loan on RD scheme is not set up";

        readonly IUnitOfWork _unitOfWork;

        public EcsRdLoanHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =====================================================================
        // Load (spec 17 §4, §6, §8)
        // =====================================================================

        public async Task<EcsRdLoanSetupVM> GetSetupAsync(decimal memId, string brCode, DateTime currentDate)
        {
            var openDay = await _unitOfWork.EcsLoanDisbursement.GetOpenBusinessDayAsync(brCode);
            if (openDay?.Date != currentDate.Date)
            {
                return new EcsRdLoanSetupVM
                {
                    ApplicationDate = currentDate.Date,
                    SetupError = "The business day has changed since you logged in (open business day: " +
                                 (openDay.HasValue ? openDay.Value.ToString("dd-MM-yyyy") : "none") + "). Log in again."
                };
            }
            return await GetPreviewAsync(memId, brCode, currentDate);
        }

        public async Task<EcsRdLoanSetupVM> GetPreviewAsync(decimal memId, string brCode, DateTime businessDay,
            decimal excludeStagingId = 0)
        {
            var day = businessDay.Date;
            var result = new EcsRdLoanSetupVM { ApplicationDate = day };
            var errors = new List<string>();

            // §8 rule 11: member status.
            var member = await _unitOfWork.EcsRdCreate.GetMemberByIdAsync(memId, brCode);
            if (member == null || member.IsDeleted || member.IsExpired || member.MemberStatus != 1)
                errors.Add("Member is not active");

            // §2.2 / §8 rule 2: scheme.
            var (scheme, schemeError) = await GetSchemeAsync(brCode);
            if (scheme == null) errors.Add(schemeError!);
            else
            {
                result.Scheme_Id = scheme.Scheme_Id;
                result.SchemeName = scheme.Scheme_Name?.Trim();
                try
                {
                    result.ProvisionalLoanNo = await _unitOfWork.EcsRdLoan.PeekNextLoanNoAsync(scheme, brCode);
                }
                catch (InvalidOperationException ex)
                {
                    errors.Add(ex.Message);
                }

                // §6.2 margin
                var margin = await _unitOfWork.EcsRdLoan.GetRoiMarginAsync(scheme.Scheme_Id, brCode, day);
                if (margin == null) errors.Add("Rate of interest margin for Loan on RD is not defined");
                else result.Margin = margin.Value;
            }

            // §6.1 eligibility %
            var eligPer = await _unitOfWork.EcsRdLoan.GetEligibilityPercentAsync(brCode, day);
            if (eligPer == null || eligPer <= 0) errors.Add("Loan eligibility percentage for RD is not defined");
            else result.EligPer = eligPer.Value;

            if (errors.Count > 0) result.SetupError = string.Join(". ", errors);

            // §4.1: the RD list of the RD receipt (open, member is a holder, nothing paid out, BrCode on every alias).
            var rds = await _unitOfWork.EcsRdReceipt.GetEligibleRdsAsync(memId, brCode);
            if (rds.Count == 0)
            {
                result.Message = "The member has no open recurring deposits";
                return result;
            }

            var tdIds = rds.Select(r => r.TD_Id).ToList();
            var liened = await _unitOfWork.EcsRdLoan.GetLienLoanNosAsync(tdIds, brCode);
            var demand = await _unitOfWork.EcsRdRefund.GetUnrecoveredDemandAsync(tdIds, brCode);
            var locked = await GetPendingLoanTdIdsAsync(brCode, excludeStagingId);
            locked.UnionWith(await GetPendingTdIdsAsync<DtoEcsRdReceipt>(
                await _unitOfWork.EcsRdReceipt.GetPendingReceiptDataAsync(brCode, excludeStagingId), r => r.Lines.Select(l => l.TD_Id)));
            locked.UnionWith(await GetPendingTdIdsAsync<DtoEcsRdRefund>(
                await _unitOfWork.EcsRdRefund.GetPendingRefundDataAsync(brCode, excludeStagingId), r => r.Lines.Select(l => l.TD_Id)));

            foreach (var rd in rds)
            {
                var line = new DtoEcsRdLoanLine
                {
                    TD_Id = rd.TD_Id,
                    TD_No = rd.TD_No?.Trim(),
                    TDScheme_Id = (int)rd.TDScheme_Id,
                    Instalment = rd.Instalment,
                    ValueDate = rd.ValueDate,
                    PeriodInMonths = rd.PeriodInMonths,
                    RateOfInterest = rd.RateOfInterest,
                    MaturityDate = rd.MaturityDate ?? rd.ValueDate,
                    MaturityAmount = rd.MaturityAmount,
                    Received = rd.Received,
                    DrawingPower = EcsRdLoanCalculator.DrawingPower(rd.Received, result.EligPer)
                };

                if (liened.TryGetValue(rd.TD_Id, out var loanNo))
                    line.BlockReason = $"RD No {line.TD_No} is already pledged for loan No {loanNo ?? "(unknown)"}";
                else if (locked.Contains(rd.TD_Id))
                    line.BlockReason = $"RD No {line.TD_No} is in another pending transaction";
                else if (demand.TryGetValue(rd.TD_Id, out var due))
                    line.BlockReason = $"RD No {line.TD_No} is in the demand of {(due.HasValue ? due.Value.ToString("dd-MM-yyyy") : "an earlier date")}, not yet recovered";
                else if (rd.Received <= 0)
                    line.BlockReason = $"RD No {line.TD_No} has no amount received";

                result.Lines.Add(line);
            }
            return result;
        }

        /// <summary>§2.2: exactly one live Loan_Type 4 scheme, with its principal and interest ledgers set.</summary>
        private async Task<(Loan_Schemes? scheme, string? error)> GetSchemeAsync(string brCode)
        {
            var schemes = await _unitOfWork.EcsRdLoan.GetRdLoanSchemesAsync(brCode);
            if (schemes.Count != 1) return (null, SchemeNotSetUp);
            var s = schemes[0];
            if (!LedgerSet(s.PrlLed_Id) || !LedgerSet(s.IntLed_Id)) return (null, SchemeNotSetUp);
            return (s, null);
        }

        /// <summary>§2.2: a ledger id ending in 0000000 (the branch's zero id) counts as "not set".</summary>
        private static bool LedgerSet(decimal ledId) => ledId > 0 && ledId % 10000000m != 0;

        public async Task<HashSet<decimal>> GetPendingLoanTdIdsAsync(string brCode, decimal excludeStagingId = 0) =>
            await GetPendingTdIdsAsync<DtoEcsRdLoan>(
                await _unitOfWork.EcsRdLoan.GetPendingLoanDataAsync(brCode, excludeStagingId), r => r.Lines.Select(l => l.TD_Id));

        private static Task<HashSet<decimal>> GetPendingTdIdsAsync<T>(IEnumerable<string> jsons, Func<T, IEnumerable<decimal>> ids)
        {
            var set = new HashSet<decimal>();
            foreach (var json in jsons)
            {
                try
                {
                    var staged = JsonSerializer.Deserialize<T>(json, JsonOptions);
                    if (staged != null) set.UnionWith(ids(staged));
                }
                catch (JsonException)
                {
                    // A malformed staging row cannot lock RDs; ignore it.
                }
            }
            return Task.FromResult(set);
        }

        // =====================================================================
        // Approval posting (spec 17 §9, §11)
        // =====================================================================

        public async Task<EcsRdLoanPosting> BuildPostingAsync(DtoEcsRdLoan staged, decimal stagingId, DateTime stagingDate,
            string brCode, decimal yrId, decimal usrId, decimal vocId, int trnType, string status)
        {
            const string Retry = ". Reject it and re-initiate.";
            var appDate = staged.ApplicationDate.Date;

            // --- context checks (§9) ---
            var society = await _unitOfWork.General.GetSocietyData(brCode);
            if ((society?.Bank_Type ?? 0).ToInstitutionType() != InstitutionType.ECS)
                throw new EcsRecoveryException("Loan on RD can be approved only in an ECS society.");

            var openDay = await _unitOfWork.EcsLoanDisbursement.GetOpenBusinessDayAsync(brCode);
            if (appDate != stagingDate.Date || openDay?.Date != appDate)
                throw new EcsRecoveryException(
                    $"Loan on RD staged on {appDate:dd-MM-yyyy} must be approved on the same business day " +
                    $"(open business day: {(openDay.HasValue ? openDay.Value.ToString("dd-MM-yyyy") : "none")}). Reject it and re-initiate.");

            if (staged.Lines.Count == 0)
                throw new EcsRecoveryException("Loan on RD has no RDs" + Retry);
            if (staged.Lines.Select(l => l.TD_Id).Distinct().Count() != staged.Lines.Count)
                throw new EcsRecoveryException("Loan on RD lists an RD twice" + Retry);

            // --- recompute (§6) and compare with the staged figures (§9) ---
            var preview = await GetPreviewAsync(staged.MemId, brCode, appDate, stagingId);
            if (!string.IsNullOrEmpty(preview.SetupError))
                throw new EcsRecoveryException(preview.SetupError + Retry);
            var (scheme, _) = await GetSchemeAsync(brCode);
            if (scheme == null || scheme.Scheme_Id != staged.Scheme_Id)
                throw new EcsRecoveryException("The Loan on RD scheme has changed since this loan was initiated" + Retry);

            var byId = preview.Lines.ToDictionary(l => l.TD_Id);
            var fresh = new DtoEcsRdLoan
            {
                MemId = staged.MemId,
                MemberNo = staged.MemberNo,
                MemberName = staged.MemberName,
                ApplicationDate = appDate,
                CashOrAdjustment = staged.CashOrAdjustment,
                Scheme_Id = scheme.Scheme_Id,
                SchemeName = scheme.Scheme_Name?.Trim(),
                EligPer = preview.EligPer,
                Margin = preview.Margin,
                LoanAmount = staged.LoanAmount
            };
            foreach (var s in staged.Lines)
            {
                if (!byId.TryGetValue(s.TD_Id, out var f))
                    throw new EcsRecoveryException($"RD No {s.TD_No?.Trim()} is not open" + Retry);
                if (!string.IsNullOrEmpty(f.BlockReason))
                    throw new EcsRecoveryException(f.BlockReason + Retry);

                string? changed =
                    !EcsRdLoanCalculator.Near(s.Received, f.Received) ? "the amount received" :
                    !EcsRdLoanCalculator.Near(s.RateOfInterest, f.RateOfInterest) ? "the rate of interest" :
                    s.MaturityDate.Date != f.MaturityDate.Date ? "the maturity date" : null;
                if (changed != null)
                    throw new EcsRecoveryException($"RD No {f.TD_No}: {changed} has changed since this loan was initiated" + Retry);
                f.Selected = true;
                fresh.Lines.Add(f);
            }

            fresh.ExistingOutstanding = await _unitOfWork.EcsRdLoan.GetExistingOutstandingAsync(fresh.Lines.Select(l => l.TD_Id), brCode);
            EcsRdLoanCalculator.Totals(fresh);

            // Per RD drawing power, then the totals.
            foreach (var s in staged.Lines)
            {
                var f = fresh.Lines.First(l => l.TD_Id == s.TD_Id);
                if (!EcsRdLoanCalculator.Near(s.DrawingPower, f.DrawingPower))
                    throw new EcsRecoveryException($"RD No {f.TD_No}: the drawing power has changed since this loan was initiated" + Retry);
            }
            string? total =
                !EcsRdLoanCalculator.Near(staged.EligPer, fresh.EligPer) ? "the eligibility percentage" :
                !EcsRdLoanCalculator.Near(staged.TotalReceived, fresh.TotalReceived) ? "the total received" :
                !EcsRdLoanCalculator.Near(staged.BorrowingPower, fresh.BorrowingPower) ? "the borrowing power" :
                !EcsRdLoanCalculator.Near(staged.ExistingOutstanding, fresh.ExistingOutstanding) ? "the existing loan outstanding" :
                !EcsRdLoanCalculator.Near(staged.EligibleAmount, fresh.EligibleAmount) ? "the eligible amount" :
                !EcsRdLoanCalculator.Near(staged.LoanRoi, fresh.LoanRoi) ? "the loan rate of interest" :
                staged.DueDate.Date != fresh.DueDate.Date ? "the loan due date" : null;
            if (total != null)
                throw new EcsRecoveryException($"Loan on RD: {total} has changed since this loan was initiated" + Retry);

            var invalid = EcsRdLoanCalculator.Validate(fresh);
            if (invalid != null) throw new EcsRecoveryException(invalid + Retry);

            // --- build rows (§11) ---
            var alloc = await _unitOfWork.EcsRdLoan.CreateIdAllocatorAsync(brCode);
            decimal loanId = alloc.NextLoanId();
            string loanNo = await alloc.NextLoanNoAsync(scheme);
            decimal zeroId = decimal.TryParse(brCode + "0000000", out var z) ? z : 0;
            double amount = fresh.LoanAmount;
            var posting = new EcsRdLoanPosting();

            // §11.1 loan_master
            posting.LoanMaster = new Loan_Master
            {
                Loan_Id = loanId,
                Scheme_Id = scheme.Scheme_Id,
                Loan_No = loanNo,
                Mem_Id = staged.MemId,
                App_Id = 0,
                Res_Date = null,
                San_Amt = amount,
                San_Date = appDate,
                Loan_Type = scheme.Loan_Type,
                Loan_Status = 1,
                Pur_Id = 1,
                SubGrp_Id = 0,
                Grp_Id = 0,
                Ags_Id = 0,
                Diff_Prd = 0,
                DiffIntRec_Prd = 0,
                Grace_Prd = 0,
                Prl_Prd = 0,
                Int_Prd = 0,
                Inst_Amt = 0,
                Inst_Date = 0,
                FirstInt_DueDate = appDate,
                FirstPrl_DueDate = appDate,
                Last_DueDate = fresh.DueDate.Date,
                Roi = fresh.LoanRoi,
                Pi = 0,
                LastIntApplication_Date = appDate,
                SecurityFaceValue = fresh.TotalReceived,
                DrawingPower = fresh.BorrowingPower,
                SecurityType = 0,
                Loan_Oe = false,
                Loan_Delete = false,
                IsAccountClosed = false,
                Voc_Id = vocId,
                Usr_Id = usrId,
                Yr_Id = yrId,
                BrCode = brCode,
                Voc_Status = "V"
            };

            // §11.2 loan_trn (Trn_Id / Trn_SlNo by AddLoanTrnListAsync), loan_disb, loan_roi
            posting.LoanTrn = new Loan_Trn
            {
                Trn_Id = 0,
                Loan_Id = loanId,
                Demand_Id = zeroId,
                Trn_Status = "I",
                Trn_Date = appDate,
                Disb_Date = appDate,
                Disb_Amt = amount,
                IntCalc_Amt = 0,
                IntCalc_Date = appDate,
                Soc_Roi = fresh.LoanRoi,
                SocPI_Rate = 0,
                TrnTr_Oe = false,
                TrnTr_Delete = false,
                Voc_Id = vocId,
                Usr_Id = usrId,
                Yr_Id = yrId,
                Trn_SlNo = 1,
                BrCode = brCode,
                Voc_Status = "V"
            };
            posting.LoanDisb = new Loan_Disb
            {
                Disb_Id = alloc.NextDisbId(),
                Loan_Id = loanId,
                Disb_Date = appDate,
                SocDisb_No = 1,
                SocDisb_Amt = amount,
                SocDisb_Date = appDate,
                Disb_Oe = false,
                Disb_SlNo = 1,
                Is_FinalDisbursement = true,
                LoanDisb_Delete = false,
                Voc_Id = vocId,
                Usr_Id = usrId,
                Yr_Id = yrId,
                BrCode = brCode,
                Voc_Status = "V"
            };
            posting.LoanRoi = new Loan_Roi
            {
                Roi_Id = alloc.NextRoiId(),
                Loan_Id = loanId,
                Agency = "S",
                Roi_Wef = appDate,
                Roi = fresh.LoanRoi,
                Pi = 0,
                Prd = 0,
                InstallmentAmount = 0,
                Loanroi_Delete = false,
                LoanRoi_Oe = false,
                Voc_Id = vocId,
                Usr_Id = usrId,
                Yr_Id = yrId,
                BrCode = brCode,
                Voc_Status = "V"
            };

            // §11.3 lien (one per loan) and lien_trn (one per RD)
            string description = "RD Nos " + string.Join(" ", fresh.Lines.Select(l => l.TD_No));
            if (description.Length > 100) description = description[..100];
            decimal lienId = alloc.NextLienId();
            posting.Lien = new Lien
            {
                Lien_Id = lienId,
                Loan_Id = loanId,
                TD_Id = fresh.Lines[0].TD_Id,
                TD_Amount = fresh.TotalReceived,
                Lien_Amount = amount,
                Lien_Recovered = 0,
                SecurityDescription = description,
                Lien_Closed = false,
                Lien_OE = false,
                Lien_Delete = false,
                Voc_Id = vocId,
                Usr_Id = usrId,
                Yr_Id = yrId,
                BrCode = brCode,
                Voc_Status = "V"
            };
            var shares = EcsRdLoanCalculator.LienShares(amount, fresh.Lines, fresh.BorrowingPower);
            for (int i = 0; i < fresh.Lines.Count; i++)
            {
                var rd = fresh.Lines[i];
                posting.LienTrns.Add(new Lien_Trn
                {
                    LienTr_Id = alloc.NextLienTrId(),
                    Lien_Id = lienId,
                    Loan_Id = loanId,
                    TD_Id = rd.TD_Id,
                    TDTr_Amount = rd.Received,
                    DrawingPower = rd.DrawingPower,
                    LienTr_Amount = shares[i],
                    LienTr_Recovered = 0,
                    LienTr_Closed = false,
                    LienTr_OE = false,
                    LienTr_Delete = false,
                    Voc_Id = vocId,
                    Usr_Id = usrId,
                    Yr_Id = yrId,
                    BrCode = brCode,
                    Voc_Status = "V"
                });
                posting.LienTdIds.Add(rd.TD_Id);
            }

            // §11.5 fin_voucher_trn: Dr Loan on RD (payment side)
            string member = $"{staged.MemberNo?.Trim()} {staged.MemberName?.Trim()}".Trim();
            string desc = "Loan No " + loanNo;
            posting.VoucherTrns.Add(GetModalObject.GetFinVoucherTrObject(vocId, scheme.PrlLed_Id, 0, amount, trnType,
                (member + " " + desc).Trim(), false, usrId, yrId, status, desc, staged.MemId, brCode, loanId, amount, 0));

            return posting;
        }
    }
}
