
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Fin_Yr_Master
    {
        [Key]
        public decimal Yr_Id { get; set; }
        public Nullable<System.DateTime> From_Date { get; set; }
        public Nullable<System.DateTime> To_Date { get; set; }
        public decimal Usr_Id { get; set; }
        public bool Yr_closed { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
