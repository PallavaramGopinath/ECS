using Infin8.Coapp.Dto;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>Certificate reports (117 loan, 118 FD loan, 119 FD interest payment), ported from TSIS.</summary>
    public interface IReportsCertificateHandler
    {
        Task<List<DropdownItem>> GetMemberLoansAsync(decimal memId, string brCode);

        /// <summary>Rows, DataSet name and certificate wording for one request; Message is set when there is nothing to certify.</summary>
        Task<CertificateReportData> BuildAsync(rptReportCertificateObject request);
    }

    public class CertificateReportData
    {
        /// <summary>RDLC DataSet name, or null for report 117, whose layout has no DataSet.</summary>
        public string? DataSetName { get; set; }
        public System.Collections.IEnumerable? Rows { get; set; }
        public string Content { get; set; } = "";
        /// <summary>Why no certificate can be printed (unknown report, nothing in the period); null when it can.</summary>
        public string? Message { get; set; }
    }
}
