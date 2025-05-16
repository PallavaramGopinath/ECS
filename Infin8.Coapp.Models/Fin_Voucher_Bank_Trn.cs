
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Fin_Voucher_Bank_Trn
    {
        [Key]
        public decimal Fvb_Tr_Id { get; set; }
        public decimal fvb_Id { get; set; }
        public Nullable<System.DateTime> Gvb_Date { get; set; }
        public double Fvb_Amt_Adjusted { get; set; }
        public bool FvbTr_Delete { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public int SlNo { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
