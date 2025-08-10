using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoTermDepositFixedDepositRenewal
    {
        public int AccountId { get; set; }
        public int CashOrAdjustment { get; set; }
        public decimal Mem_Id { get; set; }
        public string? Member_No { get; set; }
        public string? Member_Name { get; set; }
        public DateTime Transaction_Date { get; set; }
        public string? PaymentStatus { get; set; }
        public DtoTermDepositFixedDepositPayment? FixedDepositPayment { get; set; }
        public DtoTermDepositFixedDepositCreation? FixedDepositCreate { get; set; }

    }
}
