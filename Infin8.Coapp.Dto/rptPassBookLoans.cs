namespace Infin8.Coapp.Dto
{
    public class rptPassBookLoans
    {
        public int Mem_Id { get; set; }
        public string? LoanNo { get; set; }
        public string? SchemeName { get; set; }
        public DateTime LoanDate { get; set; }
        public int Period { get; set; }
        public double InstAmt { get; set; }
        public double LoanAmt { get; set; }
        public double LoanOS { get; set; }
        public double PrlOD { get; set; }
        public double IntBal { get; set; }
        public double PIBal { get; set; }
        public int InstNo { get; set; }
    }
}
