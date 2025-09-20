
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Menu_Forms
    {
        [Key]
        public int Form_Id { get; set; }
        public int SubMenu_Id { get; set; }
        public string? Form_Menu { get; set; }
        public string? Form_Name { get; set; }
        public bool Form_Delete { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
        public string? Form_Icon { get; set; }
    }
}
