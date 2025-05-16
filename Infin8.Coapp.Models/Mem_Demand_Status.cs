using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{

    public partial class Mem_Demand_Status
    {
        [Key]
        public int DemStatus_Id { get; set; }
        public int Demand_Id { get; set; }
        public int OfficeId { get; set; }
        public bool IsAppropriated { get; set; }
        public bool IsRecoverySaved { get; set; }
        public bool DemStatus_Delete { get; set; }
        public int Voc_Id { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }     
    }
}
