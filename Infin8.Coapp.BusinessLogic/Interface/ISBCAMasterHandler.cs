using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface ISBCAMasterHandler
    {
        Task<(bool result, decimal accId, string accNo)> AddSBCAMasterAsync(SBCA_Master sbcaMaster);
        Task<SBCA_Master> AddNewSBAccount(SBCA_Master sbAccount);
        Task<bool> EditSBCAMasterAsync(SBCA_Master sbcaMaster);
        Task<List<DropdownItem>> GetSBCANosByMemIdAsync(decimal memId,string brCode);
        Task<string> GetSBCANoByAccIdAsync(decimal accId, string brCode);
        Task<string> GetSBCANoByMemIdAsync(decimal memId, string brCode);
        Task<DtoSBAccountNo> GetSBAccountDataByMemIdAsync(decimal memId, string brCode);
    }
}
