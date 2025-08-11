using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Reporting.NETCore;

namespace Infin8.Coapp.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsAuditController : ControllerBase
    {
        private readonly IReportsHandler _reportHandler;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IGeneralHandler _generalHandler;
        private readonly IReportsFinalAccountsHandler _reportsFinalAccountHandler;
        string societyName = "";
        string reportHeader = "";
        public ReportsAuditController(IGeneralHandler generalHandler, IReportsMemberHandler reportsMemberHandler,
            IWebHostEnvironment webHostEnvironment, IReportsHandler reportHandler, IReportsFinalAccountsHandler reportsFinalAccountHandler)
        {
            _generalHandler = generalHandler;
            _webHostEnvironment = webHostEnvironment;
            _reportHandler = reportHandler;
            _reportsFinalAccountHandler = reportsFinalAccountHandler;
        }

        [HttpPost]
        [Route("print-audit-memtrn")]
        public async Task<FileContentResult> Print_Audit_JewelLoan(rptReportAuditObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            try
            {
                List<rptMemberTrn> memTrnList = new();
                societyName = await _generalHandler.GetSocietyName(rptObject.BrCode!);
                report = await _reportHandler.GetReportNameWithSignature(rptObject.ReportId);
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
                LocalReport localReport = new LocalReport
                {
                    EnableExternalImages = true,
                };

                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    localReport.LoadReportDefinition(stream);
                }
                switch (rptObject.TrnType)
                {
                    case 3:
                        reportHeader = "Member's Share Capital Schedule from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
                        break;
                    case 2:
                        reportHeader = "Member's Due By Schedule from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
                        break;
                    case 1:
                        reportHeader = "Member's Due To Schedule from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
                        break;
                    case 5:
                        reportHeader = "Staff Due To Schedule from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
                        break;
                    case 6:
                        reportHeader = "Staff Due By Schedule from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
                        break;
                    case 7:
                        reportHeader = "Member's Savings Bank Account Schedule from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
                        break;
                }
                var memTrnListData = await _reportsFinalAccountHandler.GetRptMemberTrnSchedule(rptObject.FromDate, rptObject.ToDate, rptObject.TrnType, rptObject.BrCode);
                //var memTrnListData = await _reportsMemberHandler.GetRptMemberTrn(rptObject.FromDate ,  rptObject.ToDate,rptObject.TrnType ,rptObject.BrCode);
                if (memTrnListData != null && memTrnListData.Any())
                {
                    memTrnList = memTrnListData.ToList();
                }
                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ),
                    new ReportParameter("paramReportHeader", reportHeader )
                };

                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_ScheduleMemberTrn", memTrnList));
                localReport.SetParameters(parameters);
                localReport.Refresh();
                pdfAsBytes = localReport.Render("PDF");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return CreatePDFAsBytes(pdfAsBytes);
        }

        [HttpPost]
        [Route("print-audit-jewelloan")]
        public async Task<FileContentResult> Print_Audit_MemTrn(rptReportAuditObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            try
            {
                List<rptFALoanOutstanding> jlList = new();
                societyName = await _generalHandler.GetSocietyName(rptObject.BrCode!);
                report = await _reportHandler.GetReportNameWithSignature(rptObject.ReportId);
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
                LocalReport localReport = new LocalReport
                {
                    EnableExternalImages = true,
                };

                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    localReport.LoadReportDefinition(stream);
                }

                reportHeader = "Jewel Loan outstanding Schedule from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");

                var jlListData = await _reportsFinalAccountHandler.GetLoanOutstandingForFA_HL(rptObject.FromDate, rptObject.ToDate, rptObject.LoanType, rptObject.BrCode);

                if (jlListData != null && jlListData.Any())
                {
                    jlList = jlListData.ToList();
                }
                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ),
                    new ReportParameter("paramReportHeader", reportHeader )
                };

                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_FALoanOutstanding", jlList));
                localReport.SetParameters(parameters);
                localReport.Refresh();
                pdfAsBytes = localReport.Render("PDF");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return CreatePDFAsBytes(pdfAsBytes);
        }

        [HttpPost]
        [Route("print-audit-termdeposit")]
        public async Task<FileContentResult> Print_Audit_TermDeposit(rptReportAuditObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            try
            {
                List<rptTermDepositOutstanding> tdList = new();
                societyName = await _generalHandler.GetSocietyName(rptObject.BrCode!);
                report = await _reportHandler.GetReportNameWithSignature(rptObject.ReportId);
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
                LocalReport localReport = new LocalReport
                {
                    EnableExternalImages = true,
                };

                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    localReport.LoadReportDefinition(stream);
                }
                if(rptObject.TDType =="F")
                    reportHeader = "Fixed deposit outstanding from Schedule from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
                else
                    reportHeader = "Recurring deposit outstanding from Schedule from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");

                var tdListData = await _reportsFinalAccountHandler.GetTermDepositOutstandingFA(rptObject.FromDate, rptObject.ToDate, rptObject.TDType, rptObject.BrCode);

                if (tdListData != null && tdListData.Any())
                {
                    tdList = tdListData.ToList();
                }
                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ),
                    new ReportParameter("paramReportHeader", reportHeader )
                };

                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_TermDepositOutstanding", tdList));
                localReport.SetParameters(parameters);
                localReport.Refresh();
                pdfAsBytes = localReport.Render("PDF");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return CreatePDFAsBytes(pdfAsBytes);
        }

        [HttpPost]
        [Route("print-audit-ledger")]
        public async Task<FileContentResult> Print_Audit_Ledger(rptReportAuditObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            try
            {
                List<rptFALedgerTrn> ledgetList = new();
                societyName = await _generalHandler.GetSocietyName(rptObject.BrCode!);
                report = await _reportHandler.GetReportNameWithSignature(rptObject.ReportId);
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
                LocalReport localReport = new LocalReport
                {
                    EnableExternalImages = true,
                };

                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    localReport.LoadReportDefinition(stream);
                }
                switch (rptObject.FnlId)
                {
                    case 0:
                        reportHeader = "Cash on hand from  " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
                        break;
                    case 1:
                        reportHeader = "Assets Ledger outstanding from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
                        break;
                    case 2:
                        reportHeader = "Liability Ledger outstanding from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
                        break;
                    case 3:
                        reportHeader = "Income Ledger outstanding from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
                        break;
                    case 4:
                        reportHeader = "Expenditure Ledger outstanding from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
                        break;
                    case 5:
                        reportHeader = "All Ledger outstanding from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
                        break;
                }

                var ledgerListData = await _reportsFinalAccountHandler.GetLedgerOutstandingFA(rptObject.FromDate, rptObject.ToDate, rptObject.YrId, rptObject.FnlId, rptObject.BrCode);

                if (ledgerListData != null && ledgerListData.Any())
                {
                    ledgetList = ledgerListData.ToList();
                }
                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ),
                    new ReportParameter("paramReportHeader", reportHeader )
                };

                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_FALedgerTrn", ledgetList));
                localReport.SetParameters(parameters);
                localReport.Refresh();
                pdfAsBytes = localReport.Render("PDF");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return CreatePDFAsBytes(pdfAsBytes);
        }

        #region Create PDF AS Bytes
        private FileContentResult CreatePDFAsBytes(byte[] pdfAsBytes)
        {
            try
            {
                //var PDFDoc = File(pdfAsBytes, "application/pdf", "report.pdf");
                return File(pdfAsBytes, "application/pdf");
            }
            catch (Exception)
            {
                return File(Array.Empty<byte>(), "application/pdf");
            }
        }
        #endregion 
    }
}
