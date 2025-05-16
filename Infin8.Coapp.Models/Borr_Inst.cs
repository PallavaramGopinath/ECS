namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Borr_Inst
    {
        [Key]
        public int Inst_Id { get; set; }
        public int Borr_Id { get; set; }
        public Nullable<System.DateTime> Wef { get; set; }
        public double Inst_Amt { get; set; }
        public bool Oe { get; set; }
        public int SlNo { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
