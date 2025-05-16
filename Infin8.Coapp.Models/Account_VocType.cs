using System.ComponentModel.DataAnnotations;

namespace Infin8.Coapp.Models
{
    public partial class Account_VocType
    {
        [Key]
        public int Voc_Type { get; set; }
        public string? Voc_Name { get; set; }
    }
}
