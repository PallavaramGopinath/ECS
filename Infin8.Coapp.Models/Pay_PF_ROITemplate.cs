
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Pay_PF_ROITemplate
    {
        [Key]
        public decimal Roi_Id { get; set; }
        public DateTime Roi_Wef { get; set; }
        public double Roi { get; set; }
        public bool Roi_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
