using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class MemPayableRepository : Repository<Mem_Payable>, IMemPayableRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public MemPayableRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddMemPayableAsync(Mem_Payable memPayable)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Mem_Payable.MaxAsync(x => x.Pble_Id);
                maxId++;
                memPayable.Pble_Id  = maxId;
                await AddAsync(memPayable);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Dividend calculation data not saved");
            }
            return result;
        }

        public async Task<bool> EditMemPayableAsync(Mem_Payable memPayable)
        {
            bool result = false;
            try
            {
                memPayable.Pble_Delete = true;
                await EditAsync(memPayable);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Dividend calculation data not modified");
            }
            return result;
        }
    }
}
