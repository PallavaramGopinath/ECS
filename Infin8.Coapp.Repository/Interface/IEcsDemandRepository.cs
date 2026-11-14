using Infin8.Coapp.Models;

namespace Infin8.Coapp.Repository
{
    /// <summary>
    /// Reads and change-tracker writes for ECS Demand Calculation (spec 12). Every query filters BrCode on every
    /// alias; ledger aggregates count only valid rows dated before the due date. Nothing here calls SaveChanges.
    /// </summary>
    public interface IEcsDemandRepository
    {
        // ----- calculation inputs -----
        Task<List<EcsDemandMemberRow>> GetMembersAsync(string brCode);
        Task<List<decimal>> GetStoppedMemberIdsAsync(DateTime dueDate, string brCode);
        Task<List<EcsDemandLoanRow>> GetOpenLoansAsync(string brCode);
        Task<List<EcsDemandLoanSumRow>> GetLoanSumsAsync(string brCode, DateTime dueDate);
        Task<List<Loan_Trn>> GetLoanTrnRowsAsync(IEnumerable<decimal> loanIds, string brCode, DateTime dueDate);
        Task<List<Loan_Roi>> GetRoiRowsAsync(IEnumerable<decimal> loanIds, string brCode, DateTime dueDate);
        Task<List<Deposit_Masters>> GetDepositMastersAsync(string brCode);
        Task<List<Deposit_Options>> GetDepositOptionsAsync(string brCode, DateTime dueDate);
        Task<List<Deposit_Roi_Templates>> GetDepositRoiAsync(string brCode);
        Task<List<EcsDemandDepositSumRow>> GetDepositSumsAsync(string brCode, DateTime dueDate);
        Task<List<EcsDemandRdRow>> GetOpenRdsAsync(string brCode, DateTime dueDate);
        Task<List<EcsDemandDueToRow>> GetDueToSumsAsync(string brCode, DateTime dueDate);

        // ----- saved demands -----
        Task<List<Mem_Demand_Master>> GetLiveMastersAsync(string brCode, int take);
        Task<Mem_Demand_Master?> GetMasterForUpdateAsync(decimal demandId, string brCode);
        Task<List<Mem_Demand>> GetLinesAsync(decimal demandId, string brCode, bool forUpdate);
        Task<List<Mem_Demand_Status>> GetStatusRowsForUpdateAsync(decimal demandId, string brCode);
        Task<List<EcsDemandTotalRow>> GetTotalsAsync(IEnumerable<decimal> demandIds, string brCode);
        Task<Dictionary<string, string>> GetAccountLabelsAsync(IEnumerable<Mem_Demand> lines, string brCode);

        // ----- posted demands -----
        Task<List<Loan_Trn>> GetPostedLoanTrnsAsync(decimal demandId, string brCode);
        Task<List<Deposit_Trn>> GetPostedDepositTrnsAsync(decimal demandId, string brCode);
        Task<List<TermDeposit_Trn>> GetPostedRdTrnsAsync(decimal demandId, string brCode);
        /// <summary>Accounts with a valid row added after this demand's posted row (higher Trn_SlNo).</summary>
        Task<List<string>> GetLaterTransactionsAsync(decimal demandId, string brCode);

        // ----- stop demand (mem_demand_stop) -----
        Task<List<EcsStopRow>> GetStopRowsAsync(DateTime dueDate, string brCode);
        Task<List<Mem_Demand_Stop>> GetActiveStopRowsForUpdateAsync(decimal memId, DateTime dueDate, string brCode);
        Task<Mem_Demand_Stop?> GetStopRowForUpdateAsync(decimal id, string brCode);
        /// <summary>A member of the branch that is not deleted or closed, by member no.</summary>
        Task<EcsDemandMemberRow?> FindMemberAsync(string memberNo, string brCode);
        /// <summary>A member of the branch that is not deleted (closed accounts included), by member no.</summary>
        Task<EcsDemandMemberRow?> FindAnyMemberAsync(string memberNo, string brCode);
        /// <summary>Adds a stop row with the next branch-prefixed MAX+1 id (change tracker only).</summary>
        Task AddStopRowAsync(Mem_Demand_Stop row);

