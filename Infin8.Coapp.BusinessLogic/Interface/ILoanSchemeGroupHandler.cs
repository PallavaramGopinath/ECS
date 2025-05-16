using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface ILoanSchemeGroupHandler
    {
        Task<bool> AddLoanSchemeGroupAsync(Loan_Schemes_Group loanSchemeGrp);
        Task<bool> EditLoanSchemeGroupAsync(Loan_Schemes_Group loanSchemeGrp);
    }
}
