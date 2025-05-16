namespace Infin8.Coapp.Dto
{
    public class rptMemberNewAdmission
    {
        public decimal Mem_Id { get; set; }
        public string? MemberNo  { get; set; }
        public string? MemberName { get; set; }
        public string? PerNo { get; set; }
        public string? FatherName { get; set; }
        public DateTime? AdmissionDate { get; set; }
        public DateTime Trn_Date { get; set; }
        public double Rpt_Amt { get; set; }
    }
}
