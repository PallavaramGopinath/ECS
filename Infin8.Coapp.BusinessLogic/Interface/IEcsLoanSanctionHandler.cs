using Infin8.Coapp.Dto;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>ECS Loan Sanction (spec 06 §7.2).</summary>
    public interface IEcsLoanSanctionHandler
    {
        Task<List<DropdownItem>> GetEcsSchemesAsync(string brCode);
        Task<SuretyLoanSanctionDataVM> GetMemberAsync(string memberNo, DateTime asOn, string brCode);
        Task<EcsPayDetailsVM> GetPayDetailsAsync(decimal memId, string brCode);

        /// <summary>
        /// Records a missing date of birth in mem_master and returns the member with retirement date and
        /// service months recomputed. Message is set (and nothing saved) when the date is not acceptable.
        /// </summary>
        Task<SuretyLoanSanctionDataVM> UpdateMemberDobAsync(EcsMemberDobVM vm, DateTime asOn, string brCode);

        /// <summary>Scheme defaults + warnings: DUPLICATE / ALREADY_SAVED (block), STIPULATED_PERIOD (confirm).</summary>
        Task<SuretyLoanSchemeDefaultsVM> GetSchemeDefaultsAsync(int schemeId, decimal memId, decimal excludeSanctionId,
            DateTime asOn, string brCode);

        Task<EcsRecoverablesVM> GetRecoverablesAsync(EcsRecoverablesRequestVM req, DateTime asOn, string brCode);

        /// <summary>Surety's share capital and loans in overdue, for the Surety Details panel.</summary>
        Task<EcsSuretyDetailsVM> GetSuretyDetailsAsync(int schemeId, decimal suretyMemId, string brCode);

        /// <summary>Status / DM_Id / ledger name for a manual deduction row (§3.8).</summary>
        Task<DeductableLoanRowVM> ResolveDeductionAsync(decimal ledId, string brCode);

        Task<LoanEligibilityResultVM> CalculateEligibilityAsync(LoanEligibilityInputVM input);

        /// <summary>Create (LoanSanction_Id = 0) or modify (soft-delete + re-issue under a new id).</summary>
        Task<SanctionSaveResultVM> SaveAsync(SuretyLoanSanctionSaveVM vm, EcsUserContextVM ctx);
        Task<SanctionSaveResultVM> DeleteAsync(decimal sanctionId, EcsUserContextVM ctx);
        Task<List<EcsSanctionListItemVM>> ListAsync(EcsSanctionListFilterVM filter, string brCode);
        Task<EcsSanctionDetailVM?> GetAsync(decimal sanctionId, string brCode);

        /// <summary>Office Note data (header, notes, lines); null when the sanction is not found.
        /// Society name and signatures are left for the caller.</summary>
        Task<OfficeNoteVM?> GetOfficeNoteAsync(decimal sanctionId, string brCode);

        /// <summary>Renders the Office Note PDF; null when the sanction is not found.</summary>
        Task<byte[]?> GetOfficeNotePdfAsync(decimal sanctionId, string brCode, Stream reportStream,
            Dictionary<string, string> parameters);
    }
}
