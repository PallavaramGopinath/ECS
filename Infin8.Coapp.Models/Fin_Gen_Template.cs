
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Fin_Gen_Template
    {
        [Key]
        public int Voc_Id { get; set; }
        public string? Voc_Module { get; set; }
        public string? Voc_Prefix { get; set; }
        public string? BrCode { get; set; }
    }
}
