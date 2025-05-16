using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class TermDepositLoanEligibleTemplateRepository: Repository<TermDeposit_LoanEligibleTemplate> , ITermDepositLoanEligibleTemplateRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public TermDepositLoanEligibleTemplateRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddTermDepositLoanEligibleTemplateAsync(TermDeposit_LoanEligibleTemplate termDepositLoanEligibleTemplate)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.TermDeposit_LoanEligibleTemplate.MaxAsync(x => x.TDLoanelig_Id);
                maxId++;
                termDepositLoanEligibleTemplate.TDLoanelig_Id = maxId;
                await AddAsync(termDepositLoanEligibleTemplate);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Term deposit loan eligible template not saved");
            }
            return result;
        }

        public async Task<bool> EditTermDepositLoanEligibleTemplateAsync(TermDeposit_LoanEligibleTemplate termDepositLoanEligibleTemplate)
        {
            bool result = false;
            try
            {
                //termDepositFCTemplate.TDfc_Delete = true;
                await EditAsync(termDepositLoanEligibleTemplate);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Term deposit loan eligible template not deleted");
            }
            return result;
        }

        public async Task<List<TermDeposit_LoanEligibleTemplate>> GetTermDepositLoanEligibleTemplateListAsync()
        {
            List<TermDeposit_LoanEligibleTemplate> list = new List<TermDeposit_LoanEligibleTemplate>();
            try
            {
                var templateList = await CSISContext.TermDeposit_LoanEligibleTemplate.Where(x => x.TD_Delete == false).OrderBy(x => new { x.TD_Type, x.Wef_Date }).ToListAsync();
                if(templateList != null && templateList.Count >0) list = templateList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching term deposit loan eligible template List");
            }
            return list;
        }

        public async Task<double> GetTDLoanEligiblePercentageAsync(int tdSchemeType , string brCode)
        {
            double loanEligiblePercentage = 0;
            try
            {
                var maxWefDate = await CSISContext.TermDeposit_LoanEligibleTemplate
                .Where(t => t.TD_Type == tdSchemeType
                    && t.BrCode == brCode)
                .MaxAsync(t => (DateTime?)t.Wef_Date);

                if (maxWefDate.HasValue)
                {
                    var roi = CSISContext.TermDeposit_LoanEligibleTemplate
                        .Where(t => t.TD_Type == tdSchemeType && t.Wef_Date == maxWefDate.Value && t.BrCode == brCode )
                        .Select(t => (double?)t.Elig_Per)
                        .FirstOrDefault();
                    if(roi != null) {loanEligiblePercentage = (double)roi.Value; }  
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching term deposit loan eligible percentage");
            }
            return loanEligiblePercentage;
        }
    }
}
