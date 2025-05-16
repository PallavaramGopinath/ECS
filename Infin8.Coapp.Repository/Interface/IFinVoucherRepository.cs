using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IFinVoucherRepository
    {
        Task<bool> AddFinVoucherAsync(Fin_Voucher finVouocher);
        Task<bool> EditFinVoucherAsync(Fin_Voucher finVouocher);
    }
}
