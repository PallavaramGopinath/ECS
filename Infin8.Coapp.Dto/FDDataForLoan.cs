using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class FDDataForLoan
    {
        public decimal FD_Id { get; set; }
        public string? FD_No { get; set; }
        public double FD_Amount { get; set; }
        public int Period_In_Months { get; set; }
        public int Period_In_Days { get; set; }
        public double Rate_Of_Interest { get; set; }
        public DateTime Value_Date { get; set; }
        public double Maturity_Amount { get; set; }
        public DateTime Maturity_Date { get; set; }
        public double Drawing_Power { get; set; } 
        public double Loan_Amount { get; set; } 
    }
}
