using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IJLEligibleRepository
    {
        Task<bool> AddJLEligibleAsync(JL_LoanEligible jLLoanEligible); 
        Task<bool> EditJLEligibleAsync(JL_LoanEligible jLLoanEligible);
        Task<double> GetJLEligiblePercentageAsync(string brCode);
    }
}
