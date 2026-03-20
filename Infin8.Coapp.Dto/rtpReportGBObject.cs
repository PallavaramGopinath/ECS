using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class rtpReportGBObject
    {
        public int ReportId { get; set; }
        public decimal PbleMasterId { get; set; }
        public int PbleType { get; set; }
        public DateTime AsOnDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? FromDateString { get; set; }
        public string? ToDateString { get; set; }
        public decimal YrId { get; set; }
        public string? BrCode { get; set; }
    }
}
