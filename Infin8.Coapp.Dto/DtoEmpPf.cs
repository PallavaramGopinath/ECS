using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoEmpPf
    {
        public decimal PF_Id { get; set; }
        public decimal mem_id { get; set; }
        public int no_of_days { get; set; }
        public double epf_Interest { get; set; }
        public double bpf_Interest { get; set; }
        public double pf_Product { get; set; }
        public double bpf_product { get; set; }
        public DateTime int_calc_upto { get; set; }
        public double pf_balance { get; set; }
        public double vpf_balance { get; set; }
        public double bpf_balance { get; set; }
    }
}
