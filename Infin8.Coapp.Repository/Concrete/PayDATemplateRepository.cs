using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class PayDATemplateRepository : Repository<Pay_DA_Template>, IPayDATemplateRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public PayDATemplateRepository(DbContext context) : base(context)
        {
        }

        public async Task<Pay_DA_Template> AddPayDATemplateAsync(Pay_DA_Template payDaTemplate)
        {
            try
            {
                decimal maxId = await CSISContext.Pay_DA_Template.MaxAsync(x => x.DA_Id);
                maxId++;
                payDaTemplate.DA_Id  = maxId;
                await AddAsync(payDaTemplate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " DA Template data not saved");
            }
            return payDaTemplate;
        }

        public async Task<bool> EditPayDATemplateAsync(Pay_DA_Template payDaTemplate)
        {
            bool result = false;
            try
            {
                payDaTemplate.DA_Delete  = true;
                await EditAsync(payDaTemplate);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " DA date not modified");
            }
            return result;
        }

        public async Task<List<Pay_DA_Template>> GetPayDATemplateListAsync(string status)
        {
            List<Pay_DA_Template> list = new List<Pay_DA_Template>();
            try
            {
                var daTemplateList = await  CSISContext.Pay_DA_Template.Where(x=> x.Status == status && x.DA_Delete == false).ToListAsync();
                if (daTemplateList != null && daTemplateList.Count > 0) list = daTemplateList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching DA Template list");
            }
            return list;
        }

        public Task<Pay_DA_Template> GetPayDATemplate(DateTime wef, string status, string brCode)
        {
            Pay_DA_Template daTemplate = new();
            try
            {
                var maxDate = CSISContext.Pay_DA_Template
                .Where(t => t.DA_Delete == false
                            && t.Wef <= wef
                            && t.Status == status)
                .Max(t => t.Wef);

                var result = CSISContext.Pay_DA_Template
                    .Where(t => t.DA_Delete == false
                                && t.Status == status
                                && t.Wef == maxDate);
                if(result !=null) daTemplate =  result.First();
                //if (result != null) return Task.FromResult<Pay_DA_Template>(result.First());
            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex.Message);
            }
            return Task.FromResult<Pay_DA_Template>(daTemplate);
            //return Task.FromResult<Pay_DA_Template>(null!);
        }
    }
}
