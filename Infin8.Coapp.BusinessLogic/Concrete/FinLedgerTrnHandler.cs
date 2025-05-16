using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class FinLedgerTrnHandler : IFinLedgerTrnHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public FinLedgerTrnHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddFinLedgerTrnAsync(Fin_Ledger_Trn finLedgerTrn)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.FinLedgerTrn.AddFinLedgerTrnAsync(finLedgerTrn);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan daily market rate not saved");
            }
            return result;
        }

        public async Task<bool> EditFinLedgerTrnAsync(Fin_Ledger_Trn finLedgerTrn)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.FinLedgerTrn.EditFinLedgerTrnAsync(finLedgerTrn);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Ledger subgroup name not deleted");
            }
            return result;
        }
    }
}
