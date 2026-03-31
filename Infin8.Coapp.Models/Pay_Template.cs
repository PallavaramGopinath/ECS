
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

        [Required(ErrorMessage = "Salary ledger is required.")]
        [Range(1, 999999999999, ErrorMessage = "Please select selary ledger")]
        public decimal Salary_Led_Id { get; set; }

        [Required(ErrorMessage = "Employee provident fund ledger is required.")]
        [Range(1, 999999999999, ErrorMessage = "Please select employee provident fund ledger")]
        public decimal Emp_PF_Led_Id { get; set; }

        [Required(ErrorMessage = "Society contribution towards provident fund ledger is required.")]
        [Range(1, 999999999999, ErrorMessage = "Please society controcution towards provident fund ledger")]
        public decimal Banks_PF_Led_Id { get; set; }

        [Required(ErrorMessage = "Interest on provident fund ledger is required.")]
        [Range(1, 999999999999, ErrorMessage = "Please select interest on provident fund ledger")]
        public decimal Int_On_PF_Led_Id { get; set; }

        [Required(ErrorMessage = "Ex-gratia ledger is required.")]
        [Range(1, 999999999999, ErrorMessage = "Please select ex-gratia ledger")]
        public decimal ExGratia_Led_Id { get; set; }

        [Required(ErrorMessage = "Bonus ledger is required.")]
        [Range(1, 999999999999, ErrorMessage = "Please select bonux ledger")]
        public decimal Bonus_Led_Id { get; set; }
        public double PF_Roi { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
