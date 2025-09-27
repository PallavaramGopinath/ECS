using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IStagingHistoryHandler
    {
        Task<bool> AddStagingHistoryList(List<Staging_History> stagingHistoryList);
    }
}
