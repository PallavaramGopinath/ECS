using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class DtoLoanRepaymentSchedule
    {
        public string? Dem_Status { get; set; }
        public DateTime  Due_Date { get; set; }
        public double Principal_Demand { get; set; }
        public double Interest_Demand { get; set; }
        public double Total_Demand { get; set; }
        public double Non_OD_Amt { get; set; }
        public double Loan_Outstanding { get; set; }
       
    }
}
