namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class TermDeposit_IntCalc_Calendar
    {
        [Key]
        public decimal TDCalc_Id { get; set; }
        public Nullable<System.DateTime> TDCalc_Date { get; set; }
        public Nullable<System.DateTime> TDNextCalc_Date { get; set; }
        public bool TDCalc_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
