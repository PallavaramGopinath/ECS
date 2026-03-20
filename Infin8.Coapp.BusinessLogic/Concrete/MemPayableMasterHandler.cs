using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  class MemPayableMasterHandler : IMemPayableMasterHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public MemPayableMasterHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddMemPayableMasterAsync(Mem_Payable_Master memPayableMaster)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.MemPayableMaster.AddMemPayableMasterAsync(memPayableMaster);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Dividend calculation master not saved");
            }
            return result;
        }

        public async Task<bool> EditMemPayableMasterAsync(Mem_Payable_Master memPayableMaster)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.MemPayableMaster.EditMemPayableMasterAsync(memPayableMaster);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Dividend calculation master not deleted");
            }
            return result;
        }

        public async Task<Mem_Payable_Master> GetMemPayableMasterByPbleMasterId(decimal pbleMasterId, string brCode)
        {
            return await _unitOfWork.MemPayableMaster.GetMemPayableMasterByPbleMasterId(pbleMasterId, brCode);
        }

        public async Task<Mem_Payable_Master> GetDividendLastCalculatedData(int pbleType, string status, string brCode)
        {
            return await _unitOfWork.MemPayableMaster.GetDividendLastCalculatedData(pbleType , status, brCode);
        }

        public async Task<List<Mem_Payable_Master>> GetCalculatedDataList(int pbleType, string status, string brCode)
        {
            return await _unitOfWork.MemPayableMaster.GetCalculatedDataList(pbleType , status, brCode);
        }

       
    }
}
