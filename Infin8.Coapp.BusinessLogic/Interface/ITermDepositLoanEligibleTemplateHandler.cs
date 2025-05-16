using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface ITermDepositLoanEligibleTemplateHandler
    {
        Task<bool> AddTermDepositLoanEligibleTemplateAsync(TermDeposit_LoanEligibleTemplate termDepositLoanEligibleTemplate);
        Task<bool> EditTermDepositLoanEligibleTemplateAsync(TermDeposit_LoanEligibleTemplate termDepositLoanEligibleTemplate);
        Task<List<TermDeposit_LoanEligibleTemplate>> GetTermDepositLoanEligibleTemplateListAsync();
        Task<double> GetTDLoanEligiblePercentageAsync(int tdSchemeType,string brCode);
    }
}
