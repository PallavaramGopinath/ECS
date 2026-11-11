using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
namespace Infin8.Coapp.Repository
{
    /// <summary>Data access for the ECS Loan Sanction page (spec 06 §7.1). Never calls SaveChanges.</summary>
    public interface IEcsLoanSanctionRepository
    {
        // ----- lookups -----
        Task<List<DropdownItem>> GetSchemeItemsAsync(IEnumerable<int> loanTypes, string brCode);
        Task<SuretyLoanSanctionDataVM> GetMemberAsync(string memberNo, DateTime asOn, string brCode);
        Task<SuretyLoanSanctionDataVM> GetMemberByIdAsync(decimal memId, DateTime asOn, string brCode);
        Task<EcsPayDetailsVM> GetPayDetailsAsync(decimal memId, string brCode);
        Task<Loan_Schemes?> GetSchemeAsync(int schemeId, string brCode);
        /// <summary>Latest ROI / PI from Loan_Roi_Template (agency "S", Wef ≤ asOn). Null when none.</summary>
        Task<(double roi, double pi)?> GetRoiAsync(int schemeId, DateTime asOn, string brCode);
        Task<List<int>> GetDeductableSchemeIdsAsync(int schemeId, string brCode);

        // ----- checks -----
        Task<bool> ExistsSanctionOnDateAsync(decimal memId, int schemeId, DateTime date, string[] statuses,
            decimal excludeSanctionId, string brCode);
        /// <summary>MAX(loan_master.san_date) for member + scheme (§3.5).</summary>
        Task<DateTime?> GetLastLoanSanctionDateAsync(decimal memId, int schemeId, string brCode);
        Task<List<string>> GetSubsequentTransactionMessagesAsync(decimal sanctionId, string brCode);

        // ----- recoverable sources -----
        Task<List<EcsLoanBalanceVM>> GetLoanBalancesAsync(decimal memId, string brCode);
        Task<Dictionary<decimal, string>> GetLedgerNamesAsync(IEnumerable<decimal> ledIds, string brCode);
        Task<double> GetLedgerBalanceAsync(decimal memId, decimal ledId, string brCode);
        Task<double> GetDepositBalanceAsync(decimal memId, int dmId, string brCode);
        Task<List<DeductableLoanRowVM>> GetDueToRowsAsync(decimal memId, string brCode);
        Task<List<PendingDepositRowVM>> GetPendingDepositsAsync(decimal memId, DateTime asOn, string brCode);
        Task<double> GetCurrentDepositDemandAsync(decimal memId, double basicPay, DateTime calcDate, DateTime demandDate, string brCode);
        /// <summary>Status for a manual row (§3.8): (status, dmId). Status ∈ D, DTo, DBy, MS, G.</summary>
        Task<(string status, int dmId, string? ledName)> ResolveDeductionStatusAsync(decimal ledId, string brCode);

        // ----- list / load / delete / report -----
        Task<List<EcsSanctionListItemVM>> GetSanctionListAsync(EcsSanctionListFilterVM filter, string brCode);
        Task<Loan_Sanction?> GetSanctionHeaderAsync(decimal sanctionId, string brCode);
        Task<List<Loan_Sanction_Trn>> GetSanctionLinesAsync(decimal sanctionId, string brCode);
        Task<Dictionary<decimal, string?>> GetLoanNumbersAsync(IEnumerable<decimal> loanIds, string brCode);
        /// <summary>Marks header + lines + eligibility deleted. No SaveChanges.</summary>
        Task SoftDeleteSanctionAsync(decimal sanctionId, string brCode);
        Task<List<rptOfficeNote>> GetOfficeNoteAsync(decimal sanctionId, string brCode);

        /// <summary>Assigns MAX+1 ids, back-links and adds all three. No SaveChanges — the handler owns the transaction.</summary>
        Task<(decimal sanctionId, int eligId)> AddSanctionAsync(Loan_Eligibility eligibility, Loan_Sanction sanction,
            List<Loan_Sanction_Trn> trnList);
    }
}
