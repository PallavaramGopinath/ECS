using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public  interface ILoanSchemeGroupRepository
    {
        Task<bool> AddLoanSchemeGroupAsync(Loan_Schemes_Group loanSchemeGrp);
        Task<bool> EditLoanSchemeGroupAsync(Loan_Schemes_Group loanSchemeGrp);
        Task<List<Loan_Schemes_Group>> GetLoanSchemesGroup();
    }
}
