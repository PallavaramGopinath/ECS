using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    public partial class Map_Fees
    {
        [Key]
        public int Led_Id { get; set; }
        public string? Led_Name { get; set; }
        public int Led_Type { get; set; }
        public string? BrCode { get; set; }
    }
}
