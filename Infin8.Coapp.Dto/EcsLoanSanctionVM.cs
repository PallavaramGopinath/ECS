namespace Infin8.Coapp.Dto
{
    /// <summary>A scheme-selection warning (spec 06 §3.4 / §3.5).</summary>
    public class SanctionWarningVM
    {
        /// <summary>DUPLICATE | ALREADY_SAVED | STIPULATED_PERIOD | ROI_NOT_FOUND | NOT_ECS | NO_SERVICE | PERIOD_REDUCED</summary>
        public string Code { get; set; } = "";
        public bool Blocking { get; set; }
        public string Message { get; set; } = "";
    }

    /// <summary>Date of birth entered on the sanction page for a member whose mem_master.dob is empty.</summary>
    public class EcsMemberDobVM
    {
        public decimal MemId { get; set; }
        public DateTime DateOfBirth { get; set; }
    }

    /// <summary>Optional payroll prefill of the pay details (latest Pay_Slip).</summary>
    public class EcsPayDetailsVM
    {
        public bool Found { get; set; }
        public double BasicPay { get; set; }
        public double GrossPay { get; set; }
        public double SocietyDeduction { get; set; }
        public double TotalDeductions { get; set; }
    }

    /// <summary>Request for <c>GetRecoverables</c>.</summary>
    public class EcsRecoverablesRequestVM
    {
        public decimal MemId { get; set; }
        public decimal SuretyMemId { get; set; }
        public int SchemeId { get; set; }
        public double LoanAmount { get; set; }
        public int PeriodOfLoan { get; set; }
        public double Roi { get; set; }
        public double BasicPay { get; set; }
        public DateTime? FirstDueDate { get; set; }
    }

    /// <summary>Everything the Loan Details / Recoverables / Non-Deductable / Pending Deposits tabs need.</summary>
    public class EcsRecoverablesVM
    {
        public bool Success { get; set; } = true;
        public string? Message { get; set; }

        public List<DeductableLoanRowVM> Recoverables { get; set; } = new();
        public List<NonDeductableLoanRowVM> NonDeductable { get; set; } = new();
        public List<PendingDepositRowVM> PendingDeposits { get; set; } = new();
        public List<SuretyLoanOverdueVM> SuretyOverdues { get; set; } = new();

        public double MemberSC { get; set; }
        public double SuretySC { get; set; }
        public double MemberTD { get; set; }
        public double MemberFWD { get; set; }
        public double RequiredSC { get; set; }

        /// <summary>Calculated instalment (§3.2). The page may raise it but not lower it.</summary>
        public int InstalmentAmount { get; set; }
        public int NoOfInstalmentsCompleted { get; set; }
        public int NoOfInstalmentsRecovered { get; set; }

        /// <summary>Σ demand of every non-deductable loan (before any is moved into recoverables).</summary>
        public double ExistingDemand { get; set; }
        public double NonDeductableOS { get; set; }
        public double NonDeductableOSWithinLimit { get; set; }
        public double DepositDemand { get; set; }
    }

    /// <summary>A loan of the member that the selected scheme does NOT deduct automatically.</summary>
    public class NonDeductableLoanRowVM
    {
        public decimal LoanId { get; set; }
        public string? LoanNo { get; set; }
        public int SchemeId { get; set; }
        public string? Purpose { get; set; }
        public double LoanOS { get; set; }
        public double PrlOD { get; set; }
        public double IntBal { get; set; }
        public double PIBal { get; set; }
        public decimal PrlLedId { get; set; }
        public decimal IntLedId { get; set; }
        public decimal PILedId { get; set; }
        public string? PrlLedName { get; set; }
        public string? IntLedName { get; set; }
        public string? PILedName { get; set; }
        public int LoanSlNo { get; set; }
        public double DemandAmount { get; set; }
        public bool AdoptLoanLimit { get; set; }
        public int MatchSC { get; set; }

        // Editable on the page before "Add to Recovery"
        public double PrlRecovery { get; set; }
        public double IntRecovery { get; set; }
        public double PIRecovery { get; set; }
    }

    /// <summary>A deposit with demand raised but not yet received.</summary>
    public class PendingDepositRowVM
    {
        public int DMId { get; set; }
        public decimal MemId { get; set; }
        public decimal LedId { get; set; }
        public string? LedName { get; set; }
        public double Balance { get; set; }
        public double PendingAmount { get; set; }
        public double CalcAmount { get; set; }
        public double ODAmount { get; set; }
        /// <summary>Editable on the page before "Add to Recovery".</summary>
        public double RecoveryAmount { get; set; }
    }

    /// <summary>Filter for the Sanctions tab.</summary>
    public class EcsSanctionListFilterVM
    {
        /// <summary>ALL | ID | MEM | DATE</summary>
        public string Mode { get; set; } = "ALL";
        public decimal? SanctionId { get; set; }
        public string? MemberNo { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }

    public class EcsSanctionListItemVM
    {
        public decimal LoanSanction_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public decimal SuretyMem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public DateTime? SanctionDate { get; set; }
        public double LoanAmount { get; set; }
        public int Scheme_Id { get; set; }
        public string? SchemeName { get; set; }
    }

    /// <summary>A saved sanction loaded back for Modify / Delete / Print.</summary>
    public class EcsSanctionDetailVM
    {
        public decimal LoanSanction_Id { get; set; }
        public DateTime? SanctionDate { get; set; }
        public SuretyLoanSanctionSaveVM Sanction { get; set; } = new();
        public SuretyLoanSanctionDataVM Member { get; set; } = new();
        public List<DeductableLoanRowVM> Recoverables { get; set; } = new();
        public double TotalReceiptAmount { get; set; }
    }

    /// <summary>Server-side user context for writes, taken from claims (never from the body).</summary>
    public class EcsUserContextVM
    {
        public decimal UsrId { get; set; }
        public decimal YrId { get; set; }
        public string BrCode { get; set; } = "";
        public DateTime CurrentDate { get; set; }
        public DateTime YrBeginningDate { get; set; }
        public DateTime YrEndDate { get; set; }
    }

    /// <summary>Loan balances of a member as summed from Loan_Trn (repository → handler).</summary>
    public class EcsLoanBalanceVM
    {
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public int Scheme_Id { get; set; }
        public string? Scheme_Name { get; set; }
        public decimal PrlLed_Id { get; set; }
        public decimal IntLed_Id { get; set; }
        public decimal PILed_Id { get; set; }
        public double San_Amt { get; set; }
        public double Roi { get; set; }
        public double Inst_Amt { get; set; }
        public int Int_Application { get; set; }
        public int AdoptLoanLimit { get; set; }
        public int MatchShareCapital { get; set; }
        public double PrlOS { get; set; }
        public double PrlOD { get; set; }
        public double IntOS { get; set; }
        public double PIBal { get; set; }
        public int MaxTrnSlNo { get; set; }
        /// <summary>Number of principal demands raised.</summary>
        public int DemandsRaised { get; set; }
        /// <summary>Number of demand rows with principal collected.</summary>
        public int DemandsRecovered { get; set; }
    }
}
