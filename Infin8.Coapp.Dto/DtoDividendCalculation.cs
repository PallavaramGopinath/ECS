using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class DtoDividendCalculation
    {
        public int PbleType { get; set; }
        public int Action { get; set; }
        public decimal Ledger_Id { get; set; }
        public DateTime CurrentDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public double Roi { get; set; } = 14;
        public DateTime CalculatedDate { get; set; }
        public DateTime TransferedDate { get; set; }
        public decimal CreatedBy { get; set; }
        public decimal YrId { get; set; }
        public string? BrCode { get; set; }
    }
}
