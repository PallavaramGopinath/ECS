namespace Infin8.Coapp.Dto
{
    public class SecurityDepositVM
    {
        public int TD_Id { get; set; }
        public int Mem_Id { get; set; }
        public DateTime ValueDate { get; set; }
        public double DepositAmount { get; set; }
        public double RateOfInterest { get; set; }
        public DateTime? InterestAppliedDate { get; set; }
        public double InterestCalculatedAmount { get; set; }
        public double InterestPaidAmount { get; set; }
        public double IntPayableAmount { get; set; }
        public double IntCalcAmount { get; set; }
        public DateTime IntCalcDate { get; set; }
        public int Led_Id { get; set; }
        public int IntLed_Id { get; set; }
    }
}
