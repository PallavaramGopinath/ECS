
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class TermDeposit_Schemes
    {
        [Key]
        public int TDScheme_Id { get; set; }

        [Required(ErrorMessage = "Scheme name is required")]
        [StringLength(100, ErrorMessage = "Scheme name cannot exceed 100 characters")]
        [Display(Name = "Scheme Name")]
        public string? TDScheme_Name { get; set; }

        [Required(ErrorMessage = "Minimum period is required")]
        [Range(0, 3650, ErrorMessage = "Minimum period must be between 0 and 3650")]
        [Display(Name = "Minimum Period")]
        public int MinimumPeriod { get; set; }

        [Required(ErrorMessage = "Maximum period is required")]
        [Range(1, 3650, ErrorMessage = "Maximum period must be between 1 and 3650")]
        [Display(Name = "Maximum Period")]
        public int MaximumPeriod { get; set; }

        [Required(ErrorMessage = "Period type is required")]
        [Display(Name = "Period Type")]
        public string? PeriodType { get; set; }

        [Required(ErrorMessage = "Scheme type is required")]
        [Display(Name = "Scheme Type")]
        public string? TDSchemeType { get; set; }

        [Required(ErrorMessage = "Principal GL is required")]
        //[Range(1, double.MaxValue, ErrorMessage = "Please select valid Principal GL")]
        [Display(Name = "Principal GL")]
        public decimal Led_Id { get; set; }

        [Required(ErrorMessage = "Interest GL is required")]
        //[Range(1, decimal.MaxValue, ErrorMessage = "Please select valid Interest GL")]
        [Display(Name = "Interest GL")]
        public decimal IntLed_Id { get; set; }

        [Required(ErrorMessage = "Penal Interest GL is required")]
        //[Range(1, double.MaxValue, ErrorMessage = "Please select valid Penal Interest GL")]
        [Display(Name = "Penal Interest GL")]
        public decimal PiLed_Id { get; set; }
        public bool TDScheme_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
