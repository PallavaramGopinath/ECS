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
    public class TransactionsRepository : Repository<Account_Transactions>, ITransactionsRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public TransactionsRepository(CSISContext context) : base(context)
        {
        }

        public async Task<List<DropdownItem>> GetTransactionAccounts(string accountStatus, string accountBelongTo)
        {
            List<DropdownItem> trnList = new List<DropdownItem>();
            var accTypes = new List<string> { "C", accountStatus };
            var belongsToList = new List<string> { "B", accountBelongTo };
            try
            {
                trnList = await  CSISContext.Account_Transactions
                .Where(a => !a.Acc_Delete &&
                            accTypes.Contains(a.Acc_Type) &&
                            belongsToList.Contains(a.Acc_BelongsTo))
                .Select(a => new DropdownItem
                {
                    Value = a.Acc_Id.ToString(),
                    Text = a.Acc_Name
                })
                .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return trnList;
        }

        public async Task<List<DtoAccount_Transactions>> GetAllAccountsTransactions()
        {
            return await (from trn in CSISContext.Account_Transactions
                          where trn.Acc_Delete == false
                          select new DtoAccount_Transactions
                          {
                              Acc_Id = trn.Acc_Id,
                              Acc_Name = trn.Acc_Name,
                              Acc_Status = trn.Acc_Status,
                              Acc_Type = trn.Acc_Type,
                              Component_Name = trn.Component_Name,
                          }).ToListAsync();
        }
        public async Task<List<DropdownItem>> GetSuspenseLedgerItems(int suspeneType,string brCode)
        {
            List<DropdownItem> trnList = new List<DropdownItem>();
            try
            {
                var result = await ( from map in   CSISContext.Map_SuspenseAccounts
                                  join led in CSISContext.Fin_Ledger 
                                    on map.Led_Id equals led.Led_Id 
                                where map.Sus_Type == suspeneType   
                                && map.BrCode == brCode
                                select new DropdownItem
                                {
                                    Value = map.Led_Id.ToString(),  
                                    Text = led.Led_Name 
                                }).ToListAsync();
                if (result != null) trnList = result;
            }
            catch (Exception)
            {
                throw;
            }
            return trnList;
        }

        public async Task<List<DropdownItem>> GetShareCapitalLedgerItem(string brCode)
        {
            List<DropdownItem> trnList = new List<DropdownItem>();
            try
            {
                var result = await ( from map in  CSISContext.Map_General
                                     join led in CSISContext.Fin_Ledger 
                                     on map.ShareCapital_Led_Id equals led.Led_Id 
                                     where map.BrCode == brCode
                                     select new DropdownItem
                                     {
                                         Value = map.ShareCapital_Led_Id .ToString(),
                                         Text = led.Led_Name 
                                     }).ToListAsync ();
                if (result != null) trnList = result;
            }
            catch (Exception)
            {
                throw;
            }
            return trnList;
        }

        public async Task<List<DropdownItem>> GetBankLedgerItems(string brCode)
        {
            List<DropdownItem> trnList = new List<DropdownItem>();
            try
            {
                var result = await(from map in CSISContext.Map_Banks
                                   join led in CSISContext.Fin_Ledger
                                   on map.Led_Id equals led.Led_Id
                                   where map.BrCode == brCode
                                   select new DropdownItem
                                   {
                                       Value = map.Led_Id.ToString(),
                                       Text = led.Led_Name
                                   }).ToListAsync();
                if (result != null) trnList = result;
            }
            catch (Exception)
            {
                throw;
            }
            return trnList;
        }

        public async Task<List<DropdownItem>> GetLedgersExpectBankLedgerItems(string brCode)
        {
            List<DropdownItem> trnList = new List<DropdownItem>();
            try
            {
                var result = await  CSISContext.Fin_Ledger
                .Where(fl => fl.Led_Delete == false &&
                       !CSISContext.Map_General.Select(m => m.Cash_Led_Id)
                            .Union(CSISContext.Map_Banks.Select(m => m.Led_Id))
                            .Contains(fl.Led_Id))
                .Select(fl => new DropdownItem
                {
                    Value = fl.Led_Id.ToString(),
                    Text = fl.Led_Name
                })
                .ToListAsync();
                if (result != null) trnList = result;
            }
            catch (Exception)
            {
                throw;
            }
            return trnList;
        }

        public async Task<string> GetComponentName(int accountId)
        {
            string componentName = "";
            try
            {
                var result = await CSISContext.Account_Transactions.Where(x => x.Acc_Id == accountId).Select(x => x.Component_Name).FirstOrDefaultAsync();
                if (result != null) componentName = result;
                
            }
            catch (Exception)
            {
                throw;
            }
            return componentName;
        }
    }
}
