
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Menu_Main
    {
        [Key]
        public int Menu_Id { get; set; }
        public string? Menu_Name { get; set; }
        public bool Menu_Delete { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
        public string? Menu_Icon { get; set; }
        public string Role { get; set; }
    }
}
