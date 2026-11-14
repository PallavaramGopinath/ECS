using Infin8.Coapp.Models;

namespace Infin8.Coapp.Repository
{
    /// <summary>
    /// Reads and change-tracker writes for ECS Recovery Appropriation (spec 13). Every query filters BrCode on every
    /// alias. Nothing here calls SaveChanges — EcsRecoveryHandler does. Demand masters, status rows and the open
    /// business day are read through <see cref="IEcsDemandRepository"/>.
    /// </summary>
    public interface IEcsRecoveryRepository
    {
        Task<Dictionary<decimal, string>> GetOfficeNamesAsync(string brCode);
        /// <summary>Per office (member's current office): members, demand and collection of the demand's live lines.</summary>
        Task<List<EcsRecoveryOfficeStatRow>> GetOfficeStatsAsync(decimal demandId, string brCode);
        /// <summary>Open members whose current office is <paramref name="officeId"/>.</summary>
        Task<List<EcsRecoveryMemberRow>> GetOfficeMembersAsync(decimal officeId, string brCode);
        /// <summary>Open members of the branch keyed by member no and per no (for the remittance list).</summary>
        Task<List<EcsRecoveryMemberRow>> GetBranchMembersAsync(string brCode);
        /// <summary>Live lines of the demand whose member's current office is <paramref name="officeId"/>.</summary>
        Task<List<Mem_Demand>> GetOfficeLinesAsync(decimal demandId, decimal officeId, string brCode, bool forUpdate);
        Task<List<Mem_CollectionPriority>> GetPrioritiesAsync(string brCode);

        Task<Dictionary<decimal, EcsRecoveryLoanRow>> GetLoansAsync(IEnumerable<decimal> loanIds, string brCode);
        /// <summary>Open Loan_Type 1 loans of the members with principal outstanding.</summary>
        Task<List<EcsRecoveryLoanRow>> GetOpenLoansOfMembersAsync(IEnumerable<decimal> memIds, string brCode);
        /// <summary>Σ Pmt_Amt − Σ Rpt_Amt of Trn_Type 1 (due-to) rows, keyed (memId, ledId).</summary>
        Task<Dictionary<(decimal, decimal), double>> GetDueToBalancesAsync(IEnumerable<decimal> memIds, string brCode);
        Task<Dictionary<decimal, string>> GetLoanLabelsAsync(IEnumerable<decimal> loanIds, string brCode);

        // ----- save -----
        Task<int> GetMaxRecoveryIdAsync(string brCode);
        Task<List<EcsRecoveryLedgerRow>> GetLedgersAsync(string brCode);
        Task<decimal?> GetLastDebitLedgerAsync(decimal officeId, string brCode);
        Task<Dictionary<int, Loan_Schemes>> GetSchemesAsync(IEnumerable<int> schemeIds, string brCode);
        Task<Dictionary<int, (decimal LedId, decimal PiLedId)>> GetDepositLedgersAsync(string brCode);
        Task<Dictionary<decimal, decimal>> GetRdLedgersAsync(IEnumerable<decimal> tdIds, string brCode);
        Task<Dictionary<decimal, DateTime>> GetLastLoanTrnDatesAsync(IEnumerable<decimal> loanIds, string brCode);
        Task<Dictionary<decimal, string?>> GetVoucherNosAsync(IEnumerable<decimal> vocIds, string brCode);
        Task<EcsRecoveryIdAllocator> CreateIdAllocatorAsync(string brCode);
        Task AddLinesAsync(IEnumerable<Mem_Demand> lines);
        Task AddSaveAsync(EcsRecoverySavePosting posting);

        // ----- delete saved -----
        Task<EcsRecoveryReceipts> GetReceiptsForUpdateAsync(decimal vocId, string brCode);
        Task<List<string>> GetLaterTransactionsAsync(decimal vocId, string brCode);

        // ----- recovery refund (spec 13 §16) -----
        Task<List<Mem_Demand>> GetMemberLinesAsync(decimal demandId, decimal memId, string brCode, bool forUpdate);
        /// <summary>The member's loan / deposit / RD receipts on the voucher, plus every voucher line (all tracked).</summary>
        Task<EcsRecoveryReceipts> GetMemberReceiptsForUpdateAsync(decimal vocId, decimal memId, string brCode);
        /// <summary>The member's accounts with a transaction after their receipt on the voucher.</summary>
        Task<List<string>> GetLaterMemberTransactionsAsync(decimal vocId, decimal memId, string brCode);
        Task<Dictionary<int, string>> GetDepositNamesAsync(string brCode);
        Task<Dictionary<decimal, string>> GetRdNosAsync(IEnumerable<decimal> tdIds, string brCode);

