using Infin8.Coapp.Models;

namespace Infin8.Coapp.Repository
{
    /// <summary>
    /// Queries for the ECS RD Refund component, acc 10 (spec 16). The open RDs and their ROI rows come from
    /// <see cref="IEcsRdReceiptRepository"/> (same §4.1 rule). Never calls SaveChanges.
    /// </summary>
    public interface IEcsRdRefundRepository
    {
        /// <summary>termdeposit_fctemplate rows of the branch for an RD (TD_Type 4), not deleted.</summary>
        Task<List<TermDeposit_FcTemplate>> GetRdForeclosureTemplatesAsync(string brCode);

        /// <summary>§7: the RDs among <paramref name="tdIds"/> with LienMarked or an open, valid lien_trn row.</summary>
        Task<HashSet<decimal>> GetLienedTdIdsAsync(IEnumerable<decimal> tdIds, string brCode);

        /// <summary>Scheme ledgers per RD: deposit (Led_Id), interest (IntLed_Id), penal (PiLed_Id).</summary>
        Task<Dictionary<decimal, (decimal LedId, decimal IntLedId, decimal PiLedId)>> GetRdLedgersAsync(IEnumerable<decimal> tdIds, string brCode);

        /// <summary>Related_Account_Data of the branch's pending (I/M) acc 10 staging lines, except <paramref name="excludeStagingId"/>.</summary>
        Task<List<string>> GetPendingRefundDataAsync(string brCode, decimal excludeStagingId);

        /// <summary>
        /// §8 rule 8: RDs among <paramref name="tdIds"/> in a live, posted demand whose recovery for the member's current pay
        /// office is not yet saved, with that demand's due date.
        /// </summary>
        Task<Dictionary<decimal, DateTime?>> GetUnrecoveredDemandAsync(IEnumerable<decimal> tdIds, string brCode);

        /// <summary>§11.2: sets AccountClosed on the tracked masters. Saved with the approval's CompleteAsync.</summary>
        Task MarkClosedAsync(IEnumerable<decimal> tdIds, string brCode);
    }
}
