using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class Transaction_Accounting
    {
        public DateTime Transaction_Date { get; set; }
        public int Cash_Adjustment { get; set; }
        public int Receipt_Payment { get; set; }
        public decimal Ledger_Id { get; set; }
        public bool Is_Bank_Ledger { get; set; }
        public string? Cheque_No { get; set; }
        public DateTime? Cheque_Date { get; set; }
        public string? Issue_Bank { get; set; }
        public double Ledger_Balance { get; set; }
        public double Ledger_Entry { get; set; }
        public double Yr_Id { get; set; }
        public int Fnl_Id { get; set; }
        public decimal Cash_Led_Id { get; set; }
        public string? Narration { get; set; }
    }
}
