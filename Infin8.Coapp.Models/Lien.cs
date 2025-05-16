
using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{

    public partial class Lien
    {
        [Key]
        public decimal Lien_Id { get; set; }
        public decimal Loan_Id { get; set; }
        public double TD_Amount { get; set; }
        public double Lien_Amount { get; set; }
        public double Lien_Recovered { get; set; }
        public string? SecurityDescription { get; set; }
        public decimal TD_Id { get; set; }
        public bool Lien_Closed { get; set; }
        public bool Lien_OE { get; set; }
        public bool Lien_Delete { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
