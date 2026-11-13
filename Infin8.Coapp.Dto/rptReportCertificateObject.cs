namespace Infin8.Coapp.Dto
{
    /// <summary>Request for the certificate reports (117 loan, 118 FD loan, 119 FD payment).</summary>
    public class rptReportCertificateObject
    {
        public int ReportId { get; set; }
        public decimal MemId { get; set; }
        /// <summary>Report 117 only.</summary>
        public decimal LoanId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        /// <summary>Date printed on the certificate.</summary>
        public DateTime AsOnDate { get; set; }
        public string? BrCode { get; set; }
    }
}
