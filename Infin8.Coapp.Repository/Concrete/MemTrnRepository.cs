using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.AspNetCore.Routing.Constraints;
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

        public async Task<bool> AddMemTrnAsync(Mem_Trn memTrn)
        {
            bool result = false;
            try
            {
                //decimal maxId = await CSISContext.Mem_Trn.Where(x => x.BrCode == memTrn.BrCode).MaxAsync(x => x.Mem_Trn_Id);
                decimal maxId = await CSISContext.Mem_Trn
                    .Where(x => x.BrCode == memTrn.BrCode)
                    .Select(x => (decimal?)x.Mem_Trn_Id)
                    .MaxAsync() ?? 0;

                //int maxTrnId = await CSISContext.Mem_Trn.Where(x=> x.Mem_Id == memTrn.Mem_Id && x.Trn_Type == memTrn.Trn_Type && x.Led_Id == memTrn.Led_Id && x.BrCode == memTrn.BrCode).MaxAsync(x => x.Trn_SlNo);
                int maxTrnId = await CSISContext.Mem_Trn
                .Where(x => x.Mem_Id == memTrn.Mem_Id &&
                            x.Trn_Type == memTrn.Trn_Type &&
                            x.Led_Id == memTrn.Led_Id &&
                            x.BrCode == memTrn.BrCode)
                .Select(x => (int?)x.Trn_SlNo)
                .MaxAsync() ?? 0;
                maxId++;
                maxTrnId++;
                memTrn.Mem_Trn_Id = maxId;
                memTrn.Trn_SlNo = maxTrnId;
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

        public async Task<bool> AddMemTrnListAsync(List<Mem_Trn> memTrnList)
        {
            bool result = false;
            decimal maxId = 0;
            int maxSlNo = 0;
            decimal memId = memTrnList.Select(x=> x.Mem_Id ).FirstOrDefault ();
            int memType = memTrnList.Select(x => x.Trn_Type).FirstOrDefault();
            decimal ledId = memTrnList.Select(x=> x.Led_Id ).FirstOrDefault();
            string brCode = memTrnList.Select(x => x.BrCode).FirstOrDefault()!;
            try
            {
                maxId = await CSISContext.Mem_Trn.MaxAsync(x => x.Mem_Trn_Id);
                maxSlNo = await CSISContext.Mem_Trn
                .Where(x => x.Mem_Id == memId &&
                            x.Trn_Type == memType &&
                            x.Led_Id == ledId  &&
                            x.BrCode == brCode)
                .Select(x => (int?)x.Trn_SlNo)
                .MaxAsync() ?? 0;
                foreach (var trn in memTrnList)
                {
                    maxId++;
                    maxSlNo++;
                    trn.Mem_Trn_Id  = maxId;
                    trn.Trn_SlNo = maxSlNo;
                    await AddAsync(trn);
                    CSISContext.SaveChanges();
                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan trn list not saved");
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

        public async Task<DtoSBAccountBalanceWithIds> GetSBAccountBalanceWithIds(decimal memId, string brCode)
        {
            DtoSBAccountBalanceWithIds sbData = new();
            try
            {
                var query = await (from mt in CSISContext.Mem_Trn
                                   join sm in CSISContext.SBCA_Master on mt.Acc_Id equals sm.Acc_Id
                                   join ss in CSISContext.SBCA_Schemes on sm.Scheme_Id equals ss.Scheme_Id
                                   where mt.Trn_Type == 7
                                      && mt.Mem_Id == memId
                                      && mt.MemTrn_Delete == false
                                      && sm.Acc_Delete == false
                                      && mt.BrCode == brCode
                                      && sm.BrCode == brCode
                                   group new { mt, sm, ss } by new // Group by the necessary fields
                                   {
                                       sm.Acc_Id,
                                       sm.Acc_No,
                                       ss.SBCA_Led_Id
                                   } into g // 'g' is now an IGrouping
                                   select new DtoSBAccountBalanceWithIds // Or an anonymous type: new { ... }
                                   {
                                       Acc_Id = g.Key.Acc_Id,
                                       Acc_No = g.Key.Acc_No,
                                       SBCA_Led_Id = g.Key.SBCA_Led_Id,
                                       Balance_Amount = g.Sum(x => x.mt.Rpt_Amt) - g.Sum(x => x.mt.Pmt_Amt)
                                   }).FirstOrDefaultAsync();
                if (query != null) sbData = query;

            }
            catch (Exception)
            {

                throw;
            }
            return sbData;
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
                            t.BrCode == brCode)
                .SumAsync(t => (double?)(t.Pmt_Amt - t.Rpt_Amt));
                double.TryParse(suspenseTmp.ToString(), out suspenseAmt);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching member's suspense account total balance");
            }
            return suspenseAmt;
        }

        public async Task<List<MemberTransactionVM>> GetMemberTrnBalanceList(decimal MemId, int TrnType, string brCode)
        {
            List<MemberTransactionVM> memTrnList = new();
            /// 1= mem due to, 2= mem due by, 3= share capital, 5=staff due to,6=staff due by, 7=sb account (correct)
            /// wrong trn_Type 4= factory due to, 5= factory due by, 6= staff due to, 7= staff due by (wrong)
            try
            {
                if (TrnType == 1 || TrnType == 4 || TrnType == 5)
                {
                    var result = await (from mem in CSISContext.Mem_Trn
                                        join led in CSISContext.Fin_Ledger on mem.Led_Id equals led.Led_Id
                                        where !mem.MemTrn_Delete
                                        group new { mem, led } by new
                                        {
                                            mem.Trn_Type,
                                            mem.Mem_Id,
                                            mem.Led_Id,
                                            led.Led_Name,
                                            mem.BrCode
                                        } into g
                                        where g.Key.Mem_Id == MemId
                                              && g.Key.Trn_Type == TrnType
                                              && g.Key.BrCode == brCode
                                              && (g.Sum(x => (double)x.mem.Pmt_Amt) - g.Sum(x => (double)x.mem.Rpt_Amt) > 0)
                                        select new MemberTransactionVM
                                        {
                                            TrnType = (byte)g.Key.Trn_Type,
                                            MemId = g.Key.Mem_Id,
                                            LedId = (decimal)g.Key.Led_Id,
                                            LedName = g.Key.Led_Name,
                                            Rpt = g.Sum(x => (double)x.mem.Rpt_Amt),
                                            Pmt = g.Sum(x => (double)x.mem.Pmt_Amt),
                                            IntCalculatedAmt = g.Sum(x => (int)x.mem.IntCalc_Amt),
                                            IntPaid = g.Sum(x => (int)x.mem.IntPaid_Amt)
                                        }).ToListAsync();
                    if (result != null && result.Any())
                    {
                        memTrnList = result.ToList();
                    }
                }

                if (TrnType == 2 || TrnType == 3 || TrnType == 6 || TrnType == 7)
                {
                    var result = await (from mem in CSISContext.Mem_Trn
                                        join led in CSISContext.Fin_Ledger on mem.Led_Id equals led.Led_Id
                                        where !mem.MemTrn_Delete
                                        group new { mem, led } by new
                                        {
                                            mem.Trn_Type,
                                            mem.Mem_Id,
                                            mem.Led_Id,
                                            led.Led_Name,
                                            mem.BrCode,
                                        } into g
                                        where g.Key.Mem_Id == MemId
                                              && g.Key.Trn_Type == TrnType
                                              && g.Key.BrCode == brCode
                                              && (g.Sum(x => (double)x.mem.Rpt_Amt) - g.Sum(x => (double)x.mem.Pmt_Amt) > 0)
                                        select new MemberTransactionVM
                                        {
                                            TrnType = (byte)g.Key.Trn_Type,
                                            MemId = g.Key.Mem_Id,
                                            LedId = (decimal)g.Key.Led_Id,
                                            LedName = g.Key.Led_Name,
                                            Rpt = g.Sum(x => (double)x.mem.Rpt_Amt),
                                            Pmt = g.Sum(x => (double)x.mem.Pmt_Amt),
                                            IntCalculatedAmt = g.Sum(x => (int)x.mem.IntCalc_Amt),
                                            IntPaid = g.Sum(x => (int)x.mem.IntPaid_Amt)
                                        }).ToListAsync();
                    if (result != null && result.Any())
                    {
                        memTrnList = result.ToList();
                    }
                }

                if (memTrnList != null && memTrnList.Any())
                {
                    foreach (var item in memTrnList)
                    {
                        if (item.TrnType == 2 || item.TrnType == 3 || item.TrnType == 6 || item.TrnType == 7)   /// Mem Sus Cr or Share capital or Factory Sus Cr or Staff Sus Cr
                        {
                            if (item.Rpt - item.Pmt > 0)
                                item.Balance = item.Rpt - item.Pmt;
                            else
                                item.Balance = 0;
                        }
                        if (item.TrnType == 1 || item.TrnType == 5)   /// 1=Mem Sus Dr or  5=Staff Sus Dr /// wrong  Factory Sus DR 5
                        {
                            if (item.Pmt - item.Rpt > 0)
                                item.Balance = item.Pmt - item.Rpt;
                            else
                                item.Balance = 0;
                        }
                        item.IntBalance = item.IntCalculatedAmt - item.IntPaid;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return memTrnList!;
        }

        public async Task<List<DividendOrIntOnTDPaymentVM>> GetDividendPayableListAsync(decimal memId, DateTime asOnDate, string brCode)
        {
            List<DividendOrIntOnTDPaymentVM> dividendList = new();
            try
            {
                var result = await (from trn in CSISContext.Mem_Trn
                                    join dividend in CSISContext.Mem_Payable_Master on trn.PbleMaster_Id equals dividend.PbleMaster_Id
                                    where trn.Mem_Id == memId && trn.Trn_Date <= asOnDate && trn.MemTrn_Delete == false
                                    && trn.Trn_Type == 3 && trn.BrCode == brCode
                                    group new { trn, dividend } by new
                                    {
                                        trn.Mem_Id,
                                        trn.PbleMaster_Id,
                                        trn.Led_Id ,
                                        trn.Trn_Type,
                                        dividend.PbleType ,
                                        dividend.FromDate ,
                                        dividend.ToDate ,
                                        trn.BrCode
                                    } into g
                                    where g.Key.Mem_Id == memId
                                             && g.Key.Trn_Type == 3
                                             && g.Key.BrCode == brCode
                                             && (g.Sum(x => (int)x.trn.IntCalc_Amt) - g.Sum(x => (int)x.trn.IntPaid_Amt) > 0)
                                    select new DividendOrIntOnTDPaymentVM
                                    {
                                        PbleMaster_Id = g.Key.PbleMaster_Id,
                                        Mem_Id = g.Key.Mem_Id,
                                        Led_Id = g.Key.Led_Id,
                                        FromDate = (DateTime)g.Key.FromDate!,
                                        ToDate = (DateTime)g.Key.ToDate!,
                                        PbleType = g.Key.PbleType,
                                        IntCalc_Amt = g.Sum(x=> x.trn.IntCalc_Amt ),
                                        IntPaid_Amt = g.Sum(x=> x.trn.IntPaid_Amt  ),
                                        Int_Bal = g.Sum(x => x.trn.IntCalc_Amt) - g.Sum(x => x.trn.IntPaid_Amt),
                                        Dividend_Paid = 0
                                    }
                             ).ToListAsync();
                if(result != null && result.Any())
                {
                    dividendList = result.ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return dividendList;
        }

        public async Task<List<Mem_Demand>> CalculateDueToDemand( decimal memId,DateTime demandDate, string brCode)
        {
            List<Mem_Demand> dueToDemand = [];
            List<MemberTransactionVM> memTrnList = [];
            try
            {
                memTrnList = await (from mem in CSISContext.Mem_Trn
                              join led in CSISContext.Fin_Ledger on mem.Led_Id equals led.Led_Id
                              where mem.Mem_Id == memId &&  mem.MemTrn_Delete == false && mem.Trn_Type == 1 && mem.BrCode == brCode && led.BrCode == brCode 
                              group new { mem, led } by new
                              {
                                  mem.Trn_Type,
                                  mem.Mem_Id,
                                  mem.Led_Id,
                                  led.Led_Name
                              } into g
                              let rpt = g.Sum(x => (double)x.mem.Rpt_Amt)
                              let pmt = g.Sum(x => (double)x.mem.Pmt_Amt)
                              where pmt - rpt > 0
                              select new MemberTransactionVM
                              {
                                  TrnType = (byte)g.Key.Trn_Type,
                                  MemId = g.Key.Mem_Id,
                                  LedId = (int)g.Key.Led_Id,
                                  LedName = g.Key.Led_Name,
                                  Rpt = rpt,
                                  Pmt = pmt,
                                  IntCalculatedAmt = g.Sum(x => (int)x.mem.IntCalc_Amt),
                                  IntPaid = g.Sum(x => (int)x.mem.IntPaid_Amt)
                              }).ToListAsync();

                
                foreach (MemberTransactionVM dueto in memTrnList)
                {
                    if (Math.Round(dueto.Pmt, 2) - Math.Round(dueto.Rpt, 2) > 0)
                    {
                        Mem_Demand single = new()
                        {
                            Id = 0,
                            Demand_Id = 0,
                            Recovery_Date = null,
                            Demand_Type = "S",
                            Mem_Id = dueto.MemId,
                            Loan_Id = 0,
                            Debtor_Led_Id = dueto.LedId,
                            Debtor_Arrear = 0,
                            Debtor_Current = Math.Round(dueto.Pmt, 2) - Math.Round(dueto.Rpt, 2),
                            Debtor_Total = dueto.Pmt - dueto.Rpt,
                            Total_Demand = dueto.Pmt - dueto.Rpt,
                            Usr_Id = 0,
                            Yr_Id = 0,
                            Voc_Id = 0,
                            Recovery_Id = 0
                        };
                        dueToDemand.Add(single);
                    }
                }
            }
            catch (Exception)
            {
            }
            return dueToDemand;
        }
    }
}
