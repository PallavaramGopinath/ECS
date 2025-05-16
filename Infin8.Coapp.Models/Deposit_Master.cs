namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Deposit_Master
    {
        [Key]
        public int DM_Id { get; set; }
        public string? DMName { get; set; }
        public int DMLed_id { get; set; }
        public int DMIntled_id { get; set; }
        public int DMPILed_Id { get; set; }
        public double DMPercentage { get; set; }
        public double DMMaximumAmount { get; set; }
        public double DMFixedAmount { get; set; }
        public string? DMFrequency { get; set; }
        public int DemandMonth { get; set; }
        public bool DM_Delete { get; set; }
        public bool IsInterestTransferToDeposit { get; set; }
        public double PercentageOfInterestTransferToDeposit { get; set; }
        public double PercentageOfInterestTransferToLedger { get; set; }
        public int TransfereeLedger_Id { get; set; }
        public bool IsOptionAllowed { get; set; }
        public int ArrearDemandApplication { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public int InterestCalcPeriod { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
