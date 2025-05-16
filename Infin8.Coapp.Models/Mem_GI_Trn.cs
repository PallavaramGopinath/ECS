using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{

    public partial class Mem_GI_Trn
    {
        [Key]
        public int GiTrn_Id { get; set; }
        public int Gi_Id { get; set; }
        public int Mem_Id { get; set; }
        public int LoanOS { get; set; }
        public int FWDBal { get; set; }
        public int PolicyAmt { get; set; }
        public double PremiumAmt { get; set; }
        public double SurchargeAmt { get; set; }
        public double TotalAmt { get; set; }
        public double TotalRoundedAmt { get; set; }
        public double Inst_Amt { get; set; }
        public double GrossPay { get; set; }
        public int Voc_Id { get; set; }
        public int Yr_Id { get; set; }
        public int Usr_Id { get; set; }
        public bool GiTrn_Delete { get; set; }
        public string? Status { get; set; }
        public string? BrCode { get; set; }
    }
}
