using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
namespace Infin8.Coapp.Repository
{
    public interface ISuretyLoanSanctionRepository
    {
        /// <summary>Assembles member + surety + salary + dates + assets for the Surety Loan sanction screen.</summary>
        Task<SuretyLoanSanctionDataVM> GetSuretyLoanMemberAsync(string memberNo, string brCode);

        /// <summary>Scheme-driven defaults (period, limits, installment type) for a selected loan purpose.</summary>
        Task<SuretyLoanSchemeDefaultsVM> GetSchemeDefaultsAsync(int schemeId, string brCode);

        /// <summary>Deductable loans, required-share-capital + sundry rows, verification data (Sec 9/10.1/11).</summary>
        Task<SuretyDeductionsDataVM> GetDeductionsDataAsync(decimal memId, decimal suretyMemId, int schemeId,
            double loanAmount, string brCode);

        /// <summary>Most recent sanction date for a member + scheme (duplicate-sanction guard, Sec 12.2). Null if none.</summary>
        Task<DateTime?> GetLastSanctionDateAsync(decimal memId, int schemeId, string brCode);

        /// <summary>
        /// Atomically persists the eligibility row, the sanction header, and its trn lines (one SaveChanges).
        /// Assigns all PKs and back-links (LoanElig_Id, LoanSanction_Id, SlNo) before saving.
        /// </summary>
        Task<(decimal sanctionId, int eligId)> PersistSanctionAsync(
            Models.Loan_Eligibility eligibility, Models.Loan_Sanction sanction, List<Loan_Sanction_Trn> trnList);
    }
}
