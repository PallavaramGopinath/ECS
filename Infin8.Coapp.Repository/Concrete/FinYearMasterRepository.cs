using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class FinYearMasterRepository : Repository<Fin_Yr_Master>, IFinYearMasterRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public FinYearMasterRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddFinYearMasterAsync(Fin_Yr_Master finYrMaster)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Fin_Yr_Master.MaxAsync(x => x.Yr_Id);
                maxId++;
                finYrMaster.Yr_Id = maxId;
                await AddAsync(finYrMaster);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! New financial year not saved");
            }
            return result;
        }

        public async Task<Fin_Yr_Master> GetWorkingYear()
        {
            Fin_Yr_Master currentYear = new();
            var result = await CSISContext.Fin_Yr_Master
            .Where(f => f.Yr_Id == CSISContext.Fin_Yr_Master.Max(f2 => f2.Yr_Id))
            .FirstOrDefaultAsync();
            if (result != null && result.Yr_Id >0)
            {
                currentYear = result;
            }
            return currentYear;
        }

        //public async Task<bool> EditFinYearMasterAsync(Fin_Yr_Master finYrMaster)
        //{
        //    bool result = false;
        //    try
        //    {
        //        finYrMaster.dele = true;
        //        await EditAsync(finLedger);
        //        result = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        result = false;
        //        throw new InvalidOperationException(ex.Message + " Something went wrong! General ledger not deleted");
        //    }
        //    return result;
        //}
    }
}
