using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface ILoanSanctionTrnRepository
    {
        Task<bool> AddLoanSanctionTrnAsync(List<Loan_Sanction_Trn> loanSanctionTrnList);
        Task<bool> EditLoanSanctionTrnAsync(List<Loan_Sanction_Trn> loanSanctionTrnList);

    }
}
