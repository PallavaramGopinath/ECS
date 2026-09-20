using Infin8.Coapp.Models;
//using Infin8.Coapp.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public  class PayComponentAssignmentsRepository : Repository<Pay_Component_Assignments>, IPayComponentAssignmentsRepository 
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public PayComponentAssignmentsRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<Pay_Component_Assignments>> AddPayComponentAssignments(Pay_Component_Assignments entity)
        {
            List<Pay_Component_Assignments> components = new();
            try
            {
                decimal maxId = await CSISContext.Pay_Component_Assignments.MaxAsync(x => x.Id);
                maxId++;
                entity.Id = maxId;
                await AddAsync(entity);
                await CSISContext.SaveChangesAsync();
                var query = await CSISContext.Pay_Component_Assignments.Where(x => x.BrCode == entity.BrCode).ToListAsync();
                if (query != null && query.Count > 0) components = query.ToList();
            }
            catch (Exception ex)
            {
                components = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! in addition of pay component assignments");
            }
            return components;
        }

        public async Task<List<Pay_Component_Assignments>> AddPayComponentAssignments(List<Pay_Component_Assignments> entities)
        {
            List<Pay_Component_Assignments> components = new();
            try
            {
                string brCode = entities.Select(x => x.BrCode!).First();
                decimal maxId = await CSISContext.Pay_Component_Assignments.MaxAsync(x => x.Id);
                maxId++;
                foreach (var entity in entities)
                {
                    entity.Id = maxId;
                    maxId++;
                }
                await AddRangeAsync(entities);
                //await CSISContext.SaveChangesAsync();
                var query = await CSISContext.Pay_Component_Assignments.Where(x => x.BrCode == brCode).ToListAsync();
                if (query != null && query.Count > 0) components = query.ToList();
            }
            catch (Exception ex)
            {
                components = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! in addition of pay component assignments");
            }
            return components;
        }

        public async Task<List<Pay_Component_Assignments>> EditPayComponentassignments(Pay_Component_Assignments entity)
        {
            List<Pay_Component_Assignments> components = new();
            try
            {
                await EditAsync(entity);
                await CSISContext.SaveChangesAsync();
                //var query = await CSISContext.Pay_Component_Assignments.Where(x => x.BrCode == entity.BrCode).ToListAsync();
                //if (query != null && query.Count > 0) components = query.ToList();
            }
            catch (Exception ex)
            {
                components = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! in modification of pay component assignments");
            }
            return components;
        }

        public async Task<List<Pay_Component_Assignments>> EditPayComponentassignments(List<Pay_Component_Assignments> entities)
        {
            List<Pay_Component_Assignments> components = new();
            try
            {
                string brCode = entities.Select(x=> x.BrCode!).First();
                 
                foreach (var entity in entities)
                {
                    await EditAsync(entity);
                    //await CSISContext.SaveChangesAsync();
                }
                await CSISContext.SaveChangesAsync();
                //var query = await CSISContext.Pay_Component_Assignments.Where(x => x.BrCode == brCode).ToListAsync();
                //if (query != null && query.Count > 0) components = query.ToList();
            }
            catch (Exception ex)
            {
                components = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! in modification of pay component assignments");
            }
            return components;
        }

        public async Task<List<Pay_Component_Assignments>> GetPayComponentAssignments(string brCode)
        {
            List<Pay_Component_Assignments> components = new();
            try
            {
                var query = await CSISContext.Pay_Component_Assignments.Where(x => x.BrCode == brCode).ToListAsync();
                if (query != null && query.Count > 0) components = query.ToList();
            }
            catch (Exception ex)
            {
                components = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! in fetching of pay component assignments");
            }
            return components;
        }

        public async Task<List<Pay_Component_Assignments>> GetExistingPayComponentAssignments(List<Pay_Component_Assignments> entities)
        {
            var employeeIds = entities.Select(e => e.Employee_Id).Distinct().ToList();
            var componentIds = entities.Select(e => e.Component_Id).Distinct().ToList();

            return await CSISContext.Pay_Component_Assignments
                .Where(e => employeeIds.Contains(e.Employee_Id)
                         && componentIds.Contains(e.Component_Id)
                         && e.Is_Active)  // Only active ones
                .ToListAsync();
        }
    }
}
