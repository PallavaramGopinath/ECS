using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{

    public partial class Loan_Schemes
    {
        [Key]
        public int Scheme_Id { get; set; }
        public int Grp_Id { get; set; }
        public string? Scheme_Name { get; set; }
        public string? Is_MemberOrStaff { get; set; }
        public bool Is_Member { get; set; }
        public bool Is_Staff { get; set; }
        public int Loan_Type { get; set; }
        public int Disb_Type { get; set; }
        public bool IsCompound_Int { get; set; }
        public int Compound_Prd { get; set; }
        public int Inst_Type { get; set; }
        public int Dem_Frequency { get; set; }
        public int Int_Frequency { get; set; }
        public int Int_Application { get; set; }
        public int PI_Application { get; set; }
        public int IOD_Application { get; set; }
        public int Adv_Prl_Application { get; set; }
        public decimal PILed_Id { get; set; }
        public decimal IODLed_Id { get; set; }
        public decimal IntLed_Id { get; set; }
        public decimal PrlLed_Id { get; set; }
        public decimal ReimPILed_Id { get; set; }
        public decimal ReimIODLed_Id { get; set; }
        public decimal ReimIntLed_Id { get; set; }
        public decimal ReimPrlLed_Id { get; set; }
        public double MaximumLoanAmount { get; set; }
        public int MaximumPrincipalPeriod { get; set; }
        public int MaximumInterestPeriod { get; set; }
        public bool Scheme_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public bool IsDeductBalances { get; set; }
        public bool IntFromTemplate { get; set; }
        public int StaffLoan_Int_Type { get; set; }
        public bool IsStaffAdvance { get; set; }
        public string? LoanNoStartWith { get; set; }
        public int AdoptLoanEligibility { get; set; }
        public int DeductPreviousLoans { get; set; }
        public int MatchShareCapital { get; set; }
        public int AdoptLoanLimit { get; set; }
        public int IssurityMemberRequired { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
