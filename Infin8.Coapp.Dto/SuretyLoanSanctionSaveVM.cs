namespace Infin8.Coapp.Dto
{
    /// <summary>
    /// POST body for saving an ECS Surety Loan sanction (Spec 03 — section 13).
    /// Audit fields (Usr_Id/Yr_Id/BrCode/SanctionDate) are NOT carried here — the server stamps
    /// them from the authenticated user's claims.
    /// </summary>
    public class SuretyLoanSanctionSaveVM
    {
        public decimal Mem_Id { get; set; }
        public decimal SuretyMem_Id { get; set; }
        public int Scheme_Id { get; set; }
        public int Inst_Type { get; set; }

        // Loan details (Sec 5)
        public double LoanAmount { get; set; }
        public int PeriodOfLoan { get; set; }
        public double RateOfInterest { get; set; }
        public double PenalRate { get; set; }
        public int InstalmentAmount { get; set; }
        public DateTime? FirstDueDate { get; set; }

        // Salary snapshot (Sec 3)
        public double BasicPay { get; set; }
        public double GrossPay { get; set; }
        public double SocietyDeduction { get; set; }
        public double TotalDeductions { get; set; }
        public double NetPay { get; set; }

        // Assets snapshot (Sec 6/7)
        public int MemberShareCapitalAmount { get; set; }
        public int SuretyShareCapitalAmount { get; set; }
        public int MemberTD { get; set; }

        // Limits / history (Sec 5)
        public int ThisLoanLimit { get; set; }
        public int MemberTotalLimit { get; set; }
        public int NoOfInstalmentsCompleted { get; set; }
        public int NoOfInstalmentsRecovered { get; set; }

        // Eligibility re-verification input (Sec 10) — recomputed server-side before saving.
        public LoanEligibilityInputVM? EligibilityInput { get; set; }

        // Deduction lines from the grid (deductable loans + SC-shortfall + sundry + manual rows).
        public List<DeductableLoanRowVM> DeductionRows { get; set; } = new();
    }

    /// <summary>Result of a sanction save — drives the page's success / rejection message.</summary>
    public class SanctionSaveResultVM
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public decimal LoanSanction_Id { get; set; }
        public int LoanElig_Id { get; set; }
    }
}
