using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Infin8.Coapp.Models
{
    public  class Locker_Status_Master
    {
        [Key]
        public string? Status_Code { get; set; }
        public string? Status_Name { get; set; }
        public bool Is_Active { get; set; }
        public int Display_Order { get; set; }
    }
}
