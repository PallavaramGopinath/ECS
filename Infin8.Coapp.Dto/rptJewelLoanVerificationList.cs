namespace Infin8.Coapp.Dto
{
    public class rptJewelLoanVerificationList
    {
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public DateTime San_Date { get; set; }
        public double Disb_Amt { get; set; }
        public double RatePerGram { get; set; }
        public double GrossWeight { get; set; }
        public double NetWeight { get; set; }
        public double NetValue { get; set; }
        public double Loan_OS { get; set; }
        public string? JLO_Name { get; set; }
        public int JLO_Nos { get; set; }
        public double Wastage { get; set; }     
    }
}
