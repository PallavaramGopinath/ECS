namespace Infin8.Coapp.Dto
{
    public class DividendAndIntOnTDPaymentVM
    {
        public int DivPbleMaster_Id { get; set; }
        public int IntOnTDPbleMaster_Id { get; set; }
        public int SlNo { get; set; }
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? SBAccountNo { get; set; }
        public string? BankName { get; set; }
        public int OfficeId { get; set; }
        public string? ReferName { get; set; }
        public string? IFSCCode { get; set; }
        public int Dividend { get; set; }
        public int IntOnTD { get; set; }
        public int TotalPayable { get; set; }
    }
}
