using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Reporting.NETCore;
using System.Data;

namespace Infin8.Coapp.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsTDController : ControllerBase
    {
        private readonly IReportsHandler _reportHandler;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IReportsTermDepositsHandler _reportsTermDepositsHandler;
        private readonly IGeneralHandler _generalHandler;
        string societyName = "";
        public ReportsTDController(IWebHostEnvironment webHostEnvironment, IReportsHandler reportsHandler,
            IReportsTermDepositsHandler reportsTermDepositsHandler, IGeneralHandler generalHandler)
        {
            _reportHandler = reportsHandler;
            _webHostEnvironment = webHostEnvironment;
            _reportsTermDepositsHandler = reportsTermDepositsHandler;
            _generalHandler = generalHandler;
        }

        [HttpGet]
        [Route("GetTDNos/{fromDate}/{toDate}/{tdSchemeType}/{brCode}")]
        public async Task<ActionResult<List<DropdownItem>>> GetTDNos(int tdSchemeId, string fromDate, string toDate, string tdSchemeType, string brCode)
        {
            List<DropdownItem> loanlist = new();
            DateTime.TryParse(fromDate, out DateTime fromDateParse);
            DateTime.TryParse(toDate, out DateTime toDateParse);

            var result = await _reportsTermDepositsHandler.GetTDNos(tdSchemeType, fromDateParse, toDateParse, brCode);
            if (result != null && result.Any())
            {
                loanlist = result.ToList();
                return Ok(loanlist);
            }
            else
                return NotFound();
        }
        [HttpGet]
        [Route("GetTDNosByMemId/{memId:decimal}/{fromDate}/{toDate}/{tdSchemeType}/{brCode}")]
        public async Task<ActionResult<List<DropdownItem>>> GetTDNos(decimal memId, int tdSchemeId, string fromDate, string toDate, string tdSchemeType, string brCode)
        {
            List<DropdownItem> loanlist = new();
            DateTime.TryParse(fromDate, out DateTime fromDateParse);
            DateTime.TryParse(toDate, out DateTime toDateParse);

            var result = await _reportsTermDepositsHandler.GetTDNos(memId, tdSchemeType, fromDateParse, toDateParse, brCode);
            if (result != null && result.Any())
            {
                loanlist = result.ToList();
                return Ok(loanlist);
            }
            else
                return NotFound();
        }

        [HttpPost]
        [Route("print-termdeposit-register")]
        public async Task<FileContentResult> Print_TermDeposit_Register(rptReportTDObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            try
            {
                List<rptTermDepositRegister> tdRegisterList = new();
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

                var tdList = await _reportsTermDepositsHandler.GetTermDepositRegister(rptObject.TDList!, rptObject.FromDate, rptObject.ToDate, rptObject.TDSchemeType!);
                if (tdList != null && tdList.Any())
                {
                    tdRegisterList = tdList.ToList();
                }
                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName )
                };

                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_FixedDepositRegister", tdRegisterList));
                localReport.SetParameters(parameters);
                localReport.Refresh();
                pdfAsBytes = localReport.Render("PDF");
            }
            catch (Exception)
            {

                throw;
            }
            return CreatePDFAsBytes(pdfAsBytes);
        }

        [HttpPost]
        [Route("print-termdeposit-outstanding")]
        public async Task<FileContentResult> Print_TermDeposit_Outstanding(rptReportTDObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string reportHeader = "";
            try
            {
                List<rptFDOutstanding> tdOutstandingList = new();
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

                var tdList = await _reportsTermDepositsHandler.GetTermDepositOutstanding(rptObject.AsOnDate, rptObject.TDSchemeType!, rptObject.BrCode!);
                if (tdList != null && tdList.Any())
                {
                    tdOutstandingList = tdList.ToList();
                }

                if (rptObject.TDSchemeType == "F")
                {
                    if (rptObject.ReportId == 42)
                        reportHeader = "Fixed Deposit Outstanding as on " + rptObject.AsOnDate.ToString("dd-MM-yyyy") + " FD No-Wise";
                    if (rptObject.ReportId == 41)
                    {
                        reportHeader = "Fixed Deposit Outstanding as on " + rptObject.AsOnDate.ToString("dd-MM-yyyy") + " Mem No-Wise";
                        tdList = tdList!.OrderBy(x => x.MemberNo).ThenBy(x => x.TD_No).ToList();
                    }
                    if (rptObject.ReportId == 123)
                    {
                        reportHeader = "Fixed Deposit Outstanding";
                    }
                }

                if (rptObject.TDSchemeType == "R")
                {
                    if (rptObject.ReportId == 52)
                        reportHeader = "Recurring Deposit Outstanding as on " + rptObject.AsOnDate.ToString("dd-MM-yyyy") + " RD No-wise";
                    if (rptObject.ReportId == 51)
                        reportHeader = "Recurring Deposit Outstanding as on " + rptObject.AsOnDate.ToString("dd-MM-yyyy") + " Mem No-wise";
                }
                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ),
                     new ReportParameter("paramReportHeader", reportHeader )
                };

                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_FixedDepositOutstanding", tdOutstandingList));
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
        [Route("print-termdeposit-outstanding-byMemId")]
        public async Task<FileContentResult> Print_TermDeposit_Outstanding_ByMemId(rptReportTDObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string reportHeader = "";
            try
            {
                List<rptFDOutstanding> tdOutstandingList = new();
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

                var tdList = await _reportsTermDepositsHandler.GetTermDepositOutstandingIndividual(rptObject.MemId, rptObject.AsOnDate, rptObject.TDSchemeType!, rptObject.BrCode!);
                if (tdList != null && tdList.Any())
                {
                    tdOutstandingList = tdList.ToList();
                }

                if (rptObject.TDSchemeType == "F")
                {
                    if (rptObject.ReportId == 42)
                        reportHeader = "Fixed Deposit Outstanding as on " + rptObject.AsOnDate.ToString("dd-MM-yyyy") + " FD No-Wise";
                    if (rptObject.ReportId == 41)
                    {
                        reportHeader = "Fixed Deposit Outstanding as on " + rptObject.AsOnDate.ToString("dd-MM-yyyy") + " Mem No-Wise";
                        tdList = tdList!.OrderBy(x => x.MemberNo).ThenBy(x => x.TD_No).ToList();
                    }
                    if (rptObject.ReportId == 123)
                    {
                        reportHeader = "Fixed Deposit Outstanding for the Member " + rptObject.MemberNo + " as on " + rptObject.AsOnDate.ToString("dd-MM-yyyy");
                    }
                }

                if (rptObject.TDSchemeType == "R")
                {
                    if (rptObject.ReportId == 52)
                        reportHeader = "Recurring Deposit Outstanding as on " + rptObject.AsOnDate.ToString("dd-MM-yyyy") + " RD No-wise";
                    if (rptObject.ReportId == 51)
                        reportHeader = "Recurring Deposit Outstanding as on " + rptObject.AsOnDate.ToString("dd-MM-yyyy") + " Mem No-wise";
                }
                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ),
                     new ReportParameter("paramReportHeader", reportHeader )
                };

                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_FixedDepositOutstanding", tdOutstandingList));
                localReport.SetParameters(parameters);
                localReport.Refresh();
                pdfAsBytes = localReport.Render("PDF");
            }
            catch (Exception)
            {

                throw;
            }
            return CreatePDFAsBytes(pdfAsBytes);
        }

        [HttpPost]
        [Route("print-termdeposit-InterestPayable")]
        public async Task<FileContentResult> Print_TermDeposit_InterestPayable(rptReportTDObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string reportHeader = "";
            try
            {
                List<rptTermDepositPayable> tdPayableList = new();
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

                var tdList = await _reportsTermDepositsHandler.GetTermDepositPayable(rptObject.AsOnDate, rptObject.TDSchemeType!, rptObject.BrCode!);
                if (tdList != null && tdList.Any())
                {
                    tdPayableList = tdList.ToList();
                }
                reportHeader = "Term Deposit Payable as on " + rptObject.AsOnDate.ToString("dd-MM-yyyy");

                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ),
                     new ReportParameter("paramReportHeader", reportHeader )
                };

                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_TermDepositPayable", tdPayableList));
                localReport.SetParameters(parameters);
                localReport.Refresh();
                pdfAsBytes = localReport.Render("PDF");
            }
            catch (Exception)
            {

                throw;
            }
            return CreatePDFAsBytes(pdfAsBytes);
        }

        [HttpPost]
        [Route("print-termdeposit-maturityamount-payable")]
        public async Task<FileContentResult> Print_TermDeposit_MaturityAmount_Payable(rptReportTDObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string reportHeader = "";
            try
            {
                List<rptTermDepositPayable> tdPayableList = new();
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

                var tdList = await _reportsTermDepositsHandler.GetTermDepositMaturityPayable(rptObject.AsOnDate, rptObject.TDSchemeType!, rptObject.BrCode!);
                if (tdList != null && tdList.Any())
                {
                    tdPayableList = tdList.ToList();
                }
                reportHeader = "Term Deposit Payable as on " + rptObject.AsOnDate.ToString("dd-MM-yyyy");

                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ),
                     new ReportParameter("paramReportHeader", reportHeader )
                };

                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_TermDepositPayable", tdPayableList));
                localReport.SetParameters(parameters);
                localReport.Refresh();
                pdfAsBytes = localReport.Render("PDF");
            }
            catch (Exception)
            {

                throw;
            }
            return CreatePDFAsBytes(pdfAsBytes);
        }

        [HttpPost]
        [Route("print-termdeposit-received-duringperiod")]
        public async Task<FileContentResult> Print_TermDeposit_Received_DuringPeriod(rptReportTDObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string reportHeader = "";
            try
            {
                List<rptTDNewBetweenDates> tdReceivedList = new();
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

                var tdList = await _reportsTermDepositsHandler.GetTermDepositrReceivedDuringPeriod(rptObject.FromDate, rptObject.ToDate, rptObject.TDSchemeType!, rptObject.BrCode!);
                if (tdList != null && tdList.Any())
                {
                    tdReceivedList = tdList.ToList();
                }
                if (rptObject.TDSchemeType == "F")
                {
                    if (rptObject.ReportId == 120)
                        reportHeader = "New Fixed Deposit for the period from  " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
                }

                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ),
                     new ReportParameter("paramReportHeader", reportHeader )
                };

                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_NewTDBetweenDates", tdReceivedList));
                localReport.SetParameters(parameters);
                localReport.Refresh();
                pdfAsBytes = localReport.Render("PDF");
            }
            catch (Exception)
            {

                throw;
            }
            return CreatePDFAsBytes(pdfAsBytes);
        }

        [HttpPost]
        [Route("print-termdeposit-refund-duringperiod")]
        public async Task<FileContentResult> Print_TermDeposit_Refund_DuringPeriod(rptReportTDObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string reportHeader = "";
            try
            {
                List<rptTDRefundBetweenDates> tdRefundList = new();
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

                var tdList = await _reportsTermDepositsHandler.GetFDRefundBetweenDated(rptObject.FromDate, rptObject.ToDate, rptObject.TDSchemeType!, rptObject.BrCode!);
                if (tdList != null && tdList.Any())
                {
                    tdRefundList = tdList.ToList();
                }
                reportHeader = "Fixed Deposit Refund between  " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");

                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ),
                     new ReportParameter("paramReportHeader", reportHeader )
                };

                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_TDRefundBetweenDates", tdRefundList));
                localReport.SetParameters(parameters);
                localReport.Refresh();
                pdfAsBytes = localReport.Render("PDF");
            }
            catch (Exception ex)
            {
                Console.WriteLine (ex.Message );
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
