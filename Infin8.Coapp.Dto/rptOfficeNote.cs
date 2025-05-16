namespace Infin8.Coapp.Dto
{
    public class rptOfficeNote
    {
        public int LoanSanction_Id { get; set; }
        public DateTime SanctionDate { get; set; }
        public double LoanAmount { get; set; }
        public int PeriodOfLoan { get; set; }
        public int InstalmentAmount { get; set; }
        public double TotalReceiptAmount { get; set; }
        public bool IsMemberBecomeDefaulter { get; set; }
        public bool isSurityBecomeDefaulter { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public int SlNo { get; set; }
        public string? ReceiptDescription { get; set; }
        public double ReceiptItem { get; set; }
        public string? PaymentDescription { get; set; }
        public double PaymentItem { get; set; }
        public string? SuretymemberNo { get; set; }
        public string? SuretyPerNo { get; set; }
        public string? SuretymemberName { get; set; }
        public int MemberShareCapitalAmount { get; set; }
        public int NoOfInstalmentsCompleted { get; set; }
        public int NoOfInstalmentsRecovered { get; set; }
        public int SurityShareCapitalAmount { get; set; }
        public int GrossPay { get; set; }
    }
}
