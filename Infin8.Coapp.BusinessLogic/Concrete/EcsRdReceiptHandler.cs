using System.Text.Json;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using Infin8.Coapp.Utility.Enums;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>
    /// ECS RD Receipt, acc 2 (spec 14). Loads a member's RDs with pending instalments and penal
    /// (<see cref="EcsRdReceiptAppropriator"/>) and, at approval, rechecks the staged appropriation and builds the
    /// termdeposit_trn / fin_voucher_trn rows.
    /// </summary>
    public class EcsRdReceiptHandler : IEcsRdReceiptHandler
    {
        private const double Tolerance = EcsRdReceiptAppropriator.Tolerance;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        readonly IUnitOfWork _unitOfWork;

        public EcsRdReceiptHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =====================================================================
        // Load (spec 14 §4–§6, §8)
        // =====================================================================

        public async Task<EcsRdReceiptLoadVM> GetRdsAsync(decimal memId, string brCode, DateTime currentDate)
        {
            var openDay = await _unitOfWork.EcsLoanDisbursement.GetOpenBusinessDayAsync(brCode);
            if (openDay?.Date != currentDate.Date)
            {
                return new EcsRdReceiptLoadVM
                {
                    ApplicationDate = currentDate.Date,
                    Message = "The business day has changed since you logged in (open business day: " +
                              (openDay.HasValue ? openDay.Value.ToString("dd-MM-yyyy") : "none") + "). Log in again."
                };
            }
            return await GetRdPreviewAsync(memId, brCode, currentDate);
        }

        public async Task<EcsRdReceiptLoadVM> GetRdPreviewAsync(decimal memId, string brCode, DateTime businessDay,
            decimal excludeStagingId = 0)
        {
            var day = businessDay.Date;
            var result = new EcsRdReceiptLoadVM { ApplicationDate = day };

            var rds = await _unitOfWork.EcsRdReceipt.GetEligibleRdsAsync(memId, brCode);
            if (rds.Count == 0)
            {
                result.Message = "No recurring deposits receivable for this member";
                return result;
            }

            var templates = await _unitOfWork.EcsRdReceipt.GetPenalTemplatesAsync(rds.Select(r => r.TDScheme_Id), brCode);
            var locked = await GetLockedTdIdsAsync(brCode, excludeStagingId);

            foreach (var rd in rds)
            {
                rd.PenalRate = PenalRate(templates, rd);
                EcsRdReceiptAppropriator.Compute(rd, day);
                rd.Locked = locked.Contains(rd.TD_Id);
            }
            result.Lines = rds;
            return result;
        }

        /// <summary>
        /// §6.1 as settled: the scheme's monthly row whose period band holds the RD's period, latest Wef on or before
        /// the value date. No row → 0 (no penal).
        /// </summary>
        private static double PenalRate(List<TermDeposit_Roi_Template> templates, DtoEcsRdReceiptLine rd) =>
            templates.Where(t => t.TDScheme_Id == (int)rd.TDScheme_Id
                                 && t.PeriodBegin <= rd.PeriodInMonths && rd.PeriodInMonths <= t.PeriodEnd
                                 && t.Wef.Date <= rd.ValueDate.Date)
                     .OrderBy(t => t.Wef)
                     .LastOrDefault()?.PenalRateForRD ?? 0;

        private async Task<HashSet<decimal>> GetLockedTdIdsAsync(string brCode, decimal excludeStagingId)
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

        // =====================================================================
        // Approval posting (spec 14 §8, §10)
        // =====================================================================

        public async Task<EcsRdReceiptPosting> BuildPostingAsync(DtoEcsRdReceipt staged, decimal stagingId, DateTime stagingDate,
            string brCode, decimal yrId, decimal usrId, decimal vocId, int trnType, string status)
        {
            var appDate = staged.ApplicationDate.Date;

            // --- context checks ---
            var society = await _unitOfWork.General.GetSocietyData(brCode);
            if ((society?.Bank_Type ?? 0).ToInstitutionType() != InstitutionType.ECS)
                throw new EcsRecoveryException("RD Receipt can be approved only in an ECS society.");

            var openDay = await _unitOfWork.EcsLoanDisbursement.GetOpenBusinessDayAsync(brCode);
            if (appDate != stagingDate.Date || openDay?.Date != appDate)
                throw new EcsRecoveryException(
                    $"RD receipt staged on {appDate:dd-MM-yyyy} must be approved on the same business day " +
                    $"(open business day: {(openDay.HasValue ? openDay.Value.ToString("dd-MM-yyyy") : "none")}). Reject it and re-initiate.");

            if (staged.Lines.Count == 0 || staged.ReceiptAmount <= 0)
                throw new EcsRecoveryException("RD receipt has no RDs or no amount. Reject it and re-initiate.");
            if (staged.Lines.Select(l => l.TD_Id).Distinct().Count() != staged.Lines.Count)
                throw new EcsRecoveryException("RD receipt lists an RD twice. Reject it and re-initiate.");

            // --- recompute (§8) in the staged order ---
            var preview = await GetRdPreviewAsync(staged.MemId, brCode, appDate, stagingId);
            var byId = preview.Lines.ToDictionary(l => l.TD_Id);
            var fresh = new List<DtoEcsRdReceiptLine>();
            foreach (var line in staged.Lines)
            {
                if (!byId.TryGetValue(line.TD_Id, out var rd))
                    throw new EcsRecoveryException($"RD No {line.TD_No} is closed, withdrawn or no longer this member's. Reject it and re-initiate.");
                if (rd.Locked)
                    throw new EcsRecoveryException($"RD No {line.TD_No} is in another pending RD receipt. Reject it and re-initiate.");
                fresh.Add(rd);
            }

            double unallocated = EcsRdReceiptAppropriator.Appropriate(fresh, staged.ReceiptAmount);
            var invalid = EcsRdReceiptAppropriator.Validate(fresh, staged.ReceiptAmount, unallocated);
            if (invalid != null) throw new EcsRecoveryException(invalid + ". Reject it and re-initiate.");

            for (int i = 0; i < staged.Lines.Count; i++)
            {
                var s = staged.Lines[i];
                var f = fresh[i];
                bool same = Near(s.RdPending, f.RdPending) && Near(s.PiArrear, f.PiArrear) && Near(s.PiCurrent, f.PiCurrent)
                            && Near(s.RdReceipt, f.RdReceipt) && Near(s.PiReceipt, f.PiReceipt);
                if (!same)
                    throw new EcsRecoveryException(
                        $"RD No {s.TD_No}: the dues have changed since this receipt was initiated. Reject it and re-initiate.");
            }

            // --- build rows (§10) from the recomputed figures ---
            var ledgers = await _unitOfWork.EcsRdReceipt.GetRdLedgersAsync(fresh.Select(f => f.TD_Id), brCode);
            var posting = new EcsRdReceiptPosting();
            string member = $"M.No {staged.MemberNo?.Trim()} {staged.MemberName?.Trim()}";

            foreach (var rd in fresh)
            {
                if (rd.RdReceipt <= 0 && rd.PiReceipt <= 0) continue;

                ledgers.TryGetValue(rd.TD_Id, out var led);
                if (rd.RdReceipt > 0 && led.LedId == 0)
                    throw new EcsRecoveryException($"RD No {rd.TD_No}: the scheme has no RD ledger. Set it on the scheme first.");
                if (rd.PiReceipt > 0 && led.PiLedId == 0)
                    throw new EcsRecoveryException($"RD No {rd.TD_No}: the scheme has no penal interest ledger. Set it on the scheme first.");

                int instalments = EcsRdReceiptAppropriator.InstalmentCount(rd.RdReceipt, rd.Instalment);
                DateTime? lastInstalment = null;
                if (rd.RdReceipt > 0)
                    lastInstalment = EcsRdReceiptAppropriator.AddMonthsKeepEnd((rd.LastInstalmentDate ?? rd.ValueDate).Date, instalments);

                posting.TermDepositTrns.Add(new TermDeposit_Trn
                {
                    TDTrn_Id = 0,                      // assigned by AddTermDepositTrnListAsync (MAX+1)
                    Trn_Date = appDate,
                    TD_Id = rd.TD_Id,
                    DepositDemandAmount = 0,
                    DepositReceiptAmount = rd.RdReceipt,
                    MaturityAmount = 0,
                    InterestCalculatedAmount = 0,
                    InterestAppliedDate = null,
                    InterestPaidAmount = 0,
                    DepositPaidAmount = 0,
                    PenalCalculatedAmount = rd.PiCurrent,
                    PenalAppliedDate = rd.PiCurrent > 0 ? appDate : null,
                    PenalReceivedAmount = rd.PiReceipt,
                    NoOfInstalments = instalments,
                    InstalmentAmount = rd.RdReceipt > 0 ? (int)Math.Round(rd.Instalment, MidpointRounding.AwayFromZero) : 0,
                    LastInstalmentDate = lastInstalment,
                    LastPaymentDate = null,
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

                string desc = "RD No " + rd.TD_No?.Trim();
                string narr = member + " " + desc;
                if (rd.RdReceipt > 0)
                    posting.VoucherTrns.Add(GetModalObject.GetFinVoucherTrObject(vocId, led.LedId, rd.RdReceipt, 0, trnType, narr, false,
                        usrId, yrId, status, desc, staged.MemId, brCode, 0, 0, 0));
                if (rd.PiReceipt > 0)
                    posting.VoucherTrns.Add(GetModalObject.GetFinVoucherTrObject(vocId, led.PiLedId, rd.PiReceipt, 0, trnType, narr, false,
                        usrId, yrId, status, desc, staged.MemId, brCode, 0, 0, 0));
            }

            double posted = R2(posting.VoucherTrns.Sum(v => v.Voc_Rpt));
            if (Math.Abs(posted - R2(staged.ReceiptAmount)) > Tolerance)
                throw new EcsRecoveryException($"RD receipt amount ₹{staged.ReceiptAmount:N2} does not match the appropriated ₹{posted:N2}. Reject it and re-initiate.");

            return posting;
        }

        public async Task<string?> GetRdBlockingDeleteAsync(decimal vocId, string brCode) =>
            await _unitOfWork.EcsRdReceipt.GetRdWithLaterRowsAsync(vocId, brCode);

        private static bool Near(double x, double y) => Math.Abs(x - y) <= Tolerance;
        private static double R2(double v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
    }
}
