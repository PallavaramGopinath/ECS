using Infin8.Coapp.Dto;

namespace Infin8.Coapp.Utility
{
    /// <summary>
    /// Pure rules of the ECS Loan on Recurring Deposit, acc 14 (spec 17 §6, §8, §11.3). Shared by the WASM component
    /// (totals and Initiate checks) and the server (load and recheck at approval), so both sides always agree. No DB access.
    /// </summary>
    public static class EcsRdLoanCalculator
    {
        public const double Tolerance = 0.005;

        /// <summary>termdeposit_loaneligibletemplate.TD_Type of a recurring deposit.</summary>
        public const int RdEligibilityType = 2;

        public static bool Near(double a, double b) => Math.Abs(a - b) <= Tolerance;

        /// <summary>§6.1 drawingPower(rd) = received × EligPer / 100 (to the paisa).</summary>
        public static double DrawingPower(double received, double eligPer) =>
            R2(received * eligPer / 100.0);

        /// <summary>
        /// §6.1–§6.3 over the selected lines: per-RD drawing power, total received, borrowing power (rounded once to a rupee,
        /// AwayFromZero), eligible = max(0, borrowing power − existing), ROI = highest RD rate + margin, due date = latest
        /// maturity. EligPer, Margin and ExistingOutstanding must already be set on <paramref name="d"/>.
        /// </summary>
        public static void Totals(DtoEcsRdLoan d)
        {
            foreach (var l in d.Lines) l.DrawingPower = DrawingPower(l.Received, d.EligPer);

            d.TotalReceived = R2(d.Lines.Sum(l => l.Received));
            d.BorrowingPower = (double)Math.Round((decimal)d.TotalReceived * (decimal)d.EligPer / 100m, 0, MidpointRounding.AwayFromZero);
            d.EligibleAmount = Math.Max(0, R2(d.BorrowingPower - d.ExistingOutstanding));
            d.RdRoiMax = d.Lines.Count == 0 ? 0 : d.Lines.Max(l => l.RateOfInterest);
            d.LoanRoi = d.Lines.Count == 0 ? 0 : R2(d.RdRoiMax + d.Margin);
            d.DueDate = d.Lines.Count == 0 ? d.ApplicationDate.Date : d.Lines.Max(l => l.MaturityDate).Date;
        }

        /// <summary>
        /// §11.3 lien_trn.LienTr_Amount: the loan split over the RDs in proportion to their drawing power, each share rounded
        /// to a rupee, the last RD taking the remainder. Returned in the order of <paramref name="lines"/>.
        /// </summary>
        public static List<double> LienShares(double loanAmount, IList<DtoEcsRdLoanLine> lines, double borrowingPower)
        {
            var shares = new List<double>();
            double given = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                double share;
                if (i == lines.Count - 1)
                    share = R2(loanAmount - given);
                else if (borrowingPower <= 0)
                    share = 0;
                else
                    share = (double)Math.Round((decimal)loanAmount * (decimal)lines[i].DrawingPower / (decimal)borrowingPower,
                        0, MidpointRounding.AwayFromZero);
                given = R2(given + share);
                shares.Add(share);
            }
            return shares;
        }

        /// <summary>§8 rules 4, 5–9 (via the block reasons), 6 and 10 on a computed loan. Null when it can be initiated.</summary>
        public static string? Validate(DtoEcsRdLoan d)
        {
            if (d.Lines.Count == 0) return "Select the recurring deposit(s) to pledge";
            var blocked = d.Lines.FirstOrDefault(l => !string.IsNullOrEmpty(l.BlockReason));
            if (blocked != null) return blocked.BlockReason;
            var empty = d.Lines.FirstOrDefault(l => l.Received <= 0);
            if (empty != null) return $"RD No {empty.TD_No} has no amount received";
            if (d.EligPer <= 0) return "Loan eligibility percentage for RD is not defined";
            if (d.LoanAmount <= 0) return "Enter the loan amount";
            if (Math.Abs(d.LoanAmount - Math.Round(d.LoanAmount)) > Tolerance) return "Enter the loan amount in whole rupees";
            if (d.LoanAmount > d.EligibleAmount + Tolerance)
                return $"Loan amount exceeds the eligible amount of ₹{d.EligibleAmount:N0}";
            return null;
        }

        private static double R2(double v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
    }
}
