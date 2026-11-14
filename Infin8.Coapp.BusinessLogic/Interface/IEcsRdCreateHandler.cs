using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>ECS RD New Account, acc 17 (spec 15).</summary>
    public interface IEcsRdCreateHandler
    {
        /// <summary>
        /// Schemes, ROI rows, provisional RD number, first holder and last nominee for the Maker screen (§4). Returns a
        /// Message instead when the token's business day is no longer the branch's open day, or the member is inactive.
        /// </summary>
        Task<EcsRdCreateSetupVM> GetSetupAsync(decimal memId, string brCode, DateTime currentDate);

        /// <summary>A joint holder by member number, age as on the business day; refuses an inactive member (§4.2, §7.3).</summary>
        Task<EcsRdHolderLookupVM> GetHolderAsync(string memberNo, string brCode, DateTime businessDay);

        /// <summary>Server recomputation and validation of a staged opening (§6, §7), without saving anything.</summary>
        Task<EcsRdCreateCalcVM> CalculateAsync(DtoEcsRdCreate dto, string brCode, DateTime businessDay);

        /// <summary>
        /// Approval (§8, §10): rechecks every staged figure, allocates TD_Id / TDMem_Id / TD_No, adds the master and
        /// members to the change tracker, and returns the opening termdeposit_trn (ids left 0 for
        /// AddTermDepositTrnListAsync) and the credit fin_voucher_trn. Never saves. Throws
        /// <see cref="EcsRecoveryException"/> with a message meant for the Checker.
        /// </summary>
        Task<EcsRdCreatePosting> BuildPostingAsync(DtoEcsRdCreate staged, decimal stagingId, DateTime stagingDate,
            string brCode, decimal yrId, decimal usrId, decimal vocId, int trnType, string status);
    }

    public class EcsRdCreatePosting
    {
        public TermDeposit_Master? Master { get; set; }
        public List<TermDeposit_Members> Members { get; } = new();
        public List<TermDeposit_Trn> TermDepositTrns { get; } = new();
        public List<Fin_Voucher_Trn> VoucherTrns { get; } = new();
    }
}
