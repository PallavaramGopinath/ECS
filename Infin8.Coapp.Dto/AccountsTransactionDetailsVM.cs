namespace Infin8.Coapp.Dto
{
    public class AccountsTransactionDetailsVM
    {
        public int Voc_Id { get; set; }
        public string? Voc_No { get; set; }
        public int Voc_Rpt_SlNo { get; set; }
        public string? Voc_Rpt_No { get; set; }
        public int Voc_Pmt_SlNo { get; set; }
        public string? Voc_Pmt_No { get; set; }
        public string? Voc_Date { get; set; }
        public double Voc_Amt { get; set; }
        public int Voc_Type { get; set; }
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
    }
}
