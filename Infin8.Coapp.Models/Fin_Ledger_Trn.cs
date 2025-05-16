
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Fin_Ledger_Trn
    {
        [Key]
        public decimal Trn_Id { get; set; }
        public decimal Led_Id { get; set; }
        public double OB_Amt { get; set; }
        public double Tot_Rpt_Amt { get; set; }
        public double Tot_Pmt_Amt { get; set; }
        public double CB_Amt { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public bool LedgerTrn_Delete { get; set; }
        public string? BrCode { get; set; }
    }
}
