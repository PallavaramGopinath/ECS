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
    public class TermDepositFCTemplateRepository : Repository<TermDeposit_FcTemplate>, ITermDepositFCTemplateRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public TermDepositFCTemplateRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddTermDepositFCTemplateAsync(TermDeposit_FcTemplate termDepositFCTemplate)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.TermDeposit_FcTemplate.MaxAsync(x => x.TDfc_Id);
                maxId++;
                termDepositFCTemplate.TDfc_Id=maxId;
                await AddAsync(termDepositFCTemplate);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Term deposit fore closure template not saved");
            }
            return result;
        }

        public async Task<bool> EditTermDepositFCTemplateAsync(TermDeposit_FcTemplate termDepositFCTemplate)
        {
            bool result = false;
            try
            {
                //termDepositFCTemplate.TDfc_Delete = true;
                await EditAsync(termDepositFCTemplate);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Term deposit fore closure template not deleted");
            }
            return result;
        }

        public async Task<List<TermDeposit_FcTemplate>> GetTermDepositFCTemplateListAsync()
        {
            List<TermDeposit_FcTemplate> list = new List<TermDeposit_FcTemplate>();
            try
            {
                var fcList = await CSISContext.TermDeposit_FcTemplate.Where(x => x.TDfc_Delete == false).OrderBy(x => new { x.TD_Type, x.TDfc_Wef }).ToListAsync();
                if(fcList != null && fcList.Count > 0) list = fcList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching term deposit fore closure template list");
            }
            return list;
        }

        public async Task<double> GetTDForeClosureROIAsync(int tdSchemeType, DateTime wef)
        {
            double fcRoi = 0;
            try
            {
                var maxWefDate =await  CSISContext.TermDeposit_FcTemplate
            .Where(t => t.TDfc_Delete == false
                && t.TDfc_Wef <= wef
                && t.TD_Type == tdSchemeType)
            .MaxAsync(t => t.TDfc_Wef);

                // Get the ROI using the max WEF date
                var roi = await  CSISContext.TermDeposit_FcTemplate
                    .Where(t => t.TD_Type == tdSchemeType
                        && t.TDfc_Delete == false
                        && t.TDfc_Wef == maxWefDate)
                    .Select(t => t.TDfc_Roi)
                    .FirstOrDefaultAsync();
                if (roi >0) fcRoi = roi;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching term deposit fore closure rate of interest");
            }
            return fcRoi;   
        }
    }
}
