using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public  interface ITermDepositTrnRepository
    {
        Task<bool> AddTermDepositTrnAsync(TermDeposit_Trn termDepositTrn);
        Task<bool> AddTermDepositTrnListAsync(List<TermDeposit_Trn> termDepositTrnList);
        Task<bool> EditTermDepositTrnAsync(TermDeposit_Trn termDepositTrn);
        Task<List<DropdownItem>> GetTDNosByMemIdAsync(decimal memId, string tdSchemeType, string brCode);
        Task<List<decimal>> GetFDIdListForDayEndCalculation(int day, string brCode);
        Task<List<FDDetailsVM>> GetFDPayableByTDIdsAsync(decimal[] fdNos);
        Task<FDDetailsVM> GetFDDataByTDId(decimal tdId, string brCode);
        Task<List<DropdownItem>> GetTDNosByMemIdForRenewal(decimal memId , string tdSchemeType, DateTime trnDate, string brCode);
        Task<DtoNominee> GetNomineeForTermDeposit(decimal memId, string tdSchemeType, string brCode);
        Task<DtoSecurityDepositData> GetSecurityDepositData(decimal empId, string brCode);
    }
}
