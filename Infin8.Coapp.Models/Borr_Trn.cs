namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Borr_Trn
    {
        [Key]
        public int Trn_Id { get; set; }
        public int Borr_Id { get; set; }
        public Nullable<System.DateTime> Trn_Date { get; set; }
        public int Trn_Type { get; set; }
        public int Voc_Id { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public int Trn_SlNo { get; set; }
        public bool Oe { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
