using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoSBAccountTransaction
    {
        public int AccountId { get; set; }
        public int CashOrAdjustment { get; set; }
        public decimal Mem_Id { get; set; }
        public string? Member_No { get; set; }
        public string? Member_Name { get; set; }
        public DateTime Transaction_Date { get; set; }
        public decimal SBLed_Id { get; set; }
        public decimal SBAccount_Id { get; set; }
        public string? SBAccount_No { get; set; }
        public double Balance_Amount { get; set; }
        public double Receipt_Amount { get; set; }
        public double Payment_Amount { get; set; }
        public string? BrCode { get; set; }
        public int ReceiptOrPayment { get; set; }
    }
}
