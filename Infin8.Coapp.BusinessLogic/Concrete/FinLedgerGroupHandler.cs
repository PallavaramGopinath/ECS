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
    public class FinLedgerGroupHandler : IFinLedgerGroupHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public FinLedgerGroupHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddFinLedgerGroupAsync(Fin_Ledger_Grp finLedgerGroup)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.FinLedgerGroup.AddFinLedgerGroupAsync(finLedgerGroup);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Ledger group  not saved");
            }
            return result;
        }

        public async  Task<bool> EditFinLedgerGroupAsync(Fin_Ledger_Grp finLedgerGroup)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.FinLedgerGroup.EditFinLedgerGroupAsync(finLedgerGroup);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan daily market rate not deleted");
            }
            return result;
        }

        public async Task<List<Fin_Ledger_Grp>> GetFinLedgerGroupListAsync(int fnlId, string brCode)
        {
            return await _unitOfWork.FinLedgerGroup.GetFinLedgerGroupListAsync(fnlId,brCode);
        }

        public async Task<List<Fin_Ledger_Grp>> GetFinLedgerGroupListAsync(string brCode)
        {
            return await _unitOfWork.FinLedgerGroup.GetFinLedgerGroupListAsync(brCode);
        }

        public async Task<List<FinBal>> GetGroupLedgerBalance(decimal yrId, string brCode)
        {
            return await _unitOfWork.FinLedgerGroup.GetGroupLedgerBalance(yrId,brCode);
        }
    }
}
