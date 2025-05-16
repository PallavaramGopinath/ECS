namespace Infin8.Coapp.Dto
{
    public class FactoryRecoveryRefundRDVM
    {
        public int TDTrn_Id { get; set; }
        public int TD_Id { get; set; }
        public string? TD_No { get; set; }
        public double InstalmentAmount { get; set; }
        public double DepositReceiptAmount { get; set; }
        public double PenalReceivedAmount { get; set; }
        public int NoOfInstalments { get; set; }
        public int Led_Id { get; set; }
        public int IntLed_Id { get; set; }
        public int PiLed_Id { get; set; }

        public double DepositRefundAmount { get; set; }
        public double PenalRefundAmount { get; set; }
        public int NoOfInstalmentsRefund { get; set; }
    }
}
