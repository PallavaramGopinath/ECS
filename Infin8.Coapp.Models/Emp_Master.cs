
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Emp_Master
    {
        [Key]
        public decimal Mem_Id { get; set; }
        public Nullable<System.DateTime> Emp_Increment_Date { get; set; }
        public int Emp_Marital_Status { get; set; }
        public decimal Emp_Status_Id { get; set; }
        public int Emp_Height { get; set; }
        public int Emp_Weight { get; set; }
        public int Emp_BloodGrp { get; set; }
        public int Emp_Lenses { get; set; }
        public string? Emp_Desgn { get; set; }
        public string? Emp_Scale { get; set; }
        public decimal Emp_Desgn_Id { get; set; }
        public decimal Emp_Category_Id { get; set; }
        public decimal Emp_Grade_Id { get; set; }
        public double Emp_Basic { get; set; }
        public double Emp_PerPay { get; set; }
        public double Emp_GradePay { get; set; }
        public double Emp_VPF { get; set; }
        public bool Emp_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
