namespace Infin8.Coapp.Dto
{
    /// <summary>Loan repayment certificate (report 117): one loan's principal and interest paid in a period.</summary>
    public class rptCertificateLoan
    {
        public decimal Mem_Id { get; set; }
        public decimal Loan_Id { get; set; }
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
