
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Lien_Trn
    {
        [Key]
        public decimal LienTr_Id { get; set; }
        public decimal Lien_Id { get; set; }
        public decimal Loan_Id { get; set; }
        public decimal TD_Id { get; set; }
        public double TDTr_Amount { get; set; }
        public double DrawingPower { get; set; }
        public double LienTr_Amount { get; set; }
        public double LienTr_Recovered { get; set; }
        public bool LienTr_Closed { get; set; }
        public bool LienTr_OE { get; set; }
        public bool LienTr_Delete { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; } = "V";
    }
}
