using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface ITermDepositMasterRepository
    {
        Task<bool> AddTermDepositMasterAsync(TermDeposit_Master termDepositMaster);
        Task<bool> EditTermDepositMasterAsync(TermDeposit_Master termDepositMaster);
        Task<FDRenewalObject> GetFDRnewalObjectByMemIdAsync(decimal memId);
        Task<DateTime> GetMaxMaturityDate(decimal[] tdIds, DateTime toDate);
        Task<List<FDDataForLoan>> GetFDDetailsForLoan(decimal[] tdIds);

    }
}
