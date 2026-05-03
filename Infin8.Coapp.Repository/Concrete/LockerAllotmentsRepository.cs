using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    internal class LockerAllotmentsRepository : Repository<Locker_Allotments>, ILockerAllotmentsRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LockerAllotmentsRepository(DbContext context) : base(context)
        {

        }
        public async Task<List<LockerAllotmentVM>> AddLockerAllotment(Locker_Allotments lockerAllotment)
        {
            List<LockerAllotmentVM> lockerAllotmentList = new();
            try
            {
                decimal maxId = await CSISContext.Locker_Allotments
                .MaxAsync(x => (decimal?)x.Id) ?? 0;
                if (maxId == 0)
                {
                    decimal.TryParse(lockerAllotment.BrCode + "0000000", out maxId);
                }
                maxId++;
                lockerAllotment.Id = maxId;
                await AddAsync(lockerAllotment);
                CSISContext.SaveChanges();
                var result = await GetLockerAllotmentList(lockerAllotment.BrCode!);
                if (result != null && result.Count > 0)
                {
                    lockerAllotmentList = result.ToList();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while adding locker allotment");
            }
            return lockerAllotmentList;
        }

        public Task<List<Locker_Allotments>> EditLockerAllotment(Locker_Allotments lockerAllotment)
        {
            throw new NotImplementedException();
        }

        public async Task<List<LockerAllotmentVM>> GetLockerAllotmentList(string brCode)
        {
            List<LockerAllotmentVM> lockerAllotments = new();
            try
            {
                var query = await (from la in CSISContext.Locker_Allotments
                                   join mem in CSISContext.mem_master on la.Customer_Id equals mem.mem_id
                                   join l in CSISContext.Lockers on la.Locker_Id equals l.Id
                                   where la.BrCode == brCode
                                   && mem.brcode == brCode
                                   && l.BrCode == brCode
                                   select new LockerAllotmentVM
                                   {
                                       Id = la.Id,
                                       CustomerId = la.Customer_Id,
                                       LockerId = la.Locker_Id,
                                       AllotmentDate = la.Allotment_Date,
                                       DepositAmount = la.Deposit_Amount,
                                       InterestRate = la.Interest_Rate,
                                       LastRentAdjustmentDate = la.Last_Rent_Adjustment_Date,
                                       NextRentDueDate = la.Next_Rent_Due_Date,
                                       Status = la.Status,
                                       ClosureDate = la.Closure_Date,
                                       RefundAmount = la.Refund_Amount,
                                       BrCode = la.BrCode,
                                       CreatedBy = la.Created_By,
                                       CustomerName = mem.membername!,
                                       LockerNumber = l.Locker_Number!
                                   }).ToListAsync();
                lockerAllotments = query.ToList();
            }
            catch (Exception)
            {

                throw;
            }
            return lockerAllotments;
        }

        public async Task<LockerAllotmentVM> GetLockerAllotmentByAllotmentId(decimal allotmentId, string brCode)
        {
            LockerAllotmentVM lockerAllotment = new();
            try
            {
                var query = await (from la in CSISContext.Locker_Allotments
                                   join mem in CSISContext.mem_master on la.Customer_Id equals mem.mem_id
                                   join l in CSISContext.Lockers on la.Locker_Id equals l.Id
                                   join ls in CSISContext.Locker_Size_Master on l.Size_Id equals ls.Id
                                   where la.Id == allotmentId
                                   && la.BrCode == brCode
                                   && mem.brcode == brCode
                                   && l.BrCode == brCode
                                   select new LockerAllotmentVM
                                   {
                                       Id = la.Id,
                                       CustomerId = la.Customer_Id,
                                       LockerId = la.Locker_Id,
                                       AllotmentDate = la.Allotment_Date,
                                       DepositAmount = la.Deposit_Amount,
                                       InterestRate = la.Interest_Rate,
                                       RentAmount = ls.Rent_Amount,
                                       LastRentAdjustmentDate = la.Last_Rent_Adjustment_Date,
                                       NextRentDueDate = la.Next_Rent_Due_Date,
                                       Status = la.Status,
                                       ClosureDate = la.Closure_Date,
                                       RefundAmount = la.Refund_Amount,
                                       BrCode = la.BrCode,
                                       CreatedBy = la.Created_By,
                                       CustomerName = mem.membername!,
                                       LockerNumber = l.Locker_Number!
                                   }).FirstOrDefaultAsync();
                lockerAllotment = query;
            }
            catch (Exception)
            {
                throw;
            }
            return lockerAllotment;
        }

    }
}
