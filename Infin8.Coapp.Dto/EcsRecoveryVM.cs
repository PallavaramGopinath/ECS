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
        public string State => IsSaved ? "Saved" : IsAppropriated ? "Appropriated" : "Pending";
    }

    /// <summary>One office of a demand, with its members, for the appropriate / save / delete views.</summary>
    public class EcsRecoveryOfficeDetailVM
    {
        public decimal DemandId { get; set; }
        public DateTime DueDate { get; set; }
        public EcsRecoveryOfficeVM Office { get; set; } = new();
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
        /// <summary>"M" — items are keyed by member no; "P" — by per no.</summary>
        public string MatchBy { get; set; } = "M";
        public List<EcsRecoveryItemVM> Items { get; set; } = new();
    }

    public class EcsRecoveryItemVM
    {
        public string? Key { get; set; }
        public double Amount { get; set; }
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
}