        // ----- delete demand for expired members (loan_trn.ExpiredDeletion) -----
        Task<List<EcsExpiredLoanRow>> GetMemberLoansAsync(decimal memId, string brCode);
        /// <summary>Valid rows of the loan from its last row dated on or before the expiry date onwards (TSIS window).</summary>
        Task<List<Loan_Trn>> GetExpiredWindowRowsAsync(decimal loanId, DateTime expiryDate, string brCode);
        Task<List<Loan_Trn>> GetExpiredDeletedRowsAsync(decimal loanId, string brCode);
        Task<Loan_Trn?> GetLoanTrnForUpdateAsync(decimal trnId, string brCode);
        Task<EcsDemandMemberRow?> GetMemberOfLoanAsync(decimal loanId, string brCode);

        // ----- business day -----
        Task<DateTime?> GetOpenBusinessDayAsync(string brCode);
        /// <summary>True when no later non-holiday business day of the same month exists for the branch.</summary>
        Task<bool> IsLastBusinessDayOfMonthAsync(DateTime date, string brCode);

        // ----- writes -----
        Task<EcsDemandIdAllocator> CreateIdAllocatorAsync(string brCode);
        Task AddCalculationAsync(Mem_Demand_Master master, IEnumerable<Mem_Demand> lines, IEnumerable<Mem_Demand_Status> statuses);
        Task AddPostingAsync(IEnumerable<Loan_Trn> loanTrns, IEnumerable<Deposit_Trn> depositTrns, IEnumerable<TermDeposit_Trn> rdTrns);
    }

    public class EcsDemandMemberRow
    {
        public decimal MemId { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public decimal OfficeId { get; set; }
        public int MemberStatus { get; set; }
        public double BasicPay { get; set; }
        public bool IsExpired { get; set; }
        public DateTime? ExpiredDate { get; set; }
    }

    public class EcsExpiredLoanRow
    {
        public decimal LoanId { get; set; }
        public string? LoanNo { get; set; }
        public string? SchemeName { get; set; }
        public bool IsClosed { get; set; }
    }

    public class EcsStopRow
    {
        public decimal Id { get; set; }
        public decimal MemId { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public decimal OfficeId { get; set; }
        public int MemberStatus { get; set; }
    }

    public class EcsDemandLoanRow
    {
        public decimal LoanId { get; set; }
        public string? LoanNo { get; set; }
        public decimal MemId { get; set; }
        public int SchemeId { get; set; }
        public string? SchemeName { get; set; }
        public int InstType { get; set; }
        public int DemFrequency { get; set; }
        public int IntApplication { get; set; }
        public int PIApplication { get; set; }
        public int AdvPrlApplication { get; set; }
        public double InstAmt { get; set; }
        public int PrlPrd { get; set; }
        public DateTime? FirstPrlDueDate { get; set; }
        public double Roi { get; set; }
        public double Pi { get; set; }
    }

    public class EcsDemandLoanSumRow
    {
        public decimal LoanId { get; set; }
        public double Disb { get; set; }
        public double PrlDem { get; set; }
        public double PrlColl { get; set; }
        public double IntCalc { get; set; }
        public double IntColl { get; set; }
        public double PICalc { get; set; }
        public double PIColl { get; set; }
    }

    public class EcsDemandDepositSumRow
    {
        public decimal MemId { get; set; }
        public int DepositId { get; set; }
        public double Demand { get; set; }
        public double Receipt { get; set; }
        public double PiCalc { get; set; }
        public double PiReceived { get; set; }
    }

    public class EcsDemandRdRow
    {
        public decimal TdId { get; set; }
        public string? TdNo { get; set; }
        public decimal MemId { get; set; }
        public int SchemeId { get; set; }
        public DateTime ValueDate { get; set; }
        public int PeriodInMonths { get; set; }
        public double Instalment { get; set; }
        public double Received { get; set; }
        public double Paid { get; set; }
        public double PenalCalc { get; set; }
        public double PenalReceived { get; set; }
        public double PenalRate { get; set; }
    }

    public class EcsDemandDueToRow
    {
        public decimal MemId { get; set; }
        public decimal LedId { get; set; }
        public string? LedName { get; set; }
        public double Payments { get; set; }
        public double Receipts { get; set; }
    }

    public class EcsDemandTotalRow
    {
        public decimal DemandId { get; set; }
        public string? DemandType { get; set; }
        public int Lines { get; set; }
        public double Total { get; set; }
    }
}
