using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class LockerLookupDto
    {
        public decimal Id { get; set; }
        public string LockerNumber { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public double RentAmount { get; set; }
        public double DepositAmount { get; set; }
        public double InterestRate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
