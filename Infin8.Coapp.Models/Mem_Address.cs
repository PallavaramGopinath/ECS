using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{

    public partial class Mem_Address
    {
        [Key]
        public decimal address_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public string? Mem_Add1 { get; set; }
        public string? Mem_Add2 { get; set; }
        public string? Mem_City { get; set; }
        public string? Mem_Pin { get; set; }
        public decimal Mem_Village_Id { get; set; }
        public string? Mem_Add_Stat { get; set; }
        public bool address_delete { get; set; }
        public string? BrCode { get; set; }
    }
}
