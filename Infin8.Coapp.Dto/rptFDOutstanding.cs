namespace Infin8.Coapp.Dto
{
    public class rptFDOutstanding
    {
        public int TDScheme_Id { get; set; }
        public string? TDScheme_Name { get; set; }
        public decimal TD_Id { get; set; }
        public string? TD_No { get; set; }
        public string? TDH_Name { get; set; }
        public DateTime ValueDate { get; set; }
        public double DepositAmount { get; set; }
        public double DepositReceiptAmount { get; set; }
        public int PeriodInMonths { get; set; }
        public int PeriodInDays { get; set; }
        public double RateOfInterest { get; set; }
        public DateTime MaturityDate { get; set; }
        public double MaturityAmount { get; set; }
        public double InterestCalculatedAmount { get; set; }
        public DateTime? InterestAppliedDate { get; set; }
        public double InterestPaidAmount { get; set; }
        public double DepositPaidAmount { get; set; }
        public double PenalCalculatedAmount { get; set; }
        public DateTime? PenalAppliedDate { get; set; }
        public double PenalReceivedAmount { get; set; }
        public string? TDSchemeType { get; set; }
        public string? MemberNo { get; set; }
    }
}
