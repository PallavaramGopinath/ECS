using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IPaySlipLoanTrnRepository
    {
        Task<bool> AddPaySlipLoanTrnAsync(Pay_Slip_Loan_Trn paySlipLoanTrn);
        Task<bool> EditPaySlipLoanTrnAsync(Pay_Slip_Loan_Trn paySlipLoanTrn);
        Task<List<Pay_Slip_Loan_Trn>> GetPaySlipLoanTrnList(decimal payId, decimal empId, string brCode);
    }
}
