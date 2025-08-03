using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.BusinessLogic
{
    public interface ILoanSchemeHandler
    {
        List<DropdownItem> GetLoanSchemeItems(int loanType);
        Task<List<DropdownItem>> GetLoanSchemeItemsAsync(int loanType);
        Task<List<DropdownItem>> GetLoanSchemesItemsByLoanTypeArrayAsync(int[] loanTypeList);
        bool AddLoanScheme(Loan_Schemes loanScheme);
        Task<bool> AddLoanSchemeAsync(Loan_Schemes loanScheme);
        bool EditLoanScheme(Loan_Schemes loanScheme);
        Task<bool> EditLoanSchemeAsync(Loan_Schemes loanScheme);
        Task<string> GetLoanNoStartWithAsync(int loanType);
        Task<List<DropdownItem>> GetInterestApplicationForLoanAsync();
        Task<List<DropdownItem>> GetEMIInterestApplicationForLoanAsync();
        Task<List<DropdownItem>> GetPenalInterestApplicationForLoanAsync();
        Task<List<DropdownItem>> GetPrincipalDemandFrequencyAsync();
        Task<List<DropdownItem>> GetInterestDemandFrequencyAsync();
        Task<List<DropdownItem>> GetInstalmentTypeAsync();
        Task<List<DropdownItem>> GetDisbursementTypeAsync();
        Task<Loan_Schemes> GetLoanSchemesAsync(int schemeId);
        Task<Loan_Schemes> GetLoanSchemeByType(int loanType);
        Task<int> GetPeriodOfLoan(int schemeId);
    }
}
