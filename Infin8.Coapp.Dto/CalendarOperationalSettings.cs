using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class CalendarOperationalSettings
    {
        public decimal Setting_Id { get; set; }
        public decimal Calendar_Id { get; set; }
        public TimeSpan Day_Begin_Time { get; set; } = new TimeSpan(9, 0, 0);
        public TimeSpan Day_End_Time { get; set; } = new TimeSpan(17, 0, 0);
        public string Working_Days { get; set; } = "Mon-Fri";
        public decimal Yr_Id { get; set; }

        //public CalendarMaster Calendar { get; set; }
    }
}
