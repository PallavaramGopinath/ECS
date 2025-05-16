
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class TermDeposit_FcTemplate
    {
        [Key]
        public decimal TDfc_Id { get; set; }
        public int TD_Type { get; set; }
        public Nullable<System.DateTime> TDfc_Wef { get; set; }
        public double TDfc_Roi { get; set; }
        public bool TDfc_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
