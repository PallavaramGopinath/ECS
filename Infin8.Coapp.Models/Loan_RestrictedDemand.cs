using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    using System;

    public partial class Loan_RestrictedDemand
    {
        [Key]
        public int Restricted_Id { get; set; }
        public Nullable<System.DateTime> Restricted_wef { get; set; }
        public double RestrictedDemand_Amt { get; set; }
        public bool Restricted_Delete { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
