using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoTermDepositLoanBalance
    {
        public decimal TD_Id { get; set; }
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public DateTime Loan_Date { get; set; }
        public double Loan_Amount { get; set; }
        public double Rate_Of_Interest { get; set; }
        public DateTime Due_Date { get; set; }
        public double Interest_Balance { get; set; }
        public double Current_Interest { get; set; }
        public DateTime? Interest_Application_Date { get; set; }
        public double Principal_Balance { get; set; }
        public double Total_Balance { get; set; }
        public double Interest_Collection { get; set; }
        public double Principal_Collection { get; set; }
        public double Total_Collection { get; set; }
    }
}
