
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Pay_Gen_Info
    {
        [Key]
        public decimal Pay_Info_Id { get; set; }
        public string? Pay_Info_Name { get; set; }
        public int Pay_Info_Type { get; set; }
        public int Pay_Info_Min_Exp { get; set; }
        public string? Pay_Info_Notes { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal yr_Id { get; set; }
        public string? BrCode { get; set; }
        public bool Pay_Info_Delete { get; set; }
    }
}
