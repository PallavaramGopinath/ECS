using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IPaySlipLoanTrnHandler
    {
        Task<bool> AddPaySlipLoanTrnAsync(Pay_Slip_Loan_Trn paySlipLoanTrn);
        Task<bool> EditPaySlipLoanTrnAsync(Pay_Slip_Loan_Trn paySlipLoanTrn);
    }
}
