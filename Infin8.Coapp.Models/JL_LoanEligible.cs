
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class JL_LoanEligible
    {
        [Key]
        public decimal Id { get; set; }
        public System.DateTime Wef { get; set; }
        public double EligiblePercentage { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public bool Eligible_Delete { get; set; }
    }
}
