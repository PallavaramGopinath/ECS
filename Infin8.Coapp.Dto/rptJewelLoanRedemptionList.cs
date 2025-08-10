namespace Infin8.Coapp.Dto
{
    public class rptJewelLoanRedemptionList
    {
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public DateTime Trn_Date { get; set; }
        public DateTime San_Date { get; set; }
        public double San_Amt { get; set; }
        public double PIColl_Amt { get; set; }
        public double IntColl_Amt { get; set; }
        public double PrlColl_Amt { get; set; }
        public double PIBal { get; set; }
        public double IntBal { get; set; }
        public double PrlOS { get; set; }
    }
}
