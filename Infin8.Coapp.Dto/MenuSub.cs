using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class MenuSub
    {
        public int SubMenuId { get; set; }
        public int MenuId { get; set; }
        public string? SubMenuName { get; set; }
        public string? SubMenuIcon { get; set; }
        public string? FormName { get; set; }
        public bool SubMenuDelete { get; set; }
        public string? BrCode { get; set; }
        public string? VocStatus { get; set; }
        public List<MenuForm> Forms { get; set; } = new();
    }
}
