namespace Infin8.Coapp.Dto
{
    public class FinVoucherEntriesForDelete
    {
        public int Voc_Id { get; set; }
        public string? Voc_No { get; set; }
        public DateTime Voc_Date { get; set; }
        public int Led_Id { get; set; }
        public string? Led_Name { get; set; }
        public double ReceiptAmount { get; set; }
        public double PaymentAmount { get; set; }
        public string? ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }
        public string? IssueBankName { get; set; }
        public string? Status { get; set; }
    }
}
