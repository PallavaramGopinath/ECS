using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class DtoDividendPayment
    {
        [Required]
        public decimal Mem_Id { get; set; }
        public int AccountId { get; set; }
        public int CashOrAdjustment { get; set; }
        public string? Member_No { get; set; }
        public string? Member_Name { get; set; }
        public DateTime Transaction_Date { get; set; }
        public int DividendPayable { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Enter a valid Dividend Paid amount")]
        public int DividendPaid { get; set; }
        public List<DividendOrIntOnTDPaymentVM>? DivideneList { get; set; }
        public string? BrCode { get; set; }
        public decimal Created_By { get; set; }
    }
}
