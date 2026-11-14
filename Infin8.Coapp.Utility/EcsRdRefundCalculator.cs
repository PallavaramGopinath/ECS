using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.Utility
{
    /// <summary>
    /// Pure rules of the ECS RD Refund, acc 10 (spec 16 §5, §6, §8). Shared by the WASM component (totals) and the server
    /// (load and recheck at approval), so both sides always agree. No DB access.
    /// </summary>
    public static class EcsRdRefundCalculator
    {
        public const double Tolerance = 0.005;

        /// <summary>termdeposit_fctemplate.TD_Type of a recurring deposit.</summary>
        public const int RdForeclosureType = 4;

        /// <summary>§5.2 m: completed months from the value date, AddMonthsKeepEnd(valueDate, m) ≤ refund date.</summary>
        public static int CompletedMonths(DateTime valueDate, DateTime refundDate)
        {
            var v = valueDate.Date;
            var r = refundDate.Date;
            int m = Math.Max(0, EcsRdReceiptAppropriator.MonthsBetween(v, r));
            while (m > 0 && EcsRdReceiptAppropriator.AddMonthsKeepEnd(v, m) > r) m--;
            return m;
        }

        /// <summary>§5.3: TD_Type 4, not deleted, latest Wef on or before the value date. None → null (0 deducted).</summary>
        public static TermDeposit_FcTemplate? PickForeclosure(IEnumerable<TermDeposit_FcTemplate> rows, DateTime valueDate) =>
            rows.Where(f => f.TD_Type == RdForeclosureType && !f.TDfc_Delete
                            && f.TDfc_Wef.HasValue && f.TDfc_Wef.Value.Date <= valueDate.Date)
                .OrderBy(f => f.TDfc_Wef)
                .ThenBy(f => f.TDfc_Id)
                .LastOrDefault();

        /// <summary>§5.4 premature interest: D × n × (n + 1) × ROI / 2400, rounded once to a rupee, AwayFromZero.</summary>
        public static double SimpleInterest(double instalment, int months, double roi)
        {
            if (instalment <= 0 || months <= 0 || roi <= 0) return 0;
            decimal x = (decimal)instalment * months * (months + 1) * (decimal)roi / 2400m;
            return (double)Math.Round(x, 0, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Fills §5.2–§5.5 and §6 on <paramref name="line"/> as on <paramref name="refundDate"/>. Needs the §4.1 sums and
        /// PenalRate already set. Sets <see cref="DtoEcsRdRefundLine.Block"/> when no ROI row exists for n (§5.3); other
        /// blocks (lien, locks, demand) are the server's.
        /// </summary>
        public static void Compute(DtoEcsRdRefundLine line, DateTime refundDate,
            IEnumerable<TermDeposit_Roi_Template> roiRows, IEnumerable<TermDeposit_FcTemplate> fcRows)
        {
            var day = refundDate.Date;
            double d = line.Instalment;
            int period = Math.Max(0, line.PeriodInMonths);

            int i = EcsRdReceiptAppropriator.InstalmentCount(line.Received, d);
            line.InstalmentsReceived = i;
            line.IsMatured = line.MaturityDate.HasValue && line.MaturityDate.Value.Date <= day;
            bool fullyPaid = d > 0 && line.Received >= d * period - Tolerance;

            line.BandRoi = 0;
            line.ForeclosureRoi = 0;
            line.AppliedRoi = 0;
            line.Interest = 0;

            if (line.IsMatured && fullyPaid)
            {
                // Matured: the contracted maturity amount (§5.4).
                line.MonthsRun = period;
                line.BandRoi = line.RateOfInterest;
                line.AppliedRoi = line.RateOfInterest;
                line.Interest = R2(Math.Max(0, line.MaturityAmount - line.Received));
                line.InterestAppliedDate = line.MaturityDate!.Value.Date;
            }
            else
            {
                // Premature, or matured but short (§5.2, §5.4, §15.4).
                int n = line.IsMatured ? Math.Min(i, period) : Math.Min(Math.Min(i, CompletedMonths(line.ValueDate, day)), period);
                line.MonthsRun = Math.Max(0, n);
                line.InterestAppliedDate = line.IsMatured ? line.MaturityDate!.Value.Date : day;
                if (line.MonthsRun > 0)
                {
                    var band = EcsRdCreateCalculator.PickRoi(roiRows, line.TDScheme_Id, line.MonthsRun, line.ValueDate);
                    if (band == null)
                    {
                        line.Block ??= $"Rate of interest is not defined for scheme {line.TDScheme_Id} for {line.MonthsRun} months";
                    }
                    else
                    {
                        line.BandRoi = band.Roi;
                        line.ForeclosureRoi = line.IsMatured ? 0 : PickForeclosure(fcRows, line.ValueDate)?.TDfc_Roi ?? 0;
                        line.AppliedRoi = Math.Max(0, line.BandRoi - line.ForeclosureRoi);
                        line.Interest = SimpleInterest(d, line.MonthsRun, line.AppliedRoi);
                    }
                }
            }

            // §6 penal pending, exactly as the RD receipt computes it on the same day.
            var receiptView = new DtoEcsRdReceiptLine
            {
                TD_Id = line.TD_Id,
                Instalment = d,
                ValueDate = line.ValueDate,
                PeriodInMonths = line.PeriodInMonths,
                MaturityDate = line.MaturityDate,
                PenalRate = line.PenalRate,
                Received = line.Received,
                PenalCalculated = line.PenalCalculated,
                PenalReceived = line.PenalReceived,
                LastPenalAppliedDate = line.LastPenalAppliedDate
            };
            EcsRdReceiptAppropriator.Compute(receiptView, day);
            line.PenalArrear = receiptView.PiArrear;
            line.PenalCurrent = receiptView.PiCurrent;
            line.PenalPending = receiptView.PiPending;

            line.TotalPayable = R2(line.Received + line.Interest - line.PenalPending);
        }

        /// <summary>§4.4 totals of the selected lines into <paramref name="dto"/>.</summary>
        public static void Totals(DtoEcsRdRefund dto)
        {
            dto.TotalReceived = R2(dto.Lines.Sum(l => l.Received));
            dto.TotalInterest = R2(dto.Lines.Sum(l => l.Interest));
            dto.TotalPenal = R2(dto.Lines.Sum(l => l.PenalPending));
            dto.NetPayable = R2(dto.TotalReceived + dto.TotalInterest - dto.TotalPenal);
        }

        /// <summary>§8 rules 1, 3, 4, 5, 6, 7, 8 on a refund whose totals are set; null when it may be initiated.</summary>
        public static string? Validate(DtoEcsRdRefund dto)
        {
            if (dto.Lines.Count == 0) return "Select the recurring deposit(s) to refund";
            if (dto.Lines.Select(l => l.TD_Id).Distinct().Count() != dto.Lines.Count) return "An RD is listed twice";
            foreach (var l in dto.Lines)
            {
                if (!string.IsNullOrEmpty(l.Block)) return l.Block;
                if (l.TotalPayable < -Tolerance) return $"Penal on RD No {l.TD_No?.Trim()} exceeds the amount payable";
            }
            if (dto.NetPayable <= Tolerance) return "Refund amount is nil";
            return null;
        }

        public static bool Near(double x, double y) => Math.Abs(x - y) <= Tolerance;

        private static double R2(double v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
    }
}
