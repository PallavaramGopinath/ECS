using Infin8.Coapp.Models;
namespace Infin8.Coapp.Dto
{
    public class TDLedgerVM 
    {
        public TermDeposit_Master? TDMaster { get; set; }
        public string? TDScheme_Name { get; set; }
        //public List<TDLedgerTrnVM> RDTransactions { get; set; }
        public List<TDLedgerTrn>? TDTrnList { get; set; }
    }
}
