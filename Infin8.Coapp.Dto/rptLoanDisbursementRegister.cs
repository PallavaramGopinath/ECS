namespace Infin8.Coapp.Dto
{
    public class rptLoanDisbursementRegister
    {
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public double LoanAmount { get; set; }
        public double TotalReceiptAmount { get; set; }
        public string? Fvb_Bank_Name { get; set; }
        public string? Fvb_Cheque_No { get; set; }
        public DateTime? Fvb_Cheque_Date { get; set; }
        public string? ReceiptDescription { get; set; }
        public double ReceiptItem { get; set; }
        public string? Led_Name { get; set; }
        public string? Loan_No { get; set; }
        public string? SuretymemberNo { get; set; }
        public string? SuretyPerNo { get; set; }
        public string? SuretymemberName { get; set; }
    }
}
