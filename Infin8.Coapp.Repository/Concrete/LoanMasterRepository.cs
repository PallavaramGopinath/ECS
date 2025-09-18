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
    public class LoanMasterRepository : Repository<Loan_Master>, ILoanMasterRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LoanMasterRepository(DbContext context) : base(context)
        {
        }
        public async Task<(bool result, decimal loanId, string loanNo)> AddLoanMasterAsync(Loan_Master loanMaster)
        {
            bool result = false;
            decimal loanId = 0;
            int maxSlNo = 0;
            string loanNo = string.Empty;
            try
            {
                //decimal maxId = await CSISContext.Loan_Master.MaxAsync(x => x.Loan_Id);
                decimal maxId = await CSISContext.Loan_Master.AnyAsync()
                    ? await CSISContext.Loan_Master.MaxAsync(sh => sh.Loan_Id)
                    : 0;
                var maxLoanNo = await GetNewLoanNo(loanMaster.Scheme_Id);
                maxId++;
                loanMaster.Loan_Id = maxId;
                loanMaster.Loan_No = maxLoanNo;
                await AddAsync(loanMaster);
                result = true;
                loanId = maxId;
                loanNo = maxLoanNo;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan Master not saved");
            }

            return (result,loanId,loanNo);
        }

        public async Task<bool> EditLoanMasterAsync(Loan_Master loanMaster)
        {
            bool result = false;
            try
            {
                await EditAsync(loanMaster);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan Master not modified");
            }
            return result;
        }

        public async Task<bool> IsLoanSchemeReferedInLoanMaster(int schemeId)
        {
            bool result = false;
            try
            {
                int count = await CSISContext.Loan_Master.Where(x => x.Scheme_Id == schemeId && x.Loan_Delete == false).CountAsync();
                if (count > 0) result  = true;
                else result = false;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan Master not modified");
            }
            return result;
        }

        public async Task<string> GetNewLoanNo(int schemeId)
        {
            string newLoanNo = string.Empty;
            decimal maxId = 0;
            try
            {
                //var maxLoanNo = await (from lm in CSISContext.Loan_Master
                //                       where lm.Scheme_Id == schemeId
                //                       select lm.Loan_No).MaxAsync();

                var maxLoanNo = await CSISContext.Loan_Master
                .Where(x => x.Scheme_Id == schemeId)
                .OrderByDescending(x => Convert.ToInt64(x.Loan_No))
                .Select(x => x.Loan_No)
                .FirstOrDefaultAsync() ?? "0";


                //if (string.IsNullOrWhiteSpace(maxLoanNo))
                if(Convert.ToInt64(maxLoanNo) >0 )
                {
                    maxId = Convert.ToDecimal(maxLoanNo) + 1;
                    newLoanNo = Convert.ToString(maxId);
                }
                else
                {
                    var LoanNo = await (from ln in CSISContext.Loan_Schemes
                                  where ln.Scheme_Id == schemeId
                                  select ln.LoanNoStartWith).FirstOrDefaultAsync();
                    if(string.IsNullOrWhiteSpace(LoanNo))
                    {
                        Console.Write("Loan Scheme does not have Loan No Start With defined");
                        throw new InvalidOperationException("Loan Scheme does not have Loan No Start With defined");
                    }
                    else
                    {
                        var brCode = await (from ln in CSISContext.Loan_Schemes
                                        where ln.Scheme_Id == schemeId
                                        select ln.BrCode).FirstOrDefaultAsync();
                        var loanNo2 = brCode! + LoanNo! + "0000001";
                        newLoanNo = loanNo2;
                        //decimal maxId2 = Convert.ToDecimal(loanNo2) + 1;
                        //newLoanNo = LoanNo + maxId2.ToString();
                    }
                }
            }
            catch (Exception)
            {
                Console.Write("Error in fetching New Loan No based on Loan Scheme");
                throw new InvalidOperationException("Error in fetching New Loan No based on Loan Scheme");
            }
            return newLoanNo;
        }

        
    }
}
