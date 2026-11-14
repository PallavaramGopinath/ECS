using Infin8.Coapp.Dto;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>ECS Demand Calculation (spec 12): calculate, post and delete the monthly demand.</summary>
    public interface IEcsDemandHandler
    {
        Task<EcsDemandStateVM> GetStateAsync(EcsUserContextVM ctx);
        /// <summary>Calculates in memory only; nothing is written.</summary>
        Task<EcsDemandSummaryVM> PreviewAsync(DateTime dueDate, EcsUserContextVM ctx);
        /// <summary>A saved demand with its per-member lines, or null when it does not exist.</summary>
        Task<EcsDemandSummaryVM?> GetSavedSummaryAsync(decimal demandId, string brCode);
        Task<EcsDemandResultVM> SaveCalculationAsync(DateTime dueDate, EcsUserContextVM ctx);
        Task<EcsDemandResultVM> PostAsync(decimal demandId, EcsUserContextVM ctx);
        Task<EcsDemandResultVM> DeleteCalculationAsync(decimal demandId, EcsUserContextVM ctx);
        Task<EcsDemandResultVM> DeletePostedAsync(decimal demandId, EcsUserContextVM ctx);

        // ----- stop demand -----
        Task<EcsStopDemandListVM> GetStopListAsync(DateTime dueDate, string brCode);
        Task<EcsDemandResultVM> AddStopAsync(EcsStopDemandRequestVM request, EcsUserContextVM ctx);
        Task<EcsDemandResultVM> RemoveStopAsync(decimal stopId, EcsUserContextVM ctx);

        // ----- delete demand for expired members -----
        Task<EcsExpiredMemberVM?> GetExpiredMemberAsync(string memberNo, string brCode);
        Task<EcsExpiredLoanRowsVM?> GetExpiredLoanRowsAsync(decimal loanId, string brCode);
        Task<EcsDemandResultVM> DeleteExpiredDemandRowAsync(decimal trnId, EcsUserContextVM ctx);
        Task<EcsDemandResultVM> RevokeExpiredDeletionAsync(decimal trnId, EcsUserContextVM ctx);

        /// <summary>
        /// Day End step 7: on the last business day of the month, calculates and saves (does not post) the next
        /// demand for an ECS branch. Runs inside Day End's transaction and saves its rows; returns what it did, or
        /// null when it had nothing to do. Throws on failure so Day End rolls back.
        /// </summary>
        Task<string?> CalculateForDayEndAsync(DateTime processDate, decimal usrId, decimal yrId, string brCode);
    }
}
