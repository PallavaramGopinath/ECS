using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>ECS Loan on Recurring Deposit, acc 14 (spec 17).</summary>
    public interface IEcsRdLoanHandler
    {
        /// <summary>
        /// §4.1: scheme, eligibility %, ROI margin and the member's RDs with received amount and block reason, as on the token's
        /// business day. Sets SetupError instead when the business day changed or the setup is incomplete.
        /// </summary>
        Task<EcsRdLoanSetupVM> GetSetupAsync(decimal memId, string brCode, DateTime currentDate);

        /// <summary>The same, without the business-day check, excluding <paramref name="excludeStagingId"/> from the locks.</summary>
        Task<EcsRdLoanSetupVM> GetPreviewAsync(decimal memId, string brCode, DateTime businessDay, decimal excludeStagingId = 0);

        /// <summary>The TD_Ids pledged in pending (I/M) acc 14 lines of the branch, except <paramref name="excludeStagingId"/>.</summary>
        Task<HashSet<decimal>> GetPendingLoanTdIdsAsync(string brCode, decimal excludeStagingId = 0);

        /// <summary>
        /// Approval (§9, §11): rechecks every staged figure, allocates the loan no. and ids, and builds the loan, lien and
        /// voucher rows. Never saves. Throws <see cref="EcsRecoveryException"/> with a message meant for the Checker.
        /// </summary>
        Task<EcsRdLoanPosting> BuildPostingAsync(DtoEcsRdLoan staged, decimal stagingId, DateTime stagingDate,
            string brCode, decimal yrId, decimal usrId, decimal vocId, int trnType, string status);
    }

    public class EcsRdLoanPosting
    {
        public Loan_Master LoanMaster { get; set; } = new();
        public Loan_Trn LoanTrn { get; set; } = new();       // Trn_Id / Trn_SlNo assigned by AddLoanTrnListAsync
        public Loan_Disb LoanDisb { get; set; } = new();
        public Loan_Roi LoanRoi { get; set; } = new();
        public Lien Lien { get; set; } = new();
        public List<Lien_Trn> LienTrns { get; } = new();
        public List<Fin_Voucher_Trn> VoucherTrns { get; } = new();
        public List<decimal> LienTdIds { get; } = new();
    }
}
