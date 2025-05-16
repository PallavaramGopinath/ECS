using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class SBCASchemesHandler : ISBCASchemesHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public SBCASchemesHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddSBCASchemesAsync(SBCA_Schemes sbcaSchemes, string brCode)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.SBCASchemes.AddSBCASchemesAsync(sbcaSchemes,brCode);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while adding new SB account scheme");
            }
            return result;
        }

        public async Task<bool> EditSBCASchemesAsync(SBCA_Schemes sbcaSchemes, string brCode)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.SBCASchemes.EditSBCASchemesAsync(sbcaSchemes,brCode);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred whiel modifying SB account scheme");
            }
            return result;
        }

        public async Task<(decimal prlLedId, decimal intledId)> GetSBAccountLedgerIds(string brCode)
        {
            return await _unitOfWork.SBCASchemes.GetSBAccountLedgerIds(brCode);
        }
    }
}
