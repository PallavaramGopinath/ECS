namespace Infin8.Coapp.Dto
{
    public class DividendPaymentVM
    {
        public int DivPbleMaster_Id { get; set; }
        public int SlNo { get; set; }
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? dividendYear { get; set; }
        public int dividend { get; set; }
        public string? SBAccountNo { get; set; }
        public string? BankName { get; set; }
        public string? IFSCCode { get; set; }

    }
}
