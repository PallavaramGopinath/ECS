using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    public class Locker_Settings
    {
        [Key]
        public int Id { get; set; }
        public string? Rent_Cycle { get; set; }
        public int Grace_Days { get; set; } = 30;
        public string? Penalty_Type { get; set; }
        public decimal Rent_Ledger_Id { get; set; }
        public string? BrCode { get; set; }
        public decimal Created_By { get; set; }
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
    }
}
