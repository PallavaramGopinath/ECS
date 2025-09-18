using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class MenuForm
    {
        public int FormId { get; set; }
        public int SubMenuId { get; set; }
        public string? FormMenu { get; set; }
        public string? FormName { get; set; }
        public bool FormDelete { get; set; }
        public string? BrCode { get; set; }
        public string? VocStatus { get; set; }
    }
}
