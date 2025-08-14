using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Models
{
    public  class Staging_Balance
    {
        [Key]
        public decimal Id { get; set; }
        public decimal Ledger_Id { get; set; }
        public int Fnl_Id { get; set; }
        public DateTime Balance_Date { get; set; }
        public double Balance_Amount { get; set; }
        public decimal Created_By { get; set; }
        public DateTime  Created_Date { get; set; }
        public bool Balance_Delete { get; set; }
        public string? BrCode { get; set; }
        public decimal Yr_Id { get; set; }
        public decimal Voc_Id { get; set; }
    }
}
