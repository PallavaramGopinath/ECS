namespace Infin8.Coapp.Dto
{
    public class rptTDRefundBetweenDates
    {
        public DateTime Trn_Date { get; set; }
        public decimal TD_Id { get; set; }
        public string? TD_No { get; set; }
        public int TDScheme_Id { get; set; }
        public string? TDScheme_Name { get; set; }
        public decimal Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public decimal Voc_Id { get; set; }
        public double DepositPaidAmount { get; set; }

    }
}
