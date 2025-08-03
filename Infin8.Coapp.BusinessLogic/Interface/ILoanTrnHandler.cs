using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface ILoanTrnHandler
    {
        Task<bool> AddLoanTrnListAsync(List<Loan_Trn> loanTrnList);
        Task<bool> AddLoanTrn(Loan_Trn loanTrn);
        Task<bool> EditLoanTrnListAsync(List<Loan_Trn> loanTrnList);
        Task<List<DropdownItem>> GetLoanHavingOSItemsBySchemeIdAsync(int schemeId);
        Task<double> GetJLExistingLoanOutstandingAsync(decimal memId);
        Task<List<DropdownItem>> GetLoanNosAsync(decimal memId, int loanType);
        Task<List<DropdownItem>> GetLoanNosByMemIdAndLoanTypeAsync(decimal memId, int loanType);
        Task<List<JewelLoanBalance>> GetJewelLoanNoBalanceAsync(decimal[] loanIdList, DateTime endDate,string brCode);

        Task<List<LoanDetailsHL>> GetLoanDetailsListByLoanIdsHSISAsync(decimal[] loanIds, DateTime trnDate, int intCalcType, int societyType);

       
        Task<List<LoanDetailsVM>> GetLoanDetailsList2ByLoanIdsAsync(decimal[] loanIds);
        Task<(double appraisalFee, double bankCharges, double serviceCharges)> GetJewelLoanAppraisalFees(double loanAmount);

        #region td loan
        Task<List<LoanDetailsVM>> GetTDLoanDetailsByTDIdsAsync(decimal[] TDNos, DateTime toDate);
        Task<List<decimal>> GetLoanIdListByTdIdListAsync(decimal[] tdIds);
        Task<List<TDLoanData>> GetTDLoanDetailsByTDIds(decimal[] tdIds);
        Task<List<DtoTermDepositLoan>> GetTDLoanDataByTDIds(List<decimal> tdIdList,DateTime toDate);
        Task<List<DtoTermDepositLoanBalance>> GetTDLoanBalanceByTDIds(List<decimal> loanIdList, DateTime toDate, string brCode);
        #endregion 
    }

}
