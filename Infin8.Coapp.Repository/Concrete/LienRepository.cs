using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public  class LienRepository : Repository<Lien>, ILienRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LienRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddLienAsync(Lien lien)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Lien.MaxAsync(x => x.Lien_Id);
                maxId++;
                lien.Lien_Id = maxId;
                await AddAsync(lien);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Lien not saved");
            }

            return result;
        }

        public async Task<bool> EditLienAsync(Lien lien)
        {
            bool result = false;
            try
            {
                lien.Lien_Delete = true;
                await EditAsync(lien);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Lien not deleted");
            }
            return result;
        }
    }
}
