namespace Infin8.Coapp.Dto
{
    public class rptPassBookOtherLoans
    {
        public DateTime TrnDate { get; set; }
        public string? Loan_No { get; set; }
        public double DisbAmt { get; set; }
        public double PrlColl { get; set; }
        public double IntCalc { get; set; }
        public double IntColl { get; set; }
        public double PICalc { get; set; }
        public double PIColl { get; set; }
        public double TotalColl { get; set; }
        public double PrlSched { get; set; }
        public double LoanOS { get; set; }
        public double IntBal { get; set; }
        public double PrlOD { get; set; }

    }
}
