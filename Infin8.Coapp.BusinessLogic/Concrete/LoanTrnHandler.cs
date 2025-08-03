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
    public  class LoanTrnHandler : ILoanTrnHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LoanTrnHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddLoanTrnListAsync(List<Loan_Trn> loanTrnList)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanTrn.AddLoanTrnListAsync(loanTrnList);    
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan trn list not saved");
            }
            return result;
        }
        public async Task<bool> AddLoanTrn(Loan_Trn loanTrn)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanTrn.AddLoanTrn(loanTrn);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan trn not saved");
            }
            return result;
        }

        public async Task<bool> EditLoanTrnListAsync(List<Loan_Trn> loanTrnList)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanTrn.EditLoanTrnListAsync(loanTrnList);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan trn list not deleted");
            }
            return result;
        }

        public async Task<List<JewelLoanBalance>> GetJewelLoanNoBalanceAsync(decimal[] loanIdList, DateTime endDate,string brCode)
        {
            decimal id = 0;
            int jlMinimumDays = 0;
            double intCalc = 0;
            double piCalc = 0;
            DateTime fromDate;
            DateTime toDate;
            int noOfDays = 0;
            UtilityHandler handler = new UtilityHandler();
            List<JewelLoanBalance> jlBalanceList = new List<JewelLoanBalance> { new JewelLoanBalance() };
            try
            {
                id = Convert.ToDecimal(brCode + "0000001");
                Map_General mapGeneral = new Map_General();
                mapGeneral = await _unitOfWork.MapGeneral.GetMapGeneralAsync(brCode);
                jlMinimumDays = mapGeneral.JL_minimum_Days;

                jlBalanceList  = await _unitOfWork.LoanTrn.GetJewelLoanNoBalanceAsync(loanIdList, endDate,brCode);

                if (jlBalanceList.Count >0)
                {
                    foreach (var jl in jlBalanceList) 
                    {
                        fromDate = new DateTime(jl.San_Date.Year, jl.San_Date.Month, jl.San_Date.Day);
                        toDate = new DateTime(endDate.Year, endDate.Month, endDate.Day);
                        intCalc = 0;
                        piCalc = 0;
                        noOfDays = handler.GetNoOfDays(endDate, fromDate);
                        if (noOfDays < jlMinimumDays)
                            toDate = fromDate.AddDays(jlMinimumDays);
                        else
                            toDate = endDate;
                        if (jl.IntCalc_Date != null) fromDate = (DateTime)jl.IntCalc_Date;
                        if (jl.Prl_Bal > 0)
                        {
                            intCalc = handler.Calculate_Interest(jl.Prl_Bal, jl.Roi, handler.GetNoOfDays(toDate.Date, fromDate.Date));
                        }
                        if (handler.GetNoOfDays(toDate, jl.JL_DueDate) > 0)
                        {
                            if (jl.PICalc_Date != null)
                                fromDate = (DateTime)jl.PICalc_Date;
                            else
                                fromDate = jl.JL_DueDate;
                            piCalc = handler.Calculate_Interest(jl.Prl_Bal, jl.Pi, handler.GetNoOfDays(endDate, fromDate));
                        }
                        jl.IntCalc_Amt = intCalc;
                        jl.PICalc_Amt = piCalc;
                        if (intCalc > 0) jl.IntCalc_DateNow = toDate.Date;
                        else
                            jl.IntCalc_DateNow = null;
                        if (piCalc > 0) jl.PICalc_DateNow = toDate.Date;
                        else
                            jl.PICalc_DateNow = null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while calculating jewel loan balance");
            }
            return jlBalanceList;   
        }

        public async Task<double> GetJLExistingLoanOutstandingAsync(decimal memId)
        {
            return await _unitOfWork.LoanTrn.GetJLExistingLoanOutstandingAsync(memId);
        }

        public async Task<List<LoanDetailsVM>> GetLoanDetailsList2ByLoanIdsAsync(decimal[] loanIds)
        {
            return await _unitOfWork.LoanTrn.GetLoanDetailsList2ByLoanIdsAsync(loanIds);
        }

        public async Task<List<LoanDetailsHL>> GetLoanDetailsListByLoanIdsHSISAsync(decimal[] loanIds, DateTime trnDate, int intCalcType, int societyType)
        {
            return await _unitOfWork.LoanTrn.GetLoanDetailsListByLoanIdsHSISAsync(loanIds,trnDate,intCalcType ,societyType);
        }

        public async Task<List<DropdownItem>> GetLoanHavingOSItemsBySchemeIdAsync(int schemeId)
        {
            return await _unitOfWork.LoanTrn.GetLoanHavingOSItemsBySchemeIdAsync(schemeId);
        }

        public async Task<List<DropdownItem>> GetLoanNosAsync(decimal memId, int loanType)
        {
            return await _unitOfWork.LoanTrn.GetLoanNosAsync(memId, loanType);
        }

        public async Task<List<DropdownItem>> GetLoanNosByMemIdAndLoanTypeAsync(decimal memId, int loanType)
        {
            return await _unitOfWork.LoanTrn.GetLoanNosByMemIdAndLoanTypeAsync((decimal)memId, loanType);
        }

        public async Task<(double appraisalFee, double bankCharges, double serviceCharges)> GetJewelLoanAppraisalFees(double loanAmount)
        {
            return await _unitOfWork.LoanTrn.GetJewelLoanAppraisalFees(loanAmount);
        }

        #region TD Loan
        public async Task<List<LoanDetailsVM>> GetTDLoanDetailsByTDIdsAsync(decimal[] TDNos, DateTime toDate)
        {
            DateTime IntCalcDate;
            DateTime IntToDate = toDate;
            List<LoanDetailsVM> loanDetailsList = new List<LoanDetailsVM>();
            List<decimal> loanIdList = new List<decimal>();
            UtilityHandler utilityHandler = new UtilityHandler();
            try
            {
                loanIdList = await _unitOfWork.LoanTrn.GetLoanIdListByTdIdListAsync(TDNos);
                if (loanIdList.Count > 0)
                {
                    var loanList = await _unitOfWork.LoanTrn.GetLoanDetailsList2ByLoanIdsAsync(loanIdList.ToArray());
                    if (loanList != null && loanList.Count > 0) loanDetailsList = loanList;
                    if (loanDetailsList.Count > 0)
                    {
                        foreach (var loan in loanDetailsList)
                        {
                            if (loan.maxintcalcdate == null) IntCalcDate = loan.disbursementdate;
                            else IntCalcDate = (DateTime)loan.maxintcalcdate;
                            loan.intcalcamt = utilityHandler.Calculate_Interest(loan.disbamt - loan.prlcoll, loan.roi, utilityHandler.GetNoOfDays(IntToDate, IntCalcDate));
                            loan.intcalcdate = IntToDate;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching jewel loan balance by member no");
            }
            return loanDetailsList;
        }
        public async Task<List<decimal>> GetLoanIdListByTdIdListAsync(decimal[] tdIds)
        {
            return await _unitOfWork.LoanTrn.GetLoanIdListByTdIdListAsync(tdIds);
        }
        public async Task<List<TDLoanData>> GetTDLoanDetailsByTDIds(decimal[] tdIds)
        {
            return await _unitOfWork.LoanTrn.GetTDLoanDetailsByTDIds(tdIds);
        }
        public async Task<List<DtoTermDepositLoan>> GetTDLoanDataByTDIds(List<decimal> tdIdList, DateTime toDate)
        {
            List<DtoTermDepositLoan> loanList = new List<DtoTermDepositLoan>();
            DateTime IntCalcDate;
            try
            {
                UtilityHandler utilityHandler = new UtilityHandler();
                var result = await  _unitOfWork.LoanTrn.GetTDLoanDataByTDIds(tdIdList);
                if (result != null)
                {
                    loanList = result.ToList();
                    foreach(var loan in loanList)
                    {
                        if (loan.IntCalc_Date == null)
                        {
                            IntCalcDate = loan.Loan_Date;
                            loan.IntCalc_Date = loan.IntCalc_Date;
                        }
                        else IntCalcDate = (DateTime)loan.IntCalc_Date;
                        loan.Current_Interest = utilityHandler.Calculate_Interest(loan.Principal_Balance , loan.Rate_Of_Interest , utilityHandler.GetNoOfDays(toDate, IntCalcDate));
                        loan.Current_IntCalc_Date = toDate;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return loanList;
        }

        public async Task<List<DtoTermDepositLoanBalance>> GetTDLoanBalanceByTDIds(List<decimal> loanIdList, DateTime toDate, string brCode)
        {
            return await _unitOfWork.LoanTrn.GetTDLoanBalanceByTDIds(loanIdList, toDate, brCode);
        }

        

        #endregion
    }
}
