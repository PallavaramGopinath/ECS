namespace Infin8.Coapp.Dto
{
    /// <summary>FD interest payment certificate (report 119): one row per fixed deposit with payments in the period.</summary>
    public class rptCertificateFDPayment
    {
        public decimal Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public string? PerNo { get; set; }
        public decimal TD_Id { get; set; }
        public string? TD_No { get; set; }
        public double DepositAmount { get; set; }
        public string? TDH_Name { get; set; }
        public double InterestPaidAmount { get; set; }
        public double DepositPaidAmount { get; set; }
        public string? Content { get; set; }
    }
}
