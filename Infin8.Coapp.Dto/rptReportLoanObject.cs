using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class rptReportLoanObject
    {

        public int LoanType { get; set; }
        public int ReportId { get; set; }
        public DateTime AsOnDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? FromDateString { get; set; }
        public string? ToDateString { get; set; }
        public string? BrCode { get; set; }
        public List<decimal>? LoanNoList { get; set; }
    }

}
