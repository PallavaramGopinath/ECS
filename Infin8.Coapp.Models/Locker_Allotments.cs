using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    public  class Locker_Allotments
    {
        [Key]
        public decimal Id { get; set; }
        public decimal Customer_Id { get; set; }
        public decimal Locker_Id { get; set; }
        public DateTime Allotment_Date { get; set; }
        public double Deposit_Amount { get; set; } = 0;
        public double Interest_Rate { get; set; } = 0;
        public DateTime? Last_Rent_Adjustment_Date { get; set; }
        public DateTime? Next_Rent_Due_Date { get; set; }
        public string? Status { get; set; }
        public DateTime? Closure_Date { get; set; }
        public double Refund_Amount { get; set; }
        public string? BrCode { get; set; }
        public decimal Created_By { get; set; }
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
        public decimal Voc_Id { get; set; }
    }
}
