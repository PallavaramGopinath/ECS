using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Models
{
    [Table("refresh_tokens")]
    public class Refresh_Token
    {
        [Key]
        public int Id { get; set; }
        public string? Token { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public decimal User_Id { get; set; } // Link to the user who owns the token
    }
}
