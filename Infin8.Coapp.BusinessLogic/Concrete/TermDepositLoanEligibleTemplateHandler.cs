using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class TermDepositLoanEligibleTemplateHandler : ITermDepositLoanEligibleTemplateHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public TermDepositLoanEligibleTemplateHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddTermDepositLoanEligibleTemplateAsync(TermDeposit_LoanEligibleTemplate termDepositLoanEligibleTemplate)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.TermDepositLoanEligibleTemplate.AddTermDepositLoanEligibleTemplateAsync(termDepositLoanEligibleTemplate); 
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan eligible on term deposit template not saved");
            }
            return result;
        }

        public async  Task<bool> EditTermDepositLoanEligibleTemplateAsync(TermDeposit_LoanEligibleTemplate termDepositLoanEligibleTemplate)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.TermDepositLoanEligibleTemplate.EditTermDepositLoanEligibleTemplateAsync(termDepositLoanEligibleTemplate);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan eligible on term deposit template not deleted");
            }
            return result;
        }

        public async Task<double> GetTDLoanEligiblePercentageAsync(int tdSchemeType,string brCode)
        {
            return await _unitOfWork.TermDepositLoanEligibleTemplate.GetTDLoanEligiblePercentageAsync(tdSchemeType, brCode);
        }

        public async Task<List<TermDeposit_LoanEligibleTemplate>> GetTermDepositLoanEligibleTemplateListAsync()
        {
            return await _unitOfWork.TermDepositLoanEligibleTemplate.GetTermDepositLoanEligibleTemplateListAsync();
        }
    }
}
