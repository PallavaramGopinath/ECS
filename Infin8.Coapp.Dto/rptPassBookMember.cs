namespace Infin8.Coapp.Dto
{
    public class rptPassBookMember
    {
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string?  OfficeName { get; set; }
        public string? SectionName { get; set; }
        public string? SuretyMemNo { get; set; }
        public string? SuretyPerNo { get; set; }
        public string? SuretyName { get; set; }
        public double MemShareCapital { get; set; }
        public double SuretyShareCaital { get; set; }
        public int Loan_Id { get; set; }
        public int DM_Id { get; set; }
        public int TD_Id { get; set; }
        public DateTime Trn_Date { get; set; }
        public string? Status { get; set; }
        public string? AccountNo { get; set; }
        public string? VocNo { get; set; }
        public string?  AccountName { get; set; }
        public double Prl_OB { get; set; }
        public double Prl_Rpt { get; set; }
        public double Prl_Pmt { get; set; }
        public double Prl_CB { get; set; }
        public double Prl_OD { get; set; }
        public double Int_OB { get; set; }
        public double Int_Calc { get; set; }
        public double Int_Rpt { get; set; }
        public double Int_Pmt { get; set; }
        public double Int_CB { get; set; }
        public double Int_OD { get; set; }
        public string? MonthYear { get; set; }
        public string? MonthYearString { get; set; }
        public List<rptPassBookLoans>? LoanList { get; set; }
    }
}
