namespace Infin8.Coapp.Dto
{
    public class AccountsAppropriationPaymentDto
    {
        public int IsBankId { get; set; }
        public int Led_Id { get; set; }
        public string? Led_Name { get; set; }
        public double Voc_Pmt { get; set; }
        public string? ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }
        public string? BankName { get; set; }
    }
}
