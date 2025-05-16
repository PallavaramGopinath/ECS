using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{

    public partial class Loan_Sanction_Trn
    {
        [Key]
        public decimal LoanSanctionTr_Id { get; set; }
        public decimal LoanSanction_Id { get; set; }
        public decimal LoanElig_Id { get; set; }
        public int SlNo { get; set; }
        public decimal Loan_Id { get; set; }
        public int RecScheme_Id { get; set; }
        public int DM_Id { get; set; }
        public string? Status { get; set; }
        public decimal Mem_Id { get; set; }
        public decimal Led_Id { get; set; }
        public string? ReceiptDescription { get; set; }
        public double ReceiptItem { get; set; }
        public double CalcAmount { get; set; }
        public double ODAmount { get; set; }
        public decimal Payment_Id { get; set; }
        public string? PaymentDescription { get; set; }
        public double PaymentItem { get; set; }
        public bool LoanSanctionTr_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public int Loan_SlNo { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
