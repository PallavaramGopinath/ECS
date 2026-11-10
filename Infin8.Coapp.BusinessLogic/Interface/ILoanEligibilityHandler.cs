using Infin8.Coapp.Dto;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>
    /// Shared loan-eligibility calculator (Spec 03 — section 10). Reused by Surety/Education/Draught.
    /// Compute-only today; persistence to Loan_Eligibility is added with the sanction save (Phase 3).
    /// </summary>
    public interface ILoanEligibilityHandler
    {
        Task<LoanEligibilityResultVM> CalculateAsync(LoanEligibilityInputVM input);
    }
}
