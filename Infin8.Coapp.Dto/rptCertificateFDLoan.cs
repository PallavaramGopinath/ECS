namespace Infin8.Coapp.Dto
{
    public class rptCertificateFDLoan
    {
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public string? PerNo { get; set; }
        public int Loan_Id { get; set; }
        public string? Loan_No  { get; set; }
        public double Disb_Amt { get; set; }
        public DateTime San_Date { get; set; }
        public double PrlColl_Amt { get; set; }
        public double  Loan_OS { get; set; }
        public double IntCalc_Amt { get; set; }
        public double IntColl_Amt { get; set; }

    }
}
