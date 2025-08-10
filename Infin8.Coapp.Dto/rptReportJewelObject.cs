using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class rptReportJewelObject
    {
        public int ReportId { get; set; }
        public DateTime AsOnDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public double MaximumLimit { get; set; }
        public string? FromDateString { get; set; }
        public string? ToDateString { get; set; }
        public decimal YrId { get; set; }
        public decimal MemId { get; set; }
        public string? MemberNo { get; set; }
        public string? DistrictName { get; set; }
        public string? RegionalOfficeName { get; set; }
        public string? BrCode { get; set; }
        public List<decimal>? LoanNoList { get; set; }
    }
}
