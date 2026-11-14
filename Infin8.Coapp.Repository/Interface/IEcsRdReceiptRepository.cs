using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.Repository
{
    /// <summary>Queries for the ECS RD Receipt component, acc 2 (spec 14). Never calls SaveChanges.</summary>
    public interface IEcsRdReceiptRepository
    {
        /// <summary>
        /// The member's open RDs (held directly or as a joint holder; scheme type 'R'; master not deleted, not closed,
        /// Voc_Status 'V'; no payout made) in TD_No order, with the §4 ledger sums filled. Computed figures are left
        /// at zero. PenalRate is left at zero; see <see cref="GetPenalTemplatesAsync"/>.
        /// </summary>
        Task<List<DtoEcsRdReceiptLine>> GetEligibleRdsAsync(decimal memId, string brCode);

        /// <summary>Monthly-period roi template rows of the given schemes, not deleted, for the penal pick (§6.1).</summary>
        Task<List<TermDeposit_Roi_Template>> GetPenalTemplatesAsync(IEnumerable<decimal> schemeIds, string brCode);

        /// <summary>TD_Id → (instalment ledger, penal ledger) from the RD's scheme (§13.1).</summary>
        Task<Dictionary<decimal, (decimal LedId, decimal PiLedId)>> GetRdLedgersAsync(IEnumerable<decimal> tdIds, string brCode);

        /// <summary>
        /// related_account_data of every pending (I/M on master and detail) acc 2 staging line in the branch, except
        /// <paramref name="excludeStagingId"/>. Branch-wide, not per member, so a joint holder's receipt also locks.
        /// </summary>
        Task<List<string>> GetPendingReceiptDataAsync(string brCode, decimal excludeStagingId);

        /// <summary>
        /// §11: the TD_No of an RD of voucher <paramref name="vocId"/> that has a valid termdeposit_trn row with a higher
        /// Trn_SlNo than the voucher's rows, or null. Not wired to any screen yet (no approved-voucher delete exists).
        /// </summary>
        Task<string?> GetRdWithLaterRowsAsync(decimal vocId, string brCode);
    }
}
