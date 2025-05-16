using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{

    public partial class Mem_NoPrefix
    {
        [Key]
        public int Mem_NoPrefixId { get; set; }
        public int Mem_Type { get; set; }
        public string? Mem_NoPrefix1 { get; set; }
        public string? BrCode { get; set; }
    }
}
