using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class LockerAllotmentVM
    {
        public decimal Id { get; set; }

        [Required(ErrorMessage = "Please select a member")]
        public decimal CustomerId { get; set; }
        public int Age { get; set; }

        [Required(ErrorMessage = "Please select a locker")]
        public decimal LockerId { get; set; }

        [Required(ErrorMessage = "Allotment date is required")]
        public DateTime AllotmentDate { get; set; } = DateTime.UtcNow;

        [Range(0, double.MaxValue, ErrorMessage = "Deposit amount must be positive")]
        public double DepositAmount { get; set; } = 0;

        [Range(0, 100, ErrorMessage = "Interest rate must be between 0 and 100")]
        public double InterestRate { get; set; } = 0;
        public double RentAmount { get; set; }
        public DateTime? LastRentAdjustmentDate { get; set; }
        public DateTime? NextRentDueDate { get; set; }
        public string? Status { get; set; } = "Active";
        public DateTime? ClosureDate { get; set; }
        public double RefundAmount { get; set; } = 0;
        public decimal YrId { get; set; }
        public string? BrCode { get; set; }
        public decimal CreatedBy { get; set; }

        ///  for account transactions
        public int CashOrAdjustment { get; set; }
        public int AccountId { get; set; }
        public decimal  PaymentLedgerId { get; set; }
        public double LedgerBalance { get; set; }
        public double PaymentAmount { get; set; }
        public string? ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }
        public string? IssueBank { get; set; }
        // Display properties
        public string? CustomerNo { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string LockerNumber { get; set; } = string.Empty;
        public string LockerSize { get; set; } = string.Empty;
        public double LockerRentAmount { get; set; }
    }
}
