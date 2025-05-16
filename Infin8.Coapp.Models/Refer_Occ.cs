
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Refer_Occ
    {
        [Key]
        public int Occ_Id { get; set; }
        public string? Occ_Name { get; set; }
        public string? Notes { get; set; }
        public string? BrCode { get; set; }
    }
}
