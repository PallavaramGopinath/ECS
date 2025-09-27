using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
                accountType = await _unitOfWork.StagingDetails.GetAccountType(stagingDetails.Related_Account_Id); /// stagingDetails.Created_By, stagingDetails.Member_Id, "I", stagingDetails.Created_Date, stagingDetails.BrCode!);
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

        public async Task<bool> AddStagingForAccountTransaction(List<Staging_Details> stagingDetails)
        {
           
            //string accountType = "";
            decimal stagingId = 0;
            string status = stagingDetails[0].Staging_Status!.Trim();
            
            Staging_Master master = new()
                {
                    Staging_Id = 0,
                    Session_Id = "",
                    Member_Id = 0,
                    Created_By = stagingDetails[0].Created_By,
                    Created_Date = stagingDetails[0].Created_Date,
                    Checked_By = 0,
                    Checked_Date = null,
                    Staging_Status = status,
                    BrCode = stagingDetails[0].BrCode,
                    Type = stagingDetails[0].Module_Name,
                    Voc_Id = 0
                };
            stagingId = await _unitOfWork.StagingMaster.AddStagingMaster(master);
            
            return await _unitOfWork.StagingDetails.AddStagingDetailsForAccountTransaciton(stagingDetails, stagingId);
        }
        
        public async Task<bool> DeleteStagingDetailsByStagingId(decimal stagingId, int relateAccountId)
        {
            bool result = false;
            try
            {
                _unitOfWork.BeginTransaction();
                 result = await _unitOfWork.StagingDetails.DeleteStagingDetailsByStagingId(stagingId, relateAccountId);
                int count = await VerifyStagingIdExistinsInStagingDetails(stagingId);
                if (count == 0) 
                { 
                    result = await  _unitOfWork.StagingMaster.DeleteStagingMaster(stagingId,"11001");
                }
                _unitOfWork.Complete();
                _unitOfWork.CommitTransaction();
            }
            catch (Exception)
            {
                result = false;
                _unitOfWork.RollBack();
            }
            return result;
        }
        public async Task<bool> DeleteStagingDetails(List<Staging_Details> detailsList)
        {
            return await _unitOfWork.StagingDetails.DeleteStagingDetails(detailsList);
        }
        public async Task<int> VerifyStagingIdExistinsInStagingDetails(decimal stagingId)
        {
            return await _unitOfWork.StagingDetails.VerifyStagingIdExistinsInStagingDetails(stagingId);
        }
        public async Task<List<AccountTransactionVM>> GetAccountTransactions( DateTime createdDate, decimal memId, string stagingStatus, string brCode)
        {
            return await _unitOfWork.StagingDetails.GetAccountTransactions( createdDate,memId, stagingStatus, brCode);
        }
        public async Task<List<AccountTransactionVM>> GetChekerDashboardById(decimal stagingId)
        {
            return await _unitOfWork.StagingDetails.GetChekerDashboardById(stagingId);
        }
        public async Task<List<DtoCheckerDashboard>> GetCheckerDashboard(DateTime createdDate,  string stagingStatus, string brCode)
        {
            return await _unitOfWork.StagingDetails.GetCheckerDashboard(createdDate, stagingStatus, brCode);
        }
        public async Task<string> GetAccountType(int accId)
        {
            return await _unitOfWork.StagingDetails.GetAccountType(accId);
        }
        //public async Task<string> GetAccountType(decimal createdBy, decimal memId, string stagingStatus, DateTime createdDate, string brCode)
        //{
        //    return await _unitOfWork.StagingDetails.GetAccountType(createdBy, memId, stagingStatus, createdDate, brCode);
        //}

        public async Task<Staging_Details> GetStagingDetailsById(decimal stagingId, int relatedAccountId)
        {
            return await _unitOfWork.StagingDetails.GetStagingDetailsById(stagingId,relatedAccountId);
        }

        public async Task<List<DtoAccountTransactionRelatedData>> GetStagingDetailsListById(decimal stagingId)
        {
            List<DtoAccountTransactionRelatedData> accRelatedDataList = new();
            DtoAccountTransactionRelatedData accRelatedData = new();
            var result =  await _unitOfWork.StagingDetails.GetStagingDetailsListById(stagingId);
            if (result != null && result.Any()) accRelatedDataList = result.ToList();
            return accRelatedDataList;
        }
        public Task<List<Staging_Details>> GetAllStagingDetails(decimal createdBy, decimal memId, string stagingStatus, DateTime createdDate)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Staging_Details>> GetStagingDetailsByDate(DateTime stagingDate, string brCode)
        {
            return await _unitOfWork.StagingDetails.GetStagingDetailsByDate(stagingDate, brCode);
        }
        public async Task<int> IsAlreadyTransactedButNotVerifiedOrRejected(decimal memId, string transactedDate, int relatedAccountId)
        {
            return await _unitOfWork.StagingDetails.IsAlreadyTransactedButNotVerifiedOrRejected (memId, transactedDate, relatedAccountId);
        }
        public async Task<bool> MakeStagingDetails(decimal stagingId)
        {
            bool result = false;
            try
            {
                _unitOfWork.BeginTransaction();
                var resultDetails = await _unitOfWork.StagingDetails.MakeStagingDetails(stagingId);
                var resultMaster = await _unitOfWork.StagingMaster.MakeStagingMaster(stagingId); 

                if(resultDetails  && resultMaster)
                {
                    _unitOfWork.Complete();
                    result = true;
                }
                else
                {
                    _unitOfWork.RollBack();
                    result = false;
                }
                _unitOfWork.CommitTransaction();
            }
            catch (Exception)
            {
                _unitOfWork.RollBack();
                result =false;
            }
            return result;
        }

        public async Task<bool> CheckerStateStaging(decimal stagingId, decimal vocId, decimal checkerBy, string stagingStatus)
        {
            bool result = false;
            try
            {
                _unitOfWork.BeginTransaction();
                var resultDetails = await _unitOfWork.StagingMaster.CheckerStateStaging(stagingId, vocId,checkerBy,stagingStatus );
                var resultMaster = await _unitOfWork.StagingDetails.CheckerStateStaging(stagingId,vocId, checkerBy,stagingStatus );

                if (resultDetails && resultMaster)
                {
                    _unitOfWork.Complete();
                    result = true;
                }
                else
                {
                    _unitOfWork.RollBack();
                    result = false;
                }
                _unitOfWork.CommitTransaction();
            }
            catch (Exception)
            {
                _unitOfWork.RollBack();
                result = false;
            }
            return result;
        }

        public async Task<bool> VerifyForFixedDepositLoanRecovery(int accountId, decimal memId, DateTime createdDate)
        {
            return await _unitOfWork.StagingDetails.VerifyForFixedDepositLoanRecovery(accountId, memId,createdDate);
        }

        
    }
}
