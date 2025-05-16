
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class User_SecurityDetails
    {
        [Key]
        public int Securiy_Id { get; set; }
        public byte Usr_Id { get; set; }
        public short Activity_No { get; set; }
        public string? BrCode { get; set; }
    }
}
