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
    public class FinLedgerRepository : Repository<Fin_Ledger>, IFinLedgerRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public FinLedgerRepository(DbContext context) : base(context)
        {
        }

        public async Task<decimal> AddFinLedgerAsync(Fin_Ledger finLedger)
        {
            //bool result = false;
            decimal ledId = 0;
            try
            {
                decimal maxId = await CSISContext.Fin_Ledger.MaxAsync(x => x.Led_Id);
                maxId++;
                finLedger.Led_Id = maxId;
                await AddAsync(finLedger);
                //result = true;
            }
            catch (Exception ex)
            {
                //result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! General ledger not saved");
            }
            return ledId;
        }

        public async Task<bool> EditFinLedgerAsync(Fin_Ledger finLedger)
        {
            bool result = false;
            try
            {
                finLedger.Led_Delete = true;
                await EditAsync(finLedger);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! General ledger not deleted");
            }
            return result;
        }

        public async Task<List<DropdownItem>> GetLedgerListAsync(int fnlId, decimal cashLedId, string brCode)
        {
            List<DropdownItem> ledgerList = new List<DropdownItem>();
            try
            {
                if (fnlId == 0)
                {
                    ledgerList = (from ledger in CSISContext.Fin_Ledger
                                  join ledgerGrp in CSISContext.Fin_Ledger_Grp on ledger.Grp_Id equals ledgerGrp.Grp_Id
                                  where ledger.Led_Delete == false
                                        && ledgerGrp.Grp_Delete == false
                                        && ledger.Led_Id != cashLedId
                                        && ledger.BrCode == brCode
                                  select new DropdownItem
                                  {
                                      Value = ledger.Led_Id.ToString(),
                                      Text = ledger.Led_Name
                                  }).ToList();
                }
                else
                {
                    ledgerList = (from ledger in CSISContext.Fin_Ledger
                                  join ledgerGrp in CSISContext.Fin_Ledger_Grp on ledger.Grp_Id equals ledgerGrp.Grp_Id
                                  where ledger.Led_Delete == false
                                        && ledgerGrp.Fnl_Id == fnlId
                                        && ledgerGrp.Grp_Delete == false
                                        && ledger.Led_Id != cashLedId
                                        && ledger.BrCode == brCode
                                  select new DropdownItem
                                  {
                                      Value = ledger.Led_Id.ToString(),
                                      Text = ledger.Led_Name
                                  }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! Error in fetching ledger list");
            }
            return await Task.FromResult(ledgerList);
        }

        //public async  Task<decimal> GetCashLedgerIdAsync()
        //{
        //    decimal cashLedId = 0;
        //    try
        //    {
        //        var cashOnHandLedId = await  CSISContext.Map_General.Select(m => m.Cash_Led_Id).FirstOrDefaultAsync();
        //        if (cashOnHandLedId >0) cashLedId = cashOnHandLedId;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new InvalidOperationException(ex.Message + " Something went wrong! Error in fetching cash ledger id");
        //    }
        //    return cashLedId;
        //}

        public async Task<List<Fin_Ledger>> GetLedgerListByGrpIdAsync(int grpId, string brCode)
        {
            List<Fin_Ledger> ledgerList = new List<Fin_Ledger>();
            try
            {
                if (grpId == 0)
                    ledgerList = await CSISContext.Fin_Ledger.Where(x => x.Led_Delete == false && x.BrCode == brCode).ToListAsync();
                else
                    ledgerList = await CSISContext.Fin_Ledger.Where(x => x.Grp_Id == grpId && x.Led_Delete == false && x.BrCode == brCode).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching ledger list by group id");
            }
            return ledgerList;
        }

        public async Task<List<DropdownItem>> GetLedgerItemsExceptBankLedgersAsyn(string brCode)
        {
            List<DropdownItem> ledgerList = new List<DropdownItem>();
            try
            {
                var result = await CSISContext.Fin_Ledger
                .Where(fl => fl.Led_Delete == false && fl.BrCode == brCode)
                .Where(fl => !CSISContext.Map_General.Where(x => x.BrCode == brCode).Select(mf => mf.Cash_Led_Id)
                            .Union(CSISContext.Map_Banks.Where(x => x.BrCode == brCode).Select(mb => mb.Led_Id))
                            .Contains(fl.Led_Id))
                .Select(fl => new DropdownItem
                {
                    Value = fl.Led_Id.ToString(), // CAST to VARCHAR equivalent
                    Text = fl.Led_Name
                })
                .ToListAsync();
                if (result != null) ledgerList = result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching ledger items that are not included in the bank ledger");
            }
            return ledgerList;
        }

        public async Task<List<DropdownItem>> GetLedgerItemsByFnlIdAsync(int fnlId, decimal cashLedId, string brCode)
        {
            List<DropdownItem> ledgerList = new List<DropdownItem>();
            try
            {
                //var ledgerList3 = CSISContext.Fin_Ledger
                //.Join(CSISContext.Fin_Ledger_Grp,
                //    ledger => ledger.Grp_Id,
                //    group => group.Grp_Id,
                //    (ledger, group) => new { ledger, group })
                //.Where(x => !x.ledger.Led_Delete &&
                //            x.group.Fnl_Id == fnlId &&
                //            !x.group.Grp_Delete &&
                //            x.ledger.Led_Id != cashLedId)
                //.Select(x => new DropdownItem
                //{
                //    Value = x.ledger.Led_Id.ToString(),
                //    Text = x.ledger.Led_Name
                //})
                //.ToList();

                if (fnlId == 0)
                {
                    var ledgerList1 = await (from fl in CSISContext.Fin_Ledger
                                             join flg in CSISContext.Fin_Ledger_Grp
                                             on fl.Grp_Id equals flg.Grp_Id
                                             where fl.Led_Delete == false
                                                   && flg.Fnl_Id == fnlId
                                                   && flg.Grp_Delete == false
                                                   && fl.Led_Id != cashLedId
                                                   && fl.BrCode == brCode
                                                   && flg.BrCode == brCode
                                             select new DropdownItem
                                             {
                                                 Value = fl.Led_Id.ToString(),
                                                 Text = fl.Led_Name
                                             }).ToListAsync();
                    if (ledgerList1 != null) ledgerList = ledgerList1;
                }
                else
                {
                    var ledgerList2 = await (from fl in CSISContext.Fin_Ledger
                                             join flg in CSISContext.Fin_Ledger_Grp
                                             on fl.Grp_Id equals flg.Grp_Id
                                             where fl.Led_Delete == false
                                                   && flg.Fnl_Id == fnlId
                                                   && flg.Grp_Delete == false
                                                   && fl.Led_Id != cashLedId
                                                   && fl.BrCode == brCode
                                                   && flg.BrCode == brCode
                                             select new DropdownItem
                                             {
                                                 Value = fl.Led_Id.ToString(),
                                                 Text = fl.Led_Name
                                             }).ToListAsync();
                    if (ledgerList2 != null) ledgerList = ledgerList2;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching ledger items by final ledger id");
            }
            return ledgerList;
        }

        public async Task<List<Fin_Ledger>> GetLedgerList10Async(string brCode)
        {
            List<Fin_Ledger> ledgerList = new List<Fin_Ledger>();
            try
            {
                ledgerList = await CSISContext.Fin_Ledger
                    .Where(x => x.Led_Delete == false && x.BrCode == brCode)
                    .OrderByDescending(x => x.Led_Id)  // or x.CreatedDate, x.Led_Id
                    .Take(10)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching ledger list by group id");
            }
            return ledgerList;
        }
    }
}
