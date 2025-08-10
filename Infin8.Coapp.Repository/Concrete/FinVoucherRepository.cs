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
    public class FinVoucherRepository : Repository<Fin_Voucher>, IFinVoucherRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public FinVoucherRepository(DbContext context) : base(context)
        {
        }

        public async Task<(bool result, decimal vocId)> AddFinVoucherAsync(Fin_Voucher finVouocher)
        {
            bool result = false;
            decimal vocId = 0;
            try
            {
                decimal maxId = await CSISContext.Fin_Voucher.MaxAsync(x => x.Voc_Id);
                maxId++;
                vocId = maxId;
                finVouocher.Voc_Id = maxId;
                await AddAsync(finVouocher);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Voucher not saved");
            }
            return (result,vocId);
        }

        public async Task<bool> EditFinVoucherAsync(Fin_Voucher finVouocher)
        {
            bool result = false;
            try
            {
                finVouocher.Voc_Delete = true;
                await EditAsync(finVouocher);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Foucher not deleted");
            }
            return result;
        }
    }
}
