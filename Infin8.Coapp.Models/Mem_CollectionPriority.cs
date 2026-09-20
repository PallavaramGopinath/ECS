using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{

    public partial class Mem_CollectionPriority
    {
        [Key]
        public decimal Priority_Id { get; set; }
        public int PriorityNo { get; set; }
        public string? PriorityType { get; set; }
        public int Scheme_Id { get; set; }
        public int Deposit_Id { get; set; }
        public string? PriorityName { get; set; }
        public int PriorityLed_Id { get; set; }
        public bool Is_Active { get; set; }
        public string? BrCode { get; set; }
        public decimal Usr_Id { get; set; }
    }
}
