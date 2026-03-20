using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IMemPayableMasterRepository
    {
        Task<bool> AddMemPayableMasterAsync(Mem_Payable_Master memPayableMaster);
        Task<bool> EditMemPayableMasterAsync(Mem_Payable_Master memPayableMaster);
        Task<Mem_Payable_Master> GetMemPayableMasterByPbleMasterId(decimal pbleMasterId, string brCode);
        Task<Mem_Payable_Master> GetDividendLastCalculatedData(int pbleType, string status, string brCode);
        Task<List<Mem_Payable_Master>> GetCalculatedDataList(int pbleType, string status, string brCode);
    }
}
