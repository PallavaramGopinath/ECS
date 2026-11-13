namespace Infin8.Coapp.Dto
{
    /// <summary>Request for the loan repayment (EMI) schedule, report 126.</summary>
    public class rptReportEmiObject
    {
        public double Principal { get; set; }
        /// <summary>Rate of interest, % per year.</summary>
        public double Roi { get; set; }
        public int Months { get; set; }
        /// <summary>1 = equal monthly instalment (principal / months), 2 = equated monthly instalment (EMI).</summary>
        public int InstalmentType { get; set; } = 1;
    }
}
