using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public  interface ILoanTrnRepository
    {
        Task<bool> AddLoanTrnListAsync(List<Loan_Trn> loanTrnList);
        Task<bool> AddLoanTrn(Loan_Trn loanTrn);
        Task<bool> EditLoanTrnListAsync(List<Loan_Trn> loanTrnList);
        Task<List<DropdownItem>> GetLoanHavingOSItemsBySchemeIdAsync(int schemeId);
        Task<double> GetJLExistingLoanOutstandingAsync(decimal memId);
        Task<List<DropdownItem>> GetLoanNosAsync(decimal memId, int loanType);
        Task<List<DropdownItem>> GetLoanNosByMemIdAndLoanTypeAsync(decimal memId, int loanType);
        Task<List<JewelLoanBalance>> GetJewelLoanNoBalanceAsync(decimal[] loanIdList, DateTime endDate,string brCode);
        Task<List<LoanDetailsHL>> GetLoanDetailsListByLoanIdsHSISAsync(decimal[] LoanNos, DateTime trnDate, int intCalcType, int societyType);
        Task<List<LoanDetailsHL>> GetLoanDetailsListByLoanIdLTAsync(decimal[] loanIds, DateTime trnDate);
        Task<List<LoanDetailsHL>> GetLoanDetailsListByLoanIdsAsync(decimal[] loanIds);
        Task<List<LoanDetailsVM>> GetLoanDetailsList2ByLoanIdsAsync(decimal[] loanIds);
        Task<LoanInterestCalculatedItems> GetCalculatedInterestComponentsForLoanAsync(decimal loanId, DateTime firstIntDueDate, DateTime maxTrnDate, DateTime? maxDueDate, DateTime intFromDate, DateTime? piFromDate, DateTime toDate, DateTime piToDate, int Int_Application, int PI_Application, int IOD_Application, double DisbAmt, double PrlColl, double PrlSchedule, double PrlDemand, double IntCalcAmt, double IntCollAmt, string DisbAgency);
        
        Task<(double appraisalFee, double bankCharges, double serviceCharges)> GetJewelLoanAppraisalFees(double loanAmount);

        #region staff loan
        Task<List<PayLoanBalanceVM>> GetPayLoanBalance(decimal empId, int loanType, DateTime toDate, string brCode);

        Task<List<PayLoanBalanceVM>> GetPayLoanBalance(decimal[] loanIdList,  DateTime toDate, string brCode);
        Task<DtoLoanDisbursementStaff> GetStaffLoanDisbursement(decimal vocId, string brCode);
        #endregion

        #region td loan
        Task<List<decimal>> GetLoanIdListByTdIdListAsync(decimal[] tdIds);
        Task<List<TDLoanData>> GetTDLoanDetailsByTDIds(decimal[] tdIds);
        Task<List<DtoTermDepositLoan>> GetTDLoanDataByTDIds(List<decimal> tdIdList);
        Task<List<DtoTermDepositLoanBalance>> GetTDLoanBalanceByTDIds(List<decimal> loanIdList,DateTime toDate, string brCode);
        
        #endregion 

    }
}
