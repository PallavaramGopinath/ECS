using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoDayProcess
    {
        public DateTime ProcessDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? BrCode { get; set; }
        public decimal Created_By { get; set; }
        public decimal YrId { get; set; }
        public string? Message { get; set; }
    }
}
