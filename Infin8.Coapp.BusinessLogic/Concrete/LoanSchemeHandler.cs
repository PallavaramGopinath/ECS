using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;

namespace Infin8.Coapp.BusinessLogic
{
    public class LoanSchemeHandler : ILoanSchemeHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LoanSchemeHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public List<DropdownItem> GetLoanSchemeItems(int loanType)
        {
            return _unitOfWork.LoanScheme.GetLoanSchemeItems(loanType);
        }
        public async Task<List<DropdownItem>> GetLoanSchemeItemsAsync(int loanType, string brCode)
        {
            return await _unitOfWork.LoanScheme.GetLoanSchemeItemsAsync(loanType,brCode );
        }
        public bool AddLoanScheme(Loan_Schemes loanScheme)
        {
            _unitOfWork.LoanScheme.AddLoanScheme(loanScheme);
            _unitOfWork.Complete();
            return true;
        }

        public async Task<bool> AddLoanSchemeAsync(Loan_Schemes loanScheme)
        {
            bool result = false;
            result =  await  _unitOfWork.LoanScheme.AddLoanSchemeAsync(loanScheme);
            await  _unitOfWork.CompleteAsync();
            return result;
        }

        public bool EditLoanScheme(Loan_Schemes loanScheme)
        {
            return _unitOfWork.LoanScheme.EditLoanScheme(loanScheme);
        }

        public async Task<bool> EditLoanSchemeAsync(Loan_Schemes loanScheme)
        {
            bool result = false;
            result = await _unitOfWork.LoanScheme.AddLoanSchemeAsync(loanScheme);
            await _unitOfWork.CompleteAsync();
            return result;
        }

        public async Task<string> GetLoanNoStartWithAsync(int loanType)
        {
            return await _unitOfWork.LoanScheme.GetLoanNoStartWithAsync(loanType);;
        }

        public async Task<List<DropdownItem>> GetInterestApplicationForLoanAsync()
        {
            return await _unitOfWork.LoanScheme.GetInterestApplicationForLoanAsync();
        }

        public async Task<List<DropdownItem>> GetEMIInterestApplicationForLoanAsync()
        {
            return await _unitOfWork.LoanScheme.GetEMIInterestApplicationForLoanAsync();
        }

        public async Task<List<DropdownItem>> GetPenalInterestApplicationForLoanAsync()
        {
            return await _unitOfWork.LoanScheme.GetPenalInterestApplicationForLoanAsync();
        }

        public async Task<List<DropdownItem>> GetPrincipalDemandFrequencyAsync()
        {
            return await _unitOfWork.LoanScheme.GetPrincipalDemandFrequencyAsync();
        }

        public async Task<List<DropdownItem>> GetInterestDemandFrequencyAsync()
        {
            return await _unitOfWork.LoanScheme.GetInterestDemandFrequencyAsync();
        }

        public async Task<List<DropdownItem>> GetInstalmentTypeAsync()
        {
            return await _unitOfWork.LoanScheme.GetInstalmentTypeAsync();
        }

        public async Task<List<DropdownItem>> GetDisbursementTypeAsync()
        {
            return await _unitOfWork.LoanScheme.GetDisbursementTypeAsync();
        }

        public async Task<Loan_Schemes> GetLoanSchemesAsync(int schemeId, string brCode)
        {
            return await _unitOfWork.LoanScheme.GetLoanSchemesAsync(schemeId,brCode );
        }

        public async Task<List<DropdownItem>> GetLoanSchemesItemsByLoanTypeArrayAsync(int[] loanTypeList)
        {
            return await _unitOfWork.LoanScheme.GetLoanSchemesItemsByLoanTypeArrayAsync(loanTypeList);
        }

        public async Task<int> GetPeriodOfLoan(int schemeId, string brCode)
        {
           return await _unitOfWork.LoanScheme.GetPeriodOfLoan(schemeId,brCode);
        }

        public async Task<Loan_Schemes> GetLoanSchemeByType(int loanType, string brCode)
        {
            return  await _unitOfWork.LoanScheme.GetLoanSchemeByType(loanType, brCode);
        }

        public async Task<List<Loan_Schemes>> GetLoanSchemeListByType(int loanType, string brCode)
        {
            return await _unitOfWork.LoanScheme.GetLoanSchemeListByType(loanType, brCode);
        }

        public async Task<List<Loan_Schemes>> GetAllLoanProductes(string brCode)
        {
            return await _unitOfWork.LoanScheme.GetAllLoanProductes(brCode);
        }
    }
}
