using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class rptReportAccountObject
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? FromDateString { get; set; }
        public string? ToDateString { get; set; }
        public decimal YrId { get; set; }
        public int ReportId { get; set; }
        public List<decimal>? LedgerList { get; set; }
        public string? BrCode { get; set; }
    }
}
