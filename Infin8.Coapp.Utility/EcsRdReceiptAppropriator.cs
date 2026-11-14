using Infin8.Coapp.Dto;

namespace Infin8.Coapp.Utility
{
    /// <summary>
    /// Pure rules of the ECS RD Receipt, acc 2 (spec 14 §5, §6, §7.2, §10.1). Shared by the WASM component (live
    /// grid) and the server (recheck at approval), so both sides always agree. No DB access.
    /// </summary>
    public static class EcsRdReceiptAppropriator
    {
        public const double Tolerance = 0.005;

        /// <summary>Calendar-month difference, ignoring the day (same as EcsDemandCalculator.MonthsBetween).</summary>
        public static int MonthsBetween(DateTime from, DateTime to) => (to.Year - from.Year) * 12 + to.Month - from.Month;

        /// <summary>TSIS ExtensionMethods.AddMonths: a month-end date stays at month end.</summary>
        public static DateTime AddMonthsKeepEnd(DateTime date, int months)
        {
            bool monthEnd = date.Day == DateTime.DaysInMonth(date.Year, date.Month);
            return monthEnd ? date.AddDays(1).AddMonths(months).AddDays(-1) : date.AddMonths(months);
        }

        /// <summary>
        /// Fills N, n, RD pending, advance and the penal figures of <paramref name="line"/> as on
        /// <paramref name="businessDay"/>. Needs the §4 sums and PenalRate already set.
        /// </summary>
        public static void Compute(DtoEcsRdReceiptLine line, DateTime businessDay)
        {
            var b = businessDay.Date;
            double d = line.Instalment;
            int period = Math.Max(0, line.PeriodInMonths);

            // §5 instalments due: the current month's instalment falls due on the 1st.
            bool matured = line.MaturityDate.HasValue && line.MaturityDate.Value.Date <= b;
            int n = matured ? period : MonthsBetween(line.ValueDate, b) + 1;
            n = Math.Clamp(n, 0, period);
            line.InstalmentsDue = n;

            line.RdPending = R2(Math.Max(0, n * d - line.Received));
            line.Advance = R2(Math.Max(0, line.Received - n * d));
            int paid = d > 0 ? (int)Math.Floor(line.Received / d + 1e-9) : 0;
            line.InstalmentsReceived = paid;

            // §6.3 penal: arrear already booked, plus fresh penal for months after the last penal applied.
            line.PiArrear = R2(Math.Max(0, line.PenalCalculated - line.PenalReceived));
            line.PiCurrent = PenalCurrent(line.ValueDate, n, paid, d, line.PenalRate, line.LastPenalAppliedDate);
            line.PiPending = R2(line.PiArrear + line.PiCurrent);
        }

        /// <summary>
        /// TSIS penal (spec 14 §6.2) one month at a time. Instalment j (0-based) falls due in month j after the value
        /// date's month. In month t the instalments due before t and still unpaid (t − paid of them) each carry
        /// D × r / 100. Month t = due − 1 is the business day's month, so the total over t = paid+1 … due−1 is
        /// D × r/100 × k(k+1)/2 with k = (due − 1) − paid. Months up to the month of the last penal applied are skipped.
        /// Rounded once to whole rupees, AwayFromZero.
        /// </summary>
        public static double PenalCurrent(DateTime valueDate, int due, int paid, double instalment, double ratePerMonth,
            DateTime? lastPenalApplied)
        {
            if (ratePerMonth <= 0 || instalment <= 0) return 0;
            int skipUpTo = lastPenalApplied.HasValue ? MonthsBetween(valueDate, lastPenalApplied.Value) : int.MinValue;
            double sum = 0;
            for (int t = paid + 1; t <= due - 1; t++)
            {
                if (t <= skipUpTo) continue;
                sum += (t - paid) * instalment * ratePerMonth / 100.0;
            }
            return Math.Round(sum, 0, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// §7.2: split <paramref name="total"/> over <paramref name="lines"/> in the given order (TD_No), penal first,
        /// then whole instalments. The last line may also take whole instalments in advance, up to the maturity
        /// ceiling. Sets RdReceipt / PiReceipt on every line and returns the unallocated rest.
        /// </summary>
        public static double Appropriate(IList<DtoEcsRdReceiptLine> lines, double total)
        {
            double rem = R2(Math.Max(0, total));
            for (int i = 0; i < lines.Count; i++)
            {
                var l = lines[i];
                l.PiReceipt = R2(Math.Min(rem, l.PiPending));
                rem = R2(rem - l.PiReceipt);

                double d = l.Instalment;
                if (d <= 0) { l.RdReceipt = 0; continue; }

                l.RdReceipt = rem >= l.RdPending - Tolerance ? l.RdPending : WholeInstalments(rem, d);
                rem = R2(rem - l.RdReceipt);

                if (i == lines.Count - 1 && rem > 0)
                {
                    double ceiling = R2(Math.Max(0, l.PeriodInMonths * d - l.Received) - l.RdReceipt);
                    double advance = WholeInstalments(Math.Min(rem, ceiling), d);
                    l.RdReceipt = R2(l.RdReceipt + advance);
                    rem = R2(rem - advance);
                }
            }
            return rem;
        }

        /// <summary>§7.3 checks on an appropriated receipt; null when it may be initiated.</summary>
        public static string? Validate(IList<DtoEcsRdReceiptLine> lines, double total, double unallocated)
        {
            if (total <= 0) return "Total RD receipt is missing";
            if (lines.Count == 0) return "Select at least one RD";
            if (lines.Any(l => l.Locked)) return "An RD in this receipt is pending approval in another transaction";
            if (lines.Select(l => l.TD_Id).Distinct().Count() != lines.Count) return "An RD is listed twice";
            if (Math.Abs(unallocated) > Tolerance) return $"₹{unallocated:N2} is not allocated; change the amount";
            foreach (var l in lines)
            {
                if (l.RdReceipt > 0 && !IsWholeInstalments(l.RdReceipt, l.Instalment) && Math.Abs(l.RdReceipt - l.RdPending) > Tolerance)
                    return $"RD No {l.TD_No}: the RD receipt must be whole instalments of ₹{l.Instalment:N2}";
            }
            if (lines.All(l => l.RdReceipt + l.PiReceipt <= 0)) return "Nothing is allocated to any RD";
            return null;
        }

        /// <summary>termdeposit_trn.NoOfInstalments for an RD receipt.</summary>
        public static int InstalmentCount(double rdReceipt, double instalment) =>
            instalment > 0 ? (int)Math.Floor(rdReceipt / instalment + 1e-9) : 0;

        private static double WholeInstalments(double amount, double instalment) =>
            amount <= 0 ? 0 : R2(Math.Floor(amount / instalment + 1e-9) * instalment);

        private static bool IsWholeInstalments(double amount, double instalment) =>
            instalment > 0 && Math.Abs(amount - WholeInstalments(amount, instalment)) <= Tolerance;

        private static double R2(double v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
    }
}
