using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public  class MemPayableNotNEFTRepository : Repository<Mem_Payable_NotNEFT>, IMemPayableNotNEFTRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public MemPayableNotNEFTRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddMemPayableNotNEFTAsync(Mem_Payable_NotNEFT memPayableNotNEFT)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Mem_Payable_NotNEFT.MaxAsync(x => x.PbleCash_Id);
                maxId++;
                memPayableNotNEFT.PbleCash_Id = maxId;
                await AddAsync(memPayableNotNEFT);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Dividend payment not eligible for NEFT payment not saved");
            }
            return result;
        }

        public async Task<bool> DeleteMemPayableNotNEFTAsync(Mem_Payable_NotNEFT memPayableNotNEFT)
        {
            bool result = false;
            try
            {
                await DeleteAsync(memPayableNotNEFT);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Dividend payment not eligible for NEFT payment not modified");
            }
            return result;
        }
    }
}
