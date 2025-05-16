
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Refer_Religion
    {
        [Key]
        public int Religion_Id { get; set; }
        public string? Religion_Name { get; set; }
        public string? Notes { get; set; }
        public string? BrCode { get; set; }
    }
}
