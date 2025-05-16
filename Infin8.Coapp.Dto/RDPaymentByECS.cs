namespace Infin8.Coapp.Dto
{
    public class RDPaymentByECS
    {
        public int TD_Id { get; set; }
        public string? TD_No { get; set; }
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public string? TDH_Name { get; set; }
        public double MaturityAmount { get; set; }
        public DateTime MaturityDate { get; set; }
        public string? BankName { get; set; }
        public string? SBAccountNo { get; set; }
        public string? IFSCCode { get; set; }
        public int PeriodInMonths { get; set; }
        public double DepositReceiptAmount { get; set; }
        public DateTime LastReceiptDate { get; set; }
    }
}
