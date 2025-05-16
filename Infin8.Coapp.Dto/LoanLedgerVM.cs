using Infin8.Coapp.Models;
namespace Infin8.Coapp.Dto
{
    public class LoanLedgerVM
    {
        public LoanHeaderDetailsVM?  Header { get; set; }
        public List<Loan_Roi>?  LoanROI { get; set; }
        public List<Loan_Inst>? LoanInst { get; set; }
        public List<LoanTransactionVM>? Transactions { get; set; }
    }
}
