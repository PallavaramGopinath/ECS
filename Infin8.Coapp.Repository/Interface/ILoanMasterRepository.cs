using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface ILoanMasterRepository
    {
        Task<bool> AddLoanMasterAsync(Loan_Master loanMaster);
        Task<bool> EditLoanMasterAsync(Loan_Master loanMaster);
        Task<bool> IsLoanSchemeReferedInLoanMaster(int schemeId);
        
    }
}
