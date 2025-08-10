using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IStagingMasterRepository
    {
        Task<decimal> AddStagingMaster(Staging_Master stagingMaster);
        bool IsStagingMasterCreated(decimal createdBy, decimal memId, string stagingStatus, DateTime createdDate);
        Task<bool> DeleteStagingMaster(decimal stagingId);
        Task<Decimal> GetStagingMasterId(decimal createdBy, decimal memId, string stagingStatus, DateTime createdDate);
        Task<bool> MakeStagingMaster(decimal stagingId);
        Task<bool> CheckerStateStaging(decimal stagingId,decimal vocId, decimal checkedBy, string stagingStatus);
    }
}
