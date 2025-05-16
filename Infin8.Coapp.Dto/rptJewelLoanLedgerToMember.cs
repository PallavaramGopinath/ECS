namespace Infin8.Coapp.Dto
{
    public  class rptJewelLoanLedgerToMember
    {
        public decimal Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public DateTime  San_Date { get; set; }
        public double San_Amt { get; set; }
        public string? Scheme_Name { get; set; }
        public DateTime Trn_Date { get; set; }
        public double Disb_Amt { get; set; }
        public double PICalc_Amt { get; set; }
        public double IntCalc_Amt { get; set; }
        public double PIColl_Amt { get; set; }
        public double IntColl_Amt { get; set; }
        public double PrlColl_Amt { get; set; }
        public double Prl_OS { get; set; }
        public double Int_Bal { get; set; }
        public double PI_Bal { get; set; }

    }
}
