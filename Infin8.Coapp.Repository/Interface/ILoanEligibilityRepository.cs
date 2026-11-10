using Infin8.Coapp.Models;

namespace Infin8.Coapp.Repository
{
    public interface ILoanEligibilityRepository
    {
        /// <summary>Persists a Loan_Eligibility row (auto-increments the PK) and returns the new id.</summary>
        Task<int> AddLoanEligibilityAsync(Loan_Eligibility eligibility);

        /// <summary>Persists the per-loan outstanding breakdown lines for an eligibility row.</summary>
        Task<bool> AddLoanEligOutstandingListAsync(List<Loan_Elig_Outstanding> outstandingList);
    }
}
