using Infin8.Coapp.Models;
//using Infin8.Coapp.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public  class PayComponentRepository : Repository<Pay_Components>, IPayComponentRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public PayComponentRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<Pay_Components>> AddPayComponent(Pay_Components component)
        {
            List<Pay_Components> components = [];
            try
            {
                decimal maxId = await CSISContext.Pay_Components.MaxAsync(x=> x.Component_Id);
                maxId++;
                component.Component_Id = maxId;
                await AddAsync(component);
                await CSISContext.SaveChangesAsync();
                var query = await CSISContext.Pay_Components.Where(x => x.BrCode == component.BrCode).ToListAsync();
                if (query != null && query.Count > 0) components = [.. query];
            }
            catch (Exception ex)
            {
                components = [];
                throw new InvalidOperationException(ex.Message + " Something went wrong! in addition of pay component");
            }
            return components;
        }

        public async Task<List<Pay_Components>> EditPayComponent(Pay_Components component)
        { 
           List<Pay_Components> components = [];
            try
            {
                await EditAsync(component);
                await CSISContext.SaveChangesAsync();
                var query = await CSISContext.Pay_Components.Where(x => x.BrCode == component.BrCode).ToListAsync();
                if (query != null && query.Count > 0) components = [.. query];
            }
            catch (Exception ex)
            {
                components = [];
                throw new InvalidOperationException(ex.Message + " Something went wrong! in modification of pay component");
            }
            return components;
        }

        public async Task<List<Pay_Components>> GetPayComponents(string brCode)
        {
            List<Pay_Components> components = [];
            try
            {
                var query = await CSISContext.Pay_Components.Where(x => x.BrCode == brCode).ToListAsync();
                if (query != null && query.Count > 0) components = [.. query];
            }
            catch (Exception ex)
            {
                components = [];
                throw new InvalidOperationException(ex.Message + " Something went wrong! in fetching pay components");
            }
            return components;
        }
    }
}
