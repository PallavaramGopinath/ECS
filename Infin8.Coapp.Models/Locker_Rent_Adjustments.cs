using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Models
{
    public class Locker_Rent_Adjustments
    {
        [Key]   
        public decimal Id { get; set; }
        public decimal Allotment_Id { get; set; }
        public DateTime? Rent_Applied_Date { get; set; }
        public double Rent_Receivable { get; set; } = 0;
        public DateTime? Rent_Received_Date { get; set; }
        public double Rent_Adjusted_From_Interest { get; set; } = 0;
        public double Rent_Received { get; set; } = 0;
        public string? BrCode { get; set; }
        public decimal Created_By { get; set; }
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
        public decimal Voc_Id { get; set; }
    }
}
