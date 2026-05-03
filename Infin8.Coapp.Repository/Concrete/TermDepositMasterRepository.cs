using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infin8.Coapp.Repository
{
    public class TermDepositMasterRepository : Repository<TermDeposit_Master>, ITermDepositMasterRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public TermDepositMasterRepository(DbContext context) : base(context)
        {
        }

        public async Task<(bool result, decimal tdId, string tdNo)> AddTermDepositMasterAsync(TermDeposit_Master termDepositMaster)
        {
            bool result = false;
            decimal tdId = 0;
            string tdNo = string.Empty; 
            try
            {
                decimal maxId = await CSISContext.TermDeposit_Master.MaxAsync(x => x.TD_Id);
                maxId++;
                //var newTDNo = await GetNewTDNo(termDepositMaster.TDScheme_Id);
                //if(newTDNo!= null) {tdNo = newTDNo;}
                termDepositMaster.TD_Id = maxId;
                //termDepositMaster.TD_No = tdNo;
                await AddAsync(termDepositMaster);
                tdId = maxId;
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Term deposit master not saved");
            }
            return (result,tdId,tdNo);
        }

        public async Task<bool> EditTermDepositMasterAsync(TermDeposit_Master termDepositMaster)
        {
            bool result = false;
            try
            {
                //termDepositFCTemplate.TDfc_Delete = true;
                await EditAsync(termDepositMaster);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Term deposit master not deleted");
            }
            return result;
        }

        public async Task<FDRenewalObject> GetFDRnewalObjectByMemIdAsync(decimal memId)
        {
            FDRenewalObject renewalObject = new FDRenewalObject();
            decimal maxTDId = 0;
            TermDeposit_Master fdDetails = new();
            try
            {
                maxTDId = await CSISContext.TermDeposit_Master.Where(x => x.Mem_Id == memId).MaxAsync(x => x.TD_Id);
                decimal.TryParse(maxTDId.ToString(), out decimal tdId);
                if (tdId > 0)
                    fdDetails = await CSISContext.TermDeposit_Master.Where(t => t.TD_Id == tdId).FirstOrDefaultAsync() ?? new TermDeposit_Master();
                if (fdDetails != null)
                {
                    renewalObject.TDH_Name = fdDetails.TDH_Name;
                    int.TryParse(fdDetails.TDH_Age.ToString(), out int _tdhAge);
                    renewalObject.TDH_Age = _tdhAge;
                    renewalObject.Nominee1Name = fdDetails.Nominee1Name;
                    renewalObject.Nominee1Age = fdDetails.Nominee1Age;
                    renewalObject.Nominee1Relationship = fdDetails.Nominee1Relationship;
                    renewalObject.Nominee2Name = fdDetails.Nominee2Name;
                    renewalObject.Nominee2Age = fdDetails.Nominee2Age;
                    renewalObject.Nominee2Relationship = fdDetails.Nominee2Relationship;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching previous Term deposit data for renewal");
            }
            return renewalObject;
        }

        public async Task<DateTime> GetMaxMaturityDate(decimal[] tdIds, DateTime toDate)
        {
            DateTime maxMaturityDte;
            try
            {
                var loanIdList = await (from lien in CSISContext.Lien_Trn
                                        where tdIds.Contains(lien.TD_Id) && lien.LienTr_Delete == false
                                        select lien.Loan_Id)
                  .Distinct()
                  .ToListAsync();
                var IntToDate = await (from td in CSISContext.TermDeposit_Master
                                       join lien in CSISContext.Lien_Trn on td.TD_Id equals lien.TD_Id
                                       where loanIdList.Contains(lien.Loan_Id)
                                       select td.MaturityDate)
                     .MaxAsync();
                maxMaturityDte = IntToDate;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching maximum maturitydate for fd loans interest calculation");
            }
            return maxMaturityDte;
        }

        public async Task<List<FDDataForLoan>> GetFDDetailsForLoan(decimal[] tdIds)
        {
            List<FDDataForLoan> tdDetails = new List<FDDataForLoan>();
            try
            {
                var query = await  CSISContext.TermDeposit_Master
                    .Where(td => tdIds.Contains(td.TD_Id))
                    .Select(td => new FDDataForLoan
                    {
                        FD_Id = td.TD_Id,
                        FD_No = td.TD_No,
                        FD_Amount = td.DepositAmount,
                        Period_In_Months = td.PeriodInMonths,
                        Period_In_Days = td.PeriodInDays,
                        Rate_Of_Interest = td.RateOfInterest,
                        Value_Date = td.ValueDate,
                        Maturity_Amount = td.MaturityAmount,
                        Maturity_Date = td.MaturityDate
                    }).ToListAsync();
                if(query != null && query.Count >0) tdDetails = query.ToList ();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching term deposit data for loan");
            }
            return tdDetails;
        }

        public async Task<string> GetNewTDNo(int schemeId)
        {
            string numberAsString = schemeId.ToString();
            string brCode = numberAsString.Substring(0, 5); // Result will be "11001"
            string forFDString = brCode + "400";
            int forFD = int.Parse(forFDString);
            //int forFD = int.Parse(numberAsString.Substring(5))+400; 
            string lastThreeChars = numberAsString.Substring(numberAsString.Length - 3); // Result will be "301"
            int lastThreeDigitsAsInt = int.Parse(lastThreeChars); // Result will be 301
            string newTDNo = string.Empty;
            decimal maxId = 0;
            try
            {
                if(lastThreeDigitsAsInt < 400) /// for fixed deposit
                {
                    var maxNo = await (from lm in CSISContext.TermDeposit_Master
                                       where lm.TDScheme_Id < forFD
                                       select lm.TD_No).MaxAsync();
                    if (!string.IsNullOrWhiteSpace(maxNo))
                    {
                        maxId = Convert.ToDecimal(maxNo) + 1;
                        maxNo = Convert.ToString(maxId);
                    }
                    else
                    {
                        maxNo = brCode + "030000001"; // If no records found, start with 001
                    }
                    newTDNo = maxNo;
                }
                else if(lastThreeDigitsAsInt >= 400 && lastThreeDigitsAsInt <500) /// for recurring deposit
                {
                    var maxNo = await (from lm in CSISContext.TermDeposit_Master
                                       where lm.TDScheme_Id == schemeId
                                       select lm.TD_No).MaxAsync();
                    if (string.IsNullOrEmpty(maxNo))
                    {
                        maxId = Convert.ToDecimal(maxNo) + 1;
                        maxNo = Convert.ToString(maxId);
                    }
                    else
                    {
                        maxNo = brCode + "040000001"; // If no records found, start with 001
                    }
                    newTDNo = maxNo;
                }
                
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Error in fetching New Loan No based on Loan Scheme");
            }
            return newTDNo;
        }

        public async Task<bool> UpdateTermDepositMasterAsClosed(decimal tdId)
        {
            bool result = false;
            try
            {
                var master = await CSISContext.TermDeposit_Master.Where(x=> x.TD_Id == tdId).FirstOrDefaultAsync();
                if(master != null)
                {
                    master.AccountClosed = true;
                    await EditAsync(master);
                    result = true;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }
    }
}
