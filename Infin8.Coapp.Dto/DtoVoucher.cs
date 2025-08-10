using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoVoucher
    {
        public decimal Voc_Id { get; set; }
        public string? Voc_Rpt_No { get; set; }
        public string? Voc_Pmt_No { get; set; }
        public DateTime Voc_Date { get; set; }
        public int Voc_Type { get; set; }
        public string? Voc_Narration { get; set; }
        public string brCode { get; set; }
        public List<DtoVoucherTrn> Transactions { get; set; } = new();
    }
}
