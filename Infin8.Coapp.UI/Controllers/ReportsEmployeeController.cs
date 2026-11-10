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
    public class ReportsEmployeeController : ControllerBase
    {
        private readonly IReportsHandler _reportHandler;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IReportsEmployeeHandler _reportsEmployeeHandler;
        private readonly IGeneralHandler _generalHandler;
        string societyName = "";
        public ReportsEmployeeController(IReportsHandler reportsHandler,
            IWebHostEnvironment webHostEnvironment,
            IReportsEmployeeHandler  reportsEmployeeHandler,
            IGeneralHandler generalHandler )
        {
            _reportHandler = reportsHandler;
            _webHostEnvironment = webHostEnvironment;
            _reportsEmployeeHandler = reportsEmployeeHandler;
            _generalHandler = generalHandler;
        }

        [HttpPost]
        [Route("Print_PayBillOfficeNote")]
        public async Task<FileContentResult> Print_PayBillOfficeNote(rptReportEmployeeObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string reportHeader = "";
            try
            {
                societyName = await _generalHandler.GetSocietyName(rptObject.BrCode!);
                List<rptEmpPayBill> payList = new();
               
                report = await _reportHandler.GetReportNameWithSignature(rptObject.ReportId);
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
                LocalReport localReport = new LocalReport
                {
                    EnableExternalImages = true,
                };
                reportHeader = rptObject.ReportId == 66 ? "PAY BILL" : "OFFICE NOTE";
                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    localReport.LoadReportDefinition(stream);
                }
                var payBillData = await _reportsEmployeeHandler.GetEmpPayBill(rptObject.Emp_Id, rptObject.Pay_Id);
                if (payBillData != null && payBillData.Any())
                {
                    payList = payBillData.ToList();
                }
                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("paramReportHeader", reportHeader ) 
                };
                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_EmpPayBill", payList));
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
        [Route("Print_PFRegister")]
        public async Task<FileContentResult> Print_PFRegister(rptReportEmployeeObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string reportHeader = "";
            try
            {
                societyName = await _generalHandler.GetSocietyName(rptObject.BrCode!);
                List<rptEmpPF> pfList = new List<rptEmpPF>();
                string roiList = "";
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
                (var resultPFList ,var resultRoiList )  = await _reportsEmployeeHandler.GetEmpPFLedger(rptObject.Emp_Id , rptObject.FromDate, rptObject.ToDate ,rptObject.BrCode! );
                if (resultPFList != null && resultPFList.Any())
                {
                    pfList = resultPFList.ToList();
                }
                if (resultRoiList != null)
                {
                    roiList = resultRoiList;
                }
                foreach (var pf in pfList)
                {
                    pf.Total_Balance = pf.Pf_Balance + pf.Bpf_Balance;
                }
                var pfFirst = pfList.FirstOrDefault();
                reportHeader = "Providend Fund Register for " + pfFirst!.MemberName + " From " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("paramReportHeader", reportHeader ),
                    new ReportParameter("paramRoiList", roiList )
                };
                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_EmpPFRegister", pfList));
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
        [Route("Print_OneYearPayBill")]
        public async Task<FileContentResult> Print_OneYearPayBill(rptReportEmployeeObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string reportHeader = "";
            try
            {
                societyName = await _generalHandler.GetSocietyName(rptObject.BrCode!);
                List<rptEmp12MonthsSalary> payList = new();
                //string roiList = "";
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
                var result= await _reportsEmployeeHandler.GetEmployee12MonthsSalary(rptObject.FromDate, rptObject.ToDate, rptObject.BrCode!);
                if (result != null && result.Any())
                {
                    payList = result.ToList();
                }
                reportHeader = "Salary for the Period From " + rptObject.FromDate .ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
                
                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("paramReportHeader", reportHeader )
                };
                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_SalaryForTheYear", payList));
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
