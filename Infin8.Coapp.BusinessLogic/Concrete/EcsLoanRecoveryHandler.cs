using System.Text.Json;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using Infin8.Coapp.Utility.Enums;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>
    /// ECS Loan Recovery, acc 58 (spec 11). Loads the member's ECS loans, accrues interest and penal with the
    /// segment-wise product (<see cref="EcsInterestCalculator"/>) and, at approval, rechecks the staged
    /// appropriation (<see cref="EcsRecoveryAppropriator"/>) and builds the loan_trn / fin_voucher_trn rows.
    /// </summary>
    public class EcsLoanRecoveryHandler : IEcsLoanRecoveryHandler
    {
        private const double Tolerance = EcsRecoveryAppropriator.Tolerance;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        readonly IUnitOfWork _unitOfWork;

        public EcsLoanRecoveryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =====================================================================
        // Preview (spec 11 §4, §6, §7)
        // =====================================================================

        public async Task<EcsLoanRecoveryPreviewVM> GetRecoveryLoansAsync(decimal memId, EcsUserContextVM ctx)
        {
            var openDay = await _unitOfWork.EcsLoanDisbursement.GetOpenBusinessDayAsync(ctx.BrCode);
            if (openDay?.Date != ctx.CurrentDate.Date)
            {
                return new EcsLoanRecoveryPreviewVM
                {
                    ApplicationDate = ctx.CurrentDate.Date,
                    Message = "The business day has changed since you logged in (open business day: " +
                              (openDay.HasValue ? openDay.Value.ToString("dd-MM-yyyy") : "none") + "). Log in again."
                };
            }
            return await GetRecoveryPreviewAsync(memId, ctx.BrCode, ctx.CurrentDate);
        }

        public async Task<EcsLoanRecoveryPreviewVM> GetRecoveryPreviewAsync(decimal memId, string brCode, DateTime applicationDate,
            decimal excludeStagingId = 0)
        {
            var appDate = applicationDate.Date;
            var result = new EcsLoanRecoveryPreviewVM { ApplicationDate = appDate };

            var candidates = await _unitOfWork.EcsLoanRecovery.GetCandidateLoansAsync(memId, brCode);
            if (candidates.Count == 0)
            {
                result.Message = "No ECS loans with dues for this member";
                return result;
            }

            var ids = candidates.Select(c => c.Loan_Id).ToList();
            var trnRows = await _unitOfWork.EcsLoanRecovery.GetLoanTrnRowsAsync(ids, brCode, appDate);
            var roiRows = await _unitOfWork.EcsLoanRecovery.GetRoiRowsAsync(ids, brCode, appDate);
            var pendingIds = await GetPendingLoanIdsAsync(memId, brCode, excludeStagingId);

            var trnByLoan = trnRows.GroupBy(t => t.Loan_Id).ToDictionary(g => g.Key, g => g.ToList());
            var roiByLoan = roiRows.GroupBy(r => r.Loan_Id)
                .ToDictionary(g => g.Key, g => (IReadOnlyCollection<EcsRateEvent>)g.Select(r => new EcsRateEvent(r.Roi_Wef, r.Roi, r.Pi)).ToList());

            foreach (var loan in candidates)
            {
                if (!trnByLoan.TryGetValue(loan.Loan_Id, out var rows) || rows.Count == 0) continue;
                var rates = roiByLoan.TryGetValue(loan.Loan_Id, out var r) ? r : Array.Empty<EcsRateEvent>();
                FillBalances(loan, rows, rates, appDate);

                bool hasDues = loan.PrlOutstanding > Tolerance
                               || loan.IntOverdue + loan.IntNow > Tolerance
                               || loan.PIOverdue + loan.PINow > Tolerance;
                if (!hasDues) continue;

                loan.IsPending = pendingIds.Contains(loan.Loan_Id);
                result.Loans.Add(loan);
            }

            result.Loans = EcsRecoveryAppropriator.Order(result.Loans);
            if (result.Loans.Count == 0) result.Message = "No ECS loans with dues for this member";
            return result;
        }

        /// <summary>Spec 11 §4.1 balances and §6 accrual for one loan, from its loan_trn rows.</summary>
        private static void FillBalances(EcsLoanRecoveryLoanVM loan, List<Loan_Trn> rows, IReadOnlyCollection<EcsRateEvent> rates,
            DateTime appDate)
        {
            double disb = rows.Sum(t => t.Disb_Amt);
            double dem = rows.Sum(t => t.Prl_Dem);
            double prlColl = rows.Sum(t => t.PrlColl_Amt);
            double intCalc = rows.Sum(t => t.IntCalc_Amt);
            double intColl = rows.Sum(t => t.IntColl_Amt);
            double piCalc = rows.Sum(t => t.PICalc_Amt);
            double piColl = rows.Sum(t => t.PIColl_Amt);

            loan.TotalDisbursed = R2(disb);
            loan.TotalPrlDemand = R2(dem);
            loan.TotalPrlCollection = R2(prlColl);
            loan.PrlOutstanding = R2(Math.Max(0, disb - prlColl));
            loan.PrlOverdue = R2(Math.Max(0, dem - prlColl));   // negative = advance principal → 0
            loan.IntOverdue = R2(Math.Max(0, intCalc - intColl));
            loan.PIOverdue = R2(Math.Max(0, piCalc - piColl));

            loan.Disb_Date = rows.Where(t => t.Trn_Status == "I" && t.Disb_Date.HasValue).Min(t => t.Disb_Date)
                             ?? rows.Where(t => t.Disb_Amt > 0).Select(t => (DateTime?)t.Trn_Date).Min();
            loan.LastIntDate = rows.Max(t => t.IntCalc_Date) ?? loan.Disb_Date;
            loan.LastPIDate = rows.Max(t => t.PICalc_Date) ?? loan.Disb_Date;

            var deltas = rows.Select(t => new EcsTrnDelta(t.Trn_Date, t.Disb_Amt, t.Prl_Dem, t.PrlColl_Amt, t.IntCalc_Amt, t.IntColl_Amt)).ToList();

            if (loan.LastIntDate.HasValue)
            {
                var acc = EcsInterestCalculator.Calculate(EcsAccrualHead.Interest, loan.Int_Application, deltas, rates,
                    loan.Roi, loan.Pi, loan.LastIntDate.Value, appDate);
                (loan.IntNow, loan.IntFrom, loan.IntTo, loan.IntDays) = (acc.Amount, acc.From, acc.To, acc.Days);
            }
            if (loan.LastPIDate.HasValue)
            {
                var acc = EcsInterestCalculator.Calculate(EcsAccrualHead.Penal, loan.PI_Application, deltas, rates,
                    loan.Roi, loan.Pi, loan.LastPIDate.Value, appDate);
                (loan.PINow, loan.PIFrom, loan.PITo, loan.PIDays) = (acc.Amount, acc.From, acc.To, acc.Days);
            }

            (loan.CurrentRoi, loan.CurrentPi) = EcsInterestCalculator.RateOn(rates, loan.Roi, loan.Pi, appDate);

            double interestAndPenal = loan.IntOverdue + loan.IntNow + loan.PIOverdue + loan.PINow;
            loan.Recoverable = R2(loan.PrlOverdue + interestAndPenal);
            loan.ToClose = R2(loan.PrlOutstanding + interestAndPenal);
        }

        private async Task<HashSet<decimal>> GetPendingLoanIdsAsync(decimal memId, string brCode, decimal excludeStagingId)
        {
            var ids = new HashSet<decimal>();
            foreach (var json in await _unitOfWork.EcsLoanRecovery.GetPendingRecoveryDataAsync(memId, brCode, excludeStagingId))
            {
                try
                {
                    var staged = JsonSerializer.Deserialize<DtoEcsLoanRecovery>(json, JsonOptions);
                    foreach (var line in staged?.Lines ?? new()) ids.Add(line.Loan_Id);
                }
                catch (JsonException)
                {
                    // A malformed staging row cannot lock loans; ignore it.
                }
            }
            return ids;
        }

        // =====================================================================
        // Approval posting (spec 11 §9, §11)
        // =====================================================================

        public async Task<EcsRecoveryPosting> BuildPostingAsync(DtoEcsLoanRecovery staged, decimal stagingId, DateTime stagingDate,
            string brCode, decimal yrId, decimal usrId, decimal vocId, int trnType, string narration)
        {
            var appDate = staged.ApplicationDate.Date;

            // --- context checks ---
            var society = await _unitOfWork.General.GetSocietyData(brCode);
            if ((society?.Bank_Type ?? 0).ToInstitutionType() != InstitutionType.ECS)
                throw new EcsRecoveryException("ECS Loan Recovery can be approved only in an ECS society.");

            var openDay = await _unitOfWork.EcsLoanDisbursement.GetOpenBusinessDayAsync(brCode);
            if (appDate != stagingDate.Date || openDay?.Date != appDate)
                throw new EcsRecoveryException(
                    $"ECS loan recovery staged on {appDate:dd-MM-yyyy} must be approved on the same business day " +
                    $"(open business day: {(openDay.HasValue ? openDay.Value.ToString("dd-MM-yyyy") : "none")}). Reject it and re-initiate.");

            if (staged.Lines.Count == 0 || staged.RecoveryAmount <= 0)
                throw new EcsRecoveryException("ECS loan recovery has no loans or no amount. Reject it and re-initiate.");
            if (staged.Lines.Select(l => l.Loan_Id).Distinct().Count() != staged.Lines.Count)
                throw new EcsRecoveryException("ECS loan recovery lists a loan twice. Reject it and re-initiate.");

            // --- recompute (§9) ---
            var preview = await GetRecoveryPreviewAsync(staged.MemId, brCode, appDate, stagingId);
            var byId = preview.Loans.ToDictionary(l => l.Loan_Id);
            var selected = new List<EcsLoanRecoveryLoanVM>();
            foreach (var line in staged.Lines)
            {
                if (!byId.TryGetValue(line.Loan_Id, out var loan))
                    throw new EcsRecoveryException($"Loan {line.Loan_No} is closed or has no dues any more. Reject it and re-initiate.");
                if (loan.IsPending)
                    throw new EcsRecoveryException($"Loan {line.Loan_No} is in another pending recovery. Reject it and re-initiate.");
                selected.Add(loan);
            }

            var invalid = EcsRecoveryAppropriator.Validate(selected, staged.RecoveryAmount);
            if (invalid != null) throw new EcsRecoveryException(invalid + ". Reject it and re-initiate.");

            var appropriation = EcsRecoveryAppropriator.Appropriate(selected, staged.RecoveryAmount).ToDictionary(a => a.Loan_Id);
            foreach (var line in staged.Lines)
            {
                var loan = byId[line.Loan_Id];
                var a = appropriation[line.Loan_Id];
                bool same = Near(line.IntNow, loan.IntNow) && Near(line.PINow, loan.PINow)
                            && Near(line.PIColl, a.PIColl) && Near(line.IntColl, a.IntColl) && Near(line.PrlColl, a.PrlColl)
                            && line.ClosesLoan == a.ClosesLoan;
                if (!same)
                    throw new EcsRecoveryException(
                        $"Loan {line.Loan_No}: the dues have changed since this recovery was initiated. Reject it and re-initiate.");
            }

            // --- build rows (§11) from the recomputed figures ---
            decimal zeroId = decimal.TryParse(brCode + "0000000", out var z) ? z : 0;
            var posting = new EcsRecoveryPosting();
            var masters = (await _unitOfWork.EcsLoanRecovery.GetLoanMastersForUpdateAsync(selected.Select(s => s.Loan_Id), brCode))
                .ToDictionary(m => m.Loan_Id);

            foreach (var loan in selected)
            {
                var a = appropriation[loan.Loan_Id];
                bool penalApplies = EcsInterestCalculator.PenalApplies(loan.PI_Application);

                posting.LoanTrns.Add(new Loan_Trn
                {
                    Trn_Id = 0,                       // assigned by LoanTrnRepository.AddLoanTrnListAsync (MAX+1)
                    Loan_Id = loan.Loan_Id,
                    Demand_Id = zeroId,
                    Trn_Status = "R",
                    Trn_Date = appDate,
                    IntCalc_Amt = loan.IntNow,
                    IntCalc_Date = appDate,           // stamped even when IntNow = 0
                    PICalc_Amt = penalApplies ? loan.PINow : 0,
                    PICalc_Date = penalApplies ? appDate : null,
                    PIColl_Amt = a.PIColl,
                    IntColl_Amt = a.IntColl,
                    PrlColl_Amt = a.PrlColl,
                    Prl_Dem = 0,
                    Soc_Roi = loan.CurrentRoi,
                    SocPI_Rate = loan.CurrentPi,
                    Voc_Id = vocId,
                    Usr_Id = usrId,
                    Yr_Id = yrId,
                    Trn_SlNo = 0,                     // assigned by AddLoanTrnListAsync (MAX per loan + 1)
                    BrCode = brCode,
                    Voc_Status = "V"
                });

                string desc = "Loan No : " + loan.Loan_No?.Trim();
                string narr = narration + " Loan No: " + loan.Loan_No?.Trim();
                double piDue = loan.PIOverdue + loan.PINow, intDue = loan.IntOverdue + loan.IntNow;
                if (a.PrlColl > 0)
                    posting.VoucherTrns.Add(GetModalObject.GetFinVoucherTrObject(vocId, loan.PrlLed_Id, a.PrlColl, 0, trnType, narr, false,
                        usrId, yrId, "P", desc, staged.MemId, brCode, loan.Loan_Id, loan.PrlOutstanding, a.PrlOsAfter));
                if (a.IntColl > 0)
                    posting.VoucherTrns.Add(GetModalObject.GetFinVoucherTrObject(vocId, loan.IntLed_Id, a.IntColl, 0, trnType, narr, false,
                        usrId, yrId, "I", desc, staged.MemId, brCode, loan.Loan_Id, R2(intDue), R2(intDue - a.IntColl)));
                if (a.PIColl > 0)
                    posting.VoucherTrns.Add(GetModalObject.GetFinVoucherTrObject(vocId, loan.PILed_Id, a.PIColl, 0, trnType, narr, false,
                        usrId, yrId, "PI", desc, staged.MemId, brCode, loan.Loan_Id, R2(piDue), R2(piDue - a.PIColl)));

                if (!masters.TryGetValue(loan.Loan_Id, out var master))
                    throw new EcsRecoveryException($"Loan {loan.Loan_No} not found. Reject it and re-initiate.");
                master.LastIntApplication_Date = appDate;
                if (penalApplies) master.LastPIApplication_Date = appDate;
                if (a.ClosesLoan)
                {
                    master.IsAccountClosed = true;
                    master.AccountClosedDate = appDate;
                }
            }

            double posted = R2(posting.VoucherTrns.Sum(v => v.Voc_Rpt));
            if (Math.Abs(posted - R2(staged.RecoveryAmount)) > Tolerance)
                throw new EcsRecoveryException($"Recovery amount ₹{staged.RecoveryAmount:N2} does not match the appropriated ₹{posted:N2}. Reject it and re-initiate.");

            return posting;
        }

        private static bool Near(double x, double y) => Math.Abs(x - y) <= Tolerance;
        private static double R2(double v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
    }
}
