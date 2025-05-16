namespace Infin8.Coapp.Dto
{
    public class PaymentForCancellation
    {
        public int Led_Id { get; set; }
        public string? Led_Name { get; set; }
        public double ReceiptAmount { get; set; }
        public double PaymentAmount { get; set; }
        public long Mem_Id  { get; set; }
        public string? Mem_No { get; set; }
        public string? Mem_Name { get; set; }
        public int DM_Id { get; set; }
        public double IntCalcAmount { get; set; }
        public string? Status { get; set; }

    }
}
