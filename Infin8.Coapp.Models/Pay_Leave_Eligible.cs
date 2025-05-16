
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Pay_Leave_Eligible
    {
        [Key]
        public decimal Lv_elg_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public int Lv_Type { get; set; }
        public Nullable<System.DateTime> Lv_From { get; set; }
        public Nullable<System.DateTime> Lv_To { get; set; }
        public int Lv_elg_Days { get; set; }
        public string? Lv_Notes { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public bool Lv_Delete { get; set; }
    }
}
