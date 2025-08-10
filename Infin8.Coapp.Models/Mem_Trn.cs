
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Mem_Trn
    {
        [Key]
        public decimal Mem_Trn_Id { get; set; }
        public byte Trn_Type { get; set; }
        public decimal Mem_Id { get; set; }
        public decimal Led_Id { get; set; }
        public DateTime Trn_Date { get; set; }
        public double Rpt_Amt { get; set; }
        public double Pmt_Amt { get; set; }
        public bool MemTrn_OE { get; set; }
        public bool MemTrn_Delete { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public int Trn_SlNo { get; set; }
        public int IntCalc_Amt { get; set; }
        public DateTime? IntCalc_Date { get; set; }
        public int IntPaid_Amt { get; set; }
        public int Amt_CB { get; set; }
        public int Int_CB { get; set; }
        public int Amt_OB { get; set; }
        public string? Status { get; set; }
        public decimal PbleMaster_Id { get; set; }
        public decimal Acc_Id { get; set; }
        public int Demand_Amt { get; set; }
        public string?  BrCode { get; set; }
        public string? Voc_Status { get; set; } = "V";
    }
}
