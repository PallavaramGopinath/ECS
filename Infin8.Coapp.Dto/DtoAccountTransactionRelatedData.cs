using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class DtoAccountTransactionRelatedData
    {
        public int Fnl_Id { get; set; }
        public string? Ledger_Name { get; set; }
        public double OB { get; set; }
        public double Receipt_Amount { get; set; }
        public double Payment_Amount { get; set; }
        public double CB { get; set; }
        public string? Narration { get; set; }
    }
}
