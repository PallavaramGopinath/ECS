using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{

    public partial class Mem_CollectionPriority
    {
        [Key]
        public int Priority_Id { get; set; }
        public int ReferId { get; set; }
        public int PriorityNo { get; set; }
        public string? PriorityType { get; set; }
        public int Scheme_Id { get; set; }
        public int DM_Id { get; set; }
        public string? PriorityName { get; set; }
        public int PriorityLed_Id { get; set; }
        public bool Priority_Delete { get; set; }
        public string? BrCode { get; set; }
    }
}
