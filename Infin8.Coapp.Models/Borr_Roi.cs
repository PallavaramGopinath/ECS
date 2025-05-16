namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Borr_Roi
    {
        [Key]
        public int Roi_Id { get; set; }
        public int Borr_Id { get; set; }
        public Nullable<System.DateTime> Wef { get; set; }
        public double Roi { get; set; }
        public double Pi { get; set; }
        public bool Oe { get; set; }
        public int SlNo { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
