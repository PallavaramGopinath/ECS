
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Loan_Eligibility_Template
    {
        [Key]
        public int Eligibility_Id { get; set; }
        public Nullable<System.DateTime> Eligibility_wef { get; set; }
        public double EligibilityForFreshLoan { get; set; }
        public double EligibilityForSubsequentLoan { get; set; }
        public string? BrCode { get; set; }
    }
}
