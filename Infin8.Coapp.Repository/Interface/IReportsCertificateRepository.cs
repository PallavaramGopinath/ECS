using Infin8.Coapp.Dto;

namespace Infin8.Coapp.Repository
{
    /// <summary>Data for the certificate reports (117 loan, 118 FD loan, 119 FD interest payment).</summary>
    public interface IReportsCertificateRepository
    {
        /// <summary>The member's loans (not deleted, 'V'), for the loan-certificate picker.</summary>
        Task<List<DropdownItem>> GetMemberLoansAsync(decimal memId, string brCode);

        /// <summary>Principal and interest collected on one loan between the dates; null when the loan is not found.</summary>
        Task<rptCertificateLoan?> GetCertificateLoanAsync(decimal loanId, DateTime fromDate, DateTime toDate, string brCode);

        /// <summary>The member's FD loans (Loan_Type 3) with principal or interest collected between the dates.</summary>
        Task<List<rptCertificateFDLoan>> GetCertificateFDLoanAsync(decimal memId, DateTime fromDate, DateTime toDate, string brCode);

        /// <summary>The member's fixed deposits (scheme type 'F') with interest or deposit paid between the dates.</summary>
        Task<List<rptCertificateFDPayment>> GetCertificateFDPaymentAsync(decimal memId, DateTime fromDate, DateTime toDate, string brCode);
    }
}
