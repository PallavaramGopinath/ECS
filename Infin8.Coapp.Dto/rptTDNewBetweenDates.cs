namespace Infin8.Coapp.Dto
{
    public class rptTDNewBetweenDates
    {
        public int TDScheme_Id { get; set; }
        public string? TDScheme_Name { get; set; }
        public string? TDSchemeType { get; set; }
        public string? TD_No { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public string? TDH_Name { get; set; }
        public DateTime AccountOpenDate { get; set; }
        public DateTime ValueDate { get; set; }
        public double DepositAmount { get; set; }
        public int PeriodInMonths { get; set; }
        public int PeriodInDays { get; set; }
        public double RateOfInterest { get; set; }
        public DateTime MaturityDate { get; set; }
        public double MaturityAmount { get; set; }
        public int InterestPayableFrequency { get; set; }
        public string? TDFrequency { get; set; }
    }
}
