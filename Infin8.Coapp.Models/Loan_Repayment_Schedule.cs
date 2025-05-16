using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    using System;

    public partial class Loan_Repayment_Schedule
    {
        [Key]
        public decimal Dem_Id { get; set; }
        public decimal Loan_Id { get; set; }
        public string? Dem_Status { get; set; }
        public Nullable<System.DateTime> Due_Date { get; set; }
        public double Principal_Demand { get; set; }
        public double Interest_Demand { get; set; }
        public double Non_OD_Amt { get; set; }
        public bool IsDemandRaised { get; set; }
        public bool Demand_Delete { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public string?  Voc_Status { get; set; }
    }
}
