using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.BusinessLogic
{
    public class ECSDemandCalculationHandler : IECSDemandCalculationHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public ECSDemandCalculationHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<Mem_Demand>> CalculateECSDemandAsync(DateTime demandCalculateDate, DateTime dueDate, int societyType,string brCode)
        {
            List<MemberDetailsVM> membersList = [];
            List<Mem_Demand> demandList = [];
            try
            {
                membersList = await _unitOfWork.Members.GetMembersForECSDemand(brCode);
                foreach(var mem in membersList )
                {
                    var tempLloanList = await _unitOfWork.LoanTrn.CalculateLoanDemand(mem.Mem_Id, (int)mem.MemberStatus!, demandCalculateDate, dueDate);
                    if(tempLloanList != null && tempLloanList.Count > 0)
                    {
                        demandList.AddRange(tempLloanList);
                    }

                    var tempDepositDemandList = await _unitOfWork.DepositTrn.CalculateDepositDemand(mem.Mem_Id, DateOnly.FromDateTime(demandCalculateDate), DateOnly.FromDateTime(dueDate), (Double)mem.BasicPay!, brCode);
                    if(tempDepositDemandList != null && tempDepositDemandList.Count > 0)
                    {
                        demandList.AddRange(tempDepositDemandList);
                    }

                    var tempRdDemandList = await _unitOfWork.RecurringDeposit.CalculateRecurringDepositDemand(mem.Mem_Id, DateOnly.FromDateTime(demandCalculateDate), dueDate, brCode);
                    if(tempDepositDemandList != null && tempDepositDemandList.Count > 0)
                    {
                        demandList.AddRange(tempRdDemandList);
                    }
                    var tempSundryDebtorDemandList = await _unitOfWork.MemTrn.CalculateDueToDemand(mem.Mem_Id,  dueDate, brCode);
                    if(tempSundryDebtorDemandList != null && tempSundryDebtorDemandList.Count > 0)
                    {
                        demandList.AddRange(tempSundryDebtorDemandList);
                    }
                }
                /// Save Demand List to DB
                /// 
            }
            catch (Exception)
            {
                throw;
            }
            return demandList;
;

        }
    }
}
