
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Fin_Voucher_Trn
    {
        [Key]
        public decimal Voc_Trn_Id { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Led_Id { get; set; }
        public double Voc_Rpt { get; set; }
        public double Voc_Pmt { get; set; }
        public int Voc_Trn_Type { get; set; }
        public string? Voc_Narr { get; set; }
        public int Voc_Cash_Adj_Id { get; set; }
        public bool FinVocTr_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? Status { get; set; }
        public string? Description { get; set; }
        public decimal Mem_Id { get; set; }
        public decimal Loan_Id { get; set; }
        public double Outstanding { get; set; }
        public double Balance { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; } = "V";
    }
}
