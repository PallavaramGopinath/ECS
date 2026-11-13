namespace Infin8.Coapp.Dto
{
    /// <summary>Input for the loan-eligibility calculator (spec 06 §3.3).</summary>
    public class LoanEligibilityInputVM
    {
        public decimal Mem_Id { get; set; }
        public int Scheme_Id { get; set; }
        public string? BrCode { get; set; }

        public double LoanAmount { get; set; }
        public int PeriodOfLoan { get; set; }
        public double InstallmentAmount { get; set; }
        public double RateOfInterest { get; set; }
        public int InstType { get; set; }

        // Salary
        public double GrossPay { get; set; }
        public double TotalDeductions { get; set; }
        public double SocietyDeduction { get; set; }

        /// <summary>MonthsBetween(sanction date, DOR) − MinInstalmentsForLoan; null when DOR is unknown.</summary>
        public int? ServiceInMonths { get; set; }

        /// <summary>Σ current deposit demand from Deposit_Masters / Deposit_Options (§3.3).</summary>
        public double DepositDemand { get; set; }
        /// <summary>Σ demand of non-deductable loans not moved into recoverables.</summary>
        public double ExistingDemand { get; set; }

        public bool AdoptLoanEligibility { get; set; }
    }

    /// <summary>Result of the eligibility calculation (spec 06 §3.3).</summary>
    public class LoanEligibilityResultVM
    {
        public double InterestDemand { get; set; }       // first month's interest (0 for Inst_Type 2 EMI)
        public double NewLoanDemand { get; set; }        // instalment + InterestDemand
        public double DepositDemand { get; set; }
        public double Demand { get; set; }               // NewLoanDemand + DepositDemand
        public double ExistingDemand { get; set; }
        public double OtherDeductions { get; set; }      // Total − Society
        public double NetPay { get; set; }               // Gross − Demand − Existing − Other

        public double TwentyFiveTimesOfGross { get; set; }
        public double FiftyPercentOfGross { get; set; }
        public double TwentyFivePercentOfGross { get; set; }

        public bool FailsDemandCeiling { get; set; }     // Demand + Existing > 50 %
        public bool FailsNetPayFloor { get; set; }       // 25 % > NetPay
        public bool FailsServiceCheck { get; set; }      // ServiceInMonths < PeriodOfLoan

        public bool IsEligible { get; set; }
        public List<string> Reasons { get; set; } = new();
    }
}
