using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    using System;

    public partial class Mem_Demand_Stop
    {
        [Key]
        public int StopDemand_Id { get; set; }
        public int Mem_Id { get; set; }
        public Nullable<System.DateTime> DemandDate { get; set; }
        public bool StopDemand_Delete { get; set; }
        public int Voc_Id { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
