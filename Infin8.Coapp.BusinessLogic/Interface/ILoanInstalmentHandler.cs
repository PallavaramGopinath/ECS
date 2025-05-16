using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface ILoanInstalmentHandler
    {
        Task<bool> AddLoanInstalmentAsync(Loan_Inst loanInst);
        Task<bool> EditLoanInstalmentAsync(Loan_Inst loanInst);
    }
}
