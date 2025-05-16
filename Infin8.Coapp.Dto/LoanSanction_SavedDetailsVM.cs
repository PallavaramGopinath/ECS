using Infin8.Coapp.Models;

namespace Infin8.Coapp.Dto
{
    public class LoanSanction_SavedDetailsVM
    {
        public Loan_Sanction? LoanSanctionedItem { get; set; }
        public List<LoanSanctionTr_SavedDetailsVM>? LoanSanctionTrList { get; set; }
    }
}
