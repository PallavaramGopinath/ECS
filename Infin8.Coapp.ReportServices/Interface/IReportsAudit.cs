using Infin8.Coapp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Reporting.NETCore;
namespace Infin8.Coapp.ReportServices.Interface
{
    public  interface IReportsAudit
    {
        Task<byte[]> GetLedgerOutstanding(rptReportAuditObject rptObject);
    }
}
