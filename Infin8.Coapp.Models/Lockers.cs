using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    public  class Lockers
    {
        [Key]
        public decimal Id { get; set; }
        public string? Locker_Number { get; set; }
        public decimal Size_Id { get; set; }
        public string? Status { get; set; } = "Vacant";
        public string? BrCode { get; set; }
        public decimal Created_By { get; set; }
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
    }
}
