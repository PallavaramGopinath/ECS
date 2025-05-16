namespace Infin8.Coapp.Dto
{
    public class RecoverablesForLoanDisbursemetVM
    {
        public int LoanId { get; set; }
        public string? LoanNo { get; set; }
        public int DMId { get; set; }
        public string? Status { get; set; }
        public int MemId { get; set; }
        public int LedId { get; set; }
        public string? LedName { get; set; }
        public double Outstanding { get; set; }
        public double CalculatedAmount { get; set; }
        public double ODAmount { get; set; }
        public int SchemeId { get; set; }
        public int LoanSlNo { get; set; }
    }
}
