using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IFinVoucherBankTrnHandler
    {
        Task<bool> AddFinVoucherBankTrAsync(Fin_Voucher_Bank_Trn finVoucherBankTrn);
        Task<bool> EditFinVoucherBankTrAsync(Fin_Voucher_Bank_Trn finVoucherBankTrn);
    }
}
