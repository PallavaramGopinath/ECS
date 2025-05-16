
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Mem_RetirementAge
    {
        [Key]
        public int Mem_Id { get; set; }
        public int RetirementAge { get; set; }
        public bool RetirementAge_Delete { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
