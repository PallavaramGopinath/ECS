using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  class LockerAllotmentsHandler :ILockerAllotmentsHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LockerAllotmentsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool result, decimal allotmentId)> AddLockerAllotment(LockerAllotmentVM  lockerAllotment)
        {
            bool result = false;
            decimal allotmentId = 0;
            try
            {
                Locker_Allotments newLocker = new()
                {
                    Id = 0,
                    Customer_Id = lockerAllotment.CustomerId,
                    Locker_Id = lockerAllotment.LockerId,
                    Allotment_Date = lockerAllotment.AllotmentDate,
                    Deposit_Amount = lockerAllotment.DepositAmount,
                    Interest_Rate = lockerAllotment.InterestRate,
                    Last_Rent_Adjustment_Date = lockerAllotment.AllotmentDate,
                    Next_Rent_Due_Date = lockerAllotment.NextRentDueDate,
                    Status = lockerAllotment.Status,
                    Closure_Date = null,
                    Refund_Amount = 0,
                    BrCode = lockerAllotment.BrCode ,
                    Created_By = lockerAllotment.CreatedBy ,
                    Created_At = lockerAllotment.AllotmentDate 
                };
                (result,allotmentId ) =  await _unitOfWork.LockerAllotments.AddLockerAllotment(newLocker);

            }
            catch (Exception)
            {
                
            }
           return (result, allotmentId );
        }

        public async Task<List<Locker_Allotments>> EditLockerAllotment(Locker_Allotments lockerAllotment)
        {
            return await  _unitOfWork.LockerAllotments.EditLockerAllotment(lockerAllotment);
        }

        public async Task<LockerAllotmentVM> GetLockerAllotmentByAllotmentId(decimal allotmentId, string brCode)
        {
            return await _unitOfWork.LockerAllotments.GetLockerAllotmentByAllotmentId(allotmentId, brCode);
        }

        public async Task<List<LockerAllotmentVM>> GetLockerAllotmentList(string brCode)
        {
            return await _unitOfWork.LockerAllotments.GetLockerAllotmentList(brCode);   
        }
    }
}
