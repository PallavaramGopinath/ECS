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
    public class MemTrnRepository : Repository<Mem_Trn>, IMemTrnRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public MemTrnRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddMemTrnAsync(Mem_Trn memTrn, string brCode)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Mem_Trn.Where(x => x.BrCode == brCode).MaxAsync(x => x.Mem_Trn_Id);
                maxId++;
                memTrn.Mem_Trn_Id = maxId;
                await AddAsync(memTrn);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while adding member transaction");
            }
            return result;
        }

        public async Task<bool> EditMemTrnAsync(Mem_Trn memTrn, string brCode)
        {
            bool result = false;
            try
            {
                await EditAsync(memTrn);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while modifying member transaction");
            }
            return result;
        }

        public async Task<double> GetSBAccountBalanceByAccId(decimal accId, string brCode)
        {
            double balance = 0;
            try
            {
                var bal = await (from mem in CSISContext.Mem_Trn
                                 where mem.MemTrn_Delete == false && mem.Acc_Id == accId
                                 && mem.BrCode == brCode
                                 select (double?)(mem.Rpt_Amt - mem.Pmt_Amt))
                   .SumAsync();
                double.TryParse(bal.ToString(), out double result);
                balance = result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching sb account balance by accId");
            }
            return balance;
        }

        public async Task<double> GetSBAccountInterestBalanceByAccId(decimal accId, string brCode)
        {
            double balance = 0;
            try
            {
                var bal = await CSISContext.Mem_Trn
                .Where(mem => mem.MemTrn_Delete == false
                       && mem.Acc_Id == accId 
                       && mem.BrCode == brCode)
                .GroupBy(x => 1) // Group all records together
                .Select(g => (double?)(g.Sum(mem => mem.IntCalc_Amt) - g.Sum(mem => mem.IntPaid_Amt)))
                .FirstOrDefaultAsync();
                double.TryParse(bal.ToString(), out double result);
                balance = result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching sb account interest balance by accId");
            }
            return balance;
        }

        public async Task<double> GetMemTrnByMemIdAndLedIdAsync(decimal memId, decimal ledId, string brCode)
        {
            double balance = 0;
            try
            {
                var data = await (from mem in CSISContext.Mem_Trn
                            join led in CSISContext.Fin_Ledger
                            on mem.Led_Id equals led.Led_Id
                            where mem.MemTrn_Delete == false
                                  && mem.Mem_Id == memId
                                  && mem.Led_Id == ledId
                                  && mem.BrCode == brCode
                            group new { mem, led } by new
                            {
                                mem.Trn_Type,
                                mem.Led_Id,
                                mem.Mem_Id,
                                led.Led_Name
                            } into g
                            select new MemberTransactionVM
                            {
                                TrnType = g.Key.Trn_Type,
                                LedId = g.Key.Led_Id,
                                MemId = g.Key.Mem_Id, // CAST to int equivalent
                                Rpt = g.Sum(x => x.mem.Rpt_Amt),
                                Pmt = g.Sum(x => x.mem.Pmt_Amt),
                                LedName = g.Key.Led_Name
                            }).FirstOrDefaultAsync();
                if (data != null)
                {
                    if (data.TrnType == 2 || data.TrnType == 3 || data.TrnType == 6 || data.TrnType == 7)   /// Mem Sus Cr or Share capital or Factory Sus Cr or Staff Sus Cr
                    {
                        balance = data.Rpt - data.Pmt;
                    }
                    if (data.TrnType == 1 || data.TrnType == 4 || data.TrnType == 5)   /// Mem Sus Dr or Factory Sus Dr or Staff Sus Dr
                    {
                        balance = data.Pmt - data.Rpt;
                    }
                    double.TryParse(balance.ToString(), out double result);
                    balance = result;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching sb account interest balance by accId");
            }
            return balance;
        }

        public async Task<double> GetmemTrnTotalSuspenseAmount(decimal memId, int trnType, string brCode)
        {
            double suspenseAmt = 0;
            try
            {
                var suspenseTmp = await CSISContext.Mem_Trn
                .Where(t => t.Trn_Type == trnType &&
                            t.MemTrn_Delete == false &&
                            t.Mem_Id == memId &&
                            t.BrCode == brCode )
                .SumAsync(t => (double?)(t.Pmt_Amt - t.Rpt_Amt));
                double.TryParse(suspenseTmp.ToString(), out suspenseAmt);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching member's suspense account total balance");
            }
            return suspenseAmt;
        }
    }
}
