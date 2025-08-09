using Infin8.Coapp.Dto;
using Infin8.Coapp.Repository;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class ReportsAccountHandler : IReportsAccountHandler
    {
        readonly IUnitOfWork _unitOfWork;
        private IUtilityHandler _utility;
        readonly IAccountsHandler _accountsHandler;
        public ReportsAccountHandler(IUnitOfWork unitOfWork, IUtilityHandler utility, IAccountsHandler accountsHandler)
        {
            _unitOfWork = unitOfWork;
            _utility = utility;
            _accountsHandler = accountsHandler;
        }
        public async Task<List<rptLedgerNameList>> GetLedgerNameList(string brCode)
        {
            return await _unitOfWork.ReportsAccount.GetLedgerNameList(brCode);
        }
        public async Task<List<rptDayBook2>> GetChittaBook(string fromDate, string toDate, decimal yrId, string brCode)
        {
            double OBAmount = 0, CBAmount = 0;
            string rsInWords = "";
            decimal cashLedId = 0;
            List<rptDayBook2> dayBookList = new List<rptDayBook2>();
            
            try
            {
                cashLedId = await _unitOfWork.Accounts.GetCashLedgerId(brCode);
                dayBookList = await _unitOfWork.ReportsAccount.GetChittaBook(fromDate, toDate , yrId, brCode);
                foreach (var dayBook in dayBookList)
                {
                    OBAmount = 0; CBAmount = 0;
                    (OBAmount, CBAmount) = await _unitOfWork.Accounts.GetLedgerOBAndCBAmount(cashLedId, yrId, dayBook.Voc_Date, brCode);
                    dayBook.Fin_db_ob = OBAmount;
                    dayBook.Fin_db_cb = CBAmount;
                    if (CBAmount > 0)
                        rsInWords = _utility.RupeesInWords(CBAmount);
                    else
                        rsInWords = "Zero";
                    dayBook.RsInWords = rsInWords;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return dayBookList;
        }
        public async Task<List<rptDayBook2>> GetDayBook(string fromDate, string toDate, decimal yrId, string brCode)
        {
            double OBAmount = 0, CBAmount = 0;
            string rsInWords = "";
            decimal cashledId = 0;
            List<rptDayBook2> dayBookList = new List<rptDayBook2>();
            try
            {
                cashledId = await _unitOfWork.Accounts.GetCashLedgerId(brCode);
                dayBookList = await _unitOfWork.ReportsAccount.GetDayBook(fromDate, toDate, yrId, brCode);
                //DateTime.TryParse(fromDate, out DateTime fromDateParse);
                DateTime.TryParse(toDate, out DateTime toDateParse);
                foreach (var dayBook in dayBookList)
                {
                    (OBAmount, CBAmount) = await _unitOfWork.Accounts.GetLedgerOBAndCBAmount(cashledId, yrId, toDateParse, brCode);
                    OBAmount = 0; CBAmount = 0;
                    (OBAmount, CBAmount) = await _unitOfWork.Accounts.GetLedgerOBAndCBAmount(cashledId, yrId, dayBook.Voc_Date, brCode);
                    dayBook.Fin_db_ob = OBAmount;
                    dayBook.Fin_db_cb = CBAmount;
                    if (CBAmount > 0)
                        rsInWords = _utility.RupeesInWords(CBAmount);
                    else
                        rsInWords = "Zero";
                    dayBook.RsInWords = rsInWords;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return dayBookList;
        }
        public async Task<List<rptFinGeneralLedger>> GetGeneralLedger(DateTime fromDate, DateTime toDate, List<decimal> glLedIdList, decimal yrId, string brCode)
        {
            List<rptFinGeneralLedger> glListFinal = new List<rptFinGeneralLedger>();    
            List<rptFinGeneralLedger> glList = new List<rptFinGeneralLedger>();
            int monthNo = 0;
            decimal ledId = 0;
            decimal cashledId = 0;
            double previousRpt = 0, previousPmt = 0, openingBalance = 0, closingBalance = 0;
            try
            {
                cashledId = await _unitOfWork.Accounts.GetCashLedgerId(brCode);
                glList = await _unitOfWork.ReportsAccount.GetGeneralLedger(fromDate, toDate, glLedIdList, yrId,brCode );
                if (glList != null && glList.Count > 0)
                {
                    foreach (var gl in glList)
                    {
                        if (monthNo != gl.GL_Date.Month || ledId != gl.Led_Id)
                        {
                            previousRpt =  await  _unitOfWork.Accounts.GetPreviousReceipt(gl.Led_Id,yrId , gl.GL_Date);
                            
                            previousPmt = await _unitOfWork.Accounts.GetPreviousPayment(gl.Led_Id,yrId , gl.GL_Date);

                            if (ledId == gl.Led_Id)
                            {
                                openingBalance =  await  _unitOfWork.Accounts.GetLedgerBalance(gl.Led_Id,yrId, gl.GL_Date,brCode);
                            }
                            monthNo = gl.GL_Date.Month;
                            if (ledId != gl.Led_Id)
                            {
                                openingBalance = 0; closingBalance = 0;
                                openingBalance = await  _unitOfWork.Accounts.GetLedgerBalance(gl.Led_Id,yrId, gl.GL_Date,brCode);
                                closingBalance = openingBalance;
                                ledId = gl.Led_Id;
                            }
                        }

                        switch (gl.Fnl_Id)
                        {
                            case 1: /// Assets
                            case 4: /// Expenditure
                                if (gl.Led_Id == cashledId)
                                    closingBalance += gl.Receipts - gl.Payments;
                                else
                                    closingBalance += gl.Payments - gl.Receipts;
                                break;
                            case 2: /// Liability
                            case 3: /// Income
                                closingBalance += gl.Receipts - gl.Payments;
                                break;
                        }
                        gl.OpeningBalance = openingBalance;
                        gl.ClosingBalance = closingBalance;
                        gl.PreviousPmt = previousPmt;
                        gl.PreviousRpt = previousRpt;

                        /// if ledger name is "" 
                        if (gl.Led_Name == "") gl.Led_Name = _unitOfWork.Accounts.GetLedgerNameByLedId(gl.Led_Id);
                    }
                }
                if(glList !=null) glListFinal = glList;
            }
            catch (Exception)
            {
                throw;
            }
            return glListFinal;
        }
        public async Task<List<rptFinBalanceSheet>> GetBalanceSheet(DateTime fromDate, DateTime toDate, decimal yrId,string brCode)
        {
            List<rptFinBalanceSheet> balanceSheet = new List<rptFinBalanceSheet>();
            try
            {
                if (!await _unitOfWork.Accounts.UpdateLedgerBalance(yrId, fromDate, toDate, brCode))
                {
                    return balanceSheet;
                }
                var balanceSheetTmp = await _unitOfWork.ReportsAccount.GetBalanceSheet(fromDate, toDate, yrId,brCode);
                if(balanceSheetTmp != null) balanceSheet= balanceSheetTmp;
            }
            catch (Exception)
            {
                throw;
            }
            return balanceSheet;
        }
        public async Task<List<rptFinLossAndProfit>> GetLossAndProfit(DateTime fromDate, DateTime toDate, decimal yrId,string brCode)
        {
            List<rptFinLossAndProfit> lossAndProfitList = new List<rptFinLossAndProfit>();
            try
            {
                if(! await _unitOfWork.Accounts.UpdateLedgerBalance(yrId,fromDate ,toDate ,brCode))
                {
                    return lossAndProfitList;
                }
                var lpTmp = await _unitOfWork.ReportsAccount.GetLossAndProfit(fromDate, toDate, yrId,brCode);
                if(lpTmp != null) lossAndProfitList = lpTmp;
            }
            catch (Exception)
            {
                throw;
            }
            return lossAndProfitList;
        }
        public async Task<List<rptFinReceiptAndCharges>> GetReceiptAndCharges(DateTime fromDate, DateTime toDate, decimal yrId,string brCode)
        {
            return await _unitOfWork.ReportsAccount.GetReceiptAndCharges(fromDate, toDate, yrId,brCode);
        }
    }
}
