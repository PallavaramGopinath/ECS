
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Pay_VPF
    {
        [Key]
        public decimal Vpf_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public Nullable<System.DateTime> vpf_wef { get; set; }
        public double Vpf_Amount { get; set; }
        public bool Vpf_Delete { get; set; }
        public bool Vpf_AssCurr { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
