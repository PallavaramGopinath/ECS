using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class LockerRentAdjustmentsHandler : ILockerRentAdjustmentsHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITermDepositTrnHandler _termDepositTrnHandler;
        //private readonly AuthenticationStateProvider _authStateProvider;
        public LockerRentAdjustmentsHandler(IUnitOfWork unitOfWork, ITermDepositTrnHandler termDepositTrnHandler
           )
        {
            _unitOfWork = unitOfWork;
            _termDepositTrnHandler = termDepositTrnHandler;
            //_authStateProvider = authStateProvider;
        }

        public async Task<bool> AddLockerRentAdjustment(Locker_Rent_Adjustments lockerRentAdjustment)
        {
            return await _unitOfWork.LockerRentAdjustments.AddLockerRentAdjustment(lockerRentAdjustment);
        }

        public async Task<List<Locker_Rent_Adjustments>> EditLockerRentAdjustment(Locker_Rent_Adjustments lockerRentAdjustment)
        {
            return await _unitOfWork.LockerRentAdjustments.EditLockerRentAdjustment(lockerRentAdjustment);
        }

        public async Task<List<LockerClosureBalanceDto>> GetLockerClosureBalanceListAsync(decimal customerId, DateTime currentDate, string brCode)
        {
            List<LockerClosureBalanceDto> balanceList = new();
            try
            {
                //var userInfo = await Utilities.GetUserInfoDtoFromASP(_authStateProvider);
                balanceList = await _unitOfWork.LockerRentAdjustments.GetLockerClosureBalanceListAsync(customerId, brCode);
                foreach (var bal in balanceList)
                {
                    List<FDDetailsVM> fdDetails = new();
                    
                    decimal[] fdId = { bal.DepositId };
                    fdDetails = await _termDepositTrnHandler.GetFDPayableByTDIdsAsync(fdId, currentDate , 7, brCode);
                    var fd = fdDetails.FirstOrDefault();
                    if (fd != null)
                    {
                        bal.InterestPreviousBalance = fd.FDIntAlreadyCalculated;
                        bal.InterestCalculated = fd.FDIntCalculatedNow;
                        bal.InterestCalculatedDate = fd.FDIntCalculatedDateNow;
                    }
                    bal.LockerClosureDate = currentDate;
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
