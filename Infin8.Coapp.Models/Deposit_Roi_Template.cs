namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Deposit_Roi_Template
    {
        [Key]
        public int DMRoi_Id { get; set; }
        public int DM_Id { get; set; }
        public Nullable<System.DateTime> DMRoi_Wef { get; set; }
        public double DM_Roi { get; set; }
        public double DM_Pi { get; set; }
        public bool DMRoi_Delete { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
