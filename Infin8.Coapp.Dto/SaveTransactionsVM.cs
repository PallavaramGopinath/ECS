using Infin8.Coapp.Models;
namespace Infin8.Coapp.Dto
{
    public class SaveTransactionsVM
    {
        public int VocId { get; set; }
        public int UsrId { get; set; }
        public int YrId { get; set; }
        public DateTime TrnDate { get; set; }

        public double CashReceipt { get; set; }
        public double CashPayment { get; set; }
        public double AdjustmetReceipt { get; set; }
        public double AdjustmentPayment { get; set; }
        public bool IsChequeOnly { get; set; }
        public string? ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }
        public bool IsMemExpired { get; set; }
        public DateTime? MemExpiredDate { get; set; }

        public bool IsMemStatusChanged { get; set; }
        public int MemStatus { get; set; }

        public double VocAmt { get; set; }
        public int RptPmtId { get; set; }
        public int CashAdjId { get; set; }
        public int AccountId { get; set; }
        public string? AccountName { get; set; }

        public int LedgerId { get; set; }
        public string? LedgerName { get; set; }

        /// Member detals
        public MemberDetailsVM? MemberTransacted { get; set; }
        public MemberDetailsVM? MemberACHolder { get; set; }


        /// FinAccount
        public Fin_Voucher? FinVoucher { get; set; }
        public List<Fin_Voucher_Trn>? FinVoucherTrList { get; set; }

        public AccountsRptPmtNosVM? RptNoAndPmtNo { get; set; }
        public List<AccountTransactionVM>? voucherList { get; set; }

        /// Loan receipt - 1
        public List<Loan_Trn>? LoanRecovery { get; set; }
        public bool MemStopDemand { get; set; }
        /// RD Receipt  - 2
        public List<TermDeposit_Trn>? RDReceiptList { get; set; }

        /// Member suspense creditor - 3
        public Mem_Trn? MemSusCreditor { get; set; }

        /// Member suspense debtor  - 4
        public Mem_Trn? MemSusDebtor { get; set; }

        /// Member share capital    - 5
        public Mem_Trn? MemShareCapital { get; set; }

        public List<Mem_Trn>? MemTrnList { get; set; }
        /// Member Advance deposit receipt  - 6
        public Deposit_Trn?  MemAdvanceDepositReceipt { get; set; }

        /// FD interest payment - 7
        public List<TermDeposit_Trn>? FDInterestPaymentList { get; set; }

        /// FD Refund   - 8
        public List<TermDeposit_Trn>? FDRefundList { get; set; }
        public List<Loan_Trn>? FDRefundLoanRecoveryList { get; set; }

        /// FD Renewal  - 9
        public List<TermDeposit_Trn>? FDRenewalOldTrnList { get; set; }
        public List<Loan_Trn>? FDRenewalLoanRecoveryList { get; set; }
        public TermDeposit_Master? FDRenewalNewAccount { get; set; }
        public TermDeposit_Trn? FDRenewalNewTrn { get; set; }
        public List<TermDeposit_Members>? FDRenewalNewMembers { get; set; }

        /// RD Refund   - 10
        public List<TermDeposit_Trn>? RDRefundList { get; set; }
        public List<Loan_Trn>? RDRefundLoanRecoveryList { get; set; }

        /// Bank    - 11 
        public List<Fin_Voucher_Bank>? GenVocBankList { get; set; }

        /// Housing Society Loan Disbursement - 12
        public Loan_Master? LoanMasterHSIS { get; set; }
        public Loan_Trn?  LoanTrnHSIS { get; set; }
        public Loan_Disb? LoanDisbHSIS { get; set; }
        public Loan_Inst? LoanInstalmentHSIS { get; set; }
        public List<Loan_Roi>? LoanRoiListHSIS { get; set; }
        public List<Loan_Members>? LoanMemListHSIS { get; set; }

        /// FD Loan disbursement  - 13 
        public TDLoanBaseVM? FDLoanModel { get; set; }

        /// RD Loan disbursement - 14 (15-staff loan disbursement)
        public TDLoanBaseVM? RDLoanModel { get; set; }
        //public List<Lien_Tr> RDLoanLienTr { get; set; }

        /// TD Create   - Create FD 16: Create RD 17
        public TermDeposit_Master? TDMaster { get; set; }
        public TermDeposit_Trn? TDTransaction { get; set; }
        public List<TermDeposit_Members>? TDMembers { get; set; }
        ///SB Account Creation  - 18 
        public SBCA_Master? SBCAAccountCreate { get; set; }
        public Mem_Trn? SBAccountCreateMemtrn { get; set; }

        ///(19-staff loan receipt, 20-PF subscription, 21-ledger entry)
        
        ///22-staff suspense creditor, 23-staff suspense debtor

        /// FD loan receipt - 24
        public List<Loan_Trn>? FDLoanRecovery { get; set; }

        /// FD loan receipt - 25
        public List<Loan_Trn>? RDLoanRecovery { get; set; }

        /// Pending Deposit receipt - 26
        public List<Deposit_Trn>? PendingDepositReceipt { get; set; }

        /// Member Deposit Payment - 27 (28-null)
        public List<Mem_Trn>? MemCancellationMemTrn { get; set; }
        public List<Deposit_Trn>? MemCancellationDepositTrn { get; set; }

        /// Dividend Payment - 29
        public List<Mem_Trn>? MemDividendPayemnt { get; set; }

