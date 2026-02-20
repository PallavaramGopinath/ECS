
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Pay_PF_Template
    {
        [Key]
        public decimal Pf_Id { get; set; }
        public Nullable<System.DateTime> Wef { get; set; }
        public decimal ExGratia_Led_Id { get; set; }
        public decimal Bonus_Led_Id { get; set; }
        public double PF_Roi { get; set; }
        public bool Pf_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
