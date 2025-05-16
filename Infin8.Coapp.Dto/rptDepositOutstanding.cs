namespace Infin8.Coapp.Dto
{
    public class rptDepositOutstanding
    {
        public int Mem_Id { get; set; }
        public int DM_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? Token_PersonNo { get; set; }
        public double Deposit_OB { get; set; }
        public double Interest_OB { get; set; }
        public double Receipt_Amt { get; set; }
        public double Paid_Amt { get; set; }
        public double InterestCalculated_Amt { get; set; }
        public double InterestPaid_Amt { get; set; }
        public int OfficeId { get; set; }
        public string? ReferName { get; set; }
    }
}
