
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class TermDeposit_Master
    {
        [Key]
        public decimal TD_Id { get; set; }
        public string? TD_No { get; set; }
        public int TDScheme_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public string? TDH_Name { get; set; }
        public int TDH_Age { get; set; }
        public int ModeOfOperation { get; set; }
        public DateTime AccountOpenDate { get; set; }
        public DateTime ValueDate { get; set; }
        public double DepositAmount { get; set; }
        public int PeriodInMonths { get; set; }
        public int PeriodInDays { get; set; }
        public double RateOfInterest { get; set; }
        public DateTime MaturityDate { get; set; }
        public double MaturityAmount { get; set; }
        public double LeinAmount { get; set; }
        public bool IsNomineeProvided { get; set; }
        public int InterestPayableFrequency { get; set; }
        public double PenalRate { get; set; }
        public bool IsDiscountRate { get; set; }
        public bool IsCompoundInterest { get; set; }
        public int CompoundFrequency { get; set; }
        public string? Nominee1Name { get; set; }
        public int Nominee1Age { get; set; }
        public string? Nominee1Relationship { get; set; }
        public string? Nominee2Name { get; set; }
        public int Nominee2Age { get; set; }
        public string? Nominee2Relationship { get; set; }
        public bool LienMarked { get; set; }
        public decimal Loan_Id { get; set; }
        public bool TD_OE { get; set; }
        public bool TD_Delete { get; set; }
        public bool AccountClosed { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? Status { get; set; }
        public decimal RenewalTD_Id { get; set; }
        public string? RenewalTD_No { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; } = "V";
    }
}
