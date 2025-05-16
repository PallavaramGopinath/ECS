
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Refer_Taluk
    {
        [Key]
        public int Taluk_Id { get; set; }
        public int District_Id { get; set; }
        public string? Taluk_Name { get; set; }
        public string? BrCode { get; set; }
    }
}
