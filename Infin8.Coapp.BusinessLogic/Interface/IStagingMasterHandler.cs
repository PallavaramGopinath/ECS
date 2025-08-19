using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  interface IStagingMasterHandler
    {
        Task<decimal> AddStagingMaster(Staging_Master stagingMaster);
        bool IsStagingMasterCreated(decimal createdBy, decimal memId, string stagingStatus, DateTime createdDate);
        Task<Decimal> GetStagingMasterId(decimal createdBy, decimal memId, string stagingStatus, DateTime createdDate);
        Task<Staging_Master> GetStagingMasterById(decimal stagingId);
    }
}
