namespace Infin8.Coapp.Dto
{
    // ECS Recovery Appropriation (spec 13) — page ⇄ api/ecsrecovery.

    public class EcsRecoveryResultVM
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> Reasons { get; set; } = new();
        public string? VocNo { get; set; }
    }

    public class EcsRecoveryStateVM
    {
        public DateTime BusinessDay { get; set; }
        public bool IsOpenBusinessDay { get; set; }
        public string? Message { get; set; }
        /// <summary>Posted demands, newest first: those still being recovered and the last fully saved one.</summary>
        public List<EcsRecoveryDemandVM> Demands { get; set; } = new();
    }

    public class EcsRecoveryDemandVM
    {
        public decimal DemandId { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsRecoveryAppropriated { get; set; }
        public bool IsRecoveryUpdated { get; set; }
        /// <summary>A later demand is calculated, so saved recoveries of this one can no longer be deleted.</summary>
        public bool HasLaterDemand { get; set; }
        public List<EcsRecoveryOfficeVM> Offices { get; set; } = new();
    }

    public class EcsRecoveryOfficeVM
    {
        public decimal OfficeId { get; set; }
        public string? OfficeName { get; set; }
        public int Members { get; set; }
        public double Demand { get; set; }
        public double Recovery { get; set; }
        public bool IsAppropriated { get; set; }
        public bool IsSaved { get; set; }
        public int RecoveryId { get; set; }
        public DateTime? RecoveryDate { get; set; }
        public decimal VocId { get; set; }
        public string? VocNo { get; set; }
        public string State => IsSaved ? "Saved" : IsAppropriated ? "Awaiting check" : "Pending";

        // Spec 13a Maker / Checker
        public decimal? AppropriatedBy { get; set; }
        public string? AppropriatedByName { get; set; }
        public DateTime? AppropriatedOn { get; set; }
        /// <summary>Set when a Checker rejected the last appropriation (cleared by the next one).</summary>
        public string? RejectReason { get; set; }
        public string? RejectedByName { get; set; }
        public DateTime? RejectedOn { get; set; }
    }

    /// <summary>One office of a demand, with its members, for the appropriate / save / delete views.</summary>
    public class EcsRecoveryOfficeDetailVM
    {
        public decimal DemandId { get; set; }
        public DateTime DueDate { get; set; }
        public EcsRecoveryOfficeVM Office { get; set; } = new();
        /// <summary>Spec 13a §2: ledger rows are dated on the recovery date (the due date) …</summary>
        public DateTime RecoveryDate { get; set; }
        /// <summary>… and the voucher on the application date (the business day of the Checker's save).</summary>
        public DateTime ApplicationDate { get; set; }
        /// <summary>The caller appropriated this office and is not Admin, so cannot save or reject it (spec 13a §6).</summary>
        public bool IsOwnAppropriation { get; set; }
        public List<EcsRecoveryMemberVM> Members { get; set; } = new();
        public EcsRecoveryHeadTotalsVM Totals { get; set; } = new();

        /// <summary>Debit (remittance) ledgers for Save; bank ledgers need cheque / reference details.</summary>
        public List<EcsRecoveryLedgerVM> Ledgers { get; set; } = new();
        public decimal DefaultDebitLedId { get; set; }

        public bool CanDeleteSaved { get; set; }
        public List<string> DeleteSavedBlockedReasons { get; set; } = new();
    }

    public class EcsRecoveryHeadTotalsVM
    {
        public double Principal { get; set; }
        public double Interest { get; set; }
        public double Penal { get; set; }
        public double Deposit { get; set; }
        public double DueTo { get; set; }
        public double Rd { get; set; }
        public double Advance { get; set; }
        public double Excess { get; set; }
        public double Total { get; set; }
    }

    public class EcsRecoveryMemberVM
    {
        public decimal MemId { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public bool Stopped { get; set; }
        /// <summary>Demand of the non-stopped lines (revised once appropriated).</summary>
        public double Demand { get; set; }
        public double Recovery { get; set; }
        public double Loan { get; set; }
        public double Deposit { get; set; }
        public double Other { get; set; }
        public double Advance { get; set; }
        public double Excess { get; set; }
        public List<EcsRecoveryLineVM> Lines { get; set; } = new();
    }

    public class EcsRecoveryLineVM
    {
        public string? Type { get; set; }
        public string? Account { get; set; }
        public bool Stopped { get; set; }
        public double Demand { get; set; }
        public double Principal { get; set; }
        public double Interest { get; set; }
        public double Penal { get; set; }
        public double Amount { get; set; }
        public double Collected { get; set; }
    }

    public class EcsRecoveryLedgerVM
    {
        public decimal LedId { get; set; }
        public string? LedName { get; set; }
        public bool IsBank { get; set; }
    }

    public class EcsAppropriateRequestVM
    {
        public decimal DemandId { get; set; }
        public decimal OfficeId { get; set; }
        public List<EcsRecoveryItemVM> Items { get; set; } = new();
    }

    /// <summary>
    /// One member's recovery: a row of the uploaded sheet (spec 13a §3) or an amount typed on the page. Matched by
    /// Employee No (<c>mem_master.perno</c>) and/or Member No; when both are given they must name the same member.
    /// </summary>
    public class EcsRecoveryItemVM
    {
        /// <summary>Excel row number; 0 for an amount typed on the page.</summary>
        public int RowNo { get; set; }
        public string? EmployeeNo { get; set; }
        public string? MemberNo { get; set; }
        public string? SheetName { get; set; }
        public double Amount { get; set; }
    }

    /// <summary>The uploaded sheet matched against the office's members (spec 13a §3). Nothing is written.</summary>
    public class EcsSheetPreviewVM
    {
        public string? FileName { get; set; }
        public List<EcsSheetPreviewRowVM> Rows { get; set; } = new();
        /// <summary>Problems that are not tied to one row (unreadable file, no header …).</summary>
        public List<string> Errors { get; set; } = new();
        public double SheetTotal { get; set; }
        public int MatchedCount { get; set; }
        public double MatchedTotal { get; set; }
        /// <summary>The office's demand (non-stopped lines), for comparison.</summary>
        public double OfficeDemand { get; set; }
        public int ErrorCount => Errors.Count + Rows.Count(r => r.Status == "Error");
        public int WarningCount => Rows.Count(r => r.Status == "Warning");
        public bool CanAppropriate => ErrorCount == 0 && MatchedCount > 0;
    }

    public class EcsSheetPreviewRowVM
    {
        public int RowNo { get; set; }
        public string? EmployeeNo { get; set; }
        public string? MemberNo { get; set; }
        public string? SheetName { get; set; }
        public decimal MemId { get; set; }
        public string? MatchedMemberNo { get; set; }
        public string? MatchedName { get; set; }
        public double Amount { get; set; }
        /// <summary>"OK", "Warning" (name differs) or "Error".</summary>
        public string Status { get; set; } = "OK";
        public string? Message { get; set; }
    }

    /// <summary>A specimen recovery sheet for a pay office (spec 13a §3).</summary>
    public class EcsSpecimenFileVM
    {
        public string FileName { get; set; } = "";
        public string ContentType { get; set; } = "";
        public byte[] Content { get; set; } = Array.Empty<byte>();
    }

    public class EcsRejectRequestVM
    {
        public decimal DemandId { get; set; }
        public decimal OfficeId { get; set; }
        public string? Reason { get; set; }
    }

    public class EcsSaveRecoveryRequestVM
    {
        public decimal DemandId { get; set; }
        public decimal OfficeId { get; set; }
        public List<EcsRecoveryDebitVM> Debits { get; set; } = new();
    }

    public class EcsRecoveryDebitVM
    {
        public decimal LedId { get; set; }
        public double Amount { get; set; }
        public string? ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }
        public string? BankName { get; set; }
    }

    public class EcsRecoveryOfficeRequestVM
    {
        public decimal DemandId { get; set; }
        public decimal OfficeId { get; set; }
    }

    // ----- Recovery Refund (spec 13 §16) -----

    /// <summary>One member's saved recovery against a demand, with what can still be refunded per receipt.</summary>
    public class EcsRefundVM
    {
        public decimal DemandId { get; set; }
        public DateTime DueDate { get; set; }
        public decimal MemId { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? OfficeName { get; set; }
        public string? VocNo { get; set; }
        public string? ExcessLedgerName { get; set; }
        public bool CanRefund { get; set; }
        public List<string> BlockedReasons { get; set; } = new();
        public List<EcsRefundLineVM> Lines { get; set; } = new();
        /// <summary>Already moved to the excess ledger for this demand (B lines).</summary>
        public double Excess { get; set; }
    }

    /// <summary>One receipt row of the recovery voucher: Kind L (loan_trn), D (deposit_trn) or R (termdeposit_trn).</summary>
    public class EcsRefundLineVM
    {
        public string Kind { get; set; } = "";
        public decimal TrnId { get; set; }
        public string? Account { get; set; }
        /// <summary>Loan principal; deposit / RD receipt.</summary>
        public double Principal { get; set; }
        public double Interest { get; set; }
        public double Penal { get; set; }
    }

    public class EcsRefundRequestVM
    {
        public decimal DemandId { get; set; }
        public decimal MemId { get; set; }
        public List<EcsRefundLineVM> Lines { get; set; } = new();
    }
}
