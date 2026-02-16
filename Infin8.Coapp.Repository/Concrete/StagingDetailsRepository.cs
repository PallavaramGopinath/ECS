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
        private static readonly string[] sourceArrayInitiatedOrMacked = new[] { "I", "M" };
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

        public async Task<bool> AddStagingDetailsForAccountTransaciton(List<Staging_Details> stagingDetails, decimal stagingId)
        {
            bool result = false;
            string brCode = stagingDetails.FirstOrDefault()?.BrCode ?? string.Empty;
            await using var transaction = await CSISContext.Database.BeginTransactionAsync();
            try
            {
                decimal maxId = await CSISContext.Staging_Details
                .Where(x => x.BrCode == brCode)
                .MaxAsync(x => (decimal?)x.Id) ?? 0;
                if (maxId == 0)
                {
                    maxId = Convert.ToDecimal(brCode) * 10000000 + 1;
                }
                else
                {
                    maxId++;
                }
                foreach (var stagingDetail in stagingDetails)
                {
                    // Assign the same maxId to all staging details
                    stagingDetail.Id = maxId;
                    stagingDetail.Staging_Id = stagingId;
                    maxId++; // Increment maxId for the next detail
                }
                

                await CSISContext.Staging_Details.AddRangeAsync(stagingDetails);
                await CSISContext.SaveChangesAsync();
                await transaction.CommitAsync();
                result = true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result = false;
                Console.WriteLine($"Error: {ex.ToString()}");
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while adding new Staging Details for account transaction");
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

        public async Task<bool> DeleteStagingDetails(List<Staging_Details> detailsList)
        {
            try
            {
                if (detailsList == null || !detailsList.Any())
                {
                    Console.WriteLine("No staging detals records to delete.");
                    return true;
                }

                // Replace 'Id' with your actual primary key property name
                var idsToDelete = detailsList.Select(m => m.Staging_Id).ToList();

                Console.WriteLine($"Attempting to delete  staging details {idsToDelete.Count} records...");

                var rowsAffected = await CSISContext.Staging_Details
                    .Where(m => idsToDelete.Contains(m.Staging_Id))
                    .ExecuteDeleteAsync();

                Console.WriteLine($"Successfully deleted staging details {rowsAffected} records.");
                return rowsAffected == idsToDelete.Count; // Return true only if all were deleted
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during bulk deletion of staging details: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
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

        public async Task<List<Staging_Details>> GetStagingDetailsByDate(DateTime stagingDate, string brCode)
        {
            List<Staging_Details> detailsList = new();
            try
            {
                var result = await (from detail in CSISContext.Staging_Details
                                    join master in CSISContext.Staging_Master on detail.Staging_Id equals master.Staging_Id
                                    where detail.BrCode == brCode &&
                                    detail.Staging_Status == "V" &&
                                    master.BrCode == brCode &&
                                    master.Staging_Status == "V" &&
                                    master.Created_Date == stagingDate 
                                    select detail).ToListAsync();
                if (result != null && result.Any()) detailsList = result.ToList();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return detailsList;
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
                                        CashPayment = details.Cash_Or_Adjustment == 1 ? details.Payment_Amount : 0,
                                        AdjustmentReceipt = details.Cash_Or_Adjustment == 2 ? details.Receipt_Amount : 0,
                                        AdjustmentPaymnet = details.Cash_Or_Adjustment == 2 ? details.Payment_Amount : 0,
                                    }).ToListAsync();
                if (result != null && result.Count > 0) accList = result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return accList;
        }

        public async Task<List<AccountTransactionVM>> GetChekerDashboardById(decimal stagingId)
        {
            List<AccountTransactionVM> accList = new List<AccountTransactionVM>();
            try
            {
                #region olqd linq
                //var result = await (from master in CSISContext.Staging_Master
                //                    join details in CSISContext.Staging_Details
                //                        on master.Staging_Id equals details.Staging_Id
                //                    join account in CSISContext.Account_Transactions
                //                        on details.Related_Account_Id equals account.Acc_Id
                //                    join mem in CSISContext.mem_master
                //                        on master.Member_Id equals mem.mem_id
                //                    where master.Staging_Id ==stagingId 
                //                    select new AccountTransactionVM
                //                    {
                //                        Staging_Id = master.Staging_Id,
                //                        Member_Id = master.Member_Id,
                //                        Member_No = mem.memberno,
                //                        PerNo = mem.perno,
                //                        Member_Name = mem.membername,
                //                        Created_Date = master.Created_Date,
                //                        Created_By = master.Created_By,
                //                        LedId = details.Ledger_Id,
                //                        AccountId = details.Related_Account_Id,
                //                        ReceiptAmount = details.Receipt_Amount,
                //                        PaymentAmount = details.Payment_Amount,
                //                        CashAdjId = details.Cash_Or_Adjustment,
                //                        AccountName = account.Acc_Name,
                //                        ChequeNo = details.Cheque_No,
                //                        ChequeDate = details.Cheque_Date,
                //                        IssueBankName = details.Issue_Bank_Name,
                //                        Status = account.Acc_Status,
                //                        CashReceipt = details.Cash_Or_Adjustment == 1 ? details.Receipt_Amount : 0,
                //                        CashPayment = details.Cash_Or_Adjustment == 1 ? details.Payment_Amount : 0
                //                    }).ToListAsync();
                //if (result != null && result.Count > 0) accList = result.ToList();
                #endregion

                #region new linq with left join member master for account transaction
                var result = await (from master in CSISContext.Staging_Master
                                    join details in CSISContext.Staging_Details
                                        on master.Staging_Id equals details.Staging_Id
                                    join account in CSISContext.Account_Transactions
                                        on details.Related_Account_Id equals account.Acc_Id
                                    join mem in CSISContext.mem_master
                                        on master.Member_Id equals mem.mem_id into memGroup
                                    from mem in memGroup.DefaultIfEmpty() // This creates the left join
                                    where master.Staging_Id == stagingId
                                    select new AccountTransactionVM
                                    {
                                        Staging_Id = master.Staging_Id,
                                        Member_Id = master.Member_Id,
                                        Member_No = mem != null ? mem.memberno : null,
                                        PerNo = mem != null ? mem.perno : null,
                                        Member_Name = mem != null ? mem.membername : null,
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

                if (result != null && result.Any())
                    accList = result.ToList();
                #endregion 
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
                #region old linq
                //var result = await  (from sm in CSISContext.Staging_Master
                //              join sd in CSISContext.Staging_Details on sm.Staging_Id equals sd.Staging_Id
                //              join mm in CSISContext.mem_master on sm.Member_Id equals mm.mem_id
                //              join u in CSISContext.Users on sm.Created_By equals u.id
                //              where sm.Created_Date == createdDate
                //                  && sm.Staging_Status == stagingStatus 
                //                  && sm.BrCode == brCode
                //              group new { sm, sd, mm, u } by new
                //              {
                //                  sm.Staging_Id,
                //                  sm.Member_Id,
                //                  mm.memberno,
                //                  mm.perno,
                //                  mm.membername,
                //                  sm.Type,
                //                  sm.Created_By,
                //                  sm.Created_Date,
                //                  u.username
                //              } into g
                //              select new DtoCheckerDashboard
                //              {
                //                 Staging_Id =  g.Key.Staging_Id,
                //                 Member_Id =  g.Key.Member_Id,
                //                 Member_No =  g.Key.memberno,
                //                 PerNo =  g.Key.perno,
                //                 Member_Name =  g.Key.membername,
                //                 Type =  g.Key.Type,
                //                 Created_By =  g.Key.Created_By,
                //                 Created_Date = g.Key.Created_Date,
                //                 Created_By_Name =  g.Key.username,
                //                 Receipt_Amount =   g.Sum(x => x.sd.Receipt_Amount),
                //                 Payment_Amount =   g.Sum(x => x.sd.Payment_Amount)
                //              }).ToListAsync();
                //if (result != null && result.Count > 0) checkersDashboardList = result.ToList();
                #endregion

                #region new linq with left join member master for account transaction
                // Using left join to include member details even if they are not present in mem_master
                var result = await (from sm in CSISContext.Staging_Master
                                    join sd in CSISContext.Staging_Details on sm.Staging_Id equals sd.Staging_Id
                                    join mm in CSISContext.mem_master on sm.Member_Id equals mm.mem_id into mmGroup
                                    from mm in mmGroup.DefaultIfEmpty() // This creates the left join
                                    join u in CSISContext.Users on sm.Created_By equals u.Id
                                    where sm.Created_Date == createdDate
                                        && sm.Staging_Status == stagingStatus
                                        && sm.BrCode == brCode
                                    group new { sm, sd, mm, u } by new
                                    {
                                        sm.Staging_Id,
                                        sm.Member_Id,
                                        memberno = mm != null ? mm.memberno : null,
                                        perno = mm != null ? mm.perno : null,
                                        membername = mm != null ? mm.membername : null,
                                        sm.Type,
                                        sm.Created_By,
                                        sm.Created_Date,
                                        u.Username
                                    } into g
                                    select new DtoCheckerDashboard
                                    {
                                        Staging_Id = g.Key.Staging_Id,
                                        Member_Id = g.Key.Member_Id,
                                        Member_No = g.Key.memberno,
                                        PerNo = g.Key.perno,
                                        Member_Name = g.Key.membername,
                                        Type = g.Key.Type,
                                        Created_By = g.Key.Created_By,
                                        Created_Date = g.Key.Created_Date,
                                        Created_By_Name = g.Key.Username,
                                        Receipt_Amount = g.Sum(x => x.sd.Receipt_Amount),
                                        Payment_Amount = g.Sum(x => x.sd.Payment_Amount)
                                    }).ToListAsync();

                if (result != null && result.Any())
                    checkersDashboardList = result.ToList();
                #endregion 
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
        public async Task<List<DtoAccountTransactionRelatedData>> GetStagingDetailsListById(decimal stagingId)
        {
            //List<Staging_Details> details = new();
            List<DtoAccountTransactionRelatedData> accRelatedDataList = new();
            DtoAccountTransactionRelatedData accRelatedData = new();
            decimal cashLedId = 0;
            try
            {
                
                //var query = await CSISContext.Staging_Details.Where(x => x.Staging_Id == stagingId).ToListAsync();
                //if(query != null && query.Any())    details = query.ToList();

                var query2 = await (from sd in CSISContext.Staging_Details
                                    join fl in CSISContext.Fin_Ledger
                                        on sd.Ledger_Id equals fl.Led_Id
                                    join flg in CSISContext.Fin_Ledger_Grp
                                        on fl.Grp_Id equals flg.Grp_Id
                                    where sd.Staging_Id == stagingId
                                    select new
                                    {
                                        Fnl_Id = flg.Fnl_Id,
                                        Ledger_Id = sd.Ledger_Id,
                                        Receipt_Amount = sd.Receipt_Amount,
                                        Payment_Amount = sd.Payment_Amount,
                                        Cash_Or_Adjustment = sd.Cash_Or_Adjustment,
                                        Related_Account_Data = sd.Related_Account_Data,
                                        BrCode = sd.BrCode
                                    }).ToListAsync();
                if (query2 != null && query2.Any())
                {
                    cashLedId = await CSISContext.Map_General.Where(x => x.BrCode == query2[0].BrCode).Select(x => x.Cash_Led_Id).FirstOrDefaultAsync();
                    foreach (var item in query2)
                    {
                        accRelatedData = Utility.JsonbObject.ConvertFromJsonForAccountTransactionRelatedData(item.Related_Account_Data);
                        switch (item.Fnl_Id)
                        {
                            case 1:
                            case 4:
                                if (item.Ledger_Id == cashLedId)
                                {
                                    accRelatedData.CB = accRelatedData.OB + item.Receipt_Amount + item.Payment_Amount;
                                }
                                else
                                {
                                    accRelatedData.CB = accRelatedData.OB + item.Payment_Amount - item.Receipt_Amount;
                                }
                                break;
                            case 2:
                            case 3:
                                accRelatedData.CB = accRelatedData.OB + item.Receipt_Amount - item.Payment_Amount;
                                break;
                        }
                        DtoAccountTransactionRelatedData dto = new()
                        {
                            Fnl_Id = item.Fnl_Id,
                            Ledger_Name = accRelatedData.Ledger_Name ,
                            OB = accRelatedData.OB, // Assuming OB is not available in the query
                            Receipt_Amount = item.Receipt_Amount,
                            Payment_Amount = item.Payment_Amount,
                            CB = accRelatedData.CB  // Assuming CB is not available in the query
                        };
                        accRelatedDataList.Add(dto);
                    }
                }
            }
            catch (Exception ex)
            {
                accRelatedDataList = new();
                Console.Write(ex.ToString());
            }
            return accRelatedDataList;
        }
        public async Task<int> IsAlreadyTransactedButNotVerifiedOrRejected(decimal memId, string transactedDate, int relatedAccountId)
        {
            int resultCount = 0;
            //string[] sourceArrayInitiatedOrMacked = new[] { "I", "M" };
            DateTime.TryParse(transactedDate, out DateTime transactedDateparsed);
            try
            {
                var count = await  (from master in CSISContext.Staging_Master
                             join details in CSISContext.Staging_Details
                             on master.Staging_Id equals details.Staging_Id
                             where master.Created_Date == transactedDateparsed
                                   && details.Member_Id == memId
                                   && details.Related_Account_Id == relatedAccountId
                                   && sourceArrayInitiatedOrMacked.Contains(details.Staging_Status)
                                   && sourceArrayInitiatedOrMacked.Contains(master.Staging_Status)
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
                        item.Checked_Date = item.Created_Date;
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

        public async Task<bool> VerifyForFixedDepositLoanRecovery(int accountId, decimal memId, DateTime createdDate)
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
                //var count = await CSISContext.Staging_Details
                //         .CountAsync(d=> d.Member_Id == memId &&
                //                    accountIds.Contains(d.Related_Account_Id) &&
                //                     new[] { "I", "M" }.Contains(d.Staging_Status));
                var count = await (from details in CSISContext.Staging_Details
                             join master in CSISContext.Staging_Master on details.Staging_Id equals master.Staging_Id
                             where accountIds.Contains(details.Related_Account_Id)
                             && new[] { "I", "M" }.Contains(details.Staging_Status)
                             && details.Member_Id == memId
                             && master.Created_Date == createdDate
                             select details).CountAsync();
                         
                if (count > 0)  result = true; else result = false;
            }
            catch (Exception)
            {
                result = true;
            }
            return result;
        }
    }
}
