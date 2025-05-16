using Infin8.Coapp.Dto;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class ReportsTermDepositsHandler : IReportsTermDepositsHandler
    {
        readonly IUnitOfWork _unitOfWork;
        readonly IUtilityHandler _UtilityHandler;
        public ReportsTermDepositsHandler(IUnitOfWork unitOfWork, IUtilityHandler utility )
        {
            _unitOfWork = unitOfWork;
            _UtilityHandler = utility;
        }

        public async Task<rptFDBond> GetFDBondPreprinted(decimal vocId)
        {
            rptFDBond fDBond = new rptFDBond();

            try
            {
                var fDBondTmp = await _unitOfWork.ReportsTermDeposits.GetFDBondPreprinted(vocId);
                if (fDBondTmp != null) fDBond = fDBondTmp;
                fDBond.RsInWords = _UtilityHandler.RupeesInWords(fDBond.DepositAmount);
            }
            catch (Exception)
            {
                throw;
            }
            return fDBond;
        }

        public async Task<List<rptTDRefundBetweenDates>> GetFDRefundBetweenDated(DateTime fromDate, DateTime toDate, string TDSchemeType, string brCode)
        {
            return await _unitOfWork.ReportsTermDeposits.GetFDRefundBetweenDated(fromDate, toDate, TDSchemeType, brCode);
        }

        public async Task<List<rptTermDepositPayable>> GetTermDepositMaturityPayable(DateTime asOnDate, string tdSchemeType, string brCode)
        {
            return await _unitOfWork.ReportsTermDeposits.GetTermDepositMaturityPayable(asOnDate, tdSchemeType, brCode); 
        }

        public async Task<List<rptFDOutstanding>> GetTermDepositOutstanding(DateTime asOnDate, string TDSchemeType, string brCode)
        {
            return await _unitOfWork.ReportsTermDeposits.GetTermDepositOutstanding(asOnDate, TDSchemeType, brCode);
        }

        public async Task<List<rptFDOutstanding>> GetTermDepositOutstandingIndividual(decimal memId, DateTime asOnDate, string TDSchemeType)
        {
            return await _unitOfWork.ReportsTermDeposits.GetTermDepositOutstandingIndividual(memId, asOnDate, TDSchemeType);
        }

        public async Task<List<rptTermDepositPayable>> GetTermDepositPayable(DateTime asOnDate, string brCode)
        {
            return await _unitOfWork.ReportsTermDeposits.GetTermDepositPayable(asOnDate, brCode);
        }

        public async Task<List<rptTermDepositRegister>> GetTermDepositRegister(List<decimal> TDIdList, DateTime fromDate, DateTime toDate, string TDSchemeType)
        {
            return await _unitOfWork.ReportsTermDeposits.GetTermDepositRegister(TDIdList, fromDate, toDate, TDSchemeType);
        }

        public async Task<List<rptTDNewBetweenDates>> GetTermDepositReceivedDuringPeriod(DateTime fromDate, DateTime toDate, string TDSchemeType, string brCode)
        {
            return await _unitOfWork.ReportsTermDeposits.GetTermDepositReceivedDuringPeriod(fromDate, toDate, TDSchemeType, brCode);
        }

        public async Task<List<rptTDNewBetweenDates>> GetTermDepositrReceivedDuringPeriod(DateTime fromDate, DateTime toDate, string TDSchemeType, string brCode)
        {
            return await _unitOfWork.ReportsTermDeposits.GetTermDepositReceivedDuringPeriod(fromDate,toDate, TDSchemeType, brCode);
        }
    }
}
