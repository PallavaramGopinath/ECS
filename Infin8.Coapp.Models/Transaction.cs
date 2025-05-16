
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Transaction
    {
        [Key]
        public int Trn_Id { get; set; }
        public Nullable<System.DateTime> Trn_Date { get; set; }
        public int Mode { get; set; }
        public int Trn_Type { get; set; }
        public int Acc_Type { get; set; }
        public int Acc_Id { get; set; }
        public double Rpt_Amt { get; set; }
        public double Pmt_Amt { get; set; }
        public string? Narration { get; set; }
        public bool Is_saved { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
