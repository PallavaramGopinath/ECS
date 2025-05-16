using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{

    public partial class Mem_Fees
    {
        [Key]
        public int Fee_Id { get; set; }
        public int Mem_Id { get; set; }
        public int Voc_Type { get; set; }
        public int Voc_Id { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public int Led_Type { get; set; }
        public string? BrCode { get; set; }
    }
}