        /// Int on Thrift Deposit payment - 30 
        public List<Deposit_Trn>? MemIntOnTDPayment { get; set; }
        /// (31-jl recovery)
        public List<Loan_Trn>? JewelLoanRecovery { get; set; }
       

        /// Dividend and int on td payment model (bulk)  - 32 (33-Ex-gratia, 34-bonus, 35- staff pf withdrawn)
        public List<DividendAndIntOnTDPaymentVM>? dividendAndIntOnTDPaymentList { get; set; }

        public List<Mem_Trn>? DividendPaymentBulk { get; set; }
        public List<Deposit_Trn>? IntOnTDPaymentBulk { get; set; }

        /// SB Account Transaction   - 36 (37-JL disbursement)
        public Mem_Trn? SBAccountTrn { get; set; }

        /// Group insurance payment - 38
        public Mem_GI_Master?  GIMaster { get; set; }
        public List<Mem_GI_Trn>? GITrnList { get; set; }
        public List<Deposit_Trn>? GIDepositTrnList { get; set; } /// FWD Payment
        public List<Mem_Trn>? GIMemTrnList { get; set; }  /// Due By Demand
        public List<Deposit_Options>? GIDepositOptionList { get; set; }

        /// STAFF TRANSACTIONS
        /// Staff loan disbursement 15
        public Loan_Master? StaffLoanMaster { get; set; }
        public Loan_Trn? StaffLoanTrn { get; set; }
        public Loan_Disb? StaffLoanDisbursement { get; set; }
        public Loan_Roi? StaffLoanROI { get; set; }
        /// Staff Loan recovery - 19
        public List<Loan_Trn>? StaffLoanRecovery { get; set; }
        /// Salary Payment - 39 (Update Pay_Slip Pmt = true, Pmt_Date = trnDate, Voc_Id = vocId, where Emp_Id = empId, and Pay_Id  = payid)
        public List<int>? SalaryPaymentEmpIdList { get; set; }
        public int SalaryPaymentPayId { get; set; }
        public List<Loan_Trn>? SalaryPaymentLoanReceipt { get; set; } /// Staff loan receipt from pay roll
        public List<Emp_Pf>? SalaryPaymentEmpPF { get; set; }    /// staff PF payment from pay roll
        public List<Mem_Trn>? SalaryPaymentMemTrn { get; set; }   /// staff suspense debtor receipt from pay roll

        /// DA Arrears Payment - 40 (Update Pay_Slip Pmt = true, Pmt_ = trnDate, Voc_Id = vocId, where Emp_Id = empId, and Pay_Id  = payid)
        public List<int>? DAArrearsPaymentEmpIdList { get; set; }
        public int DAArrearsPaymentPayId { get; set; }
        public List<Emp_Pf>? DAArrearsPaymentEmpPF { get; set; }

        /// Surrender Leave Salary payment - 41
        /// if Pay_Id = 0 then insert Pay_Init table
        public int SLSPay_Id { get; set; }
        public Pay_Init? SLSPayInit { get; set; }
        
        public Pay_Slip? SurrenderLeaveSalary { get; set; }

        /// Security Deposit Receipt - 42
        public TermDeposit_Master? SecurityDepositMaster { get; set; }
        public TermDeposit_Trn? SecurityDepositReceiptTrn { get; set; }
        public TermDeposit_Members?  SecurityDepositMember { get; set; }

        /// Security Deposit/Intereston Security Deposit payment - 43
        public List<TermDeposit_Trn>? SecurityDepositPaymentList { get; set; }
        
        /// Staff suspense creditor -22
        public List<Mem_Trn>? EmpSusCreditorList { get; set; }
        /// Staff suspense debtor 23
        public List<Mem_Trn>? EmpSusDebtorList { get; set; }

        /// PF Subscription - 20
        public List<Emp_Pf>? PFSubscriptionList { get; set; }
        /// PF Withdrawal   - 35
        public List<Emp_Pf>? PFWithdrawalList { get; set; }

        /// (Ex-gratia/Bonus Payments)  - 33
        public int EXGPay_Id { get; set; }
        public Pay_Init? EXGPayInit { get; set; }
        public List<Pay_Slip>? Ex_gratiaPaymentList { get; set; }
        /// Staff bonus payment - 34
        public int BONPay_Id { get; set; }
        public Pay_Init? BONPayInit { get; set; }
        public List<Pay_Slip>? BonusPaymentList { get; set; }
        /// STAFF TRANSACTIONS END
       
        /// Member suspense transactions Multiple   44, 45
        public List<Mem_Trn>? MemDueToMultiple { get; set; }
        public List<Mem_Trn>? MemDueByMultiple { get; set; }
        /// Member suspense transaction Multiple end

        /// Jewel loan disbursement 37
        public Loan_Master? JewelLoanMaster { get; set; }
        public Loan_Trn? JewelLoanTrn { get; set; }
        public Loan_Disb? JewelLoanDisbursement { get; set; }
        public Loan_Roi? JewelLoanROI { get; set; }
        public List<Loan_Roi>? JewelLoanROIList { get; set; }
        public Loan_Inst? JewelLoanInstalment { get; set; }
        public JL_Details? JewelDetails { get; set; }
        public List<JL_Ornments>? JewelOrnmentList { get; set; }
        public List<Loan_Repayment_Schedule>? LoanRepaymentScheduleList { get; set; }

        /// Loan Reimbursemen
        public Loan_Disb? LoanReimbursement { get; set; }

        /// Loan disbursement PLDB 48
        
    }
}
