namespace Infin8.Coapp.Dto
{
    public class DepositPendingVM
    {
        public int DMId { get; set; }
        public string? Status { get; set; }
        public int ArrearDemandApplication { get; set; }
        public int MemId { get; set; }
        public int DMLedId { get; set; }
        public int DMPILedId { get; set; }
        public string? LedName { get; set; }
        public double PendingAmount { get; set; }
        public double ODAmount { get; set; }
        public double CalculatedAmount { get; set; }
        public DateTime? CalculatedDate { get; set; }
        public double RecoveryAmount { get; set; }
    }
}
