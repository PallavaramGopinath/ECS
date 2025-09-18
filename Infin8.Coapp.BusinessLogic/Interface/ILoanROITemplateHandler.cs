using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface ILoanROITemplateHandler
    {
        Task<List<Loan_Roi_Template>> GetLoanRateOfInterestListAsync(int schemeId);
        Task<bool> AddLoanROIAsync(Loan_Roi_Template roiTemplate);
        Task<bool> EditLoanROIAsync(Loan_Roi_Template roiTemplate);
        Task<LoanROIAndPIVM> GetLoanROIAndPIFromTemplateAsync(int schemeId, string agency, DateTime wef);
        Task<LoanROIAndPIVM> GetLoanROIAndPIFromTemplateAsync(int schemeId, string agency, DateTime wef, string brCode);
    }
}
