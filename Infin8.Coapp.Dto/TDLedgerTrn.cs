namespace Infin8.Coapp.Dto
{
    public class TDLedgerTrn
    {
        public int TD_Id { get; set; }
        public DateTime Trn_Date { get; set; }
        public string? Voc_No { get; set; }
        public double DepositReceiptAmount { get; set; }
        public double InterestCalculatedAmount { get; set; }
        public DateTime? InterestAppliedDate { get; set; }
        public double InterestPaidAmount { get; set; }
        public double InterestPayableAmount { get; set; }
        public double PenalCalculatedAmount { get; set; }
        public DateTime? PenalAppliedDate { get; set; }
        public double PenalReceivedAmount { get; set; }
        public double PenalReceivableAmount { get; set; }
        public double DepositPaidAmount { get; set; }
        public int Voc_Id { get; set; }
    }
}
