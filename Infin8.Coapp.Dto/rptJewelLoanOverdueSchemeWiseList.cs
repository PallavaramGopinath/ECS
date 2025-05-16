namespace Infin8.Coapp.Dto
{
    public  class rptJewelLoanOverdueSchemeWiseList
    {
        public int Scheme_Id { get; set; }
        public string? Scheme_Name { get; set; }
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public DateTime San_Date { get; set; }
        public double San_Amt { get; set; }
        public double Roi { get; set; }
        public double Pi { get; set; }
        public DateTime JL_DueDate { get; set; }
        public double Prl_Bal { get; set; }
        public double Int_Bal { get; set; }
        public DateTime? IntCalc_Date { get; set; }
        public double PI_Bal { get; set; }
        public DateTime? PICalc_Date { get; set; }
        public double IntCalc_Amt { get; set; }
        public double PICalc_Amt { get; set; }
        public DateTime? IntCalc_DateNow { get; set; }
        public DateTime? PICalc_DateNow { get; set; }
        public int OD_3M { get; set; }
        public int OD_6M { get; set; }
        public int OD_9M { get; set; }
        public int OD_12MAbove { get; set; }
    }
}
