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
    public  class FinVoucherBankRepository : Repository<Fin_Voucher_Bank>, IFinVoucherBankRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public FinVoucherBankRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddFinVoucherBankAsync(Fin_Voucher_Bank finVoucherBank)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Fin_Voucher_Bank.MaxAsync(x => x.Fvb_Id);
                maxId++;
                finVoucherBank.Fvb_Id = maxId;
                await AddAsync(finVoucherBank);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Bank accounttransaciont not saved");
            }
            return result;
        }

        public async Task<bool> EditFinVoucherBankAsync(Fin_Voucher_Bank finVoucherBank)
        {
            bool result = false;
            try
            {
                finVoucherBank.Fvb_Delete = true;
                await EditAsync(finVoucherBank);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Bank account transaction not deleted");
            }
            return result;
        }
    }
}
