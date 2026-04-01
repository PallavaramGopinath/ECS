using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class PayPFTemplateRepository : Repository<Pay_PF_Template>, IPayPFTemplateRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public PayPFTemplateRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<Pay_PF_Template>> AddPayPFTemplateAsync(Pay_PF_Template payPFTemplate)
        {
            List<Pay_PF_Template> list = new();
            try
            {
                decimal maxId = await CSISContext.Pay_PF_Template.MaxAsync(x => x.Pf_Id);
                maxId++;
                payPFTemplate.Pf_Id = maxId;
                await AddAsync(payPFTemplate);
                CSISContext.SaveChanges();
                
                var templateList = await CSISContext.Pay_PF_Template.Where(x => x.BrCode == payPFTemplate.BrCode && x.Pf_Delete == false).OrderBy(x => x.Wef).ToListAsync();
                if (templateList != null && templateList.Count > 0) list = templateList;
            }
            catch (Exception ex)
            {
                list = new();
                //throw new InvalidOperationException(ex.Message + " PF Template not saved");
            }
            return list;
        }

        public async Task<List<Pay_PF_Template>> EditPayPFTemplateAsync(Pay_PF_Template payPFTemplate)
        {
            List<Pay_PF_Template> list = new();
            try
            {
                await EditAsync(payPFTemplate);
                CSISContext.SaveChanges();

                var templateList = await CSISContext.Pay_PF_Template.Where(x => x.BrCode == payPFTemplate.BrCode && x.Pf_Delete == false).OrderBy(x => x.Wef).ToListAsync();
                if (templateList != null && templateList.Count > 0) list = templateList;
            }
            catch (Exception)
            {
                list = new();
                //throw new InvalidOperationException(ex.Message + " Somethis wend wrong! An error occurred while modifying PF Template");
            }
            return list;
        }

        public async Task<List<Pay_PF_Template>> GetPayPFTemplateListAsync(string brCode)
        {
            List<Pay_PF_Template> list = new List<Pay_PF_Template>();
            try
            {
                var templateList = await  CSISContext.Pay_PF_Template.Where(x => x.BrCode == brCode && x.Pf_Delete == false).OrderBy(x => x.Wef).ToListAsync();
                if (templateList != null && templateList.Count > 0) list = templateList;
            }
            catch (Exception)
            {
                list = new();
                //throw new InvalidOperationException(ex.Message + " Somethis wend wrong! An error occurred while fetching PF Template data");
            }
            return list;
        }
    }
}
