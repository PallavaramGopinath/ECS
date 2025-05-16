
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Loan_Elig_Outstanding
    {
        [Key]
        public int EligOS_Id { get; set; }
        public int LoanElig_Id { get; set; }
        public int Loan_Id { get; set; }
        public double Loan_OS { get; set; }
        public double LoanInt_Due { get; set; }
        public bool EligOS_Delete { get; set; }
        public int Voc_Id { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
