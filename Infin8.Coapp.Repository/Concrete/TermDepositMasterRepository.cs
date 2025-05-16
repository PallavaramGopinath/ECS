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
    public class TermDepositMasterRepository : Repository<TermDeposit_Master>, ITermDepositMasterRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public TermDepositMasterRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddTermDepositMasterAsync(TermDeposit_Master termDepositMaster)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.TermDeposit_Master.MaxAsync(x => x.TD_Id);
                maxId++;
                termDepositMaster.TD_Id = maxId;
                await AddAsync(termDepositMaster);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Term deposit master not saved");
            }
            return result;
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
    }
}
