using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class FinLedgerTrnRepository : Repository<Fin_Ledger_Trn>, IFinLedgerTrnRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public FinLedgerTrnRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddFinLedgerTrnAsync(Fin_Ledger_Trn finLedgerTrn)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Fin_Ledger_Trn.MaxAsync(x => x.Trn_Id);
                maxId++;
                finLedgerTrn.Trn_Id = maxId;
                await AddAsync(finLedgerTrn);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! General ledger trn not saved");
            }
            return result;
        }

        public async Task<bool> AddFinLedgerTrnListAsync(List<Fin_Ledger_Trn> finLedgerTrnList)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Fin_Ledger_Trn.MaxAsync(x => x.Trn_Id);
                //maxId++;
                //finLedgerTrn.Trn_Id = maxId;
                //await AddAsync(finLedgerTrn);
                foreach(var ledger in finLedgerTrnList)
                {
                    maxId++;
                    ledger.Trn_Id = maxId;
                }
                await CSISContext.AddRangeAsync(finLedgerTrnList);
                await CSISContext.SaveChangesAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! General ledger trn not saved");
            }
            return result;
        }
        public async Task<bool> EditFinLedgerTrnAsync(Fin_Ledger_Trn finLedgerTrn)
        {
            bool result = false;
            try
            {
                finLedgerTrn.LedgerTrn_Delete = true;
                await EditAsync(finLedgerTrn);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! General ledger trn not deleted");
            }
            return result;
        }

        public async Task<bool> UpdateLedgerBalance(decimal yrId, DateTime fromDate, DateTime toDate,string brCode )
        {
            decimal cashLedId = 0;
            double ledgerbalance = 0;
            bool result = false;
            try
            {
                var query = await  (from fvt in CSISContext.Fin_Voucher_Trn
                              join fv in CSISContext.Fin_Voucher on fvt.Voc_Id equals fv.Voc_Id
                              join flt in CSISContext.Fin_Ledger_Trn on fvt.Led_Id equals flt.Led_Id
                              join fl in CSISContext.Fin_Ledger on fvt.Led_Id equals fl.Led_Id
                              join flg in CSISContext.Fin_Ledger_Grp on fl.Grp_Id equals flg.Grp_Id
                              where fvt.FinVocTr_Delete == false
                                    && flt.Yr_Id == yrId
                                    && fvt.Yr_Id == yrId
                                    && fv.Yr_Id == yrId
                                    && fv.Voc_Date >= fromDate
                                    && fv.Voc_Date <= toDate
                              group new { fvt, flg, flt } by new { fvt.Led_Id, flg.Fnl_Id, flt.OB_Amt } into g
                              select new FinBal
                              {
                                  Led_Id = g.Key.Led_Id,
                                  Fnl_Id = g.Key.Fnl_Id,
                                  OB_Amt = g.Key.OB_Amt,
                                  TotalReceipts = g.Sum(x => x.fvt.Voc_Rpt),
                                  TotalPayments = g.Sum(x => x.fvt.Voc_Pmt)
                              }).ToListAsync();
                foreach(var bal in query)
                {
                    /// calculate ledger balance
                    switch (bal.Fnl_Id)
                    {
                        case 1:
                        case 4:
                            if (bal.Led_Id == cashLedId)
                            {
                                ledgerbalance += bal.TotalReceipts - bal.TotalPayments;
                            }
                            else
                            {
                                ledgerbalance += bal.TotalPayments - bal.TotalReceipts;
                            }
                            break;
                        case 2:
                        case 3:
                            ledgerbalance += bal.TotalReceipts - bal.TotalPayments;
                            break;
                    }
                      await  CSISContext.Database.ExecuteSqlRawAsync(@"Update Fin_Ledger_Trn set CB_Amt = @CB,Tot_Rpt_Amt = @totRpt,Tot_Pmt_Amt = @totPmt where Led_Id = @ledId And Yr_ID = @yrId"
                       , new NpgsqlParameter("@CB", ledgerbalance)
                       , new NpgsqlParameter("@totRpt", bal.TotalReceipts)
                       , new NpgsqlParameter("@totPmt", bal.TotalPayments)
                       , new NpgsqlParameter("@ledId", bal.Led_Id)
                       , new NpgsqlParameter("@yrId", yrId));
                }
                result = true;
            }
            catch (Exception)
            {
                result  =false;
            }
            return result ;
        }

        public async Task<List<FinBal>> GetGeneralLedgerBalance(int grpId,decimal yrId, string brCode)
        {
            List<FinBal> result = new List<FinBal>();
            try
            {
                var query = await  (from trn in CSISContext.Fin_Ledger_Trn
                             join led in CSISContext.Fin_Ledger on trn.Led_Id equals led.Led_Id
                             where trn.Yr_Id == yrId
                             && trn.LedgerTrn_Delete == false
                             && led.Grp_Id == grpId
                             && led.Led_Delete == false
                             && trn.BrCode == brCode
                             && led.BrCode == brCode 
                             group new { trn, led } by new {trn.Led_Id ,led.Led_Name,led.Grp_Id  } into g
                             orderby g.Key.Grp_Id
                             select new FinBal
                             {
                                 Led_Id = g.Key.Led_Id,
                                 Led_Name = g.Key.Led_Name, 
                                 Grp_Id = g.Key.Grp_Id,
                                 OB_Amt = g.Sum(x=> x.trn.OB_Amt),
                                 TotalReceipts = g.Sum(x=> x.trn.Tot_Rpt_Amt ),
                                 TotalPayments =g.Sum(x=> x.trn.Tot_Pmt_Amt ),
                                 CB_Amt = g.Sum(x=> x.trn.CB_Amt),
                             }).ToListAsync();
                if(query != null && query.Count > 0) result.AddRange(query);
            }
            catch (Exception)
            {
                result = new();
            }
            return result;
        }
    }
}
