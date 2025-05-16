
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Refer_Supervisor
    {
        [Key]
        public int Supervisor_Id { get; set; }
        public string? Supervisor_Name { get; set; }
        public string? BrCode { get; set; }
    }
}
