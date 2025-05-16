namespace Infin8.Coapp.Dto
{
    public class FDLoanBalanceVM
    {
        public int LoanId { get; set; }
        public string? LoanNo { get; set; }
        public DateTime LoanDate { get; set; }
        public double LoanAmount { get; set; }
        public double Roi { get; set; }
        public double LoanAmountReceipt { get; set; }
        public double IntAlreadyCalculated { get; set; }
        public DateTime? IntAlreadyCalculatedDate { get; set; }
        public double IntAlreadyReceipt { get; set; }
        public double IntCalculatedNow { get; set; }
        public DateTime? IntCalculatedDateNow { get; set; }
    }
}
