
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class refer_caste
    {
        [Key]
        public int Caste_Id { get; set; }
        public string? Caste_Name { get; set; }
        public string? Notes { get; set; }
        public string? BrCode { get; set; }
    }
}
