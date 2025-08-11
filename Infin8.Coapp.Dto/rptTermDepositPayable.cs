namespace Infin8.Coapp.Dto
{
    public class rptTermDepositPayable
    {
        public int TDScheme_Id { get; set; }
        public string? TDScheme_Name { get; set; }
        public string? TDSchemeType { get; set; }
        public decimal TD_Id { get; set; }
        public string? TD_No { get; set; }
        public DateTime Trn_Date { get; set; }
        public string? TDH_Name { get; set; }
        public double RateOfInterest { get; set; }
        public DateTime ValueDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public double DepositAmount { get; set; }
        public double DepositReceiptAmount { get; set; }
        public double InterestCalculatedAmount { get; set; }
        public double InterestPaidAmount { get; set; }
        public double IntPayable { get; set; }
        public double TotalPayable { get; set; }
        public DateTime? InterestAppliedDate { get; set; }
        public int PeriodInMonths { get; set; }
        public int PeriodInDays { get; set; }
    }
}
