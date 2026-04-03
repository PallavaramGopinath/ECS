using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class TermDepositROITemplateHandler : ITermDepositROITemplateHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public TermDepositROITemplateHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddTermDepositROITemplateAsync(TermDeposit_Roi_Template termDepositROITemplate)
        {
            return await _unitOfWork.TermDepositROITemplate.AddTermDepositROITemplateAsync(termDepositROITemplate);
        }

        public async Task<List<TermDeposit_Roi_Template>> AddTermDepositROITemplateList(List<TermDeposit_Roi_Template> termDepositList)
        {
            return await _unitOfWork.TermDepositROITemplate.AddTermDepositROITemplateList(termDepositList);
        }

        public async Task<List<TermDeposit_Roi_Template>> EditTermDepositROITemplateAsync(TermDeposit_Roi_Template termDepositROITemplate)
        {
            return await _unitOfWork.TermDepositROITemplate.EditTermDepositROITemplateAsync(termDepositROITemplate);
        }

        public async Task<double> GetROIForTermDepositAsync(DateTime depositDate, int schemeId, int prdInMonths, int prdInDays, string brCode)
        {
            return await _unitOfWork.TermDepositROITemplate.GetROIForTermDepositAsync(depositDate, schemeId, prdInMonths, prdInDays,brCode );
        }

        public async Task<List<TDRateOfInterstDto>> GetTermDepositROITemplateListAsync(string[] tdSchemeTypeList)
        {
            return await _unitOfWork.TermDepositROITemplate.GetTermDepositROITemplateListAsync(tdSchemeTypeList);
        }
        public async Task<double> GetPIForRDAsync(DateTime depositDate, int schemeId, int prdInMonths, int prdInDays, string brCode)
        {
            return await _unitOfWork.TermDepositROITemplate.GetPIForRDAsync(depositDate,schemeId, prdInMonths,brCode );
        }

        public async Task<List<TermDeposit_Roi_Template>> GetTermDepositRateOfInterestList(string SchemeType, string brCode)
        {
            return await _unitOfWork.TermDepositROITemplate.GetTermDepositRateOfInterestList(SchemeType, brCode);
        }

        public async Task<double> GetSecurityDepositRoi(int schemeId, DateTime toDate, string brCode)
        {
            return await _unitOfWork.TermDepositROITemplate.GetSecurityDepositRoi(schemeId, toDate, brCode);
        }
    }
}
