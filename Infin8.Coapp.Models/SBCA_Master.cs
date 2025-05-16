
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class SBCA_Master
    {
        [Key]
        public decimal Acc_Id { get; set; }
        public int Scheme_Id { get; set; }
        public int Mem_Id { get; set; }
        public string? Acc_No { get; set; }
        public int Acc_Status { get; set; }
        public bool Acc_OE { get; set; }
        public bool Acc_Delete { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
