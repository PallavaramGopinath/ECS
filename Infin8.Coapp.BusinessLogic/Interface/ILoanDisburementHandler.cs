using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface ILoanDisburementHandler
    {
        Task<bool> AddLoanDisbursementAsync(Loan_Disb loanDisb);
        Task<bool> EditLoanDisbursementAsync(Loan_Disb loanDisb);
    }
}
