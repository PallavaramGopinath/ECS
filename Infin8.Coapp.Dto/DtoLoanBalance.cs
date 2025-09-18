using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoLoanBalance
    {
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public string? Scheme_Name { get; set; }
        public string? Status { get; set; }
        public double Loan_Amount { get; set; }
        public DateTime Loan_Date { get; set; }
        public double ROI { get; set; }
        public double PI_Rate { get; set; }
        public DateTime Due_Date { get; set; }
        public DateTime FirstPrl_DueDate { get; set; }
        public DateTime FirstInt_DueDate { get; set; }
        public DateTime? Interest_Application_Date { get; set; }
        public double Outstanding { get; set; }
        public double Principal_Demand { get; set; }
        public double Calculated_Amount { get; set; }
        public double Recovery_Amount { get; set; }
        public decimal Led_Id { get; set; }
        public double OverDue_Amount { get; set; }
        public int OrderNo { get; set; }
    }
}
