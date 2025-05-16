using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IPayBasicPayHandler
    {
        Task<bool> AddPayBasicPayAsync(Pay_BasicPay payBasicPay);
        Task<bool> EditPayBasicPayAsync(Pay_BasicPay payBasicPay);
    }
}
