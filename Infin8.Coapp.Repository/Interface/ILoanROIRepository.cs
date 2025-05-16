using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public  interface ILoanROIRepository
    {
        Task<bool> AddLoanROIAsync(Loan_Roi loanRoi);
        Task<bool> AddLoanRoiListAsync(List<Loan_Roi> loanRoiList);
        Task<bool> EditLoanROIAsync(Loan_Roi loanRoi);
        Task<bool> EditLoanRoiListAsync(List<Loan_Roi> loanRoiList);
        
    }
}
