
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Pay_Slip_Trn
    {
        [Key]
        public decimal Pay_tr_Id { get; set; }
        public decimal Pay_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public decimal All_Id { get; set; }
        public decimal Ded_Id { get; set; }
        public decimal Loan_Id { get; set; }
        public decimal Led_Id { get; set; }
        public int All_Type { get; set; }
        public int Ded_Type { get; set; }
        public int All_Ded_Type { get; set; }
        public double All_Ded_Amt { get; set; }
        public double Allowance_Amt { get; set; }
        public double Deduction_Amt { get; set; }
        public bool PayTr_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
