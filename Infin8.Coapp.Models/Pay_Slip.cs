
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Pay_Slip
    {
        [Key]
        public decimal PaySlip_Id { get; set; }
        public decimal Pay_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public double Pay_Basic { get; set; }
        public double Pay_Basic_Earned { get; set; }
        public double Pay_PP { get; set; }
        public double Pay_PP_Earned { get; set; }
        public double Pay_GradePay { get; set; }
        public double Pay_GradePay_Earned { get; set; }
        public double Pay_DA_Percent { get; set; }
        public double Pay_DA_Earned { get; set; }
        public int Pay_SLS_Days { get; set; }
        public double Pay_SLS { get; set; }
        public double Pay_ExGratia { get; set; }
        public double Pay_Bonus { get; set; }
        public double Pay_PF { get; set; }
        public double Pay_VPF { get; set; }
        public double Pay_Tot_Allowance { get; set; }
        public double Pay_Tot_Deductions { get; set; }
        public double Pay_Net { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public bool Pmt { get; set; }
        public Nullable<System.DateTime> Pmt_Date { get; set; }
        public decimal Voc_Id { get; set; }
        public bool Pay_Delete { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
