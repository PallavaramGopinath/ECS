namespace Infin8.Coapp.Dto
{
    public class rptDividendTDFWDWorking
    {
        public int PbleMaster_Id { get; set; }
        public int PbleType { get; set; }
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? TokenNo { get; set; }
        public DateTime Trn_Date { get; set; }
        public int Receipt_Amount { get; set; }
        public int Payment_Amount { get; set; }
        public int Closing_Balance { get; set; }
        public int NoOfDays { get; set; }
        public int Interest_Amount { get; set; }
    }
}
