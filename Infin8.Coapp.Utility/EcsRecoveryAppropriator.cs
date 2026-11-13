using Infin8.Coapp.Dto;

namespace Infin8.Coapp.Utility
{
    /// <summary>
    /// Pure appropriation of an ECS loan recovery amount across the selected loans (spec 11 §8).
    /// Shared by the WASM component (live grid columns) and the server (recheck at approval), so both
    /// sides always agree. No DB access.
    /// </summary>
    public static class EcsRecoveryAppropriator
    {
        public const double Tolerance = 0.005;

        /// <summary>Oldest loan first: earliest disbursement date, then lowest Loan_Id.</summary>
        public static List<EcsLoanRecoveryLoanVM> Order(IEnumerable<EcsLoanRecoveryLoanVM> loans) =>
            loans.OrderBy(l => l.Disb_Date ?? DateTime.MaxValue).ThenBy(l => l.Loan_Id).ToList();

        public static double TotalRecoverable(IEnumerable<EcsLoanRecoveryLoanVM> loans) => R2(loans.Sum(l => l.Recoverable));
        public static double TotalToClose(IEnumerable<EcsLoanRecoveryLoanVM> loans) => R2(loans.Sum(l => l.ToClose));

        /// <summary>
        /// Split <paramref name="amount"/> penal → interest → principal overdue → principal in advance.
        /// Each step fills a loan in full before moving on to the next. Any amount beyond the closure total is
        /// left unappropriated (callers validate amount ≤ To close first).
        /// </summary>
        public static List<EcsAppropriationLineVM> Appropriate(IEnumerable<EcsLoanRecoveryLoanVM> selected, double amount)
        {
            var loans = Order(selected);
            var lines = loans.Select(l => new EcsAppropriationLineVM { Loan_Id = l.Loan_Id }).ToList();
            double left = R2(Math.Max(0, amount));

            void Pass(Func<EcsLoanRecoveryLoanVM, EcsAppropriationLineVM, double> due, Action<EcsAppropriationLineVM, double> apply)
            {
                for (int i = 0; i < loans.Count && left > Tolerance; i++)
                {
                    double d = R2(Math.Max(0, due(loans[i], lines[i])));
                    double take = Math.Min(d, left);
                    if (take <= 0) continue;
                    apply(lines[i], take);
                    left = R2(left - take);
                }
            }

            Pass((l, a) => l.PIOverdue + l.PINow, (a, x) => a.PIColl = R2(a.PIColl + x));
            Pass((l, a) => l.IntOverdue + l.IntNow, (a, x) => a.IntColl = R2(a.IntColl + x));
            Pass((l, a) => l.PrlOverdue, (a, x) => a.PrlColl = R2(a.PrlColl + x));
            Pass((l, a) => l.PrlOutstanding - a.PrlColl, (a, x) => a.PrlColl = R2(a.PrlColl + x));

            for (int i = 0; i < loans.Count; i++)
            {
                var l = loans[i];
                var a = lines[i];
                a.PrlOsAfter = R2(l.PrlOutstanding - a.PrlColl);
                double intLeft = l.IntOverdue + l.IntNow - a.IntColl;
                double piLeft = l.PIOverdue + l.PINow - a.PIColl;
                a.ClosesLoan = a.PrlOsAfter <= Tolerance && intLeft <= Tolerance && piLeft <= Tolerance;
            }
            return lines;
        }

        /// <summary>Validation message for the amount, or null when valid (spec 11 §8).</summary>
        public static string? Validate(IEnumerable<EcsLoanRecoveryLoanVM> selected, double amount)
        {
            var list = selected.ToList();
            if (list.Count == 0) return "Select at least one loan.";
            if (amount <= 0) return "Recovery amount must be greater than zero.";
            double toClose = TotalToClose(list);
            if (amount - toClose > Tolerance)
                return $"Amount exceeds the closure amount of the selected loans (₹{toClose:N2})";
            return null;
        }

        private static double R2(double v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
    }
}
