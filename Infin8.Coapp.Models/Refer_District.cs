
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Refer_District
    {
        [Key]
        public int District_Id { get; set; }
        public string? District_Name { get; set; }
        public string? BrCode { get; set; }
    }
}
