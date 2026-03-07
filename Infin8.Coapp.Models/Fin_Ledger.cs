
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Fin_Ledger
    {
        [Key]
        public decimal Led_Id { get; set; }
        public string? Led_No { get; set; }
        [Required(ErrorMessage = "Ledger name is required")]
        public string? Led_Name { get; set; }
        [Required(ErrorMessage = "Description is required")]
        public string? Led_Desc { get; set; }
        [Required(ErrorMessage = "Group name is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Group must be greater than 0 (not selected)")]
        public int Grp_Id { get; set; }
        public int SubGrp_Id { get; set; }
        public double Led_OB { get; set; }
        public Nullable<System.DateTime> Led_OB_Date { get; set; }
        public double Led_Tot_Rpt { get; set; }
        public double Led_Tot_Pmt { get; set; }
        public bool Led_Mapped { get; set; }
        public int Led_RealNominal { get; set; }
        public int Led_Type { get; set; }
        [Required(ErrorMessage = "Sundry type is required")]
        public int Led_Sundry_Type { get; set; }
        public bool Led_GLAvailable { get; set; }
        public double Led_CB { get; set; }
        public int Led_SlNo { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public bool Led_Delete { get; set; }
        public string? BrCode { get; set; }
    }
}
