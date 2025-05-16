
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Mem_Payable_NotNEFT
    {
        [Key]
        public decimal PbleCash_Id { get; set; }
        public Nullable<System.DateTime> PbleCash_Date { get; set; }
        public decimal Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? Status { get; set; }
        public string? BrCode { get; set; }
    }
}
