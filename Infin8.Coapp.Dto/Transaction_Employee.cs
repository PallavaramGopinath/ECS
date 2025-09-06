using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class Transaction_Employee
    {
        public DateTime  Transaction_Date { get; set; }
        public int Receipt_Payment { get; set; }
        public int Cash_Adjustment { get; set; }
        public decimal Employee_Id { get; set; }
        public string? Employee_Name { get; set; }
        public string? Employee_Designation { get; set; }
        public int Account_Id { get; set; }
        public decimal Ledger_Id { get; set; }
        public double Ledger_Balance { get; set; }
        public double Ledger_Entry { get; set; }
        public string? Cheque_No { get; set; }
        public DateTime? Cheque_Date { get; set; }
        public string? Issue_Bank { get; set; }
        public int Fnl_Id { get; set; }
        public decimal Cash_Led_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public decimal Created_By { get; set; }
        public string? BrCode { get; set; }
    }
}
