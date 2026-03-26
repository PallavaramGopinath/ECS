using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IFinVoucherBankHandler
    {
        Task<bool> AddFinVoucherBankAsync(Fin_Voucher_Bank finVoucherBank);
        Task<bool> AddFinVoucherBankListAsync(List<Fin_Voucher_Bank> finVoucherBankList);
        Task<bool> EditFinVoucherBankAsync(Fin_Voucher_Bank finVoucherBank);
    }
}
