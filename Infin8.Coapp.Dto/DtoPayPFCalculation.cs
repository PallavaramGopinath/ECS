using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class DtoPayPFCalculation
    {
        public DateTime CurrentDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<RateOfInterestVM>? RoiList { get; set; }
        public decimal UsrId { get; set; }
        public decimal YrId { get; set; }
        public string? BrCode { get; set; }
    }
}
