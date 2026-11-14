using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>ECS RD Receipt, acc 2 (spec 14).</summary>
    public interface IEcsRdReceiptHandler
    {
        /// <summary>
        /// The member's receivable RDs with pending instalments and penal as on the token's business day (§4–§6). Returns
        /// a Message instead when the token's business day is no longer the branch's open day.
        /// </summary>
        Task<EcsRdReceiptLoadVM> GetRdsAsync(decimal memId, string brCode, DateTime currentDate);

        /// <summary>The same figures, without the business-day check, excluding <paramref name="excludeStagingId"/> from the lock.</summary>
        Task<EcsRdReceiptLoadVM> GetRdPreviewAsync(decimal memId, string brCode, DateTime businessDay, decimal excludeStagingId = 0);

        /// <summary>
        /// Approval (§8, §10): rechecks every staged figure and builds the termdeposit_trn rows (ids left 0 for
        /// TermDepositTrnRepository.AddTermDepositTrnListAsync) and the credit-side fin_voucher_trn lines. Never saves.
        /// Throws <see cref="EcsRecoveryException"/> with a message meant for the Checker.
        /// </summary>
        Task<EcsRdReceiptPosting> BuildPostingAsync(DtoEcsRdReceipt staged, decimal stagingId, DateTime stagingDate,
            string brCode, decimal yrId, decimal usrId, decimal vocId, int trnType, string status);

        /// <summary>§11: the RD No that blocks deleting voucher <paramref name="vocId"/>'s RD receipt, or null.</summary>
        Task<string?> GetRdBlockingDeleteAsync(decimal vocId, string brCode);
    }

    public class EcsRdReceiptPosting
    {
        public List<TermDeposit_Trn> TermDepositTrns { get; } = new();
        public List<Fin_Voucher_Trn> VoucherTrns { get; } = new();
    }
}
