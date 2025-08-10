
using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    using System;

    public partial class Loan_Inst
    {
        [Key]
        public decimal Inst_Id { get; set; }
        public decimal Loan_Id { get; set; }
        public DateTime Inst_Wef { get; set; }
        public double Inst_Amt { get; set; }
        public bool Inst_Oe { get; set; }
        public bool Inst_Delete { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; } = "V";
    }
}
