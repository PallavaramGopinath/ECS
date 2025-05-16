
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class JL_Max_Limit
    {
        [Key]
        public decimal JL_Max_Id { get; set; }
        public Nullable<System.DateTime> WithEffectFrom { get; set; }
        public double Max_Loan_Limit { get; set; }
        public bool JL_Delete { get; set; }
        public string? BrCode  { get; set; }
    }
}
