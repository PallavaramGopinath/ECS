using Infin8.Coapp.Models;

namespace Infin8.Coapp.Repository
{
    /// <summary>
    /// Queries and change-tracker writes for the ECS Recurring Deposit Loan Receipt component, acc 25 (spec 18). The loan_trn
    /// and loan_roi rows come from <see cref="IEcsLoanRecoveryRepository"/> (same rules); the RD-loan schemes from
    /// <see cref="IEcsRdLoanRepository.GetRdLoanSchemesAsync"/>. Never calls SaveChanges.
    /// </summary>
    public interface IEcsRdLoanReceiptRepository
    {
        /// <summary>
        /// §4.1: the member's open loans on RD — Loan_Type 4, not deleted, not closed, Voc_Status 'V', of the branch — oldest
        /// first (San_Date, Loan_Id). Not tracked.
        /// </summary>
        Task<List<Loan_Master>> GetOpenRdLoansAsync(decimal memId, string brCode);

        /// <summary>§4.2: the RD Nos pledged to each loan through its live lien_trn rows.</summary>
        Task<Dictionary<decimal, List<string>>> GetPledgedRdNosAsync(IEnumerable<decimal> loanIds, string brCode);

        /// <summary>§9: related_account_data of the branch's pending (I/M) acc 25 lines, except <paramref name="excludeStagingId"/>.</summary>
        Task<List<string>> GetPendingReceiptDataAsync(string brCode, decimal excludeStagingId);

        /// <summary>
        /// §11.3: closes the loan (IsAccountClosed, AccountClosedDate), its lien and live lien_trn rows (closed, recovered =
        /// amount), and clears LienMarked on each pledged RD that no other open lien holds. Tracked rows, no save.
        /// </summary>
        Task CloseLoanAsync(decimal loanId, string brCode, DateTime closedOn);
    }
}
