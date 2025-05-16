
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Mem_Section_Master
    {
        [Key]
        public int Section_Id { get; set; }
        public string? SectionName { get; set; }
        public bool Section_Delete { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
