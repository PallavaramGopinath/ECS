using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    using System;

    public partial class Mem_GI_Master
    {
        [Key] 
        public int GIMaster_Id { get; set; }
        public Nullable<System.DateTime> FromDate { get; set; }
        public Nullable<System.DateTime> ToDate { get; set; }
        public double RatePerThousand { get; set; }
        public double SurchargeRate { get; set; }
        public bool GIMaster_Delete { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
