
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Pay_Template
    {
        [Key]
        public decimal PayTemplate_Id { get; set; }
        public Nullable<System.DateTime> Wef { get; set; }
        public double DA_Percent { get; set; }
        public double PF_Percent { get; set; }
        public decimal Salary_Led_Id { get; set; }
        public decimal Emp_PF_Led_Id { get; set; }
        public decimal Banks_PF_Led_Id { get; set; }
        public decimal Int_On_PF_Led_Id { get; set; }
        public decimal ExGratia_Led_Id { get; set; }
        public decimal Bonus_Led_Id { get; set; }
        public double PF_Roi { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
