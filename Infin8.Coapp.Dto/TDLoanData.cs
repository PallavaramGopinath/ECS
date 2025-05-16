using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class TDLoanData
    {
        public decimal  Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public DateTime Loan_Date { get; set; }
        public double Loan_Amount { get; set; }
        public double Loan_Outstanding { get; set; }
        public double Interest_Balance { get; set; }
        public double Interest_Calculated { get; set; }
        public DateTime? Interest_Applied_Date { get; set; }
        public double Total_Balance => Loan_Outstanding + Interest_Balance + Interest_Calculated;
    }
}
