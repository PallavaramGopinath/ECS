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
    public class TermDepositMasterHandler : ITermDepositMasterHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public TermDepositMasterHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddTermDepositMasterAsync(TermDeposit_Master termDepositMaster)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.TermDepositMaster.AddTermDepositMasterAsync(termDepositMaster);  
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Term deposit master not saved");
            }
            return result;
        }

        public async Task<bool> EditTermDepositMasterAsync(TermDeposit_Master termDepositMaster)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.TermDepositMaster.EditTermDepositMasterAsync(termDepositMaster);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Term deposit master not deleted");
            }
            return result;
        }

        public async Task<FDRenewalObject> GetFDRnewalObjectByMemIdAsync(decimal memId)
        {
            return await _unitOfWork.TermDepositMaster.GetFDRnewalObjectByMemIdAsync(memId);
        }

        public async Task<DateTime> GetMaxMaturityDate(decimal[] tdIds, DateTime toDate)
        {
            return await _unitOfWork.TermDepositMaster.GetMaxMaturityDate(tdIds, toDate);
        }

        public async Task<List<FDDataForLoan>> GetFDDetailsForLoan(decimal[] tdIds)
        {
            return await _unitOfWork.TermDepositMaster.GetFDDetailsForLoan(tdIds);
        }
    }
}
