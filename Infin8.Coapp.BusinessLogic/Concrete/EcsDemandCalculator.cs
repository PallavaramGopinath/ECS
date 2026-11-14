namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>One loan_trn row (valid, Trn_Date before the due date) as the demand calculator needs it.</summary>
    public sealed record EcsDemandLoanTrn(DateTime Date, string? Status, DateTime? DisbDate, double Disb, double PrlDem,
        double PrlColl, double IntCalc, double IntColl, DateTime? IntCalcDate, double PICalc, double PIColl, DateTime? PICalcDate);

    /// <summary>An open Loan_Type 1 loan with its scheme rules, ledger rows and rate history.</summary>
    public sealed record EcsDemandLoan(decimal LoanId, int InstType, int DemFrequency, int IntApplication, int PIApplication,
        int AdvPrlApplication, double InstAmt, int PrlPrd, DateTime? FirstPrlDueDate, double Roi, double Pi,
        IReadOnlyList<EcsDemandLoanTrn> Trns, IReadOnlyCollection<EcsRateEvent> Rates);

    public sealed record EcsLoanDemandFigures(double Outstanding, double PrlArrear, double IntArrear, double PiArrear,
        double PrlCurrent, double IntCurrent, double PiCurrent)
    {
        public double Total => EcsDemandCalculator.R2(PrlArrear + IntArrear + PiArrear + PrlCurrent + IntCurrent + PiCurrent);
    }

    /// <summary>
    /// A deposit_masters row for one member, with the member's option (latest Wef ≤ due date, active), the penal
    /// rate from deposit_roi_templates (latest Wef ≤ due date) and the member's deposit_trn sums before the due date.
    /// </summary>
    public sealed record EcsDemandDeposit(int DepositId, double Percentage, double FixedAmount, double MaximumAmount,
        string? Frequency, int DemandMonth, bool ArrearApplicable, double? OptionAmount, double PenalRate,
        double SumDemand, double SumReceipt, double SumPiCalc, double SumPiReceived);

    public sealed record EcsDepositDemandFigures(double Arrear, double Current, double PiArrear, double PiCurrent)
    {
        public double Total => EcsDemandCalculator.R2(Arrear + Current + PiArrear + PiCurrent);
    }

    /// <summary>An open RD with its termdeposit_trn sums before the due date and its RD penal rate.</summary>
    public sealed record EcsDemandRd(decimal TdId, DateTime ValueDate, int PeriodInMonths, double Instalment,
        double Received, double PenalCalc, double PenalReceived, double PenalRate);

    public sealed record EcsRdDemandFigures(int Instalment, int MissedInstalments, double Demand, double PiArrear, double PiCurrent)
    {
        public double Total => EcsDemandCalculator.R2(Demand + PiArrear + PiCurrent);
    }

    /// <summary>
    /// ECS monthly demand rules (spec 12 §4). Pure, no DB access, so each rule can be checked by hand.
    /// Interest and penal current reuse <see cref="EcsInterestCalculator"/> — the same accrual ECS loan recovery
    /// uses — so demand and recovery never accrue the same period twice.
    /// </summary>
    public static class EcsDemandCalculator
    {
        public static double R2(double v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        /// <summary>Whole months from <paramref name="from"/>'s month to <paramref name="to"/>'s month.</summary>
        public static int MonthsBetween(DateTime from, DateTime to) => (to.Year - from.Year) * 12 + to.Month - from.Month;

        /// <summary>Next due date: one month on, keeping month-end when the due date is a month-end.</summary>
        public static DateTime NextDemandDate(DateTime dueDate)
        {
            var d = dueDate.Date;
            var next = d.AddMonths(1);
            return d.Day == DateTime.DaysInMonth(d.Year, d.Month)
                ? new DateTime(next.Year, next.Month, DateTime.DaysInMonth(next.Year, next.Month))
                : next;
        }

        /// <summary>A member's demand is stopped when inactive (memberstatus 2) or listed in mem_demand_stop.</summary>
        public static bool IsStopped(int memberStatus, bool hasStopRow) => memberStatus == 2 || hasStopRow;

        /// <summary>
        /// Interest for an expired member accrues only up to the expiry date (inclusive), so the accrual's
        /// application date is the day after it. Null = no limit.
        /// </summary>
        public static DateTime? InterestStopDate(bool isExpired, DateTime? expiredDate) =>
            isExpired && expiredDate.HasValue ? expiredDate.Value.Date.AddDays(1) : null;

        // =====================================================================
        // Loans (§4.1, §4.2)
        // =====================================================================

        /// <summary>
        /// A principal due date: on or after the first principal due date and a whole multiple of
        /// Dem_Frequency months from it.
        /// </summary>
        public static bool IsPrincipalDueDate(DateTime? firstPrlDueDate, int demFrequency, DateTime dueDate)
        {
            if (!firstPrlDueDate.HasValue || dueDate.Date < firstPrlDueDate.Value.Date) return false;
            int freq = Math.Max(1, demFrequency);
            return MonthsBetween(firstPrlDueDate.Value, dueDate) % freq == 0;
        }

        /// <summary>The last instalment's due date: first due date + (Prl_Prd − 1) instalments.</summary>
        public static DateTime? LastDueDate(DateTime? firstPrlDueDate, int prlPrd, int demFrequency)
        {
            if (!firstPrlDueDate.HasValue) return null;
            return firstPrlDueDate.Value.Date.AddMonths(Math.Max(0, prlPrd - 1) * Math.Max(1, demFrequency));
        }

        /// <summary>
        /// Principal current (§4.2). <paramref name="interestCurrent"/> is needed for EMI (inst_type 2).
        /// </summary>
        public static double PrincipalCurrent(EcsDemandLoan loan, DateTime dueDate, double disb, double prlDem,
            double prlColl, double interestCurrent)
        {
            var due = dueDate.Date;
            var last = LastDueDate(loan.FirstPrlDueDate, loan.PrlPrd, loan.DemFrequency);
            double outstanding = Math.Max(0, disb - prlColl);
            double arrear = Math.Max(0, prlDem - prlColl);

            double current = 0;
            if (last.HasValue && due >= last.Value)
            {
                // Last instalment (and any month after it): the whole balance not yet demanded.
                current = Math.Max(0, disb - prlDem);
            }
            else if (IsPrincipalDueDate(loan.FirstPrlDueDate, loan.DemFrequency, due))
            {
                current = loan.InstType switch
                {
                    1 => loan.InstAmt,                                   // equal instalment
                    2 => Math.Max(0, loan.InstAmt - interestCurrent),    // EMI, floored at 0
                    _ => 0                                               // 3 bullet (until last), 4 none
                };
                if (loan.AdvPrlApplication == 1)
                {
                    double advance = Math.Max(0, prlColl - prlDem);
                    current = Math.Max(0, current - advance);
                }
            }

            // Cap: arrear + current never exceeds the principal outstanding.
            current = Math.Min(current, Math.Max(0, outstanding - arrear));
            return R2(Math.Max(0, current));
        }

        /// <param name="interestStop">From <see cref="InterestStopDate"/>; accrual ends the day before it.</param>
        public static EcsLoanDemandFigures Loan(EcsDemandLoan loan, DateTime dueDate, DateTime? interestStop)
        {
            var due = dueDate.Date;
            var rows = loan.Trns.Where(t => t.Date.Date < due).ToList();

            double disb = rows.Sum(t => t.Disb), prlDem = rows.Sum(t => t.PrlDem), prlColl = rows.Sum(t => t.PrlColl);
            double intCalc = rows.Sum(t => t.IntCalc), intColl = rows.Sum(t => t.IntColl);
            double piCalc = rows.Sum(t => t.PICalc), piColl = rows.Sum(t => t.PIColl);

            double outstanding = R2(Math.Max(0, disb - prlColl));
            // Capped at the outstanding: migrated loans can carry Σ Prl_Dem above the amount disbursed (TSIS capped too).
            double prlArrear = R2(Math.Min(Math.Max(0, prlDem - prlColl), outstanding));
            double intArrear = R2(Math.Max(0, intCalc - intColl));
            double piArrear = R2(Math.Max(0, piCalc - piColl));

            DateTime? disbDate = rows.Where(t => t.Status == "I" && t.DisbDate.HasValue).Min(t => t.DisbDate)
                                 ?? rows.Where(t => t.Disb > 0).Select(t => (DateTime?)t.Date).Min();
            DateTime? lastInt = rows.Max(t => t.IntCalcDate) ?? disbDate;
            DateTime? lastPi = rows.Max(t => t.PICalcDate) ?? disbDate;

            var appDate = interestStop.HasValue && interestStop.Value < due ? interestStop.Value : due;
            var deltas = rows.Select(t => new EcsTrnDelta(t.Date, t.Disb, t.PrlDem, t.PrlColl, t.IntCalc, t.IntColl)).ToList();

            double intCurrent = lastInt.HasValue
                ? EcsInterestCalculator.Calculate(EcsAccrualHead.Interest, loan.IntApplication, deltas, loan.Rates,
                    loan.Roi, loan.Pi, lastInt.Value, appDate).Amount
                : 0;
            double piCurrent = lastPi.HasValue
                ? EcsInterestCalculator.Calculate(EcsAccrualHead.Penal, loan.PIApplication, deltas, loan.Rates,
                    loan.Roi, loan.Pi, lastPi.Value, appDate).Amount
                : 0;

            double prlCurrent = PrincipalCurrent(loan, due, disb, prlDem, prlColl, intCurrent);

            return new EcsLoanDemandFigures(outstanding, prlArrear, intArrear, piArrear, prlCurrent, intCurrent, piCurrent);
        }

        // =====================================================================
        // Deposits (§4.3)
        // =====================================================================

        /// <summary>
        /// Current deposit demand: the member's option if any; else percentage × basic pay (rounded to the
        /// rupee) when percentage &gt; 0; else the fixed amount. Annual deposits are due only in their
        /// Demand_Month (an option does not change that). Capped at Maximum_Amount when it is &gt; 0.
        /// </summary>
        public static double DepositCurrent(EcsDemandDeposit d, double basicPay, DateTime dueDate)
        {
            if (string.Equals(d.Frequency, "A", StringComparison.OrdinalIgnoreCase) && d.DemandMonth != dueDate.Month)
                return 0;

            double current = d.OptionAmount is > 0
                ? d.OptionAmount.Value
                : d.Percentage > 0
                    ? Math.Round(basicPay * d.Percentage / 100.0, 0, MidpointRounding.AwayFromZero)
                    : d.FixedAmount;

            if (d.MaximumAmount > 0) current = Math.Min(current, d.MaximumAmount);
            return R2(Math.Max(0, current));
        }

        public static EcsDepositDemandFigures Deposit(EcsDemandDeposit d, double basicPay, DateTime dueDate)
        {
            double arrear = d.ArrearApplicable ? R2(Math.Max(0, d.SumDemand - d.SumReceipt)) : 0;
            double piArrear = R2(Math.Max(0, d.SumPiCalc - d.SumPiReceived));
            // One month's penal on the arrear, rounded half up to the rupee.
            double piCurrent = d.PenalRate > 0
                ? Math.Round(arrear * d.PenalRate / 100.0 / 12.0, 0, MidpointRounding.AwayFromZero)
                : 0;
            return new EcsDepositDemandFigures(arrear, DepositCurrent(d, basicPay, dueDate), piArrear, piCurrent);
        }

        // =====================================================================
        // Recurring deposits (§4.4)
        // =====================================================================

        /// <summary>
        /// Instalments due = months from the value date to the due month, + 1, capped at the RD period.
        /// Demand = instalments due × instalment − received. Penal current: one month's penal at the RD penal
        /// rate on each instalment still unpaid before this month.
        /// </summary>
        public static EcsRdDemandFigures Rd(EcsDemandRd rd, DateTime dueDate)
        {
            int due = Math.Clamp(MonthsBetween(rd.ValueDate, dueDate) + 1, 0, Math.Max(0, rd.PeriodInMonths));
            double demand = R2(Math.Max(0, due * rd.Instalment - rd.Received));
            int unpaid = rd.Instalment > 0 ? (int)Math.Ceiling(demand / rd.Instalment - 0.0001) : 0;
            int missed = Math.Max(0, unpaid - 1);   // the current month's instalment is not yet late
            double piArrear = R2(Math.Max(0, rd.PenalCalc - rd.PenalReceived));
            double piCurrent = rd.PenalRate > 0
                ? Math.Round(missed * rd.Instalment * rd.PenalRate / 100.0 / 12.0, 0, MidpointRounding.AwayFromZero)
                : 0;
            return new EcsRdDemandFigures((int)Math.Round(rd.Instalment), missed, demand, piArrear, piCurrent);
        }

        // =====================================================================
        // Due-to (§4.5)
        // =====================================================================

        /// <summary>Due-to balance owed by the member (payments − receipts), demanded in full.</summary>
        public static double DueTo(double payments, double receipts) => R2(Math.Max(0, payments - receipts));
    }
}
