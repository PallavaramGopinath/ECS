using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    using System;

    public partial class Mem_Demand_Stop
    {
        [Key]
        public decimal Id { get; set; }
        public decimal Mem_Id { get; set; }
        public DateOnly Demand_Date { get; set; }
        public bool Is_Active { get; set; } = true;
        public int Voc_Id { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
