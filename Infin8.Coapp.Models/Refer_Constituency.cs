using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Models
{
    public class Refer_Constituency
    {
        [Key]
        public int id { get; set; }
        public string? constituency { get; set; }
        public string? reserved { get; set; }
        public string? district { get; set; }
        public string? locksabha { get; set; }
    }
}
