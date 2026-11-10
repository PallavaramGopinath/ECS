namespace Infin8.Coapp.Dto
{
    /// <summary>
    /// Composite data assembled on member lookup for the ECS Surety Loan sanction screen
    /// (Spec 03 — sections 2 (member), 3 (salary), 4 (dates), 6/7 (assets), 8 (overdue)).
    /// </summary>
    public class SuretyLoanSanctionDataVM
    {
        public bool Found { get; set; }
        public string? Message { get; set; }

        // Member (Sec 2)
        public decimal Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }

        // Surety (Sec 2)
        public decimal SuretyMem_Id { get; set; }
        public string? SuretyMemberNo { get; set; }
        public string? SuretyMemberName { get; set; }

        // Salary (Sec 3) — Net Pay = Gross Pay − Total Deductions
        public double BasicPay { get; set; }
        public double GrossPay { get; set; }
        public double SocietyDeduction { get; set; }
        public double TotalDeductions { get; set; }
        public double NetPay { get; set; }

        // Dates (Sec 4)
        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfRetirement { get; set; }
        public int ServiceInMonths { get; set; }

        // Member assets (Sec 6) — display only
        public double MemberShareCapital { get; set; }
        public double MemberThriftDeposit { get; set; }
        public double MemberFamilyWelfareDeposit { get; set; }

        // Surety assets (Sec 7) — display only
        public double SuretyShareCapital { get; set; }

        // Surety loan overdue & history (Sec 8) — populated only when an overdue exists
        public List<SuretyLoanOverdueVM> SuretyLoanOverdues { get; set; } = new();
    }

    public class SuretyLoanOverdueVM
    {
        public decimal Loan_Id { get; set; }
        public string? LoanNo { get; set; }
        public string? Purpose { get; set; }
        public double OverdueAmount { get; set; }
    }

    /// <summary>
    /// Scheme-driven defaults loaded when a loan purpose (scheme) is selected (Spec 03 — section 5).
    /// The user may override Period / ROI / Penal / Installment on the screen.
    /// </summary>
    public class SuretyLoanSchemeDefaultsVM
    {
        public int Scheme_Id { get; set; }
        public string? Scheme_Name { get; set; }
        public int PeriodOfLoan { get; set; }        // default from Loan_Schemes.MaximumPrincipalPeriod
        public double RateOfInterest { get; set; }   // from ROI template (0 when not configured)
        public double PenalRate { get; set; }        // from ROI template (0 when not configured)
        public double ThisLoanLimit { get; set; }    // from Loan_Schemes.MaximumLoanAmount
        public double MaxLoanLimit { get; set; }     // from Map_General.MaxLoanLimit (scoped by brcode)
        public int InstType { get; set; }            // Loan_Schemes.Inst_Type (installment-calc method)
        public bool AdoptLoanEligibility { get; set; } // Loan_Schemes.AdoptLoanEligibility (gates the 50% rule)
        public int NoOfInstalmentsCompleted { get; set; }
        public int NoOfInstalmentsRecovered { get; set; }

        // Loan Scheme Details (Caption/Content grid) — scheme config codes rendered as readable text.
        public List<SchemeDetailRowVM> SchemeDetails { get; set; } = new();
    }

    /// <summary>One Caption/Content row of the Loan Scheme Details grid.</summary>
    public class SchemeDetailRowVM
    {
        public string? Caption { get; set; }
        public string? Content { get; set; }
    }

    /// <summary>
    /// Deductions + verification data assembled for a member + selected scheme (Spec 03 — sections 9, 10.1, 11).
    /// </summary>
    public class SuretyDeductionsDataVM
    {
        // Deductable loans grid (Sec 9.2) + share-capital-shortfall and sundry-debtor rows (Sec 10.1)
        public List<DeductableLoanRowVM> DeductableLoans { get; set; } = new();

        // Required share-capital check (Sec 10.1)
        public double ShareCapitalPercentageOnLoan { get; set; }
        public double RequiredShareCapital { get; set; }
        public double MemberShareCapital { get; set; }
        public double SuretyShareCapital { get; set; }
        public double MemberShareCapitalShortfall { get; set; }
        public double SuretyShareCapitalShortfall { get; set; }

        // Verification tabs (Sec 11)
        public List<NonDeductableLoanVM> NonDeductableLoans { get; set; } = new();
        public List<PendingDepositVM> PendingDeposits { get; set; } = new();

        // Eligibility demand inputs (Sec 10.2)
        public double CurrentDepositDemand { get; set; }       // Σ pending thrift + FWD demand
        public double NonDeductableLoanDemand { get; set; }    // Σ demand of non-deductable loans
    }

    /// <summary>One row of the Deductable Loans grid (Sec 9.2). Also used for SC-shortfall / sundry-debtor rows.</summary>
    public class DeductableLoanRowVM
    {
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public int DM_Id { get; set; }
        public string? Status { get; set; }
        public decimal Mem_Id { get; set; }
        public decimal Led_Id { get; set; }
        public string? Ledger_Name { get; set; }
        public double Outstanding { get; set; }
        public double ODAmount { get; set; }
        public double InterestBalance { get; set; } // interest outstanding — drives the "I" recovery line on save
        public int Scheme_Id { get; set; }
        public int Loan_SlNo { get; set; }
        public double LDeduct { get; set; }      // editable recoverable amount (semantics to be detailed by owner)
        public bool MatchSC { get; set; }        // true for share-capital-shortfall rows
        public string? RowType { get; set; }     // "Loan" | "ShareCapital" | "Sundry" | "Manual"
    }

    /// <summary>Non-deductable loan (Sec 11 verification tab).</summary>
    public class NonDeductableLoanVM
    {
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public string? Purpose { get; set; }
        public double LoanOutstanding { get; set; }
        public double InterestBalance { get; set; }
        public double PenalInterestBalance { get; set; }
    }

    /// <summary>Pending deposit (thrift / family-welfare) for the verification tab (Sec 11).</summary>
    public class PendingDepositVM
    {
        public string? DepositName { get; set; }
        public double Balance { get; set; }
        public double PendingDemand { get; set; }
    }
}
