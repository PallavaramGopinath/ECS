using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class rptReportAuditObject
    {
        public int ReportId { get; set; }
        public int TrnType { get; set; }
        public int LoanType { get; set; }
        public string TDType { get; set; }
        public DateTime AsOnDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime  ToDate { get; set; }
        public string? FromDateString { get; set; }
        public string? ToDateString { get; set; }
        public int OrderById { get; set; }
        public int FnlId { get; set; }
        public decimal YrId { get; set; }
        public string BrCode { get; set; }
    }
}
