using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IPayGenInfoRepository
    {
        Task<bool> AddPayGenInfoAsync(Pay_Gen_Info payGenInfo);
        Task<bool> EditPayGenInfoAsync(Pay_Gen_Info payGenInfo);
        Task<Pay_Gen_Info> GetPayGenInfoAsync(decimal infoId);
        Task<List<Pay_Gen_Info>> GetPayGenInfoListAsync(string brCode);
    }
}
