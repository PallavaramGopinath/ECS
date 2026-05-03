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
    public class SBCAMasterRepository  : Repository<SBCA_Master>, ISBCAMasterRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public SBCAMasterRepository(DbContext context) : base(context)
        {
        }

        public async Task<(bool result, decimal accId, string accNo)> AddSBCAMasterAsync(SBCA_Master sbcaMaster)
        {
            bool result = false;
            decimal accId = 0;
            string accNo = string.Empty;

            try
            {
                decimal maxId = await CSISContext.SBCA_Master
                .MaxAsync(x => (decimal?)x.Acc_Id) ?? 0;
                if(maxId == 0)
                {
                    decimal.TryParse(sbcaMaster.BrCode + "0000000", out maxId);
                }
                var maxAccountNo = await GetNewSBAccountNo(sbcaMaster.BrCode!);
                maxId++;
                sbcaMaster.Acc_Id = maxId;
                sbcaMaster.Acc_No = maxAccountNo;
                await AddAsync(sbcaMaster);
                CSISContext.SaveChanges();
                result = true;
                accId = maxId;
                accNo = maxAccountNo;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while saving new SB Account");
            }
            return (result,accId,accNo );
        }

        public async Task<SBCA_Master> AddNewSBAccount(SBCA_Master sbAccount)
        {
            try
            {
                decimal maxId = await CSISContext.SBCA_Master
                .MaxAsync(x => (decimal?)x.Acc_Id) ?? 0;
                if (maxId == 0)
                {
                    decimal.TryParse(sbAccount.BrCode + "0000000", out maxId);
                }
                var maxAccountNo = await GetNewSBAccountNo(sbAccount.BrCode!);
                maxId++;
                sbAccount.Acc_Id = maxId;
                sbAccount.Acc_No = maxAccountNo;
                await AddAsync(sbAccount);
                CSISContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while saving new SB Account");
            }
            return sbAccount;
        }

        public async Task<bool> EditSBCAMasterAsync(SBCA_Master sbcaMaster)
        {
            bool result = false;
            try
            {
                //termDepositFCTemplate.TDfc_Delete = true;
                await EditAsync(sbcaMaster);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while modifying SB account");
            }
            return result;
        }

        public async Task<List<DropdownItem>> GetSBCANosByMemIdAsync(decimal memId,string brCode)
        {
            List<DropdownItem> list = new List<DropdownItem>();
            try
            {
                var sbAccountList = await CSISContext.SBCA_Master
                .Where(acc => acc.Mem_Id == memId && acc.BrCode == brCode  && acc.Acc_Delete == false)
                .Select(acc => new DropdownItem
                {
                    Value = acc.Acc_Id.ToString(),
                    Text = acc.Acc_No
                })
                .ToListAsync();
                if(sbAccountList != null && sbAccountList.Count >0) list = sbAccountList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching SB account No(s) by member id");
            }
            return list;
        }

        public async Task<string> GetNewSBAccountNo(string brCode)
        {
            string newLoanNo = string.Empty;
            decimal maxId = 0;
            try
            {
                decimal schemeId = await (from lm in CSISContext.SBCA_Schemes
                                       where lm.BrCode == brCode && lm.SBCA_Delete == false
                                       select lm.Scheme_Id).FirstOrDefaultAsync();

                var maxLoanNo = await (from lm in CSISContext.SBCA_Master
                                       where lm.Scheme_Id == schemeId
                                       select lm.Acc_No).MaxAsync();
                if (!string.IsNullOrEmpty(maxLoanNo))
                {
                    maxId = Convert.ToDecimal(maxLoanNo) + 1;
                    newLoanNo = Convert.ToString(maxId);
                }
                else
                {
                    newLoanNo = brCode + "0000001";
                }
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Error in fetching New Loan No based on Loan Scheme");
            }
            return newLoanNo;
        }

        public async Task<string> GetSBCANoByAccIdAsync(decimal accId, string brCode)
        {
            string sbAccountNo = string.Empty;
            try
            {
                var accountNo = await CSISContext.SBCA_Master
                .Where(acc => acc.Acc_Id == accId && acc.BrCode == brCode && acc.Acc_Delete == false)
                .Select(acc => acc.Acc_No)
                .FirstOrDefaultAsync();
                if (!string.IsNullOrEmpty(accountNo)) sbAccountNo = accountNo;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching SB account No by account id");
            }
            return sbAccountNo;
        }

        public async Task<string> GetSBCANoByMemIdAsync(decimal memId, string brCode)
        {
            string sbAccountNo = string.Empty;
            try
            {
                var accountNo = await CSISContext.SBCA_Master
                .Where(acc => acc.Mem_Id == memId && acc.BrCode == brCode && acc.Acc_Delete == false)
                .Select(acc => acc.Acc_No)
                .FirstOrDefaultAsync();
                if (!string.IsNullOrEmpty(accountNo)) sbAccountNo = accountNo;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching SB account No by member id");
            }
            return sbAccountNo;
        }

        public async Task<DtoSBAccountNo> GetSBAccountDataByMemIdAsync(decimal memId, string brCode)
        {
            DtoSBAccountNo SBAccountNo = new DtoSBAccountNo();
            try
            {
                var sbData = await CSISContext.SBCA_Master
                .Where(acc => acc.Mem_Id == memId && acc.BrCode == brCode && acc.Acc_Delete == false)
                .FirstOrDefaultAsync();

                if (sbData != null)
                {
                    SBAccountNo.SBAccountNo  = sbData.Acc_No ;
                    SBAccountNo.SBAccountId = sbData.Acc_Id;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching SB account data by member id");
            }
            return SBAccountNo;
        }
    }
}
