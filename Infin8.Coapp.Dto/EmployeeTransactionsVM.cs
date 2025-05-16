using Infin8.Coapp.Models;
namespace Infin8.Coapp.Dto
{
    public class EmployeeTransactionsVM
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

        /// Member suspense creditor
        public List<Mem_Trn>? EmpSusCreditorList { get; set; }

        /// Member suspense debtor
        public List<Mem_Trn>? EmpSusDebtorList { get; set; }

        /// Salary Payment
        /// (Update Pay_Slip Pmt = true, Pmt_Date = trnDate, Voc_Id = vocId, where Emp_Id = empId, and Pay_Id  = payid)
        public List<Loan_Trn>? SalaryPaymentLoanReceipt { get; set; }
        public List<Emp_Pf>? SalaryPaymentEmpPF { get; set; }
        public List<Mem_Trn>? SalaryPaymentMemTrn { get; set; }
        /// DA Arrears Payment
        /// (Update Pay_Slip Pmt = true, Pmt_Date = trnDate, Voc_Id = vocId, where Emp_Id = empId, and Pay_Id  = payid)
        public List<Emp_Pf>? DAArrearsPaymentEmpPF { get; set; }
        /// Surrender Leave Salary payment
        public List<Pay_Slip>? SurrenderLeaveSalaryList { get; set; }
        /// Security Deposit/Intereston Security Deposit payment
        public List<TermDeposit_Trn>? SecurityDepositPaymentList { get; set; }
        public List<TermDeposit_Trn>? SecurityDepositReceiptList  { get; set; }
        /// PF Subscription
        public List<Emp_Pf>? PFSubscriptionList { get; set; }
        /// PF Withdrawal
        public List<Emp_Pf>? PFWithdrawalList { get; set; }
        /// Staff Loan Payment
        public List<Loan_Trn>? StaffLoanReceiptList { get; set; }
        /// Staff Loan recovery
        /// (Ex-gratia/Bonus Payments)
        public List<Pay_Slip>? Ex_gratiaPaymentList { get; set; }
        /// sundry creditor and debtors
        public List<Pay_Slip>? BonusPaymentList { get; set; }

    }
}
