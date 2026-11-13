namespace Infin8.Coapp.Dto
{
    /// <summary>
    /// Member + surety + dates + assets assembled on member lookup for the ECS Loan Sanction screen
    /// (spec 06 §2.2). Pay details are typed in by hand; see <see cref="EcsPayDetailsVM"/> for the
    /// optional payroll prefill.
    /// </summary>
    public class SuretyLoanSanctionDataVM
    {
        public bool Found { get; set; }
        public string? Message { get; set; }

        // Member
        public decimal Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }

        // Surety (mem_master.suretymem_id)
        public decimal SuretyMem_Id { get; set; }
        public string? SuretyMemberNo { get; set; }
        public string? SuretyMemberName { get; set; }

        // Salary — kept for payload compatibility; not prefilled on lookup.
        public double BasicPay { get; set; }
        public double GrossPay { get; set; }
        public double SocietyDeduction { get; set; }
        public double TotalDeductions { get; set; }
        public double NetPay { get; set; }

        // Dates (spec §3.6)
        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfRetirement { get; set; }
        /// <summary>MonthsBetween(sanction date, DOR) − MinInstalmentsForLoan. Null when DOR is unknown.</summary>
        public int? ServiceInMonths { get; set; }

        // Member assets — display only
        public double MemberShareCapital { get; set; }
        public double MemberThriftDeposit { get; set; }
        public double MemberFamilyWelfareDeposit { get; set; }

        // Surety assets — display only
        public double SuretyShareCapital { get; set; }
    }

    /// <summary>A surety's (or member's) loan that is in overdue.</summary>
    public class SuretyLoanOverdueVM
    {
        public decimal Loan_Id { get; set; }
        public string? LoanNo { get; set; }
        public string? Purpose { get; set; }
        public double OverdueAmount { get; set; }
        public int Loan_Type { get; set; }
        /// <summary>True when the overdue is in a Loan_Type 1 (ECS) loan — the sanction cannot be saved.</summary>
        public bool BlocksSanction { get; set; }
    }

    /// <summary>
    /// Scheme-driven defaults loaded when a scheme is selected (spec 06 §2.3 step 3), plus the
    /// duplicate / already-saved / stipulated-period warnings (§3.4, §3.5).
    /// </summary>
    public class SuretyLoanSchemeDefaultsVM
    {
        public bool Found { get; set; }
        public string? Message { get; set; }

        public int Scheme_Id { get; set; }
        public string? Scheme_Name { get; set; }
        public int Loan_Type { get; set; }
        public double LoanAmount { get; set; }       // default = Loan_Schemes.MaximumLoanAmount
        public int PeriodOfLoan { get; set; }        // default = Loan_Schemes.MaximumPrincipalPeriod
        public double RateOfInterest { get; set; }   // Loan_Roi_Template as on sanction date
        public double PenalRate { get; set; }        // Loan_Roi_Template as on sanction date
        public bool RoiTemplateFound { get; set; }
        public double ThisLoanLimit { get; set; }    // Loan_Schemes.MaximumLoanAmount
        public double MaxLoanLimit { get; set; }     // Map_General.MaxLoanLimit
        public int InstType { get; set; }
        public bool AdoptLoanEligibility { get; set; }
        public int MatchShareCapital { get; set; }   // 1 member+surety, 2 member only, 3 none
        public bool AdoptLoanLimit { get; set; }
        public bool IsSuretyRequired { get; set; }
        public DateTime FirstDueDate { get; set; }

        public List<SchemeDetailRowVM> SchemeDetails { get; set; } = new();
        public List<SanctionWarningVM> Warnings { get; set; } = new();
    }

    /// <summary>One Caption/Content row of the Loan Scheme Details grid.</summary>
    public class SchemeDetailRowVM
    {
        public string? Caption { get; set; }
        public string? Content { get; set; }
    }

    /// <summary>
    /// One recoverable row (WinForms dgvOS). Status codes per spec §3.8:
    /// P / I / PI (deductable loan), DTo, DBy, MS / SS (share-capital shortfall), D (deposit), G (general).
    /// </summary>
    public class DeductableLoanRowVM
    {
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public int DM_Id { get; set; }
        public string? Status { get; set; }
        public decimal Mem_Id { get; set; }
        public decimal Led_Id { get; set; }
        public string? Ledger_Name { get; set; }
        public double Outstanding { get; set; }       // the amount recovered (loan_sanction_trn.receiptitem)
        public double CalcAmount { get; set; }
        public double ODAmount { get; set; }
        public int Scheme_Id { get; set; }
        public int Loan_SlNo { get; set; }            // MAX(loan_trn.trn_slno) snapshot for §3.13
        public bool MatchSC { get; set; }
        /// <summary>True for rows added by hand (ledger + amount). Only these, and zero rows, can be removed.</summary>
        public bool IsManual { get; set; }
        /// <summary>"Deductable" | "NonDeductable" | "Pending" | "ShareCapital" | "DueTo" | "Manual".</summary>
        public string? RowType { get; set; }
    }
}
