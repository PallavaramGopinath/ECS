using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class DtoTermDepositFixedDepositLoan
    {
        public int AccountId { get; set; }
        public int CashOrAdjustment { get; set; }
        public decimal Mem_Id { get; set; }
        public string? Member_No { get; set; }
        public string? Member_Name { get; set; }
        public DateTime Transaction_Date { get; set; }
        public List<DtoTermDepositFixedDepositNos>? FD_Nos { get; set; } = new();
        public List<FDDataForLoan>? FD_Datas { get; set; } = new();
        public List<TDLoanData>? Existing_Loan_Datas { get; set; } = new();

        // Store selected FD Ids
        public List<decimal> Selected_FD_Ids { get; set; } = new();

        public double Loan_Eligible_Percentage { get; set; }
        public double Total_FD_Amount { get; set; }
        public double Total_Borrowing_Power { get; set; }
        public double Total_Existing_Loan_Amount { get; set; }
        public double Eligible_Loan_Amount { get; set; }
        public double Loan_Rate_Of_Interest { get; set; }

        [Required(ErrorMessage = "Loan Amount is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Loan Amount must be greater than 0.")]
        // Custom validation will be added in the component logic to check against Eligible_Loan_Amount
        public double Loan_Amount { get; set; }

    }
}
