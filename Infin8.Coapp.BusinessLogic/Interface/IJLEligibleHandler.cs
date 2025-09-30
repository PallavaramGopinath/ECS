using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  interface IJLEligibleHandler
    {
        Task<bool> AddJLLoanEligibleAsync(JL_LoanEligible jLLoanEligible);
        Task<bool> EditJLLoanEligibleAsync(JL_LoanEligible jLLoanEligible);
        Task<double> GetJLEligiblePercentageAsync(string brCode);
    }
}
