namespace Infin8.Coapp.Dto
{
    public class AccountsReceiptVM
    {
        public string? ReceiptHead { get; set; }
        public string? Voc_Rpt_No { get; set; }
        public string? Voc_Rpt_Mode { get; set; }
        public DateTime Voc_Date { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? IntCalcDetails { get; set; }
        public double TotalReceiptAmount { get; set; }
        public string? RsInWords { get; set; }
        public string? AdjString { get; set; }
        public string? FirstSignatory { get; set; }
        public string? SecondSignatory { get; set; }
        public string? ThirdSignatory { get; set; }
        public List<AccountsReceiptTrnVM>  ReceiptItems { get; set; } = new List<AccountsReceiptTrnVM>();
    }
}
