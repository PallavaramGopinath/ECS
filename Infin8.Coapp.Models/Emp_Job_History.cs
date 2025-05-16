
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Emp_Job_History
    {
        [Key]
        public decimal Job_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public Nullable<System.DateTime> Job_Start_Date { get; set; }
        public Nullable<System.DateTime> Job_End_Date { get; set; }
        public string? Job_Organisation { get; set; }
        public string? Job_Department { get; set; }
        public string? Job_Desgn { get; set; }
        public int Job_Desgn_Id { get; set; }
        public double Job_Basic { get; set; }
        public bool Job_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string?  BrCode { get; set; }
    }
}
