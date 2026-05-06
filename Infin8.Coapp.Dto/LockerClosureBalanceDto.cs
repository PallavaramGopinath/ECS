using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class LockerClosureBalanceDto
    {
        public decimal AllotmentId { get; set; }
        public decimal LockerId { get; set; }
        public string? LockerNumber { get; set; }
        public string? SizeName { get; set; }
        public DateTime TransactionDate { get; set; }
        public double RentAmount { get; set; }
        public double RentReceivable { get; set; }
        public double RentReceived { get; set; }
        public decimal DepositId { get; set; }
        public string? DepositNo { get; set; }
        public DateTime DepositDate { get; set; }
        public double InterestRate { get; set; }
        public double DepositRefundAmount { get; set; }
        public double InterestPreviousBalance { get; set; }
        public DateTime? InterestPreviousAppliedDate { get; set; }
        public double InterestCalculated { get; set; }
        public DateTime? InterestCalculatedDate { get; set; }
        public DateTime LockerClosureDate { get; set; }
        ///public double InterestPaid { get; set; }
    }
}
