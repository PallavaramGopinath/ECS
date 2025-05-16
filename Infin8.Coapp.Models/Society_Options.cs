
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Society_Options
    {
        [Key]
        public decimal Option_Id { get; set; }
        public bool AdoptDayBeginDayEnd { get; set; }
        public bool AdoptMakerChecker { get; set; }
        public bool HasBranches { get; set; }
        public int PaiseRoundToRupee { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
