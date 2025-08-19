namespace Infin8.Coapp.Dto
{
    public class AccountTransactionVM
    {
        public decimal Staging_Id { get; set; }
        public decimal Created_By { get; set; }
        public DateTime Created_Date { get; set; }
        public int CashAdjId { get; set; }
        public decimal LedId { get; set; }
        public int AccountId { get; set; }
        public string? AccountName { get; set; }
        public double ReceiptAmount { get; set; }
        public double PaymentAmount { get; set; }
        public string? ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }
        public string? IssueBankName { get; set; }
        public string? Status { get; set; }
        public decimal Member_Id { get; set; }
        public string? Member_No { get; set; }
        public string? PerNo { get; set; }
        public string? Member_Name { get; set; }
        public double CashReceipt { get; set; }
        public double CashPayment { get; set; }
        public string? Narration { get; set; }
        //public int DMId { get; set; }
    }
}
