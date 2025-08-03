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
    public  class SBCAMasterHandler : ISBCAMasterHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public SBCAMasterHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool result, decimal accId, string accNo)> AddSBCAMasterAsync(SBCA_Master sbcaMaster)
        {
            return await _unitOfWork.SBCAMaster.AddSBCAMasterAsync(sbcaMaster);
        }

        public async Task<bool> EditSBCAMasterAsync(SBCA_Master sbcaMaster)
        {
            return await _unitOfWork.SBCAMaster.EditSBCAMasterAsync(sbcaMaster);
        }

        public async Task<List<DropdownItem>> GetSBCANosByMemIdAsync(decimal memId)
        {
            return await _unitOfWork.SBCAMaster.GetSBCANosByMemIdAsync(memId);
        }
    }
}
