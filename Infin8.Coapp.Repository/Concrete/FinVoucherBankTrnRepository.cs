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
    public class FinVoucherBankTrnRepository: Repository<Fin_Voucher_Bank_Trn>, IFinVoucherBankTrnRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public FinVoucherBankTrnRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddFinVoucherBankTrAsync(Fin_Voucher_Bank_Trn finVoucherBankTrn)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Fin_Voucher_Bank_Trn.MaxAsync(x => x.Fvb_Tr_Id);
                maxId++;
                finVoucherBankTrn.Fvb_Tr_Id = maxId;
                await AddAsync(finVoucherBankTrn);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Bank account transction (trn) not saved");
            }
            return result;
        }

        public async Task<bool> EditFinVoucherBankTrAsync(Fin_Voucher_Bank_Trn finVoucherBankTrn)
        {
            bool result = false;
            try
            {
                finVoucherBankTrn.FvbTr_Delete = true;
                await EditAsync(finVoucherBankTrn);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Bank account transaction (trn) not deleted");
            }
            return result;
        }
    }
}
