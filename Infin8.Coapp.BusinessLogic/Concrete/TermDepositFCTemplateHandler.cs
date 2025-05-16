using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class TermDepositFCTemplateHandler : ITermDepositFCTemplateHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public TermDepositFCTemplateHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddTermDepositFCTemplateAsync(TermDeposit_FcTemplate termDepositFCTemplate)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.TermDepositFCTemplate.AddTermDepositFCTemplateAsync(termDepositFCTemplate);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Fore closure for term deposit not saved");
            }
            return result;
        }

        public async Task<bool> EditTermDepositFCTemplateAsync(TermDeposit_FcTemplate termDepositFCTemplate)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.TermDepositFCTemplate.EditTermDepositFCTemplateAsync(termDepositFCTemplate);  
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Fore closure for term deposit not deleted");
            }
            return result;
        }

        public async Task<double> GetTDForeClosureROIAsync(int tdSchemeType, DateTime wef)
        {
            return await _unitOfWork.TermDepositFCTemplate.GetTDForeClosureROIAsync(tdSchemeType, wef);
        }

        public async Task<List<TermDeposit_FcTemplate>> GetTermDepositFCTemplateListAsync()
        {
            return await _unitOfWork.TermDepositFCTemplate.GetTermDepositFCTemplateListAsync();
        }
    }
}
