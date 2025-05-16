namespace Infin8.Coapp.Dto
{
    public class DepositLedgerVM
    {
        public int DD_Id { get; set; }
        public int DM_Id { get; set; }
        public int Mem_Id { get; set; }
        public string? DDStatus { get; set; }
        public DateTime DDTrn_Date { get; set; }
        public DateTime? DDDue_Date { get; set; }
        public double? Demand_Amt { get; set; }
        public double? Receipt_Amt { get; set; }
        public double? Paid_Amt { get; set; }
        public double? InterestCalculated_Amt { get; set; }
        public DateTime? InterestCalculated_Date { get; set; }
        public double? InterestPaid_Amt { get; set; }
        public double? PenalInterestCalculated_Amt { get; set; }
        public DateTime? PenalInterestCalculated_Date { get; set; }
        public double? PenalInterestCollected_Amt { get; set; }
        public double? Bal_Amt { get; set; }
        public string? DMName { get; set; }
        public string? Voc_No { get; set; }
        public string? Voc_Rpt_No { get; set; }
        public int? Voc_Type { get; set; }
        public double? Int_Bal { get; set; }

        public double Prl_OB { get; set; }
        public double Int_OB { get; set; }
        public double PI_OB { get; set; }
        public double PI_CB { get; set; }
        public int Trn_SlNo { get; set; }

    }
}
