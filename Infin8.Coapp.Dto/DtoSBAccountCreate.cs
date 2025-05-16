using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class DtoSBAccountCreate
    {
        public decimal Mem_Id { get; set; }
        public string? Member_No { get; set; }
        public string? Membe_Name { get; set; }
        public DateTime Transaction_Date { get; set; }
        public string? SBAccount_No { get; set; }

        [Required]
        public double Receipt_Amount { get; set; }
    }
}
