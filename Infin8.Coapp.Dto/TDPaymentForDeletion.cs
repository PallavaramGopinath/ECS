namespace Infin8.Coapp.Dto
{
    public class TDPaymentForDeletion
    {
        public int TD_Id { get; set; }
        public string? TD_No { get; set; }
        public int TDScheme_Id { get; set; }
        public string? TDH_Name { get; set; }
        public DateTime ValueDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public double DepositAmount { get; set; }
        public int PeriodInMonths { get; set; }
        public int PeriodInDays { get; set; }
        public double RateOfInterest { get; set; }
        public double MaturityAmount { get; set; }
        public double InterestPaidAmount { get; set; }
        public double DepositPaidAmount { get; set; }

    }
}
