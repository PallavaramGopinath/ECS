using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    using System;

    public partial class Mem_GI
    {
        [Key]
        public int Gi_Id { get; set; }
        public string? PolicyNo { get; set; }
        public Nullable<System.DateTime> Trn_Date { get; set; }
        public Nullable<System.DateTime> FromDate { get; set; }
        public Nullable<System.DateTime> ToDate { get; set; }
        public int PolicyAmt { get; set; }
        public double RatePerThousand { get; set; }
        public double SurchargeRate { get; set; }
        public double TotalPolicyAmt { get; set; }
        public double TotalPremiumAmt { get; set; }
        public double TotalSurchargeAmt { get; set; }
        public double TotalAmt { get; set; }
        public double TotalRoundedAmt { get; set; }
        public int Voc_Id { get; set; }
        public int Yr_Id { get; set; }
        public int Usr_Id { get; set; }
        public bool Gi_Delete { get; set; }
        public int GIMaster_Id { get; set; }
        public bool Is_DemandRaised { get; set; }
        public string? BrCode { get; set; }
    }
}
