namespace Infin8.Coapp.Dto
{
    /// <summary>
    /// Input for the shared loan-eligibility calculator (Spec 03 — section 10).
    /// Built on the client from the assembled sanction data + the user's loan-detail entries.
    /// </summary>
    public class LoanEligibilityInputVM
    {
        public decimal Mem_Id { get; set; }
        public int Scheme_Id { get; set; }
        public string? BrCode { get; set; }

        public double LoanAmount { get; set; }
        public double InstallmentAmount { get; set; }
        public double RateOfInterest { get; set; }

        // Salary (Sec 3)
        public double GrossPay { get; set; }
        public double NetPay { get; set; }
        public double TotalDeductions { get; set; }
        public double SocietyDeduction { get; set; }

        // Service / retirement (Sec 4 / 10.3)
        public int ServiceInMonths { get; set; }

        // Demand inputs (Sec 10.2) — supplied by the deductions step
        public double CurrentDepositDemand { get; set; }      // thrift + family-welfare pending demand
        public double NonDeductableLoanDemand { get; set; }   // Σ demand of non-deductable loans
        public double ExistingDemand { get; set; }            // existing demand for eligibility ceiling

        // Scheme flag
        public bool AdoptLoanEligibility { get; set; }
    }

    /// <summary>
    /// Result of the eligibility calculation (Spec 03 — section 10.2/10.3). Compute-only in Phase 2;
    /// persisted to <c>Loan_Eligibility</c> with the sanction in Phase 3.
    /// </summary>
    public class LoanEligibilityResultVM
    {
        // Demand breakdown (Sec 10.2)
        public double CurrentLoanDemand { get; set; }   // principal (installment) + interest on loan amount
        public double CurrentDepositDemand { get; set; }
        public double OtherDeductions { get; set; }      // Total Deductions − Society Deduction
        public double NonDeductableLoanDemand { get; set; }
        public double TotalDemand { get; set; }
        public double NewNetPay { get; set; }            // Gross − Total Demand

        // Ceilings (Sec 10.3 / 10.4)
        public double FiftyPercentOfGross { get; set; }
        public double TwentyFivePercentOfGross { get; set; }
        public double TwentyFiveTimesOfGross { get; set; }

        // Rule outcomes
        public bool FailsDemandCeiling { get; set; }     // (TotalDemand + ExistingDemand) > 50% gross
        public bool FailsNetPayFloor { get; set; }       // 25% gross > Net Pay
        public bool FailsServiceCheck { get; set; }      // service < retirement-age-in-months

        public bool IsEligible { get; set; }
        public List<string> Reasons { get; set; } = new();
    }
}
