
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Fin_Voucher_Bank
    {
        [Key]
        public decimal Fvb_Id { get; set; }
        public Nullable<System.DateTime> Fvb_Date { get; set; }
        public decimal Mem_Id { get; set; }
        public string? Fvb_Bank_Mode { get; set; }
        public string? Fvb_Bank_Name { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Led_Id { get; set; }
        public double Fvb_Amount { get; set; }
        public string? Fvb_Cheque_No { get; set; }
        public Nullable<System.DateTime> Fvb_Cheque_Date { get; set; }
        public Nullable<System.DateTime> Fvb_Cheque_Real_Date { get; set; }
        public double Fvb_Real_Amount { get; set; }
        public bool Fvb_Cheque_Return { get; set; }
        public Nullable<System.DateTime> Fvb_Cheque_Return_Date { get; set; }
        public string? Fvb_Reason { get; set; }
        public int Fvb_Return_Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public bool Fvb_Delete { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
