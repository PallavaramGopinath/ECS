using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface ILoanSanctionHandler
    {
        Task<bool> AddLoanSanction(Loan_Sanction loanSanction);
        Task<bool> EditLoanSanction(Loan_Sanction loanSanction);
    }
}
