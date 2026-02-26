using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.Repository
{
    public interface ILoanSchemeRepository
    {
        List<DropdownItem> GetLoanSchemeItems(int loanType);
        Task<List<DropdownItem>> GetLoanSchemeItemsAsync(int loanType,string brCode);
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
        Task<Loan_Schemes> GetLoanSchemesAsync(int schemeId,string brCode);
        Task<Loan_Schemes> GetLoanSchemeByType(int loanType, string brCode);
        Task<List<Loan_Schemes>> GetLoanSchemeListByType(int loanType, string brCode);
        Task<int> GetPeriodOfLoan(int schemeId,string brCode);
        Task<List<Loan_Schemes>> GetAllLoanProductes(string brCode);

    }
}
