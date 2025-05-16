
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class TermDeposit_Schemes
    {
        [Key]
        public int TDScheme_Id { get; set; }
        public string? TDScheme_Name { get; set; }
        public int MinimumPeriod { get; set; }
        public int MaximumPeriod { get; set; }
        public string? PeriodType { get; set; }
        public string? TDSchemeType { get; set; }
        public decimal Led_Id { get; set; }
        public decimal IntLed_Id { get; set; }
        public decimal PiLed_Id { get; set; }
        public bool TDScheme_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
