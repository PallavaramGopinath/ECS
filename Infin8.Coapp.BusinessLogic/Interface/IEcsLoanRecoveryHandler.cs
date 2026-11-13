using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>ECS Loan Recovery, acc 58 "Loan Receipt" (spec 11).</summary>
    public interface IEcsLoanRecoveryHandler
    {
        /// <summary>
        /// The recovery grid for the Maker: refuses when the business day in the caller's token is not the
        /// branch's open business day (Day Begin / Day End since login), else <see cref="GetRecoveryPreviewAsync"/>.
        /// </summary>
        Task<EcsLoanRecoveryPreviewVM> GetRecoveryLoansAsync(decimal memId, EcsUserContextVM ctx);

        /// <summary>
        /// The member's ECS loans with dues as on <paramref name="applicationDate"/>, with interest / penal
        /// accrued up to the day before (spec 11 §4, §6, §7). Writes nothing.
        /// </summary>
        Task<EcsLoanRecoveryPreviewVM> GetRecoveryPreviewAsync(decimal memId, string brCode, DateTime applicationDate,
            decimal excludeStagingId = 0);

        /// <summary>
        /// Approval-time posting for one acc 58 staging line (spec 11 §9, §11). Recomputes and rechecks every
        /// staged figure, puts the loan_master updates in the change tracker and returns the loan_trn and
        /// fin_voucher_trn rows for the caller to insert. Throws <see cref="EcsRecoveryException"/> when the
        /// staged figures are stale. Never begins, saves or commits — it runs inside SaveTransaction's transaction.
        /// </summary>
        Task<EcsRecoveryPosting> BuildPostingAsync(DtoEcsLoanRecovery staged, decimal stagingId, DateTime stagingDate,
            string brCode, decimal yrId, decimal usrId, decimal vocId, int trnType, string narration);
    }

    public class EcsRecoveryPosting
    {
        public List<Loan_Trn> LoanTrns { get; } = new();
        public List<Fin_Voucher_Trn> VoucherTrns { get; } = new();
    }

    /// <summary>A recovery that cannot be approved as staged; the message is shown to the Checker.</summary>
    public class EcsRecoveryException : Exception
    {
        public EcsRecoveryException(string message) : base(message) { }
    }
}
