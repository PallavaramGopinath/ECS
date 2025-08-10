
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Fin_Voucher
    {
        [Key]
        public decimal Voc_Id { get; set; }
        public int Voc_SlNo { get; set; }
        public string? Voc_No { get; set; }
        public int Voc_Rpt_SlNo { get; set; }
        public string? Voc_Rpt_No { get; set; }
        public string? Voc_Rpt_Mode { get; set; }
        public int Voc_Pmt_SlNo { get; set; }
        public string? Voc_Pmt_No { get; set; }
        public string? Voc_Pmt_Mode { get; set; }
        public DateTime Voc_Date { get; set; }
        public int Voc_Type { get; set; }
        public string? Voc_Mode { get; set; }
        public double Voc_Amt { get; set; }
        public string? Voc_Narration { get; set; }
        public bool Voc_Bus { get; set; }
        public bool Voc_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; } = "V"; // Fixed both CS0029 and CS1002  
    }
}
