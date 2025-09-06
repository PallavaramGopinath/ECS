
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Pay_Init
    {
        [Key]
        public decimal Pay_Id { get; set; }
        public int Pay_Month { get; set; }
        public int Pay_Year { get; set; }
        public bool Pay_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? Pay_Des { get; set; }
        public Nullable<System.DateTime> From_Date { get; set; }
        public Nullable<System.DateTime> To_Date { get; set; }
        public decimal DA_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
