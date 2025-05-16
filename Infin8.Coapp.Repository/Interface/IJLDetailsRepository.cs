using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IJLDetailsRepository
    {
        Task<bool> AddJLDetailsAsync(JL_Details jlDetails);
        Task<bool> EditJLDetailsAsync(JL_Details jlDetails);
        Task<JewelLoanMarketRate> GetMarketRateAndAdoptedRateAsync();
    }
}
