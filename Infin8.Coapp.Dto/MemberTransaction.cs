namespace Infin8.Coapp.Dto
{
    public class MemberTransaction
    {
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? TokenNo { get; set; }
        public string? MemberName { get; set; }
        public byte Trn_Type { get; set; }
        public double BalanceAmount { get; set; }
        public int Led_Id { get; set; }
        public string? Led_Name { get; set; }
        public string? SBAccountNo { get; set; }
        public string? BankName { get; set; }
        public string? IFSCCode { get; set; }

    }
}
