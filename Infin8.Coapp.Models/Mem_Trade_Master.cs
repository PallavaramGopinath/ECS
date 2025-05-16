
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Mem_Trade_Master
    {
        [Key]
        public int Trade_Id { get; set; }
        public string? TradeName { get; set; }
        public bool Trade_Delete { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
