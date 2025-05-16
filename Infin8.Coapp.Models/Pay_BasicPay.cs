
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Pay_BasicPay
    {
        [Key]
        public decimal BP_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public Nullable<System.DateTime> BP_wef { get; set; }
        public double BP_Amount { get; set; }
        public double BP_PerPay { get; set; }
        public double BP_GradePay { get; set; }
        public bool BP_Delete { get; set; }
        public bool BP_AssCurr { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
