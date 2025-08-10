using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Utility
{
    public class TransactionStateService
    {
        public event Func<bool, Task>? OnInitiate;
        public async Task NotifyIntiate(bool result)
        {
            if(OnInitiate != null)
            {
                await OnInitiate.Invoke(result);
            }
        }
    }
}
