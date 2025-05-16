

namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Pay_Retirement
    {
        [Key]
        public decimal Ret_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public int Ret_Type { get; set; }
        public Nullable<System.DateTime> Ret_Date { get; set; }
        public string? Ret_Notes { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public bool Ret_Delete { get; set; }
    }
}
