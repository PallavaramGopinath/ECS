using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public  class StagingBalanceRepository : Repository<Staging_Balance>,  IStagingBalanceRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public StagingBalanceRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<DtoAccountsBalance>> GetStagingBalance(decimal yrId, DateTime accountingDate, decimal created_By, string brCode)
        {
            //bool result = true;
            List<DtoAccountsBalance> balanceList = new();
            List<Staging_Balance > stagingBalanceList = new();
            try
            {
                var response = await  (from trn in CSISContext.Fin_Ledger_Trn 
                               join ledger in CSISContext.Fin_Ledger on trn.Led_Id equals ledger.Led_Id 
                               join grp in CSISContext.Fin_Ledger_Grp on ledger.Grp_Id equals grp.Grp_Id
                               join fnl in CSISContext.Fin_Ledger_Fnl on  grp.Fnl_Id equals fnl.Fnl_Id
                               where trn.Yr_Id == yrId
                               && trn.LedgerTrn_Delete == false
                               && trn.BrCode == brCode  
                               select new DtoAccountsBalance
                               {
                                   Ledger_Id = trn.Led_Id,
                                   Grp_Id = grp.Grp_Id,
                                   Fnl_Id = fnl.Fnl_Id,
                                   Ledger_Name = ledger.Led_Name,
                                   Group_Name = grp.Grp_Name,
                                   Final_Name = fnl.Fnl_Name,
                                   Accounting_Date = accountingDate,
                                   Opening_Balance = trn.OB_Amt ,
                                   Receipt = trn.Tot_Rpt_Amt ,
                                   Payment =trn.Tot_Pmt_Amt ,
                                   Closing_Balance = trn.CB_Amt ,
                                   Yr_Id = trn.Yr_Id 
                               }).ToListAsync();
                if(response != null && response.Any())
                {
                    balanceList = response.ToList();
                }
                //if(balanceList != null && balanceList.Any())
                //{
                //    foreach(var bal in balanceList )
                //    {
                //        Staging_Balance stgBal = new Staging_Balance()
                //        {
                //            Id = 0,
                //            Ledger_Id = bal.Ledger_Id,
                //            Fnl_Id = bal.Fnl_Id,
                //            Accounting_Date = accountingDate,
                //            Opening_Balance = bal.Opening_Balance,
                //            Receipt = bal.Receipt,
                //            Payment = bal.Payment,
                //            Closing_Balance = bal.Closing_Balance,
                //            Yr_Id = bal.Yr_Id,
                //            Created_By = created_By ,
                //            BrCode = brCode 
                //        };
                //        stagingBalanceList.Add( stgBal );
                //    }
                //    //CSISContext.Staging_Balance.AddRange(stagingBalanceList);
                //    await CSISContext.Staging_Balance.AddRangeAsync(stagingBalanceList);
                //    await CSISContext.SaveChangesAsync();
                //}
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return balanceList;
        }

        public async Task<bool> AddStagingBalance(decimal yrId, DateTime accountingDate, decimal created_By, string brCode)
        {
            bool result = true;
            List<DtoAccountsBalance> balanceList = new();
            List<Staging_Balance> stagingBalanceList = new();
            try
            {
                var response = await(from trn in CSISContext.Fin_Ledger_Trn
                                     join ledger in CSISContext.Fin_Ledger on trn.Led_Id equals ledger.Led_Id
                                     join grp in CSISContext.Fin_Ledger_Grp on ledger.Grp_Id equals grp.Grp_Id
                                     join fnl in CSISContext.Fin_Ledger_Fnl on grp.Fnl_Id equals fnl.Fnl_Id
                                     where trn.Yr_Id == yrId
                                     && trn.LedgerTrn_Delete == false
                                     && trn.BrCode == brCode
                                     select new DtoAccountsBalance
                                     {
                                         Ledger_Id = trn.Led_Id,
                                         Grp_Id = grp.Grp_Id,
                                         Fnl_Id = fnl.Fnl_Id,
                                         Ledger_Name = ledger.Led_Name,
                                         Group_Name = grp.Grp_Name,
                                         Final_Name = fnl.Fnl_Name,
                                         Accounting_Date = accountingDate,
                                         Opening_Balance = trn.OB_Amt,
                                         Receipt = trn.Tot_Rpt_Amt,
                                         Payment = trn.Tot_Pmt_Amt,
                                         Closing_Balance = trn.CB_Amt,
                                         Yr_Id = trn.Yr_Id
                                     }).ToListAsync();
                if (response != null && response.Any())
                {
                    balanceList = response.ToList();
                }
                var maxBalanceId = await CSISContext.Staging_Balance.AnyAsync()
                   ? await CSISContext.Staging_Balance.MaxAsync(lb => lb.Id)
                   : 0;
                maxBalanceId++;
                if (balanceList != null && balanceList.Any())
                {
                    foreach (var bal in balanceList)
                    {
                        Staging_Balance stgBal = new Staging_Balance()
                        {
                            Id = maxBalanceId,
                            Ledger_Id = bal.Ledger_Id,
                            Fnl_Id = bal.Fnl_Id,
                            Accounting_Date = accountingDate,
                            Opening_Balance = bal.Opening_Balance,
                            Receipt = bal.Receipt,
                            Payment = bal.Payment,
                            Closing_Balance = bal.Closing_Balance,
                            Yr_Id = bal.Yr_Id,
                            Created_By = created_By,
                            BrCode = brCode
                        };
                        maxBalanceId++;
                        stagingBalanceList.Add(stgBal);
                    }
                    //CSISContext.Staging_Balance.AddRange(stagingBalanceList);
                    await CSISContext.Staging_Balance.AddRangeAsync(stagingBalanceList);
                    await CSISContext.SaveChangesAsync();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                result = false;
            }
            return result;
        }
    }
}
