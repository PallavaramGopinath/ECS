namespace Infin8.Coapp.Dto
{
    public class TransactionEntriesForDelete
    {
        public int Voc_Id { get; set; }
        public string? Voc_No { get; set; }
        public int AccountId { get; set; }
        public string? AccountName { get; set; }
        public double ReceiptAmount { get; set; }
        public double PaymentAmount { get; set; }
        public string? ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }
        public string? IssueBankName { get; set; }
        public string? Status { get; set; }
        public int? MemId { get; set; }
        public string? MemNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemName { get; set; }
        public double CashReceipt { get; set; }
        public double CashPayment { get; set; }
        public int DMId { get; set; }
        public DateTime Voc_Date { get; set; }
        public int Led_Id { get; set; }
    }
}
