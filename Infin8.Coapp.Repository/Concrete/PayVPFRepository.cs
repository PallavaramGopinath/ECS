using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class PayVPFRepository : Repository<Pay_VPF>, IPayVPFRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public PayVPFRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddPayVPFAsync(Pay_VPF payVPF)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Pay_VPF.MaxAsync(x => x.Vpf_Id);
                maxId++;
                payVPF.Vpf_Id = maxId;
                await AddAsync(payVPF);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Voluntary PF data not saved");
            }
            return result;
        }

        public async Task<bool> EditPayVPFAsync(Pay_VPF payVPF)
        {
            bool result = false;
            try
            {
                payVPF.Vpf_Delete = true;
                await EditAsync(payVPF);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Voluntary PF data not modified");
            }
            return result;
        }
    }
}
