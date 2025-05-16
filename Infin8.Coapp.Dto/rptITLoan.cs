namespace Infin8.Coapp.Dto
{
    public class rptITLoan
    {
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public string? MemberType { get; set; }
        public string? PerNo { get; set; }
        public string? TokenNo { get; set; }
        public string? Address { get; set; }
        public string? PANNo { get; set; }
        public string? AadharNo { get; set; }
        public string? Loan_No { get; set; }
        public string? Scheme_Name { get; set; }
        public DateTime Disb_Date { get; set; }
        public double Loan_OpeningBalance { get; set; }
        public double Loan_Issued { get; set; }
        public double Loan_Collected { get; set; }
        public double Loan_ClosingBalance { get; set; }
        public double Roi { get; set; }
        public double Int_Collected { get; set; }
        public double EMI_Collected { get; set; }
        public double PI_Collected { get; set; }
        public string? Is_OS_MoreThan3Years { get; set; }
    }
}
