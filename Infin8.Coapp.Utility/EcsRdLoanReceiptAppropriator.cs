using Infin8.Coapp.Dto;

namespace Infin8.Coapp.Utility
{
    /// <summary>
    /// Pure rules of the ECS Recurring Deposit Loan Receipt, acc 25 (spec 18 §6, §8 rule 2/4, §11.3). Shared by the WASM
    /// component (appropriation, manual split, Initiate checks) and the server (recheck at approval), so both sides always
    /// agree. No DB access. Lines are expected oldest loan first (San_Date, Loan_Id), as the server returns them.
    /// </summary>
    public static class EcsRdLoanReceiptAppropriator
    {
        public const double Tolerance = 0.005;

        public static bool Near(double a, double b) => Math.Abs(a - b) <= Tolerance;

        /// <summary>Interest due = arrear (calculated, not collected) + current (§5.2).</summary>
        public static double InterestDue(DtoEcsRdLoanReceiptLine l) => R2(l.InterestArrear + l.InterestCurrent);

        /// <summary>Principal outstanding + interest due of one loan.</summary>
        public static double Due(DtoEcsRdLoanReceiptLine l) => R2(l.PrincipalOutstanding + InterestDue(l));

        /// <summary>Total due of the loans that can be received (blocked loans excluded).</summary>
        public static double TotalDue(IEnumerable<DtoEcsRdLoanReceiptLine> lines) =>
            R2(lines.Where(l => string.IsNullOrEmpty(l.BlockReason)).Sum(Due));

        /// <summary>
        /// §6.1: applies <paramref name="amount"/> to the interest of every loan, oldest first, then to the principal of every
        /// loan, oldest first. Overwrites InterestReceived / PrincipalReceived on every line. Returns an error message when the
        /// amount is above the total due (the lines are then left at zero), else null.
        /// </summary>
        public static string? Appropriate(IList<DtoEcsRdLoanReceiptLine> lines, double amount)
        {
            foreach (var l in lines)
            {
                l.InterestDue = InterestDue(l);
                l.InterestReceived = 0;
                l.PrincipalReceived = 0;
            }

            var open = lines.Where(l => string.IsNullOrEmpty(l.BlockReason)).ToList();
            double totalDue = TotalDue(open);
            amount = R2(amount);
            if (amount < 0) amount = 0;
            if (amount > totalDue + Tolerance)
            {
                Refresh(lines);
                return $"Amount received exceeds the total due of ₹{totalDue:N2}";
            }

            double left = amount;
            foreach (var l in open)
            {
                double take = Math.Min(left, l.InterestDue);
                l.InterestReceived = R2(take);
                left = R2(left - take);
            }
            foreach (var l in open)
            {
                double take = Math.Min(left, l.PrincipalOutstanding);
                l.PrincipalReceived = R2(take);
                left = R2(left - take);
            }
            Refresh(lines);
            return null;
        }

        /// <summary>Recomputes InterestDue and the §11.3 closure flag on every line.</summary>
        public static void Refresh(IEnumerable<DtoEcsRdLoanReceiptLine> lines)
        {
            foreach (var l in lines)
            {
                l.InterestDue = InterestDue(l);
                l.ClosesLoan = ClosesLoan(l);
            }
        }

        /// <summary>§11.3: principal outstanding − principal received = 0 and interest due − interest received = 0.</summary>
        public static bool ClosesLoan(DtoEcsRdLoanReceiptLine l) =>
            (l.InterestReceived > 0 || l.PrincipalReceived > 0)
            && Near(l.PrincipalOutstanding - l.PrincipalReceived, 0)
            && Near(InterestDue(l) - l.InterestReceived, 0);

        /// <summary>
        /// Fills the staged model from the screen lines: keeps only lines with something received, refreshes the flags and
        /// sums the totals.
        /// </summary>
        public static void Totals(DtoEcsRdLoanReceipt d, IEnumerable<DtoEcsRdLoanReceiptLine> allLines)
        {
            var lines = allLines.ToList();
            Refresh(lines);
            d.Lines = lines.Where(l => l.InterestReceived > 0 || l.PrincipalReceived > 0).ToList();
            d.TotalDue = R2(d.Lines.Sum(Due));
            d.TotalInterest = R2(d.Lines.Sum(l => l.InterestReceived));
            d.TotalPrincipal = R2(d.Lines.Sum(l => l.PrincipalReceived));
            d.TotalReceived = R2(d.TotalInterest + d.TotalPrincipal);
        }

        /// <summary>§8 rules 2 and 4 (the §6 limits) on the staged lines. Null when the receipt can be initiated.</summary>
        public static string? Validate(DtoEcsRdLoanReceipt d)
        {
            if (d.Lines.Count == 0 || d.TotalReceived <= Tolerance) return "Enter the amount received";
            if (d.Lines.Select(l => l.Loan_Id).Distinct().Count() != d.Lines.Count) return "A loan is listed twice";

            foreach (var l in d.Lines)
            {
                string no = l.Loan_No?.Trim() ?? "";
                if (!string.IsNullOrEmpty(l.BlockReason)) return l.BlockReason;
                if (l.InterestReceived < 0 || l.PrincipalReceived < 0)
                    return $"Loan No {no}: amounts received cannot be negative";
                double intDue = InterestDue(l);
                if (!WholeOrExact(l.InterestReceived, intDue) || !WholeOrExact(l.PrincipalReceived, l.PrincipalOutstanding))
                    return $"Loan No {no}: enter the amounts received in whole rupees";
                if (l.InterestReceived > intDue + Tolerance)
                    return $"Loan No {no}: interest received exceeds the interest due of ₹{intDue:N2}";
                if (l.PrincipalReceived > l.PrincipalOutstanding + Tolerance)
                    return $"Loan No {no}: principal received exceeds the principal outstanding of ₹{l.PrincipalOutstanding:N2}";
                if (l.PrincipalReceived > Tolerance && !Near(l.InterestReceived, intDue))
                    return $"Loan No {no}: receive the full interest due (₹{intDue:N2}) before principal";
            }

            double sum = R2(d.Lines.Sum(l => l.InterestReceived + l.PrincipalReceived));
            if (!Near(sum, d.TotalReceived)) return "The amount received does not match the loans";
            return null;
        }

        /// <summary>A received figure is a whole rupee amount, or exactly the (possibly fractional) balance it settles.</summary>
        private static bool WholeOrExact(double received, double balance) =>
            Near(received, Math.Round(received, 0, MidpointRounding.AwayFromZero)) || Near(received, balance);

        private static double R2(double v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
    }
}
