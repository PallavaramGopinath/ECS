using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class FinYearMasterHandler : IFinYearMasterHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public FinYearMasterHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddFinYearMasterAsync(Fin_Yr_Master finYrMaster)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.FinYearMaster.AddFinYearMasterAsync(finYrMaster);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! New financial year not saved");
            }
            return result;
        }
    }
}
