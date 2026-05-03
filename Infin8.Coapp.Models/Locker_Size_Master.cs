using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    public class Locker_Size_Master
    {
        [Key]
        public decimal Id { get; set; }
        public string? Size_Code { get; set; }
        public string? Size_Name { get; set; }
        public double Deposit_Amount { get; set; }
        public double Interest_Rate { get; set; }
        public double Rent_Amount { get; set; }
        public bool Is_Active { get; set; }
        public int Display_Order { get; set; }
        public string? BrCode { get; set; }
        public decimal Created_By { get; set; }
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
    }
}
