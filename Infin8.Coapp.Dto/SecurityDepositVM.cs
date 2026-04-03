namespace Infin8.Coapp.Dto
{
    public class SecurityDepositVM
    {
        public decimal TD_Id { get; set; }
        public string? TD_No { get; set; }
        public decimal Mem_Id { get; set; }
        public DateTime ValueDate { get; set; }
        public double DepositAmount { get; set; }
        public double RateOfInterest { get; set; }
        public DateTime InterestAppliedDate { get; set; }
        public double InterestCalculatedAmount { get; set; }
        public double InterestPaidAmount { get; set; }
        public double IntPayableAmount { get; set; }
        public double CurrentIntCalcAmount { get; set; }
        public DateTime CurrentIntCalcDate { get; set; }
        public double DepositPaidOnSeparation { get; set; }
        public decimal Led_Id { get; set; }
        public decimal IntLed_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
