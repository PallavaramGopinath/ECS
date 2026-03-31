
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Emp_Qualification
    {
        [Key]
        public decimal Qua_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public string? Qua_Emp_Degree { get; set; }
        public string? Qua_Institution { get; set; }
        public string? Qua_Medium { get; set; }
        public DateTime Qua_Start_Date { get; set; }
        public DateTime Qua_End_Date { get; set; }
        public string? Qua_Grade { get; set; }
        public double Qua_Percentage { get; set; }
        public bool Qua_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
