namespace Infin8.Coapp.Dto
{
    /// <summary>One member ledger line of a member suspense bulk payment voucher (report 91, MemberTransactionBulk.rdlc).</summary>
    public class rptMemberSuspenseBulk
    {
        public decimal Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? Led_Name { get; set; }
        public double Rpt_Amt { get; set; }
        public double Pmt_Amt { get; set; }
        public string? Voc_No { get; set; }
        public DateTime Trn_Date { get; set; }
    }
}
