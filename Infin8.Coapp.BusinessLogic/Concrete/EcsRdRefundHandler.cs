using System.Text.Json;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using Infin8.Coapp.Utility.Enums;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>
    /// ECS RD Refund, acc 10 (spec 16). Loads a member's open RDs with the payable figures
    /// (<see cref="EcsRdRefundCalculator"/>) and, at approval, rechecks the staged figures and builds the closing
    /// termdeposit_trn / fin_voucher_trn rows.
    /// </summary>
    public class EcsRdRefundHandler : IEcsRdRefundHandler
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        readonly IUnitOfWork _unitOfWork;

        public EcsRdRefundHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =====================================================================
        // Load (spec 16 §4–§8)
        // =====================================================================

        public async Task<EcsRdRefundLoadVM> GetRdsAsync(decimal memId, string brCode, DateTime currentDate)
        {
            var openDay = await _unitOfWork.EcsLoanDisbursement.GetOpenBusinessDayAsync(brCode);
            if (openDay?.Date != currentDate.Date)
            {
                return new EcsRdRefundLoadVM
                {
                    ApplicationDate = currentDate.Date,
                    Message = "The business day has changed since you logged in (open business day: " +
                              (openDay.HasValue ? openDay.Value.ToString("dd-MM-yyyy") : "none") + "). Log in again."
                };
            }
            return await GetRdPreviewAsync(memId, brCode, currentDate);
        }

        public async Task<EcsRdRefundLoadVM> GetRdPreviewAsync(decimal memId, string brCode, DateTime businessDay,
            decimal excludeStagingId = 0)
        {
            var day = businessDay.Date;
            var result = new EcsRdRefundLoadVM { ApplicationDate = day };

            // §4.1 is the same rule as the RD receipt's list: open RD of the member, no payout made yet.
            var rds = await _unitOfWork.EcsRdReceipt.GetEligibleRdsAsync(memId, brCode);
            if (rds.Count == 0)
            {
                result.Message = "The member has no open recurring deposits";
                return result;
            }

            var tdIds = rds.Select(r => r.TD_Id).ToList();
            var roiRows = await _unitOfWork.EcsRdReceipt.GetPenalTemplatesAsync(rds.Select(r => r.TDScheme_Id), brCode);
            var fcRows = await _unitOfWork.EcsRdRefund.GetRdForeclosureTemplatesAsync(brCode);
            var liened = await _unitOfWork.EcsRdRefund.GetLienedTdIdsAsync(tdIds, brCode);
            var demand = await _unitOfWork.EcsRdRefund.GetUnrecoveredDemandAsync(tdIds, brCode);
            var lockedByRefund = await GetPendingRefundTdIdsAsync(brCode, excludeStagingId);
            var lockedByReceipt = await GetPendingReceiptTdIdsAsync(brCode, excludeStagingId);
            lockedByReceipt.UnionWith(await GetPendingRdLoanTdIdsAsync(brCode));

            foreach (var rd in rds)
            {
                var line = new DtoEcsRdRefundLine
                {
                    TD_Id = rd.TD_Id,
                    TD_No = rd.TD_No?.Trim(),
                    TDScheme_Id = (long)rd.TDScheme_Id,
                    Instalment = rd.Instalment,
                    ValueDate = rd.ValueDate,
                    PeriodInMonths = rd.PeriodInMonths,
                    RateOfInterest = rd.RateOfInterest,
                    MaturityDate = rd.MaturityDate,
                    MaturityAmount = rd.MaturityAmount,
                    PenalRate = PenalRate(roiRows, rd),
                    Received = rd.Received,
                    PenalCalculated = rd.PenalCalculated,
                    PenalReceived = rd.PenalReceived,
                    LastPenalAppliedDate = rd.LastPenalAppliedDate
                };

                if (liened.Contains(rd.TD_Id))
                    line.Block = $"RD No {line.TD_No} is pledged for a loan; close the loan first";
                else if (lockedByRefund.Contains(rd.TD_Id) || lockedByReceipt.Contains(rd.TD_Id))
                    line.Block = $"RD No {line.TD_No} is in another pending transaction";
                else if (demand.TryGetValue(rd.TD_Id, out var due))
                    line.Block = $"RD No {line.TD_No} is in the demand of {(due.HasValue ? due.Value.ToString("dd-MM-yyyy") : "an earlier date")}, not yet recovered";

                EcsRdRefundCalculator.Compute(line, day, roiRows, fcRows);
                result.Lines.Add(line);
            }
            return result;
        }

        /// <summary>Spec 14 §6.1 as settled: the scheme's monthly row whose band holds the RD's period, latest Wef ≤ value date.</summary>
        private static double PenalRate(List<TermDeposit_Roi_Template> templates, DtoEcsRdReceiptLine rd) =>
            templates.Where(t => t.TDScheme_Id == (long)rd.TDScheme_Id
                                 && t.PeriodBegin <= rd.PeriodInMonths && rd.PeriodInMonths <= t.PeriodEnd
                                 && t.Wef.Date <= rd.ValueDate.Date)
                     .OrderBy(t => t.Wef)
                     .LastOrDefault()?.PenalRateForRD ?? 0;

        public async Task<HashSet<decimal>> GetPendingRefundTdIdsAsync(string brCode, decimal excludeStagingId = 0)
        {
            var ids = new HashSet<decimal>();
            foreach (var json in await _unitOfWork.EcsRdRefund.GetPendingRefundDataAsync(brCode, excludeStagingId))
            {
                try
                {
                    var staged = JsonSerializer.Deserialize<DtoEcsRdRefund>(json, JsonOptions);
                    foreach (var line in staged?.Lines ?? new()) ids.Add(line.TD_Id);
                }
                catch (JsonException)
                {
                    // A malformed staging row cannot lock RDs; ignore it.
                }
            }
            return ids;
        }

        private async Task<HashSet<decimal>> GetPendingReceiptTdIdsAsync(string brCode, decimal excludeStagingId)
        {
            var ids = new HashSet<decimal>();
            foreach (var json in await _unitOfWork.EcsRdReceipt.GetPendingReceiptDataAsync(brCode, excludeStagingId))
            {
                try
                {
                    var staged = JsonSerializer.Deserialize<DtoEcsRdReceipt>(json, JsonOptions);
                    foreach (var line in staged?.Lines ?? new()) ids.Add(line.TD_Id);
                }
                catch (JsonException)
                {
                    // A malformed staging row cannot lock RDs; ignore it.
                }
            }
            return ids;
        }

        /// <summary>Spec 17 §9: an RD pledged in a pending Loan on RD line (acc 14) cannot be refunded.</summary>
        private async Task<HashSet<decimal>> GetPendingRdLoanTdIdsAsync(string brCode)
        {
            var ids = new HashSet<decimal>();
            foreach (var json in await _unitOfWork.EcsRdLoan.GetPendingLoanDataAsync(brCode, 0))
            {
                try
                {
                    var staged = JsonSerializer.Deserialize<DtoEcsRdLoan>(json, JsonOptions);
                    foreach (var line in staged?.Lines ?? new()) ids.Add(line.TD_Id);
                }
                catch (JsonException)
                {
                    // A malformed staging row cannot lock RDs; ignore it.
                }
            }
            return ids;
        }

        // =====================================================================
        // Approval posting (spec 16 §9, §11)
        // =====================================================================

        public async Task<EcsRdRefundPosting> BuildPostingAsync(DtoEcsRdRefund staged, decimal stagingId, DateTime stagingDate,
            string brCode, decimal yrId, decimal usrId, decimal vocId, int trnType, string status)
        {
            var appDate = staged.ApplicationDate.Date;

            // --- context checks ---
            var society = await _unitOfWork.General.GetSocietyData(brCode);
            if ((society?.Bank_Type ?? 0).ToInstitutionType() != InstitutionType.ECS)
                throw new EcsRecoveryException("RD Refund can be approved only in an ECS society.");

            var openDay = await _unitOfWork.EcsLoanDisbursement.GetOpenBusinessDayAsync(brCode);
            if (appDate != stagingDate.Date || openDay?.Date != appDate)
                throw new EcsRecoveryException(
                    $"RD refund staged on {appDate:dd-MM-yyyy} must be approved on the same business day " +
                    $"(open business day: {(openDay.HasValue ? openDay.Value.ToString("dd-MM-yyyy") : "none")}). Reject it and re-initiate.");

            if (staged.Lines.Count == 0)
                throw new EcsRecoveryException("RD refund has no RDs. Reject it and re-initiate.");
            if (staged.Lines.Select(l => l.TD_Id).Distinct().Count() != staged.Lines.Count)
                throw new EcsRecoveryException("RD refund lists an RD twice. Reject it and re-initiate.");

            // --- recompute (§5, §6) and compare with the staged figures (§9) ---
            var preview = await GetRdPreviewAsync(staged.MemId, brCode, appDate, stagingId);
            var byId = preview.Lines.ToDictionary(l => l.TD_Id);
            var fresh = new DtoEcsRdRefund
            {
                MemId = staged.MemId,
                MemberNo = staged.MemberNo,
                MemberName = staged.MemberName,
                ApplicationDate = appDate,
                CashOrAdjustment = staged.CashOrAdjustment
            };
            foreach (var s in staged.Lines)
            {
                if (!byId.TryGetValue(s.TD_Id, out var f))
                    throw new EcsRecoveryException($"RD No {s.TD_No?.Trim()} is not open for refund. Reject it and re-initiate.");
                if (!string.IsNullOrEmpty(f.Block))
                    throw new EcsRecoveryException(f.Block + ". Reject it and re-initiate.");

                string? changed =
                    !EcsRdRefundCalculator.Near(s.Received, f.Received) ? "the deposit received" :
                    s.MonthsRun != f.MonthsRun ? "the months run" :
                    !EcsRdRefundCalculator.Near(s.AppliedRoi, f.AppliedRoi) ? "the applied rate of interest" :
                    !EcsRdRefundCalculator.Near(s.Interest, f.Interest) ? "the interest payable" :
                    !EcsRdRefundCalculator.Near(s.PenalPending, f.PenalPending) ? "the penal pending" :
                    !EcsRdRefundCalculator.Near(s.TotalPayable, f.TotalPayable) ? "the total payable" : null;
                if (changed != null)
                    throw new EcsRecoveryException($"RD No {f.TD_No}: {changed} has changed since this refund was initiated. Reject it and re-initiate.");
                fresh.Lines.Add(f);
            }

            EcsRdRefundCalculator.Totals(fresh);
            var invalid = EcsRdRefundCalculator.Validate(fresh);
            if (invalid != null) throw new EcsRecoveryException(invalid + ". Reject it and re-initiate.");
            if (!EcsRdRefundCalculator.Near(fresh.NetPayable, staged.NetPayable))
                throw new EcsRecoveryException($"RD refund net payable ₹{staged.NetPayable:N2} no longer matches ₹{fresh.NetPayable:N2}. Reject it and re-initiate.");

            // --- build rows (§11) ---
            var ledgers = await _unitOfWork.EcsRdRefund.GetRdLedgersAsync(fresh.Lines.Select(l => l.TD_Id), brCode);
            var posting = new EcsRdRefundPosting();
            bool cash = trnType == 1;
            const int Adjustment = 2;
            string member = $"{staged.MemberNo?.Trim()} {staged.MemberName?.Trim()}".Trim();

            foreach (var rd in fresh.Lines)
            {
                ledgers.TryGetValue(rd.TD_Id, out var led);
                if (rd.Received > 0 && led.LedId == 0)
                    throw new EcsRecoveryException($"RD No {rd.TD_No}: the scheme has no RD ledger. Set it on the scheme first.");
                if (rd.Interest > 0 && led.IntLedId == 0)
                    throw new EcsRecoveryException($"RD No {rd.TD_No}: the scheme has no RD interest ledger. Set it on the scheme first.");
                if (rd.PenalPending > 0 && led.PiLedId == 0)
                    throw new EcsRecoveryException($"RD No {rd.TD_No}: the scheme has no penal interest ledger. Set it on the scheme first.");

                posting.TermDepositTrns.Add(new TermDeposit_Trn
                {
                    TDTrn_Id = 0,                      // assigned by AddTermDepositTrnListAsync (MAX+1)
                    Trn_Date = appDate,
                    TD_Id = rd.TD_Id,
                    DepositDemandAmount = 0,
                    DepositReceiptAmount = 0,
                    MaturityAmount = 0,
                    InterestCalculatedAmount = rd.Interest,
                    InterestAppliedDate = rd.InterestAppliedDate,
                    InterestPaidAmount = rd.Interest,
                    DepositPaidAmount = rd.Received,
                    PenalCalculatedAmount = rd.PenalCurrent,
                    PenalAppliedDate = rd.PenalCurrent > 0 ? appDate : null,
                    PenalReceivedAmount = rd.PenalPending,
                    NoOfInstalments = 0,
                    InstalmentAmount = 0,
                    LastInstalmentDate = null,
                    LastPaymentDate = appDate,
                    TD_OE = false,
                    TD_Delete = false,
                    Voc_Id = vocId,
                    Usr_Id = usrId,
                    Yr_Id = yrId,
                    Trn_SlNo = 0,                      // assigned by AddTermDepositTrnListAsync (MAX per TD_Id + 1)
                    Demand_Id = 0,
                    TDCalc_Id = 0,
                    BrCode = brCode,
                    Voc_Status = "V"
                });
                posting.ClosedTdIds.Add(rd.TD_Id);

                string desc = "RD No " + rd.TD_No;
                string narr = (member + " " + desc).Trim();
                void Debit(decimal ledId, double amount, int type)
                {
                    if (amount > 0)
                        posting.VoucherTrns.Add(GetModalObject.GetFinVoucherTrObject(vocId, ledId, 0, R2(amount), type, narr, false,
                            usrId, yrId, status, desc, staged.MemId, brCode, 0, 0, 0));
                }

                if (cash && rd.PenalPending > 0)
                {
                    // §11.3 cash with penal: the penal is met by adjustment out of the deposit, then the interest.
                    double fromDeposit = Math.Min(rd.PenalPending, rd.Received);
                    double fromInterest = R2(rd.PenalPending - fromDeposit);
                    Debit(led.LedId, fromDeposit, Adjustment);
                    Debit(led.LedId, R2(rd.Received - fromDeposit), 1);
                    Debit(led.IntLedId, fromInterest, Adjustment);
                    Debit(led.IntLedId, R2(rd.Interest - fromInterest), 1);
                }
                else
                {
                    Debit(led.LedId, rd.Received, trnType);
                    Debit(led.IntLedId, rd.Interest, trnType);
                }
                if (rd.PenalPending > 0)
                    posting.VoucherTrns.Add(GetModalObject.GetFinVoucherTrObject(vocId, led.PiLedId, R2(rd.PenalPending), 0,
                        cash ? Adjustment : trnType, narr, false, usrId, yrId, status, desc, staged.MemId, brCode, 0, 0, 0));
            }

            // Final check (§11.3): the lines of the staged type net to the payable.
            var ofType = posting.VoucherTrns.Where(v => !cash || v.Voc_Trn_Type == 1).ToList();
            double net = R2(ofType.Sum(v => v.Voc_Pmt) - ofType.Sum(v => v.Voc_Rpt));
            if (!EcsRdRefundCalculator.Near(net, fresh.NetPayable))
                throw new EcsRecoveryException($"RD refund posting ₹{net:N2} does not match the net payable ₹{fresh.NetPayable:N2}. Reject it and re-initiate.");

            return posting;
        }

        private static double R2(double v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
    }
}
