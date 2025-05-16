using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class PaySlipTrnRepository : Repository<Pay_Slip_Trn>, IPaySlipTrnRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public PaySlipTrnRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddPaySlipTrnAsync(Pay_Slip_Trn paySlipTrn)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Pay_Slip_Trn.MaxAsync(x => x.Pay_tr_Id);
                maxId++;
                paySlipTrn.Pay_tr_Id = maxId;
                await AddAsync(paySlipTrn);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Pay slip transactions not saved");
            }
            return result;
        }

        public async Task<bool> EditPaySlipTrnAsync(Pay_Slip_Trn paySlipTrn)
        {
            bool result = false;
            try
            {
                paySlipTrn.PayTr_Delete = true;
                await EditAsync(paySlipTrn);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Pay slip transactions not modified");
            }
            return result;
        }
    }
}
