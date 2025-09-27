namespace Infin8.Coapp.Dto
{
    public class LoanHeaderDetailsVM
    {
        public decimal MemId { get; set; }
        public decimal SanctionId { get; set; }
        public int SchemeId { get; set; }
        public int PurposeId { get; set; }
        public string? LoanNo { get; set; }
        public double LoanAppliedAmount { get; set; }
        public double LoanAmount { get; set; }
        public DateTime DisbursementDate { get; set; }
        public int PrlPrd { get; set; }
        public int IntPrd { get; set; }
        public double RateOfInterest { get; set; }
        public double PenalRate { get; set; }
        public double InstalmentAmount { get; set; }
        public DateTime? FirstPrlDueDate { get; set; }
        public DateTime? FirstIntDueDate { get; set; }
        public string? SchemeName { get; set; }
    }
}
