
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class InterestApplication_OnMemExpiry
    {
        [Key]
        public decimal Expiry_Id { get; set; }
        public int IntOnLoanApplication_OnExpiry { get; set; }
        public int DividendApplication_OnExpiry { get; set; }
        public int IntOnTDFWDApplication_OnExpiry { get; set; }
        public int IntOnFWDApplication_OnExpiry { get; set; }
        public string? BrCode { get; set; }
    }
}
