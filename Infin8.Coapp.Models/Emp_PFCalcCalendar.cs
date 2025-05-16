
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Emp_PFCalcCalendar
    {
        [Key]
        public decimal PFCalc_Id { get; set; }
        public Nullable<System.DateTime> PFCalc_Date { get; set; }
        public Nullable<System.DateTime> PFCalc_NextDate { get; set; }
        public bool PFCalc_Delete { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
