
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Map_Banks
    {
        [Key]
        public decimal Led_Id { get; set; }
        public string? Led_Name { get; set; }
        public string? AccountNo { get; set; }
        public string? BrCode { get; set; }
    }
}
