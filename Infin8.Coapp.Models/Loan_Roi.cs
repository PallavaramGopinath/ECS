using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    using System;

    public partial class Loan_Roi
    {
        [Key]
        public decimal Roi_Id { get; set; }
        public decimal Loan_Id { get; set; }
        public string? Agency { get; set; }
        public DateTime Roi_Wef { get; set; }
        public double Roi { get; set; }
        public double Pi { get; set; }
        public int Prd { get; set; }
        public double InstallmentAmount { get; set; }
        public bool Loanroi_Delete { get; set; }
        public bool LoanRoi_Oe { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
