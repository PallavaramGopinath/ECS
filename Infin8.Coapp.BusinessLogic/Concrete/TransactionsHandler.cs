using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
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
        public async Task<bool> SaveTransaction(decimal stagingId, string vocMode, decimal Checked_By, decimal yrId)
        {
            bool result = false;
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
                    return false;
                }

                cashOrAdj = transactions.Select(x => x.Cash_Or_Adjustment).First();
                brCode = transactions.Select(x => x.BrCode!).First();
                //Checked_By = transactions.Select(x => x.Checked_By).First();
                Transacted_Member_Id = transactions.Select(x => x.Transacted_Member_Id).First();
                var mem = await _unitOfWork.Members.GetMemberDetailsByMemIdAsync(Transacted_Member_Id);
                if(mem !=null)
                {
                    Transacted_MemNo = mem.MemberNo!;
                    Transacted_MemName = mem.MemberName!;
                }
                Transacted_Date = transactions.Select(x => x.Transacted_Date).First();
                receiptAmount = transactions.Select(x => x.Receipt_Amount).First();
                paymentAmount = transactions.Select(x => x.Payment_Amount).First();

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
                if(rptPmtNo == null)
                {
                    //throw new Exception("Failed to generate receipt and payment numbers.");
                    return false;
                }


                Fin_Voucher voc = Utility.GetModalObject.GetFinVoucherObject(vocId, "", 0, Transacted_Date, 5, vocMode, vocAmt, true,
                                rptPmtNo.Voc_Rpt_SlNo, rptPmtNo.Voc_Rpt_No, rptPmtNo.Voc_Rpt_Mode, rptPmtNo.Voc_Pmt_SlNo, rptPmtNo.Voc_Pmt_No, rptPmtNo.Voc_Pmt_Mode,
                                 Checked_By, yrId, brCode, Transacted_Member_Id);
                (result, vocId) = await _unitOfWork.FinVoucher.AddFinVoucherAsync(voc);
                Map_General mapGeneral = await _unitOfWork.MapGeneral.GetMapGeneralAsync(brCode);

                /// cash transactions
                if(cashPayment >0 || cashReceipt >0)
                {
                    vocTrn = new();
                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, mapGeneral.Cash_Led_Id, cashReceipt, cashPayment, 1, Transacted_MemNo.Trim() + Transacted_MemName.Trim(), false, Checked_By, yrId, "C", "", Transacted_Member_Id, brCode, 0, 0, 0);
                    finVoucherTrns.Add(vocTrn);
                }
                foreach (var trns in transactions)
                {
                    trns.Checked_By = Checked_By;
                    Status = await _unitOfWork.TransactionsRepository.GetTransactionStatusByAccId(trns.Related_Account_Id);
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
                            DtoOtherRelatedData otherRelatedData = new();
                            otherRelatedData = Utility.JsonbObject.ConvertFromJsonForOtherRelatedData(trns.Related_Account_Data!);
                            Narration = otherRelatedData.Member_No!.Trim() + " " + otherRelatedData.Member_Name!.Trim();
                            scTrn = Utility.GetModalObject.GetMemTrnObject(3, trns.Account_Holder_Member_Id, trns.Ledger_Id, Transacted_Date, trns.Receipt_Amount, trns.Payment_Amount, false, false, vocId, Checked_By, yrId, 0, 0, null, 0, "", 0, 0, 0, brCode);
                            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, trns.Ledger_Id, trns.Receipt_Amount, trns.Payment_Amount, trns.Cash_Or_Adjustment , Narration, false, Checked_By, yrId, Status, "", trns.Transacted_Member_Id, brCode, 0, 0, 0);
                            finVoucherTrns.Add(vocTrn);
                            result = await _unitOfWork.MemTrn.AddMemTrnAsync(scTrn);
                            result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns);
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
                            result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns);
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
                            if (trns.Cash_Or_Adjustment == 1)
                            {
                                if (totalIntPayment < 0)
                                {
                                    vocTrn = new();
                                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, mapGeneral.FD_Led_Id, 0, fdRefund.Net_Payable, 1, Narration + " FD No : " + fdRefund.Fixed_Deposit_Datas.Select(x => x.FD_No).First(), false, Checked_By, yrId, Status, "TD No: " + fdRefund.Fixed_Deposit_Datas.Select(x => x.FD_No).First(), fdRefund.Mem_Id, brCode, 0, 0, 0);
                                    finVoucherTrns.Add(vocTrn);
                                    vocTrn = new();
                                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, mapGeneral.FD_Led_Id, 0, fdRefund.Total_Deposit_Payable - fdRefund.Net_Payable, 2, Narration + " FD No : " + fdRefund.Fixed_Deposit_Datas.Select(x => x.FD_No).First(), false, Checked_By, yrId, Status, "FD No :" + fdRefund.Fixed_Deposit_Datas.Select(x => x.FD_No).First(), fdRefund.Mem_Id,brCode, 0, 0, 0);
                                    finVoucherTrns.Add(vocTrn);
                                    vocTrn = new();
                                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, mapGeneral.FD_Excess_IntPaid_Id, Math.Abs(totalIntPayment), 0, 2, Narration + " FD No : " + fdRefund.Fixed_Deposit_Datas.Select(x => x.FD_No).First(), false, Checked_By, yrId, Status, fdRefund.Fixed_Deposit_Datas.Select(x => x.FD_No).First(), fdRefund.Mem_Id, brCode, 0, 0, 0);
                                    finVoucherTrns.Add(vocTrn);
                                }
                            }
                            else
                            {
                                vocTrn = new();
                                vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, mapGeneral.FD_Led_Id, 0, fdRefund.Total_Deposit_Payable, fdRefund.CashOrAdjustment, Narration + " FD No :" + fdRefund.Fixed_Deposit_Datas.Select(x => x.FD_No).First(), false, Checked_By, yrId, Status, "TD No: " + fdRefund.Fixed_Deposit_Datas.Select(x => x.FD_No).First(), fdRefund.Mem_Id,brCode, 0, 0, 0);
                                finVoucherTrns.Add(vocTrn);

                                vocTrn = new();
                                vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, mapGeneral.FD_Int_Led_Id, 0, fdRefund.Total_Interest_Payable, fdRefund.CashOrAdjustment, Narration + " FD No :" + fdRefund.Fixed_Deposit_Datas.Select(x => x.FD_No).First(), false, Checked_By, yrId, Status, "TD No: " + fdRefund.Fixed_Deposit_Datas.Select(x => x.FD_No).First(), fdRefund.Mem_Id,brCode, 0, 0, 0);
                                finVoucherTrns.Add(vocTrn);
                            }
                            #endregion

                            #region Loan on FD Receipt
                            if (fdRefund.Total_Loan_Interest_Receipt + fdRefund.Total_Loan_Principal_Receipt > 0)
                            {
                                Loan_Schemes fdLoanScheme2 = await _unitOfWork.LoanScheme.GetLoanSchemeByType(3);
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
                            result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns);
                            /// update fd refund as closed
                            foreach (var fd in fdRefund.Fixed_Deposit_Datas)
                            {
                                result = await _unitOfWork.TermDepositMaster.UpdateTermDepositMasterAsClosed(fd.FD_Id);
                            }
                            #endregion 
                            break;
                        case 9: /// FD Renewal
                            #region FD Renewal
                            decimal newFDIdForRenewal = 0;
                            string newFDNoForRenewal = "";
                            decimal renewalTDId = 0;
                            string? renewalTDNo = "";
                            List<TermDeposit_Trn> fdRenewalTrnList = new();
                            TermDeposit_Trn fdRenewalTrn = new();
                            List<TermDeposit_Members> fdRenewalMembers = new();
                            TermDeposit_Members fdRenewalMem = new();
                            //List<Fin_Voucher_Trn> fdRenewalVocTrnList = new();
                            //Fin_Voucher_Trn fdRenewalVocTrn = new();
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
                                fdRenewal.FixedDepositCreate.Common.Nominee2Relationship!, false, false, false, vocId, Checked_By, yrId, "N", renewalTDId, renewalTDNo!,brCode);

                            (result, newFDIdForRenewal, newFDNoForRenewal) = await _unitOfWork.TermDepositMaster.AddTermDepositMasterAsync(newFDForRenewal);
                            if (!result)
                            {
                                return false;
                            }
                            fdRenewalTrn = Utility.GetModalObject.GetTermDepositTrn(0, fdRenewal.Transaction_Date, newFDIdForRenewal, 0,
                                fdRenewal.FixedDepositCreate.Deposit_Amount, fdRenewal.FixedDepositCreate.Maturity_Amount, 0, null, 0, 0, 0, null, 0, 0, 0, null, null, false, false,
                                vocId, Checked_By, yrId, 1, 0,brCode);
                            fdRenewalTrnList.Add(fdRenewalTrn);
                            int memSlNo = 1;
                            foreach (var memRem in fdRenewal.FixedDepositCreate.Members)
                            {
                                fdRenewalMem = Utility.GetModalObject.GetTermDepositMembers(0, newFDIdForRenewal, memRem.Mem_Id, memSlNo, false, vocId, Checked_By, yrId,brCode);
                                fdRenewalMembers.Add(fdRenewalMem);
                                memSlNo++;
                            }
                            vocTrn = new();
                            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, tdScheme.Led_Id, fdRenewal.FixedDepositCreate.Deposit_Amount, 0, fdRenewal.FixedDepositCreate.CashorAdjustment,
                                Narration, false, Checked_By, yrId, Status, "FD No: " + newFDNoForRenewal, 0,brCode, 0, 0, 0);
                            finVoucherTrns.Add(vocTrn);
                            #endregion

                            #region FD Renewal Payment
                            /// FD payment for renewal
                            foreach (var fd in fdRenewal.FixedDepositPayment.Fixed_Deposit_Datas!)
                            {
                                fdRenewalTrn = Utility.GetModalObject.GetTermDepositTrn(0, fdRenewal.Transaction_Date, fd.FD_Id, 0, 0, 0, fd.Current_Interest_Calculated, fd.Current_Interest_Applied_Date,
                                    fd.Total_Interest_Payable, fd.Deposit_Refund, 0, null, 0, 0, 0, null, null, false, false,
                                    vocId, Checked_By, yrId, 0, 0,brCode);
                                fdRenewalTrnList.Add(fdRenewalTrn);
                                vocTrn = new();
                                vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, tdScheme.Led_Id, 0, fd.Deposit_Refund, fdRenewal.CashOrAdjustment,
                                    Narration + " FD No : " +fd.FD_No , false, Checked_By, yrId, Status, "FD No: " + fd.FD_No, fdRenewal.Mem_Id, brCode, newFDIdForRenewal, 0, 0);
                            }
                            #endregion

                            #region FD Loan Receipt if any
                            foreach (var loan in fdRenewal.FixedDepositPayment.Loan_On_FixedDeposits!)
                            {
                                fdRenewalLoanTrn = Utility.GetModalObject.GetLoanTrnObject(loan.Loan_Id, 0, "R", fdRenewal.Transaction_Date, null, null, 0, 0, 0, 0, null, 0, null, loan.Current_Interest, loan.Current_IntCalc_Date, 0, 0, loan.Interest_Balance, loan.Principal_Balance, 0, 0, 0, null, 0, null, 0, null, 0, 0, 0, 0, false, false, vocId, Checked_By, yrId, 0, false, 0, 0, 0, 0, 0,brCode);
                                fdRenewalLoanTrnList.Add(fdRenewalLoanTrn);
                            }
                            #endregion 

                            #region Save FD Renewal
                            result = await _unitOfWork.TermDepositMember.AddTermDepositMemberListAsync(fdRenewalMembers);
                            result = await _unitOfWork.TermDepositTrn.AddTermDepositTrnListAsync(fdRenewalTrnList);
                            result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns);
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
                            Narration = chequeData.Member_No!.Trim() + " " + chequeData.Member_Name!.Trim();
                            finVocBank = Utility.GetModalObject.GetFinVocBankObject(trns.Transacted_Date, chequeData.Member_Id, trns.Receipt_Amount > 0 ? "C" : "O", chequeData.Issue_Bank_Name!, vocId, trns.Ledger_Id, trns.Receipt_Amount > 0 ? trns.Receipt_Amount : trns.Payment_Amount, chequeData.Cheque_No!, chequeData.Cheque_Date!, null, 0, false, null, "", null, 0, Checked_By, yrId, false, brCode);
                            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, trns.Ledger_Id, trns.Receipt_Amount, trns.Payment_Amount,trns.Cash_Or_Adjustment , Narration, false, Checked_By, yrId, Status, "", trns.Transacted_Member_Id, brCode, 0, 0, 0);
                            finVoucherTrns.Add(vocTrn);
                            result = await _unitOfWork.FinVoucherBank.AddFinVoucherBankAsync(finVocBank);
                            result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns);
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
                            Loan_Schemes fdLoanScheme = await _unitOfWork.LoanScheme.GetLoanSchemeByType(3);
                            faceValue = fdLoan.FD_Datas!.Select(x => x.FD_Amount).Sum();
                            drawingPower = fdLoan.FD_Datas!.Select(x => x.Drawing_Power).Sum();
                            //firstFDNo = fdLoan.FD_Datas!.Select(x => x.FD_No).FirstOrDefault() ?? "";
                            fdLoanMaster = Utility.GetModalObject.GetLoanMasterObject(fdLoanScheme.Scheme_Id, "", fdLoan.Mem_Id, 0, "", null, "", null, fdLoan.Loan_Amount, fdLoan.Transaction_Date, 3, 0, 0, 0, 0, 0, 0, fdLoan.Transaction_Date, fdLoan.Transaction_Date, 0, "", "", "", null, null, 0, fdLoan.Loan_Rate_Of_Interest, 0, fdLoan.Transaction_Date, 0, false, false, false, vocId, Checked_By, yrId,faceValue,drawingPower ,brCode, 0, 0, 1);
                            (result, fdLoanId, fdLoanNo) = await _unitOfWork.LoanMaster.AddLoanMasterAsync(fdLoanMaster);

                            fdLoanTrn = Utility.GetModalObject.GetLoanTrnObject(fdLoanId, 0, "I", fdLoan.Transaction_Date, null, null, 
                                fdLoan.Loan_Amount, 0, 0, 0, null, 0, null, 0,null, 0, 0, 0, 0, 0, 0,0, null, 0, null, 0, null, 
                                fdLoan.Loan_Rate_Of_Interest,0, 0, 0, false, false,vocId, Checked_By, yrId, 1, false, fdLoan.Loan_Amount , 0,0 , 0,0,brCode);
                            fdLoanDisb = Utility.GetModalObject.GetLoanDisbursementObject(fdLoanId, fdLoan.Transaction_Date, 1,fdLoan.Loan_Amount, fdLoan.Transaction_Date,null,"",null,0,null,"",null,false,1,true,false,vocId,Checked_By,yrId,brCode);
                            fdLoanRoi = Utility.GetModalObject.GetLoanROIObject(fdLoanId,"S",fdLoan.Transaction_Date, fdLoan.Loan_Rate_Of_Interest,0, 0, 0, false, false, vocId, Checked_By, yrId,brCode);
                            lien = Utility.GetModalObject.GetLienObject(fdLoanId, faceValue, fdLoan.Loan_Amount ,0,  fdLoan.FD_Datas!.Select(x => x.FD_No).FirstOrDefault() ?? "", fdLoan.FD_Datas!.Select(x => x.FD_Id).FirstOrDefault(),false, false,vocId, Checked_By, yrId,brCode);
                            foreach(var fd in fdLoan.FD_Datas!)
                            {
                                lienTrn = new();
                                lienTrn = Utility.GetModalObject.GetLienTrObject(0, fdLoanId, fd.FD_Id, fd.FD_Amount, fd.Drawing_Power,fd.Loan_Amount,0, false, false, vocId, Checked_By, yrId, brCode);
                                lienTrns.Add(lienTrn);
                            }
                            vocTrn = new();
                            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, fdLoanScheme.PrlLed_Id,0, fdLoan.Loan_Amount, fdLoan.CashOrAdjustment,
                                Narration, false, Checked_By, yrId, Status, "Loan No: " + fdLoanNo, fdLoan.Mem_Id,brCode, fdLoanId, fdLoan.Loan_Amount , 0);
                            finVoucherTrns.Add(vocTrn);
                            result = await _unitOfWork.LoanTrn.AddLoanTrn (fdLoanTrn);
                            result = await _unitOfWork.LoanDisbursement.AddLoanDisbursementAsync(fdLoanDisb);
                            result = await _unitOfWork.LoanROI.AddLoanROIAsync(fdLoanRoi);
                            result = await _unitOfWork.Lien.AddLienAsync(lien);
                            result = await _unitOfWork.LienTrn.AddLienTrnListAsync(lienTrns);
                            result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns); 
                            #endregion
                            break;
                        case 14: /// RD Loan Disbursement
                            break;
                        case 15:    /// Loan to staff
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
                                yrId, "N", 0, "",brCode);
                            (result, newTDId, newTDNo) = await _unitOfWork.TermDepositMaster.AddTermDepositMasterAsync(tdMasterNew);
                            if (!result)
                            {
                                return false;
                            }
                            tdTrnNew = Utility.GetModalObject.GetTermDepositTrn(0, newFD.Common.Account_Opendate, newTDId, 0,
                                newFD.Deposit_Amount, newFD.Maturity_Amount, 0, null, 0, 0, 0, null, 0, 0, 0, null, null, false, false,
                                vocId, Checked_By, yrId, 1, 0,brCode);

                            foreach (var memNew in newFD.Members)
                            {
                                TermDeposit_Members tdMem = new();
                                tdMem = Utility.GetModalObject.GetTermDepositMembers(0, newTDId, memNew.Mem_Id, 1, false, vocId, Checked_By, yrId,brCode);
                                tdMembers.Add(tdMem);
                            }
                            TermDeposit_Schemes tdNewScheme = await _unitOfWork.TermDepositScheme.GetTermDepositSchemeByIdAsync(newFD.Common.Tdscheme_Id);
                            vocTrn = new();
                            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, tdNewScheme.Led_Id, newFD.Deposit_Amount, 0, newFD.CashorAdjustment,
                                Narration + "FD No : " + newTDNo, false, Checked_By, yrId, Status, "TD No: " + newTDNo, newFD.Mem_Id,brCode, newTDId, newFD.Deposit_Amount, 0);
                            finVoucherTrns.Add(vocTrn);

                            /// Save new fixed deposit
                            result = await _unitOfWork.TermDepositTrn.AddTermDepositTrnAsync(tdTrnNew);
                            result = await _unitOfWork.TermDepositMember.AddTermDepositMemberListAsync(tdMembers);
                            result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns); 

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
                            sbCAMaster = Utility.GetModalObject.GetSBCAMasterObject(0,sbCAScheme.Scheme_Id, sbAcc.Mem_Id,"",1,false,false, vocId, Checked_By, yrId, brCode);
                            
                            (result, accId, accNo) = await _unitOfWork.SBCAMaster.AddSBCAMasterAsync(sbCAMaster);  
                            sbMemTrn = Utility.GetModalObject.GetMemTrnObject(7, sbAcc.Mem_Id,sbCAScheme.SBCA_Led_Id,sbAcc.Transaction_Date, sbAcc.Receipt_Amount,0, false, false, vocId, Checked_By, yrId, 1, 0, null, 0,"", 0, accId, 0, brCode);
                            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, sbCAScheme.SBCA_Led_Id, sbAcc.Receipt_Amount, 0, sbAcc.CashOrAdjustment,
                                Narration + " SB AC No : " + accNo , false, Checked_By, yrId, Status, "SB AC No: " + accNo, sbAcc.Mem_Id,brCode, accId, 0, 0);
                            finVoucherTrns.Add(vocTrn);
                            result = await _unitOfWork.MemTrn.AddMemTrnAsync(sbMemTrn);
                            result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns);
                            #endregion
                            break;
                        case 19:    /// Staff Loan Receipt
                            break;
                        case 20:    /// PF Subscription
                            break;
                        case 21:    /// Ledger Entry
                            break;
                        case 22:    /// Staff Suspense Creditor
                            break;
                        case 23:    /// Staff Suspense Debtor
                            break;
                        case 24:    /// FD Loan Receipt
                            #region FD Loan Receipt
                            List<Loan_Trn> fdLoanRecTrnList = new();
                            Loan_Trn fdLoanRecTrn = new();
                            DtoTermDepositLoanRecovery fdLoanRec = new();
                            fdLoanRec = Utility.JsonbObject.ConvertFromJsonForFixedDepositLoanRecovery(trns.Related_Account_Data!);
                            Narration = fdLoanRec.Member_No + " " + fdLoanRec.Member_Name;
                            foreach(var loan in fdLoanRec.Loan_Balance_List!)
                            {
                                fdLoanRecTrn = Utility.GetModalObject.GetLoanTrnObject(loan.Loan_Id, 0, "R", fdLoanRec.Transaction_Date, null, null, 0, 0, 0, 0, null, 0, null, loan.Current_Interest,
                                    loan.Interest_Applied_Date, 0, 0, loan.Interest_Collection, loan.Principal_Collection, 0, 0, 0, null, 0, null, 0, null,
                                    loan.Rate_Of_Interest , 0, 0,0, false, false, vocId, Checked_By, yrId, 0, false, loan.Principal_Balance-loan.Principal_Collection , 0, 0, 0, 0, brCode);
                                fdLoanRecTrnList.Add(fdLoanRecTrn);

                                if(loan.Interest_Collection >0)
                                {
                                    vocTrn = new();
                                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, loan.IntLed_Id, loan.Interest_Collection, 0, fdLoanRec.CashOrAdjustment,
                                        Narration + " Loan No : " + loan.Loan_No ,  false, Checked_By, yrId, Status, "Loan No: " + loan.Loan_No, fdLoanRec.Mem_Id, brCode, loan.Loan_Id, 0, 0);
                                    finVoucherTrns.Add(vocTrn);
                                }
                                if(loan.Principal_Collection > 0)
                                {
                                    vocTrn = new();
                                    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, loan.PrlLed_Id, loan.Principal_Collection, 0, fdLoanRec.CashOrAdjustment,
                                        Narration + " Loan No : " + loan.Loan_No  , false, Checked_By, yrId, Status, "Loan No: " + loan.Loan_No, fdLoanRec.Mem_Id, brCode, loan.Loan_Id, loan.Principal_Balance-loan.Principal_Collection, 0);
                                    finVoucherTrns.Add(vocTrn);
                                }
                            }
                            result = await _unitOfWork.LoanTrn.AddLoanTrnListAsync(fdLoanRecTrnList);
                            result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns);
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
                            jlTrnList = Utility.JsonbObject.GetJewelLoanRecovery(jewelLoanRecovery.JewelLoanBalance_List!, Transacted_Date, Checked_By, vocId, yrId,brCode);
                            List<Fin_Voucher_Trn> jlVocList = new();
                            jlVocList = Utility.JsonbObject.GetVoucherTrnForJewelLoanRecovery(jewelLoanRecovery.JewelLoanBalance_List!, vocId, jewelLoanRecovery.CashOrAdjustment, jewelLoanRecovery.Mem_Id, jewelLoanRecovery.Member_No!, jewelLoanRecovery.Member_Name!, Checked_By, yrId, Status,brCode);
                            finVoucherTrns.AddRange(jlVocList);
                            result = await _unitOfWork.LoanTrn.AddLoanTrnListAsync(jlTrnList);
                            result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns);
                            
                            #endregion
                            break;
                        case 32:    /// Dividend/TD Interest payment (bulk)
                            break;
                        case 33:    /// Ex-Gratia payment
                            break;
                        case 34:    /// Bonus Payment
                            break;
                        case 35:    /// Staff PF withdrawal
                            break;
                        case 36:    /// SB Account transaction
                            #region SB Account Transaction
                            Mem_Trn sbMemberTrn = new();
                            DtoSBAccountTransaction sbTrn = new();
                            sbTrn = Utility.JsonbObject.ConvertFromJsonForNewSBAccountTransaction(trns.Related_Account_Data!);
                            Narration = sbTrn.Member_No + " " + sbTrn.Member_Name;
                            sbMemberTrn = Utility.GetModalObject.GetMemTrnObject(7, sbTrn.Mem_Id, sbTrn.SBLed_Id , sbTrn.Transaction_Date, sbTrn.Receipt_Amount,
                                sbTrn.Payment_Amount, false, false, vocId, Checked_By, yrId, 0, 0,null, 0,"", 0,sbTrn.SBAccount_Id,0,brCode);
                            vocTrn = new();
                            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, sbTrn.SBLed_Id, sbTrn.Receipt_Amount, sbTrn.Payment_Amount, 
                                sbTrn.CashOrAdjustment, Narration + " SB AC No :" + sbTrn.SBAccount_No, false, Checked_By, yrId, Status, "SB AC No :" + sbTrn.SBAccount_No, sbTrn.Mem_Id,brCode, 0, 0, 0);
                            finVoucherTrns.Add(vocTrn);
                            result = await _unitOfWork.MemTrn.AddMemTrnAsync(sbMemberTrn);
                            result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns);
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
                            jlSchemes = await _unitOfWork.LoanScheme.GetLoanSchemesAsync(dtoJLDisb.Scheme_Id);

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
                            vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, jlSchemes.PrlLed_Id, 0, dtoJLDisb.Loan_Amount,
                                dtoJLDisb.CashOrAdjustment, Narration + " Loan No : " + jlLoanNo , false, Checked_By, yrId, Status, "Loan No :" + jlLoanNo,
                                dtoJLDisb.Mem_Id, brCode, jlLoanId, dtoJLDisb.Loan_Amount, 0);
                            finVoucherTrns.Add(vocTrn);

                            Map_General map = await _unitOfWork.MapGeneral.GetMapGeneralAsync(brCode );
                            if(dtoJLDisb.AppraisalFee > 0)
                            {
                                vocTrn = new();
                                vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, map.Appraisal_Fee_Led_Id, dtoJLDisb.AppraisalFee, 0, dtoJLDisb.CashOrAdjustment, 
                                    Narration + " Loan No :" + jlLoanNo , false, Checked_By, yrId, Status, "Loan No: " + jlLoanNo, dtoJLDisb.Mem_Id, brCode, 
                                    jlLoanId, dtoJLDisb.Loan_Amount, 0);
                                finVoucherTrns.Add(vocTrn);
                            }
                            if(dtoJLDisb.BankCharges >0)
                            {
                                vocTrn = new();
                                vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, map.Bank_Charges_Led_Id, dtoJLDisb.BankCharges, 
                                    0, dtoJLDisb.CashOrAdjustment, Narration + " Loan No :" + jlLoanNo, false, Checked_By, yrId, Status, "Loan No: " + jlLoanNo, 
                                    dtoJLDisb.Mem_Id, brCode, jlLoanId, dtoJLDisb.Loan_Amount, 0);
                                finVoucherTrns.Add(vocTrn);
                            }

                            result = await _unitOfWork.LoanTrn.AddLoanTrn(jlTrn);
                            result = await _unitOfWork.LoanDisbursement.AddLoanDisbursementAsync(jlDisb);
                            result = await _unitOfWork.LoanROI.AddLoanROIAsync(jlRoi);
                            result = await _unitOfWork.JLDetails.AddJLDetailsAsync(jlDetails);
                            result = await _unitOfWork.JLOrnment.AddJLOrnmentListAsync(ornmentList);
                            result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrns);
                            #endregion
                            break;
                        case 38:    /// Group Insurance Payment
                            break;
                        case 39:    /// Salary Payment
                            break;
                        case 40:    /// dA Arrear Payment
                            break;
                        case 41:    /// Surrender leave salary payment
                            break;
                        case 42:    /// Staff security deposit receipt
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
                        case 48:    /// Loan disbursment for PLDB
                            break;
                        case 51:    /// Decreed loan
                            break;
                        case 52:    /// Jewel loan disbursement with appraisal fee
                            #region Jewel Loan Disbursement with appraisal fee
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
                    }
                }

                /// update staging_master with checked by and checked date
                result = await _unitOfWork.StagingMaster.CheckerStateStaging(stagingId,vocId, Checked_By, "V");
                result = await _unitOfWork.StagingDetails.CheckerStateStaging(stagingId,vocId, Checked_By, "V");
                result = true;
                _unitOfWork.Complete();
                _unitOfWork.CommitTransaction();
            }
            catch (Exception ex)
            {
                string er = ex.Message;
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

       
    }
}