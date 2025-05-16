using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class PayTemplateRepository : Repository<Pay_Template> , IPayTemplateRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public PayTemplateRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddPayTemplateAsync(Pay_Template payTemplate)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Pay_Template.MaxAsync(x => x.PayTemplate_Id);
                maxId++;
                payTemplate.PayTemplate_Id = maxId;
                await AddAsync(payTemplate);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Pay template not saved");
            }
            return result;
        }

        public async Task<bool> EditPayTemplateAsync(Pay_Template payTemplate)
        {
            bool result = false;
            try
            {
                await EditAsync(payTemplate);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while modification of Pay template");
            }
            return result;
        }

        public async Task<Pay_Template> GetPayTemplateAsync()
        {
            Pay_Template payTemplate = new Pay_Template();
            try
            {
                var template = await CSISContext.Pay_Template.FirstAsync();
                if(template != null) payTemplate = template;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching Pay template data");
            }
            return payTemplate;
        }
    }
}
