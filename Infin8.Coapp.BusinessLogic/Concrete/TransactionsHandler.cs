using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class TransactionsHandler : ITransactionsHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public TransactionsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<DropdownItem>> GetTransactionAccounts(string accountStatus, string accountBelongTo)
        {
            return await _unitOfWork.TransactionsRepository.GetTransactionAccounts(accountStatus, accountBelongTo);
        }
        public async Task<List<DtoAccount_Transactions>> GetAllAccountsTransactions()
        {
            return await _unitOfWork.TransactionsRepository.GetAllAccountsTransactions();
        }
        public async Task<List<DropdownItem>> GetAllLedgerItems(string brCode)
        {
            return await _unitOfWork.TransactionsRepository.GetAllLedgerItems(brCode);
        }
        public async Task<List<DropdownItem>> GetSuspenseLedgerItems(int suspeneType, string brCode)
        {
            return await _unitOfWork.TransactionsRepository.GetSuspenseLedgerItems(suspeneType, brCode);
        }
        public async Task<List<DropdownItem>> GetShareCapitalLedgerItem(string brCode)
        {
            return await _unitOfWork.TransactionsRepository.GetShareCapitalLedgerItem(brCode);
        }
        public async Task<List<DropdownItem>> GetBankLedgerItems(string brCode)
        {
            return await _unitOfWork.TransactionsRepository.GetBankLedgerItems(brCode);
        }
        public async Task<List<DropdownItem>> GetLedgersExpectBankLedgerItems(string brCode)
        {
            return await _unitOfWork.TransactionsRepository.GetLedgersExpectBankLedgerItems(brCode);
        }
        public async Task<string> GetComponentName(int accountId)
        {
            return await _unitOfWork.TransactionsRepository.GetComponentName(accountId);
        }
        public async Task<string> GetViewComponentName(int accountId)
        {
            return await _unitOfWork.TransactionsRepository.GetViewComponentName(accountId);
        }
        public async Task<DtoVoucher> SaveTransaction(decimal stagingId, string vocMode, decimal Checked_By, decimal yrId)
        {
            bool result = false;
            DtoVoucher? voucherData = new();
            List<DtoVoucherTrn > dtoVoucherTrns = new List<DtoVoucherTrn>();
            int cashOrAdj = 0;
            string brCode = "";
            string Status = "";
            string Narration = "";
            decimal vocId = 0, Transacted_Member_Id = 0;
            string Transacted_MemNo = "";
            string Transacted_MemName = "";
            DateTime Transacted_Date;
            bool IsChequeOnly = false;
            double vocAmt = 0, receiptAmount = 0, paymentAmount = 0, cashReceipt = 0, cashPayment = 0, adjReceipt = 0, adjPayment = 0;
            List<DtoTransaction> transactions = new();
            DtoTransactionRptPmtNos rptPmtNo = new();
            List<Fin_Voucher_Trn> finVoucherTrns = new();
            Fin_Voucher_Trn vocTrn = new();
            try
            {
                _unitOfWork.BeginTransaction();
                transactions = await _unitOfWork.TransactionsRepository.GetStagingDataByStagingId(stagingId);
                if (transactions == null || transactions.Count == 0)
                {
                    return voucherData!;
                }
                voucherData.brCode = transactions!.Select(x => x.BrCode).FirstOrDefault();
                
                foreach(var trn in transactions )
                {
                    DtoVoucherTrn vTrn = new()
                    {
                        Voc_Trn_Type = trn.Cash_Or_Adjustment,
                        Led_Id = trn.Ledger_Id,
                        Voc_Rpt = trn.Receipt_Amount,
                        Voc_Pmt = trn.Payment_Amount
                    };
                    dtoVoucherTrns.Add(vTrn);
                }
                voucherData.Transactions.Clear();
                voucherData.Transactions = dtoVoucherTrns;
                cashOrAdj = transactions.Select(x => x.Cash_Or_Adjustment).First();
                brCode = transactions.Select(x => x.BrCode!).First();
                string type = transactions.Select(x => x.Type!).First();
                int vocType = 0;
                switch (type)
                {
                    case "Member Transaction":
                        vocType = 5;
                        break;
                    case "Staff Transaction":
                        vocType = 50;
                        break;
                    case "Account Transaction":
                        vocType = 40;
                        break;
                }

                //Checked_By = transactions.Select(x => x.Checked_By).First();
                Transacted_Member_Id = transactions.Select(x => x.Transacted_Member_Id).First();
                var mem = await _unitOfWork.Members.GetMemberDetailsByMemIdAsync(Transacted_Member_Id);
                if (mem != null)
                {
                    Transacted_MemNo = mem.MemberNo!;
                    Transacted_MemName = mem.MemberName!;
                }


                Transacted_Date = transactions.Select(x => x.Transacted_Date).First();
                //receiptAmount = transactions.Select(x => x.Receipt_Amount).First();
                //paymentAmount = transactions.Select(x => x.Payment_Amount).First();

                cashReceipt = transactions.Select(x => x.CashReceipt_Amount).Sum();
                cashPayment = transactions.Select(x => x.CashPayment_Amount).Sum();
                adjPayment = transactions.Select(x => x.AdjustmentPayment_Amount).Sum();
                adjReceipt = transactions.Select(x => x.AdjustmentReceipt_Amount).Sum();

                receiptAmount = cashReceipt + adjReceipt;
                paymentAmount = cashPayment + adjPayment;

                //if (cashOrAdj == 1)
                //{
                //    cashReceipt = receiptAmount;
                //    cashPayment = paymentAmount;
                //}
                //else
                //{
                //    adjReceipt = receiptAmount;
                //    adjPayment = paymentAmount;
                //}


                if (receiptAmount >= paymentAmount) vocAmt = receiptAmount;
                if (paymentAmount >= receiptAmount) vocAmt = paymentAmount;
                IsChequeOnly = await _unitOfWork.TransactionsRepository.IsChequeOnly(stagingId);
                rptPmtNo = _unitOfWork.TransactionsRepository.GetReceiptAndPaymentNo(cashReceipt, cashPayment, adjReceipt, adjPayment, IsChequeOnly, yrId);
                if (rptPmtNo == null)
                {
                    //throw new Exception("Failed to generate receipt and payment numbers.");
                    return voucherData;
                }


                Fin_Voucher voc = Utility.GetModalObject.GetFinVoucherObject(vocId, "", 0, Transacted_Date, vocType, vocMode, vocAmt, true,
                                rptPmtNo.Voc_Rpt_SlNo, rptPmtNo.Voc_Rpt_No, rptPmtNo.Voc_Rpt_Mode, rptPmtNo.Voc_Pmt_SlNo, rptPmtNo.Voc_Pmt_No, rptPmtNo.Voc_Pmt_Mode,
                                 Checked_By, yrId, brCode, Transacted_Member_Id);
                (result, vocId) = await _unitOfWork.FinVoucher.AddFinVoucherAsync(voc);
                voucherData.Voc_Id = vocId;

                Map_General mapGeneral = await _unitOfWork.MapGeneral.GetMapGeneralAsync(brCode);

                /// cash transactions
                if (cashReceipt > 0)
                {
                    vocTrn = new();
                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, mapGeneral.Cash_Led_Id, cashReceipt, 0, 1, Transacted_MemNo.Trim() + Transacted_MemName.Trim(), false, Checked_By, yrId, "C", "", Transacted_Member_Id, brCode, 0, 0, 0);
                    finVoucherTrns.Add(vocTrn);
                }
                if (cashPayment > 0)
                {
                    vocTrn = new();
                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, mapGeneral.Cash_Led_Id, 0, cashPayment, 1, Transacted_MemNo.Trim() + Transacted_MemName.Trim(), false, Checked_By, yrId, "C", "", Transacted_Member_Id, brCode, 0, 0, 0);
                    finVoucherTrns.Add(vocTrn);
                }
                DtoOtherRelatedData otherRelatedData = new();
                // Batch-fetch all transaction statuses upfront
                var allAccIds = transactions.Select(t => t.Related_Account_Id).Distinct().ToList();
                var statusLookup = await _unitOfWork.TransactionsRepository.GetTransactionStatusesByAccIds(allAccIds);

                foreach (var trns in transactions)
                {
                    trns.Checked_By = Checked_By;
                    Status = statusLookup.TryGetValue(trns.Related_Account_Id, out var s) ? s : "";
                    switch (trns.Related_Account_Id)
                    {
                        case 1: /// Loan recovery
                            break;
                        case 2: /// RD Receipt
                            break;
                        case 3: /// Member Creditor
                            break;
                        case 4: /// Member Debtor
                            break;
                        case 5: /// Member Share capital
                            #region Member Share capital
                            Mem_Trn scTrn = new();
                            otherRelatedData = new();
                            otherRelatedData = Utility.JsonbObject.ConvertFromJsonForOtherRelatedData(trns.Related_Account_Data!);
                            Narration = otherRelatedData.Member_No!.Trim() + " " + otherRelatedData.Member_Name!.Trim();
                            scTrn = Utility.GetModalObject.GetMemTrnObject(3, trns.Account_Holder_Member_Id, trns.Ledger_Id, Transacted_Date, trns.Receipt_Amount, trns.Payment_Amount, false, false, vocId, Checked_By, yrId, 0, 0, null, 0, "", 0, 0, 0, brCode);
                            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, trns.Ledger_Id, trns.Receipt_Amount, trns.Payment_Amount, trns.Cash_Or_Adjustment, Narration, false, Checked_By, yrId, Status, "", trns.Transacted_Member_Id, brCode, 0, 0, 0);
                            finVoucherTrns.Add(vocTrn);
                            result = await _unitOfWork.MemTrn.AddMemTrnAsync(scTrn);
                            #endregion 
                            break;
                        case 6: /// Member advance deposit receipt
                            break;
                        case 7: /// FD Interest Payment
                            #region FD Interest Payment
                            DtoTermDepositFixedDepositPayment fdIntPayment = new();
                            fdIntPayment = Utility.JsonbObject.ConvertFromJsonForNewFixedDepositPayment(trns.Related_Account_Data!);
                            Narration = fdIntPayment.Member_No + " " + fdIntPayment.Member_Name;
                            List<TermDeposit_Trn> fdIntPaymentList = new();
                            foreach (var fd in fdIntPayment.Fixed_Deposit_Datas!)
                            {
                                TermDeposit_Trn tdTrn = new();
                                tdTrn = Utility.GetModalObject.GetTermDepositTrn(0, fdIntPayment.Transaction_Date, fd.FD_Id, 0, 0, 0, fd.Current_Interest_Calculated, fd.Current_Interest_Applied_Date, fd.Total_Interest_Payable, fd.Deposit_Refund, 0, null, 0, 0, 0, null, null, false, false, vocId, Checked_By, yrId, 0, 0, brCode);
                                fdIntPaymentList.Add(tdTrn);
                            }
                            vocTrn = new();
                            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, mapGeneral.FD_Int_Led_Id, 0, fdIntPayment.Total_Interest_Payable, fdIntPayment.CashOrAdjustment, Narration + " FD No :" + fdIntPayment.Fixed_Deposit_Datas.Select(x => x.FD_No).First(), false, Checked_By, yrId, Status, "TD No : " + fdIntPayment.Fixed_Deposit_Datas.Select(x => x.FD_No).First(), fdIntPayment.Mem_Id, brCode, 0, 0, 0);
                            finVoucherTrns.Add(vocTrn);
                            /// Save Fixed Depoist interst payment
                            result = await _unitOfWork.TermDepositTrn.AddTermDepositTrnListAsync(fdIntPaymentList);
                            //result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns);
                            #endregion
                            break;
                        case 8: /// FD Refund
                            #region FD Refund
                            List<Loan_Trn> fdRefundLoanList = new();
                            Loan_Trn fdRefundLoan = new();
                            //double totalPayable = 0;
                            double totalIntPayment = 0;
                            DtoTermDepositFixedDepositPayment fdRefund = new();
                            fdRefund = Utility.JsonbObject.ConvertFromJsonForNewFixedDepositPayment(trns.Related_Account_Data!);
                            Narration = fdRefund.Member_No + " " + fdRefund.Member_Name;
                            List<TermDeposit_Trn> fdTrnRefundList = new();
                            foreach (var fd in fdRefund.Fixed_Deposit_Datas!)
                            {
                                TermDeposit_Trn tdTrn = new();
                                tdTrn = Utility.GetModalObject.GetTermDepositTrn(0, fdRefund.Transaction_Date, fd.FD_Id, 0, 0, 0, fd.Current_Interest_Calculated, fd.Current_Interest_Applied_Date, fd.Total_Interest_Payable, fd.Deposit_Refund, 0, null, 0, 0, 0, null, null, false, false, vocId, Checked_By, yrId, 0, 0, brCode);
                                fdTrnRefundList.Add(tdTrn);
                            }
                            totalIntPayment = fdRefund.Total_Interest_Payable;

                            #region FD Payment
                            if (trns.Cash_Or_Adjustment == 1 && totalIntPayment < 0)
                            {
                                //if (totalIntPayment < 0)
                                //{
                                vocTrn = new();
                                vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, mapGeneral.FD_Led_Id, 0, fdRefund.Net_Payable, 1, Narration + " FD No : " + fdRefund.Fixed_Deposit_Datas.Select(x => x.FD_No).First(), false, Checked_By, yrId, Status, "TD No: " + fdRefund.Fixed_Deposit_Datas.Select(x => x.FD_No).First(), fdRefund.Mem_Id, brCode, 0, 0, 0);
                                finVoucherTrns.Add(vocTrn);
                                vocTrn = new();
                                vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, mapGeneral.FD_Led_Id, 0, fdRefund.Total_Deposit_Payable - fdRefund.Net_Payable, 2, Narration + " FD No : " + fdRefund.Fixed_Deposit_Datas.Select(x => x.FD_No).First(), false, Checked_By, yrId, Status, "FD No :" + fdRefund.Fixed_Deposit_Datas.Select(x => x.FD_No).First(), fdRefund.Mem_Id, brCode, 0, 0, 0);
                                finVoucherTrns.Add(vocTrn);
                                vocTrn = new();
                                vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, mapGeneral.FD_Excess_IntPaid_Id, Math.Abs(totalIntPayment), 0, 2, Narration + " FD No : " + fdRefund.Fixed_Deposit_Datas.Select(x => x.FD_No).First(), false, Checked_By, yrId, Status, fdRefund.Fixed_Deposit_Datas.Select(x => x.FD_No).First(), fdRefund.Mem_Id, brCode, 0, 0, 0);
                                finVoucherTrns.Add(vocTrn);
                                //}

                            }
                            else
                            {
                                if (fdRefund.Total_Deposit_Payable > 0)
                                {
                                    vocTrn = new();
                                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, mapGeneral.FD_Led_Id, 0, fdRefund.Total_Deposit_Payable, fdRefund.CashOrAdjustment, Narration + " FD No :" + fdRefund.Fixed_Deposit_Datas.Select(x => x.FD_No).First(), false, Checked_By, yrId, Status, "TD No: " + fdRefund.Fixed_Deposit_Datas.Select(x => x.FD_No).First(), fdRefund.Mem_Id, brCode, 0, 0, 0);
                                    finVoucherTrns.Add(vocTrn);
                                }
                                if (fdRefund.Total_Interest_Payable > 0)
                                {
                                    vocTrn = new();
                                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, mapGeneral.FD_Int_Led_Id, 0, fdRefund.Total_Interest_Payable, fdRefund.CashOrAdjustment, Narration + " FD No :" + fdRefund.Fixed_Deposit_Datas.Select(x => x.FD_No).First(), false, Checked_By, yrId, Status, "TD No: " + fdRefund.Fixed_Deposit_Datas.Select(x => x.FD_No).First(), fdRefund.Mem_Id, brCode, 0, 0, 0);
                                    finVoucherTrns.Add(vocTrn);
                                }
                            }

                            #endregion

                            #region Loan on FD Receipt
                            if (fdRefund.Total_Loan_Interest_Receipt + fdRefund.Total_Loan_Principal_Receipt > 0)
                            {
                                Loan_Schemes fdLoanScheme2 = await _unitOfWork.LoanScheme.GetLoanSchemeByType(3,brCode);
                                foreach (var loan in fdRefund.Loan_On_FixedDeposits!)
                                {
                                    fdRefundLoan = Utility.GetModalObject.GetLoanTrnObject(loan.Loan_Id, 0, "R", fdRefund.Transaction_Date, null, null, 0, 0, 0, 0, null, 0, null, loan.Current_Interest, loan.IntCalc_Date, 0, 0, loan.Interest_Balance, loan.Principal_Balance, 0, 0, 0, null, 0, null, 0, null, 0, 0, 0, 0, false, false, vocId, Checked_By, yrId, 0, false, 0, 0, 0, 0, 0, brCode);
                                    fdRefundLoanList.Add(fdRefundLoan);
                                }

                                if (fdRefund.Total_Loan_Interest_Receipt > 0)
                                {
                                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, fdLoanScheme2.IntLed_Id, fdRefund.Total_Loan_Interest_Receipt, 0, fdRefund.CashOrAdjustment, Narration + " Loan No : " + fdRefund.Loan_On_FixedDeposits.Select(x => x.Loan_No).First(), false, Checked_By, yrId, Status, "Loan No :" + fdRefund.Loan_On_FixedDeposits.Select(x => x.Loan_No).First(), fdRefund.Mem_Id, brCode, 0, 0, 0);
                                    finVoucherTrns.Add(vocTrn);
                                }
                                if (fdRefund.Total_Loan_Principal_Receipt > 0)
                                {
                                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, fdLoanScheme2.PrlLed_Id, fdRefund.Total_Loan_Principal_Receipt, 0, fdRefund.CashOrAdjustment, Narration + " Loan No : " + fdRefund.Loan_On_FixedDeposits.Select(x => x.Loan_No).First(), false, Checked_By, yrId, Status, "Loan No :" + fdRefund.Loan_On_FixedDeposits.Select(x => x.Loan_No).First(), fdRefund.Mem_Id, brCode, 0, 0, 0);
                                    finVoucherTrns.Add(vocTrn);
                                }
                            }
                            #endregion
                            /// Save Fixed Depoist interst payment
                            result = await _unitOfWork.TermDepositTrn.AddTermDepositTrnListAsync(fdTrnRefundList);
                            /// update fd refund as closed
                            foreach (var fd in fdRefund.Fixed_Deposit_Datas)
                            {
                                result = await _unitOfWork.TermDepositMaster.UpdateTermDepositMasterAsClosed(fd.FD_Id);
                            }
                            /// insert fd loan receipt
                            result = await _unitOfWork.LoanTrn.AddLoanTrnListAsync(fdRefundLoanList);
                            #endregion 
                            break;
                        case 9: /// FD Renewal
                            #region FD Renewal
                            string existFirstFDNoForRenewal = string.Empty;
                            decimal newFDIdForRenewal = 0;
                            string newFDNoForRenewal = "";
                            decimal renewalTDId = 0;
                            string? renewalTDNo = "";
                            //Map_General mapGeneral = new();
                            List<TermDeposit_Trn> fdRenewalTrnList = new();
                            TermDeposit_Trn fdRenewalTrn = new();
                            List<TermDeposit_Members> fdRenewalMembers = new();
                            TermDeposit_Members fdRenewalMem = new();
                            //List<Fin_Voucher_Trn> fdRenewalVocTrnList = new();
                            //Fin_Voucher_Trn fdRenewalVocTrn = new();
                            //mapGeneral = await _unitOfWork.MapGeneral.GetMapGeneralAsync(trns.BrCode!);
                            List<Loan_Trn> fdRenewalLoanTrnList = new();
                            Loan_Trn fdRenewalLoanTrn = new();

                            DtoTermDepositFixedDepositRenewal fdRenewal = new();
                            fdRenewal = Utility.JsonbObject.ConvertFromJsonForNewFixedDepositRenewal(trns.Related_Account_Data!);
                            Narration = fdRenewal.Member_No!.Trim() + " " + fdRenewal.Member_Name!.Trim();
                            renewalTDId = fdRenewal.FixedDepositPayment!.Fixed_Deposit_Datas!.Select(x => x.FD_Id).FirstOrDefault();
                            renewalTDNo = fdRenewal.FixedDepositPayment.Fixed_Deposit_Datas!.Select(x => x.FD_No).FirstOrDefault();
                            TermDeposit_Schemes tdScheme = await _unitOfWork.TermDepositScheme.GetTermDepositSchemeByIdAsync(fdRenewal.FixedDepositCreate!.Common.Tdscheme_Id);
                            if (tdScheme == null)
                            {
                                throw new Exception("Term Deposit Scheme not found for the given ID.");
                            }
                            #region FD New Account
                            TermDeposit_Master newFDForRenewal = new();
                            newFDForRenewal = Utility.GetModalObject.GetTermDepositMaster(0, "", fdRenewal.FixedDepositCreate!.Common.Tdscheme_Id, fdRenewal.FixedDepositCreate.Mem_Id, fdRenewal.FixedDepositCreate.Common.Tdh_Name!, fdRenewal.FixedDepositCreate.Members.Select(x => x.Age).First(),
                                fdRenewal.FixedDepositCreate.Common.Mode_Of_Operation, fdRenewal.FixedDepositCreate.Common.Account_Opendate, fdRenewal.FixedDepositCreate.Common.Value_Date,
                                fdRenewal.FixedDepositCreate.Deposit_Amount, fdRenewal.FixedDepositCreate.Period_In_Months, fdRenewal.FixedDepositCreate.Period_In_Days,
                                fdRenewal.FixedDepositCreate.Rate_Of_Interest, fdRenewal.FixedDepositCreate.Maturity_Date, fdRenewal.FixedDepositCreate.Maturity_Amount,
                                fdRenewal.FixedDepositCreate.IsNomineeProvided, fdRenewal.FixedDepositCreate.Interest_Payable_Frequency, 0, fdRenewal.FixedDepositCreate.Is_DiscountRate, fdRenewal.FixedDepositCreate.IsCompoundInterest,
                                fdRenewal.FixedDepositCreate.compoundfrequency, fdRenewal.FixedDepositCreate.Common.Nominee1Name!, fdRenewal.FixedDepositCreate.Common.Nominee1Age,
                                fdRenewal.FixedDepositCreate.Common.Nominee1Relationship!, fdRenewal.FixedDepositCreate.Common.Nominee2Name!, fdRenewal.FixedDepositCreate.Common.Nominee2Age,
                                fdRenewal.FixedDepositCreate.Common.Nominee2Relationship!, false, false, false, vocId, Checked_By, yrId, "N", renewalTDId, renewalTDNo!, brCode);

                            (result, newFDIdForRenewal, newFDNoForRenewal) = await _unitOfWork.TermDepositMaster.AddTermDepositMasterAsync(newFDForRenewal);
                            if (!result)
                            {
                                return voucherData;
                            }
                            fdRenewalTrn = Utility.GetModalObject.GetTermDepositTrn(0, fdRenewal.Transaction_Date, newFDIdForRenewal, 0,
                                fdRenewal.FixedDepositCreate.Deposit_Amount, fdRenewal.FixedDepositCreate.Maturity_Amount, 0, null, 0, 0, 0, null, 0, 0, 0, null, null, false, false,
                                vocId, Checked_By, yrId, 1, 0, brCode);
                            fdRenewalTrnList.Add(fdRenewalTrn);
                            int memSlNo = 1;
                            foreach (var memRem in fdRenewal.FixedDepositCreate.Members)
                            {
                                fdRenewalMem = Utility.GetModalObject.GetTermDepositMembers(0, newFDIdForRenewal, memRem.Mem_Id, memSlNo, false, vocId, Checked_By, yrId, brCode);
                                fdRenewalMembers.Add(fdRenewalMem);
                                memSlNo++;
                            }

                            #endregion

                            #region FD Renewal Payment
                            /// FD payment for renewal
                            foreach (var fd in fdRenewal.FixedDepositPayment.Fixed_Deposit_Datas!)
                            {
                                existFirstFDNoForRenewal += fd.FD_No + " ";
                                fdRenewalTrn = Utility.GetModalObject.GetTermDepositTrn(0, fdRenewal.Transaction_Date, fd.FD_Id, 0, 0, 0, fd.Current_Interest_Calculated, fd.Current_Interest_Applied_Date,
                                    fd.Total_Interest_Payable, fd.Deposit_Refund, 0, null, 0, 0, 0, null, null, false, false,
                                    vocId, Checked_By, yrId, 0, 0, brCode);
                                fdRenewalTrnList.Add(fdRenewalTrn);
                                //vocTrn = new();
                                //vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, tdScheme.Led_Id, 0, fd.Deposit_Refund, fdRenewal.CashOrAdjustment,
                                //    Narration + " FD No : " + fd.FD_No, false, Checked_By, yrId, Status, "FD No: " + fd.FD_No, fdRenewal.Mem_Id, brCode, newFDIdForRenewal, 0, 0);
                            }
                            #endregion

                            #region FD Loan Receipt if any
                            foreach (var loan in fdRenewal.FixedDepositPayment.Loan_On_FixedDeposits!)
                            {
                                fdRenewalLoanTrn = Utility.GetModalObject.GetLoanTrnObject(loan.Loan_Id, 0, "R", fdRenewal.Transaction_Date, null, null, 0, 0, 0, 0, null, 0, null, loan.Current_Interest, loan.Current_IntCalc_Date, 0, 0, loan.Interest_Balance, loan.Principal_Balance, 0, 0, 0, null, 0, null, 0, null, 0, 0, 0, 0, false, false, vocId, Checked_By, yrId, 0, false, 0, 0, 0, 0, 0, brCode);
                                fdRenewalLoanTrnList.Add(fdRenewalLoanTrn);
                            }
                            #endregion

                            #region fd renewal fin_voucher_trn
                            double existingFDAmt = fdRenewal.FixedDepositPayment.Total_Deposit_Payable;
                            double intPayable = fdRenewal.FixedDepositPayment.Total_Interest_Payable;
                            if (fdRenewal.CashOrAdjustment == 1) /// cash transactions
                            {
                                if (cashPayment > 0)
                                {
                                    if (existingFDAmt > cashPayment)
                                    {
                                        vocTrn = new();
                                        vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, tdScheme.Led_Id, 0, cashPayment, 1,
                                        Narration + " FD No " + existFirstFDNoForRenewal, false, Checked_By, yrId, Status, "FD No: " + existFirstFDNoForRenewal, 0, brCode, 0, 0, 0);
                                        finVoucherTrns.Add(vocTrn);
                                        vocTrn = new();
                                        vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, tdScheme.Led_Id, 0, existingFDAmt - cashPayment, 2,
                                        Narration + " FD No " + existFirstFDNoForRenewal, false, Checked_By, yrId, Status, "FD No: " + existFirstFDNoForRenewal, 0, brCode, 0, 0, 0);
                                        finVoucherTrns.Add(vocTrn);
                                    }
                                }
                                else
                                {
                                    vocTrn = new();
                                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, tdScheme.Led_Id, 0, existingFDAmt, 2,
                                    Narration + " FD No " + existFirstFDNoForRenewal, false, Checked_By, yrId, Status, "FD No: " + existFirstFDNoForRenewal, 0, brCode, 0, 0, 0);
                                    finVoucherTrns.Add(vocTrn);
                                }
                                if (intPayable > 0)
                                {
                                    vocTrn = new();
                                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, tdScheme.IntLed_Id, 0, intPayable, 2,
                                    Narration + " " + existFirstFDNoForRenewal, false, Checked_By, yrId, Status, "FD No: " + existFirstFDNoForRenewal, 0, brCode, 0, 0, 0);
                                    finVoucherTrns.Add(vocTrn);
                                }
                                if (intPayable < 0)
                                {
                                    vocTrn = new();
                                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, mapGeneral.FD_Excess_IntPaid_Id, Math.Abs(intPayable), 0, 2,
                                    Narration + " " + existFirstFDNoForRenewal, false, Checked_By, yrId, Status, "FD No: " + existFirstFDNoForRenewal, 0, brCode, 0, 0, 0);
                                    finVoucherTrns.Add(vocTrn);
                                }
                            }
                            if (fdRenewal.CashOrAdjustment != 1) /// adjustment transactions
                            {
                                vocTrn = new();
                                vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, tdScheme.Led_Id, 0, fdRenewal.FixedDepositPayment.Total_Deposit_Payable, 2,
                                    Narration + " " + existFirstFDNoForRenewal, false, Checked_By, yrId, Status, "FD No: " + existFirstFDNoForRenewal, 0, brCode, 0, 0, 0);
                                finVoucherTrns.Add(vocTrn);
                                if (intPayable > 0)
                                {
                                    vocTrn = new();
                                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, tdScheme.IntLed_Id, 0, intPayable, 2,
                                    Narration + " " + existFirstFDNoForRenewal, false, Checked_By, yrId, Status, "FD No: " + existFirstFDNoForRenewal, 0, brCode, 0, 0, 0);
                                    finVoucherTrns.Add(vocTrn);
                                }
                                if (intPayable < 0)
                                {
                                    vocTrn = new();
                                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, mapGeneral.FD_Excess_IntPaid_Id, Math.Abs(intPayable), 0, 2,
                                    Narration + " " + existFirstFDNoForRenewal, false, Checked_By, yrId, Status, "FD No: " + existFirstFDNoForRenewal, 0, brCode, 0, 0, 0);
                                    finVoucherTrns.Add(vocTrn);
                                }
                            }
                            vocTrn = new();
                            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, tdScheme.Led_Id, fdRenewal.FixedDepositCreate.Deposit_Amount, 0, 2,
                                Narration + " FD No " + newFDNoForRenewal, false, Checked_By, yrId, Status, "FD No: " + newFDNoForRenewal, 0, brCode, 0, 0, 0);
                            finVoucherTrns.Add(vocTrn);
                            #endregion 

                            #region Loan on FD Receipt
                            if (fdRenewal.FixedDepositPayment.Total_Loan_Principal_Receipt + fdRenewal.FixedDepositPayment.Total_Loan_Interest_Receipt  > 0)
                            {
                                Loan_Schemes fdLoanScheme2 = await _unitOfWork.LoanScheme.GetLoanSchemeByType(3, brCode);
                                
                                if (fdRenewal.FixedDepositPayment.Total_Loan_Interest_Receipt > 0)
                                {
                                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, fdLoanScheme2.IntLed_Id, fdRenewal.FixedDepositPayment.Total_Loan_Interest_Receipt, 0, 2, Narration + " Loan No : " + fdRenewal.FixedDepositPayment.Loan_On_FixedDeposits.Select(x => x.Loan_No).First(), false, Checked_By, yrId, Status, "Loan No :" + fdRenewal.FixedDepositPayment.Loan_On_FixedDeposits.Select(x => x.Loan_No).First(), fdRenewal.Mem_Id, brCode, 0, 0, 0);
                                    finVoucherTrns.Add(vocTrn);
                                }
                                if (fdRenewal.FixedDepositPayment.Total_Loan_Principal_Receipt > 0)
                                {
                                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, fdLoanScheme2.PrlLed_Id, fdRenewal.FixedDepositPayment.Total_Loan_Principal_Receipt, 0, 2, Narration + " Loan No : " + fdRenewal.FixedDepositPayment.Loan_On_FixedDeposits.Select(x => x.Loan_No).First(), false, Checked_By, yrId, Status, "Loan No :" + fdRenewal.FixedDepositPayment.Loan_On_FixedDeposits.Select(x => x.Loan_No).First(), fdRenewal.Mem_Id, brCode, 0, 0, 0);
                                    finVoucherTrns.Add(vocTrn);
                                }
                            }
                            #endregion
                            #region Save FD Renewal
                            result = await _unitOfWork.TermDepositMember.AddTermDepositMemberListAsync(fdRenewalMembers);
                            result = await _unitOfWork.TermDepositTrn.AddTermDepositTrnListAsync(fdRenewalTrnList);
                            //result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns);
                            result = await _unitOfWork.LoanTrn.AddLoanTrnListAsync(fdRenewalLoanTrnList);
                            result = await _unitOfWork.TermDepositMaster.UpdateTermDepositMasterAsClosed(renewalTDId);

                            #endregion 

                            #endregion
                            break;
                        case 10:    /// RD Refund
                            break;
                        case 11:    /// Bank
                            #region Cheque data
                            Fin_Voucher_Bank finVocBank = new();
                            DtoOtherRelatedData chequeData = new();
                            chequeData = Utility.JsonbObject.ConvertFromJsonForOtherRelatedData(trns.Related_Account_Data!);
                            Narration = chequeData.Member_No!.Trim() + " " + chequeData.Member_Name!.Trim() + " Cheque No " + chequeData.Cheque_No;
                            finVocBank = Utility.GetModalObject.GetFinVocBankObject(trns.Transacted_Date, chequeData.Member_Id, trns.Receipt_Amount > 0 ? "C" : "O", chequeData.Issue_Bank_Name!, vocId, trns.Ledger_Id, trns.Receipt_Amount > 0 ? trns.Receipt_Amount : trns.Payment_Amount, chequeData.Cheque_No!, chequeData.Cheque_Date!, null, 0, false, null, "", null, 0, Checked_By, yrId, false, brCode);
                            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, trns.Ledger_Id, trns.Receipt_Amount, trns.Payment_Amount, trns.Cash_Or_Adjustment, Narration, false, Checked_By, yrId, Status, "", trns.Transacted_Member_Id, brCode, 0, 0, 0);
                            finVoucherTrns.Add(vocTrn);
                            result = await _unitOfWork.FinVoucherBank.AddFinVoucherBankAsync(finVocBank);
                            //result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns);
                            #endregion 
                            break;
                        case 12:    /// Loan account create HSIS
                            break;
                        case 13:    /// FD Loan Disbursement
                            #region FD Loan Disbursement
                            decimal fdLoanId = 0;
                            string fdLoanNo = ""; ///,firstFDNo = "";
                            double faceValue = 0, drawingPower = 0;
                            DtoTermDepositFixedDepositLoan fdLoan = new();
                            fdLoan = Utility.JsonbObject.ConvertFromJsonForFixedDepositLoanDisbursement(trns.Related_Account_Data!);
                            Narration = fdLoan.Member_No + " " + fdLoan.Member_Name;
                            Loan_Master fdLoanMaster = new();
                            Loan_Trn fdLoanTrn = new();
                            Loan_Disb fdLoanDisb = new();
                            Loan_Roi fdLoanRoi = new();
                            Lien lien = new();
                            List<Lien_Trn> lienTrns = new();
                            Lien_Trn lienTrn = new();
                            Loan_Schemes fdLoanScheme = await _unitOfWork.LoanScheme.GetLoanSchemeByType(3, brCode);
                            faceValue = fdLoan.FD_Datas!.Select(x => x.FD_Amount).Sum();
                            drawingPower = fdLoan.FD_Datas!.Select(x => x.Drawing_Power).Sum();
                            //firstFDNo = fdLoan.FD_Datas!.Select(x => x.FD_No).FirstOrDefault() ?? "";
                            fdLoanMaster = Utility.GetModalObject.GetLoanMasterObject(fdLoanScheme.Scheme_Id, "", fdLoan.Mem_Id, 0, "", null, "", null, fdLoan.Loan_Amount, fdLoan.Transaction_Date, 3, 0, 0, 0, 0, 0, 0, fdLoan.Transaction_Date, fdLoan.Transaction_Date, 0, "", "", "", null, null, 0, fdLoan.Loan_Rate_Of_Interest, 0, fdLoan.Transaction_Date, 0, false, false, false, vocId, Checked_By, yrId, faceValue, drawingPower, brCode, 0, 0, 1);
                            (result, fdLoanId, fdLoanNo) = await _unitOfWork.LoanMaster.AddLoanMasterAsync(fdLoanMaster);

                            fdLoanTrn = Utility.GetModalObject.GetLoanTrnObject(fdLoanId, 0, "I", fdLoan.Transaction_Date, null, null,
                                fdLoan.Loan_Amount, 0, 0, 0, null, 0, null, 0, null, 0, 0, 0, 0, 0, 0, 0, null, 0, null, 0, null,
                                fdLoan.Loan_Rate_Of_Interest, 0, 0, 0, false, false, vocId, Checked_By, yrId, 1, false, fdLoan.Loan_Amount, 0, 0, 0, 0, brCode);
                            fdLoanDisb = Utility.GetModalObject.GetLoanDisbursementObject(fdLoanId, fdLoan.Transaction_Date, 1, fdLoan.Loan_Amount, fdLoan.Transaction_Date, null, "", null, 0, null, "", null, false, 1, true, false, vocId, Checked_By, yrId, brCode);
                            fdLoanRoi = Utility.GetModalObject.GetLoanROIObject(fdLoanId, "S", fdLoan.Transaction_Date, fdLoan.Loan_Rate_Of_Interest, 0, 0, 0, false, false, vocId, Checked_By, yrId, brCode);
                            lien = Utility.GetModalObject.GetLienObject(fdLoanId, faceValue, fdLoan.Loan_Amount, 0, fdLoan.FD_Datas!.Select(x => x.FD_No).FirstOrDefault() ?? "", fdLoan.FD_Datas!.Select(x => x.FD_Id).FirstOrDefault(), false, false, vocId, Checked_By, yrId, brCode);
                            foreach (var fd in fdLoan.FD_Datas!)
                            {
                                lienTrn = new();
                                lienTrn = Utility.GetModalObject.GetLienTrObject(0, fdLoanId, fd.FD_Id, fd.FD_Amount, fd.Drawing_Power, fd.Loan_Amount, 0, false, false, vocId, Checked_By, yrId, brCode);
                                lienTrns.Add(lienTrn);
                            }
                            vocTrn = new();
                            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, fdLoanScheme.PrlLed_Id, 0, fdLoan.Loan_Amount, fdLoan.CashOrAdjustment,
                                Narration, false, Checked_By, yrId, Status, "Loan No: " + fdLoanNo, fdLoan.Mem_Id, brCode, fdLoanId, fdLoan.Loan_Amount, 0);
                            finVoucherTrns.Add(vocTrn);
                            result = await _unitOfWork.LoanTrn.AddLoanTrn(fdLoanTrn);
                            result = await _unitOfWork.LoanDisbursement.AddLoanDisbursementAsync(fdLoanDisb);
                            result = await _unitOfWork.LoanROI.AddLoanROIAsync(fdLoanRoi);
                            result = await _unitOfWork.Lien.AddLienAsync(lien);
                            result = await _unitOfWork.LienTrn.AddLienTrnListAsync(lienTrns);
                            //result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns);
                            #endregion
                            break;
                        case 14: /// RD Loan Disbursement
                            break;
                        case 15:    /// Loan to staff
                            #region Staff Loan disbursement
                            DtoLoanDisbursementStaff staffLoan = new();
                            staffLoan = Utility.JsonbObject.ConvertFromJsonForStaffLoan(trns.Related_Account_Data!);
                            decimal staffLoanId = 0;
                            string staffLoanNo = "";

                            Loan_Master staffLoanMaster = new();
                            Loan_Trn staffLoanTrn = new();
                            Loan_Disb staffLoanDisb = new();
                            Loan_Inst staffLoanInst = new();
                            Loan_Roi staffLoanRoi = new();
                            Loan_Schemes staffLoanScheme = await _unitOfWork.LoanScheme.GetLoanSchemesAsync(staffLoan.Scheme_Id,brCode);
                            staffLoanMaster = Utility.GetModalObject.GetLoanMasterObject(staffLoan.Scheme_Id, "", staffLoan.Employee_Id, 0, "", null, "", null, staffLoan.DisbursementAmount, (DateTime)staffLoan.Transaction_Date!, 5, 0, 0, 0, 0, staffLoan.Principal_Period, staffLoan.Interest_Period, staffLoan.InstalmentStart_Date, staffLoan.InstalmentStart_Date, 0, "", "", "", null, null, 0, staffLoan.Rate_Of_Interest, 0, (DateTime)staffLoan.Transaction_Date!, 0, false, false, false, vocId, Checked_By, yrId, 0, 0, brCode, 0, 0, 1);
                            (result, staffLoanId, staffLoanNo) = await _unitOfWork.LoanMaster.AddLoanMasterAsync(staffLoanMaster);
                            Narration = staffLoan.Employee_Name! + " Loan No " + staffLoanNo;
                            staffLoanTrn = Utility.GetModalObject.GetLoanTrnObject(staffLoanId, 0, "I", (DateTime)staffLoan.Transaction_Date, null, staffLoan.Transaction_Date,
                                staffLoan.DisbursementAmount, 0, 0, 0, null, 0, null, 0, null, 0, 0, 0, 0, 0, 0, 0, null, 0, null, 0, null,
                                staffLoan.Rate_Of_Interest, 0, 0, 0, false, false, vocId, Checked_By, staffLoan.YrId, 1, false, staffLoan.DisbursementAmount, 0, 0, 0, 0, brCode);
                            staffLoanDisb = Utility.GetModalObject.GetLoanDisbursementObject(staffLoanId, (DateTime)staffLoan.Transaction_Date, 1, staffLoan.DisbursementAmount, (DateTime)staffLoan.Transaction_Date!, null, "", null, 0, null, "", null, false, 1, true, false, vocId, Checked_By, staffLoan.YrId, brCode);
                            staffLoanInst = Utility.GetModalObject.GetLoanInstalmentObject(staffLoanId, (DateTime)staffLoan.Transaction_Date, staffLoan.InstalmentAmount, false, false, vocId, Checked_By, staffLoan.YrId, staffLoan.BrCode!);
                            staffLoanRoi = Utility.GetModalObject.GetLoanROIObject(staffLoanId, "S", (DateTime)staffLoan.Transaction_Date, staffLoan.Rate_Of_Interest, 0, 0, 0, false, false, vocId, Checked_By, staffLoan.YrId, brCode);

                            vocTrn = new();
                            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, staffLoanScheme.PrlLed_Id, 0, staffLoan.DisbursementAmount, staffLoan.CashOrAdjustment,
                                Narration, false, Checked_By, staffLoan.YrId, Status, "Loan No: " + staffLoanNo, staffLoan.Employee_Id, staffLoan.BrCode!, staffLoanId, staffLoan.DisbursementAmount, 0);
                            finVoucherTrns.Add(vocTrn);

                            result = await _unitOfWork.LoanTrn.AddLoanTrn(staffLoanTrn);
                            result = await _unitOfWork.LoanDisbursement.AddLoanDisbursementAsync(staffLoanDisb);
                            result = await _unitOfWork.LoanInstalment.AddLoanInstalmentAsync(staffLoanInst);
                            result = await _unitOfWork.LoanROI.AddLoanROIAsync(staffLoanRoi);
                            #endregion
                            break;
                        case 16:    /// FD New Account
                            #region FD New Account
                            decimal newTDId = 0;
                            string newTDNo = "";
                            DtoTermDepositFixedDepositCreation newFD = new();
                            newFD = Utility.JsonbObject.ConvertFromJsonForNewFixedDeposit(trns.Related_Account_Data!);
                            Narration = newFD.Member_No!.Trim() + " " + newFD.Member_Name!.Trim();
                            TermDeposit_Master tdMasterNew = new();
                            TermDeposit_Trn tdTrnNew = new();
                            List<TermDeposit_Members> tdMembers = new();
                            tdMasterNew = Utility.GetModalObject.GetTermDepositMaster(0, "", newFD.Common.Tdscheme_Id, newFD.Mem_Id, newFD.Common.Tdh_Name!, newFD.Members.Select(x => x.Age).First(), newFD.Common.Mode_Of_Operation,
                                newFD.Common.Account_Opendate, newFD.Common.Value_Date, newFD.Deposit_Amount, newFD.Period_In_Months, newFD.Period_In_Days, newFD.Rate_Of_Interest,
                                newFD.Maturity_Date, newFD.Maturity_Amount, true, newFD.Interest_Payable_Frequency, 0, newFD.Is_DiscountRate, (newFD.compoundfrequency > 0 ? true : false),
                                newFD.compoundfrequency, newFD.Common.Nominee1Name!, newFD.Common.Nominee1Age, newFD.Common.Nominee1Relationship!,
                                newFD.Common.Nominee2Name!, newFD.Common.Nominee2Age, newFD.Common.Nominee2Relationship!, false, false, false, vocId, Checked_By,
                                yrId, "N", 0, "", brCode);
                            (result, newTDId, newTDNo) = await _unitOfWork.TermDepositMaster.AddTermDepositMasterAsync(tdMasterNew);
                            if (!result)
                            {
                                return voucherData;
                            }
                            tdTrnNew = Utility.GetModalObject.GetTermDepositTrn(0, newFD.Common.Account_Opendate, newTDId, 0,
                                newFD.Deposit_Amount, newFD.Maturity_Amount, 0, null, 0, 0, 0, null, 0, 0, 0, null, null, false, false,
                                vocId, Checked_By, yrId, 1, 0, brCode);

                            foreach (var memNew in newFD.Members)
                            {
                                TermDeposit_Members tdMem = new();
                                tdMem = Utility.GetModalObject.GetTermDepositMembers(0, newTDId, memNew.Mem_Id, 1, false, vocId, Checked_By, yrId, brCode);
                                tdMembers.Add(tdMem);
                            }
                            TermDeposit_Schemes tdNewScheme = await _unitOfWork.TermDepositScheme.GetTermDepositSchemeByIdAsync(newFD.Common.Tdscheme_Id);
                            vocTrn = new();
                            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, tdNewScheme.Led_Id, newFD.Deposit_Amount, 0, newFD.CashorAdjustment,
                                Narration + "FD No : " + newTDNo, false, Checked_By, yrId, Status, "TD No: " + newTDNo, newFD.Mem_Id, brCode, 0, 0, 0);
                            finVoucherTrns.Add(vocTrn);

                            /// Save new fixed deposit
                            result = await _unitOfWork.TermDepositTrn.AddTermDepositTrnAsync(tdTrnNew);
                            result = await _unitOfWork.TermDepositMember.AddTermDepositMemberListAsync(tdMembers);
                            //result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns);

                            #endregion
                            break;
                        case 17:    /// RD New Accounttrns
                            break;
                        case 18:    /// SB Account Creation
                            #region SB Account Creation
                            decimal accId = 0;
                            string accNo = "";
                            SBCA_Schemes sbCAScheme = new();
                            SBCA_Master sbCAMaster = new();
                            Mem_Trn sbMemTrn = new();
                            DtoSBAccountCreate sbAcc = new();
                            sbAcc = Utility.JsonbObject.ConvertFromJsonForNewSBAccount(trns.Related_Account_Data!);
                            Narration = sbAcc.Member_No + " " + sbAcc.Member_Name;
                            sbCAScheme = await _unitOfWork.SBCASchemes.GetSBCAScheme(brCode);
                            sbCAMaster = Utility.GetModalObject.GetSBCAMasterObject(0, sbCAScheme.Scheme_Id, sbAcc.Mem_Id, "", 1, false, false, vocId, Checked_By, yrId, brCode);

                            (result, accId, accNo) = await _unitOfWork.SBCAMaster.AddSBCAMasterAsync(sbCAMaster);
                            sbMemTrn = Utility.GetModalObject.GetMemTrnObject(7, sbAcc.Mem_Id, sbCAScheme.SBCA_Led_Id, sbAcc.Transaction_Date, sbAcc.Receipt_Amount, 0, false, false, vocId, Checked_By, yrId, 1, 0, null, 0, "", 0, accId, 0, brCode);
                            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, sbCAScheme.SBCA_Led_Id, sbAcc.Receipt_Amount, 0, sbAcc.CashOrAdjustment,
                                Narration + " SB AC No : " + accNo, false, Checked_By, yrId, Status, "SB AC No: " + accNo, sbAcc.Mem_Id, brCode, accId, 0, 0);
                            finVoucherTrns.Add(vocTrn);
                            result = await _unitOfWork.MemTrn.AddMemTrnAsync(sbMemTrn);
                            //result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns);
                            #endregion
                            break;
                        case 19:    /// Staff Loan Receipt
                            #region staff loan recovery
                            List<Loan_Trn> staffLoanTrnList = new();
                            DtoLoanRecoveryStaff staffLoanRecovery = new();
                            staffLoanRecovery = Utility.JsonbObject.ConvertFromJsonForStaffLoanRecovery(trns.Related_Account_Data!);
                            staffLoanTrnList = Utility.JsonbObject.GetStaffLoanRecovery(staffLoanRecovery, Transacted_Date, Checked_By, vocId, yrId, brCode);
                            List<Fin_Voucher_Trn> staffLoanVocList = new();
                            staffLoanVocList = Utility.JsonbObject.GetVoucherTrnForStaffLoanRecovery(staffLoanRecovery, vocId, staffLoanRecovery.CashOrAdjustment, staffLoanRecovery.Mem_Id, staffLoanRecovery.Member_No!, staffLoanRecovery.Member_Name!, Checked_By, yrId, Status, brCode);
                            finVoucherTrns.AddRange(staffLoanVocList);
                            result = await _unitOfWork.LoanTrn.AddLoanTrnListAsync(staffLoanTrnList);
                            #endregion
                            break;
                        case 20:    /// PF Subscription
                            #region PF subscription
                            DtoPayPFData pfContribution = new();
                            pfContribution = Utility.JsonbObject.ConvertFromJsonForPF(trns.Related_Account_Data!);
                            Narration = pfContribution.Employee_Name! + " EPF conbtribution";
                            Emp_Pf empPF = new();
                            empPF = Utility.GetModalObject.GetEmpPFObject(0, pfContribution.Employee_Id, pfContribution.Transaction_Date, pfContribution.PFReceived, pfContribution.VPFReceived, pfContribution.EmployersPFReceived, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, null, false, false, null, vocId, Checked_By, pfContribution.YrId, 0, "AR", pfContribution.BrCode!);
                            Pay_Template template = new();
                            template = await _unitOfWork.PayTemplate.GetPayTemplateAsync(pfContribution.BrCode!);

                            if (pfContribution.PFReceived > 0 || pfContribution.VPFReceived > 0)
                            {
                                vocTrn = new();
                                vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, template.Emp_PF_Led_Id, pfContribution.PFReceived + pfContribution.VPFReceived, 0, trns.Cash_Or_Adjustment, Narration, false, Checked_By, pfContribution.YrId, Status, "", pfContribution.Employee_Id, pfContribution.BrCode!, 0, 0, 0);
                                finVoucherTrns.Add(vocTrn);
                            }
                            result = await _unitOfWork.EmployeePF.AddEmployeePFAsync(empPF);
                            #endregion 
                            break;
                        case 21:    /// Ledger Entry
                            vocTrn = new();
                            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, trns.Ledger_Id, trns.Receipt_Amount, trns.Payment_Amount, trns.Cash_Or_Adjustment, Transacted_MemNo.Trim() + Transacted_MemName.Trim(), false, Checked_By, yrId, Status, "", Transacted_Member_Id, brCode, 0, 0, 0);
                            finVoucherTrns.Add(vocTrn);
                            break;
                        case 22:    /// Staff Suspense Creditor
                            #region staff suspense creditor
                            Mem_Trn staffCrMemTrn = new();
                            otherRelatedData = new();
                            otherRelatedData = Utility.JsonbObject.ConvertFromJsonForOtherRelatedData(trns.Related_Account_Data!);
                            Narration = otherRelatedData.Member_No!.Trim() + " " + otherRelatedData.Member_Name!.Trim();
                            staffCrMemTrn = Utility.GetModalObject.GetMemTrnObject(6, trns.Account_Holder_Member_Id, trns.Ledger_Id, Transacted_Date, trns.Receipt_Amount, trns.Payment_Amount, false, false, vocId, Checked_By, yrId, 0, 0, null, 0, "", 0, 0, 0, brCode);
                            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, trns.Ledger_Id, trns.Receipt_Amount, trns.Payment_Amount, trns.Cash_Or_Adjustment, Narration, false, Checked_By, yrId, Status, "", trns.Transacted_Member_Id, brCode, 0, 0, 0);
                            finVoucherTrns.Add(vocTrn);
                            result = await _unitOfWork.MemTrn.AddMemTrnAsync(staffCrMemTrn);
                            #endregion 
                            break;
                        case 23:    /// Staff Suspense Debtor
                            #region staff suspense debtor
                            Mem_Trn staffDrMemTrn = new();
                            otherRelatedData = new();
                            otherRelatedData = Utility.JsonbObject.ConvertFromJsonForOtherRelatedData(trns.Related_Account_Data!);
                            Narration = otherRelatedData.Member_No!.Trim() + " " + otherRelatedData.Member_Name!.Trim();
                            staffDrMemTrn = Utility.GetModalObject.GetMemTrnObject(5, trns.Account_Holder_Member_Id, trns.Ledger_Id, Transacted_Date, trns.Receipt_Amount, trns.Payment_Amount, false, false, vocId, Checked_By, yrId, 0, 0, null, 0, "", 0, 0, 0, brCode);
                            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, trns.Ledger_Id, trns.Receipt_Amount, trns.Payment_Amount, trns.Cash_Or_Adjustment, Narration, false, Checked_By, yrId, Status, "", trns.Transacted_Member_Id, brCode, 0, 0, 0);
                            finVoucherTrns.Add(vocTrn);
                            result = await _unitOfWork.MemTrn.AddMemTrnAsync(staffDrMemTrn);
                            #endregion 
                            break;
                        case 24:    /// FD Loan Receipt
                            #region FD Loan Receipt
                            List<Loan_Trn> fdLoanRecTrnList = new();
                            Loan_Trn fdLoanRecTrn = new();
                            DtoTermDepositLoanRecovery fdLoanRec = new();
                            fdLoanRec = Utility.JsonbObject.ConvertFromJsonForFixedDepositLoanRecovery(trns.Related_Account_Data!);
                            Narration = fdLoanRec.Member_No + " " + fdLoanRec.Member_Name;
                            foreach (var loan in fdLoanRec.Loan_Balance_List!)
                            {
                                fdLoanRecTrn = Utility.GetModalObject.GetLoanTrnObject(loan.Loan_Id, 0, "R", fdLoanRec.Transaction_Date, null, null, 0, 0, 0, 0, null, 0, null, loan.Current_Interest,
                                    loan.Interest_Applied_Date, 0, 0, loan.Interest_Collection, loan.Principal_Collection, 0, 0, 0, null, 0, null, 0, null,
                                    loan.Rate_Of_Interest, 0, 0, 0, false, false, vocId, Checked_By, yrId, 0, false, loan.Principal_Balance - loan.Principal_Collection, 0, 0, 0, 0, brCode);
                                fdLoanRecTrnList.Add(fdLoanRecTrn);

                                if (loan.Interest_Collection > 0)
                                {
                                    vocTrn = new();
                                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, loan.IntLed_Id, loan.Interest_Collection, 0, fdLoanRec.CashOrAdjustment,
                                        Narration + " Loan No : " + loan.Loan_No, false, Checked_By, yrId, Status, "Loan No: " + loan.Loan_No, fdLoanRec.Mem_Id, brCode, loan.Loan_Id, 0, 0);
                                    finVoucherTrns.Add(vocTrn);
                                }
                                if (loan.Principal_Collection > 0)
                                {
                                    vocTrn = new();
                                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, loan.PrlLed_Id, loan.Principal_Collection, 0, fdLoanRec.CashOrAdjustment,
                                        Narration + " Loan No : " + loan.Loan_No, false, Checked_By, yrId, Status, "Loan No: " + loan.Loan_No, fdLoanRec.Mem_Id, brCode, loan.Loan_Id, loan.Principal_Balance - loan.Principal_Collection, 0);
                                    finVoucherTrns.Add(vocTrn);
                                }
                            }
                            result = await _unitOfWork.LoanTrn.AddLoanTrnListAsync(fdLoanRecTrnList);
                            //result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns);
                            #endregion
                            break;
                        case 25:    /// RD Loan Receipt
                            break;
                        case 26:    /// Member Pending Deposit Receipt
                            break;
                        case 27:    /// Member Deposit Payment
                            break;
                        case 29:    /// 28 fees- 29-> Divided Payment
                            break;
                        case 30:    /// Int on TD Payment
                            break;
                        case 31:    /// Jewel Loan receipt
                            #region jewel loan recovery
                            List<Loan_Trn> jlTrnList = new();
                            DtoJewelLoanRecovery jewelLoanRecovery = new();
                            jewelLoanRecovery = Utility.JsonbObject.ConvertFromJson(trns.Related_Account_Data!);
                            jlTrnList = Utility.JsonbObject.GetJewelLoanRecovery(jewelLoanRecovery.JewelLoanBalance_List!, Transacted_Date, Checked_By, vocId, yrId, brCode);
                            List<Fin_Voucher_Trn> jlVocList = new();
                            jlVocList = Utility.JsonbObject.GetVoucherTrnForJewelLoanRecovery(jewelLoanRecovery.JewelLoanBalance_List!, vocId, jewelLoanRecovery.CashOrAdjustment, jewelLoanRecovery.Mem_Id, jewelLoanRecovery.Member_No!, jewelLoanRecovery.Member_Name!, Checked_By, yrId, Status, brCode);
                            finVoucherTrns.AddRange(jlVocList);
                            result = await _unitOfWork.LoanTrn.AddLoanTrnListAsync(jlTrnList);
                            #endregion
                            break;
                        case 32:    /// Dividend/TD Interest payment (bulk)
                            break;
                        case 33:    /// Ex-Gratia payment
                            #region Ex-gratia
                            decimal payIdExgratia = 0;
                            DtoPayExgratiaBonus exgratia = new();
                            exgratia = Utility.JsonbObject.ConvertFromJsonForExgratia_Bonus(trns.Related_Account_Data!);

                            Pay_Init payInitExgratia = new();
                            Pay_Slip paySlipExgratia = new();
                            List<Pay_Slip> paySlipExgratiaList = new();
                            Pay_Template payTemplateEXgratia = new();
                            payTemplateEXgratia = await _unitOfWork.PayTemplate.GetPayTemplateAsync();
                            payInitExgratia = Utility.GetModalObject.GetPayIntObject(0, trns.Transacted_Date.Month, trns.Transacted_Date.Year, false, Checked_By, exgratia.YrId, "E", null, null, 0, trns.BrCode!);
                            var payInitExgratiaResult = await _unitOfWork.PayInit.AddPayInitAsync(payInitExgratia);
                            if (payInitExgratiaResult != null && payInitExgratiaResult.Pay_Id > 0)
                                payIdExgratia = payInitExgratiaResult.Pay_Id;
                            foreach (var emp in exgratia.Component_List!)
                            {
                                Narration = emp.Employee_Name! + " Ex-gratia Payment";
                                paySlipExgratia = Utility.GetModalObject.GetPaySlipObject(0, payIdExgratia, emp.Employee_Id, 0, 0, 0, 0, 0, 0, 0, 0, 0, emp.PaymentAmount, 0, 0, 0, emp.PaymentAmount, 0, emp.PaymentAmount, Checked_By, exgratia.YrId, true, trns.Transacted_Date, vocId, false, 0, trns.BrCode!);
                                paySlipExgratiaList.Add(paySlipExgratia);
                                vocTrn = new();
                                vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, payTemplateEXgratia.ExGratia_Led_Id, 0, emp.PaymentAmount, trns.Cash_Or_Adjustment, Narration, false, Checked_By, exgratia.YrId, Status, "", emp.Employee_Id, exgratia.BrCode!, 0, 0, 0);
                                finVoucherTrns.Add(vocTrn);
                                
                            }
                            foreach (var slip in paySlipExgratiaList)
                            {
                                slip.Voc_Id = vocId;
                                result = await _unitOfWork.PaySlip.AddPaySlipAsync(slip);
                            }
                            
                            #endregion 
                            break;
                        case 34:    /// Bonus Payment
                            #region Bonus
                            decimal payIdBonus = 0;
                            DtoPayExgratiaBonus bonus = new();
                            bonus = Utility.JsonbObject.ConvertFromJsonForExgratia_Bonus(trns.Related_Account_Data!);

                            Pay_Init payInitBonus = new();
                            Pay_Slip paySlipBonus = new();
                            List<Pay_Slip> paySlipBonusList = new();
                            Pay_Template payTemplateBonus = new();
                            payTemplateBonus = await _unitOfWork.PayTemplate.GetPayTemplateAsync();
                            payInitBonus = Utility.GetModalObject.GetPayIntObject(0, trns.Transacted_Date.Month, trns.Transacted_Date.Year, false, Checked_By, bonus.YrId, "B", null, null, 0, trns.BrCode!);
                            var payInitBonusResult = await _unitOfWork.PayInit.AddPayInitAsync(payInitBonus);
                            if (payInitBonusResult != null && payInitBonusResult.Pay_Id > 0)
                                payIdBonus = payInitBonusResult.Pay_Id;
                            foreach (var emp in bonus.Component_List!)
                            {
                                Narration = emp.Employee_Name! + " Bonus Payment";
                                paySlipBonus = Utility.GetModalObject.GetPaySlipObject(0, payIdBonus, emp.Employee_Id, 0, 0, 0, 0, 0, 0, 0, 0, 0, emp.PaymentAmount, 0, 0, 0, emp.PaymentAmount, 0, emp.PaymentAmount, Checked_By, bonus.YrId, true, trns.Transacted_Date, vocId, false, 0, trns.BrCode!);
                                paySlipBonusList.Add(paySlipBonus);
                                vocTrn = new();
                                vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, payTemplateBonus.Bonus_Led_Id, 0, emp.PaymentAmount, trns.Cash_Or_Adjustment, Narration, false, Checked_By, bonus.YrId, Status, "", emp.Employee_Id, bonus.BrCode!, 0, 0, 0);
                                finVoucherTrns.Add(vocTrn);
                                //var bonusResult = await _unitOfWork.PaySlip.UpdatePaySlipForPayment(emp.Employee_Id, payIdBonus, vocId, trns.Transacted_Date);
                            }
                            foreach (var slip in paySlipBonusList)
                            {
                                slip.Voc_Id = vocId;
                                result = await _unitOfWork.PaySlip.AddPaySlipAsync(slip);
                            }
                            #endregion 
                            break;
                        case 35:    /// Staff PF withdrawal
                            #region PF withdrawn
                            DtoPayPFData pfWithdrawn = new();
                            pfWithdrawn = Utility.JsonbObject.ConvertFromJsonForPF(trns.Related_Account_Data!);
                            Narration = pfWithdrawn.Employee_Name! + " EPF withdrawn";
                            Emp_Pf empPFWithdrawn = new();
                            empPFWithdrawn = Utility.GetModalObject.GetEmpPFObject(0, pfWithdrawn.Employee_Id, pfWithdrawn.Transaction_Date, 0, 0, 0, 0, 0, 0, 0, 0, pfWithdrawn.PFWithdrawn, pfWithdrawn.VPFWithdrawn, pfWithdrawn.EmployersPFWithdrawn, pfWithdrawn.InterestOnPFWithdrawn, pfWithdrawn.InterestOnEmployersPFWithdrawn, pfWithdrawn.Transaction_Date, false, false, pfWithdrawn.Transaction_Date, vocId, Checked_By, pfWithdrawn.YrId, 0, "AR", pfWithdrawn.BrCode!);
                            Pay_Template templatePf = new();
                            templatePf = await _unitOfWork.PayTemplate.GetPayTemplateAsync(pfWithdrawn.BrCode!);

                            if (pfWithdrawn.PFWithdrawn > 0 || pfWithdrawn.VPFWithdrawn > 0 || pfWithdrawn.EmployersPFWithdrawn > 0)
                            {
                                vocTrn = new();
                                vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, templatePf.Emp_PF_Led_Id, 0, pfWithdrawn.PFWithdrawn + pfWithdrawn.VPFWithdrawn + pfWithdrawn.EmployersPFWithdrawn, trns.Cash_Or_Adjustment, Narration, false, Checked_By, pfWithdrawn.YrId, Status, "", pfWithdrawn.Employee_Id, pfWithdrawn.BrCode!, 0, 0, 0);
                                finVoucherTrns.Add(vocTrn);
                            }
                            if (pfWithdrawn.InterestOnPFWithdrawn > 0 || pfWithdrawn.InterestOnEmployersPFWithdrawn > 0)
                            {
                                vocTrn = new();
                                vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, templatePf.Int_On_PF_Led_Id, pfWithdrawn.InterestOnPFWithdrawn + pfWithdrawn.InterestOnEmployersPFWithdrawn, 0, trns.Cash_Or_Adjustment, Narration, false, Checked_By, pfWithdrawn.YrId, Status, "", pfWithdrawn.Employee_Id, pfWithdrawn.BrCode!, 0, 0, 0);
                                finVoucherTrns.Add(vocTrn);
                            }

                            result = await _unitOfWork.EmployeePF.AddEmployeePFAsync(empPFWithdrawn);
                            #endregion 
                            break;
                        case 36:    /// SB Account transaction
                            #region SB Account Transaction
                            Mem_Trn sbMemberTrn = new();
                            DtoSBAccountTransaction sbTrn = new();
                            sbTrn = Utility.JsonbObject.ConvertFromJsonForNewSBAccountTransaction(trns.Related_Account_Data!);
                            Narration = sbTrn.Member_No + " " + sbTrn.Member_Name;
                            sbMemberTrn = Utility.GetModalObject.GetMemTrnObject(7, sbTrn.Mem_Id, sbTrn.SBLed_Id, sbTrn.Transaction_Date, sbTrn.Receipt_Amount,
                                sbTrn.Payment_Amount, false, false, vocId, Checked_By, yrId, 0, 0, null, 0, "", 0, sbTrn.SBAccount_Id, 0, brCode);
                            vocTrn = new();
                            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, sbTrn.SBLed_Id, sbTrn.Receipt_Amount, sbTrn.Payment_Amount,
                                sbTrn.CashOrAdjustment, Narration + " SB AC No :" + sbTrn.SBAccount_No, false, Checked_By, yrId, Status, "SB AC No :" + sbTrn.SBAccount_No, sbTrn.Mem_Id, brCode, 0, 0, 0);
                            finVoucherTrns.Add(vocTrn);
                            result = await _unitOfWork.MemTrn.AddMemTrnAsync(sbMemberTrn);
                            //result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns);
                            #endregion
                            break;
                        case 37:    /// Jewel loan disbursement
                            #region Jewel loan disbursement
                            decimal jlLoanId = 0;
                            string jlLoanNo = "";
                            Loan_Master jlMaster = new();
                            Loan_Trn jlTrn = new();
                            Loan_Disb jlDisb = new();
                            Loan_Roi jlRoi = new();
                            JL_Details jlDetails = new();
                            List<JL_Ornments> ornmentList = new();
                            Loan_Schemes jlSchemes = new();
                            DtoJewelLoanDisbursement dtoJLDisb = new();
                            dtoJLDisb = Utility.JsonbObject.ConvertFromJsonForJLDisbursement(trns.Related_Account_Data!);
                            Narration = dtoJLDisb.Member_No!.Trim() + " " + dtoJLDisb.Member_Name!.Trim();
                            jlSchemes = await _unitOfWork.LoanScheme.GetLoanSchemesAsync(dtoJLDisb.Scheme_Id, brCode);

                            jlMaster = Utility.GetModalObject.GetLoanMasterObject(dtoJLDisb.Scheme_Id, "", dtoJLDisb.Mem_Id, 0, "",
                                dtoJLDisb.Tranaction_Date, "", null, dtoJLDisb.Loan_Amount, dtoJLDisb.Tranaction_Date, 2, 0, 0, 0, 0,
                                dtoJLDisb.Period_Of_Loan, dtoJLDisb.Period_Of_Loan, dtoJLDisb.Tranaction_Date, dtoJLDisb.Tranaction_Date,
                                0, "", "", "", null, null, 0, dtoJLDisb.Rate_Of_Interest, dtoJLDisb.Penal_Rate, dtoJLDisb.Tranaction_Date,
                                0, false, false, false, vocId, Checked_By, yrId, 0, 0, brCode, 0, 0, 0);
                            (result, jlLoanId, jlLoanNo) = await _unitOfWork.LoanMaster.AddLoanMasterAsync(jlMaster);

                            jlTrn = Utility.GetModalObject.GetLoanTrnObject(jlLoanId, 0, "I", dtoJLDisb.Tranaction_Date, null,
                                dtoJLDisb.Tranaction_Date, dtoJLDisb.Loan_Amount, 0, 0, 0, null, 0, null, 0, null, 0, 0, 0, 0, 0, 0, 0,
                                null, 0, null, 0, null, dtoJLDisb.Rate_Of_Interest, dtoJLDisb.Penal_Rate, 0, 0, false, false, vocId,
                                Checked_By, yrId, 1, false, dtoJLDisb.Loan_Amount, 0, 0, 0, 0, brCode);


                            jlDisb = Utility.GetModalObject.GetLoanDisbursementObject(jlLoanId, dtoJLDisb.Tranaction_Date, 1,
                                dtoJLDisb.Loan_Amount, dtoJLDisb.Tranaction_Date, null, "", null, 0, null, "", null, false, 1, true, false,
                                vocId, Checked_By, yrId, brCode);

                            jlRoi = Utility.GetModalObject.GetLoanROIObject(jlLoanId, "S", dtoJLDisb.Tranaction_Date, dtoJLDisb.Rate_Of_Interest,
                                dtoJLDisb.Penal_Rate, 0, 0, false, false, vocId, Checked_By, yrId, brCode);

                            jlDetails = Utility.GetModalObject.GetJLDetails(0, jlLoanId, dtoJLDisb.Due_Date, dtoJLDisb.Govt_Rate, dtoJLDisb.Gross_Weight,
                                dtoJLDisb.Wasgate, dtoJLDisb.Net_Weight, dtoJLDisb.Net_Weight, dtoJLDisb.Market_Rate, dtoJLDisb.Percentage_Of_Eligibility_On_Market_Rate,
                                dtoJLDisb.Jewels_Photo_Path!, false, false, vocId, Checked_By, yrId, brCode);

                            foreach (var orn in dtoJLDisb.Ornment_List!)
                            {
                                JL_Ornments jlOrn = new JL_Ornments
                                {
                                    JLO_Id = 0,
                                    Loan_Id = jlLoanId,
                                    JLO_Nos = orn.Ornment_Nos,
                                    JLO_Name = orn.Ornment_Name,
                                    JLO_OE = false,
                                    JLO_Delete = false,
                                    Voc_Id = vocId,
                                    Yr_Id = yrId,
                                    BrCode = brCode,
                                };
                                ornmentList.Add(jlOrn);
                            }
                            Map_General map = await _unitOfWork.MapGeneral.GetMapGeneralAsync(brCode);
                            if (dtoJLDisb.CashOrAdjustment == 1)
                            {
                                vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, jlSchemes.PrlLed_Id, 0, trns.CashPayment_Amount,
                                dtoJLDisb.CashOrAdjustment, Narration + " Loan No : " + jlLoanNo, false, Checked_By, yrId, Status, "Loan No :" + jlLoanNo,
                                dtoJLDisb.Mem_Id, brCode, jlLoanId, dtoJLDisb.Loan_Amount, 0);
                                finVoucherTrns.Add(vocTrn);
                                vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, jlSchemes.PrlLed_Id, 0, trns.AdjustmentPayment_Amount,
                                2, Narration + " Loan No : " + jlLoanNo, false, Checked_By, yrId, Status, "Loan No :" + jlLoanNo,
                                dtoJLDisb.Mem_Id, brCode, jlLoanId, dtoJLDisb.Loan_Amount, 0);
                                finVoucherTrns.Add(vocTrn);
                                if (dtoJLDisb.AppraisalFee > 0)
                                {
                                    vocTrn = new();
                                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, map.Appraisal_Fee_Led_Id, dtoJLDisb.AppraisalFee, 0, 2,
                                        Narration + " Loan No :" + jlLoanNo, false, Checked_By, yrId, Status, "Loan No: " + jlLoanNo, dtoJLDisb.Mem_Id, brCode,
                                        jlLoanId, dtoJLDisb.Loan_Amount, 0);
                                    finVoucherTrns.Add(vocTrn);
                                }
                                if (dtoJLDisb.BankCharges > 0)
                                {
                                    vocTrn = new();
                                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, map.Bank_Charges_Led_Id, dtoJLDisb.BankCharges,
                                        0, 2, Narration + " Loan No :" + jlLoanNo, false, Checked_By, yrId, Status, "Loan No: " + jlLoanNo,
                                        dtoJLDisb.Mem_Id, brCode, jlLoanId, dtoJLDisb.Loan_Amount, 0);
                                    finVoucherTrns.Add(vocTrn);
                                }
                            }
                            else
                            {
                                vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, jlSchemes.PrlLed_Id, 0, dtoJLDisb.Loan_Amount,
                                    dtoJLDisb.CashOrAdjustment, Narration + " Loan No : " + jlLoanNo, false, Checked_By, yrId, Status, "Loan No :" + jlLoanNo,
                                    dtoJLDisb.Mem_Id, brCode, jlLoanId, dtoJLDisb.Loan_Amount, 0);
                                finVoucherTrns.Add(vocTrn);

                                //Map_General map = await _unitOfWork.MapGeneral.GetMapGeneralAsync(brCode);
                                if (dtoJLDisb.AppraisalFee > 0)
                                {
                                    vocTrn = new();
                                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, map.Appraisal_Fee_Led_Id, dtoJLDisb.AppraisalFee, 0, dtoJLDisb.CashOrAdjustment,
                                        Narration + " Loan No :" + jlLoanNo, false, Checked_By, yrId, Status, "Loan No: " + jlLoanNo, dtoJLDisb.Mem_Id, brCode,
                                        jlLoanId, dtoJLDisb.Loan_Amount, 0);
                                    finVoucherTrns.Add(vocTrn);
                                }
                                if (dtoJLDisb.BankCharges > 0)
                                {
                                    vocTrn = new();
                                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, map.Bank_Charges_Led_Id, dtoJLDisb.BankCharges,
                                        0, dtoJLDisb.CashOrAdjustment, Narration + " Loan No :" + jlLoanNo, false, Checked_By, yrId, Status, "Loan No: " + jlLoanNo,
                                        dtoJLDisb.Mem_Id, brCode, jlLoanId, dtoJLDisb.Loan_Amount, 0);
                                    finVoucherTrns.Add(vocTrn);
                                }
                            }

                            result = await _unitOfWork.LoanTrn.AddLoanTrn(jlTrn);
                            result = await _unitOfWork.LoanDisbursement.AddLoanDisbursementAsync(jlDisb);
                            result = await _unitOfWork.LoanROI.AddLoanROIAsync(jlRoi);
                            result = await _unitOfWork.JLDetails.AddJLDetailsAsync(jlDetails);
                            result = await _unitOfWork.JLOrnment.AddJLOrnmentListAsync(ornmentList);
                            //result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns);
                            #endregion
                            break;
                        case 38:    /// Group Insurance Payment
                            break;
                        case 39:    /// Salary Payment
                            #region salary payment
                            Pay_Template payTemplate = new();
                            payTemplate = await _unitOfWork.PayTemplate.GetPayTemplateAsync();
                            DtoPaySlipPayment salaryPaymentData = new();
                            salaryPaymentData = Utility.JsonbObject.ConvertFromJsonForSalaryPayment(trns.Related_Account_Data!);
                            decimal payId = salaryPaymentData.Pay_Id;
                            List<decimal> empIdList = new();
                            empIdList = salaryPaymentData.Employee_Id_List!.ToList();
                            List<Pay_Slip_Trn> paySlipTrnsList = new();
                            //List<Pay_Slip_Loan_Trn> payLoanList = new();
                            List<LoanDetailsVM> payLoanList = new();
                            List<Mem_Trn> memTrnList = new();
                            List<Loan_Trn> loanList = new();
                            List<Emp_Pf> pfList = new();
                            Fin_Voucher_Trn vocNetPay = new Fin_Voucher_Trn();
                            Fin_Voucher_Trn vocDeductions = new Fin_Voucher_Trn();
                            Fin_Voucher_Trn vocSocietyPF = new Fin_Voucher_Trn();
                            Fin_Voucher_Trn vocReceipt = new Fin_Voucher_Trn();
                            //List<Fin_Voucher_Trn> vocList = new();
                            foreach (var emp in empIdList)
                            {
                                var empData = await _unitOfWork.Members.GetMemberDetailsByMemIdAsync(emp);
                                if (empData != null)
                                {
                                    Narration = empData.MemberNo!;
                                    Narration += " " + empData.MemberName!;
                                }
                                Pay_Slip slip = await _unitOfWork.PaySlip.GetPaySlipByMemId(payId, emp, trns.BrCode!);
                                Emp_Pf pf = new();
                                pf = Utility.GetModalObject.GetEmpPFObject(0, emp, trns.Transacted_Date, slip.Pay_PF, slip.Pay_VPF, slip.Pay_PF, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, null, false, false, null, vocId, trns.Checked_By, yrId, 0, "AR", trns.BrCode!);
                                pfList.Add(pf);
                                //var loanListResponse = await _unitOfWork.PaySlipLoanTrn.GetPaySlipLoanTrnList(payId, emp, trns.BrCode!);
                                var loanListResponse = await _unitOfWork.PaySlipLoanTrn.GetPaySlipLoanList(payId, emp, trns.BrCode!);
                                if (loanListResponse != null && loanListResponse.Any()) payLoanList = loanListResponse.ToList();
                                var salaryResult = await _unitOfWork.PaySlip.UpdatePaySlipForPayment(emp, payId, vocId, trns.Transacted_Date);

                                if (trns.Cash_Or_Adjustment == 1)  /// cash transaction
                                {
                                    /// Net salary as cash payment
                                    vocNetPay = new();
                                    vocNetPay = Utility.GetModalObject.GetFinVoucherTrObject(vocId, payTemplate.Salary_Led_Id, 0, slip.Pay_Net, 1, Narration, false, trns.Checked_By, yrId, "SP", "", emp, trns.BrCode!);
                                    finVoucherTrns.Add(vocNetPay);
                                    /// total deductions as adjustment payment
                                    vocDeductions = Utility.GetModalObject.GetFinVoucherTrObject(vocId, payTemplate.Salary_Led_Id, 0, slip.Pay_Tot_Deductions, 2, Narration, false, trns.Checked_By, yrId, "SP", "", emp, trns.BrCode!);
                                    finVoucherTrns.Add(vocDeductions);
                                }
                                if (trns.Cash_Or_Adjustment != 1)  /// adjustment transaction/ bank advice / NEFT
                                {
                                    vocNetPay = new();
                                    vocNetPay = Utility.GetModalObject.GetFinVoucherTrObject(vocId, payTemplate.Salary_Led_Id, 0, slip.Pay_Tot_Allowance, 2, Narration, false, trns.Checked_By, yrId, "SP", "", emp, trns.BrCode!);
                                    finVoucherTrns.Add(vocNetPay);
                                }
                                /// society pf contribution payment as adjustment
                                vocSocietyPF = Utility.GetModalObject.GetFinVoucherTrObject(vocId, payTemplate.Banks_PF_Led_Id, 0, slip.Pay_PF, 2, Narration, false, trns.Checked_By, yrId, "SP", "", emp, trns.BrCode!);
                                finVoucherTrns.Add(vocSocietyPF);
                                /// receipt  pf for contral to society pf contribution  
                                vocReceipt = Utility.GetModalObject.GetFinVoucherTrObject(vocId, payTemplate.Emp_PF_Led_Id, slip.Pay_PF, 0, 2, Narration, false, trns.Checked_By, yrId, "SP", "", emp, trns.BrCode!);
                                finVoucherTrns.Add(vocReceipt);
                                paySlipTrnsList = await _unitOfWork.PaySlipTrn.GetPaySlipTrnByMemId(payId, emp, trns.BrCode!);

                                foreach (var slipTrn in paySlipTrnsList.Where(x => x.Pay_Component_Type == 5).ToList())  /// due to
                                {
                                    Fin_Voucher_Trn memVoc = new();
                                    memVoc = Utility.GetModalObject.GetFinVoucherTrObject(vocId, slipTrn.Led_Id, slipTrn.Deduction_Amt, 0, 2, Narration, false, trns.Checked_By, yrId, "SP", "", emp, trns.BrCode!);
                                    finVoucherTrns.Add(memVoc);
                                    Mem_Trn memTrn = Utility.GetModalObject.GetMemTrnObject(5, emp, slipTrn.Led_Id, trns.Transacted_Date, slipTrn.Deduction_Amt, 0, false, false, vocId, trns.Checked_By, yrId, 0, 0, null, 0, "", 0, 0, 0, trns.BrCode!);
                                    memTrnList.Add(memTrn);
                                }

                                foreach (var slipTr in paySlipTrnsList.Where(x => x.Pay_Component_Type == 2).ToList())  /// fixed deductions
                                {
                                    Fin_Voucher_Trn vocEachDeduction = new Fin_Voucher_Trn();
                                    vocEachDeduction = Utility.GetModalObject.GetFinVoucherTrObject(vocId, slipTr.Led_Id, slipTr.Deduction_Amt, 0, 2, Narration, false, trns.Checked_By, yrId, "SP", "", emp, trns.BrCode!);
                                    finVoucherTrns.Add(vocEachDeduction);
                                }
                            }

                            foreach (var loan in payLoanList)
                            {
                                Loan_Trn trn = Utility.GetModalObject.GetLoanTrnObject(loan.loanid, 0, "R", trns.Transacted_Date, null, null, 0, loan.prlschedule, loan.prlschedule, 0, null, 0, null, loan.intcollamt, loan.intcalcdate, 0, 0, loan.intcollamt, loan.prlcoll, 0, 0, 0, null, 0, null, 0, null, 0, 0, 0, 0, false, false, vocId, Checked_By, yrId, 0, false, 0, 0, 0, 0, 0, trns.BrCode!);
                                loanList.Add(trn);
                                Fin_Voucher_Trn loanVoc = new();
                                if (loan.prlcoll > 0)
                                {
                                    loanVoc = Utility.GetModalObject.GetFinVoucherTrObject(vocId, loan.prlledid, loan.prlcoll, 0, 2, Narration, false, trns.Checked_By, yrId, "SP", "Loan No " + loan.loanno, loan.memid, trns.BrCode!, loan.loanid, 0, 0);
                                    finVoucherTrns.Add(loanVoc);
                                }
                                if (loan.intcollamt > 0)
                                {

                                    loanVoc = Utility.GetModalObject.GetFinVoucherTrObject(vocId, loan.intledid, loan.intcollamt, 0, 2, Narration, false, trns.Checked_By, yrId, "SP", "Loan No " + loan.loanno, loan.memid, trns.BrCode!, loan.loanid, 0, 0);
                                    finVoucherTrns.Add(loanVoc);
                                }
                            }
                            //finVoucherTrns.AddRange(vocList);
                            foreach (var pf in pfList)
                            {
                                result = await _unitOfWork.EmployeePF.AddEmployeePFAsync(pf);
                            }
                            result = await _unitOfWork.LoanTrn.AddLoanTrnListAsync(loanList);
                            foreach (var memTrn in memTrnList)
                            {
                                result = await _unitOfWork.MemTrn.AddMemTrnAsync(memTrn);
                            }
                            #endregion
                            break;
                        case 40:    /// dA Arrear Payment
                            #region DA arrears payment
                            Pay_Template daPayTemplate = new();
                            daPayTemplate = await _unitOfWork.PayTemplate.GetPayTemplateAsync();
                            DtoPaySlipPayment daArrearsPayment = new();
                            daArrearsPayment = Utility.JsonbObject.ConvertFromJsonForSalaryPayment(trns.Related_Account_Data!);
                            decimal daPayId = daArrearsPayment.Pay_Id;
                            List<decimal> daEmpIdList = new();
                            daEmpIdList = daArrearsPayment.Employee_Id_List!.ToList();

                            List<Emp_Pf> daPfList = new();
                            Fin_Voucher_Trn daVocNetPay = new Fin_Voucher_Trn();
                            Fin_Voucher_Trn daVocDeductions = new Fin_Voucher_Trn();
                            Fin_Voucher_Trn daVocSocietyPF = new Fin_Voucher_Trn();
                            Fin_Voucher_Trn daVocReceipt = new Fin_Voucher_Trn();
                            foreach (var emp in daEmpIdList)
                            {
                                var empData = await _unitOfWork.Members.GetMemberDetailsByMemIdAsync(emp);
                                if (empData != null)
                                {
                                    Narration = empData.MemberNo!;
                                    Narration += " " + empData.MemberName!;
                                }
                                Pay_Slip slip = await _unitOfWork.PaySlip.GetPaySlipByMemId(daPayId, emp, trns.BrCode!);
                                Emp_Pf pf = new();
                                pf = Utility.GetModalObject.GetEmpPFObject(0, emp, trns.Transacted_Date, slip.Pay_PF, 0, slip.Pay_PF, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, null, false, false, null, vocId, trns.Checked_By, yrId, 0, "AR", trns.BrCode!);
                                daPfList.Add(pf);

                                var daArrearsResult = await _unitOfWork.PaySlip.UpdatePaySlipForPayment(emp, daPayId, vocId, trns.Transacted_Date);

                                if (trns.Cash_Or_Adjustment == 1)  /// cash transaction
                                {
                                    /// Net salary as cash payment
                                    daVocNetPay = new();
                                    daVocNetPay = Utility.GetModalObject.GetFinVoucherTrObject(vocId, daPayTemplate.Salary_Led_Id, 0, slip.Pay_Net, 1, Narration, false, trns.Checked_By, yrId, "SP", "", emp, trns.BrCode!);
                                    finVoucherTrns.Add(daVocNetPay);
                                    /// total deductions as adjustment payment
                                    daVocDeductions = Utility.GetModalObject.GetFinVoucherTrObject(vocId, daPayTemplate.Salary_Led_Id, 0, slip.Pay_PF, 2, Narration, false, trns.Checked_By, yrId, "SP", "", emp, trns.BrCode!);
                                    finVoucherTrns.Add(daVocDeductions);
                                }
                                if (trns.Cash_Or_Adjustment != 1)  /// adjustment transaction/ bank advice / NEFT
                                {
                                    daVocNetPay = new();
                                    daVocNetPay = Utility.GetModalObject.GetFinVoucherTrObject(vocId, daPayTemplate.Salary_Led_Id, 0, slip.Pay_Tot_Allowance, 2, Narration, false, trns.Checked_By, yrId, "SP", "", emp, trns.BrCode!);
                                    finVoucherTrns.Add(daVocNetPay);
                                }
                                /// society pf contribution payment as adjustment
                                daVocSocietyPF = Utility.GetModalObject.GetFinVoucherTrObject(vocId, daPayTemplate.Banks_PF_Led_Id, 0, slip.Pay_PF, 2, Narration, false, trns.Checked_By, yrId, "SP", "", emp, trns.BrCode!);
                                finVoucherTrns.Add(daVocSocietyPF);
                                /// receipt  pf for contral to society pf contribution  
                                daVocReceipt = Utility.GetModalObject.GetFinVoucherTrObject(vocId, daPayTemplate.Emp_PF_Led_Id, slip.Pay_PF + slip.Pay_PF, 0, 2, Narration, false, trns.Checked_By, yrId, "SP", "", emp, trns.BrCode!);
                                finVoucherTrns.Add(daVocReceipt);
                            }
                            foreach (var pf in daPfList)
                            {
                                result = await _unitOfWork.EmployeePF.AddEmployeePFAsync(pf);
                            }

                            #endregion
                            break;
                        case 41:    /// Surrender leave salary payment
                            #region SLS payment
                            decimal payIdSLS = 0;
                            DtoPaySLSPayment slsPayment = new();
                            slsPayment = Utility.JsonbObject.ConvertFromJsonForSLS(trns.Related_Account_Data!);
                            Narration = slsPayment.Employee_Name! + " SLS Payment";
                            Pay_Init payInitSLS = new();
                            Pay_Slip paySlipSLS = new();
                            Pay_Template payTemplateSLS = new();
                            payTemplateSLS = await _unitOfWork.PayTemplate.GetPayTemplateAsync();
                            payInitSLS = Utility.GetModalObject.GetPayIntObject(0, trns.Transacted_Date.Month, trns.Transacted_Date.Year, false, Checked_By, slsPayment.YrId, "S", null, null, slsPayment.DAId, trns.BrCode!);
                            var payInitResult = await _unitOfWork.PayInit.AddPayInitAsync(payInitSLS);
                            if (payInitResult != null && payInitResult.Pay_Id > 0)
                                payIdSLS = payInitResult.Pay_Id;
                            paySlipSLS = Utility.GetModalObject.GetPaySlipObject(0, payIdSLS, slsPayment.Employee_Id, slsPayment.BasicPay, 0, slsPayment.PerPay, 0, slsPayment.GradePay, 0, slsPayment.DAPercentage, 0, slsPayment.SLSAmount, 0, 0, 0, 0, slsPayment.SLSAmount, 0, slsPayment.SLSAmount, Checked_By, slsPayment.YrId, true, trns.Transacted_Date, vocId, false, slsPayment.SLSDays, trns.BrCode!);
                            if (slsPayment.SLSAmount > 0)
                            {
                                vocTrn = new();
                                vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, payTemplateSLS.Salary_Led_Id, 0, slsPayment.SLSAmount, trns.Cash_Or_Adjustment, Narration, false, Checked_By, slsPayment.YrId, Status, "", slsPayment.Employee_Id, slsPayment.BrCode!, 0, 0, 0);
                                finVoucherTrns.Add(vocTrn);
                            }
                            result = await _unitOfWork.PaySlip.AddPaySlipAsync(paySlipSLS);
                            var payvocResult = await _unitOfWork.PaySlip.UpdatePaySlipForPayment(slsPayment.Employee_Id, payInitResult!.Pay_Id, vocId, trns.Transacted_Date);
                            #endregion 
                            break;
                        case 42:    /// Staff security deposit receipt
                            #region Security deposit creation
                            TermDeposit_Schemes sdScheme = new();
                            TermDeposit_Master sdMaster = new TermDeposit_Master();
                            TermDeposit_Trn sdTrn = new TermDeposit_Trn();
                            TermDeposit_Members sdMember = new TermDeposit_Members();
                            DtoSecurityDepositData sdemp = new();
                            int.TryParse(trns.BrCode + "306", out int schemeId);
                            sdemp = Utility.JsonbObject.ConvertFromJsonForSecurityDeposit(trns.Related_Account_Data!);
                            sdMaster = Utility.GetModalObject.GetTermDepositMaster(0, "", schemeId, sdemp.Employee_Id, sdemp.Employee_Name!, sdemp.Age, 1,
                                trns.Transacted_Date, trns.Transacted_Date, trns.Receipt_Amount, 0, 0, sdemp.RateOfInterest,
                                trns.Transacted_Date, 0, true, 12, 0, false, false,
                                0, sdemp.Nominee_Name!, sdemp.Nominee_Age, sdemp.Nominee_Relationship!,
                                "", 0, "", false, false, false, vocId, Checked_By,
                                yrId, "N", 0, "", brCode);
                            (result, newTDId, newTDNo) = await _unitOfWork.TermDepositMaster.AddTermDepositMasterAsync(sdMaster);
                            if (!result)
                            {
                                return voucherData;
                            }
                            sdTrn = Utility.GetModalObject.GetTermDepositTrn(0, trns.Transacted_Date, newTDId, 0,
                                trns.Receipt_Amount, 0, 0, null, 0, 0, 0, null, 0, 0, 0, null, null, false, false,
                                vocId, Checked_By, sdemp.Yr_Id, 1, 0, trns.BrCode!);


                            sdMember = Utility.GetModalObject.GetTermDepositMembers(0, newTDId, sdemp.Employee_Id, 1, false, vocId, Checked_By, sdemp.Yr_Id, trns.BrCode!);
                            result = await _unitOfWork.TermDepositTrn.AddTermDepositTrnAsync(sdTrn);
                            result = await _unitOfWork.TermDepositMember.AddTermDepositMemberAsync(sdMember);


                            Narration = sdemp.Employee_Name + " Security Deposit No " + newTDNo;
                            sdScheme = await _unitOfWork.TermDepositScheme.GetTermDepositSchemeByIdAsync(schemeId);
                            vocTrn = new();
                            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, sdScheme.Led_Id, trns.Receipt_Amount, 0, trns.Cash_Or_Adjustment, Narration, false, Checked_By, sdemp.Yr_Id, Status, "SD No : " + newTDNo, sdemp.Employee_Id, trns.BrCode!, 0, 0, 0);
                            finVoucherTrns.Add(vocTrn);

                            #endregion
                            break;
                        case 43:    /// Staff security deposit payment
                            break;
                        case 44:    /// Member due to transaction bulk
                            break;
                        case 45:    /// Member due by transaction bulk
                            break;
                        case 46:    /// Loan disbursement subsequent instalment HSIS
                            break;
                        case 47:    /// Loan Reimbursement HSIS
                            break;
                        case 48:    /// Long term Loan disbursment
                            #region Long Term Loan Disbursement
                            if (trns.Security_Type == 1) /// jewel loan pledge
                            {
                                List<Fin_Voucher_Trn> jlVocTrnList = new();
                                (result, jlVocTrnList) = await SaveJewelLoanDisbursement(trns, 6, vocId, Checked_By, yrId, brCode, Status);
                                finVoucherTrns.AddRange(jlVocTrnList);
                            }
                            else if (trns.Security_Type == 2)    /// mortgage loan
                            {
                                List<Fin_Voucher_Trn> vocTrnList = new();
                                (result, vocTrnList) = await SaveLongTermDisbursement(trns, vocId, Checked_By, yrId, brCode, Status);
                                finVoucherTrns.AddRange(vocTrnList);
                            }
                            #endregion
                            break;
                        case 51:    /// Decreed loan
                            break;
                        case 52:    /// Jewel loan disbursement with appraisal fee
                            #region Jewel Loan Disbursement with appraisal fee commented
                            //string loanNo = "";
                            //decimal loanId = 0;
                            //DtoJewelLoanDisbursement dtoJlDisb = new DtoJewelLoanDisbursement();
                            //Loan_Master jlMaster = new Loan_Master();
                            //Loan_Trn jlTrn = new Loan_Trn();
                            //Loan_Disb jlDisb = new Loan_Disb();
                            //Loan_Roi jlLoanRoi = new Loan_Roi();
                            //JL_Details jlDetails = new JL_Details();
                            //List<JL_Ornments> ornmentList = new List<JL_Ornments>();

                            //Narration = dtoJlDisb.Member_No!.Trim() + " " + dtoJlDisb.Member_Name!.Trim();

                            //dtoJlDisb = Utility.JsonbObject.ConvertFromJsonForJeweLoanDisbursement(trns.Related_Account_Data!);
                            //Loan_Schemes scheme = await _unitOfWork.LoanScheme.GetLoanSchemesAsync(dtoJlDisb.Scheme_Id);
                            //decimal prlLedId = scheme.PrlLed_Id;
                            //jlMaster = Utility.GetModalObject.GetLoanMasterObject(dtoJlDisb.Scheme_Id, "", dtoJlDisb.Mem_Id, 0, "", null, "", null, dtoJlDisb.Loan_Amount,
                            //    dtoJlDisb.Tranaction_Date, 2, 0, 0, 0, 0, dtoJlDisb.Period_Of_Loan, 0,
                            //    dtoJlDisb.Tranaction_Date, dtoJlDisb.Tranaction_Date, 0, "", "", "", null, null, 0, dtoJlDisb.Rate_Of_Interest,
                            //    dtoJlDisb.Penal_Rate, dtoJlDisb.Tranaction_Date, 0, false, false, false, vocId, Checked_By, yrId, 0, 0,brCode, 0, 0, 0);
                            //(result, loanId, loanNo) = await _unitOfWork.LoanMaster.AddLoanMasterAsync(jlMaster);
                            //if (!result)
                            //{
                            //    return false;
                            //}
                            //jlTrn = Utility.GetModalObject.GetLoanTrnObject(loanId, 0, "I", dtoJlDisb.Tranaction_Date, null, dtoJlDisb.Tranaction_Date,
                            //    dtoJlDisb.Loan_Amount, 0, 0, 0, null, 0, null, 0, null, 0, 0, 0, 0, 0, 0, 0, null, 0, null, 0, null,
                            //    dtoJlDisb.Rate_Of_Interest, dtoJlDisb.Penal_Rate, 0, 0, false, false, vocId, Checked_By, yrId, 1, false,
                            //    dtoJlDisb.Loan_Amount, 0, 0, 0, 0,brCode);
                            //jlDisb = Utility.GetModalObject.GetLoanDisbursementObject(loanId, dtoJlDisb.Tranaction_Date, 1, dtoJlDisb.Loan_Amount,
                            //    dtoJlDisb.Tranaction_Date, null, "", null, 0, null, "", null, false, 1, true, false, vocId, Checked_By, yrId, brCode);
                            //jlLoanRoi = Utility.GetModalObject.GetLoanROIObject(loanId, "S", dtoJlDisb.Tranaction_Date, dtoJlDisb.Rate_Of_Interest,
                            //    dtoJlDisb.Penal_Rate, 0, 0, false, false, vocId, Checked_By, yrId, brCode);
                            //jlDetails = Utility.GetModalObject.GetJLDetails(0, loanId, dtoJlDisb.Due_Date, dtoJlDisb.Govt_Rate, dtoJlDisb.Gross_Weight,
                            //    dtoJlDisb.Wasgate, dtoJlDisb.Net_Weight, dtoJlDisb.Jewels_Value, dtoJlDisb.Market_Rate, dtoJlDisb.Percentage_Of_Eligibility_On_Market_Rate,
                            //    dtoJlDisb.Jewels_Photo_Path!, false, false, vocId, Checked_By, yrId, brCode);
                            //foreach (var orn in dtoJlDisb.Ornment_List!)
                            //{
                            //    JL_Ornments jlOrn = new JL_Ornments();
                            //    jlOrn = Utility.GetModalObject.GetJLOrnments(0, loanId, orn.Ornment_Name!, orn.Ornment_Nos, 0, 0, 0, 0, false, false, vocId, Checked_By, yrId, brCode);
                            //    ornmentList.Add(jlOrn);
                            //}
                            //Map_General map = await _unitOfWork.MapGeneral.GetMapGeneralAsync(brCode);
                            //List<Fin_Voucher_Trn> jlVocListForDisb = new List<Fin_Voucher_Trn>();
                            //Fin_Voucher_Trn jlVocTrn = new Fin_Voucher_Trn();
                            //jlVocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, scheme.ReimIntLed_Id, 0, dtoJlDisb.Loan_Amount, dtoJlDisb.CashOrAdjustment, Narration, false, Checked_By, yrId, Status, "Loan No: " + loanNo, dtoJlDisb.Mem_Id, brCode, loanId, dtoJlDisb.Loan_Amount, 0);
                            //jlVocListForDisb.Add(jlVocTrn);
                            //jlVocTrn = new();
                            //jlVocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, map.Appraisal_Fee_Led_Id, dtoJlDisb.AppraisalFee, 0, dtoJlDisb.CashOrAdjustment, dtoJlDisb.Member_No + dtoJlDisb.Member_Name, false, Checked_By, yrId, Status, "Loan No: " + loanNo, dtoJlDisb.Mem_Id, brCode, loanId, dtoJlDisb.Loan_Amount, 0);
                            //jlVocListForDisb.Add(jlVocTrn);
                            //jlVocTrn = new();
                            //jlVocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, map.Bank_Charges_Led_Id, dtoJlDisb.BankCharges, 0, dtoJlDisb.CashOrAdjustment, dtoJlDisb.Member_No + dtoJlDisb.Member_Name, false, Checked_By, yrId, Status, "Loan No: " + loanNo, dtoJlDisb.Mem_Id, brCode, loanId, dtoJlDisb.Loan_Amount, 0);
                            //jlVocListForDisb.Add(jlVocTrn);

                            ///// Save Jewel Loan Disbursement
                            //result = await _unitOfWork.LoanTrn.AddLoanTrn(jlTrn);
                            //result = await _unitOfWork.LoanDisbursement.AddLoanDisbursementAsync(jlDisb);
                            //result = await _unitOfWork.LoanROI.AddLoanROIAsync(jlLoanRoi);
                            //result = await _unitOfWork.JLDetails.AddJLDetailsAsync(jlDetails);
                            //result = await _unitOfWork.JLOrnment.AddJLOrnmentListAsync(ornmentList);
                            //result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(jlVocListForDisb);
                            #endregion
                            break;
                        case 1000:  /// Account Trasanctions
                            vocTrn = new();
                            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, trns.Ledger_Id, trns.Receipt_Amount, trns.Payment_Amount, trns.Cash_Or_Adjustment, Transacted_MemNo.Trim() + Transacted_MemName.Trim(), false, Checked_By, yrId, "G", "", trns.Account_Holder_Member_Id, brCode, 0, 0, 0);
                            finVoucherTrns.Add(vocTrn);
                            break;
                    }
                }
                /// Insert all the voucher transactions
                result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns);
                /// update staging_master with checked by and checked date
                result = await _unitOfWork.StagingMaster.CheckerStateStaging(stagingId, vocId, Checked_By, "V");
                result = await _unitOfWork.StagingDetails.CheckerStateStaging(stagingId, vocId, Checked_By, "V");
                result = true;
                _unitOfWork.Complete();
                _unitOfWork.CommitTransaction();
            }
            catch (Exception ex)
            {
                string er = ex.Message;
                result = false;
                voucherData = new();
                _unitOfWork.RollBack();
            }
            return voucherData;
        }

        private async Task<(bool result, List<Fin_Voucher_Trn> finVoucherTrns)> SaveJewelLoanDisbursement(DtoTransaction trns, int loanType, decimal vocId, decimal Checked_By,
            decimal yrId, string brCode, string Status)
        {
            #region MI JL
            List<Fin_Voucher_Trn> finVoucherTrns = new();
            bool result = false;
            string Narration = "";
            decimal jlMILoanId = 0;
            string jlMILoanNo = "";
            Loan_Master jlMIMaster = new();
            Loan_Trn jlMITrn = new();
            Loan_Disb jlMIDisb = new();
            Loan_Roi jlMIRoi = new();
            JL_Details jlMIDetails = new();
            Fin_Voucher_Trn vocTrn = new();
            List<JL_Ornments> MIornmentList = new();
            Loan_Schemes jlMISchemes = new();
            DtoJewelLoanDisbursement dtoJLMIDisb = new();
            dtoJLMIDisb = Utility.JsonbObject.ConvertFromJsonForJLDisbursement(trns.Related_Account_Data!);
            Narration = dtoJLMIDisb.Member_No!.Trim() + " " + dtoJLMIDisb.Member_Name!.Trim();
            jlMISchemes = await _unitOfWork.LoanScheme.GetLoanSchemesAsync(dtoJLMIDisb.Scheme_Id, brCode);

            jlMIMaster = Utility.GetModalObject.GetLoanMasterObject(dtoJLMIDisb.Scheme_Id, "", dtoJLMIDisb.Mem_Id, 0, "",
                dtoJLMIDisb.Tranaction_Date, "", null, dtoJLMIDisb.Loan_Amount, dtoJLMIDisb.Tranaction_Date, loanType, 0, 0, 0, 0,
                dtoJLMIDisb.Period_Of_Loan, dtoJLMIDisb.Period_Of_Loan, dtoJLMIDisb.Tranaction_Date, dtoJLMIDisb.Tranaction_Date,
                0, "", "", "", null, null, 0, dtoJLMIDisb.Rate_Of_Interest, dtoJLMIDisb.Penal_Rate, dtoJLMIDisb.Tranaction_Date,
                0, false, false, false, vocId, Checked_By, yrId, 0, 0, brCode, 0, 0, 0);
            (result, jlMILoanId, jlMILoanNo) = await _unitOfWork.LoanMaster.AddLoanMasterAsync(jlMIMaster);

            jlMITrn = Utility.GetModalObject.GetLoanTrnObject(jlMILoanId, 0, "I", dtoJLMIDisb.Tranaction_Date, null,
                dtoJLMIDisb.Tranaction_Date, dtoJLMIDisb.Loan_Amount, 0, 0, 0, null, 0, null, 0, null, 0, 0, 0, 0, 0, 0, 0,
                null, 0, null, 0, null, dtoJLMIDisb.Rate_Of_Interest, dtoJLMIDisb.Penal_Rate, 0, 0, false, false, vocId,
                Checked_By, yrId, 1, false, dtoJLMIDisb.Loan_Amount, 0, 0, 0, 0, brCode);


            jlMIDisb = Utility.GetModalObject.GetLoanDisbursementObject(jlMILoanId, dtoJLMIDisb.Tranaction_Date, 1,
                dtoJLMIDisb.Loan_Amount, dtoJLMIDisb.Tranaction_Date, null, "", null, 0, null, "", null, false, 1, true, false,
                vocId, Checked_By, yrId, brCode);

            jlMIRoi = Utility.GetModalObject.GetLoanROIObject(jlMILoanId, "S", dtoJLMIDisb.Tranaction_Date, dtoJLMIDisb.Rate_Of_Interest,
                dtoJLMIDisb.Penal_Rate, 0, 0, false, false, vocId, Checked_By, yrId, brCode);

            jlMIDetails = Utility.GetModalObject.GetJLDetails(0, jlMILoanId, dtoJLMIDisb.Due_Date, dtoJLMIDisb.Govt_Rate, dtoJLMIDisb.Gross_Weight,
                dtoJLMIDisb.Wasgate, dtoJLMIDisb.Net_Weight, dtoJLMIDisb.Net_Weight, dtoJLMIDisb.Market_Rate, dtoJLMIDisb.Percentage_Of_Eligibility_On_Market_Rate,
                dtoJLMIDisb.Jewels_Photo_Path!, false, false, vocId, Checked_By, yrId, brCode);

            foreach (var orn in dtoJLMIDisb.Ornment_List!)
            {
                JL_Ornments jlOrn = new JL_Ornments
                {
                    JLO_Id = 0,
                    Loan_Id = jlMILoanId,
                    JLO_Nos = orn.Ornment_Nos,
                    JLO_Name = orn.Ornment_Name,
                    JLO_OE = false,
                    JLO_Delete = false,
                    Voc_Id = vocId,
                    Yr_Id = yrId,
                    BrCode = brCode,
                };
                MIornmentList.Add(jlOrn);
            }
            Map_General mapMI = await _unitOfWork.MapGeneral.GetMapGeneralAsync(brCode);
            if (dtoJLMIDisb.CashOrAdjustment == 1)
            {
                vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, jlMISchemes.PrlLed_Id, 0, trns.CashPayment_Amount,
                dtoJLMIDisb.CashOrAdjustment, Narration + " Loan No : " + jlMILoanNo, false, Checked_By, yrId, Status, "Loan No :" + jlMILoanNo,
                dtoJLMIDisb.Mem_Id, brCode, jlMILoanId, dtoJLMIDisb.Loan_Amount, 0);
                finVoucherTrns.Add(vocTrn);
                vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, jlMISchemes.PrlLed_Id, 0, trns.AdjustmentPayment_Amount,
                2, Narration + " Loan No : " + jlMILoanNo, false, Checked_By, yrId, Status, "Loan No :" + jlMILoanNo,
                dtoJLMIDisb.Mem_Id, brCode, jlMILoanId, dtoJLMIDisb.Loan_Amount, 0);
                finVoucherTrns.Add(vocTrn);
                if (dtoJLMIDisb.AppraisalFee > 0)
                {
                    vocTrn = new();
                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, mapMI.Appraisal_Fee_Led_Id, dtoJLMIDisb.AppraisalFee, 0, 2,
                        Narration + " Loan No :" + jlMILoanNo, false, Checked_By, yrId, Status, "Loan No: " + jlMILoanNo, dtoJLMIDisb.Mem_Id, brCode,
                        jlMILoanId, dtoJLMIDisb.Loan_Amount, 0);
                    finVoucherTrns.Add(vocTrn);
                }
                if (dtoJLMIDisb.BankCharges > 0)
                {
                    vocTrn = new();
                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, mapMI.Bank_Charges_Led_Id, dtoJLMIDisb.BankCharges,
                        0, 2, Narration + " Loan No :" + jlMILoanNo, false, Checked_By, yrId, Status, "Loan No: " + jlMILoanNo,
                        dtoJLMIDisb.Mem_Id, brCode, jlMILoanId, dtoJLMIDisb.Loan_Amount, 0);
                    finVoucherTrns.Add(vocTrn);
                }
            }
            else
            {
                vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, jlMISchemes.PrlLed_Id, 0, dtoJLMIDisb.Loan_Amount,
                    dtoJLMIDisb.CashOrAdjustment, Narration + " Loan No : " + jlMILoanNo, false, Checked_By, yrId, Status, "Loan No :" + jlMILoanNo,
                    dtoJLMIDisb.Mem_Id, brCode, jlMILoanId, dtoJLMIDisb.Loan_Amount, 0);
                finVoucherTrns.Add(vocTrn);

                //Map_General map = await _unitOfWork.MapGeneral.GetMapGeneralAsync(brCode);
                if (dtoJLMIDisb.AppraisalFee > 0)
                {
                    vocTrn = new();
                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, mapMI.Appraisal_Fee_Led_Id, dtoJLMIDisb.AppraisalFee, 0, dtoJLMIDisb.CashOrAdjustment,
                        Narration + " Loan No :" + jlMILoanNo, false, Checked_By, yrId, Status, "Loan No: " + jlMILoanNo, dtoJLMIDisb.Mem_Id, brCode,
                        jlMILoanId, dtoJLMIDisb.Loan_Amount, 0);
                    finVoucherTrns.Add(vocTrn);
                }
                if (dtoJLMIDisb.BankCharges > 0)
                {
                    vocTrn = new();
                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, mapMI.Bank_Charges_Led_Id, dtoJLMIDisb.BankCharges,
                        0, dtoJLMIDisb.CashOrAdjustment, Narration + " Loan No :" + jlMILoanNo, false, Checked_By, yrId, Status, "Loan No: " + jlMILoanNo,
                        dtoJLMIDisb.Mem_Id, brCode, jlMILoanId, dtoJLMIDisb.Loan_Amount, 0);
                    finVoucherTrns.Add(vocTrn);
                }
            }

            if (trns.Related_Account_Id == 48)
            {
                List<Loan_Repayment_Schedule> scheduleList = new();
                Loan_Repayment_Schedule schedule = new();
                foreach (var item in dtoJLMIDisb.RepaymentSchedule!)
                {
                    schedule = new();
                    schedule = Utility.GetModalObject.GetLoanRepaymentSchedule(0, jlMILoanId, "D", item.Due_Date, item.Principal_Demand, item.Interest_Demand, item.Loan_Outstanding, Checked_By, yrId, vocId, brCode);
                    scheduleList.Add(schedule);
                }
                result = await _unitOfWork.LoanRepaymentSchedule.AddLoanRepaymentScheduleAsync(scheduleList);
            }

            result = await _unitOfWork.LoanTrn.AddLoanTrn(jlMITrn);
            result = await _unitOfWork.LoanDisbursement.AddLoanDisbursementAsync(jlMIDisb);
            result = await _unitOfWork.LoanROI.AddLoanROIAsync(jlMIRoi);
            result = await _unitOfWork.JLDetails.AddJLDetailsAsync(jlMIDetails);
            result = await _unitOfWork.JLOrnment.AddJLOrnmentListAsync(MIornmentList);

            return (result, finVoucherTrns);
            #endregion
        }

        private async Task<(bool result, List<Fin_Voucher_Trn> finVoucherTrns)> SaveLongTermDisbursement(DtoTransaction trns, decimal vocId, decimal Checked_By,
            decimal yrId, string brCode, string Status)
        {
            List<Fin_Voucher_Trn> finVoucherTrns = new();
            bool result = false;
            string Narration = "";
            decimal loanId = 0;
            string loanNo = "";
            Loan_Master loanMaster = new();
            Loan_Trn loanTrn = new();
            Loan_Disb loanDisb = new();
            Loan_Inst lnInstalment = new Loan_Inst();
            Loan_Roi loanRoi = new();
            Fin_Voucher_Trn vocTrn = new();
            Loan_Schemes Schemes = new();
            DtoLoanDisbursementLT DtoLtDisb = new();
            DtoLtDisb = Utility.JsonbObject.ConvertFromJsonForLTDisbursement(trns.Related_Account_Data!);
            Narration = DtoLtDisb.Member_No!.Trim() + " " + DtoLtDisb.Member_Name!.Trim();
            Schemes = await _unitOfWork.LoanScheme.GetLoanSchemesAsync(DtoLtDisb.Scheme_Id, brCode);

            loanMaster = Utility.GetModalObject.GetLoanMasterObject(DtoLtDisb.Scheme_Id, "", DtoLtDisb.Mem_Id, 0, "",
                DtoLtDisb.Tranaction_Date, "", null, DtoLtDisb.DisbursementAmount, DtoLtDisb.Tranaction_Date, 6, 0, 0, 0, 0,
                DtoLtDisb.Period_Of_Loan, DtoLtDisb.Period_Of_Loan, DtoLtDisb.Tranaction_Date, DtoLtDisb.Tranaction_Date,
                0, "", "", "", null, null, 0, DtoLtDisb.Rate_Of_Interest, DtoLtDisb.Penal_Rate, DtoLtDisb.Tranaction_Date,
                0, false, false, false, vocId, Checked_By, yrId, 0, 0, brCode, 0, 0, 0);
            (result, loanId, loanNo) = await _unitOfWork.LoanMaster.AddLoanMasterAsync(loanMaster);

            loanTrn = Utility.GetModalObject.GetLoanTrnObject(loanId, 0, "I", DtoLtDisb.Tranaction_Date, null,
                DtoLtDisb.Tranaction_Date, DtoLtDisb.DisbursementAmount, 0, 0, 0, null, 0, null, 0, null, 0, 0, 0, 0, 0, 0, 0,
                null, 0, null, 0, null, DtoLtDisb.Rate_Of_Interest, DtoLtDisb.Penal_Rate, 0, 0, false, false, vocId,
                Checked_By, yrId, 1, false, DtoLtDisb.DisbursementAmount, 0, 0, 0, 0, brCode);

            lnInstalment = Utility.GetModalObject.GetLoanInstalmentObject( loanId, DtoLtDisb.Tranaction_Date,
                DtoLtDisb.InstalmentAmount, false,false,vocId, Checked_By , yrId, brCode );

            loanDisb = Utility.GetModalObject.GetLoanDisbursementObject(loanId, DtoLtDisb.Tranaction_Date, 1,
                DtoLtDisb.DisbursementAmount, DtoLtDisb.Tranaction_Date, null, "", null, 0, null, "", null, false, 1, true, false,
                vocId, Checked_By, yrId, brCode);

            loanRoi = Utility.GetModalObject.GetLoanROIObject(loanId, "S", DtoLtDisb.Tranaction_Date, DtoLtDisb.Rate_Of_Interest,
                DtoLtDisb.Penal_Rate, 0, 0, false, false, vocId, Checked_By, yrId, brCode);
            List<Loan_Members> membersList = new();
            Loan_Members member = new();
            int slno = 1;
            foreach(var mem in DtoLtDisb.LoanMembersList!.OrderBy(x=> x.Status))
            {
                member = Utility.GetModalObject.GetLoanMembersObject(0, loanId, mem.Mem_Id, slno, mem.Status, 0, false, vocId, Checked_By, yrId, brCode!);
                membersList.Add(member);
            }

            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, Schemes.PrlLed_Id, 0, DtoLtDisb.DisbursementAmount,
                DtoLtDisb.CashOrAdjustment, Narration + " Loan No : " + loanNo, false, Checked_By, yrId, Status, "Loan No :" + loanNo,
                DtoLtDisb.Mem_Id, brCode, loanId, DtoLtDisb.DisbursementAmount, 0);
            finVoucherTrns.Add(vocTrn);

            List<Loan_Repayment_Schedule> scheduleList = new();
            Loan_Repayment_Schedule schedule = new();
            foreach (var item in DtoLtDisb.RepaymentSchedule!)
            {
                schedule = new();
                schedule = Utility.GetModalObject.GetLoanRepaymentSchedule(0, loanId, "D", item.Due_Date, item.Principal_Demand, item.Interest_Demand, item.Loan_Outstanding, Checked_By, yrId, vocId, brCode);
                scheduleList.Add(schedule);
            }
            
            result = await _unitOfWork.LoanTrn.AddLoanTrn(loanTrn);
            result = await _unitOfWork.LoanDisbursement.AddLoanDisbursementAsync(loanDisb);
            result = await _unitOfWork.LoanInstalment.AddLoanInstalmentAsync(lnInstalment);
            result = await _unitOfWork.LoanROI.AddLoanROIAsync(loanRoi);
            result = await _unitOfWork.LoanRepaymentSchedule.AddLoanRepaymentScheduleAsync(scheduleList);
            result = await _unitOfWork.LoanMember.AddLoanMemberAsync(membersList);
            return (result, finVoucherTrns);
        }

        public async Task<DtoVoucher> SaveAccountTransaction(decimal stagingId, string vocMode, decimal Checked_By, decimal yrId)
        {
            bool result = false;
            int cashOrAdj = 0;
            string brCode = "";
            string Status = "G";

            decimal vocId = 0;
            DateTime Transacted_Date;
            bool IsChequeOnly = false;
            double vocAmt = 0, receiptAmount = 0, paymentAmount = 0, cashReceipt = 0, cashPayment = 0, adjReceipt = 0, adjPayment = 0;
            DtoVoucher voucherData = new();
            List<DtoTransaction> transactions = new();
            DtoTransactionRptPmtNos rptPmtNo = new();
            List<Fin_Voucher_Trn> finVoucherTrns = new();
            Fin_Voucher_Trn vocTrn = new();
            DtoAccountTransactionRelatedData relatedData = new();
            try
            {
                _unitOfWork.BeginTransaction();
                transactions = await _unitOfWork.TransactionsRepository.GetStagingDataByStagingId(stagingId);
                if (transactions == null || transactions.Count == 0)
                {
                    return voucherData;
                }
                cashOrAdj = transactions.Select(x => x.Cash_Or_Adjustment).First();
                brCode = transactions.Select(x => x.BrCode!).First();
                voucherData.brCode = brCode;
                Transacted_Date = transactions.Select(x => x.Transacted_Date).First();
                receiptAmount = transactions.Sum(x => x.Receipt_Amount);
                paymentAmount = transactions.Sum(x => x.Payment_Amount);

                if (cashOrAdj == 1)
                {
                    cashReceipt = receiptAmount;
                    cashPayment = paymentAmount;
                }
                else
                {
                    adjReceipt = receiptAmount;
                    adjPayment = paymentAmount;
                }
                if (receiptAmount > 0) vocAmt = receiptAmount;
                if (paymentAmount > 0) vocAmt = paymentAmount;
                IsChequeOnly = await _unitOfWork.TransactionsRepository.IsChequeOnly(stagingId);
                rptPmtNo = _unitOfWork.TransactionsRepository.GetReceiptAndPaymentNo(cashReceipt, cashPayment, adjReceipt, adjPayment, IsChequeOnly, yrId);
                if (rptPmtNo == null)
                {
                    //throw new Exception("Failed to generate receipt and payment numbers.");
                    return voucherData;
                }


                Fin_Voucher voc = Utility.GetModalObject.GetFinVoucherObject(vocId, "", 0, Transacted_Date, 5, vocMode, vocAmt, true,
                                rptPmtNo.Voc_Rpt_SlNo, rptPmtNo.Voc_Rpt_No, rptPmtNo.Voc_Rpt_Mode, rptPmtNo.Voc_Pmt_SlNo, rptPmtNo.Voc_Pmt_No, rptPmtNo.Voc_Pmt_Mode,
                                 Checked_By, yrId, brCode, 0);
                (result, vocId) = await _unitOfWork.FinVoucher.AddFinVoucherAsync(voc);
                voucherData.Voc_Id = vocId;
                Map_General mapGeneral = await _unitOfWork.MapGeneral.GetMapGeneralAsync(brCode);

                /// cash transactions
                if (cashPayment > 0 || cashReceipt > 0)
                {
                    vocTrn = new();
                    relatedData = Utility.JsonbObject.ConvertFromJsonForAccountTransactionRelatedData(transactions.First().Related_Account_Data!);
                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, mapGeneral.Cash_Led_Id, cashReceipt, cashPayment, 1, relatedData.Narration!, false, Checked_By, yrId, "C", "", 0, brCode, 0, 0, 0);
                    finVoucherTrns.Add(vocTrn);
                }

                // Batch-fetch all bank ledger IDs upfront
                var allLedgerIds = transactions.Select(t => t.Ledger_Id).Distinct().ToList();
                var bankLedgerIds = await _unitOfWork.Accounts.GetBankLedgerIds(allLedgerIds, brCode);

                foreach (var trns in transactions)
                {
                    relatedData = Utility.JsonbObject.ConvertFromJsonForAccountTransactionRelatedData(trns.Related_Account_Data!);
                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, trns.Ledger_Id, trns.Receipt_Amount, trns.Payment_Amount, trns.Cash_Or_Adjustment, relatedData.Narration!, false, Checked_By, yrId, Status, "", 0, brCode, 0, 0, 0);
                    finVoucherTrns.Add(vocTrn);
                    /// vefify ledger_id is bank account
                    if (bankLedgerIds.Contains(trns.Ledger_Id))
                    {
                        /// if bank account, then insert the bank transaction
                        Fin_Voucher_Bank bankTrn = new();
                        double amount = 0;
                        if (trns.Receipt_Amount > 0)
                        {
                            amount = trns.Receipt_Amount;

                        }
                        if (trns.Payment_Amount > 0)
                        {
                            amount = trns.Payment_Amount;
                        }
                        bankTrn = Utility.GetModalObject.GetFinVocBankObject(Transacted_Date, 0, trns.Receipt_Amount > 0 ? "C" : "O", trns.Issue_Bank_Name!, vocId, trns.Ledger_Id, trns.Receipt_Amount > 0 ? trns.Receipt_Amount : trns.Payment_Amount, trns.Cheque_No!, trns.Cheque_Date, null, 0, false, null, "", null, 0, Checked_By, yrId, false, brCode);
                        result = await _unitOfWork.FinVoucherBank.AddFinVoucherBankAsync(bankTrn);
                    }
                }
                /// Insert all the voucher transactions
                result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns);
                /// update staging_master with checked by and checked date
                result = await _unitOfWork.StagingMaster.CheckerStateStaging(stagingId, vocId, Checked_By, "V");
                result = await _unitOfWork.StagingDetails.CheckerStateStaging(stagingId, vocId, Checked_By, "V");
                result = true;
                _unitOfWork.Complete();
                _unitOfWork.CommitTransaction();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                Console.Write(ex.Message + " " + ex.StackTrace);
                _unitOfWork.RollBack();
            }
            return voucherData;
        }
        public async Task<bool> RejectTransaction(decimal stagingId, decimal Checked_By)
        {
            bool result = false;
            try
            {
                _unitOfWork.BeginTransaction();
                /// update staging_master with checked by and checked date
                result = await _unitOfWork.StagingMaster.CheckerStateStaging(stagingId, 0, Checked_By, "R");
                result = await _unitOfWork.StagingDetails.CheckerStateStaging(stagingId, 0, Checked_By, "R");
                _unitOfWork.Complete();
                _unitOfWork.CommitTransaction();
                result = true;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message + " " + ex.StackTrace);
                result = false;
                _unitOfWork.RollBack();
            }
            return result;
        }
        public async Task<List<DtoTransaction>> GetStagingDataByStagingId(decimal stagingId)
        {
            return await _unitOfWork.TransactionsRepository.GetStagingDataByStagingId(stagingId);
        }
        public DtoTransactionRptPmtNos GetReceiptAndPaymentNo(double cashReceipt, double cashPayment, double adjReceipt, double adjPayment, bool IsChequeOnly, decimal yrId)
        {
            return _unitOfWork.TransactionsRepository.GetReceiptAndPaymentNo(cashReceipt, cashPayment, adjReceipt, adjPayment, IsChequeOnly, yrId);
        }
        public async Task<bool> IsChequeOnly(decimal stagingId)
        {
            return await _unitOfWork.TransactionsRepository.IsChequeOnly(stagingId);
        }
        public async Task<string> GetTransactionStatusByAccId(int accId)
        {
            return await _unitOfWork.TransactionsRepository.GetTransactionStatusByAccId((int)accId);
        }

        public async Task<List<DropdownItem>> GetLedgerItems(int fnlId, string brCode)
        {
            decimal cashLedId = 0;
            List<DropdownItem> ledList = new List<DropdownItem>();
            try
            {
                cashLedId = await _unitOfWork.Accounts.GetCashLedgerId(brCode);
                ledList = await _unitOfWork.TransactionsRepository.GetLedgerItems(fnlId, cashLedId, brCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ledList = new List<DropdownItem>();
            }
            return ledList ;
        }

        public async Task<List<DropdownItem>> GetPrincipalLedgerItems(string brCode)
        {
            return await _unitOfWork.TransactionsRepository.GetPrincipalLedgerItems(brCode);
        }
    }
}