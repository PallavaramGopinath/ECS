
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Mem_Transfer_Account
    {
        [Key]
        public decimal MemTransfer_Id { get; set; }
        public DateOnly Transfer_Date { get; set; }
        public decimal FromMem_Id { get; set; }
        public decimal FromLed_Id { get; set; }
        public double FromPmt_Amt { get; set; }
        public decimal ToMem_Id { get; set; }
        public decimal ToLed_Id { get; set; }
        public double ToRpt_Amt { get; set; }
        public decimal Voc_Id { get; set; }
        public bool MemTransfer_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; } = "V";
    }
}
