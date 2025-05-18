using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class DtoAccount_Transactions
    {
        public int Acc_Id { get; set; }
        public string? Acc_Name { get; set; }
        public string? Acc_Type { get; set; }
        public string? Acc_Status { get; set; }
        public string? Component_Name { get; set; }
    }
}
