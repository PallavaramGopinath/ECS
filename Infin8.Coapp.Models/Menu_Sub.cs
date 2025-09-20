
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Menu_Sub
    {
        [Key]
        public int SubMenu_Id { get; set; }
        public int Menu_Id { get; set; }
        public string? SubMenu_Name { get; set; }
        public string? Form_Name { get; set; }
        public bool SubMenu_Delete { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
        public string? SubMenu_Icon { get; set; }
    }
}
