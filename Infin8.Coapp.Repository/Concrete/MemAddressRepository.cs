using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public  class MemAddressRepository : Repository<Mem_Address>, IMemAddressRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public MemAddressRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddMemAddressAsync(Mem_Address memAddress)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Mem_Address.MaxAsync(x => x.address_Id);
                maxId++;
                memAddress.address_Id = maxId;
                await AddAsync(memAddress);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Member address not saved");
            }
            return result;
        }

        public async Task<bool> EditMemAddressAsync(Mem_Address memAddress)
        {
            bool result = false;
            try
            {
                memAddress.address_delete = true;
                await EditAsync(memAddress);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Member address not modified");
            }
            return result;
        }
    }
}
