using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoTermDepositFixedDepositPayment
    {
        public decimal Mem_Id { get; set; }
        public string? Member_No { get; set; }
        public string? Member_Name { get; set; }
        // public string? Father_Name { get; set; }
        // public int Age { get; set; }
        public DateTime Transaction_Date { get; set; }
        public string? PaymentStatus { get; set; }
        public List<DtoTermDepositFixedDepositPayable>? Fixed_Deposit_Datas { get; set; }
        public List<DtoTermDepositNo>? FixedDeposit_Nos { get; set; }
        public List<DtoTermDepositLoan>? Loan_On_FixedDeposits { get; set; }
        public double Total_Interest_Paid { get; set; }
        public double Total_Interest_Calculated { get; set; }
        public double Total_Interest_Payable { get; set; }
        public double Total_Deposit_Payable { get; set; }
        public double Total_Loan_Interest_Balance { get; set; }
        public double Total_Loan_Principal_Balance { get; set; }
        public double Net_Payable { get; set; }
        public bool Is_Deduct_Loan_Amount { get; set; }
    }
}
