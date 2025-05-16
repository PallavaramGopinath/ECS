namespace Infin8.Coapp.Dto
{
    public  class rptLoanDisbursementHSIS
    {
        public decimal Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? Loan_No { get; set; }
        public DateTime San_Date { get; set; }
        public double San_Amt { get; set; }
        public int Prl_Prd { get; set; }
        public double Roi { get; set; }
        public double Pi { get; set; }
        public DateTime? Disb_Date { get; set; }
        public double Disb_Amt { get; set; }
        public decimal Voc_Id { get; set; }
        public string? Voc_No { get; set; }
        public string? Fvb_Bank_Name { get; set; }
        public string? Fvb_Cheque_No { get; set; }
        public DateTime? Fvb_Cheque_Date { get; set; }
        public string? Scheme_Name { get; set; }

    }
}
