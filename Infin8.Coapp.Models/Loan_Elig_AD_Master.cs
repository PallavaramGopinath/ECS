
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Loan_Elig_AD_Master
    {
        [Key]
        public int EligADM_Id { get; set; }
        public int EligADM_Type { get; set; }
        public string? EligADM_Name { get; set; }
        public int EligADMLed_Id { get; set; }
        public string? EligADM_Notes { get; set; }
        public bool EligADM_Delete { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
