
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Loan_Elig_AD_Details
    {
        [Key]
        public int EligAD_Id { get; set; }
        public int EligADM_Id { get; set; }
        public int LoanElig_Id { get; set; }
        public double AD_Amt { get; set; }
        public bool EligAD_Delete { get; set; }
        public int Voc_Id { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
