using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Utility
{
    public  class AppState
    {
        public string? UserName { get; set; }
        public decimal UserId { get; set; }
        public string? BrCode { get; set; }
        public DateTime? Yr_FromDate { get; set; }
        public DateTime? Yr_ToDate { get; set; }
        public decimal Yr_Id { get; set; }
        public int SocietyType { get; set; }
    }
}
