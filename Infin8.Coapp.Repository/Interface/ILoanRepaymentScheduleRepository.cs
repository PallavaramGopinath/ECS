using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public  interface ILoanRepaymentScheduleRepository
    {
        Task<bool> AddLoanRepaymentScheduleAsync(List<Loan_Repayment_Schedule> loanRepaymentScheduleList);
        Task<bool> EditLoanRepaymentScheduleAsync(List<Loan_Repayment_Schedule> loanRepaymentScheduleList);
    }
}
