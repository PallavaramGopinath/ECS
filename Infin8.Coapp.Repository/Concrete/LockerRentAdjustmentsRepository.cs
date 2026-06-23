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

        public async Task<List<LockerClosureBalanceDto>> GetLockerClosureBalanceListAsync(decimal customerId, string brCode)
        {
            List<LockerClosureBalanceDto> closureBalance = new List<LockerClosureBalanceDto>();
            try
            {
                // Base: one row per active allotment backed by a term deposit.
                closureBalance = await (from a in CSISContext.Locker_Allotments
                                        join l in CSISContext.Lockers on a.Locker_Id equals l.Id
                                        join s in CSISContext.Locker_Size_Master on l.Size_Id equals s.Id
                                        join t in CSISContext.TermDeposit_Master on a.Td_Id equals t.TD_Id
                                        where a.Customer_Id == customerId && a.BrCode == brCode
                                        && a.Status == "Active"
                                        select new LockerClosureBalanceDto
                                        {
                                            AllotmentId = a.Id,
                                            LockerId = a.Locker_Id,
                                            LockerNumber = l.Locker_Number,
                                            SizeName = s.Size_Name,
                                            RentAmount = s.Rent_Amount,
                                            RentReceived = 0,
                                            DepositId = a.Td_Id,
                                            DepositNo = t.TD_No,
                                            DepositDate = t.ValueDate,
                                            InterestRate = t.RateOfInterest,
                                            DepositRefundAmount = a.Deposit_Amount,
                                            InterestCalculated = 0
                                        }).ToListAsync();

                if (closureBalance.Count == 0)
                {
                    return closureBalance;
                }

                var allotmentIds = closureBalance.Select(b => b.AllotmentId).ToList();
                var depositIds = closureBalance.Select(b => b.DepositId).ToList();

                // Rent receivable aggregated separately by allotment (no cartesian product).
                var rentByAllotment = await CSISContext.Locker_Rent_Adjustments
                    .Where(r => allotmentIds.Contains(r.Allotment_Id))
                    .GroupBy(r => r.Allotment_Id)
                    .Select(g => new
                    {
                        AllotmentId = g.Key,
                        RentReceivable = g.Sum(r => r.Rent_Receivable) - g.Sum(r => r.Rent_Received)
                    })
                    .ToDictionaryAsync(x => x.AllotmentId, x => x.RentReceivable);

                // TD interest balance aggregated separately by deposit (no cartesian product).
                var interestByDeposit = await CSISContext.TermDeposit_Trn
                    .Where(tr => depositIds.Contains(tr.TD_Id))
                    .GroupBy(tr => tr.TD_Id)
                    .Select(g => new
                    {
                        DepositId = g.Key,
                        InterestPreviousBalance = g.Sum(tr => tr.InterestCalculatedAmount) - g.Sum(tr => tr.InterestPaidAmount),
                        InterestPreviousAppliedDate = g.Max(tr => tr.InterestAppliedDate)
                    })
                    .ToListAsync();
                var interestMap = interestByDeposit.ToDictionary(x => x.DepositId);

                foreach (var bal in closureBalance)
                {
                    bal.RentReceivable = rentByAllotment.TryGetValue(bal.AllotmentId, out var rent) ? rent : 0;
                    if (interestMap.TryGetValue(bal.DepositId, out var interest))
                    {
                        bal.InterestPreviousBalance = interest.InterestPreviousBalance;
                        bal.InterestPreviousAppliedDate = interest.InterestPreviousAppliedDate;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching locker closure balance");
            }
            return closureBalance;
        }

    }
}
