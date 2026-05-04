using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class LockerRentReceiptAllotmentWiseDto
    {
        public decimal AllotmentId { get; set; }
        public decimal LockerId { get; set; }
        public string? LockerNumber { get; set; }
        public string? SizeName { get; set; }
        public double RentAmount { get; set; }
        public double RentReceivable { get; set; }
        public double RentReceived { get; set; }
    }
}
