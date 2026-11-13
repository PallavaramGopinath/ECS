namespace Infin8.Coapp.Dto
{
    /// <summary>Request for the income tax reports: 109 loans, 110 share capital, 111 members, 112 fixed deposits.</summary>
    public class rptReportITObject
    {
        public int ReportId { get; set; }
        /// <summary>Report 109 only (TSIS used Loan_Type 1).</summary>
        public int LoanType { get; set; } = 1;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        /// <summary>Report 111 only.</summary>
        public DateTime AsOnDate { get; set; }
        public string? BrCode { get; set; }
    }
}
