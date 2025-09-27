using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoAccountsBalance
    {
        public decimal Ledger_Id { get; set; }
        public int Grp_Id { get; set; }
        public int Fnl_Id { get; set; }
        public string? Ledger_Name { get; set; }
        public string? Group_Name { get; set; }
        public string? Final_Name { get; set; }
        public DateTime? Accounting_Date { get; set; }
        public double Opening_Balance { get; set; }
        public double Receipt { get; set; }
        public double Payment { get; set; }
        public double Closing_Balance { get; set; }
        public decimal Yr_Id { get; set; }
    }
}
