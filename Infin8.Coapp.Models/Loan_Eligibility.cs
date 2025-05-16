
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Loan_Eligibility
    {
        [Key]
        public int LoanElig_Id { get; set; }
        public Nullable<System.DateTime> LoanElig_Date { get; set; }
        public int Loan_Id { get; set; }
        public int Mem_Id { get; set; }
        public double GrossPay { get; set; }
        public double SocietyDeductions { get; set; }
        public double OtherThanSocietyDeductions { get; set; }
        public double TotalDeductions { get; set; }
        public double NetPay { get; set; }
        public double TotalPrlOS { get; set; }
        public double TotalIntOS { get; set; }
        public double ShareCapitalDue { get; set; }
        public double ShareCapitalSurityDue { get; set; }
        public double ThriftDepositDue { get; set; }
        public double EntranceFeeDue { get; set; }
        public double FeeDues { get; set; }
        public double OtherDues { get; set; }
        public double TwentyFiveTimesOfGP { get; set; }
        public double FiftyPercentOnGP { get; set; }
        public double TwentyFivePercentOnGP { get; set; }
        public double PercentageOfNetPay { get; set; }
        public double DemandAmt { get; set; }
        public double NetPayAfterDemand { get; set; }
        public double LoanAppliedAmt { get; set; }
        public double DisbursedAmt { get; set; }
        public int PeriodOfLoan { get; set; }
        public bool IsMemberBecomeDefaulter { get; set; }
        public bool IsSurityBecomeDefaulter { get; set; }
        public bool LoanElig_Delete { get; set; }
        public int Voc_Id { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
