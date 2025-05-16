
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Pay_DA_Template
    {
        [Key]
        public decimal DA_Id { get; set; }
        public Nullable<System.DateTime> Wef { get; set; }
        public double DA_Percent { get; set; }
        public bool DA_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public Nullable<System.DateTime> Arrears_From { get; set; }
        public Nullable<System.DateTime> Arrears_To { get; set; }
        public string? Status { get; set; }
        public string? BrCode { get; set; }
    }
}
