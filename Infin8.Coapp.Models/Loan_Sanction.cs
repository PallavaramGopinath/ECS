using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    using System;

    public partial class Loan_Sanction
    {
        [Key]
        public decimal LoanSanction_Id { get; set; }
        public decimal LoanElig_Id { get; set; }
        public decimal Loan_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public decimal SuretyMem_Id { get; set; }
        public int Scheme_Id { get; set; }
        public int Inst_Type { get; set; }
        public int SuretyShareCapitalAmount { get; set; }
        public Nullable<System.DateTime> SanctionDate { get; set; }
        public int MemberTD { get; set; }
        public double BasicPay { get; set; }
        public double GrossPay { get; set; }
        public double SocietyDeduction { get; set; }
        public double TotalDeductions { get; set; }
        public double NetPay { get; set; }
        public double LoanAmount { get; set; }
        public int PeriodOfLoan { get; set; }
        public double RateOfInterest { get; set; }
        public double PenalRate { get; set; }
        public int InstalmentAmount { get; set; }
        public Nullable<System.DateTime> FirstDueDate { get; set; }
        public double TotalReceiptAmount { get; set; }
        public int ThisLoanLimit { get; set; }
        public int MemberTotalLimit { get; set; }
        public bool IsMemberBecomeDefaulter { get; set; }
        public bool IsSurityBecomeDefaulter { get; set; }
        public bool LoanSanction_Delete { get; set; }
        public string? LoanSanction_Status { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public int NoOfInstalmentsCompleted { get; set; }
        public int NoOfInstalmentsRecovered { get; set; }
        public int MemberShareCapitalAmount { get; set; }
        public double SL_PrlOS { get; set; }
        public double DA { get; set; }
        public double Lumpsum { get; set; }
        public double FWD { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
