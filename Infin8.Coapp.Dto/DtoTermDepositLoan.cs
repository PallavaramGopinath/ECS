using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoTermDepositLoan
    {
        public decimal TD_Id { get; set; }
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public DateTime Loan_Date { get; set; }
        public double Loan_Amount { get; set; }
        public double Rate_Of_Interest { get; set; }
        public double Interest_Overdue { get; set; }
        public DateTime? IntCalc_Date { get; set; }
        
        public double Current_Interest { get; set; }
        public double Principal_Balance { get; set; }
        public double Interest_Balance => Interest_Overdue + Current_Interest;
        public double Total_Balance => Principal_Balance + Interest_Balance ;
        public double Fixed_Deposit_Face_Value { get; set; }
    }
}
