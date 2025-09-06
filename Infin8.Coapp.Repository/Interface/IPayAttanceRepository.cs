using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IPayAttanceRepository
    {
        Task<bool> AddPayAttanceAsync(Pay_Att payAtt);
        Task<bool> EditPayAttanceAsync(Pay_Att payAtt);
        Task<Pay_Att> GetPayAttanceByEmpId(decimal empId, decimal payId,string brCode);

    }
}
