namespace Infin8.Coapp.Dto
{
    /// <summary>FD loan certificate (report 118): one row per FD loan (Loan_Type 3) repaid in the period.</summary>
    public class rptCertificateFDLoan
    {
        public decimal Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public string? PerNo { get; set; }
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public DateTime San_Date { get; set; }
        public double Disb_Amt { get; set; }
        /// <summary>Principal outstanding at the end of the period.</summary>
        public double Loan_OS { get; set; }
        public double PrlColl_Amt { get; set; }
        public double IntCalc_Amt { get; set; }
        public double IntColl_Amt { get; set; }
    }
}
