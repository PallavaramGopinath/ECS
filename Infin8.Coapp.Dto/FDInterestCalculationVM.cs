namespace Infin8.Coapp.Dto
{
    public class FDInterestCalculationVM
    {
        public int TD_Id { get; set; }
        public DateTime ValueDate { get; set; }
        public double DepositAmount { get; set; }
        public DateTime MaturityDate { get; set; }
        public double MaturityAmount { get; set; }
        public int InterestPayableFrequency { get; set; }
        public bool isDiscountRate { get; set; }
        public int PeriodInMonths { get; set; }
        public int PeriodInDays { get; set; }
        public double RateOfInterest { get; set; }
        public bool IsCompoundInterest { get; set; }
        public double InterestCalculatedAmount { get; set; }
        public DateTime? InterestAppliedDate { get; set; }
        public double InterestPaidAmount { get; set; }
    }
}
