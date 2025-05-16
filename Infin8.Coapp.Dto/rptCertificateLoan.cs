namespace Infin8.Coapp.Dto
{
    public class rptCertificateLoan
    {
        public int Mem_Id { get; set; }
        public int Loan_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public string? Loan_No { get; set; }
        public double PrlColl_Amt { get; set; }
        public double IntColl_Amt { get; set; }
        public string? Content { get; set; }
        public DateTime TrnDate { get; set; }
        public string? Purpose { get; set; }
    }
}
