using System.ComponentModel.DataAnnotations;
using System.Numerics;
namespace Infin8.Coapp.Models
{

    public partial class Loan_Scheme_Deduction_Rules
    {
        [Key]
        public BigInteger Rule_Id { get; set; }
        public int Scheme_Id { get; set; }
        public int Deductable_Scheme_Id { get; set; }
        public bool Is_Active { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
