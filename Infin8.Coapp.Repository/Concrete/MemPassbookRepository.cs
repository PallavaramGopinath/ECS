using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public  class MemPassbookRepository : Repository<Mem_PassBook>, IMemPassbookRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public MemPassbookRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddMemPassbookAsync(Mem_PassBook memPassbook)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Mem_PassBook.MaxAsync(x => x.PassBook_Id);
                maxId++;
                memPassbook.PassBook_Id  = maxId;
                await AddAsync(memPassbook);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Member passbook data not saved");
            }
            return result;
        }

        public async Task<bool> DeleteMemPassbookAsync(Mem_PassBook memPassbook)
        {
            bool result = false;
            try
            {
                await DeleteAsync(memPassbook);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Member passbook data not deleted");
            }
            return result;
        }
    }
}
