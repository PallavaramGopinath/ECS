using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IStagingDetailsHandler
    {
        Task<bool> AddStagingDetails(DtoStaging_Details stagingDetails);
        Task<bool> AddStagingForAccountTransaction(List<Staging_Details> stagingDetails);
        Task<bool> DeleteStagingDetailsByStagingId(decimal stagingId, int relateAccountId, decimal ledgerId, string brCode);
        Task<bool> DeleteStagingDetails(List<Staging_Details> detailsList);
        Task<int> VerifyStagingIdExistinsInStagingDetails(decimal stagingId);
        Task<List<Staging_Details>> GetAllStagingDetails(decimal createdBy, decimal memId, string stagingStatus, DateTime createdDate);
        Task<List<Staging_Details>> GetStagingDetailsByDate(DateTime stagingDate, string brCode);

        //Task<string> GetAccountType(decimal createdBy, decimal memId, string stagingStatus, DateTime createdDate, string brCode);
        Task<string> GetAccountType(int accId);
        Task<List<AccountTransactionVM>> GetAccountTransactions( DateTime createdDate,decimal memId, string stagingStatus, string brCode);
        Task<List<AccountTransactionVM>> GetChekerDashboardById(decimal stagingId);
        Task<List<DtoCheckerDashboard>> GetCheckerDashboard(DateTime createdDate,  string stagingStatus, string brCode);
        Task<Staging_Details> GetStagingDetailsById(decimal stagingId, int relatedAccountId);
        Task<List<DtoAccountTransactionRelatedData>> GetStagingDetailsListById(decimal stagingId);
        Task<int> IsAlreadyTransactedButNotVerifiedOrRejected(decimal memId, string transactedDate, int relatedAccountId,decimal ledgerId);
        Task<bool> MakeStagingDetails(decimal stagingId);
        Task<bool> CheckerStateStaging(decimal stagingId, decimal vocId, decimal checkerBy, string stagingStatus);
        Task<bool> VerifyForFixedDepositLoanRecovery(int accountId, decimal memId, DateTime createdDate);
    }
}
