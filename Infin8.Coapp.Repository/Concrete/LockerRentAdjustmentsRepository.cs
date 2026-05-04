using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    internal class LockerRentAdjustmentsRepository : Repository<Locker_Rent_Adjustments>, ILockerRentAdjustmentsRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LockerRentAdjustmentsRepository(DbContext context) : base(context)
        {
        }
        public async Task<bool> AddLockerRentAdjustment(Locker_Rent_Adjustments lockerRentAdjustment)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Locker_Rent_Adjustments 
                .MaxAsync(x => (decimal?)x.Id) ?? 0;
                if (maxId == 0)
                {
                    decimal.TryParse(lockerRentAdjustment.BrCode + "0000000", out maxId);
                }
                maxId++;
                lockerRentAdjustment.Id = maxId;
                await AddAsync(lockerRentAdjustment);
                result = true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while adding locker allotment");
            }
            return result;
        }

        public Task<List<Locker_Rent_Adjustments>> EditLockerRentAdjustment(Locker_Rent_Adjustments lockerRentAdjustment)
        {
            throw new NotImplementedException();
        }

        public Task<List<Locker_Rent_Adjustments>> GetLockerRentAdjustmentsListAsync(string brCode)
        {
            throw new NotImplementedException();
        }

        public async Task<List<LockerRentReceiptAllotmentWiseDto>> GetLockerRentAllotmentWiseListAsync(decimal customerId, string brCode)
        {
            List<LockerRentReceiptAllotmentWiseDto> allotmentWiseReceivable = new List<LockerRentReceiptAllotmentWiseDto>();
            try
            {
                var query = await  (from a in CSISContext.Locker_Allotments
                            join l in CSISContext.Lockers on a.Locker_Id equals l.Id
                            join s in CSISContext.Locker_Size_Master on l.Size_Id equals s.Id
                            join r in CSISContext.Locker_Rent_Adjustments on a.Id equals r.Allotment_Id
                            where a.Customer_Id == customerId && a.BrCode == brCode
                            && a.Status == "Active"
                            group new { a,l, s, r } by new { a.Id, a.Locker_Id, l.Locker_Number , s.Size_Name, s.Rent_Amount,   } into g
                            select new LockerRentReceiptAllotmentWiseDto
                            {
                                AllotmentId = g.Key.Id,
                                LockerId = g.Key.Locker_Id,
                                LockerNumber = g.Key.Locker_Number,
                                SizeName = g.Key.Size_Name,
                                RentAmount = g.Key.Rent_Amount ,
                                RentReceivable = g.Sum(trn=> trn.r.Rent_Receivable) - g.Sum(trn=> trn.r.Rent_Received),
                                RentReceived = 0
                            }).ToListAsync ();
                allotmentWiseReceivable = query.ToList();
            }
            catch (Exception)
            {
                allotmentWiseReceivable = new();
            }
            return allotmentWiseReceivable;
        }
    }
}
