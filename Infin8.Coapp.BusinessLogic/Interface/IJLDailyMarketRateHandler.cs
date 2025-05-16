using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  interface IJLDailyMarketRateHandler
    {
        Task<bool> AddDailyMarketRateAsync(JL_DailyMarketRate jLDailyMarketRate);
        Task<bool> EditDailyMarketRateAsync(JL_DailyMarketRate jLDailyMarketRate);
        Task<double> GetMarketRate();
    }
}
