using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoTermDepositFixedDepositPayable
    {
        public decimal FD_Id { get; set; }
        public string? FD_No { get; set; }
        public DateTime Maturity_Date { get; set; }
        public double Previous_Interest_Calculated { get; set; }
        public double Previous_Interest_Paid { get; set; }
        public double Pending_Interest { get; set; }
        public double Current_Interest_Calculated { get; set; }
        public double Rate_Of_Interest_Applied { get; set; }
        public DateTime? Current_Interest_Applied_Date { get; set; }
        public double Total_Interest_Payable { get; set; }
        public double Current_Interest_Payment { get; set; }
        public double Deposit_Refund { get; set; }
        public double Total_Payable { get; set; }
    }
}
