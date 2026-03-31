
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Emp_Master
    {
        [Key]
        public decimal Mem_Id { get; set; }
        public Nullable<System.DateTime> Emp_Increment_Date { get; set; }

        [Required(ErrorMessage = "Marital Status is required.")]
        [Range(1, 4, ErrorMessage = "Please select a valid Marital Status")]
        public int Emp_Marital_Status { get; set; }

        [Required(ErrorMessage = "Employee Status is required.")]
        [Range(1, 999999999999, ErrorMessage = "Please select Probation or Permanent")]
        public decimal Emp_Status_Id { get; set; }


        [Required(ErrorMessage = "Height is required.")]
        [Range(1, 1000, ErrorMessage = "Please select a valid Height")]
        public int Emp_Height { get; set; }

        [Required(ErrorMessage = "Weight is required.")]
        [Range(1, 3000, ErrorMessage = "Please select a valid Weight")]
        public int Emp_Weight { get; set; }

        [Required(ErrorMessage = "Blood Group is required.")]
        [Range(1, 999999999999, ErrorMessage = "Please select valid Blood Group")]
        public decimal Emp_BloodGrp { get; set; }

        [Required(ErrorMessage = "Wearing Lences is required.")]
        [Range(1, 2, ErrorMessage = "Please select a valid Wearing Lences")]
        public int Emp_Lenses { get; set; }

        [Required(ErrorMessage = "Designation is required.")]
        public string? Emp_Desgn { get; set; }

        [Required(ErrorMessage = "Scale of Pay is required.")]
        public string? Emp_Scale { get; set; }

        public decimal Emp_Desgn_Id { get; set; } = 0;


        public decimal Emp_Category_Id { get; set; } = 0;

        [Required(ErrorMessage = "Grade is required.")]
        [Range(1, 999999999999, ErrorMessage = "Please select valid Grade")]
        public decimal Emp_Grade_Id { get; set; }
        public double Emp_Basic { get; set; } = 0;
        public double Emp_PerPay { get; set; } = 0;
        public double Emp_GradePay { get; set; } = 0;
        public double Emp_VPF { get; set; } = 0;
        public bool Emp_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
