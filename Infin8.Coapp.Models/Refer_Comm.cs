
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Refer_Comm
    {
        [Key]
        public int Comm_Id { get; set; }
        public string? Comm_Name { get; set; }
        public string? Notes { get; set; }
        public string? BrCode { get; set; }
    }
}
