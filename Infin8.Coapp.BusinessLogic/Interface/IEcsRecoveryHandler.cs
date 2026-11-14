using Infin8.Coapp.Dto;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>ECS Recovery Appropriation (spec 13): appropriate, save and delete an employer's salary recovery.</summary>
    public interface IEcsRecoveryHandler
    {
        Task<EcsRecoveryStateVM> GetStateAsync(EcsUserContextVM ctx);
        /// <summary>One office of a demand with its members, or null when the demand or office does not exist.</summary>
        Task<EcsRecoveryOfficeDetailVM?> GetOfficeAsync(decimal demandId, decimal officeId, EcsUserContextVM ctx);
        Task<EcsRecoveryResultVM> AppropriateAsync(EcsAppropriateRequestVM request, EcsUserContextVM ctx);
        Task<EcsRecoveryResultVM> DeleteAppropriationAsync(decimal demandId, decimal officeId, EcsUserContextVM ctx);
        Task<EcsRecoveryResultVM> SaveAsync(EcsSaveRecoveryRequestVM request, EcsUserContextVM ctx);
        Task<EcsRecoveryResultVM> DeleteSavedAsync(decimal demandId, decimal officeId, EcsUserContextVM ctx);
        /// <summary>Recovery Refund (§16): a member's saved receipts against a demand, or null when not found.</summary>
        Task<EcsRefundVM?> GetRefundAsync(decimal demandId, string memberNo, EcsUserContextVM ctx);
        Task<EcsRecoveryResultVM> RefundAsync(EcsRefundRequestVM request, EcsUserContextVM ctx);
    }
}
