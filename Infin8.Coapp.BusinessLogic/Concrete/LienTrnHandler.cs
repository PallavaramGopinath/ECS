using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class LienTrnHandler : ILienTrnHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LienTrnHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddLienTrnListRepositoryAsync(List<Lien_Trn> lientrnList)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LienTrn.AddLienTrnListRepositoryAsync(lientrnList);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Lien trn list not saved");
            }
            return result;
        }

        public async Task<bool> EditLienTrnListRepositoryAsync(List<Lien_Trn> lientrnList)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LienTrn.EditLienTrnListRepositoryAsync(lientrnList);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Lien trn list not deleted");
            }
            return result;
        }

        public async Task<double> GetTDAmountByLoanIdListAsync(decimal[] loanIdList)
        {
            return await _unitOfWork.LienTrn.GetTDAmountByLoanIdListAsync(loanIdList);
        }
    }
}
