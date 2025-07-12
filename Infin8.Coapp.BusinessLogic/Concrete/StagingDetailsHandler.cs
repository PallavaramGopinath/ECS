using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class StagingDetailsHandler : IStagingDetailsHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public StagingDetailsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddStagingDetails(Staging_Details stagingDetails)
        {
            string accountType = "";
            decimal stagingId = 0;
            bool isStagingCreated =  _unitOfWork.StagingMaster.IsStagingMasterCreated(stagingDetails.Created_By, stagingDetails.Member_Id, "I", stagingDetails.Created_Date);
            if (isStagingCreated)
            {
                stagingId = await _unitOfWork.StagingMaster.GetStagingMasterId (stagingDetails.Created_By,stagingDetails.Member_Id,"I",stagingDetails.Created_Date);
            }
            else
            {
                accountType = await _unitOfWork.StagingDetails.GetAccountType(stagingDetails.Created_By, stagingDetails.Member_Id, "I", stagingDetails.Created_Date, stagingDetails.BrCode!);
                Staging_Master master = new()
                {
                    Staging_Id = 0,
                    Session_Id = "",
                    Member_Id = stagingDetails.Member_Id,
                    Created_By = stagingDetails.Created_By,
                    Created_Date = stagingDetails.Created_Date,
                    Checked_By = 0,
                    Checked_Date = null,
                    Staging_Status = "I",
                    BrCode = stagingDetails.BrCode,
                    Type = accountType 
                };
                stagingId = await _unitOfWork.StagingMaster.AddStagingMaster(master);
            }
            return await _unitOfWork.StagingDetails.AddStagingDetails(stagingDetails, stagingId);   
        }

        public async Task<List<AccountTransactionVM>> GetAccountTransactions( DateTime createdDate, decimal memId, string stagingStatus, string brCode)
        {
            return await _unitOfWork.StagingDetails.GetAccountTransactions( createdDate,memId, stagingStatus, brCode);
        }

        public async Task<List<DtoCheckerDashboard>> GetCheckerDashboard(DateTime createdDate,  string stagingStatus, string brCode)
        {
            return await _unitOfWork.StagingDetails.GetCheckerDashboard(createdDate, stagingStatus, brCode);
        }

        public async Task<string> GetAccountType(decimal createdBy, decimal memId, string stagingStatus, DateTime createdDate, string brCode)
        {
            return await GetAccountType(createdBy, memId, stagingStatus, createdDate, brCode);
        }

        public Task<List<Staging_Details>> GetAllStagingDetails(decimal createdBy, decimal memId, string stagingStatus, DateTime createdDate)
        {
            throw new NotImplementedException();
        }

    }
}
