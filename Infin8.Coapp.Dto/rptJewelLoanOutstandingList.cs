namespace Infin8.Coapp.Dto
{
    public class rptJewelLoanOutstandingList
    {
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public DateTime San_Date { get; set; }
        public double Disb_Amt { get; set; }
        public double Loan_OS { get; set; }
        public double Int_Bal { get; set; }
        public double PI_Bal { get; set; }
        public DateTime? IntCalc_Date { get; set; }
        public DateTime? PICalc_Date { get; set; }
        public double Roi { get; set; }
        public double Pi { get; set; }
        public DateTime JL_DueDate { get; set; }
        public double GrossWeight { get; set; }
        public double NetWeight { get; set; }
    }
}
