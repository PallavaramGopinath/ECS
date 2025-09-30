using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IFinVoucherTrnRepository
    {
        Task<bool> AddFinVoucherTrnAsync(Fin_Voucher_Trn finVoucherTrn);
        Task<bool> AddFinVoucherTrnList(List<Fin_Voucher_Trn> finVoucherTrnList);
        Task<bool> EditFinVoucherTrnAsync(Fin_Voucher_Trn finVoucherTrn);
        Task<double> GetLedgerBalanceByLedIdAsync(decimal ledId,decimal cashLedId,decimal yearId);
        Task<double> GetLedgerBalance(decimal ledId, decimal yrId, DateTime upToDate,string brCode);
        Task<decimal> GetCashLedgerId(string brCode);
        Task<DtoVoucher> GetTransactionById(decimal vocId,string brCode);
        Task<DtoVoucher> GetTransactionByIds(decimal vocId, decimal yrId, string brCode);
        Task<DtoVoucher> GetTransactionByNo(string rptNo, string pmtNo, decimal yrId,string brCode);
    }
}
