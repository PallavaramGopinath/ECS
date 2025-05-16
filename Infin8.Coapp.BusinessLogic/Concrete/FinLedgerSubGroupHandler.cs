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
    public class FinLedgerSubGroupHandler : IFinLedgerSubGroupHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public FinLedgerSubGroupHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddFinLedgerSubGroupAsync(Fin_Ledger_SubGrp finLedgerSubGrp)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.FinLedgerSubGroup.AddFinLedgerSubGroupAsync(finLedgerSubGrp);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Ledger sub group not saved");
            }
            return result;
        }

        public async Task<bool> EditFinLedgerSubGroupAsync(Fin_Ledger_SubGrp finLedgerSubGrp)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.FinLedgerSubGroup.EditFinLedgerSubGroupAsync(finLedgerSubGrp);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An occuured Ledger sub group not deleted");
            }
            return result;
        }

        public async Task<List<DropdownItem>> GetFinLedgerGroupItemsAsync()
        {
            return await _unitOfWork.FinLedgerGroup.GetFinLedgerGroupItemsAsync();
        }
    }
}
