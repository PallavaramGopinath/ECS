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
    public class FinLedgerTrnRepository : Repository<Fin_Ledger_Trn>, IFinLedgerTrnRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public FinLedgerTrnRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddFinLedgerTrnAsync(Fin_Ledger_Trn finLedgerTrn)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Fin_Ledger_Trn.MaxAsync(x => x.Trn_Id);
                maxId++;
                finLedgerTrn.Trn_Id = maxId;
                await AddAsync(finLedgerTrn);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! General ledger trn not saved");
            }
            return result;
        }

        public async Task<bool> AddFinLedgerTrnListAsync(List<Fin_Ledger_Trn> finLedgerTrnList)
        {
            bool result = false;
            try
            {
                //decimal maxId = await CSISContext.Fin_Ledger_Trn.MaxAsync(x => x.Trn_Id);
                //maxId++;
                //finLedgerTrn.Trn_Id = maxId;
                //await AddAsync(finLedgerTrn);
                await CSISContext.AddRangeAsync(finLedgerTrnList);
                await CSISContext.SaveChangesAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! General ledger trn not saved");
            }
            return result;
        }
        public async Task<bool> EditFinLedgerTrnAsync(Fin_Ledger_Trn finLedgerTrn)
        {
            bool result = false;
            try
            {
                finLedgerTrn.LedgerTrn_Delete = true;
                await EditAsync(finLedgerTrn);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! General ledger trn not deleted");
            }
            return result;
        }
    }
}
