using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface ILoanMemberHandler
    {
        Task<bool> AddLoanMemberAsync(List<Loan_Members> loanMemberList);
        Task<bool> EditLoanMemberAsync(List<Loan_Members> loanMemberList);
    }
}
