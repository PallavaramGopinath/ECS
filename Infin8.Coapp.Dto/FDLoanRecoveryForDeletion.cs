namespace Infin8.Coapp.Dto
{
    public class FDLoanRecoveryForDeletion
    {
        public int Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public double San_Amt { get; set; }
        public DateTime San_Date { get; set; }
        public double Roi { get; set; }
        public double IntCalc_Amt { get; set; }
        public DateTime? IntCalc_Date { get; set; }
        public double IntColl_Amt { get; set; }
        public double PrlColl_Amt { get; set; }
    }
}
