using Infin8.Coapp.Dto;
using Infin8.Coapp.Repository;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>
    /// Certificate reports, ported from TSIS frmReportViewer.Print_Certificate* / ReportService.GetCertificateLoan.
    /// The wording is TSIS's, with its spelling corrected.
    /// </summary>
    public class ReportsCertificateHandler : IReportsCertificateHandler
    {
        public const int LoanCertificate = 117;
        public const int FDLoanCertificate = 118;
        public const int FDPaymentCertificate = 119;

        readonly IUnitOfWork _unitOfWork;
        readonly IUtilityHandler _utility;

        public ReportsCertificateHandler(IUnitOfWork unitOfWork, IUtilityHandler utility)
        {
            _unitOfWork = unitOfWork;
            _utility = utility;
        }

        public Task<List<DropdownItem>> GetMemberLoansAsync(decimal memId, string brCode) =>
            _unitOfWork.ReportsCertificate.GetMemberLoansAsync(memId, brCode);

        public async Task<CertificateReportData> BuildAsync(rptReportCertificateObject req)
        {
            string brCode = req.BrCode ?? "";
            string period = $"{req.FromDate:dd-MM-yyyy} to {req.ToDate:dd-MM-yyyy}";
            switch (req.ReportId)
            {
                case LoanCertificate:
                {
                    var c = await _unitOfWork.ReportsCertificate.GetCertificateLoanAsync(req.LoanId, req.FromDate, req.ToDate, brCode);
                    if (c == null) return new CertificateReportData { Message = "Loan not found in this branch." };
                    double total = c.PrlColl_Amt + c.IntColl_Amt;
                    if (total <= 0) return new CertificateReportData { Message = $"Nothing was paid on loan {c.Loan_No} from {period}." };
                    return new CertificateReportData
                    {
                        Content = $"     This is to certify that Thiru/Thirumathi {c.MemberName?.Trim()} Mem No {c.MemberNo?.Trim()}"
                                + $" has paid an amount of Rs. {Rs(total)}/- ({Words(total)})"
                                + $" towards the Principal and Interest against the {LoanPurpose(c.Purpose)} taken by him/her \r\n \r\n"
                                + "The break up of which is given below: \r\n \r\n"
                                + $"The Interest paid during {period} Rs. {Rs(c.IntColl_Amt)}/- \r\n"
                                + $"The Principal paid during {period} Rs. {Rs(c.PrlColl_Amt)}/- \r\n"
                    };
                }
                case FDLoanCertificate:
                {
                    var rows = await _unitOfWork.ReportsCertificate.GetCertificateFDLoanAsync(req.MemId, req.FromDate, req.ToDate, brCode);
                    if (rows.Count == 0) return new CertificateReportData { Message = $"This member repaid no loan against fixed deposit from {period}." };
                    double disbursed = rows.Sum(r => r.Disb_Amt);
                    return new CertificateReportData
                    {
                        DataSetName = "DS_CertificateLoan",
                        Rows = rows,
                        Content = $"This is to certify that Thiru/ Thirumathi / Kumari {rows[0].MemberName?.Trim()} Mem No {rows[0].MemberNo?.Trim()}"
                                + $" has received a sum of Rs. {Rs(disbursed)} /- ({Words(disbursed)})"
                                + $" towards Loan against Fixed Deposit(s) for the period from {period} \r\nThe details are given below :"
                    };
                }
                case FDPaymentCertificate:
                {
                    var rows = await _unitOfWork.ReportsCertificate.GetCertificateFDPaymentAsync(req.MemId, req.FromDate, req.ToDate, brCode);
                    if (rows.Count == 0) return new CertificateReportData { Message = $"This member has no fixed deposit payment from {period}." };
                    double interest = rows.Sum(r => r.InterestPaidAmount);
                    return new CertificateReportData
                    {
                        DataSetName = "DS_CertificateFDPayment",
                        Rows = rows,
                        Content = $"This is to certify that Thiru/ Thirumathi / Kumari {rows[0].MemberName?.Trim()} Mem No {rows[0].MemberNo?.Trim()}"
                                + $" has received a sum of Rs. {Rs(interest)} /- ({Words(interest)})"
                                + $" towards interest on Fixed Deposit(s) for the period from {period}  \r\nThe details are given below :"
                    };
                }
                default:
                    return new CertificateReportData { Message = $"Report {req.ReportId} is not a certificate." };
            }
        }

        private static string Rs(double amount) => amount.ToString("0.##");

        /// <summary>"SURETY" -> "SURETY Loan"; a scheme name that already ends in "loan" is used as is.</summary>
        private static string LoanPurpose(string? schemeName)
        {
            var name = (schemeName ?? "").Trim();
            return name.EndsWith("loan", StringComparison.OrdinalIgnoreCase) ? name : name + " Loan";
        }

        /// <summary>IUtilityHandler.RupeesInWords already returns "Rupees … only".</summary>
        private string Words(double amount) => _utility.RupeesInWords(amount).Trim();
    }
}
