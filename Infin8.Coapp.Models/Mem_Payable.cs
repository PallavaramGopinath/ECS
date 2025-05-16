
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Mem_Payable
    {
        [Key]
        public decimal Pble_Id { get; set; }
        public decimal PbleMaster_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public Nullable<System.DateTime> Trn_Date { get; set; }
        public int Receipt_Amount { get; set; }
        public int Payment_Amount { get; set; }
        public int Closing_Balance { get; set; }
        public int NoOfDays { get; set; }
        public int Interest_Amount { get; set; }
        public int Pble_SlNo { get; set; }
        public string? Pble_Status { get; set; }
        public bool Pble_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public int St_SlNo { get; set; }
        public int Interest_Paid { get; set; }
        public decimal Acc_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
