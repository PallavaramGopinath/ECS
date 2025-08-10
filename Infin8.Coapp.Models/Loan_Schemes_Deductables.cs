using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{

    public partial class Loan_Schemes_Deductables
    {
        [Key]
        public int Deductable_Id { get; set; }
        public int Scheme_Id { get; set; }
        public int DeductableScheme_Id { get; set; }
        public bool Deductable_Delete { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; } = "V";
    }
}
