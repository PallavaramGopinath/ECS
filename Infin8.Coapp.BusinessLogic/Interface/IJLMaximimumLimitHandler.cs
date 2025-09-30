using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IJLMaximimumLimitHandler
    {
        Task<bool> AddJLMaximumLimitAsync(JL_Max_Limit jLMaxLimit);
        Task<bool> EditJLMaximumLimitAsync(JL_Max_Limit jLMaxLimit);
        Task<List<JL_Max_Limit>> GetJLMaximumLimitListAsync();
        Task<double> GetJLMaximumLimitAsync(DateTime wef,string brCode);

    }
}
