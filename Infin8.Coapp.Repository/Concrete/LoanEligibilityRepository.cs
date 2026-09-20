using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;

namespace Infin8.Coapp.Repository
{
    public class LoanEligibilityRepository : Repository<Loan_Eligibility>, ILoanEligibilityRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;

        public LoanEligibilityRepository(DbContext context) : base(context)
        {
        }

        public async Task<int> AddLoanEligibilityAsync(Loan_Eligibility eligibility)
        {
            try
            {
                int maxId = await CSISContext.Loan_Eligibility.AnyAsync()
                    ? await CSISContext.Loan_Eligibility.MaxAsync(x => x.LoanElig_Id)
                    : 0;
                eligibility.LoanElig_Id = maxId + 1;
                await AddAsync(eligibility);
                return eligibility.LoanElig_Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan eligibility not saved");
            }
        }

        public async Task<bool> AddLoanEligOutstandingListAsync(List<Loan_Elig_Outstanding> outstandingList)
        {
            try
            {
                int maxId = await CSISContext.Loan_Elig_Outstanding.AnyAsync()
                    ? await CSISContext.Loan_Elig_Outstanding.MaxAsync(x => x.EligOS_Id)
                    : 0;
                foreach (var os in outstandingList)
                {
                    maxId++;
                    os.EligOS_Id = maxId;
                    await AddAsync2(os);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan eligibility outstanding not saved");
            }
        }

        // Loan_Elig_Outstanding is a different entity type than the repository's TEntity, so add via context.
        private async Task AddAsync2(Loan_Elig_Outstanding entity)
        {
            await CSISContext.Loan_Elig_Outstanding.AddAsync(entity);
        }
    }
}
