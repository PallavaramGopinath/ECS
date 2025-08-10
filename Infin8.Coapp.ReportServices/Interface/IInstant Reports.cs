using Infin8.Coapp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.ReportServices.Interface
{
    public interface IInstant_Reports
    {
        Task Print_Member_Receipt(rptReceiptObject rptObject);
    }
}
