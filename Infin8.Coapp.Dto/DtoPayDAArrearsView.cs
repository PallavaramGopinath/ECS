using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class DtoPayDAArrearsView
    {
        public decimal Pay_Id { get; set; }
        public decimal Employee_Id { get; set; }
        public string? Employee_Name { get; set; }
        public string? Designation { get; set; }
        public double DAArrears { get; set; }
        public double PF { get; set; }
        public double NetPay { get; set; }
    }
}
