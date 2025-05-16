namespace Infin8.Coapp.Dto
{
    public class rptITFixedDeposit
    {
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? TokenNo { get; set; }
        public string? MemberName { get; set; }
        public string? MemberType { get; set; }
        public string? Address { get; set; }
        public string? PANNo { get; set; }
        public string? AadharNo { get; set; }
        public int TD_Id { get; set; }
        public string? TD_No { get; set; }
        public string? TDScheme_Name { get; set; }
        public double RateOfInterest { get; set; }
        public double Deposit_OB { get; set; }
        public double Deposit_Received { get; set; }
        public double Deposit_Paid { get; set; }
        public double Deposit_CB { get; set; }
        public double Interest_Paid { get; set; }
    }
}
