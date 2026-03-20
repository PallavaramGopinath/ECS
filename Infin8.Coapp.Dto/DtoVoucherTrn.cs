using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoVoucherTrn
    {
        public decimal Voc_Trn_Id { get; set; }
        public int Voc_Trn_Type { get; set; }
        public decimal Led_Id { get; set; }
        public string? Led_Name { get; set; }
        public double Voc_Rpt { get; set; } = 0; // Receipt amount
        public double Voc_Pmt { get; set; } = 0; // Payment amount
        public string? Voc_Narr { get; set; }
        public string? BrCode { get; set; }
    }
}
