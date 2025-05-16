using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IPaySlipRepository
    {
        Task<bool> AddPaySlipAsync(Pay_Slip paySlip);
        Task<bool> EditPaySlipAsync(Pay_Slip paySlip);
    }
}
