using Infin8.Coapp.Models;
namespace Infin8.Coapp.Dto
{
    public class TDLedgerTrnVM
    {
        public TermDeposit_Trn?  TermDepositTrn { get; set; }
        public string? Voc_No { get; set; }
        public double InterestPayableAmount { get; set; }
        public double DepositBalanceAmount { get; set; }
        public double PenalInterestReceivable { get; set; }

    }
}
