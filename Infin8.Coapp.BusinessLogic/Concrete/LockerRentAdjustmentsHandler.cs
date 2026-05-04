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
    public class LockerRentAdjustmentsHandler : ILockerRentAdjustmentsHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LockerRentAdjustmentsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddLockerRentAdjustment(Locker_Rent_Adjustments lockerRentAdjustment)
        {
            return await _unitOfWork.LockerRentAdjustments.AddLockerRentAdjustment(lockerRentAdjustment);
        }

        public async Task<List<Locker_Rent_Adjustments>> EditLockerRentAdjustment(Locker_Rent_Adjustments lockerRentAdjustment)
        {
            return await _unitOfWork.LockerRentAdjustments.EditLockerRentAdjustment(lockerRentAdjustment);
        }

        public async Task<List<LockerClosureBalanceDto>> GetLockerClosureBalanceListAsync(decimal customerId, string brCode)
        {
            List<LockerClosureBalanceDto> balanceList = new();
            DateTime IntCalcDate = DateTime.UtcNow;
            DateTime ToDate = DateTime.UtcNow;
            int NoOfMonths = 0;
            double IntCalc = 0;
            double TotalIntCalc = 0;
            UtilityHandler utilityHandler = new UtilityHandler();
            try
            {
                balanceList = await _unitOfWork.LockerRentAdjustments.GetLockerClosureBalanceListAsync(customerId, brCode);
                foreach (var bal in balanceList)
                {
                    IntCalc = 0;
                    ToDate = bal.TransactionDate;
                    bal.InterestCalculated = IntCalc;
                    if (bal.InterestPreviousAppliedDate == null) IntCalcDate = bal.DepositDate;
                    else IntCalcDate = (DateTime)bal.InterestPreviousAppliedDate;
                    NoOfMonths = utilityHandler.GetMonthsBetweenDates(IntCalcDate, bal.TransactionDate);
                    for (int i = 1; i <= NoOfMonths; i++)
                    {
                        IntCalc = 0;
                        IntCalc = utilityHandler.CalculateInterestForFixedDeposit(bal.DepositRefundAmount, bal.InterestRate , 1, false);
                        TotalIntCalc += IntCalc;
                    }
                    IntCalcDate = utilityHandler.GetNextMonthForFD(bal.TransactionDate, IntCalcDate, NoOfMonths);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return balanceList;
        }

        public async Task<List<Locker_Rent_Adjustments>> GetLockerRentAdjustmentsListAsync(string brCode)
        {
            return await _unitOfWork.LockerRentAdjustments.GetLockerRentAdjustmentsListAsync(brCode);
        }

        public async Task<List<LockerRentReceiptAllotmentWiseDto>> GetLockerRentAllotmentWiseListAsync(decimal customerId, string brCode)
        {
            return await _unitOfWork.LockerRentAdjustments.GetLockerRentAllotmentWiseListAsync(customerId, brCode);
        }
    }
}
