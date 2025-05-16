using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class JLOrnmentRepository : Repository<JL_Ornments>, IJLOrnmentRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public JLOrnmentRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddJLOrnmentListAsync(List<JL_Ornments> jLOrnmentList)
        {
            bool result = false;
            decimal maxId = 0;
            try
            {
                foreach (var jl in jLOrnmentList) 
                {
                    maxId = await CSISContext.JL_Ornments.MaxAsync(x => x.JLO_Id);
                    maxId++;
                    jl.JLO_Id = maxId;
                    await AddAsync(jl);
                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan ornment not saved");
            }

            return result;
        }

        public async Task<bool> EditJLOrnmentListAsync(List<JL_Ornments> jLOrnmentList)
        {
            bool result = false;
            try
            {
                foreach(var jl in jLOrnmentList)
                {
                    jl.JLO_Delete = true;
                    await EditAsync(jl);
                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan ornment not deleted");
            }
            return result;
        }
    }
}
