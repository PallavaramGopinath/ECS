using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoJewelLoanRecovery
    {
        [Required]
        public decimal Mem_Id { get; set; }
        public string? Member_No { get; set; }
        public string? Member_Name { get; set; }
        public DateTime Transaction_Date { get; set; }
        public double Total_Outstanding { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Enter a valid recovery amount")]
        public double Recovery_Amount { get; set; }
        public List<DtoJewelLoanBalance>? JewelLoanBalance_List { get; set; }
        public List<DtoLoanNo>? JewelLoanNo_List { get; set; }
    }
}
