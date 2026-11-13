using Infin8.Coapp.Dto;

namespace Infin8.Coapp.Utility
{
    /// <summary>
    /// Loan repayment schedule for report 126, ported as-is from TSIS frmUtilityEMI.Prepare_Schedule,
    /// LoanService.GetLoanInstalmentAmount / PMT and TermDepositService.CalcIntForFD. Pure; shared by the page
    /// (on-screen schedule) and the server (printed schedule) so both always agree.
    /// </summary>
    public static class EmiScheduleCalculator
    {
        public const int EqualInstalment = 1;
        public const int EquatedInstalment = 2;

        /// <summary>Validation message for the inputs, or null when they are usable.</summary>
        public static string? Validate(rptReportEmiObject req)
        {
            if (req.Principal <= 0) return "Enter the principal.";
            if (req.Roi < 0) return "The rate of interest cannot be negative.";
            if (req.Months <= 0) return "Enter the period in months.";
            if (req.InstalmentType is not (EqualInstalment or EquatedInstalment)) return "Choose the instalment type.";
            if (req.InstalmentType == EquatedInstalment && req.Roi <= 0) return "An equated instalment needs a rate of interest.";
            return null;
        }

        /// <summary>The monthly instalment, rounded to the rupee (TSIS GetLoanInstalmentAmount).</summary>
        public static double InstalmentAmount(rptReportEmiObject req)
        {
            double inst = req.InstalmentType == EquatedInstalment
                ? Pmt(req.Roi, req.Months, req.Principal)
                : Math.Round(req.Principal / req.Months, 0);
            return Math.Round(inst, 0, MidpointRounding.AwayFromZero);
        }

        /// <summary>One row per month; the last month clears whatever principal is left.</summary>
        public static List<rptUtlityEMI> Schedule(rptReportEmiObject req)
        {
            var rows = new List<rptUtlityEMI>();
            double instAmt = InstalmentAmount(req);
            double outstanding = req.Principal;
            for (int i = 1; i <= req.Months; i++)
            {
                double interest = MonthlyInterest(outstanding, req.Roi);
                double principal;
                if (i == req.Months)
                {
                    principal = outstanding;
                    outstanding = 0;
                }
                else
                {
                    principal = req.InstalmentType == EquatedInstalment ? instAmt - interest : instAmt;
                    outstanding -= principal;
                }
                rows.Add(new rptUtlityEMI { SlNo = i, Interest = interest, Principal = principal, Total = interest + principal, Outstanding = outstanding });
            }
            return rows;
        }

        /// <summary>The report header, as TSIS printed it (with its "Mothly" typo corrected).</summary>
        public static string Header(rptReportEmiObject req) =>
            "Loan Repayment Schedule - "
            + (req.InstalmentType == EqualInstalment ? " Equal Monthly Instalment Method : " : " Equated Monthly Instalment Method : ")
            + $" Principal = {req.Principal} Rate of Interest = {req.Roi} Period = {req.Months}";

        /// <summary>TSIS LoanService.PMT: the equated monthly instalment for a yearly rate in percent.</summary>
        private static double Pmt(double yearlyRatePercent, int months, double principal)
        {
            if (yearlyRatePercent <= 0) return 0;
            double rate = yearlyRatePercent / 100 / 12;
            double denominator = Math.Pow(1 + rate, months) - 1;
            return (rate + rate / denominator) * principal;
        }

        /// <summary>TSIS TermDepositService.CalcIntForFD(amount, roi, 1, false): one month's interest, rounded half up.</summary>
        private static double MonthlyInterest(double amount, double roi) =>
            (int)(amount * (roi / 100) * 1 / 12 + 0.5);
    }
}
