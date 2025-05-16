namespace Infin8.Coapp.Dto
{
    public class rptLoanReimbursement
    {
        public string? MemberName { get; set; }
        public string? PerNo { get; set; }
        public string? MemberNo { get; set; }
        public string? SuretymemberName { get; set; }
        public string? SuretyPerNo { get; set; }
        public string? SuretymemberNo { get; set; }
        public DateTime SanctionDate { get; set; }
        public string? Loan_No { get; set; }
        public double GrossPay { get; set; }
        public double LoanAmount { get; set; }
        public int PeriodOfLoan { get; set; }
        public double BP25Times { get; set; }
        public double LoanRecovery { get; set; }
        public double NetAmount { get; set; }
        public string? SBAccountNo { get; set; }
        public string? BankName { get; set; }
        public int RecScheme_Id { get; set; }
        public double BP50Percentage { get; set; }
        public DateTime? DORMember { get; set; }
        public DateTime? DORSuretyMember { get; set; }
    }
}
