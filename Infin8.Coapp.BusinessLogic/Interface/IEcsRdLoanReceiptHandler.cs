using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>ECS Recurring Deposit Loan Receipt, acc 25 (spec 18).</summary>
    public interface IEcsRdLoanReceiptHandler
    {
        /// <summary>
        /// §4.1: the member's open loans on RD with principal outstanding and interest due as on the token's business day. Sets
        /// SetupError instead when the business day changed since login.
        /// </summary>
        Task<EcsRdLoanReceiptSetupVM> GetLoansAsync(decimal memId, string brCode, DateTime currentDate);

        /// <summary>The same, without the business-day check, excluding <paramref name="excludeStagingId"/> from the locks.</summary>
        Task<EcsRdLoanReceiptSetupVM> GetPreviewAsync(decimal memId, string brCode, DateTime businessDay, decimal excludeStagingId = 0);

        /// <summary>
        /// Approval (§9, §11): recomputes every staged figure and builds one loan_trn 'R' row and the voucher lines per loan.
        /// Never saves; the caller closes the loans in <see cref="EcsRdLoanReceiptPosting.ClosedLoanIds"/>. Throws
        /// <see cref="EcsRecoveryException"/> with a message meant for the Checker.
        /// </summary>
        Task<EcsRdLoanReceiptPosting> BuildPostingAsync(DtoEcsRdLoanReceipt staged, decimal stagingId, DateTime stagingDate,
            string brCode, decimal yrId, decimal usrId, decimal vocId, int trnType, string status);
    }

    public class EcsRdLoanReceiptPosting
    {
        public List<Loan_Trn> LoanTrns { get; } = new();             // Trn_Id / Trn_SlNo assigned by AddLoanTrnListAsync
        public List<Fin_Voucher_Trn> VoucherTrns { get; } = new();
        public List<decimal> ClosedLoanIds { get; } = new();
    }
}
