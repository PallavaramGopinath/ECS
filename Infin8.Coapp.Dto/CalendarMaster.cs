using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class CalendarMaster
    {
        public decimal Calendar_Id { get; set; }
        public decimal  Yr_Id { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
        public string Created_By { get; set; }
        public DateTime? Updated_At { get; set; }
        public string Updated_By { get; set; }

        //public List<CalendarDay> CalendarDays { get; set; } = new();
    }
}
