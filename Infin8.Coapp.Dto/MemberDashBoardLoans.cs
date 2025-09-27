namespace Infin8.Coapp.Dto
{
    public  class MemberDashBoardLoans
    {
        public decimal Mem_Id { get; set; }
        public decimal Loan_Id { get; set; }
        public int Scheme_Id { get; set; }
        public string? Loan_No { get; set; }
        public string? Scheme_Name { get; set; }
        public double San_Amt { get; set; }
        public DateTime San_Date { get; set; }
        public int Loan_Type { get; set; }
        public double Disb_Amt { get; set; }
        public double PrlOS { get; set; }
        public double PrlOD { get; set; }
        public double IntOD { get; set; }
        public double IODBal { get; set; }
        public double PIBal { get; set; }
        public string?  Due_Date { get; set; }
        public bool IsLoanClosed { get; set; }

    }
}
