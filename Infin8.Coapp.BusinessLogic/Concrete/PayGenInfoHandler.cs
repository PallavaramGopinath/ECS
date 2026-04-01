using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class PayGenInfoHandler : IPayGenInfoHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public PayGenInfoHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<Pay_Gen_Info>> AddPayGenInfoAsync(Pay_Gen_Info payGenInfo)
        {
            List<Pay_Gen_Info> genInfoList = new();
            try
            {
                var result = await _unitOfWork.PayGenInfo.AddPayGenInfoAsync(payGenInfo);
                await _unitOfWork.CompleteAsync();
                if(result != null && result.Count > 0)
                {
                    genInfoList = result;
                }
            }
            catch (Exception ex)
            {
                genInfoList = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! Payroll Info data not saved");
            }
            return genInfoList;
        }

        public async Task<List<Pay_Gen_Info>> EditPayGenInfoAsync(Pay_Gen_Info payGenInfo)
        {
            List<Pay_Gen_Info> genInfoList = new();
            try
            {
                var result = await _unitOfWork.PayGenInfo.EditPayGenInfoAsync(payGenInfo);
                await _unitOfWork.CompleteAsync();
                if (result != null && result.Count > 0)
                {
                    genInfoList = result;
                }
            }
            catch (Exception ex)
            {
                genInfoList = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! Payroll Info data not deleted");
            }
            return genInfoList;
        }

        public async Task<List<Pay_Gen_Info>> GetPayGenInfoListAsync(string brCode)
        {
            return await _unitOfWork.PayGenInfo.GetPayGenInfoListAsync(brCode);
        }
    }
}
