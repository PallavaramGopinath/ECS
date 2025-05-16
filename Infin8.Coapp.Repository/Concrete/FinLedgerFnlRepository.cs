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
    public class FinLedgerFnlRepository : Repository<Fin_Ledger_Fnl>, IFinLedgerFnlRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public FinLedgerFnlRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<DropdownItem>> GetFinLedgerFnlListAsync()
        {
            List<DropdownItem> list = new List<DropdownItem>();
            try
            {
                list = await (from fnl in CSISContext.Fin_Ledger_Fnl
                              select new DropdownItem
                              {
                                  Value = fnl.Fnl_Id.ToString(),
                                  Text = fnl.Fnl_Name
                              }).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching ledger final account list");
            }
            return list;
        }
    }
}
