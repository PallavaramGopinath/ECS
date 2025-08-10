using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class Json_TermDeposit_Trn
    {
        public decimal Td_Id { get; set; }
        public double Interest_Calculated { get; set; }
        public DateTime? Interest_Applied_Date { get; set; }
        public double Interest_Paid { get; set; }
        public double Deposit_Paid { get; set; }
        public double Penal_Calcualted { get; set; }
        public DateTime? Penal_Applied_Date { get; set; }
        public int Non_Of_Instalments { get; set; }
    }
}
