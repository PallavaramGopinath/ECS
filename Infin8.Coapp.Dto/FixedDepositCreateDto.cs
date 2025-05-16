using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class FixedDepositCreateDto
    {
        public TDCommonDto? Common { get; set; }
        [Required(ErrorMessage = "At least one member is required.")]
        [MinLength(1, ErrorMessage = "At least one member must be added.")]
        public List<TDMembersDto> Members { get; set; } = new List<TDMembersDto>();
        [Required(ErrorMessage = "Deposit amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Deposit amount must be a positive number.")]
        public double Deposit_Amount { get; set; }
        [Required(ErrorMessage = "Interest payable frequency is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Interest payable frequency must be a positive number.")]
        public int Interest_Payable_Frequency { get; set; }
        [Required(ErrorMessage = "Period in months is required.")]
        [Range(0, 120, ErrorMessage = "Period in months must be between 0 and 120.")]
        public string? Period_Type { get; set; } 

        public bool Is_DiscounRate { get; set; }
        public int compoundfrequency { get; set; }
        public int Period_In_Months { get; set; }
        public int Period_In_Days { get; set; }
        [Required(ErrorMessage = "Rate of interest is required.")]
        [Range(0.01, 100, ErrorMessage = "Rate of interest must be between 0.01 and 100.")]
        public double Rate_Of_Interest { get; set; }
        [Required(ErrorMessage = "Maturity amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Maturity amount must be a positive number.")]
        public double Maturity_Amount { get; set; }
        [Required(ErrorMessage = "Maturity date is required.")]
        public DateTime Maturity_Date { get; set; }
    }
}
