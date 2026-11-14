using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>ECS RD Refund, acc 10 (spec 16).</summary>
    public interface IEcsRdRefundHandler
    {
        /// <summary>
        /// The member's open RDs with the payable figures as on the token's business day (§4–§6). Returns a Message
        /// instead when the token's business day is no longer the branch's open day.
        /// </summary>
        Task<EcsRdRefundLoadVM> GetRdsAsync(decimal memId, string brCode, DateTime currentDate);

        /// <summary>The same figures, without the business-day check, excluding <paramref name="excludeStagingId"/> from the locks.</summary>
        Task<EcsRdRefundLoadVM> GetRdPreviewAsync(decimal memId, string brCode, DateTime businessDay, decimal excludeStagingId = 0);

        /// <summary>
        /// Approval (§9, §11): rechecks every staged figure and builds the closing termdeposit_trn rows (ids left 0 for
        /// AddTermDepositTrnListAsync) and the fin_voucher_trn lines. Never saves. Throws <see cref="EcsRecoveryException"/>
        /// with a message meant for the Checker.
        /// </summary>
        Task<EcsRdRefundPosting> BuildPostingAsync(DtoEcsRdRefund staged, decimal stagingId, DateTime stagingDate,
            string brCode, decimal yrId, decimal usrId, decimal vocId, int trnType, string status);

        /// <summary>The TD_Ids locked by pending (I/M) acc 10 lines of the branch, except <paramref name="excludeStagingId"/>.</summary>
        Task<HashSet<decimal>> GetPendingRefundTdIdsAsync(string brCode, decimal excludeStagingId = 0);
    }

    public class EcsRdRefundPosting
    {
        public List<TermDeposit_Trn> TermDepositTrns { get; } = new();
        public List<Fin_Voucher_Trn> VoucherTrns { get; } = new();
        public List<decimal> ClosedTdIds { get; } = new();
    }
}
