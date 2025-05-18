using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoJewelLoanDisbursement
    {
        public int AccountId { get; set; }
        public int CashOrAdjustment { get; set; }
        public decimal Mem_Id { get; set; }
        public string? Member_No { get; set; }
        public string? Member_Name { get; set; }
        public DateTime Tranaction_Date { get; set; }
        public int Scheme_Id { get; set; }
        public string? Loan_No { get; set; }
        public double Market_Rate { get; set; }
        public double Govt_Rate { get; set; }
        public double Maximum_Loan_Limit { get; set; }
        public double Existing_Loan_Outstanding { get; set; }
        public double Percentage_Of_Eligibility_On_Market_Rate { get; set; }
        public int Period_Of_Loan { get; set; }
        public DateTime Due_Date { get; set; }
        public double Rate_Of_Interest { get; set; }
        public double Penal_Rate { get; set; }
        public double Loan_Amount { get; set; }
        public double Gross_Weight { get; set; }
        public double Wasgate { get; set; }
        public double Net_Weight { get; set; }
        public double Jewels_Value { get; set; }
        public string? Jewels_Photo_Path { get; set; }
        public List<DtoJewelLoanOrnments>? Ornment_List { get; set; }
        public double AppraisalFee { get; set; }
        public double BankCharges { get; set; }
    }
}
