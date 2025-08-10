
using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{

    public partial class Loan_Members
    {
        [Key]
        public decimal LoanMem_Id { get; set; }
        public decimal Loan_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public int Mem_SlNo { get; set; }
        public int Mem_Status { get; set; }
        public int Mem_Guarantor { get; set; }
        public bool Mem_Delete { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; } = "V";
    }
}
