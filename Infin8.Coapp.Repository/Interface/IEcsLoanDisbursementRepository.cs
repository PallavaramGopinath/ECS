using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.Repository
{
    /// <summary>Queries and writes for the ECS Loan Disbursement page (spec 10). Never calls SaveChanges.</summary>
    public interface IEcsLoanDisbursementRepository
    {
        Task<List<EcsBankLedgerVM>> GetBankLedgersAsync(string brCode);
        Task<DateTime?> GetOpenBusinessDayAsync(string brCode);
        Task<List<Loan_Sanction>> GetSanctionsAsync(IEnumerable<decimal> sanctionIds, string brCode);
        /// <summary>Recoverable lines (receiptitem &gt; 0, not deleted) of the given sanctions.</summary>
        Task<List<Loan_Sanction_Trn>> GetRecoverableLinesAsync(IEnumerable<decimal> sanctionIds, string brCode);
        Task<Dictionary<int, Loan_Schemes>> GetSchemesAsync(IEnumerable<int> schemeIds, string brCode);
        Task<Dictionary<decimal, (string? MemberNo, string? PerNo, string? Name)>> GetMembersAsync(IEnumerable<decimal> memIds, string brCode);
        Task<Dictionary<decimal, (string? Name, int SundryType)>> GetLedgersAsync(IEnumerable<decimal> ledIds, string brCode);
        Task<Dictionary<decimal, string?>> GetLoanNumbersAsync(IEnumerable<decimal> loanIds, string brCode);
        /// <summary>Principal outstanding (san_amt − Σ prlcoll_amt) per loan, before this disbursement.</summary>
        Task<Dictionary<decimal, double>> GetPrincipalOutstandingAsync(IEnumerable<decimal> loanIds, string brCode);
        Task<Dictionary<int, decimal>> GetDepositLedgersAsync(string brCode);
        Task<decimal> GetShareCapitalLedgerAsync(string brCode);
        Task<bool> IsChequeNoUsedAsync(decimal bankLedId, string chequeNo, string brCode);
        /// <summary>Next adjustment receipt / payment serials for the year and branch.</summary>
        Task<(int rptSlNo, int pmtSlNo)> GetNextVoucherSlNosAsync(decimal yrId, string brCode);
        /// <summary>demand_id of the member's oldest deposit demand not yet fully received (zero id when none).</summary>
        Task<decimal?> GetOldestUnpaidDepositDemandIdAsync(decimal memId, int depositId, string brCode);

        /// <summary>Reads every MAX id once, inside the open transaction (spec 10 §8.1).</summary>
        Task<EcsDisbursementIdAllocator> CreateIdAllocatorAsync(string brCode);

        /// <summary>Adds every row of the posting to the change tracker and applies the updates.</summary>
        Task AddPostingAsync(EcsDisbursementPosting posting, string brCode);
    }

    /// <summary>Everything one disbursement run writes, built in memory by the handler.</summary>
    public class EcsDisbursementPosting
    {
        public Fin_Voucher Voucher { get; set; } = new();
        public List<Loan_Master> Loans { get; } = new();
        public List<Loan_Disb> Disbursements { get; } = new();
        public List<Loan_Inst> Instalments { get; } = new();
        public List<Loan_Roi> Rois { get; } = new();
        public List<Loan_Trn> LoanTrns { get; } = new();
        public List<Mem_Trn> MemTrns { get; } = new();
        public List<Deposit_Trn> DepositTrns { get; } = new();
        public List<Fin_Voucher_Trn> VoucherTrns { get; } = new();
        public List<Fin_Voucher_Bank> BankRows { get; } = new();
        /// <summary>Recovered loans now repaid in full (spec 10 D3).</summary>
        public List<decimal> LoansToClose { get; } = new();
        /// <summary>Sanction id → (new loan id, loan amount, eligibility id).</summary>
        public List<(decimal SanctionId, decimal LoanId, double LoanAmount, decimal LoanEligId)> SanctionUpdates { get; } = new();
        public DateTime Date { get; set; }
        public decimal VocId => Voucher.Voc_Id;
    }
}
