using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        public string? Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public DateTime Created { get; set; }
        public decimal MemberId { get; set; } // Link to the user who owns the token
    }
}
