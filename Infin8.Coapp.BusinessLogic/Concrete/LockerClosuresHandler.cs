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
    public  class LockerClosuresHandler : ILockerClosuresHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LockerClosuresHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Locker_Closures>> AddLockerClosure(Locker_Closures lockerClosure)
        {
            return await _unitOfWork.LockerClosures.AddLockerClosure(lockerClosure);
        }

        public async Task<List<LockerAllotmentVM>> CloseLockerAllotment(Locker_Closures lockerClosure)
        {
            List<LockerAllotmentVM> lockerAllotments = new List<LockerAllotmentVM>();
            try
            {
                var result = await _unitOfWork.LockerClosures.CloseLockerAllotment(lockerClosure);
                if(result == true)
                {
                    var allotmentResult = await  _unitOfWork.LockerAllotments.GetLockerAllotmentList(lockerClosure.BrCode!);
                    if(allotmentResult != null && allotmentResult.Count >0)
                    {
                        lockerAllotments = allotmentResult.ToList();
                    }
                }
            }
            catch (Exception)
            {
                lockerAllotments = new List<LockerAllotmentVM>();
            }
            return lockerAllotments;
        }

        public async Task<List<Locker_Closures>> EditLockerClosure(Locker_Closures lockerClosure)
        {
            return await _unitOfWork.LockerClosures.EditLockerClosure(lockerClosure);
        }

        public async Task<List<Locker_Closures>> GetLockerClosuresListAsync(string brCode)
        {
            return await _unitOfWork.LockerClosures.GetLockerClosuresListAsync(brCode);
        }
    }
}