        // ----- spec 13a -----
        /// <summary>Display names of users (Maker / Checker on the page).</summary>
        Task<Dictionary<decimal, string>> GetUserNamesAsync(IEnumerable<decimal> userIds, string brCode);
        /// <summary>Scheme rules, rates and every valid loan_trn row of the loans — the fresh-accrual inputs (§4.2).</summary>
        Task<Dictionary<decimal, EcsRecoveryAccrualLoan>> GetAccrualInputsAsync(IEnumerable<decimal> loanIds, string brCode);
        /// <summary>deposit_trn sums per (member, deposit): before the due date, since it, and all dates (§4.3).</summary>
        Task<Dictionary<(decimal MemId, int DepositId), EcsRecoveryDepositRow>> GetDepositRevisionAsync(IEnumerable<decimal> memIds, DateTime dueDate, string brCode);
        /// <summary>RD masters with termdeposit_trn sums as on now and the RD penal rate (§4.4).</summary>
        Task<Dictionary<decimal, EcsRecoveryRdRow>> GetRdRevisionAsync(IEnumerable<decimal> tdIds, DateTime dueDate, string brCode);
        /// <summary>Members by id, open or not (a demand line's member may have closed since the demand).</summary>
        Task<List<EcsRecoveryMemberRow>> GetMembersByIdAsync(IEnumerable<decimal> memIds, string brCode);
    }

    public class EcsRecoveryOfficeStatRow
    {
        public decimal OfficeId { get; set; }
        public int Members { get; set; }
        public double Demand { get; set; }
        public double Recovery { get; set; }
        public int RecoveryId { get; set; }
        public DateOnly? RecoveryDate { get; set; }
    }

    public class EcsRecoveryMemberRow
    {
        public decimal MemId { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public decimal OfficeId { get; set; }
        /// <summary>Not deleted and not closed (only <see cref="IEcsRecoveryRepository.GetMembersByIdAsync"/> fills it; the other queries return open members only).</summary>
        public bool IsOpen { get; set; } = true;
        public bool IsExpired { get; set; }
        public DateTime? ExpiredDate { get; set; }
    }

    public class EcsRecoveryAccrualLoan
    {
        public decimal LoanId { get; set; }
        public int IntApplication { get; set; }
        public int PIApplication { get; set; }
        public double Roi { get; set; }
        public double Pi { get; set; }
        public List<Loan_Trn> Trns { get; set; } = new();
        public List<Loan_Roi> Rates { get; set; } = new();
    }

    public class EcsRecoveryDepositRow
    {
        public decimal MemId { get; set; }
        public int DepositId { get; set; }
        public double BeforeDemand { get; set; }
        public double BeforeReceipt { get; set; }
        public double BeforePiCalc { get; set; }
        public double BeforePiReceived { get; set; }
        public double SinceReceipt { get; set; }
        public double SincePiReceived { get; set; }
        public double SincePaid { get; set; }
        public double AllReceipt { get; set; }
        public double AllPaid { get; set; }
    }

    public class EcsRecoveryRdRow
    {
        public decimal TdId { get; set; }
        public int SchemeId { get; set; }
        public DateTime ValueDate { get; set; }
        public int PeriodInMonths { get; set; }
        public double Instalment { get; set; }
        public bool Closed { get; set; }
        public double Received { get; set; }
        public double Paid { get; set; }
        public double PenalCalcBeforeDue { get; set; }
        public double PenalReceived { get; set; }
        public double PenalRate { get; set; }
    }

    public class EcsRecoveryLoanRow
    {
        public decimal LoanId { get; set; }
        public decimal MemId { get; set; }
        public int SchemeId { get; set; }
        public DateTime? SanDate { get; set; }
        public double Disb { get; set; }
        public double PrlColl { get; set; }
        public double IntCalc { get; set; }
        public double IntColl { get; set; }
        public double PICalc { get; set; }
        public double PIColl { get; set; }
    }

    public class EcsRecoveryLedgerRow
    {
        public decimal LedId { get; set; }
        public string? LedName { get; set; }
        public bool IsBank { get; set; }
    }

    /// <summary>Everything one Save adds.</summary>
    public class EcsRecoverySavePosting
    {
        public Fin_Voucher? Voucher { get; set; }
        public List<Fin_Voucher_Trn> VoucherTrns { get; } = new();
        public List<Fin_Voucher_Bank> BankRows { get; } = new();
        public List<Loan_Trn> LoanTrns { get; } = new();
        public List<Deposit_Trn> DepositTrns { get; } = new();
        public List<Mem_Trn> MemTrns { get; } = new();
        public List<TermDeposit_Trn> RdTrns { get; } = new();
    }

    /// <summary>The tracked rows of one saved recovery voucher.</summary>
    public class EcsRecoveryReceipts
    {
        public List<Fin_Voucher> Vouchers { get; set; } = new();
        public List<Fin_Voucher_Trn> VoucherTrns { get; set; } = new();
        public List<Fin_Voucher_Bank> BankRows { get; set; } = new();
        public List<Loan_Trn> LoanTrns { get; set; } = new();
        public List<Deposit_Trn> DepositTrns { get; set; } = new();
        public List<Mem_Trn> MemTrns { get; set; } = new();
        public List<TermDeposit_Trn> RdTrns { get; set; } = new();
    }
}
