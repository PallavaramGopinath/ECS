using Infin8.Coapp.Models;
namespace Infin8.Coapp.Dto
{
    public class FDRenewalObject
    {
        public double DepositAmount { get; set; }
        public string? TDH_Name { get; set; }
        public int TDH_Age { get; set; }
        public string? Nominee1Name { get; set; }
        public int Nominee1Age { get; set; }
        public string? Nominee1Relationship { get; set; }
        public string? Nominee2Name { get; set; }
        public int Nominee2Age { get; set; }
        public string? Nominee2Relationship { get; set; }
        public List<MemberDetailsVM>? FDMembersList { get; set; }

        /// TermDeposit Trn
        public List<TermDeposit_Trn>? FDTrnList { get; set; }
        /// Fin voucher Tr
        public List<Fin_Voucher_Trn>? FinVoucherTrList { get; set; }
        /// Loan Tr
        public List<Loan_Trn>? FDLoanTrnList { get; set; }
    }
}
