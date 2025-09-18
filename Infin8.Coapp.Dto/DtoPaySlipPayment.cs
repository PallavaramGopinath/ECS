using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoPaySlipPayment
    {
        public decimal Pay_Id { get; set; }
        public decimal Employee_Id { get; set; }
        public List<decimal>? Employee_Id_List { get; set; }
        public double GrossPay { get; set; }
        public double TotalDeductions { get; set; }
        public double NetPay { get; set; }
        public double PF { get; set; }
        public  double TotalCommitments { get; set; }
        public string? BrCode { get; set; }
        public decimal YrId { get; set; }
        public decimal Created_By { get; set; }
    }
}
