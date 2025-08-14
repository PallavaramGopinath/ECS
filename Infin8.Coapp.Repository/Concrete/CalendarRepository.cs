using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infin8.Coapp.Repository
{
    public class CalendarRepository : Repository<Business_Day>, ICalendarRepository
    {
        /// <summary>
        /// Calendar_Status N=> Day not processed, B=> Day Begin, E=> Day End, H=> Holiday Date
        /// </summary>
        public CSISContext CSISContext => (CSISContext)Context;
        public CalendarRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> VerifyDayBegin(string brCode)
        {
            bool result = false;
            var verifyDate = await CSISContext.Business_Day
                            .Where(x => x.Calendar_Status == "N" && x.BrCode == brCode)
                            .CountAsync();
            if (verifyDate > 0) { result = true; }
            return result;
        }
        public async Task<DateTime> GetCurrentDate(string brCode)
        {
            DateTime currentDate;
            try
            {
                currentDate = await CSISContext.Business_Day
                    .Where(c => c.Calendar_Status == "N" && c.BrCode == brCode)
                    .OrderBy(c => c.Calendar_Id)
                    .Select(c => c.Calendar_Date)
                    .FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return currentDate;
        }
        public async Task<int> UpdateCalendarStatus(DateTime currentDate, string brCode, string newStatus)
        {
            return await CSISContext.Business_Day
                .Where(b => b.Calendar_Date == currentDate && b.BrCode == brCode)
                .ExecuteUpdateAsync(b => b.SetProperty(x => x.Calendar_Status, newStatus));
        }


        public async Task<bool> CanBeginDay(string brCode)
        {
            bool result = false;
            try
            {
                var query = await CSISContext.Business_Day.Where(x => x.Calendar_Status == "B").CountAsync();
                if (query > 0) result = true; else result = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
        }

        public async Task<bool> DayEndProcess(string brCode)
        {
            /// grok suggestion
            bool result = false;
            decimal cashLedId = CSISContext.Map_General.Where(x => x.BrCode == brCode).Select(x => x.Cash_Led_Id).FirstOrDefault();
            List<Staging_Details> stagingDetailsList = new();
            try
            {

                /// update business_day 'B' with current day 'E'
                var recordsToUpdate = from bd in CSISContext.Business_Day
                                      where bd.BrCode == brCode && bd.Calendar_Status == ((char)Status.DayBegin).ToString()
                                      select bd;

                foreach (var record in recordsToUpdate)
                {
                    record.Calendar_Status = ((char)Status.DayEnd).ToString();
                }

                CSISContext.SaveChanges();

                // Step 1: Retrieve all records from staging_details
                var stagingDetails = await (from sd in CSISContext.Staging_Details
                                            select sd).ToListAsync();
                if (!stagingDetails.Any())
                {
                    //await transaction.CommitAsync();
                    return false; // No records to process
                }

                /// push staging_details to staging_history
                // Step 2: Get the maximum ID from staging_history to generate new IDs
                var maxId = await CSISContext.Staging_History.AnyAsync()
                    ? await CSISContext.Staging_History.MaxAsync(sh => sh.Id)
                    : 0;
                #region  Step 3: Create new staging_history records with new IDs
                var stagingHistories = stagingDetails.Select((sd, index) => new Staging_History
                {
                    Id = maxId + index + 1, // Generate new unique ID
                    Staging_Id = sd.Staging_Id,
                    Member_Id = sd.Member_Id,
                    Ledger_Id = sd.Ledger_Id,
                    Related_Account_Id = sd.Related_Account_Id,
                    Receipt_Amount = sd.Receipt_Amount,
                    Payment_Amount = sd.Payment_Amount,
                    Module_Name = sd.Module_Name,
                    Cash_Or_Adjustment = sd.Cash_Or_Adjustment,
                    Related_Account_Data = sd.Related_Account_Data,
                    Created_By = sd.Created_By,
                    Created_Date = sd.Created_Date,
                    Checked_By = sd.Checked_By,
                    Checked_Date = sd.Checked_Date,
                    Staging_Status = sd.Staging_Status,
                    BrCode = sd.BrCode,
                    Cheque_No = sd.Cheque_No,
                    Cheque_Date = sd.Cheque_Date,
                    Issue_Bank_Name = sd.Issue_Bank_Name,
                    Voc_Id = sd.Voc_Id
                }).ToList();
                #endregion 

                // Step 4: Add records to staging_history
                CSISContext.Staging_History.AddRange(stagingHistories);
                // Step 5: Delete all records from staging_details
                CSISContext.Staging_Details.RemoveRange(stagingDetails);
                // Step 6: Save changes
                await CSISContext.SaveChangesAsync();

                #region  Step 7: Calculate balance changes per ledger_id and brcode
                var balanceChanges = await (from sd in CSISContext.Staging_Details
                                            where sd.Staging_Status == "V"
                                            group sd by new { sd.Ledger_Id, sd.BrCode } into g
                                            select new
                                            {
                                                LedgerId = g.Key.Ledger_Id,
                                                Brcode = g.Key.BrCode,
                                                Receipt_Amount = g.Sum(x => x.Receipt_Amount),
                                                Payment_Amount = g.Sum(x => x.Payment_Amount),
                                                NetAmount = g.Sum(x => x.Receipt_Amount - x.Payment_Amount)
                                            }).ToListAsync();
                #endregion

                #region  Step 8: Get the maximum ID from ledger_balances for new ID generation
                var maxBalanceId = await CSISContext.Staging_Balance.AnyAsync()
                    ? await CSISContext.Staging_Balance.MaxAsync(lb => lb.Id)
                    : 0;
                // Step 9: Update or insert ledger balances
                var ledgerBalances = new List<Staging_Balance>();
                var createdBy = 110010000001;
                var currentDate = DateTime.Today;
                #endregion 

                #region Step 9: Update Staging_Balance table
                /// Fetch staging details where staging_status ='V'

                #region  Iterate non-linked accounts commented
                //double balanceAmt = 0;
                //foreach (var change in balanceChanges)
                //{
                //    // Check if a balance record exists for this ledger_id, brcode, and date

                //    var existingBalance = await CSISContext.Staging_Balance
                //        .FirstOrDefaultAsync(lb => lb.Ledger_Id == change.LedgerId &&
                //                                  lb.BrCode == change.Brcode &&
                //                                  lb.Balance_Date == currentDate);


                //    if (existingBalance != null)
                //    {
                //        switch (existingBalance.Fnl_Id )
                //        {
                //            case 1:
                //            case 4:
                //                if (existingBalance.Ledger_Id == cashLedId)
                //                {
                //                    existingBalance.Balance_Amount += change.Receipt_Amount - change.Payment_Amount; /// rptAmt - pmtAmt;
                //                }
                //                else
                //                {
                //                    existingBalance.Balance_Amount += change.Payment_Amount - change.Receipt_Amount; ///  pmtAmt - rptAmt;
                //                }
                //                break;
                //            case 2:
                //            case 3:
                //                existingBalance.Balance_Amount += change.Receipt_Amount - change.Payment_Amount; /// rptAmt - pmtAmt;
                //                break;
                //        }
                //        // Update existing balance
                //        //existingBalance.Balance_Amount += change.NetAmount;
                //        existingBalance.Created_By = createdBy;
                //        existingBalance.Created_Date = DateTime.Now;
                //    }
                //    else
                //    {
                //        balanceAmt = 0;
                //        var fnlId = (from fl in CSISContext.Fin_Ledger
                //                     join fg in CSISContext.Fin_Ledger_Grp
                //                         on fl.Grp_Id equals fg.Grp_Id
                //                     where fl.Led_Id == change.LedgerId
                //                     select fg.Fnl_Id).FirstOrDefault();
                //        switch (fnlId)
                //        {
                //            case 1:
                //            case 4:
                //                if (change.LedgerId == cashLedId)
                //                {
                //                    balanceAmt = change.Receipt_Amount - change.Payment_Amount; /// rptAmt - pmtAmt;
                //                }
                //                else
                //                {
                //                    balanceAmt += change.Payment_Amount - change.Receipt_Amount; ///  pmtAmt - rptAmt;
                //                }
                //                break;
                //            case 2:
                //            case 3:
                //                balanceAmt += change.Receipt_Amount - change.Payment_Amount; /// rptAmt - pmtAmt;
                //                break;
                //        }
                //        // Create new balance record
                //        ledgerBalances.Add(new Staging_Balance
                //        {
                //            Id = maxBalanceId + ledgerBalances.Count + 1,
                //            Ledger_Id = change.LedgerId,
                //            BrCode = change.Brcode,
                //            Balance_Date = currentDate,
                //            Balance_Amount = balanceAmt ,
                //            Created_By = createdBy,
                //            Created_Date = DateTime.Now
                //        });
                //    }
                //}
                #endregion

                var staginTransactions = await (
                    from sd in CSISContext.Staging_Details
                    join vt in CSISContext.Fin_Voucher_Trn
                        on sd.Voc_Id equals vt.Voc_Id
                    join fl in CSISContext.Fin_Ledger
                        on vt.Led_Id equals fl.Led_Id
                    join fg in CSISContext.Fin_Ledger_Grp
                        on fl.Grp_Id equals fg.Grp_Id
                    where sd.Staging_Status == "V"
                    select new
                    {
                        fnl_id = fg.Fnl_Id,
                        led_Id = vt.Led_Id,
                        voc_rpt = vt.Voc_Rpt,
                        voc_pmt = vt.Voc_Pmt,
                        brCode = vt.BrCode
                    }).ToListAsync();

                double balanceAmt = 0;
                foreach (var trn in staginTransactions)
                {
                    var existingBalance = await CSISContext.Staging_Balance
                        .FirstOrDefaultAsync(lb => lb.Ledger_Id == trn.led_Id &&
                                                  lb.BrCode == trn.brCode &&
                                                  lb.Balance_Date == currentDate);
                    if (existingBalance != null)
                    {
                        switch (trn.fnl_id)
                        {
                            case 1:
                            case 4:
                                if (existingBalance!.Ledger_Id == cashLedId)
                                {
                                    existingBalance!.Balance_Amount += trn.voc_rpt - trn.voc_pmt; /// rptAmt - pmtAmt;
                                }
                                else
                                {
                                    existingBalance!.Balance_Amount += trn.voc_pmt - trn.voc_rpt; ///  pmtAmt - rptAmt;
                                }
                                break;
                            case 2:
                            case 3:
                                existingBalance!.Balance_Amount += trn.voc_rpt - trn.voc_pmt; /// rptAmt - pmtAmt;
                                break;
                        }
                    }
                    else
                    {
                        balanceAmt = 0;
                        var fnlId = (from fl in CSISContext.Fin_Ledger
                                     join fg in CSISContext.Fin_Ledger_Grp
                                         on fl.Grp_Id equals fg.Grp_Id
                                     where fl.Led_Id == trn.led_Id
                                     select fg.Fnl_Id).FirstOrDefault();
                        switch (fnlId)
                        {
                            case 1:
                            case 4:
                                if (trn.led_Id == cashLedId)
                                {
                                    balanceAmt = trn.voc_rpt - trn.voc_pmt; /// rptAmt - pmtAmt;
                                }
                                else
                                {
                                    balanceAmt += trn.voc_pmt - trn.voc_rpt; ///  pmtAmt - rptAmt;
                                }
                                break;
                            case 2:
                            case 3:
                                balanceAmt += trn.voc_rpt - trn.voc_pmt; /// rptAmt - pmtAmt;
                                break;
                        }
                        // Create new balance record
                        ledgerBalances.Add(new Staging_Balance
                        {
                            Id = maxBalanceId + ledgerBalances.Count + 1,
                            Ledger_Id = trn.led_Id,
                            BrCode = trn.brCode,
                            Balance_Date = currentDate,
                            Balance_Amount = balanceAmt,
                            Created_By = createdBy,
                            Created_Date = DateTime.Now
                        });
                    }
                }

                #endregion

                /// Step 10: Add new ledger balance records
                CSISContext.Staging_Balance.AddRange(ledgerBalances);
                await CSISContext.SaveChangesAsync();

                #region Step 11:  Calculate Loan interest
                #endregion

                #region Step 12: Calculate Fixed deposit interest
                #endregion

                #region stip 13: Calculate Recurring deposit interest
                #endregion

                
                result = true;

            }
            catch (Exception ex)
            {
                result = false;
                Console.Write("Error in day end process" + ex.ToString());
            }
            return result;
        }

        public async Task<bool> DayBeginProcess(string brCode)
        {
            bool result = false;
            DateTime beginDate;
            try
            {
                var beginStatus = await CanBeginDay(brCode);
                if (!beginStatus)
                {
                    result = false;
                }
                beginDate = await (from bd in CSISContext.Business_Day
                                   where bd.Calendar_Status == ((char)Status.NotProcessed).ToString()
                                   && bd.BrCode == brCode
                                   && bd.Calendar_Id == (
                                       from bd2 in CSISContext.Business_Day
                                       where bd2.Calendar_Status == ((char)Status.NotProcessed).ToString() && bd2.BrCode == brCode
                                       select bd2.Calendar_Id
                                   ).Min()
                                   select bd.Calendar_Date).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result = false;
            }
            return result;
        }

        public  async Task<List<decimal>> GetFixedDepositIdForInterestCalculation(string tdSchemeType, DateTime toDate, string brCode)
        {
            List<decimal> tdIdList = new();
            try
            {
                var result = await (from td in CSISContext.TermDeposit_Master
                                      join scheme in CSISContext.TermDeposit_Schemes
                                          on td.TDScheme_Id equals scheme.TDScheme_Id
                                      join member in CSISContext.TermDeposit_Members
                                          on td.TD_Id equals member.TD_Id
                                      where td.TD_Delete == false
                                          && td.AccountClosed == false
                                          && member.TDMem_Delete == false
                                          && scheme.TDSchemeType == tdSchemeType
                                          && td.BrCode == brCode
                                      orderby td.TD_Id
                                      select td.TD_Id).ToListAsync();
                if (result != null && result.Any() ) tdIdList = result.ToList();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                tdIdList = new();
            }
            return tdIdList;
        }

       
    }
}
