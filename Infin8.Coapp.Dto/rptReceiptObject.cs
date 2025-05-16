using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class rptReceiptObject
    {
        public int vocTrnType { get; set; }
        public decimal vocId { get; set; }
        public string? brCode { get; set; }
        public double receiptAmt { get; set; }
    }
}
