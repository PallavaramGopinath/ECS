using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class rptReportEmployeeObject
    {
        public decimal Emp_Id { get; set; }
        public decimal Pay_Id { get; set; }
        public int ReportId { get; set; }
        public DateTime FromDate  { get; set; }
        public DateTime ToDate { get; set; }
        public string? BrCode { get; set; }
        public decimal Yr_Id { get; set; }
    }
}
