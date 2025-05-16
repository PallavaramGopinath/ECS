
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Mem_PassBookStatement_Tr
    {
        [Key]
        public int PBTrn_Id { get; set; }
        public int Mem_Id { get; set; }
        public int Mem_Status { get; set; }
        public string? LoanNo { get; set; }
        public string? SchemeName { get; set; }
        public Nullable<System.DateTime> LoanDate { get; set; }
        public int Period { get; set; }
        public int InstAmt { get; set; }
        public int LoanAmt { get; set; }
        public int LoanOS { get; set; }
        public int PrlOD { get; set; }
        public int IntBal { get; set; }
        public int PIBal { get; set; }
        public int InstNo { get; set; }
        public string? BrCode { get; set; }

    }
}
