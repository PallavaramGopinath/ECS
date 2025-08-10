using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    using System;

    public partial class Mem_Demand_Master
    {
        [Key]
        public decimal Demand_Id { get; set; }
        public Nullable<System.DateTime> DemandDate { get; set; }
        public Nullable<System.DateTime> DemandCalculationDate { get; set; }
        public bool IsDemandUpdated { get; set; }
        public bool IsRecoveryAppropriated { get; set; }
        public bool IsRecoveryUpdated { get; set; }
        public Nullable<int> RecoveryUpdatedVoc_Id { get; set; }
        public bool DemandMaster_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public Nullable<System.DateTime> NextDemandDate { get; set; }
        public bool Demand_OE { get; set; }
        public bool Demand_Closed { get; set; }
        public int SocietyType { get; set; }
        public int Scheme_Id { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; } = "V";
    }
}
