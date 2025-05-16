using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface ITermDepositROITemplateHandler
    {
        Task<bool> AddTermDepositROITemplateAsync(TermDeposit_Roi_Template termDepositROITemplate);
        Task<bool> EditTermDepositROITemplateAsync(TermDeposit_Roi_Template termDepositROITemplate);
        Task<List<TDRateOfInterstDto>> GetTermDepositROITemplateListAsync(string[] tdSchemeTypeList);
        Task<double> GetROIForTermDepositAsync(DateTime depositDate, int schemeId, int prdInMonths, int prdInDays, string brCode);
        Task<double> GetPIForRDAsync(DateTime depositDate, int schemeId, int prdInMonths, int prdInDays, string brCode);
    }
}
