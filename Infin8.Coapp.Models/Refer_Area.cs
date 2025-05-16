
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Refer_Area
    {
        [Key]
        public int Area_Id { get; set; }
        public int Taluk_Id { get; set; }
        public string? Area_Name { get; set; }
        public string? Area_Notes { get; set; }
        public string? BrCode { get; set; }
    }
}
