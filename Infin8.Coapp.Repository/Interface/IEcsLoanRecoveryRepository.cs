using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.Repository
{
    /// <summary>Queries for the ECS Loan Recovery component, acc 58 (spec 11). Never calls SaveChanges.</summary>
    public interface IEcsLoanRecoveryRepository
    {
        /// <summary>
        /// The member's open ECS loans (Loan_Type 1, not deleted, not closed, Voc_Status 'V') with scheme rule
        /// columns and ledgers. Balances are left at zero for the handler to fill.
        /// </summary>
        Task<List<EcsLoanRecoveryLoanVM>> GetCandidateLoansAsync(decimal memId, string brCode);

        /// <summary>Valid loan_trn rows (not deleted, 'V', Trn_Date ≤ toDate) of the given loans. Untracked.</summary>
        Task<List<Loan_Trn>> GetLoanTrnRowsAsync(IEnumerable<decimal> loanIds, string brCode, DateTime toDate);

        /// <summary>Society-agency (Agency 'S') loan_roi rows with Roi_Wef ≤ toDate. Untracked.</summary>
        Task<List<Loan_Roi>> GetRoiRowsAsync(IEnumerable<decimal> loanIds, string brCode, DateTime toDate);

        /// <summary>
        /// Related_Account_Data JSON of the member's pending (I/M) acc 58 staging lines, excluding
        /// <paramref name="excludeStagingId"/> (the staging being approved).
        /// </summary>
        Task<List<string>> GetPendingRecoveryDataAsync(decimal memId, string brCode, decimal excludeStagingId);

        /// <summary>Loan masters loaded into the change tracker, for the approval-time update.</summary>
        Task<List<Loan_Master>> GetLoanMastersForUpdateAsync(IEnumerable<decimal> loanIds, string brCode);
    }
}
