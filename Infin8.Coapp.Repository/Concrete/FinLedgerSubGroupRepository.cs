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
    public  class FinLedgerSubGroupRepository : Repository<Fin_Ledger_SubGrp>, IFinLedgerSubGroupRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public FinLedgerSubGroupRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddFinLedgerSubGroupAsync(Fin_Ledger_SubGrp finLedgerSubGrp)
        {
            bool result = false;
            try
            {
                int maxId = await CSISContext.Fin_Ledger_SubGrp.MaxAsync(x => x.SubGrp_Id);
                maxId++;
                finLedgerSubGrp.SubGrp_Id = maxId;
                await AddAsync(finLedgerSubGrp);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! General ledger sub group not saved");
            }
            return result;
        }

        public async Task<bool> EditFinLedgerSubGroupAsync(Fin_Ledger_SubGrp finLedgerSubGrp)
        {
            bool result = false;
            try
            {
                finLedgerSubGrp.SubGrp_Delete = true;
                await EditAsync(finLedgerSubGrp);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! General ledger sub group not deleted");
            }
            return result;
        }
    }
}
