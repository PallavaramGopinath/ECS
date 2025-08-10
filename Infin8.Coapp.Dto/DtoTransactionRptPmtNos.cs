using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoTransactionRptPmtNos
    {
        public int Voc_Rpt_SlNo { get; set; }
        public string? Voc_Rpt_No { get; set; }
        public string? Voc_Rpt_Mode { get; set; }
        public int Voc_Pmt_SlNo { get; set; }
        public string? Voc_Pmt_No { get; set; }
        public string? Voc_Pmt_Mode { get; set; }
    }
}
