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
        Task<bool> AddStagingDetails(Staging_Details stagingDetails);
        Task<List<Staging_Details>> GetAllStagingDetails(decimal createdBy, decimal memId, string stagingStatus, DateTime createdDate);
        Task<string> GetAccountType(decimal createdBy, decimal memId, string stagingStatus, DateTime createdDate, string brCode);
        Task<List<AccountTransactionVM>> GetAccountTransactions( DateTime createdDate,decimal memId, string stagingStatus, string brCode);
        Task<List<DtoCheckerDashboard>> GetCheckerDashboard(DateTime createdDate,  string stagingStatus, string brCode);
    }
}
