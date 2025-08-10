using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class DtoSBAccountBalanceWithIds
    {
        public decimal Acc_Id { get; set; }
        public string? Acc_No { get; set; }
        public decimal SBCA_Led_Id { get; set; }
        public double Balance_Amount { get; set; }
    }
}
