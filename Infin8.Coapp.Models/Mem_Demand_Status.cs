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
        public int Voc_Id { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
