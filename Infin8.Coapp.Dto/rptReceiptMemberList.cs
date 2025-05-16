namespace Infin8.Coapp.Dto
{
    public class rptReceiptMemberList
    {
        public int SlNo { get; set; }
        public int Voc_Type { get; set; }
        public string? Voc_Rpt_No { get; set; }
        public DateTime Voc_Date { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? Led_Name { get; set; }
        public string? Description { get; set; }
        public double Voc_Rpt { get; set; }
        public int Grp_SlNo { get; set; }
        public int Led_SlNo { get; set; }
        public string? Led_Desc { get; set; }
        public string?  ReceipentInVoucher { get; set; }

        public double Outstanding { get; set; }
        public double Balance { get; set; }
    }
}
