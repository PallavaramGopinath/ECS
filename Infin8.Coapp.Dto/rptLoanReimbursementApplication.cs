namespace Infin8.Coapp.Dto
{
    public class rptLoanReimbursementApplication
    {
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? TokenNo { get; set; }
        public DateTime? Dob { get; set; }
        public DateTime? DOR { get; set; }
        public double GrossPay { get; set; }
        public double GrossPay25Times { get; set; }
        public double GrossPay50Pecentage { get; set; }
        public double GrossPay25Percentage { get; set; }
        public double NetPay { get; set; }
        public double LoanOS { get; set; }
        public double NetLoanAmount { get; set; }
        public int NoOfInstalments { get; set; }
        public string? SBAccountNo { get; set; }
        public string? BankName { get; set; }
        public string? IFSCCode { get; set; }
    }
}
