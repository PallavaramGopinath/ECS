
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Gen_Template
    {
        [Key]
        public int Gen_Id { get; set; }
        public int No_of_Lines { get; set; }
        public string? First_Sig { get; set; }
        public string? Second_Sig { get; set; }
        public string? Third_Sig { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
