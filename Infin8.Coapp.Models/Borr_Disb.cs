namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Borr_Disb
    {
        [Key]
        public int Disb_Id { get; set; }
        public int Borr_Id { get; set; }
        public Nullable<System.DateTime> Disb_Date { get; set; }
        public double Disb_Amt { get; set; }
        public bool Oe { get; set; }
        public int Voc_Id { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public int Disb_SlNo { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
