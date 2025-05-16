
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Mem_Transfer_Account
    {
        [Key]
        public int MemTransfer_Id { get; set; }
        public Nullable<System.DateTime> Transfer_Date { get; set; }
        public int FromMem_Id { get; set; }
        public int FromLed_Id { get; set; }
        public double FromPmt_Amt { get; set; }
        public int ToMem_Id { get; set; }
        public int ToLed_Id { get; set; }
        public Nullable<double> ToRpt_Amt { get; set; }
        public int Voc_Id { get; set; }
        public bool MemTransfer_Delete { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
