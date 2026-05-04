using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class LockerRentReceiptDto
    {
        public decimal CustomerId { get; set; }
        public int AccountId { get; set; }
        public int CashOrAdjustment { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerNo { get; set; }
        public int ReceiptOrPayment { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal LockerId { get; set; }
        public decimal AllotmentId { get; set; }
        public string? LockerNumber { get; set; }
        public double RentReceived { get; set; }
        public string? BrCode { get; set; }
        public decimal Created_By { get; set; }
        public decimal YrId { get; set; }
        //public List<LockerRentReceiptAllotmentWiseDto>? AllotmentWiseReceipt { get; set; }
    }
}
