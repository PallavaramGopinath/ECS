using Infin8.Coapp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic.Interface
{
    public  interface IReportsGBHandler
    {
        Task<byte[]> GetDividendWorkingSheet(string datasetName, decimal pbleMasterId, string brCode, FileStream reportStream, Dictionary<string, string> parameters);
        Task<byte[]> GetDividendPendingList(string datasetName, DateTime asOnDate, string brCode, FileStream reportStream, Dictionary<string, string> parameters);
        Task<byte[]> GetDividendPaidList(string datasetName, DateTime fromDate, DateTime toDate, string brCode, FileStream reportStream, Dictionary<string, string> parameters);
    }
}
