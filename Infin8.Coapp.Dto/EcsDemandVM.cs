namespace Infin8.Coapp.Dto
{
    // ECS Demand Calculation (spec 12) — page ⇄ api/ecsdemand.

    public class EcsDemandRequestVM
    {
        public DateTime DueDate { get; set; }
    }

    public class EcsDemandResultVM
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> Reasons { get; set; } = new();
        public decimal DemandId { get; set; }
    }

    /// <summary>One mem_demand_master row with its state and line totals.</summary>
    public class EcsDemandMasterVM
    {
        public decimal DemandId { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? CalculationDate { get; set; }
        public DateTime? NextDemandDate { get; set; }
        public bool IsPosted { get; set; }
        public bool IsRecoveryAppropriated { get; set; }
        public bool IsRecoveryUpdated { get; set; }
        public string State => IsRecoveryUpdated ? "Recovery saved" : IsPosted ? "Posted" : "Calculated";
        public int Lines { get; set; }
        public double Total { get; set; }
    }

    public class EcsDemandStateVM
    {
        public DateTime BusinessDay { get; set; }
        /// <summary>False when the login's business day is no longer the branch's open day (re-login needed).</summary>
        public bool IsOpenBusinessDay { get; set; }
        public string? Message { get; set; }

        /// <summary>Latest live demand (calculated or posted), null when none exists.</summary>
        public EcsDemandMasterVM? Current { get; set; }
        public EcsDemandSummaryVM? CurrentSummary { get; set; }

        public DateTime NextDueDate { get; set; }
        public bool CanCalculate { get; set; }
        public string? CalculateBlockedReason { get; set; }
        public bool CanDeletePosted { get; set; }
        public List<string> DeletePostedBlockedReasons { get; set; } = new();

        public List<EcsDemandMasterVM> History { get; set; } = new();
    }

    /// <summary>A calculated demand — a preview or a saved one — summarised for the page.</summary>
    public class EcsDemandSummaryVM
    {
        public decimal DemandId { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CalculationDate { get; set; }
        public int MemberCount { get; set; }
        public int StoppedMemberCount { get; set; }
        public int LineCount { get; set; }
        public double GrandTotal { get; set; }
        public List<EcsDemandComponentTotalVM> Components { get; set; } = new();
        public List<EcsDemandOfficeTotalVM> Offices { get; set; } = new();
        /// <summary>Per-member drill-down; empty when only totals were asked for.</summary>
        public List<EcsDemandMemberVM> Members { get; set; } = new();
        /// <summary>Preview only: why Save would be refused, if it would.</summary>
        public List<string> Messages { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public bool CanSave { get; set; }
    }

    /// <summary>
    /// Totals of one demand type. "Principal" is the loan principal, the deposit subscription, the RD instalments
    /// or the due-to balance; "Interest" applies to loans only.
    /// </summary>
    public class EcsDemandComponentTotalVM
    {
        public string Type { get; set; } = "";
        public string Name { get; set; } = "";
        public int Lines { get; set; }
        public int StoppedLines { get; set; }
        public double PrincipalArrear { get; set; }
        public double PrincipalCurrent { get; set; }
        public double InterestArrear { get; set; }
        public double InterestCurrent { get; set; }
        public double PenalArrear { get; set; }
        public double PenalCurrent { get; set; }
        public double Total { get; set; }
    }

    public class EcsDemandOfficeTotalVM
    {
        public decimal OfficeId { get; set; }
        public int Members { get; set; }
        public int StoppedMembers { get; set; }
        public double Loans { get; set; }
        public double Deposits { get; set; }
        public double Others { get; set; }
        public double Total { get; set; }
    }

    public class EcsDemandMemberVM
    {
        public decimal MemId { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public decimal OfficeId { get; set; }
        public bool Stopped { get; set; }
        public double Total { get; set; }
        public List<EcsDemandLineVM> Lines { get; set; } = new();
    }

    public class EcsDemandLineVM
    {
        public string Type { get; set; } = "";
        public decimal AccountId { get; set; }
        public string? Account { get; set; }
        public double PrincipalArrear { get; set; }
        public double PrincipalCurrent { get; set; }
        public double InterestArrear { get; set; }
        public double InterestCurrent { get; set; }
        public double PenalArrear { get; set; }
        public double PenalCurrent { get; set; }
        public double Total { get; set; }
        public double LoanOutstanding { get; set; }
    }

    // ECS Stop Demand — members excluded from one due date's demand (mem_demand_stop).

    public class EcsStopDemandRequestVM
    {
        public DateTime DueDate { get; set; }
        public string MemberNo { get; set; } = "";
    }

    public class EcsStopDemandListVM
    {
        public DateTime DueDate { get; set; }
        /// <summary>True when the demand for this due date is already calculated, so the list cannot change.</summary>
        public bool IsLocked { get; set; }
        public string? LockedReason { get; set; }
        public List<EcsStopDemandMemberVM> Members { get; set; } = new();
    }

    public class EcsStopDemandMemberVM
    {
        public decimal Id { get; set; }
        public decimal MemId { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public decimal OfficeId { get; set; }
        /// <summary>Inactive members (status 2) are stopped anyway; the row is then redundant.</summary>
        public bool IsInactive { get; set; }
    }

    // ECS Delete Demand for Expired Members — soft-delete demand rows posted after a member's death (TSIS
    // frmLoanDemandDeleteForExpiredMembers): loan_trn.TrnTr_Delete + ExpiredDeletion, revocable.

    public class EcsExpiredMemberVM
    {
        public decimal MemId { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public DateTime? ExpiredDate { get; set; }
        /// <summary>Why the member cannot be used here (not marked expired), or null.</summary>
        public string? Message { get; set; }
        public List<EcsExpiredLoanVM> Loans { get; set; } = new();
    }

    public class EcsExpiredLoanVM
    {
        public decimal LoanId { get; set; }
        public string? LoanNo { get; set; }
        public string? SchemeName { get; set; }
        public bool IsClosed { get; set; }
    }

    public class EcsExpiredLoanRowsVM
    {
        public decimal LoanId { get; set; }
        public DateTime ExpiredDate { get; set; }
        /// <summary>Rows from the last one on or before the expiry date onwards.</summary>
        public List<EcsExpiredLoanTrnVM> Rows { get; set; } = new();
        /// <summary>Rows deleted here before, which can be revoked.</summary>
        public List<EcsExpiredLoanTrnVM> Deleted { get; set; } = new();
    }

    public class EcsExpiredLoanTrnVM
    {
        public decimal TrnId { get; set; }
        public string? Status { get; set; }
        public DateTime TrnDate { get; set; }
        public double Disb { get; set; }
        public double PrlDem { get; set; }
        public double IntCalc { get; set; }
        public DateTime? IntCalcDate { get; set; }
        public double PICalc { get; set; }
        public double PrlColl { get; set; }
        public double IntColl { get; set; }
        public double PIColl { get; set; }
        public bool CanAct { get; set; }
        /// <summary>Why the row cannot be deleted / revoked, or null.</summary>
        public string? Reason { get; set; }
    }
}
