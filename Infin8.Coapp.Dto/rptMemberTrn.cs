namespace Infin8.Coapp.Dto
{
    public class rptMemberTrn
    {
        public decimal Mem_Id   { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public decimal Mem_Trn_Id { get; set; }
        public DateTime Trn_Date { get; set; }
        public decimal Led_Id { get; set; }
        public string? Led_Name { get; set; }
        public short Trn_Type { get; set; }
        public decimal Acc_Id { get; set; }
        public double Rpt_Amt { get; set; }
        public double Pmt_Amt { get; set; }
        public double Total_Amt { get; set; }
        public double Amt_CB { get; set; }
        public double Amt_OB { get; set; }
        public int Trn_SlNo { get; set; }
        public string? Voc_No { get; set; }
    }
}
