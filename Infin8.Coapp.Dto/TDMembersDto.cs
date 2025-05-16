using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class TDMembersDto
    {
        public decimal Mem_Id { get; set; }
        public string? Member_No { get; set; }
        public string? Member_Name { get; set; } 
        public string? Father_Name { get; set; }
        public int Age { get; set; }
    }
}
