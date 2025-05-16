namespace Infin8.Coapp.Dto
{
    public class DepositListOE
    {
        public int DD_Id { get; set; }
        public int DM_Id { get; set; }
        public int Mem_Id { get; set; }
        public string? DMName { get; set; }
        public DateTime DDTrn_Date { get; set; }
        public double? Receipt_Amt { get; set; }
        public double? Paid_Amt { get; set; }
        public double? InterestCalculated_Amt { get; set; }
        public DateTime? InterestCalculated_Date { get; set; }
        public double? InterestPaid_Amt { get; set; }
        public double? PenalInterestCalculated_Amt { get; set; }
        public DateTime? PenalInterestCalculated_Date { get; set; }
        public double? PenalInterestCollected_Amt { get; set; }
        public int Voc_Id { get; set; }
        public int Trn_SlNo { get; set; }
    }
}
