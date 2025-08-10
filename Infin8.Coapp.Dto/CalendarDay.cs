using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class CalendarDay
    {
        public decimal Day_Id { get; set; }
        public decimal Calendar_Id { get; set; }
        public DateTime Calendar_Date { get; set; }
        public string Day_Type { get; set; } // Working, Holiday, Festival, etc.
        public string Description { get; set; }
        public bool Is_Day_Begin { get; set; }
        public bool Is_Day_End { get; set; }
        public string FinancialYear { get; set; }

        public CalendarMaster Calendar { get; set; }
    }
}
