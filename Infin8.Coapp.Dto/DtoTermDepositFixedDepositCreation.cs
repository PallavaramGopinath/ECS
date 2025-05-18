using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoTermDepositFixedDepositCreation
    {
        public int AccountId { get; set; }
        public int CashorAdjustment { get; set; }
        public decimal Mem_Id { get; set; }
        public string? Member_No { get; set; }
        public string? Member_Name { get; set; }
        [Required(ErrorMessage = "Common information is required")]
        public DtoTermDepositCommon  Common { get; set; } = new();

        public List<DtoTermDepositMember> Members { get; set; } = new List<DtoTermDepositMember>();

        [Required(ErrorMessage = "Deposit Amount is required")]
        [Range(1, double.MaxValue, ErrorMessage = "Deposit Amount must be greater than 0")]
        public double Deposit_Amount { get; set; }

        [Required(ErrorMessage = "Interest Payable Frequency is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Please select a valid Interest Payable Frequency")]
        public int Interest_Payable_Frequency { get; set; }


        [Required(ErrorMessage = "Period Type is required")]
        [StringLength(10, ErrorMessage = "Period Type cannot exceed 10 characters")]
        public string? Period_Type { get; set; }

        public bool Is_DiscountRate { get; set; }

        public int compoundfrequency { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Period In Months must be a positive number")]
        public int Period_In_Months { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Period In Days must be a positive number")]
        public int Period_In_Days { get; set; }

        [Required(ErrorMessage = "Rate of Interest is required")]
        [Range(0.01, 100, ErrorMessage = "Rate of Interest must be between 0.01 and 100")]
        public double Rate_Of_Interest { get; set; }

        [Required(ErrorMessage = "Maturity Amount is required")]
        [Range(1, double.MaxValue, ErrorMessage = "Maturity Amount must be greater than 0")]
        public double Maturity_Amount { get; set; }

        [Required(ErrorMessage = "Maturity Date is required")]
        public DateTime Maturity_Date { get; set; }
    }
}
