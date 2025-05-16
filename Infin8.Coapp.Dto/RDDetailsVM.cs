namespace Infin8.Coapp.Dto
{
    public class RDDetailsVM
    {
        public int TD_Id { get; set; }
        public string? TD_No { get; set; }
        public int TDScheme_Id { get; set; }
        public bool IsCompoundInterest { get; set; }
        public double DepositAmount { get; set; }
        public DateTime ValueDate { get; set; }
        public double MaturityAmount { get; set; }
        public DateTime MaturityDate { get; set; }
        public int PeriodInMonths { get; set; }
        public double RateOfInterest { get; set; }
        public double AppliedRateOfInterest { get; set; }
        public int NoOfInstalmentReceived { get; set; }
        public double RDAmountReceived { get; set; }
        public double InterestPayable { get; set; }
        public DateTime IntCalcDate { get; set; }
        public int PendingInstalments { get; set; }
        public double PendingRD { get; set; }
        public double PenalCalculatedAmount { get; set; }
        public DateTime? PenalAppliedDate { get; set; }
        public double PenalReceivedAmount { get; set; }
        public double PICalc { get; set; }
        public DateTime? PICalcDate { get; set; }

        public DateTime? LastInstalmentDate { get; set; }

        public DateTime? LastReceiptDate { get; set; }

    }
}
