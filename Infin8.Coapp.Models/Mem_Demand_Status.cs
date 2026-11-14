using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{

    public partial class Mem_Demand_Status
    {
        [Key]
        public decimal Id { get; set; }
        public decimal Demand_Id { get; set; }
        public decimal Office_Id { get; set; }
        public bool Is_Appropriated { get; set; } = false;
        public bool Is_Recovery_Saved { get; set; } = false;
        public bool is_Active { get; set; } = true;
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }

        // Spec 13a Maker / Checker (columns added by specs/ecs-extension/13a-ecs-recovery-appropriation.sql).
        /// <summary>User who appropriated the office's recovery (the Maker); null when not appropriated.</summary>
        public decimal? Appropriated_By { get; set; }
        public DateTime? Appropriated_On { get; set; }
        /// <summary>Checker who last rejected the appropriation, with the reason shown to the Maker.</summary>
        public decimal? Rejected_By { get; set; }
        public DateTime? Rejected_On { get; set; }
        public string? Reject_Reason { get; set; }
    }
}
