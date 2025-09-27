using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class StagingHistoryHandler : IStagingHistoryHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public StagingHistoryHandler(IUnitOfWork unitOfWork ) 
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddStagingHistoryList(List<Staging_History> stagingHistoryList)
        {
            return await _unitOfWork.StagingHistory.AddStagingHistoryList(stagingHistoryList);
        }
    }
}
