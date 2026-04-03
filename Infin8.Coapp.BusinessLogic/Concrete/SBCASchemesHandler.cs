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

        public async Task<List<SBCA_Schemes>> AddSBCASchemesAsync(SBCA_Schemes sbcaSchemes, string brCode)
        {
            List<SBCA_Schemes> schemes = new();
            try
            {
               var result = await _unitOfWork.SBCASchemes.AddSBCASchemesAsync(sbcaSchemes,brCode);
                await _unitOfWork.CompleteAsync();
               if(result != null && result.Count >0)
                {
                    schemes = result.ToList ();
                }
            }
            catch (Exception ex)
            {
                schemes = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while adding new SB account scheme");
            }
            return schemes;
        }

        public async Task<List<SBCA_Schemes>> EditSBCASchemesAsync(SBCA_Schemes sbcaSchemes, string brCode)
        {
            List<SBCA_Schemes> schemes = new();
            try
            {
                var result = await _unitOfWork.SBCASchemes.EditSBCASchemesAsync(sbcaSchemes,brCode);
                await _unitOfWork.CompleteAsync();
                if (result != null && result.Count > 0)
                {
                    schemes = result.ToList();
                }
            }
            catch (Exception ex)
            {
                schemes = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred whiel modifying SB account scheme");
            }
            return schemes;
        }

        public async Task<(decimal prlLedId, decimal intledId)> GetSBAccountLedgerIds(string brCode)
        {
            return await _unitOfWork.SBCASchemes.GetSBAccountLedgerIds(brCode);
        }

        public Task<SBCA_Schemes> GetSBCAScheme(string brCode)
        {
            return _unitOfWork.SBCASchemes.GetSBCAScheme(brCode);
        }

        public async Task<List<SBCA_Schemes>> GetSBCASchemes(string brCode)
        {
            return await _unitOfWork.SBCASchemes.GetSBCASchemes(brCode);
        }
    }
}
