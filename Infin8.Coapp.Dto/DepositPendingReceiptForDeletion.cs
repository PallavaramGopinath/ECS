namespace Infin8.Coapp.Dto
{
    public class DepositPendingReceiptForDeletion
    {
        public int DM_Id { get; set; }
        public string? DMName { get; set; }
        public double Receipt_Amt { get; set; }
        public double PenalInterestCollected_Amt { get; set; }
        public double PenalInterestCalculated_Amt { get; set; }
        public DateTime? PenalInterestCalculated_Date { get; set; }
    }
}
