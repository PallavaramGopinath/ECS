namespace Infin8.Coapp.Dto
{
    public class rptCertificateFDPayment
    {
        public int Mem_Id  { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public int TD_Id { get; set; }
        public string? TD_No { get; set; }
        public double DepositAmount { get; set; }
        public string? TDH_Name { get; set; }
        public string? PerNo { get; set; }
        public double InterestPaidAmount { get; set; }
        public double DepositPaidAmount { get; set; }
        public string? Content { get; set; }
    }
}
