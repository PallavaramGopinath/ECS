using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class CalendarHandler : ICalendarHandler
    {
        readonly IUnitOfWork _unitOfWork;
        readonly IUtilityHandler _UtilityHandler;
        readonly ITermDepositTrnHandler _TermDepositTrnHandler;
        readonly IStagingMasterHandler _stagingMasterHandler;
        readonly IStagingDetailsHandler _stagingDetailsHandler;
        readonly IStagingHistoryHandler _stagingHistoryHandler;
        readonly IStagingBalanceHandler _stagingBalanceHandler;
        public CalendarHandler(IUnitOfWork unitOfWork, IUtilityHandler utilityHandler, ITermDepositTrnHandler termDepositTrnHandler
            , IStagingMasterHandler stagingMasterHandler, IStagingDetailsHandler stagingDetailsHandler, 
            IStagingHistoryHandler stagingHistoryHandler, IStagingBalanceHandler stagingBalanceHandler  )
        {
            _unitOfWork = unitOfWork;
            _UtilityHandler = utilityHandler;
            _TermDepositTrnHandler = termDepositTrnHandler;
            _stagingMasterHandler = stagingMasterHandler;
            _stagingDetailsHandler = stagingDetailsHandler;
            _stagingHistoryHandler = stagingHistoryHandler;
            _stagingBalanceHandler = stagingBalanceHandler;
        }
        public async Task<bool> VerifyDayBegin(string brCode)
        {
            return await _unitOfWork.Calendars.VerifyDayBegin(brCode);
        }
        public async Task<DateTime> GetCurrentDate(string brCode)
        {
            return await _unitOfWork.Calendars.GetCurrentDate(brCode);
        }
        public async Task<int> UpdateCalendarStatus(DateTime currentDate, string brCode, string newStatus)
        {
            return await _unitOfWork.Calendars.UpdateCalendarStatus(currentDate, brCode, newStatus);
        }

        public async Task<bool> DayEndProcess(DtoDayProcess dayProcess)
        {
            bool result = false;
            List<decimal> fdIdList = new();
            List<FDDetailsVM> fdList = new();
            List<TermDeposit_Trn> fdInterestCalculatedList = new();
            List<Staging_Master> masterList = new();
            List<Staging_Details> stagingDetailsList = new();
            List<DtoAccountsBalance> accountsBalanceList = new();
            try
            {
                _unitOfWork.BeginTransaction();

                #region Step 1: Get staging master list
                var masterResult = await _stagingMasterHandler.GetStagingMasterListByDate(dayProcess.ProcessDate, dayProcess.BrCode!);
                if (masterResult != null && masterResult.Any())
                {
                    masterList = masterResult.ToList();
                }
                else
                {
                    dayProcess.Message = "Pending staging data not available to day end process";
                    goto CompleteDayProcess;
                }
                #endregion 

                #region Step 2: Get staging_details list
                var detailsResult = await _stagingDetailsHandler.GetStagingDetailsByDate(dayProcess.ProcessDate, dayProcess.BrCode!);
                if(detailsResult != null && detailsResult.Any())
                {
                    stagingDetailsList = detailsResult.ToList();
                }
                else
                {
                    dayProcess.Message = "Pending staging data not available to day end process";
                    goto CompleteDayProcess;
                }
                #endregion

                #region  Step 3: Create new staging_history records with new IDs
                //Id = maxId + index + 1, // Generate new unique ID update in staging history repository
                var stagingHistories = stagingDetailsList.Select((sd, index) => new Staging_History
                {
                    Id =0,
                    Staging_Id = sd.Staging_Id,
                    Member_Id = sd.Member_Id,
                    Ledger_Id = sd.Ledger_Id,
                    Related_Account_Id = sd.Related_Account_Id,
                    Receipt_Amount = sd.Receipt_Amount,
                    Payment_Amount = sd.Payment_Amount,
                    Module_Name = sd.Module_Name,
                    Cash_Or_Adjustment = sd.Cash_Or_Adjustment,
                    Related_Account_Data = sd.Related_Account_Data,
                    Created_By = sd.Created_By,
                    Created_Date = sd.Created_Date,
                    Checked_By = sd.Checked_By,
                    Checked_Date = sd.Checked_Date,
                    Staging_Status = sd.Staging_Status,
                    BrCode = sd.BrCode,
                    Cheque_No = sd.Cheque_No,
                    Cheque_Date = sd.Cheque_Date,
                    Issue_Bank_Name = sd.Issue_Bank_Name,
                    Voc_Id = sd.Voc_Id,
                    CashReceipt_Amount = sd.CashReceipt_Amount,
                    CashPayment_Amount = sd.CashPayment_Amount,
                    AdjustmentReceipt_Amount = sd.AdjustmentReceipt_Amount,
                    AdjustmentPayment_Amount = sd.AdjustmentPayment_Amount,
                    Security_Type = sd.Security_Type,
                }).ToList();

                #endregion

                #region Step 4: Insert staging history
                var addHistoryResult = await  _stagingHistoryHandler.AddStagingHistoryList(stagingHistories);
                if(addHistoryResult == false )
                {
                    dayProcess.Message = "Push staging data to staging history failed";
                    goto CompleteDayProcess;
                }
                #endregion 

                #region Step 5:  Calculate interest on fixed deposits
                fdIdList = await _TermDepositTrnHandler.GetFDIdListForDayEndCalculation(dayProcess.ProcessDate.Day, dayProcess.BrCode!);
                if(fdIdList != null && fdIdList.Any())
                {
                    fdList = await _TermDepositTrnHandler.GetFDPayableByTDIdsAsync(fdIdList.ToArray(), dayProcess.ProcessDate, 7, dayProcess.BrCode!);
                    foreach (var fd in fdList)
                    {
                        TermDeposit_Trn tdTrn = new();
                        tdTrn = Utility.GetModalObject.GetTermDepositTrn(0, dayProcess.ProcessDate, fd.FDId, 0, 0, 0, fd.FDIntCalculatedNow, fd.FDIntCalculatedDateNow, 0, 0, 0, null, 0, 0, 0, null, null, false, false, 0, dayProcess.Created_By, dayProcess.YrId, 0, 0, dayProcess.BrCode!);
                        fdInterestCalculatedList.Add(tdTrn);
                    }
                    result = await _unitOfWork.TermDepositTrn.AddTermDepositTrnListAsync(fdInterestCalculatedList);
                    if (result == false)
                    {
                        dayProcess.Message = "Calculation if interest payable for fixed deposit(s) failed";
                        goto CompleteDayProcess;
                    }
                }

                #endregion

                #region Step 6:  Calculate payable on Recurring Deposit
                #endregion

                #region Step 7:  Calculate demand on loans on due date
                #endregion

                #region Step 8:  if year end calculate interest on provident fund
                #endregion

                #region Step 9:  Insert staging balance
                var resultAddStagingBalance = await _stagingBalanceHandler.AddStagingBalance(dayProcess.YrId, dayProcess.ProcessDate,  dayProcess.Created_By, dayProcess.BrCode!);
                if (resultAddStagingBalance == false)
                {
                    dayProcess.Message = "Addition of staginb balance failed";
                    goto CompleteDayProcess;
                }
                #endregion 

                #region Step 10: delete staging_master and staging_details table and push to staging_history
                var masterDeleteResult = await _stagingMasterHandler.DeleteStagingMaster(masterList);
                if (masterDeleteResult == false)
                {
                    dayProcess.Message = "Deletetion of staging master data failed";
                    goto CompleteDayProcess;
                }
                #endregion

                #region Step 11: Delete staging details data
                var detailsDeleteResult = await _stagingDetailsHandler.DeleteStagingDetails(stagingDetailsList);
                if (detailsDeleteResult == false)
                {
                    dayProcess.Message = "Deletetion of staging details data failed";
                    goto CompleteDayProcess;
                }
                #endregion

                #region Step 12: Close the date
                var resultCalendarUpdate = await _unitOfWork.Calendars.UpdateCalendarStatus(dayProcess.ProcessDate, dayProcess.BrCode!, "E");
                if(resultCalendarUpdate  == 0)
                {
                    dayProcess.Message = "Day not marked on the date " + dayProcess.ProcessDate.ToString("yyyy-MM-dd");
                    goto CompleteDayProcess;
                }
                #endregion
            CompleteDayProcess:
                    _unitOfWork.CommitTransaction();
                    result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _unitOfWork.RollBack();
                result = false;
            }
            return result;
        }

        public async Task<bool> CanBeginDay(string brCode)
        {
            return await _unitOfWork.Calendars.CanBeginDay(brCode);
        }

        public async Task<DateTime> DayBeginProcess(string brCode)
        {
            return await _unitOfWork.Calendars.DayBeginProcess(brCode);
        }

        public async Task<List<decimal>> GetFixedDepositIdForInterestCalculation(string tdSchemeType, DateTime toDate, string brCode)
        {
            return await _unitOfWork.Calendars.GetFixedDepositIdForInterestCalculation(tdSchemeType, toDate, brCode);
        }

        public  bool IsMonthEnd(DateTime date)
        {
            // Check if the next day is in a different month
            return date.AddDays(1).Month != date.Month;
        }

        public  bool IsYearEnd(DateTime date)
        {
            // Check if it's March 31st (financial year end)
            return date.Month == 3 && date.Day == 31;
        }

        //public static (bool isMonthEnd, bool isYearEnd) CheckDate(DateTime date)
        //{
        //    return (IsMonthEnd(date), IsYearEnd(date));
        //}
    }
}
