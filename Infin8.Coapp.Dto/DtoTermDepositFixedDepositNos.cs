using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class DtoTermDepositFixedDepositNos
    {
        public decimal FD_Id { get; set; }
        public string? FD_No { get; set; }
        public bool IsSelected { get; set; } // Helper property for checkbox binding
    }
}
