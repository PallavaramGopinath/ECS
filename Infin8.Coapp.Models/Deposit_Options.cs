
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Deposit_Options
    {
        [Key]
        public decimal Option_Id { get; set; }
        public int Deposit_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public Nullable<System.DateTime> Wef { get; set; }
        public double Option_Amount { get; set; }
        public bool Is_Active { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public decimal Voc_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
