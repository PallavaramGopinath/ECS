
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Refer_Modules
    {
        [Key]
        public int Mod_ID { get; set; }
        public int Mod_Maj_ID { get; set; }
        public int Mod_Sub_ID { get; set; }
        public string? Mod_Name { get; set; }
        public string? Mod_Maj_Name { get; set; }
        public string? Mod_Sub_Name { get; set; }
        public string? Mod_Menu_Name { get; set; }
        public string? BrCode { get; set; }
    }
}
