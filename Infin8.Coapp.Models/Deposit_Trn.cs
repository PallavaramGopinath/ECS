namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Deposit_Trn
    {
        [Key]
        public decimal Trn_Id { get; set; }
        public int Deposit_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public decimal Demand_Id { get; set; }
        public string? Status { get; set; }
        public DateTime Trn_Date { get; set; }
        public Nullable<System.DateTime> Due_Date { get; set; }
        public double Demand_Amount { get; set; }
        public double Receipt_Amount { get; set; }
        public double Paid_Amount { get; set; }
        public double Interest_Calculated_Amount { get; set; }
        public Nullable<System.DateTime> Interest_Calculated_Date { get; set; }
        public double Interest_Paid_Amount { get; set; }
        public double Penal_Interest_Calculated_Amount { get; set; }
        public Nullable<System.DateTime> Penal_Interest_Calculated_Date { get; set; }
        public double Penal_Interest_Received_Amount { get; set; }
        public bool Is_Opening_Entry { get; set; }
        public bool Is_Active { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public int Trn_SlNo { get; set; }
        public int Pble_Master_Id { get; set; }
    }
}
