using Infin8.Coapp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository.Interface
{
    public  interface IReportsGBRepository
    {
        Task<List<rptDividendTDFWDWorking>> GetDividendWorkingSheet(decimal pbleMasterId, string brCode);
        Task<List<rptDividendIntOnTDPendingList>> GetDividendPendingList(DateTime asOnDate, string brCode);
        Task<List<rptDividendPaid>> GetDividendPaidList(DateTime fromDate, DateTime toDate, string brCode);

    }
}
