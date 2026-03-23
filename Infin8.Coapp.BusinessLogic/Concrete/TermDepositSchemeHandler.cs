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

        public async Task<List<TermDeposit_Schemes>> AddTermDepositSchemeAsync(TermDeposit_Schemes termDepositScheme)
        {
            List<TermDeposit_Schemes> schemes = new();
            try
            {
                var result = await _unitOfWork.TermDepositScheme.AddTermDepositSchemeAsync(termDepositScheme);
                await _unitOfWork.CompleteAsync();
                if (result != null && result.Count > 0) schemes = result.ToList();
            }
            catch (Exception ex)
            {
                schemes = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! Term deposit scheme not saved");
            }
            return schemes;
        }

        public async Task<List<TermDeposit_Schemes>> EditTermDepositSchemeAsync(TermDeposit_Schemes termDepositScheme)
        {
            List<TermDeposit_Schemes> schemes = new();
            try
            {
                var result = await _unitOfWork.TermDepositScheme.EditTermDepositSchemeAsync(termDepositScheme);
                await _unitOfWork.CompleteAsync();
                if (result != null && result.Count > 0) schemes = result.ToList();
            }
            catch (Exception ex)
            {
                schemes = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! Term deposit scheme not deleted");
            }
            return schemes;
        }

        public async Task<TermDeposit_Schemes> GetTermDepositSchemeByIdAsync(int schemeId)
        {
            return await _unitOfWork.TermDepositScheme.GetTermDepositSchemeByIdAsync(schemeId);
        }

        public async Task<List<TermDeposit_Schemes>> GetTermDepositSchemeListAsync(string brCode)
        {
            return await _unitOfWork.TermDepositScheme.GetTermDepositSchemeListAsync(brCode);
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
