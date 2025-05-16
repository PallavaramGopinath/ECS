namespace Infin8.Coapp.Dto
{
    public class LoanSanctionDataECSVM
    {
        public int LoanSanction_Id { get; set; }
        public DateTime SanctionDate { get; set; }
        public int Mem_Id { get; set; }
        public int SlNo { get; set; }
        public int Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public int DM_Id { get; set; }
        public string? Status { get; set; }
        public int Led_Id { get; set; }
        public double ReceiptItem { get; set; }
        public double CalcAmount { get; set; }
        public double ODAmount { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
    }
}
