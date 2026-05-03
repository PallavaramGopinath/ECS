using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Models
{
    public  class Locker_Closures
    {
        [Key]
        public decimal Id { get; set; }
        public decimal Allotment_Id { get; set; }
        public DateTime Closure_Date { get; set; } = DateTime.UtcNow;
        public DateTime? Final_Interest_Calc_Upto { get; set; }
        public double Total_Interest_Earned { get; set; }
        public double Pending_Rent_Deducted { get; set; }
        public double Deposit_Returned { get; set; }
        public string? Remarks { get; set; }
        public string? BrCode { get; set; }
        public decimal Created_By { get; set; }
        public DateTime? Created_At { get; set; } = DateTime.UtcNow;
    }
}
