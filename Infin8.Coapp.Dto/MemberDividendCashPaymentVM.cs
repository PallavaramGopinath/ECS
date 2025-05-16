namespace Infin8.Coapp.Dto
{
    public class MemberDividendCashPaymentVM
    {
        public int PbleCash_Id { get; set; }
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? Status { get; set; }
        public DateTime PbleCash_Date { get; set; }
    }
}
