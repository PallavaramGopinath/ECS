
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class TermDeposit_LoanEligibleTemplate
    {
        [Key]
        public decimal TDLoanelig_Id { get; set; }
        public int TD_Type { get; set; }
        public Nullable<System.DateTime> Wef_Date { get; set; }
        public double Elig_Per { get; set; }
        public bool TD_Delete { get; set; }
        public string? BrCode { get; set; }
    }
}
