namespace Infin8.Coapp.Dto
{
    public class DepositBalanceVM
    {
        public int MemId { get; set; }
        public int DMId { get; set; }
        public DateTime TrnDate { get; set; }
        public int PrlLedId { get; set; }
        public int IntLedId { get; set; }
        public int PILedId { get; set; }
        public string? DMName { get; set; }
        public int ArrearDemandApplication { get; set; }
        public double DemandAmount { get; set; }
        public double ReceiptAmount { get; set; }
        public double PaidAmount { get; set; }
        public double BalanceAmount { get; set; }
        public double IntAlreadyCalculated { get; set; }
        public DateTime? IntAlreadyCalculatedDate { get; set; }
        public double IntPaidAmount { get; set; }
        public double? PIAlreadyCalculated { get; set; }
        public DateTime? PIAlreadyCalculatedDate { get; set; }
        public double PICollected { get; set; }
        public double IntNowCalculated { get; set; }
        public DateTime? IntNowCalculatedDate { get; set; }
        public double PINowCalculated { get; set; }
        public DateTime? PINowCalculatedDate { get; set; }
        public string? MemNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemName { get; set; }
    }
}
