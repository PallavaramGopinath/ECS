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
    public class LoanROITemplateRepository :Repository<Loan_Roi_Template>, ILoanROITemplateRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LoanROITemplateRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<Loan_Roi_Template>> GetLoanRateOfInterestListAsync(int schemeId)
        {
            List<Loan_Roi_Template> items = new List<Loan_Roi_Template>();
            try
            {
                var data = await CSISContext.Loan_Roi_Template.Where(x=> x.Scheme_Id == schemeId && x.RoiTemplate_Delete == false).OrderBy(x=> x.Roi_Id).ToListAsync();
                if (data != null)
                {
                    items = data;
                }
            }
            catch (Exception ex)
            {
                new InvalidOperationException(ex.Message + " Something went wrong! Loan rate of interest items not found");
            }
            return items;
        }

        public  async Task<bool> AddLoanROIAsync(Loan_Roi_Template roiTemplate)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Loan_Roi_Template.MaxAsync(x => x.Roi_Id);
                maxId++;
                roiTemplate.Roi_Id = maxId;
                await AddAsync(roiTemplate);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan rate of interest template not saved");
            }

            return result;
        }

        public async Task<bool> EditLoanROIAsync(Loan_Roi_Template roiTemplate)
        {
            bool result = false;
            try
            {
                await EditAsync(roiTemplate);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while modify the Loan rate of interest template");
            }
            return result;
        }

        public async Task<LoanROIAndPIVM> GetLoanROIAndPIFromTemplateAsync(int schemeId, string agency, DateTime wef)
        {
            LoanROIAndPIVM roi = new LoanROIAndPIVM();
            try
            {
                /// sample 1 linq
                //var maxWef = CSISContext.Loan_Roi_Template
                //.Where(t => t.Wef.Date <= wef.Date &&
                //            t.Agency == agency &&
                //            t.Scheme_Id == schemeId &&
                //            t.RoiTemplate_Delete == false)
                //.Max(t => (DateTime?)t.Wef);

                //var roi1 = CSISContext.Loan_Roi_Template
                //    .Where(t => t.Wef == maxWef &&
                //                t.Agency == agency &&
                //                t.Scheme_Id == schemeId &&
                //                t.RoiTemplate_Delete == false)
                //    .Select(t => new LoanROIAndPIVM
                //    {
                //        Roi = t.Roi,
                //        Pi = t.Pi
                //    })
                //    .FirstOrDefault();

                /// sample 2 linq
                var roi1 = await CSISContext.Loan_Roi_Template
                    .Where(t => t.Wef.Date == CSISContext.Loan_Roi_Template
                        .Where(t => t.Wef.Date <= wef.Date &&
                                    t.Agency == agency &&
                                    t.Scheme_Id == schemeId &&
                                    t.RoiTemplate_Delete == false)
                        .Max(t => (DateTime?)t.Wef.Date) &&
                                t.Agency == agency &&
                                t.Scheme_Id == schemeId &&
                                t.RoiTemplate_Delete == false)
                    .Select(t => new LoanROIAndPIVM
                    {
                        Roi = t.Roi,
                        Pi = t.Pi
                    })
                    .FirstOrDefaultAsync();
                if (roi1 != null) roi = roi1; // as LoanROIAndPIVM;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching Loan rate of interest from template");
            }
            return roi;
        }
    }
}
