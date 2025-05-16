
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class TermDeposit_Roi_Template
    {
        [Key]
        public decimal Roi_Id { get; set; }
        public int TDScheme_Id { get; set; }
        public string? TDHolderType { get; set; }
        public DateTime Wef { get; set; }
        public double Roi { get; set; }
        public string? PeriodType { get; set; }
        public int PeriodBegin { get; set; }
        public int PeriodEnd { get; set; }
        public double PenalRateForRD { get; set; }
        public bool TDRoi_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
