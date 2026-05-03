using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    public  class Locker_Rent_Template
    {
        [Key]
        public decimal Id { get; set; }
        public decimal  Size_Id { get; set; }
        public DateTime Effective_From { get; set; }
        public double Deposit_Amount { get; set; }
        public double Interest_Rate { get; set; }
        public double Rent_Amount { get; set; }
        public string? BrCode { get; set; }
        public decimal Created_By { get; set; }
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
    }
}
