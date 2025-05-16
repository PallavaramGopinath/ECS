namespace Infin8.Coapp.Dto
{
    public class rptPersonalRegisterTr
    {
        public int Mem_Id { get; set; }
        public int Mem_Status { get; set; } /// 1= mem, 2= surety mem
        public string? MemberNo { get; set; }
        public string? Per_No { get; set; }
        public string? MemberName { get; set; }
        public int Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public string? Scheme_Name { get; set; }
        public DateTime Disb_Date { get; set; }
        public double Disb_Amt { get; set; }
        public int Prl_Prd { get; set; }
        public double Inst_Amt { get; set; }
        public double Loan_OS { get; set; }
        public double PrlOD_Amt { get; set; }
        public double IntOD_Amt { get; set; }
        public double PIDue_Amt { get; set; }
        public int InstNo { get; set; }
        public DateTime? Due_Date { get; set; }
    }   
}
