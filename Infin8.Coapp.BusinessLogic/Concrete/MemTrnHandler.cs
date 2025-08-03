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
    public class MemTrnHandler : IMemTrnHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public MemTrnHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddMemTrnAsync(Mem_Trn memTrn)
        {
            return await _unitOfWork.MemTrn.AddMemTrnAsync(memTrn);
        }
        public async Task<DtoSBAccountBalanceWithIds> GetSBAccountBalanceWithIds(decimal memId, string brCode)
        {
            return await _unitOfWork.MemTrn.GetSBAccountBalanceWithIds(memId,brCode);
        }
        public async Task<bool> EditMemTrnAsync(Mem_Trn memTrn, string brCode)
        {
            return await _unitOfWork.MemTrn.EditMemTrnAsync(memTrn, brCode);    
        }

        public async Task<double> GetMemTrnByMemIdAndLedIdAsync(decimal memId, decimal ledId, string brCode)
        {
            return await _unitOfWork.MemTrn.GetMemTrnByMemIdAndLedIdAsync(memId, ledId,brCode );
        }

        public async Task<double> GetSBAccountBalanceByAccId(decimal accId, string brCode)
        {
            return await _unitOfWork.MemTrn.GetSBAccountBalanceByAccId(accId,brCode );
        }

        public async Task<double> GetSBAccountInterestBalanceByAccId(decimal accId, string brCode)
        {
            return await _unitOfWork.MemTrn.GetSBAccountInterestBalanceByAccId(accId, brCode);
        }

        public async Task<double> GetmemTrnTotalSuspenseAmount(decimal memId, int trnType, string brCode)
        {
            return await _unitOfWork.MemTrn.GetmemTrnTotalSuspenseAmount(memId, trnType, brCode);
        }

        
    }
}
