using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    using System;

    public partial class Loan_Roi_Template
    {
        [Key]
        public decimal Roi_Id { get; set; }
        public int Scheme_Id { get; set; }
        public string? Agency { get; set; }
        public DateTime Wef { get; set; }
        public double Roi { get; set; }
        public double Pi { get; set; }
        public bool Updt { get; set; }
        public bool RoiTemplate_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; } = "V";
    }
}
