using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class LoanRepaymentScheduleVM
    {
        public int SlNo { get; set; }
        public DateTime DemandDate { get; set; }
        public double PrincipalDemand { get; set; }
        public double InterestDemand { get; set; }
        public double NonODPrincipal { get; set; }

    }
}
