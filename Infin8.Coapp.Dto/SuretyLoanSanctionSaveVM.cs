namespace Infin8.Coapp.Dto
{
    /// <summary>
    /// POST body for saving (create or modify) an ECS loan sanction (spec 06 §3.12, §7.2).
    /// Audit fields (Usr_Id / Yr_Id / BrCode / SanctionDate) are NOT carried here — the server stamps
    /// them from the authenticated user's claims. Totals, eligibility and payment lines are recomputed
    /// on the server; only the user's entries and the chosen recoverable rows are taken from here.
    /// </summary>
    public class SuretyLoanSanctionSaveVM
    {
        /// <summary>0 = create; otherwise the sanction being modified (it is soft-deleted and re-issued).</summary>
        public decimal LoanSanction_Id { get; set; }
        public bool OverrideStipulatedPeriod { get; set; }

        public decimal Mem_Id { get; set; }
        public decimal SuretyMem_Id { get; set; }
        public int Scheme_Id { get; set; }

        // Loan details
        public double LoanAmount { get; set; }
        public int PeriodOfLoan { get; set; }
        public double RateOfInterest { get; set; }
        public double PenalRate { get; set; }
        public int InstalmentAmount { get; set; }
        public DateTime? FirstDueDate { get; set; }

        // Pay details (typed in from the salary certificate)
        public double BasicPay { get; set; }
        public double GrossPay { get; set; }
        public double SocietyDeduction { get; set; }
        public double TotalDeductions { get; set; }

        // Recoverables chosen on the page (deductable + DTo + MS/SS + moved non-deductable / pending + manual).
        public List<DeductableLoanRowVM> DeductionRows { get; set; } = new();
    }

    /// <summary>Result of a sanction save / delete — drives the page's success / rejection message.</summary>
    public class SanctionSaveResultVM
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public decimal LoanSanction_Id { get; set; }
        public int LoanElig_Id { get; set; }
        public List<string> Reasons { get; set; } = new();
    }
}
