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
    public class TermDepositROITemplateRepository : Repository<TermDeposit_Roi_Template>, ITermDepositROITemplateRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public TermDepositROITemplateRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddTermDepositROITemplateAsync(TermDeposit_Roi_Template termDepositROITemplate)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.TermDeposit_Roi_Template.MaxAsync(x => x.Roi_Id);
                maxId++;
                termDepositROITemplate.Roi_Id = maxId;
                await AddAsync(termDepositROITemplate);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while saving Term deposit scheme");
            }
            return result;
        }

        public async Task<List<TermDeposit_Roi_Template>> AddTermDepositROITemplateList(List<TermDeposit_Roi_Template> termDepositList)
        {
            List<TermDeposit_Roi_Template> roiList = new();
            int schemeId = termDepositList.Select(x=> x.TDScheme_Id).FirstOrDefault();
            try
            {
                decimal maxId = await CSISContext.TermDeposit_Roi_Template.MaxAsync(x => x.Roi_Id);
                maxId++;
                foreach(var td in termDepositList)
                {
                    td.Roi_Id = maxId;
                    maxId++;
                }
                await CSISContext.TermDeposit_Roi_Template.AddRangeAsync(termDepositList);
                await CSISContext.SaveChangesAsync();
                var query = await CSISContext.TermDeposit_Roi_Template.Where(x => x.TDScheme_Id == schemeId).ToListAsync();
                if (query != null && query.Count > 0) roiList = query.ToList();
            }
            catch (Exception)
            {
                roiList = new();
            }
            return roiList;
        }

        public async Task<bool> EditTermDepositROITemplateAsync(TermDeposit_Roi_Template termDepositROITemplate)
        {
            bool result = false;
            try
            {
                termDepositROITemplate.TDRoi_Delete = true;
                await EditAsync(termDepositROITemplate);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while modifing Term deposit scheme");
            }
            return result;
        }

        public async Task<List<TDRateOfInterstDto>> GetTermDepositROITemplateListAsync(string[] tdSchemeTypeList)
        {
            List<TDRateOfInterstDto> result = new List<TDRateOfInterstDto>();
            try
            {
                var roiList = await (from roi in CSISContext.TermDeposit_Roi_Template
                               join scheme in CSISContext.TermDeposit_Schemes on roi.TDScheme_Id equals scheme.TDScheme_Id
                               where tdSchemeTypeList.Contains(scheme.TDSchemeType) && roi.TDRoi_Delete == false
                               orderby roi.TDScheme_Id, roi.Wef, roi.PeriodType, roi.PeriodBegin
                               select new TDRateOfInterstDto
                               {
                                   Roi_Id = roi.Roi_Id,
                                   TDScheme_Id = roi.TDScheme_Id,
                                   TDScheme_Name = scheme.TDScheme_Name,
                                   Wef = (DateTime)roi.Wef,
                                   PeriodType = roi.PeriodType,
                                   PeriodBegin = roi.PeriodBegin,
                                   PeriodEnd = roi.PeriodEnd,
                                   Roi = roi.Roi,
                                   PenalRateForRD = roi.PenalRateForRD
                               }).ToListAsync();
                if (roiList != null && roiList.Count > 0) result = roiList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching Termdeposit rate of interst list");
            }
            return result;
        }

        public async Task<double> GetROIForTermDepositAsync(DateTime depositDate, int schemeId, int prdInMonths, int prdInDays, string brCode)
        {
            double rateOfInterest = 0;
            try
            {
                #region linq with sub-query
                //if(prdInMonths >0)
                //{
                //    var roi = await (from roiTemplate in CSISContext.TermDeposit_Roi_Template
                //                     where roiTemplate.TDRoi_Delete == false
                //                           && roiTemplate.Wef <= depositDate
                //                           && roiTemplate.PeriodEnd >= prdInMonths
                //                           && roiTemplate.PeriodBegin <= prdInMonths
                //                           && roiTemplate.PeriodType == "M"
                //                           && roiTemplate.TDScheme_Id == schemeId
                //                           && roiTemplate.Wef == (from subTemplate in CSISContext.TermDeposit_Roi_Template
                //                                                  where subTemplate.TDRoi_Delete == false
                //                                                        && subTemplate.Wef <= depositDate
                //                                                        && subTemplate.PeriodEnd >= prdInMonths
                //                                                        && subTemplate.PeriodBegin <= prdInMonths
                //                                                        && subTemplate.PeriodType == "M"
                //                                                        && subTemplate.TDScheme_Id == schemeId
                //                                                  select subTemplate.Wef).Max()
                //                     select roiTemplate.Roi).FirstOrDefaultAsync();
                //    if (roi > 0) rateOfInterest = roi;
                //}
                //if (prdInDays > 0)
                //{
                //    var roi = await (from roiTemplate in CSISContext.TermDeposit_Roi_Template
                //                     where roiTemplate.TDRoi_Delete == false
                //                           && roiTemplate.Wef <= depositDate
                //                           && roiTemplate.PeriodEnd >= prdInDays
                //                           && roiTemplate.PeriodBegin <= prdInDays
                //                           && roiTemplate.PeriodType == "D"
                //                           && roiTemplate.TDScheme_Id == schemeId
                //                           && roiTemplate.Wef == (from subTemplate in CSISContext.TermDeposit_Roi_Template
                //                                                  where subTemplate.TDRoi_Delete == false
                //                                                        && subTemplate.Wef <= depositDate
                //                                                        && subTemplate.PeriodEnd >= prdInDays
                //                                                        && subTemplate.PeriodBegin <= prdInDays
                //                                                        && subTemplate.PeriodType == "M"
                //                                                        && subTemplate.TDScheme_Id == schemeId
                //                                                  select subTemplate.Wef).Max()
                //                     select roiTemplate.Roi).FirstOrDefaultAsync();
                //    if (roi > 0) rateOfInterest = roi;
                //}
                #endregion 

                if (prdInMonths > 0)
                {
                    // LINQ query for PeriodType = 'M'
                    var maxWefDate = await CSISContext.TermDeposit_Roi_Template
                        .Where(t => t.TDRoi_Delete == false
                                    && t.Wef <= depositDate
                                    && t.PeriodEnd >= prdInMonths
                                    && t.PeriodBegin <= prdInMonths
                                    && t.PeriodType == "M"
                                    && t.TDScheme_Id == schemeId
                                    && t.BrCode == brCode )
                        .MaxAsync(t => (DateTime?)t.Wef);

                    if (maxWefDate.HasValue)
                    {
                        var roi = await CSISContext.TermDeposit_Roi_Template
                            .Where(t => t.TDRoi_Delete == false
                                        && t.Wef <= depositDate
                                        && t.PeriodEnd >= prdInMonths
                                        && t.PeriodBegin <= prdInMonths
                                        && t.PeriodType == "M"
                                        && t.TDScheme_Id == schemeId
                                        && t.Wef == maxWefDate.Value
                                        && t.BrCode == brCode)
                            .Select(t => t.Roi)
                            .FirstOrDefaultAsync();
                        if (roi >0) rateOfInterest = roi;
                    }
                }

                if (prdInDays > 0)
                {
                    // LINQ query for PeriodType = 'D'
                    var maxWefDate = await CSISContext.TermDeposit_Roi_Template
                        .Where(t => t.TDRoi_Delete == false
                                    && t.Wef <= depositDate
                                    && t.PeriodEnd >= prdInDays
                                    && t.PeriodBegin <= prdInDays
                                    && t.PeriodType == "D"
                                    && t.TDScheme_Id == schemeId
                                    && t.BrCode == brCode )
                        .MaxAsync(t => (DateTime?)t.Wef);

                    if (maxWefDate.HasValue)
                    {
                        var roi = await CSISContext.TermDeposit_Roi_Template
                            .Where(t => t.TDRoi_Delete == false
                                        && t.Wef <= depositDate
                                        && t.PeriodEnd >= prdInDays
                                        && t.PeriodBegin <= prdInDays
                                        && t.PeriodType == "D"
                                        && t.TDScheme_Id == schemeId
                                        && t.Wef == maxWefDate.Value
                                        && t.BrCode == brCode)
                            .Select(t => t.Roi)
                            .FirstOrDefaultAsync();
                        if (roi > 0) rateOfInterest = roi;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching Termdeposit rate of interst");
            }
            return rateOfInterest;
        }

        public async Task<double> GetPIForRDAsync(DateTime depositDate, int schemeId, int prdInMonths, string brCode)
        {
            double piRate = 0;
            try
            {
                var roi = await (from roiTemplate in CSISContext.TermDeposit_Roi_Template
                           where roiTemplate.TDRoi_Delete == false
                                 && roiTemplate.Wef <= depositDate
                                 && roiTemplate.PeriodEnd >= prdInMonths
                                 && roiTemplate.PeriodBegin <= prdInMonths
                                 && roiTemplate.PeriodType == "M"
                                 && roiTemplate.TDScheme_Id == schemeId
                                 && roiTemplate.Wef == (from subTemplate in CSISContext.TermDeposit_Roi_Template
                                                        where subTemplate.TDRoi_Delete == false
                                                              && subTemplate.Wef <= depositDate
                                                              && subTemplate.PeriodEnd >= prdInMonths
                                                              && subTemplate.PeriodBegin <= prdInMonths
                                                              && subTemplate.PeriodType == "M"
                                                              && subTemplate.TDScheme_Id == schemeId
                                                              && subTemplate.BrCode == brCode
                                                        select subTemplate.Wef).Max()
                           select roiTemplate.PenalRateForRD).FirstOrDefaultAsync();
                if(roi >0 ) piRate = roi;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching penal rate for recurring deposit");
            }
            return piRate;
        }

        public async Task<List<TermDeposit_Roi_Template>> GetTermDepositRateOfInterestList (string SchemeType, string brCode)
        {
            List<TermDeposit_Roi_Template> result = new();
            try
            {
                var roiList = await (from roi in CSISContext.TermDeposit_Roi_Template
                                     join scheme in CSISContext.TermDeposit_Schemes on roi.TDScheme_Id equals scheme.TDScheme_Id
                                     where scheme.TDSchemeType == SchemeType && scheme.TDScheme_Delete == false
                                     && roi.BrCode == brCode && roi.BrCode == brCode  && roi.TDRoi_Delete == false
                                     orderby roi.TDScheme_Id, roi.Wef, roi.PeriodType, roi.PeriodBegin
                                     select new TermDeposit_Roi_Template
                                     {
                                         Roi_Id = roi.Roi_Id,
                                         TDScheme_Id = roi.TDScheme_Id,
                                         TDHolderType = roi.TDHolderType,
                                         Wef = (DateTime)roi.Wef,
                                         PeriodType = roi.PeriodType,
                                         PeriodBegin = roi.PeriodBegin,
                                         PeriodEnd = roi.PeriodEnd,
                                         Roi = roi.Roi,
                                         PenalRateForRD = roi.PenalRateForRD,
                                         BrCode = roi.BrCode 
                                     }).ToListAsync();
                if (roiList != null && roiList.Count > 0) result = roiList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching Termdeposit rate of interst list");
            }
            return result;
        }

    }
}
