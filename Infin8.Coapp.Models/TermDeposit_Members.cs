
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class TermDeposit_Members
    {
        [Key]
        public decimal TDMem_Id { get; set; }
        public decimal TD_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public int Mem_SlNo { get; set; }
        public bool TDMem_Delete { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
