//using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.ReportServices.Interface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Reporting.NETCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.ReportServices.Concrete
{
    public class ReportsAudit
    {

    }
    //public class ReportsAudit : IReportsAudit
    //{
    //    private readonly IReportsFinalAccountsHandler _reportsFinalAccountHandler;
    //    private readonly IReportsHandler _reportsHandler;
    //    private readonly IGeneralHandler _generalHandler;
    //    private IWebHostEnvironment _webHostEnvironment;
    //    string societyName = "";
    //    string reportHeader = "";
    //    public ReportsAudit(IReportsFinalAccountsHandler reportsFinalAccountHandler, IReportsHandler reportsHandler,
    //        IGeneralHandler generalHandler, IWebHostEnvironment webHostEnvironment)
    //    {
    //        _reportsFinalAccountHandler = reportsFinalAccountHandler;
    //        _reportsHandler = reportsHandler;
    //        _generalHandler = generalHandler;
    //        _webHostEnvironment = webHostEnvironment;
    //    }
    //    public async Task<byte[]> GetLedgerOutstanding(rptReportAuditObject rptObject)
    //    {
    //        byte[] pdfAsBytes = Array.Empty<byte>();
    //        List<rptFALedgerTrn> ledgetList = new();
    //        Reports_Master report = new Reports_Master();
    //        try
    //        {
    //            societyName = await _generalHandler.GetSocietyName(rptObject.BrCode!);
    //            report = await _reportsHandler.GetReportNameWithSignature(rptObject.ReportId);
    //            var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
    //            var ledgerListData = await _reportsFinalAccountHandler.GetLedgerOutstandingFA(rptObject.FromDate, rptObject.ToDate, rptObject.YrId, rptObject.FnlId, rptObject.BrCode!);

    //            if (ledgerListData != null && ledgerListData.Any())
    //            {
    //                ledgetList = ledgerListData.ToList();
    //            }
    //            LocalReport localReport = new LocalReport
    //            {
    //                EnableExternalImages = true,
    //            };
    //            switch (rptObject.FnlId)
    //            {
    //                case 0:
    //                    reportHeader = "Cash on hand from  " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
    //                    break;
    //                case 1:
    //                    reportHeader = "Assets Ledger outstanding from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
    //                    break;
    //                case 2:
    //                    reportHeader = "Liability Ledger outstanding from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
    //                    break;
    //                case 3:
    //                    reportHeader = "Income Ledger outstanding from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
    //                    break;
    //                case 4:
    //                    reportHeader = "Expenditure Ledger outstanding from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
    //                    break;
    //                case 5:
    //                    reportHeader = "All Ledger outstanding from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
    //                    break;
    //            }
    //            var parameters = new[]
    //           {
    //            new ReportParameter("paramSocietyName", societyName ),
    //            new ReportParameter("paramReportHeader", reportHeader )
    //        };
    //            localReport.ReportPath = path;
    //            localReport.DataSources.Clear();
    //            localReport.DataSources.Add(new ReportDataSource("Ds_FALedgerTrn", ledgetList));
    //            localReport.SetParameters(parameters);
    //            localReport.Refresh();
    //            pdfAsBytes = localReport.Render("PDF");
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine(ex.ToString());
    //        }
    //        return pdfAsBytes;
    //    }
    //}
}
