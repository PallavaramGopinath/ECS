
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Refer_Data
    {
        [Key]
        public decimal ReferId { get; set; }
        public int ReferType { get; set; }
        public string? ReferName { get; set; }
        public string? ReferNotes { get; set; }
        public bool ReferFactoryRecovery { get; set; }
        public bool ReferDelete { get; set; }
        public string? BrCode { get; set; }
    }
}
