using Infin8.Coapp.Dto;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>ECS Loan Disbursement (spec 10): turns status-E sanctions into loans with one voc_type 14 voucher.</summary>
    public interface IEcsLoanDisbursementHandler
    {
        Task<List<EcsBankLedgerVM>> GetBankLedgersAsync(string brCode);
        /// <summary>Builds and validates the posting without writing anything.</summary>
        Task<EcsDisbursementPreviewVM> PreviewAsync(EcsDisbursementRequestVM request, EcsUserContextVM ctx);
        /// <summary>Re-validates inside one transaction and writes everything, or nothing.</summary>
        Task<EcsDisbursementResultVM> DisburseAsync(EcsDisbursementRequestVM request, EcsUserContextVM ctx);
    }
}
