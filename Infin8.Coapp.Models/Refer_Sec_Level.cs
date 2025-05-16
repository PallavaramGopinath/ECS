
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Refer_Sec_Level
    {
        [Key]
        public int Sec_Level_ID { get; set; }
        public string? Sec_Level_Name { get; set; }
        public string? BrCode { get; set; }
    }
}
