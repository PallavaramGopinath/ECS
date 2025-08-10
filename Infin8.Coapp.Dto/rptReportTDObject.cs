using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class rptReportTDObject
    {
        public int ReportId { get; set; }
        public string? TDSchemeType { get; set; }
        public DateTime AsOnDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? FromDateString { get; set; }
        public string? ToDateString { get; set; }
        public decimal YrId { get; set; }
        public decimal MemId { get; set; }
        public string? MemberNo { get; set; }
        public string? BrCode { get; set; }
        public List<decimal>? TDList { get; set; }
    }
}
