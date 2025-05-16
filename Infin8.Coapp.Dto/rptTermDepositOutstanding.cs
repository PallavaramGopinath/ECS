namespace Infin8.Coapp.Dto
{
    public class rptTermDepositOutstanding
    {
        public decimal TD_Id { get; set; }
        public string? TD_No { get; set; }
        public DateTime ValueDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public double RateOfInterest { get; set; }
        public bool IsCompoundInterest { get; set; }
        public int TDScheme_Id { get; set; }
        public string? TDScheme_Name { get; set; }
        public decimal Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? Token_PersonNo { get; set; }
        public double DepositAmount { get; set; }
        public double MaturityAmount { get; set; }
        public double Deposit_OB { get; set; }
        public double Interest_OB { get; set; }
        public double DepositReceiptAmount { get; set; }
        public double DepositPaidAmount { get; set; }
        public double InterestCalculatedAmount { get; set; }
        public DateTime? InterestAppliedDate { get; set; }
        public double InterestPaidAmount { get; set; }
    }
}
