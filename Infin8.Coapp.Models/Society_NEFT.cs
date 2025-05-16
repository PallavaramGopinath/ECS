
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Society_NEFT
    {
        [Key]
        public decimal ID { get; set; }
        public string? SocietyName { get; set; }
        public string? SocietyEmailId { get; set; }
        public string? SocietyBankName { get; set; }
        public string? SocietyAccountNo { get; set; }
        public decimal Led_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
