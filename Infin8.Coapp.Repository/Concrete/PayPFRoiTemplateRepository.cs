using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class PayPFRoiTemplateRepository : Repository<Pay_PF_ROITemplate>, IPayPFRoiTemplateRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public PayPFRoiTemplateRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddPayPFRoiTemplateAsync(Pay_PF_ROITemplate payPFROITemplate)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Pay_PF_ROITemplate.MaxAsync(x => x.Roi_Id);
                maxId++;
                payPFROITemplate.Roi_Id = maxId;
                await AddAsync(payPFROITemplate);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " PF rate of interest not saved");
            }
            return result;
        }

        public async  Task<bool> EditPayPFRoiTemplateAsync(Pay_PF_ROITemplate payPFROITemplate)
        {
            bool result = false;
            try
            {
                payPFROITemplate.Roi_Delete  = true;
                await EditAsync(payPFROITemplate);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " PF rate of interest not modified");
            }
            return result;
        }

        public async Task<List<Pay_PF_ROITemplate>> GetPayPFROITemplateListAsync()
        {
            List<Pay_PF_ROITemplate> list = new List<Pay_PF_ROITemplate>();
            try
            {
                var pfroiList = await CSISContext.Pay_PF_ROITemplate.Where(x => x.Roi_Delete == false).ToListAsync();
                if(pfroiList != null && pfroiList.Count > 0) list = pfroiList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching PF rate of interest template list");
            }
            return list;
        }

        public async Task<Pay_PF_ROITemplate> GetPayPFRoiTemplateByDate(DateTime salaryDate, string brCode)
        {
            Pay_PF_ROITemplate pfTemplate = new();
            try
            {
                var maxDate = await CSISContext.Pay_PF_ROITemplate
                .Where(t => t.Roi_Delete == false && t.Roi_Wef <= salaryDate && t.BrCode == brCode  )
                .MaxAsync(t => t.Roi_Wef);

                var pfroi = await CSISContext.Pay_PF_ROITemplate
                    .FirstOrDefaultAsync(t => t.Roi_Wef == maxDate && t.Roi_Delete == false && t.BrCode == brCode );  
                if (pfroi != null) pfTemplate = pfroi;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return pfTemplate;
        }
    }
}
