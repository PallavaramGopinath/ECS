using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Reporting.NETCore;
using Microsoft.ReportingServices.Interfaces;
using System.Data;

namespace Infin8.Coapp.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsJLController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IGeneralHandler _generalHandler;
        private readonly IReportsHandler _reportHandler;
        private readonly IReportsJewelLoanHandler _reportsJewelLoanHandler;
        private readonly IJLMaximimumLimitHandler _maximimumLimitHandler;
        private readonly IUtilityHandler _utilityHandler;
        private DateTime fromDateForController;
        private DateTime toDateForController;
        private string brCodeForController ="";
        string societyName = "";
        public ReportsJLController( IWebHostEnvironment webHostEnvironment,
            IReportsJewelLoanHandler reportsJewelLoanHandler, IReportsHandler reportHandler, 
            IGeneralHandler generalHandler, IJLMaximimumLimitHandler jLMaximimumLimitHandler,
            IUtilityHandler  utilityHandler)
        {
            _webHostEnvironment = webHostEnvironment;
            _reportsJewelLoanHandler = reportsJewelLoanHandler;
            _reportHandler = reportHandler;
            _generalHandler = generalHandler;
            _maximimumLimitHandler = jLMaximimumLimitHandler;
            _utilityHandler = utilityHandler;
        }

        [HttpGet]
        [Route("GetJLMaximumLimit/{asOnDate}")]
        public async Task<ActionResult<double>> GetJLMaximumLimit(string asOnDate)
        {
            DateTime.TryParse(asOnDate, out DateTime asOnDateParse);
            double maxLimit = 0;
            var result = await _maximimumLimitHandler.GetJLMaximumLimitAsync(asOnDateParse);
            double.TryParse(result.ToString(), out maxLimit);
            return maxLimit;
        }

        [HttpPost]
        [Route("print-jewelloan-verification")]
        public async Task<FileContentResult> Print_JewelLoan_Verification(rptReportJewelObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string reportHeader = "";
            try
            {
                List<rptJewelLoanVerificationList> verificationList = new();
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

                var jlList = await _reportsJewelLoanHandler.GetJewelLoanVerificationList(rptObject.FromDate, rptObject.BrCode!);
                if (jlList != null && jlList.Any())
                {
                    verificationList = jlList.ToList();
                }
                DataTable dth = new DataTable();
                dth.Columns.Add("Disb_Amt");
                dth.Columns.Add("RatePerGram");
                dth.Columns.Add("GrossWeight");
                dth.Columns.Add("NetWeight");

                dth.Columns.Add("NetValue");
                dth.Columns.Add("GrossValue");
                dth.Columns.Add("Loan_OS");
                dth.Columns.Add("JLO_Nos");
                dth.Columns.Add("Wastage");
                DataRow dr = dth.NewRow();
                dr["Disb_Amt"] = verificationList.Sum(x => x.Disb_Amt); /// Convert.ToInt64(verificationTotal.Disb_Amt);
                dr["RatePerGram"] = verificationList.Select(x => x.RatePerGram).FirstOrDefault(); ///  verificationTotal.RatePerGram;
                dr["GrossWeight"] = String.Format("{0:0.000}", verificationList.Sum(x => x.GrossWeight)); /// verificationTotal.GrossWeight);
                dr["NetWeight"] = String.Format("{0:0.000}", verificationList.Sum(x => x.NetWeight)); ///  verificationTotal.NetWeight);
                dr["Wastage"] = String.Format("{0:0.000}", verificationList.Sum(x => x.GrossWeight) - verificationList.Sum(x => x.NetValue)); /// verificationTotal.GrossWeight - verificationTotal.NetWeight);

                dr["NetValue"] = verificationList.Sum(x => x.NetValue); ///  Convert.ToInt64(verificationTotal.NetValue);
                dr["GrossValue"] = verificationList.Sum(x => x.GrossWeight) * verificationList.Select(x => x.RatePerGram).FirstOrDefault(); ///  Convert.ToInt64(verificationTotal.GrossValue);
                dr["Loan_OS"] = verificationList.Sum(x => x.Loan_OS); /// Convert.ToInt64(verificationTotal.Loan_OS);
                dr["JLO_Nos"] = verificationList.Sum(x => x.JLO_Nos); /// verificationTotal.JLO_Nos;

                dth.Rows.Add(dr);
                reportHeader = "Jewel Loan Verification List as on " + rptObject.AsOnDate.ToString("dd-MM-yyyy");
                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("paramReportHeader", reportHeader )
                };
                
                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_JLVerification", verificationList));
                localReport.DataSources.Add(new ReportDataSource("Ds_JLVerificationTotal", dth));
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
        [Route("print-jewelloan-stockregister")]
        public async Task<FileContentResult> Print_JewelLoan_StockRegister(rptReportJewelObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string reportHeader = "";
            try
            {
                List<rptJewelLoanStockRegisterList> stockList = new();
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

                var jlList = await _reportsJewelLoanHandler.GetJewelLoanStockRegisteList(rptObject.FromDate,rptObject.ToDate,2, rptObject.BrCode!);
                if (jlList != null && jlList.Any())
                {
                    stockList = jlList.ToList();
                }
                
                reportHeader = "Jewel Loan Stock Register from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " To " + rptObject.ToDate.ToString("dd-MM-yyyy");
                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("paramReportHeader", reportHeader )
                };

                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_JLStockRegister", stockList));
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
        [Route("print-jewelloan-outstanding")]
        public async Task<FileContentResult> Print_JewelLoan_Outstanding(rptReportJewelObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string reportHeader = "";
            try
            {
                List<rptJewelLoanOutstandingList> osList = new();
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

                var osListData = await _reportsJewelLoanHandler.GetJewelLoanOutstandingList(rptObject.AsOnDate ,rptObject.BrCode!);
                if (osListData != null && osListData.Any())
                {
                    osList = osListData.ToList();
                }

                reportHeader = "Jewel Loan outstanding List as on " + rptObject.AsOnDate.ToString("dd-MM-yyyy");
                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("paramReportHeader", reportHeader )
                };

                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_JLOutstanding", osList));
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
        [Route("print-jewelloan-overdue")]
        public async Task<FileContentResult> Print_JewelLoan_Overdue(rptReportJewelObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string reportHeader = "";
            try
            {
                List<rptJewelLoanOverdueList> odList = new();
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

                var odListData = await _reportsJewelLoanHandler.GetJewelLoanOverdueList(rptObject.AsOnDate, rptObject.BrCode!);
                if (odListData != null && odListData.Any())
                {
                    odList = odListData.ToList();
                }

                reportHeader = "Jewel Loan overdue List as on " + rptObject.AsOnDate.ToString("dd-MM-yyyy");
                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("paramReportHeader", reportHeader )
                };

                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_JLOvedue", odList));
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
        [Route("print-jewelloan-aboveLimit")]
        public async Task<FileContentResult> Print_JewelLoan_AboveLimit(rptReportJewelObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string reportHeader = "";
            try
            {
                List<rptJewelLoanOverdueList> odList = new();
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

                var odListData = await _reportsJewelLoanHandler.GetJewelLoanAboveLimitList(rptObject.AsOnDate,rptObject.MaximumLimit , rptObject.BrCode!);
                if (odListData != null && odListData.Any())
                {
                    odList = odListData.ToList();
                }

                reportHeader = "Jewel Loan above limit amount of Rs. " + rptObject.MaximumLimit.ToString() + " as on " + rptObject.AsOnDate.ToString("dd-MM-yyyy");
                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("paramReportHeader", reportHeader )
                };

                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_JLAboveLimit", odList));
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
        [Route("print-jewelloan-issueRegister")]
        public async Task<FileContentResult> Print_JewelLoan_IssueRegister(rptReportJewelObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string reportHeader = "";
            try
            {
                List<rptJewelLoanIssueRegister> loanList = new();
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

                var loanListData = await _reportsJewelLoanHandler.GetJewelLoanIssueRegisterList(rptObject.FromDate ,rptObject.ToDate, rptObject.BrCode!);
                if (loanListData != null && loanListData.Any())
                {
                    loanList = loanListData.ToList();
                }

                reportHeader = "Jewel Loan Issue Register from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " To " + rptObject.ToDate.ToString("dd-MM-yyyy");
                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("paramReportHeader", reportHeader )
                };

                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_JLIssueRegister", loanList));
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
        [Route("print-jewelloan-redemption")]
        public async Task<FileContentResult> Print_JewelLoan_Redemption(rptReportJewelObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string reportHeader = "";
            try
            {
                List<rptJewelLoanRedemption> loanList = new();
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

                var loanListData = await _reportsJewelLoanHandler.GetJewelLoanRedemption(rptObject.FromDate, rptObject.ToDate, rptObject.BrCode!);
                if (loanListData != null && loanListData.Any())
                {
                    loanList = loanListData.ToList();
                }

                reportHeader = "Jewel Loan Redemption Statement from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " To " + rptObject.ToDate.ToString("dd-MM-yyyy");
                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("paramReportHeader", reportHeader )
                };

                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_JLRedemption", loanList));
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
        [Route("print-jewelloan-claim")]
        public async Task<FileContentResult> Print_JewelLoan_Claim(rptReportJewelObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string reportHeader = "";
            string rsInWords = "";
            try
            {
                List<rptJewelLoanClaim> loanList = new();
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

                var loanListData = await _reportsJewelLoanHandler.GetJewelLoanClaim(rptObject.FromDate, rptObject.ToDate, rptObject.BrCode!);
                if (loanListData != null && loanListData.Any())
                {
                    loanList = loanListData.ToList();
                }
                rsInWords = _utilityHandler.RupeesInWords(loanList.Select(x => x.San_Amt).Sum());
                reportHeader = "Disbursement claim for the Jewel Loan " + rptObject.FromDate.ToString("dd-MM-yyyy") + " To " + rptObject.ToDate.ToString("dd-MM-yyyy");
                string asOnDate = rptObject.AsOnDate.ToString("dd-MM-yyyy");
                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("paramReportHeader", reportHeader ),
                    new ReportParameter("paramDistrict", rptObject.DistrictName ),
                    new ReportParameter("paramSARDBankBranch", rptObject.RegionalOfficeName ),
                    new ReportParameter("paramRupeesInWords", rsInWords ),
                    new ReportParameter("paramClaimDate", asOnDate),
                    new ReportParameter("ParamThirdSignature",report.ThirdSignature),
                };
                                         
                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_JLClaim", loanList));
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

        [HttpGet]
        [Route("GetJLNos/{memId:decimal}/{fromDate}/{toDate}/{brCode}")]
        public async Task<ActionResult<List<DropdownItem>>> GetJLNos(decimal memId, string fromDate, string toDate, string brCode)
        {
            List<DropdownItem> loanlist = new();
            DateTime.TryParse( fromDate , out DateTime fromDateParse);
            DateTime.TryParse(toDate, out DateTime toDateParse);
            if (memId == 0)
            {
                var result = await _reportsJewelLoanHandler.GetJewelLoanNosForLedger(fromDateParse, toDateParse, brCode);
                if (result != null && result.Any())
                {
                    loanlist = result.ToList();
                    return Ok(loanlist);
                }
                else
                    return NotFound();
            }
            else
            {
                var result = await _reportsJewelLoanHandler.GetJewelLoanNosForLedger(memId, fromDateParse, toDateParse, brCode);
                if (result != null && result.Any())
                {
                    loanlist = result.ToList();
                    return Ok(loanlist);
                }
                else
                    return NotFound();
            }
        }

        [HttpPost]
        [Route("print-jewelloan-ledger")]
        public async Task<FileContentResult> Print_JewelLoan_Ledger(rptReportJewelObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            try
            {
                fromDateForController = rptObject.FromDate;
                toDateForController = rptObject.ToDate;
                brCodeForController = rptObject.BrCode!;
                List<rptJewelLoanLedgerMain> loanList = new();
                societyName = await _generalHandler.GetSocietyName(rptObject.BrCode!);
                report = await _reportHandler.GetReportNameWithSignature(rptObject.ReportId);
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
                LocalReport localReport = new LocalReport
                {
                    EnableExternalImages = true,
                };

                localReport.SubreportProcessing += new
                    SubreportProcessingEventHandler(OnSubreportProcessing);

                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    localReport.LoadReportDefinition(stream);
                }

                var subreportPath = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\JewelLoan_LedgerSubOrnments.rdlc";

                using (var stream = System.IO.File.OpenRead(subreportPath))
                {
                    localReport.LoadSubreportDefinition("Ornments", stream);
                }

                var subreportPath2 = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\JewelLoan_LedgerSubTrn.rdlc";

                using (var stream = System.IO.File.OpenRead(subreportPath2))
                {
                    localReport.LoadSubreportDefinition("LoanTrn", stream);
                }

                var loanListData = await _reportsJewelLoanHandler.GetJewelLoanLedger(rptObject.LoanNoList!);
                if (loanListData != null && loanListData.Any())
                {
                    loanList = loanListData.ToList();
                }
                
                string asOnDate = rptObject.AsOnDate.ToString("dd-MM-yyyy");
                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("ParamFirstSignature", report.FirstSignature ) ,
                    new ReportParameter("ParamSecondSignature", report.SecondSignature ),
                    new ReportParameter("ParamThirdSignature", report.ThirdSignature )
                };

                //localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_JLMain", loanList));
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

        private void SubreportProcessing_Handler(object sender, SubreportProcessingEventArgs e)
        {
            try
            {
                // Get the subreport name from the event
                string subreportName = e.ReportPath;

                // Parse LoanId parameter
                decimal loanId = decimal.Parse(e.Parameters["LoanId"].Values[0].ToString());

                // Handle different subreports based on their names
                switch (subreportName.ToLower())
                {
                    case "JewelLoan_LedgerSubOrnments": // Make sure this matches your RDLC subreport name exactly
                        HandleOrnmentsSubreport(e, loanId);
                        break;

                    case "JewelLoan_LedgerSubTrn": // Make sure this matches your RDLC subreport name exactly  
                        HandleTransactionSubreport(e, loanId);
                        break;

                    default:
                        Console.WriteLine($"Unknown subreport: {subreportName}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in subreport processing: {ex.Message}");
                throw; // Re-throw to see the actual error
            }
        }

        private void HandleOrnmentsSubreport(SubreportProcessingEventArgs e, decimal loanId)
        {
            try
            {
                List<rptJewelLoanLedgerOrnmentsSub> ornmentsList = new List<rptJewelLoanLedgerOrnmentsSub>();
                var ornmentsListData = _reportsJewelLoanHandler.GetJewelLoanLedgerOrnmentsSub(loanId);

                if (ornmentsListData != null && ornmentsListData.Any())
                {
                    ornmentsList = ornmentsListData.ToList();
                }

                e.DataSources.Add(new ReportDataSource("Ds_JLSubOrnments", ornmentsList));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Ornments subreport: {ex.Message}");
                throw;
            }
        }

        private void HandleTransactionSubreport(SubreportProcessingEventArgs e, decimal loanId)
        {
            try
            {
                List<rptJewelLoanLedgerTrnSub> loanTrnList = new List<rptJewelLoanLedgerTrnSub>();
                loanTrnList = _reportsJewelLoanHandler.GetJewelLoanLedgerTrnSub(
                    loanId,
                    fromDateForController,
                    toDateForController,
                    brCodeForController
                );

                e.DataSources.Add(new ReportDataSource("Ds_JLSubTrn", loanTrnList));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Transaction subreport: {ex.Message}");
                throw;
            }
        }
        private void SubreportProcessingOrnments(object sender, SubreportProcessingEventArgs e)
        {
            try
            {
                

                // Add this line to load the subreport definition.
                // The path should be relative to your project's content root.
                //var subreportPath = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\JewelLoan_LedgerSubOrnments.rdlc";
                //LocalReport localReport = new LocalReport
                //{
                //    EnableExternalImages = true,
                //};
                //using (var stream = System.IO.File.OpenRead(subreportPath))
                //{
                //    localReport.LoadReportDefinition(stream);
                //    //e.ReportDefinition = new LocalReport().LoadReportDefinition(stream);
                //}



                decimal loanId = decimal.Parse(e.Parameters["LoanId"].Values[0].ToString());
                List<rptJewelLoanLedgerOrnmentsSub> ornmentsList = new List<rptJewelLoanLedgerOrnmentsSub>();
                var ornmentsListData =  _reportsJewelLoanHandler.GetJewelLoanLedgerOrnmentsSub(loanId);
                if (ornmentsListData != null && ornmentsListData.Any()) ornmentsList = ornmentsListData.ToList();
                //if (errorMessage.Length > 0)
                //{
                //    MessageBox.Show(errorMessage + "\n error in obtain Jewel loan sub report (Ornments)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
                e.DataSources.Add(new ReportDataSource("Ds_JLSubOrnments", ornmentsList));
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message + " error in Jewel Loan sub report (Ornments)");
            }
        }

        private void OnSubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            try
            {
                // 1. Get the subreport name from the event arguments
                //string subreportName = e.ReportPath;

                // 2. Construct the full path to the subreport's .rdlc file
                //var subreportPath = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\{subreportName}.rdlc";

                //// 3. Create a new LocalReport instance for the subreport
                //LocalReport subReport = new LocalReport();

                //// 4. Load the subreport definition from the file
                //using (var stream = System.IO.File.OpenRead(subreportPath))
                //{
                //    subReport.LoadReportDefinition(stream);
                //}

                decimal loanId = decimal.Parse(e.Parameters["LoanId"].Values[0].ToString());
               
                //e.DataSources.Add(new ReportDataSource("Ds_JLSubTrn", loanTrnList));
                
                if (e.ReportPath == "LoanTrn") // name as in RDLC, not file name
                {
                    List<rptJewelLoanLedgerTrnSub> loanTrnList = new List<rptJewelLoanLedgerTrnSub>();
                    loanTrnList = _reportsJewelLoanHandler.GetJewelLoanLedgerTrnSub(loanId, fromDateForController, toDateForController, brCodeForController);
                    e.DataSources.Add(new ReportDataSource("Ds_JLSubTrn", loanTrnList));
                }
                else if(e.ReportPath == "Ornments")
                {
                    List<rptJewelLoanLedgerOrnmentsSub> ornmentsList = new List<rptJewelLoanLedgerOrnmentsSub>();
                    var ornmentsListData = _reportsJewelLoanHandler.GetJewelLoanLedgerOrnmentsSub(loanId);
                    e.DataSources.Add(new ReportDataSource("Ds_JLSubOrnments", ornmentsListData));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine (ex.Message + " error in Jewel Loan sub report");
            }
        }

        [HttpPost]
        [Route("print-jewelloan-ledger-alternative")]
        public async Task<FileContentResult> Print_JewelLoan_Ledger_Alternative(rptReportJewelObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            try
            {
                fromDateForController = rptObject.FromDate;
                toDateForController = rptObject.ToDate;
                brCodeForController = rptObject.BrCode;

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

                // Get main data
                var loanListData = await _reportsJewelLoanHandler.GetJewelLoanLedger(rptObject.LoanNoList!);
                List<rptJewelLoanLedgerMain> loanList = new();
                if (loanListData != null && loanListData.Any())
                {
                    loanList = loanListData.ToList();
                }

                // PRE-LOAD ALL SUBREPORT DATA
                List<rptJewelLoanLedgerOrnmentsSub> allOrnmentsList = new();
                List<rptJewelLoanLedgerTrnSub> allTransactionsList = new();

                foreach (var loan in loanList)
                {
                    // Load ornaments data for each loan
                    var ornmentsData = _reportsJewelLoanHandler.GetJewelLoanLedgerOrnmentsSub(loan.Loan_Id);
                    if (ornmentsData != null && ornmentsData.Any())
                    {
                        allOrnmentsList.AddRange(ornmentsData);
                    }

                    // Load transaction data for each loan
                    var transactionData = _reportsJewelLoanHandler.GetJewelLoanLedgerTrnSub(
                        loan.Loan_Id,
                        fromDateForController,
                        toDateForController,
                        brCodeForController!
                    );
                    if (transactionData != null && transactionData.Any())
                    {
                        allTransactionsList.AddRange(transactionData);
                    }
                }

                string asOnDate = rptObject.AsOnDate.ToString("dd-MM-yyyy");
                var parameters = new[]
                {
            new ReportParameter("paramSocietyName", societyName ) ,
            new ReportParameter("ParamFirstSignature", report.FirstSignature ) ,
            new ReportParameter("ParamSecondSignature", report.SecondSignature ),
            new ReportParameter("ParamThirdSignature", report.ThirdSignature )
        };

                // Set up event handler (still needed but will have pre-loaded data)
                localReport.SubreportProcessing += (sender, e) =>
                {
                    decimal loanId = decimal.Parse(e.Parameters["LoanId"].Values[0].ToString());

                    switch (e.ReportPath)
                    {
                        case "JewelLoan_LedgerSubOrnments":
                            var ornmentsForLoan = allOrnmentsList.Where(x => x.Loan_Id == loanId).ToList();
                            e.DataSources.Add(new ReportDataSource("Ds_JLSubOrnments", ornmentsForLoan));
                            break;

                        case "JewelLoan_LedgerSubTrn":
                            var transactionsForLoan = allTransactionsList.Where(x => x.Loan_Id == loanId).ToList();
                            e.DataSources.Add(new ReportDataSource("Ds_JLSubTrn", transactionsForLoan));
                            break;
                    }
                };

                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_JLMain", loanList));
                localReport.SetParameters(parameters);
                localReport.Refresh();
                pdfAsBytes = localReport.Render("PDF");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Report generation error: {ex}");
                throw;
            }
            return CreatePDFAsBytes(pdfAsBytes);
        }

        [HttpPost]
        [Route("print-jewelloan-ledger2")]
        public async Task<FileContentResult> Print_JewelLoan_Ledger2(rptReportJewelObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            try
            {
                fromDateForController = rptObject.FromDate;
                toDateForController = rptObject.ToDate;
                brCodeForController = rptObject.BrCode;

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

                // Get main data
                var loanListData = await _reportsJewelLoanHandler.GetJewelLoanLedger(rptObject.LoanNoList!);
                List<rptJewelLoanLedgerMain> loanList = new();
                if (loanListData != null && loanListData.Any())
                {
                    loanList = loanListData.ToList();
                }

                // CRITICAL: Pre-load all subreport data before setting up the event handler
                var ornmentsCache = new Dictionary<decimal, List<rptJewelLoanLedgerOrnmentsSub>>();
                var transactionsCache = new Dictionary<decimal, List<rptJewelLoanLedgerTrnSub>>();

                foreach (var loan in loanList)
                {
                    // Cache ornaments data
                    var ornmentsData = _reportsJewelLoanHandler.GetJewelLoanLedgerOrnmentsSub(loan.Loan_Id);
                    ornmentsCache[loan.Loan_Id] = ornmentsData?.ToList() ?? new List<rptJewelLoanLedgerOrnmentsSub>();

                    // Cache transaction data
                    var transactionData = _reportsJewelLoanHandler.GetJewelLoanLedgerTrnSub(
                        loan.Loan_Id,
                        fromDateForController,
                        toDateForController,
                        brCodeForController
                    );
                    transactionsCache[loan.Loan_Id] = transactionData?.ToList() ?? new List<rptJewelLoanLedgerTrnSub>();
                }

                // Set up the subreport event handler with cached data
                localReport.SubreportProcessing += (sender, e) =>
                {
                    try
                    {
                        Console.WriteLine($"Subreport processing: {e.ReportPath}");

                        //if (!e.Parameters.ContainsKey("LoanId") || !e.Parameters["LoanId"].Values.Any())
                        //{
                        //    Console.WriteLine("LoanId parameter missing");
                        //    return;
                        //}
                        var loanIdParam = e.Parameters["LoanId"];
                        if (loanIdParam == null || loanIdParam.Values == null || !loanIdParam.Values.Any())
                        {
                            Console.WriteLine("LoanId parameter missing or empty");
                            return;
                        }
                        //if (!decimal.TryParse(e.Parameters["LoanId"].Values[0].ToString(), out decimal loanId))
                        //{
                        //    Console.WriteLine($"Invalid LoanId: {e.Parameters["LoanId"].Values[0]}");
                        //    return;
                        //}
                        if (!decimal.TryParse(loanIdParam.Values[0].ToString(), out decimal loanId))
                        {
                            Console.WriteLine($"Invalid LoanId: {loanIdParam.Values[0]}");
                            return;
                        }
                        Console.WriteLine($"Processing LoanId: {loanId} for subreport: {e.ReportPath}");

                        // Clear any existing data sources
                        e.DataSources.Clear();

                        // Handle different subreports
                        switch (e.ReportPath)
                        {
                            case "JewelLoan_LedgerSubOrnments":
                                if (ornmentsCache.ContainsKey(loanId))
                                {
                                    var ornmentsData = ornmentsCache[loanId];
                                    Console.WriteLine($"Adding {ornmentsData.Count} ornaments records");
                                    e.DataSources.Add(new ReportDataSource("Ds_JLSubOrnments", ornmentsData));
                                }
                                else
                                {
                                    Console.WriteLine($"No ornaments data found for LoanId: {loanId}");
                                    e.DataSources.Add(new ReportDataSource("Ds_JLSubOrnments", new List<rptJewelLoanLedgerOrnmentsSub>()));
                                }
                                break;

                            case "JewelLoan_LedgerSubTrn":
                                if (transactionsCache.ContainsKey(loanId))
                                {
                                    var transactionData = transactionsCache[loanId];
                                    Console.WriteLine($"Adding {transactionData.Count} transaction records");
                                    e.DataSources.Add(new ReportDataSource("Ds_JLSubTrn", transactionData));
                                }
                                else
                                {
                                    Console.WriteLine($"No transaction data found for LoanId: {loanId}");
                                    e.DataSources.Add(new ReportDataSource("Ds_JLSubTrn", new List<rptJewelLoanLedgerTrnSub>()));
                                }
                                break;

                            default:
                                Console.WriteLine($"Unknown subreport: {e.ReportPath}");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in subreport handler: {ex}");
                    }
                };

                string asOnDate = rptObject.AsOnDate.ToString("dd-MM-yyyy");
                var parameters = new[]
                {
            new ReportParameter("paramSocietyName", societyName),
            new ReportParameter("ParamFirstSignature", report.FirstSignature),
            new ReportParameter("ParamSecondSignature", report.SecondSignature),
            new ReportParameter("ParamThirdSignature", report.ThirdSignature)
        };

                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_JLMain", loanList));
                localReport.SetParameters(parameters);

                // Force refresh to trigger subreport processing
                localReport.Refresh();
                pdfAsBytes = localReport.Render("PDF");

                Console.WriteLine($"Report rendered successfully. PDF size: {pdfAsBytes.Length} bytes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Report generation error: {ex}");
                throw;
            }
            return CreatePDFAsBytes(pdfAsBytes);
        }

        [HttpPost]
        [Route("print-jewelloan-ledger-fullpath")]
        public async Task<FileContentResult> Print_JewelLoan_Ledger_FullPath(rptReportJewelObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            try
            {
                fromDateForController = rptObject.FromDate;
                toDateForController = rptObject.ToDate;
                brCodeForController = rptObject.BrCode;

                societyName = await _generalHandler.GetSocietyName(rptObject.BrCode!);
                report = await _reportHandler.GetReportNameWithSignature(rptObject.ReportId);

                // Get full paths for all report files
                var reportsPath = Path.Combine(this._webHostEnvironment.ContentRootPath, "Reports");
                var mainReportPath = Path.Combine(reportsPath, report.ReportFileName!);
                var ornmentsReportPath = Path.Combine(reportsPath, "JewelLoan_LedgerSubOrnments.rdlc");
                var transactionReportPath = Path.Combine(reportsPath, "JewelLoan_LedgerSubTrn.rdlc");

                // Debug: Check if files exist
                Console.WriteLine($"Reports folder: {reportsPath} - Exists: {Directory.Exists(reportsPath)}");
                Console.WriteLine($"Main report: {mainReportPath} - Exists: {System.IO.File.Exists(mainReportPath)}");
                Console.WriteLine($"Ornments report: {ornmentsReportPath} - Exists: {System.IO.File.Exists(ornmentsReportPath)}");
                Console.WriteLine($"Transaction report: {transactionReportPath} - Exists: {System.IO.File.Exists(transactionReportPath)}");

                if (!System.IO.File.Exists(ornmentsReportPath))
                {
                    throw new FileNotFoundException($"Ornments subreport not found: {ornmentsReportPath}");
                }

                if (!System.IO.File.Exists(transactionReportPath))
                {
                    throw new FileNotFoundException($"Transaction subreport not found: {transactionReportPath}");
                }

                LocalReport localReport = new LocalReport
                {
                    EnableExternalImages = true,
                };

                using (FileStream stream = System.IO.File.OpenRead(mainReportPath))
                {
                    localReport.LoadReportDefinition(stream);
                }

                // Get main data
                var loanListData = await _reportsJewelLoanHandler.GetJewelLoanLedger(rptObject.LoanNoList!);
                List<rptJewelLoanLedgerMain> loanList = new();
                if (loanListData != null && loanListData.Any())
                {
                    loanList = loanListData.ToList();
                }

                // Pre-load all subreport data
                var ornmentsCache = new Dictionary<decimal, List<rptJewelLoanLedgerOrnmentsSub>>();
                var transactionsCache = new Dictionary<decimal, List<rptJewelLoanLedgerTrnSub>>();

                foreach (var loan in loanList)
                {
                    var ornmentsData = _reportsJewelLoanHandler.GetJewelLoanLedgerOrnmentsSub(loan.Loan_Id);
                    ornmentsCache[loan.Loan_Id] = ornmentsData?.ToList() ?? new List<rptJewelLoanLedgerOrnmentsSub>();

                    var transactionData = _reportsJewelLoanHandler.GetJewelLoanLedgerTrnSub(
                        loan.Loan_Id, fromDateForController, toDateForController, brCodeForController!);
                    transactionsCache[loan.Loan_Id] = transactionData?.ToList() ?? new List<rptJewelLoanLedgerTrnSub>();
                }

                // Enhanced subreport handler that loads the subreport definitions
                localReport.SubreportProcessing += (sender, e) =>
                {
                    try
                    {
                        Console.WriteLine($"Processing subreport: {e.ReportPath}");

                        // Load the appropriate subreport definition
                        string subreportFilePath = e.ReportPath switch
                        {
                            "JewelLoan_LedgerSubOrnments" => ornmentsReportPath,
                            "JewelLoan_LedgerSubTrn" => transactionReportPath,
                            _ => null
                        };

                        if (!string.IsNullOrEmpty(subreportFilePath) && System.IO.File.Exists(subreportFilePath))
                        {
                            Console.WriteLine($"Loading subreport from: {subreportFilePath}");

                            // This is a workaround - manually load subreport definition
                            // Note: This might not work directly, but it's worth trying
                            try
                            {
                                using var subreportStream = System.IO.File.OpenRead(subreportFilePath);
                                var subreportContent = new byte[subreportStream.Length];
                                subreportStream.Read(subreportContent, 0, (int)subreportStream.Length);
                                // The LocalReport API doesn't directly support setting subreport definitions
                                // This is one of the limitations of RDLC in web applications
                            }
                            catch (Exception loadEx)
                            {
                                Console.WriteLine($"Failed to load subreport definition: {loadEx.Message}");
                            }
                        }

                        // Get parameters and data as before
                        var loanIdParam = e.Parameters["LoanId"];
                        if (loanIdParam?.Values?.Any() == true &&
                            decimal.TryParse(loanIdParam.Values[0].ToString(), out decimal loanId))
                        {
                            e.DataSources.Clear();

                            switch (e.ReportPath)
                            {
                                case "JewelLoan_LedgerSubOrnments":
                                    if (ornmentsCache.TryGetValue(loanId, out var ornmentsData))
                                    {
                                        e.DataSources.Add(new ReportDataSource("Ds_JLSubOrnments", ornmentsData));
                                    }
                                    break;

                                case "JewelLoan_LedgerSubTrn":
                                    if (transactionsCache.TryGetValue(loanId, out var transactionData))
                                    {
                                        e.DataSources.Add(new ReportDataSource("Ds_JLSubTrn", transactionData));
                                    }
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Subreport processing error: {ex}");
                    }
                };

                var parameters = new[]
                {
            new ReportParameter("paramSocietyName", societyName),
            new ReportParameter("ParamFirstSignature", report.FirstSignature),
            new ReportParameter("ParamSecondSignature", report.SecondSignature),
            new ReportParameter("ParamThirdSignature", report.ThirdSignature)
        };

                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_JLMain", loanList));
                localReport.SetParameters(parameters);
                localReport.Refresh();
                pdfAsBytes = localReport.Render("PDF");

                Console.WriteLine($"Report generated successfully. Size: {pdfAsBytes.Length} bytes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Report generation error: {ex}");
                throw;
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
