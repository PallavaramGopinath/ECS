using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class TransactionBy_Members
    {
        public DateTime Transaction_Date { get; set; }
        public decimal Member_Id { get; set; }
        public string?   Member_No { get; set; }
        public string? Member_Name { get; set; }
        public string? Father_Name { get; set; }
        public string? Address { get; set; }
        public int Cash_Adjustment { get; set; }
        public int Receipt_Payment { get; set; }
        public decimal Ledger_Id { get; set; }
        public int Account_Id { get; set; }
        public string? Cheque_No { get; set; }
        public DateTime? Cheque_Date { get; set; }
        public string? Issue_Bank { get; set; }
        public double Ledger_Balance { get; set; }
        public double Ledger_Entry { get; set; }
        public decimal Yr_Id { get; set; }
    }
}
