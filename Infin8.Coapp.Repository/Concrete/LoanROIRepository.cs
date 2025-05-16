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
    public class LoanROIRepository : Repository<Loan_Roi>, ILoanROIRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LoanROIRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddLoanROIAsync(Loan_Roi loanRoi)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Loan_Roi.MaxAsync(x => x.Roi_Id);
                maxId++;
                loanRoi.Roi_Id = maxId;
                await AddAsync(loanRoi);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan rate of interest not saved");
            }

            return result;
        }
        public async Task<bool> AddLoanRoiListAsync(List<Loan_Roi> loanRoiList)
        {
            bool result = false;
            decimal maxId = 0;
            try
            {
                foreach (var loanRoi in loanRoiList)
                {
                    maxId = await CSISContext.Loan_Roi.MaxAsync(x => x.Roi_Id);
                    maxId++;
                    loanRoi.Roi_Id = maxId;
                    await AddAsync(loanRoi);
                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan rate of interest not saved");
            }
            return result;
        }
        public async Task<bool> EditLoanROIAsync(Loan_Roi loanRoi)
        {
            bool result = false;
            try
            {
                loanRoi.Loanroi_Delete = true;
                await EditAsync(loanRoi);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan rate of interest not deleted");
            }
            return result;
        }

        public async Task<bool> EditLoanRoiListAsync(List<Loan_Roi> loanRoiList)
        {
            bool result = false;
            try
            {
                foreach (var loanRoi in loanRoiList)
                {
                    loanRoi.Loanroi_Delete = true;
                    await EditAsync(loanRoi);
                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan rate of interest not deleted");
            }
            return result;
        }
    }
}
