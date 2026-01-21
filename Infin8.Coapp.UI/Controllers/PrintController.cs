using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Reporting.NETCore;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Security;
using System.Security.Permissions;

namespace Infin8.Coapp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrintController : ControllerBase
    {
        private readonly IReportsHandler _reportHandler;
        private readonly IUtilityHandler _utilityHandler;
        private readonly IGeneralHandler _generalHandler;
        private readonly IWebHostEnvironment _webHostEnvironment;
        string societyName = "";
        public PrintController(IReportsHandler reportsHandler,IUtilityHandler utilityHandler, 
            IGeneralHandler generalHandler, IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _reportHandler = reportsHandler;
            _utilityHandler = utilityHandler;
            _generalHandler = generalHandler;
        }

        #region commented
        //public async Task<FileContentResult> PrintMemberReceipt(decimal vocId,string brCode)
        //{
        //    rptReceiptAndPaymentAmount rptAndPmtAmt = new rptReceiptAndPaymentAmount();
        //    Reports_Master report = new Reports_Master();
        //    byte[] pdfAsBytes = Array.Empty<byte>();
        //    try
        //    {
        //        societyName = await _generalHandler.GetSocietyName(brCode);
        //        //var path = $"{this._webHostEnvironment.ContentRootPath}\\wwwroot\\Reports\\SKSMChecklist.rdlc";
        //        var path = $"{this._webHostEnvironment.ContentRootPath}\\wwwroot\\Reports\\"; // + report.ReportFileName;

        //        rptAndPmtAmt = await _reportHandler.GetReceiptAndPaymentAmount(vocId);
        //        LocalReport localReport = new LocalReport
        //        {
        //            EnableExternalImages = true,
        //        };

        //        using (FileStream stream = System.IO.File.OpenRead(path))
        //        {
        //            localReport.LoadReportDefinition(stream);
        //        }
        //        /// print member receipt
        //        if(rptAndPmtAmt.Voc_Rpt > 0)
        //        {
        //            string receiptHeader = rptAndPmtAmt.Voc_Trn_Type == 1 ? "CASH RECEIPT" : "ADJUSTMENT RECEIPT";
        //            List<rptReceiptMemberList> rptList = new List<rptReceiptMemberList>();
        //            if (rptAndPmtAmt.Voc_Type == 5 || rptAndPmtAmt.Voc_Type == 11)
        //            {
        //                return await Print_MemberReceipt(rptAndPmtAmt, vocId, brCode);
        //            }
        //            else if(rptAndPmtAmt .Voc_Type == 14)
        //            {

        //            }

        //        }
        //        /// print voucher
        //        if (rptAndPmtAmt.Voc_Pmt > 0)
        //        {

        //        }
        //        //pdfAsBytes = localReport.Render("PDF");
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return CreatePDFAsBytes(pdfAsBytes);
        //}
        #endregion 

        [HttpGet]
        [Route("GetReportNameList/{grpId:int}/{brCode}")]
        public async Task<ActionResult<List<DropdownItem>>> GetReportNameList(int grpId, string brCode)
        {
            List<DropdownItem> rptList = new();
            var response = await _reportHandler.GetReportNameList(grpId, brCode);
            if (response.Count > 0)
            {
                rptList = response.ToList();
                return Ok(rptList);
            }
            else
            {
                return NotFound();
            }
        }
        #region Status
        [HttpGet]
        [Route("GetStatus/{vocId:decimal}/{brCode}")]
        public async Task<ActionResult<List<string>>> GetStatusList(decimal vocId, string brCode)
        {
            List<string> statusList = new();
            var result = await _reportHandler.GetStatusForMemberTransaction(vocId, brCode);
            if (result.Count > 0)
            {
                statusList = result.ToList();
                return Ok(statusList);
            }
            else
            {
                return NotFound();
            }
        }
        #endregion

        #region Receipt items
        [HttpGet]
        [Route("GetReceiptAndPaymentData/{vocId:decimal}/{brCode}")]
        public async Task<ActionResult< List<rptReceiptAndPaymentAmount>>> GetReceiptAndPaymentAmountByVocId(decimal vocId, string brCode)
        {
            List<rptReceiptAndPaymentAmount> rptAndPmtAmt = new();

            rptAndPmtAmt = await _reportHandler.GetReceiptAndPaymentAmount(vocId, brCode);
            if (rptAndPmtAmt.Count  == 0)
            {
                return NotFound(); ;
            }
            return Ok(rptAndPmtAmt);
        }

        [HttpPost]
        [Route("print-receipt")]
        public async Task<FileContentResult> Print_MemberReceipt( [FromBody] rptReceiptObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string rsInWords = "";
            //double receiptAmt = 0;
            try
            {
                societyName = await  _generalHandler.GetSocietyName(rptObject.brCode!);
                string receiptHeader = rptObject.vocTrnType == 1 ? "CASH RECEIPT" : "ADJUSTMENT RECEIPT";
                List<rptReceiptMemberList> rptList = new List<rptReceiptMemberList>();
                report = await _reportHandler.GetReportNameWithSignature("Receipt");
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
                LocalReport localReport = new LocalReport
                {
                    EnableExternalImages = true,
                };

                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    localReport.LoadReportDefinition(stream);
                }
                
                var receiptData = await _reportHandler.GetReceiptData(rptObject.vocId, rptObject.brCode!);
                rptList = receiptData.receiptData;
                //receiptAmt = rptList.Sum(x => x.Voc_Rpt);
                if (rptList != null && rptList.Any())
                {
                    rsInWords = _utilityHandler.RupeesInWords(rptObject.receiptAmt);
                    DateTime? intCalcDate = null;
                    intCalcDate = receiptData.intCalcDate;
                    DataTable dth = new DataTable();
                    dth.Columns.Add("RsInWords");
                    dth.Columns.Add("SocietyName");
                    dth.Columns.Add("ReportHeader");
                    dth.Columns.Add("IntCalcDetails");
                    dth.Columns.Add("AdjString");
                    DataRow dr = dth.NewRow();
                    dr["RsInWords"] = rsInWords;
                    dr["SocietyName"] = societyName;
                    dr["ReportHeader"] = receiptHeader;
                    if (intCalcDate != null)
                        dr["IntCalcDetails"] = "Interest Calculated upto " + intCalcDate.Value.ToString("dd-MM-yyyy");
                    else
                        dr["IntCalcDetails"] = "";
                    string _adjString = await _reportHandler.GetChequeDetailsForReceipt(rptObject.vocId, rptObject.brCode!);

                    if (_adjString != null)
                    {
                        dr["AdjString"] = _adjString;
                    }
                    else
                    {
                        dr["AdjString"] = "";
                        /// if current inters date is null then get loan id and obtain last interest calculated date here.
                    }
                    dth.Rows.Add(dr);
                    var parameters = new[]
                    {
                        new ReportParameter("ParamFirstSignature", report.FirstSignature ) ,
                        new ReportParameter("ParamSecondSignature", report.SecondSignature ),
                        new ReportParameter("ParamThirdSignature", report.ThirdSignature )
                    };

                    localReport.DataSources.Add(new ReportDataSource("Ds_Receipt", rptList));
                    localReport.DataSources.Add(new ReportDataSource("Ds_Header", dth));
                    localReport.SetParameters(parameters);
                    pdfAsBytes = localReport.Render("PDF");
                }
            }
            catch (Exception)
            {

                throw;
            }
            return  CreatePDFAsBytes(pdfAsBytes);
        }

        [HttpPost]
        [Route("print-receipt-general")]
        public async Task<FileContentResult> Print_MemberReceiptGeneral([FromBody] rptReceiptObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string rsInWords = "";
            string chequeDetails = "";
            try
            {
                societyName = await _generalHandler.GetSocietyName(rptObject.brCode!);
                string receiptHeader = rptObject.vocTrnType == 1 ? "CASH RECEIPT" : "ADJUSTMENT RECEIPT";
                List<rptReceiptMemberList> rptList = new List<rptReceiptMemberList>();
                report = await _reportHandler.GetReportNameWithSignature("Receipt General");
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
                LocalReport localReport = new LocalReport
                {
                    EnableExternalImages = true,
                };

                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    localReport.LoadReportDefinition(stream);
                }

                var receiptData = await _reportHandler.GetReceiptGeneralData(rptObject.vocId, rptObject.brCode!);
                rptList = receiptData.receiptData;
                //receiptAmt = rptList.Sum(x => x.Voc_Rpt);
                rsInWords = _utilityHandler.RupeesInWords(rptObject.receiptAmt);
;
                chequeDetails = receiptData.chequeDetails;
               
                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("ParamFirstSignature", report.FirstSignature ) ,
                    new ReportParameter("ParamSecondSignature", report.SecondSignature ),
                    new ReportParameter("ParamThirdSignature", report.ThirdSignature ),
                    new ReportParameter("paramReportHeader", receiptHeader ),
                    new ReportParameter("paramRsInWords", rsInWords ),
                    new ReportParameter("paramChequeDetails", chequeDetails)
                    };

                localReport.DataSources.Add(new ReportDataSource("Ds_ReceiptGeneral", rptList));
                localReport.SetParameters(parameters);
                pdfAsBytes = localReport.Render("PDF");
            }
            catch (Exception)
            {

                throw;
            }
            return CreatePDFAsBytes(pdfAsBytes);
        }

        [HttpPost]
        [Route("print-voucher")]
        public async Task<FileContentResult> Print_Voucher([FromBody] rptReportObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string rsInWords = "";
            string chequeDetails = "";
            try
            {
                societyName = await _generalHandler.GetSocietyName(rptObject.BrCode!);
                string receiptHeader = rptObject.VocTrnType == 1 ? "CASH RECEIPT" : "ADJUSTMENT RECEIPT";
                List<rptPaymentVoucher> voucherList = new ();
                report = await _reportHandler.GetReportNameWithSignature(rptObject.ReportId); /// .GetReportNameWithSignature("Receipt General");
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
                LocalReport localReport = new LocalReport
                {
                    EnableExternalImages = true,
                };

                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    localReport.LoadReportDefinition(stream);
                }
                
                var paymentData = await _reportHandler.GetPaymentData(rptObject.Voc_Id,rptObject.BrCode! ); /// .GetReceiptGeneralData(rptObject.vocId, rptObject.brCode!);
                voucherList = paymentData.paymentData;
                chequeDetails = paymentData.chequeDetails;
                double rs = voucherList.Sum(x => x.PaymentAmt);
                rsInWords = _utilityHandler.RupeesInWords(rs);

                DataTable dth = new DataTable();
                dth.Columns.Add("RsInWords");
                dth.Columns.Add("SocietyName");
                dth.Columns.Add("ReportHeader");
                dth.Columns.Add("ChequeDetails");
                DataRow dr = dth.NewRow();
                dr["RsInWords"] = rsInWords;
                dr["SocietyName"] = societyName;
                if (rptObject.VocTrnType == 1)
                    dr["ReportHeader"] = "Payment Voucher by Cash";
                else
                    dr["ReportHeader"] = "Payment Voucher by Adjustment";
                dr["ChequeDetails"] = chequeDetails;
                dth.Rows.Add(dr);

                var parameters = new[]
                {
                    new ReportParameter("ParamFirstSignature", report.FirstSignature ) ,
                    new ReportParameter("ParamSecondSignature", report.SecondSignature ),
                    new ReportParameter("ParamThirdSignature", report.ThirdSignature ),
                };

                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_PaymentVoucher", voucherList));
                localReport.DataSources.Add(new ReportDataSource("Ds_PaymentVoucherHeader", dth));
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

        #endregion 

        #region Fixed Deposit
        [HttpPost]
        [Route("print-fdpayment")]
        public async Task<FileContentResult> Print_FDPayment([FromBody] rptReceiptObject rptObject)
        {
            Reports_Master report = new();
            byte[] pdfAsBytes = Array.Empty<byte>();
            List<rptFDPaymentList> fdPmtList = new();
            string rsInWords = "";
            string chequeDetails = "";
            try
            {
                societyName = await _generalHandler.GetSocietyName(rptObject.brCode!);
                //string receiptHeader = rptObject.vocTrnType == 1 ? "CASH RECEIPT" : "ADJUSTMENT RECEIPT";
               
                report = await _reportHandler.GetReportNameWithSignature(92);
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
                LocalReport localReport = new LocalReport
                {
                    EnableExternalImages = true,
                };

                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    localReport.LoadReportDefinition(stream);
                }

                var fdPmtData = await _reportHandler.GetFDPaymentList(rptObject.vocId);
                
                fdPmtList = fdPmtData.fdPaymentList;
                double pmtAmt = fdPmtList.Select(x => x.InterestPaidAmount + x.DepositPaidAmount).Sum();
                string FDNos = "";
                foreach (var fd in fdPmtList)
                {
                    FDNos += fd.TD_No!.Trim() + ",";
                }
                chequeDetails = fdPmtData.chequeDetails;
                //var receiptData = await _reportHandler.GetReceiptGeneralData(rptObject.vocId, rptObject.brCode!);
                //rptList = receiptData.receiptData;
                //receiptAmt = rptList.Sum(x => x.Voc_Rpt);
                //chequeDetails = receiptData.chequeDetails;

                rsInWords = _utilityHandler.RupeesInWords(pmtAmt);

                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("ParamFirstSignature", report.FirstSignature ) ,
                    new ReportParameter("ParamSecondSignature", report.SecondSignature ),
                    new ReportParameter("ParamThirdSignature", report.ThirdSignature )
                };
                DataTable dth = new DataTable();
                dth.Columns.Add("Contentent");
                dth.Columns.Add("ChequeDetails");
                dth.Columns.Add("RsInWords");
                DataRow dr = dth.NewRow();
                dr["Contentent"] = "Received from " + societyName.Trim() + " the sum of Rs. " + pmtAmt.ToString() + " (Rupees " + rsInWords + " paid towards FD No(s) " + FDNos;
                dr["RsInWords"] = rsInWords;
                dr["ChequeDetails"] = chequeDetails;
                dth.Rows.Add(dr);


                localReport.DataSources.Add(new ReportDataSource("Ds_FDPaymentList", fdPmtList));
                localReport.DataSources.Add(new ReportDataSource("Ds_FDPayment", dth));
                localReport.SetParameters(parameters);
                pdfAsBytes = localReport.Render("PDF");
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            return CreatePDFAsBytes(pdfAsBytes);
        }

        [HttpPost]
        [Route("print-fdbond")]
        public async Task<FileContentResult> Print_FDBond([FromBody] rptReceiptObject rptObject)
        {
            Reports_Master report = new();
            byte[] pdfAsBytes = Array.Empty<byte>();
            rptFDBond fDBond = new();
            try
            {
                societyName = await _generalHandler.GetSocietyName(rptObject.brCode!);
                report = await _reportHandler.GetReportNameWithSignature(116);
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
                LocalReport localReport = new LocalReport
                {
                    EnableExternalImages = true,
                };

                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    localReport.LoadReportDefinition(stream);
                }

                var fdBondData = await _reportHandler.GetFDBondPreprinted(rptObject.vocId); ///  .GetFDPaymentList(rptObject.vocId);
                if (fdBondData != null) 
                {
                    fDBond = fdBondData;
                }
                DataTable dth = new DataTable();
                dth = ObjectToDataTable(fDBond);

                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("ParamFirstSignature", report.FirstSignature ) ,
                    new ReportParameter("ParamSecondSignature", report.SecondSignature ),
                    new ReportParameter("ParamThirdSignature", report.ThirdSignature )
                };

                localReport.DataSources.Add(new ReportDataSource("Ds_FDBond", dth));
                localReport.SetParameters(parameters);
                pdfAsBytes = localReport.Render("PDF");
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            return CreatePDFAsBytes(pdfAsBytes);
        }
        #endregion

        #region Jewel Loan 
        [HttpPost]
        [Route("print-jl-ledger")]
        public async Task<FileContentResult> Print_JL_Ledger([FromBody] rptReportObject rptObject)
        {
            string imagePath = "";
            string base64Image = "";
            string imagePath2 = "";
            string base64Image2 = "";
            Reports_Master report = new();
            byte[] pdfAsBytes = Array.Empty<byte>();
            List<rptJewelLoanLedger> loanList = new();
            string rsInWords = "";
            try
            {
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

                var loanListData = await _reportHandler.GetJewelLoanLedger(rptObject.Voc_Id); ///  .GetFDPaymentList(rptObject.vocId);
                if (loanListData.Count > 0)
                {
                    loanList = loanListData.ToList();
                }
                foreach( var loan in loanList)
                {
                    //imagePath = Path.Combine(_webHostEnvironment.WebRootPath, loan.JewelsImage!);
                    //imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "jewels", "jewel_39df9efb-5ba0-49df-aca0-671e98222e46.jpg");
                    // Split and remove empty entries
                    // Output:
                    // parts[0] = "uploads"
                    // parts[1] = "jewels"
                    // parts[2] = "jewel_39df9efb-5ba0-49df-aca0-671e98222e46.jpg"
                    string[] parts = loan.JewelsImage!.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    imagePath = Path.Combine(_webHostEnvironment.WebRootPath, parts[0], parts[1], parts[2]);
                    if (imagePath.Length >0)
                    {
                        byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
                        base64Image = Convert.ToBase64String(imageBytes);
                        loan.JewelsImage =  base64Image;
                    }
                    string[] parts2 = loan.MemberPhoto!.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    imagePath2 = Path.Combine(_webHostEnvironment.WebRootPath, parts2[0], parts2[1], parts2[2]);
                    if (imagePath2.Length > 0)
                    {
                        byte[] imageBytes2 = System.IO.File.ReadAllBytes(imagePath2);
                        base64Image2 = Convert.ToBase64String(imageBytes2);
                        //string fileExtension = Path.GetExtension(imagePath2).ToLower();
                        //string mimeType = GetMimeType(fileExtension);
                        //loan.MemberPhoto = $"data:{mimeType};base64,{base64Image}";
                        loan.MemberPhoto = base64Image2;
                    }

                }
                rsInWords = _utilityHandler.RupeesInWords(loanList.Select(x => x.San_Amt).FirstOrDefault());

                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("paramRupeesInWords", rsInWords ) ,
                    new ReportParameter("ParamFirstSignature", report.FirstSignature ) ,
                    new ReportParameter("ParamSecondSignature", report.SecondSignature ),
                    new ReportParameter("ParamThirdSignature", report.ThirdSignature )
                    //new ReportParameter("ParamJewelImage", "data:image/jpeg;base64," + base64Image) // Add image parameter
                };

                localReport.DataSources.Add(new ReportDataSource("Ds_JLLedger", loanList));
                localReport.SetParameters(parameters);
                pdfAsBytes = localReport.Render("PDF");
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            return CreatePDFAsBytes(pdfAsBytes);
        }

        private string GetMimeType(string fileExtension)
        {
            return fileExtension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".tiff" or ".tif" => "image/tiff",
                ".svg" => "image/svg+xml",
                ".webp" => "image/webp",
                ".ico" => "image/x-icon",
                _ => "image/jpeg" // default
            };
        }

        #endregion 

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

        private  System.Data.DataTable ObjectToDataTable(object o)
        {
            Type t = o.GetType();
            System.Data.DataTable dt = new System.Data.DataTable(t.Name);
            DataRow dr = dt.NewRow();
            dt.Rows.Add(dr);
            o.GetType().GetProperties().ToList().ForEach(f =>
            {
                try
                {
                    f.GetValue(o, null);
                    dt.Columns.Add(f.Name, f.PropertyType);
                    dt.Rows[0][f.Name] = f.GetValue(o, null);
                }
                catch { }
            });
            return dt;
        }

        #region Print
        //[HttpPost]
        //[Route("Print_Member_Receipt")]
        //public async Task<IActionResult> Print_Member_Receipt([FromBody] rptReceiptObject rptObject)
        //{

        //    await _reportHandler.Print_Member_Receipt(rptObject);

        //}
        #endregion 

    }
}
