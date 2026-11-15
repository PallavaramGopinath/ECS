using Infin8.Coapp.Models;

namespace Infin8.Coapp.Repository
{
    /// <summary>
    /// Queries and change-tracker writes for the ECS Loan on Recurring Deposit component, acc 14 (spec 17). The member's
    /// open RDs come from <see cref="IEcsRdReceiptRepository.GetEligibleRdsAsync"/> (same §4.1 rule). Never calls SaveChanges.
    /// </summary>
    public interface IEcsRdLoanRepository
    {
        /// <summary>§2.2: the branch's live loan schemes with Loan_Type 4 (Loans on Recurring Deposits).</summary>
        Task<List<Loan_Schemes>> GetRdLoanSchemesAsync(string brCode);

        /// <summary>§6.1: Elig_Per of termdeposit_loaneligibletemplate TD_Type 2, not deleted, latest Wef_Date ≤ <paramref name="asOn"/>.</summary>
        Task<double?> GetEligibilityPercentAsync(string brCode, DateTime asOn);

        /// <summary>§6.2: Roi of loan_roi_template for the scheme, Agency 'S', not deleted, latest Wef ≤ <paramref name="asOn"/>.</summary>
        Task<double?> GetRoiMarginAsync(int schemeId, string brCode, DateTime asOn);

        /// <summary>
        /// §8 rule 7: RDs among <paramref name="tdIds"/> with LienMarked or an open, valid lien_trn row, with the loan no. of
        /// the open lien when known (null when only LienMarked is set).
        /// </summary>
        Task<Dictionary<decimal, string?>> GetLienLoanNosAsync(IEnumerable<decimal> tdIds, string brCode);

        /// <summary>
        /// §6.1 Existing: principal outstanding (Σ Disb_Amt − Σ PrlColl_Amt over valid loan_trn rows) of open Loan_Type 4 loans
        /// holding an open lien_trn on any of <paramref name="tdIds"/>.
        /// </summary>
        Task<double> GetExistingOutstandingAsync(IEnumerable<decimal> tdIds, string brCode);

        /// <summary>Related_Account_Data of the branch's pending (I/M) acc 14 staging lines, except <paramref name="excludeStagingId"/>.</summary>
        Task<List<string>> GetPendingLoanDataAsync(string brCode, decimal excludeStagingId);

        /// <summary>§5: the next loan no. of the scheme in the branch as it stands now (provisional; shown to the Maker).</summary>
        Task<string> PeekNextLoanNoAsync(Loan_Schemes scheme, string brCode);

        /// <summary>§11: in-memory MAX+1 ids for the loan and lien rows, floored at brcode + 0000000.</summary>
        Task<EcsRdLoanIdAllocator> CreateIdAllocatorAsync(string brCode);

        /// <summary>§11.1–§11.3: adds loan_master, loan_disb, loan_roi, lien and lien_trn rows to the change tracker.</summary>
        Task AddLoanAsync(Loan_Master master, Loan_Disb disb, Loan_Roi roi, Lien lien, IEnumerable<Lien_Trn> lienTrns);

        /// <summary>§11.4: sets LienMarked on the tracked termdeposit_master rows. Saved with the approval's save.</summary>
        Task MarkLienAsync(IEnumerable<decimal> tdIds, string brCode);
    }
}
