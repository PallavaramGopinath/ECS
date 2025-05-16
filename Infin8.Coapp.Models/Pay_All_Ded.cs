
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Pay_All_Ded
    {
        [Key]
        public decimal Pay_Ad_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public Nullable<System.DateTime> All_wef { get; set; }
        public decimal All_Id { get; set; }
        public Nullable<double> All_Amount { get; set; }
        public int All_Status { get; set; }
        public double All_Percentage { get; set; }
        public double All_MaxAmount { get; set; }
        public bool All_Delete { get; set; }
        public bool All_AssCurr { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
