using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public  class FinLedgerGroupRepository :Repository<Fin_Ledger_Grp>, IFinLedgerGroupRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public FinLedgerGroupRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddFinLedgerGroupAsync(Fin_Ledger_Grp finLedgerGroup)
        {
            bool result = false;
            try
            {
                int maxId = await CSISContext.Fin_Ledger_Grp.MaxAsync(x => x.Grp_Id);
                maxId++;
                finLedgerGroup.Grp_Id = maxId;
                await AddAsync(finLedgerGroup);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! General ledger group not saved");
            }
            return result;
        }

        public async Task<bool> EditFinLedgerGroupAsync(Fin_Ledger_Grp finLedgerGroup)
        {
            bool result = false;
            try
            {
                finLedgerGroup.Grp_Delete = true;
                await EditAsync(finLedgerGroup);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! General ledger group not deleted");
            }
            return result;
        }

        public async Task<List<DropdownItem>> GetFinLedgerGroupItemsAsync()
        {
            List<DropdownItem> list = new List<DropdownItem>();
            try
            {
                list = await (from grp in CSISContext.Fin_Ledger_Grp
                          where grp.Grp_Delete == false
                          select new DropdownItem 
                          {
                              Value = grp.Grp_Id.ToString (),
                              Text = grp.Grp_Name
                          }).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching ledger group names");
            }
            return list;
        }

        public async Task<List<Fin_Ledger_Grp>> GetFinLedgerGroupListAsync(int fnlId, string brCode)
        {
            List<Fin_Ledger_Grp> list = new List<Fin_Ledger_Grp>();
            try
            {
                list = await CSISContext.Fin_Ledger_Grp.Where(x=> x.Fnl_Id  == fnlId && x.Grp_Delete == false && x.BrCode == brCode).ToListAsync();  
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching ledger group List");
            }
            return list;
        }

        public async Task<List<Fin_Ledger_Grp>> GetFinLedgerGroupListAsync(string brCode)
        {
            List<Fin_Ledger_Grp> list = new List<Fin_Ledger_Grp>();
            try
            {
                list = await CSISContext.Fin_Ledger_Grp.Where(x =>  x.Grp_Delete == false && x.BrCode == brCode).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching ledger group List");
            }
            return list;
        }

        public async Task<List<FinBal>> GetGroupLedgerBalance(decimal yrId, string brCode)
        {
            List<FinBal> list = new List<FinBal>();
            try
            {
                var query = await  (from flt in CSISContext.Fin_Ledger_Trn
                              join fl in CSISContext.Fin_Ledger on flt.Led_Id equals fl.Led_Id
                              join flg in CSISContext.Fin_Ledger_Grp on fl.Grp_Id equals flg.Grp_Id
                              join flf in CSISContext.Fin_Ledger_Fnl on flg.Fnl_Id equals flf.Fnl_Id
                              where flt.Yr_Id == yrId && flt.LedgerTrn_Delete == false
                              && flt.BrCode == brCode 
                              && fl.BrCode == brCode 
                              && flg.BrCode == brCode 
                              group new { flt, flg, flf } by new { flf.Fnl_Id, flg.Grp_Id, flg.Grp_Name } into g
                              orderby g.Key.Fnl_Id
                              select new FinBal
                              {
                                  Fnl_Id = g.Key.Fnl_Id,
                                  Grp_Id = g.Key.Grp_Id,
                                  Grp_Name = g.Key.Grp_Name ,
                                  OB_Amt = g.Sum(x => x.flt.OB_Amt),
                                  TotalReceipts = g.Sum(x => x.flt.Tot_Rpt_Amt),
                                  TotalPayments = g.Sum(x => x.flt.Tot_Pmt_Amt),
                                  CB_Amt = g.Sum(x => x.flt.CB_Amt)
                              }).ToListAsync();
                if(query != null && query.Count > 0)
                {
                    list = query.ToList();
                }
            }
            catch (Exception)
            {
                list = new();
            }
            return list;
        }
    }
}
