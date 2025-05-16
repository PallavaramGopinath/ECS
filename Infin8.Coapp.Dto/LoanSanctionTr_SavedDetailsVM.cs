namespace Infin8.Coapp.Dto
{
    public class LoanSanctionTr_SavedDetailsVM
    {
        public int LoanSanctionTr_Id { get; set; }
        public int LoanSanction_Id { get; set; }
        public int? LoanElig_Id { get; set; }
        public int SlNo { get; set; }
        public int Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public int? RecScheme_Id { get; set; }
        public int DM_Id { get; set; }
        public string? Status { get; set; }
        public int Mem_Id { get; set; }
        public int Led_Id { get; set; }
        public string? ReceiptDescription { get; set; }
        public double ReceiptItem { get; set; }
        public double CalcAmount { get; set; }
        public double ODAmount { get; set; }
        public int Payment_Id { get; set; }
        public string? PaymentDescription { get; set; }
        public double PaymentItem { get; set; }
        public bool LoanSanctionTr_Delete { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public int Loan_SlNo { get; set; }
    }
}
