namespace Infin8.Coapp.Dto
{
    public class RDTrnList
    {
        public int TD_Id { get; set; }
        public string? TD_No { get; set; }
        public string? TDScheme_Name { get; set; }
        public DateTime Trn_Date { get; set; }
        public double DepositReceiptAmount { get; set; }
        public double DepositPaidAmount { get; set; }
        public double InterestCalculatedAmount { get; set; }
        public double InterestPaidAmount { get; set; }
        public double PenalCalculatedAmount { get; set; }
        public double PenalReceivedAmount { get; set; }
        public double Deposit_OB { get; set; }
        public double Int_OB { get; set; }
        public double PI_OB { get; set; }
        public double Deposit_CB { get; set; }
        public double Int_CB { get; set; }
        public double PI_CB { get; set; }
        public string? Voc_No { get; set; }
        public string? Voc_Rpt_No { get; set; }
        public int? Voc_Type { get; set; }
        public int Trn_SlNo { get; set; }

    }
}
