
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Refer_Rights
    {
        [Key]
        public int Rights_ID { get; set; }
        public int Sec_Level_ID { get; set; }
        public int Maj_Mod_ID { get; set; }
        public int Sub_Mod_ID { get; set; }
        public int Mod_ID { get; set; }
        public bool Rts_Add { get; set; }
        public bool Rts_Modify { get; set; }
        public bool Rts_Delete { get; set; }
        public bool Rts_Print { get; set; }
        public string? Mod_Name { get; set; }
        public string? BrCode { get; set; }
    }
}
