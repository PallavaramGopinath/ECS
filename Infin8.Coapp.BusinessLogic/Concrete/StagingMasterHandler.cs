using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class StagingMasterHandler : IStagingMasterHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public StagingMasterHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<decimal> AddStagingMaster(Staging_Master stagingMaster)
        {
            decimal stagingId = 0;
            try
            {
                stagingId = 
                stagingId = await _unitOfWork.StagingMaster.AddStagingMaster(stagingMaster);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while adding new Staging Master");
            }
            return stagingId;
        }

        public async Task<decimal> GetStagingMasterId(decimal createdBy, decimal memId, string stagingStatus, DateTime createdDate)
        {
            return await _unitOfWork.StagingMaster.GetStagingMasterId(createdBy, memId, stagingStatus, createdDate);
        }

        public bool IsStagingMasterCreated(decimal createdBy, decimal memId, string stagingStatus, DateTime createdDate)
        {
            return _unitOfWork.StagingMaster.IsStagingMasterCreated(createdBy, memId, stagingStatus, createdDate);
        }
    }
}
