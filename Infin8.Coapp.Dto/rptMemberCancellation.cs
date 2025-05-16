namespace Infin8.Coapp.Dto
{
    public class rptMemberCancellation
    {
        public decimal Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? Token_PersonNo { get; set; }
        public string? Fvb_Cheque_No { get; set; }
        public DateTime? Fvb_Cheque_Date { get; set; }
        public double Voc_Rpt { get; set; }
        public double Voc_Pmt { get; set; }
        public string? Led_Name { get; set; }
    }
}
