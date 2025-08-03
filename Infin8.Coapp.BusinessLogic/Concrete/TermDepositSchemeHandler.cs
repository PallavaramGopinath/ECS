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
    public class TermDepositSchemeHandler : ITermDepositSchemeHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public TermDepositSchemeHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddTermDepositSchemeAsync(TermDeposit_Schemes termDepositScheme)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.TermDepositScheme.AddTermDepositSchemeAsync(termDepositScheme);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Term deposit scheme not saved");
            }
            return result;
        }

        public async Task<bool> EditTermDepositSchemeAsync(TermDeposit_Schemes termDepositScheme)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.TermDepositScheme.EditTermDepositSchemeAsync(termDepositScheme);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Term deposit scheme not deleted");
            }
            return result;
        }

        public async Task<TermDeposit_Schemes> GetTermDepositSchemeByIdAsync(int schemeId)
        {
            return await _unitOfWork.TermDepositScheme.GetTermDepositSchemeByIdAsync(schemeId);
        }

        public async Task<List<TermDeposit_Schemes>> GetTermDepositSchemeListAsync()
        {
            return await _unitOfWork.TermDepositScheme.GetTermDepositSchemeListAsync();
        }

        public async Task<List<DropdownItem>> GetTermDepositSchemeListBySchemeTypeArrayAsync(string[] schemeTypes, string brCode)
        {
            return await _unitOfWork.TermDepositScheme.GetTermDepositSchemeListBySchemeTypeArrayAsync(schemeTypes,brCode);
        }

        public async Task<List<DropdownItem>> GetTermDepositSchemeTypesAsync()
        {
            return await _unitOfWork.TermDepositScheme.GetTermDepositSchemeTypesAsync();
        }
    }
}
