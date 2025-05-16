using Infin8.Coapp.Models;

namespace Infin8.Coapp.Dto
{
    public class TDLoanBaseVM
    {
        public Loan_Master? LoanMaster { get; set; }
        public Loan_Disb? LoanDisbursement { get; set; }
        public Loan_Trn? LoanTrn { get; set; }
        public Loan_Roi? LoanRoi { get; set; }
        public Lien? LoanLien { get; set; }
        public List<Lien_Trn>? LoanLienTr { get; set; }
    }

}
