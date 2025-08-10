using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class StagingDetailsRepository : Repository<Staging_Details>, IStagingDetailsRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        private static readonly string[] sourceArray = new[] { "A", "R" };

        public StagingDetailsRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddStagingDetails(Staging_Details stagingDetails, decimal stagingId)
        {
            bool result = false;
            await using var transaction = await CSISContext.Database.BeginTransactionAsync();
            try
            {
                decimal maxId = await CSISContext.Staging_Details
                .Where(x => x.BrCode == stagingDetails.BrCode)
                .MaxAsync(x => (decimal?)x.Id) ?? 0;
                if (maxId == 0)
                {
                    maxId = Convert.ToDecimal(stagingDetails.BrCode) * 10000000 + 1;
                }
                else
                {
                    maxId++;
                }
                stagingDetails.Id = maxId;
                stagingDetails.Staging_Id = stagingId;
                // Ensure proper JSONB serialization
                if (stagingDetails.Related_Account_Data != null || !string.IsNullOrWhiteSpace(stagingDetails.Related_Account_Data))
                {
                    //stagingDetails.Related_Account_Data = JsonDocument.Parse(stagingDetails.Related_Account_Data).Dispose();
                    JsonDocument.Parse(stagingDetails.Related_Account_Data).Dispose();
                }

                await CSISContext.Staging_Details.AddAsync(stagingDetails);
                await CSISContext.SaveChangesAsync();
                await transaction.CommitAsync();
                //await AddAsync(stagingDetails);
                result = true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result = false;
                Console.WriteLine($"Error: {ex.ToString()}");
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while adding new Staging Details");
            }
            return result;
        }

        public async Task<bool> DeleteStagingDetailsByStagingId(decimal stagingId,int relateAccountId)
        {
            bool result = false;
            try
            {
                await CSISContext.Staging_Details
                .Where(x => x.Staging_Id == stagingId && x.Related_Account_Id == relateAccountId)
                .ExecuteDeleteAsync();
                await CSISContext.SaveChangesAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while deleting Staging Details");
            }
            return result;
        }

        public async Task<int> VerifyStagingIdExistinsInStagingDetails(decimal stagingId)
        {
            var count = await CSISContext.Staging_Details
            .Where(x => x.Staging_Id == stagingId)
            .CountAsync();
            return count;
        }

        //public async Task<string> GetAccountType(decimal createdBy, decimal memId, string stagingStatus, DateTime createdDate,string brCode)
        public async Task<string> GetAccountType(int accId)
        {
            string accountType = "";
            try
            {
                //var query = await  (CSISContext.Account_Transactions
                //.Join(CSISContext.Staging_Details,
                //    at => at.Acc_Id,
                //    sd => sd.Related_Account_Id,
                //    (at, sd) => new { at, sd })
                //.Where(x => x.sd.Member_Id == memId
                //    && x.sd.Created_By == createdBy
                //    && x.sd.Created_Date == createdDate
                //    && x.sd.BrCode == brCode
                //    && x.sd.Staging_Status == stagingStatus )
                //.Select(x => x.at.Acc_BelongsTo)
                //.Distinct())
                //.ToListAsync();
                //var query = await CSISContext.Account_Transactions
                //    .Where(x => x.Acc_Id == accId)
                //    .Select(x => x.Acc_BelongsTo)
                //    .Distinct()
                //    .ToListAsync();
                var query = await CSISContext.Account_Transactions
                    .Where(x => x.Acc_Id == accId)
                    .Select(x => x.Acc_BelongsTo)
                    .FirstAsync();
                if (query !=null)
                {
                    //string type = query.First();
                    switch (query!.Trim())
                    {
                        case "M":
                            accountType = "Member Transaction";
                            break;
                        case "S":
                            accountType = "Staff Transaction";
                            break;
                        default:
                            accountType = "Account Transaction";
                            break;
                    }
                }
                else
                {
                    accountType = "Account Transaction";
                }
            }
            catch (Exception)
            {
                accountType = "" ;
            }
            return accountType;
        }
        public Task<List<Staging_Details>> GetAllStagingDetails(decimal createdBy, decimal memId, string stagingStatus, DateTime createdDate)
        {
            throw new NotImplementedException();
        }

        public async Task<List<AccountTransactionVM>> GetAccountTransactions(DateTime createdDate, decimal memId, string stagingStatus, string brCode)
        {
            List<AccountTransactionVM> accList = new List<AccountTransactionVM>();
            try
            {
                var result = await (from master in CSISContext.Staging_Master
                                    join details in CSISContext.Staging_Details
                                        on master.Staging_Id equals details.Staging_Id
                                    join account in CSISContext.Account_Transactions
                                        on details.Related_Account_Id equals account.Acc_Id
                                    join mem in CSISContext.mem_master
                                        on master.Member_Id equals mem.mem_id
                                    where master.Created_Date == createdDate &&
                                          master.Staging_Status == stagingStatus &&
                                          master.BrCode == brCode &&
                                          master.Member_Id == memId
                                    select new AccountTransactionVM
                                    {
                                        Staging_Id = master.Staging_Id,
                                        Member_Id = master.Member_Id,
                                        Member_No = mem.memberno,
                                        PerNo = mem.perno,
                                        Member_Name = mem.membername,
                                        Created_Date = master.Created_Date,
                                        Created_By = master.Created_By,
                                        LedId = details.Ledger_Id,
                                        AccountId = details.Related_Account_Id,
                                        ReceiptAmount = details.Receipt_Amount,
                                        PaymentAmount = details.Payment_Amount,
                                        CashAdjId = details.Cash_Or_Adjustment,
                                        AccountName = account.Acc_Name,
                                        ChequeNo = details.Cheque_No,
                                        ChequeDate = details.Cheque_Date,
                                        IssueBankName = details.Issue_Bank_Name,
                                        Status = account.Acc_Status,
                                        CashReceipt = details.Cash_Or_Adjustment == 1 ? details.Receipt_Amount : 0,
                                        CashPayment = details.Cash_Or_Adjustment == 1 ? details.Payment_Amount : 0
                                    }).ToListAsync();
                if (result != null && result.Count > 0) accList = result.ToList();
            }
            catch (Exception)
            {

                throw;
            }
            return accList;
        }

        public async Task<List<AccountTransactionVM>> GetChekerDashboardById(decimal stagingId)
        {
            List<AccountTransactionVM> accList = new List<AccountTransactionVM>();
            try
            {
                var result = await (from master in CSISContext.Staging_Master
                                    join details in CSISContext.Staging_Details
                                        on master.Staging_Id equals details.Staging_Id
                                    join account in CSISContext.Account_Transactions
                                        on details.Related_Account_Id equals account.Acc_Id
                                    join mem in CSISContext.mem_master
                                        on master.Member_Id equals mem.mem_id
                                    where master.Staging_Id ==stagingId 
                                    select new AccountTransactionVM
                                    {
                                        Staging_Id = master.Staging_Id,
                                        Member_Id = master.Member_Id,
                                        Member_No = mem.memberno,
                                        PerNo = mem.perno,
                                        Member_Name = mem.membername,
                                        Created_Date = master.Created_Date,
                                        Created_By = master.Created_By,
                                        LedId = details.Ledger_Id,
                                        AccountId = details.Related_Account_Id,
                                        ReceiptAmount = details.Receipt_Amount,
                                        PaymentAmount = details.Payment_Amount,
                                        CashAdjId = details.Cash_Or_Adjustment,
                                        AccountName = account.Acc_Name,
                                        ChequeNo = details.Cheque_No,
                                        ChequeDate = details.Cheque_Date,
                                        IssueBankName = details.Issue_Bank_Name,
                                        Status = account.Acc_Status,
                                        CashReceipt = details.Cash_Or_Adjustment == 1 ? details.Receipt_Amount : 0,
                                        CashPayment = details.Cash_Or_Adjustment == 1 ? details.Payment_Amount : 0
                                    }).ToListAsync();
                if (result != null && result.Count > 0) accList = result.ToList();
            }
            catch (Exception)
            {

                throw;
            }
            return accList;
        }
        public async Task<List<DtoCheckerDashboard>> GetCheckerDashboard(DateTime createdDate,  string stagingStatus, string brCode)
        {
            List<DtoCheckerDashboard> checkersDashboardList = new List<DtoCheckerDashboard>();
            try
            {
                var result = await  (from sm in CSISContext.Staging_Master
                              join sd in CSISContext.Staging_Details on sm.Staging_Id equals sd.Staging_Id
                              join mm in CSISContext.mem_master on sm.Member_Id equals mm.mem_id
                              join u in CSISContext.Users on sm.Created_By equals u.id
                              where sm.Created_Date == createdDate
                                  && sm.Staging_Status == stagingStatus 
                                  && sm.BrCode == brCode
                              group new { sm, sd, mm, u } by new
                              {
                                  sm.Staging_Id,
                                  sm.Member_Id,
                                  mm.memberno,
                                  mm.perno,
                                  mm.membername,
                                  sm.Type,
                                  sm.Created_By,
                                  sm.Created_Date,
                                  u.username
                              } into g
                              select new DtoCheckerDashboard
                              {
                                 Staging_Id =  g.Key.Staging_Id,
                                 Member_Id =  g.Key.Member_Id,
                                 Member_No =  g.Key.memberno,
                                 PerNo =  g.Key.perno,
                                 Member_Name =  g.Key.membername,
                                 Type =  g.Key.Type,
                                 Created_By =  g.Key.Created_By,
                                 Created_Date = g.Key.Created_Date,
                                 Created_By_Name =  g.Key.username,
                                 Receipt_Amount =   g.Sum(x => x.sd.Receipt_Amount),
                                 Payment_Amount =   g.Sum(x => x.sd.Payment_Amount)
                              }).ToListAsync();
                if (result != null && result.Count > 0) checkersDashboardList = result.ToList();
            }
            catch (Exception)
            {
                checkersDashboardList = new();
            }
            return checkersDashboardList;
        }

        public async Task<Staging_Details> GetStagingDetailsById(decimal stagingId, int relatedAccountId)
        {
            Staging_Details details = new Staging_Details();
            try
            {
                var query =await  CSISContext.Staging_Details.Where(x => x.Staging_Id == stagingId && x.Related_Account_Id == relatedAccountId ).FirstAsync();
                details = query ?? new Staging_Details() ;
            }
            catch (Exception)
            {
               details = new();
            }
            return details; 
        }

        public async Task<int> IsAlreadyTransactedButNotVerifiedOrRejected(decimal memId, string transactedDate, int relatedAccountId)
        {
            int resultCount = 0;
            //string[] sourceArray = new[] { "A", "R" };
            DateTime.TryParse(transactedDate, out DateTime transactedDateparsed);
            try
            {
                var count = await  (from master in CSISContext.Staging_Master
                             join details in CSISContext.Staging_Details
                             on master.Staging_Id equals details.Staging_Id
                             where master.Created_Date == transactedDateparsed
                                   && details.Member_Id == memId
                                   && details.Related_Account_Id == relatedAccountId
                                   && !sourceArray.Contains(details.Staging_Status)
                             select details).CountAsync();
                if(count > 0)
                {
                    resultCount = count;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return await Task.FromResult(resultCount);
        }

        public async Task<bool> MakeStagingDetails(decimal stagingId)
        {
            bool result = false;
            List < Staging_Details > stagingDetails = new();
            try
            {
                var  query = await CSISContext.Staging_Details
                .Where(x => x.Staging_Id == stagingId && x.Staging_Status == "I")
                .ToListAsync();
                if(query != null && query.Any())
                {
                    stagingDetails = query.ToList();
                    foreach (var item in stagingDetails)
                    {
                        item.Staging_Status = "M";
                        //item.Checked_Date = DateTime.Now;
                        await CSISContext.SaveChangesAsync();
                    }
                    result = true;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> CheckerStateStaging(decimal stagingId, decimal vocId, decimal checkerBy, string stagingStatus)
        {
            bool result = false;
            List<Staging_Details> stagingDetails = new();
            try
            {
                var query = await CSISContext.Staging_Details
                .Where(x => x.Staging_Id == stagingId && x.Staging_Status == "M")
                .ToListAsync();
                if (query != null && query.Any())
                {
                    stagingDetails = query.ToList();
                    foreach (var item in stagingDetails)
                    {
                        item.Staging_Status = stagingStatus;
                        item.Checked_By = checkerBy;
                        item.Checked_Date = DateTime.Now;
                        item.Voc_Id = vocId;
                        CSISContext.Entry(item).State = EntityState.Modified;
                        //await EditAsync(item);
                        //await CSISContext.SaveChangesAsync();
                    }
                    result = true;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> VerifyForFixedDepositLoanRecovery(int accountId, decimal memId)
        {
            /// account id = 8 fd refund
            /// account id = 9 fd renewal
            /// account id = 24 fd loan recovery
            /// In the above all account id it is possible to make fixed deposit recovery,hence it has tobe arrested.
            List<int> accountIds = new();
            bool result = false;
            try
            {
                switch (accountId)
                {
                    case 8:
                        accountIds.Add(9);
                        accountIds.Add(24);
                        break;
                    case 9:
                        accountIds.Add(8);
                        accountIds.Add(24);
                        break;
                    case 24:
                        accountIds.Add(8);
                        accountIds.Add(9);
                        break;
                }
                var count = await CSISContext.Staging_Details
                         .CountAsync(d=> d.Member_Id == memId &&
                                    accountIds.Contains(d.Related_Account_Id) &&
                                     new[] { "I", "M" }.Contains(d.Staging_Status));
                if(count > 0)  result = true; else result = false;
            }
            catch (Exception)
            {
                result = true;
            }
            return result;
        }
    }
}
