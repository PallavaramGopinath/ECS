using Infin8.Coapp.Models;
using Infin8.Coapp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IStagingDetailsRepository
    {
        Task<bool> AddStagingDetails (Staging_Details stagingDetails, decimal stagingId);
        Task<bool> AddStagingDetailsForAccountTransaciton(List<Staging_Details> stagingDetails, decimal stagingId);
        Task<bool> DeleteStagingDetailsByStagingId(decimal stagingId, int relateAccountId);
        Task<int> VerifyStagingIdExistinsInStagingDetails(decimal stagingId);
        Task<List<Staging_Details>> GetAllStagingDetails (decimal createdBy, decimal memId, string stagingStatus, DateTime createdDate);

        //Task<string> GetAccountType(decimal createdBy, decimal memId, string stagingStatus, DateTime createdDate,string brCode);
        Task<string> GetAccountType(int accId);
        Task<List<AccountTransactionVM>> GetAccountTransactions( DateTime createdDate, decimal memId, string stagingStatus, string brCode);
        Task<List<AccountTransactionVM>> GetChekerDashboardById(decimal stagingId);
        Task<List<DtoCheckerDashboard>> GetCheckerDashboard(DateTime createdDate,  string stagingStatus, string brCode);
        Task<Staging_Details> GetStagingDetailsById(decimal stagingId, int relatedAccountId);
        Task<List<DtoAccountTransactionRelatedData>> GetStagingDetailsListById(decimal stagingId);
        Task<int> IsAlreadyTransactedButNotVerifiedOrRejected(decimal memId, string transactedDate,int relatedAccountId);
        Task<bool> MakeStagingDetails(decimal stagingId);
        Task<bool> CheckerStateStaging(decimal stagingId, decimal vocId, decimal checkerBy, string stagingStatus);
        Task<bool> VerifyForFixedDepositLoanRecovery(int accountId, decimal memId);
    }
}
