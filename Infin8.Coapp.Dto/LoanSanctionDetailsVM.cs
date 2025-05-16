namespace Infin8.Coapp.Dto
{
    public class LoanSanctionDetailsVM
    {
        public int LoanSanction_Id { get; set; }
        public int Mem_Id { get; set; }
        public int SurityMem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public DateTime SanctionDate { get; set; }
        public double LoanAmount { get; set; }
        public int Scheme_Id { get; set; }
        public string? Scheme_Name { get; set; }
    }
}
