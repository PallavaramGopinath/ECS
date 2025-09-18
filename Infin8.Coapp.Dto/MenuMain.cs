using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class MenuMain
    {
        public int MenuId { get; set; }
        public string? MenuName { get; set; }
        public bool MenuDelete { get; set; }
        public string? BrCode { get; set; }
        public string? VocStatus { get; set; }
        public List<MenuSub> SubMenus { get; set; } = new();
    }
}
