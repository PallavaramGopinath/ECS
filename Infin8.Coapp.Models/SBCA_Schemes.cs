
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class SBCA_Schemes
    {
        [Key]
        public int Scheme_Id { get; set; }
        public string? SBCA_Name { get; set; }
        public decimal SBCA_Led_Id { get; set; }
        public decimal SBCA_Int_Led_Id { get; set; }
        public bool SBCA_Delete { get; set; }
        public int InterestCalcPeriod { get; set; }
        public string? BrCode { get; set; }
    }
}
