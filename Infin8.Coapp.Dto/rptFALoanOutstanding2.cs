namespace Infin8.Coapp.Dto
{
    public  class rptFALoanOutstanding2
    {
        public decimal Loan_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? Token_PersonNo { get; set; }
        public string? Loan_No { get; set; }
        public double Roi { get; set; }
        public DateTime San_Date { get; set; }
        public int Scheme_Id { get; set; }
        public string? Scheme_Name { get; set; }
        public double Prl_OB { get; set; }
        public double Int_OB { get; set; }
        public double PI_OB { get; set; }
        public double IOD_OB { get; set; }
        public double Prl_Sched { get; set; }
        public double Disb_Amt { get; set; }
        public double PrlPayment { get; set; }
        public double PrlColl_Amt { get; set; }
        public double IntCalc_Amt { get; set; }
        public double IntColl_Amt { get; set; }
        public double PICalc_Amt { get; set; }
        public double PIColl_Amt { get; set; }
        public double IODCalc_Amt { get; set; }
        public double IODColl_Amt { get; set; }
        public double Prl_OS { get; set; }
        public double Prl_OD { get; set; }
        public double Int_Bal { get; set; }
        public double IOD_Bal { get; set; }
        public double PI_Bal { get; set; }
        public double OD3M { get; set; }
        public double OD3M_6M { get; set; }
        public double OD7M_12M { get; set; }
        public double OD13M_24M { get; set; }
        public double OD25M_36M { get; set; }
        public double OD37M_Above { get; set; }
        public double AccruedInt { get; set; }
        public int OfficeId { get; set; }
        public string? ReferName { get; set; }
        public DateTime? LastPrlCollDate { get; set; }
        public DateTime? LastIntCollDate { get; set; }
    }
}
