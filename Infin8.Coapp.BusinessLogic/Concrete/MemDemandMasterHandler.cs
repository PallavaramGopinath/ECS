using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class MemDemandMasterHandler : IMemDemandMasterHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public MemDemandMasterHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddMemDemandMasterAsync(Mem_Demand_Master memDemandMaster)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.MemDemandMaster.AddMemDemandMasterAsync(memDemandMaster);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Demand master not saved");
            }
            return result;
        }

        public async Task<bool> EditMemDemandMasterAsync(Mem_Demand_Master memDemandMaster)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.MemDemandMaster.EditMemDemandMasterAsync(memDemandMaster);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Demand Master not deleted");
            }
            return result;
        }
    }
}
