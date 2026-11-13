namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>One loan_trn row reduced to the figures that move the interest / penal base.</summary>
    public sealed record EcsTrnDelta(DateTime Date, double Disb, double PrlDem, double PrlColl, double IntCalc, double IntColl);

    /// <summary>A loan_roi row (Agency 'S'): rates in force from <see cref="Wef"/> inclusive.</summary>
    public sealed record EcsRateEvent(DateTime Wef, double Roi, double Pi);

    /// <summary>Accrued amount for one head and the period it covers (To is inclusive).</summary>
    public sealed record EcsAccrual(double Amount, DateTime? From, DateTime? To, int Days);

    public enum EcsAccrualHead { Interest, Penal }

    /// <summary>
    /// Segment-wise diminishing-product accrual for ECS loans (spec 11 §6). Pure, no DB access, so it can be
    /// checked by hand: Σ over segments (base × rate × days / 36500), rounded once to the rupee with
    /// AwayFromZero.
    /// </summary>
    public static class EcsInterestCalculator
    {
        /// <param name="application">int_application (interest) or pi_application (penal) of the scheme.</param>
        /// <param name="rows">All valid loan_trn rows of the loan with Trn_Date ≤ application date.</param>
        /// <param name="rates">loan_roi rows of the loan (any order).</param>
        /// <param name="from">Last interest / penal calculation date (the disbursement date the first time).</param>
        /// <param name="applicationDate">The business day; the period ends the day before (inclusive).</param>
        public static EcsAccrual Calculate(EcsAccrualHead head, int application, IReadOnlyCollection<EcsTrnDelta> rows,
            IReadOnlyCollection<EcsRateEvent> rates, double defaultRoi, double defaultPi, DateTime from, DateTime applicationDate)
        {
            from = from.Date;
            var appDate = applicationDate.Date;

            // pi_application 1 = no penal at all (0 / unset is treated the same); the caller does not stamp PICalc_Date.
            if (head == EcsAccrualHead.Penal && !PenalApplies(application)) return new EcsAccrual(0, null, null, 0);
            if (from >= appDate) return new EcsAccrual(0, from, appDate.AddDays(-1), 0);

            var orderedRates = rates.OrderBy(r => r.Wef.Date).ToList();
            var inPeriod = rows.Where(r => r.Date.Date >= from && r.Date.Date < appDate).ToList();

            var cuts = new SortedSet<DateTime> { from, appDate };
            foreach (var r in inPeriod) cuts.Add(r.Date.Date);
            foreach (var r in orderedRates.Where(r => r.Wef.Date > from && r.Wef.Date < appDate)) cuts.Add(r.Wef.Date);

            var points = cuts.ToList();
            double product = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                DateTime start = points[i], end = points[i + 1];
                int days = (end - start).Days;
                if (days <= 0) continue;

                // Balance in force on the segment start: every row dated on or before it (inclusive).
                var upTo = rows.Where(r => r.Date.Date <= start).ToList();
                double disb = upTo.Sum(r => r.Disb), dem = upTo.Sum(r => r.PrlDem), coll = upTo.Sum(r => r.PrlColl);
                double intCalc = upTo.Sum(r => r.IntCalc), intColl = upTo.Sum(r => r.IntColl);

                double baseAmt = head == EcsAccrualHead.Interest
                    ? InterestBase(application, disb, dem, coll)
                    : PenalBase(application, dem, coll, intCalc, intColl);
                if (baseAmt <= 0) continue;

                var rate = orderedRates.LastOrDefault(r => r.Wef.Date <= start);
                double pct = head == EcsAccrualHead.Interest ? (rate?.Roi ?? defaultRoi) : (rate?.Pi ?? defaultPi);
                if (pct <= 0) continue;

                product += baseAmt * pct * days / 36500.0;
            }

            double amount = Math.Round(product, 0, MidpointRounding.AwayFromZero);
            return new EcsAccrual(amount, from, appDate.AddDays(-1), (appDate - from).Days);
        }

        /// <summary>Penal is charged only for pi_application 2 or 3; 1 (and unset 0) means no penal.</summary>
        public static bool PenalApplies(int piApplication) => piApplication is 2 or 3;

        /// <summary>Rate in force on <paramref name="date"/> (latest Roi_Wef ≤ date), else the loan master's.</summary>
        public static (double Roi, double Pi) RateOn(IReadOnlyCollection<EcsRateEvent> rates, double defaultRoi, double defaultPi, DateTime date)
        {
            var r = rates.Where(x => x.Wef.Date <= date.Date).OrderBy(x => x.Wef).LastOrDefault();
            return r == null ? (defaultRoi, defaultPi) : (r.Roi, r.Pi);
        }

        /// <summary>
        /// int_application 1: non-overdue principal, MIN(Disb − Demand, Disb − Collection), floored at 0
        /// (the second term takes over only after advance principal payment).
        /// int_application 2 (and anything else): principal outstanding.
        /// </summary>
        private static double InterestBase(int application, double disb, double dem, double coll) =>
            application == 1
                ? Math.Max(0, Math.Min(disb - dem, disb - coll))
                : Math.Max(0, disb - coll);

        /// <summary>pi_application 2: principal overdue; 3: principal overdue + booked interest overdue.</summary>
        private static double PenalBase(int application, double dem, double coll, double intCalc, double intColl)
        {
            double prlOd = Math.Max(0, dem - coll);
            return application switch
            {
                2 => prlOd,
                3 => prlOd + Math.Max(0, intCalc - intColl),
                _ => 0
            };
        }
    }
}
