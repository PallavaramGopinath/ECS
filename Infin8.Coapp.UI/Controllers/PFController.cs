using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PFController : ControllerBase
    {
        private readonly IPayPFRoiTemplateHandler _payPFRoiTemplateHandler;
        private readonly IEmpPFHandler _empPFHandler;
        private readonly IReportsEmployeeHandler _reportsEmployeeHandler;
        private readonly IPayPFTemplateHandler _payPFTemplateHandler;
        public PFController(IPayPFRoiTemplateHandler payPFRoiTemplateHandler, IEmpPFHandler empPFHandler,
            IReportsEmployeeHandler reportsEmployeeHandler,
            IPayPFTemplateHandler payPFTemplateHandler  )
        {
            _payPFRoiTemplateHandler = payPFRoiTemplateHandler;
            _empPFHandler = empPFHandler;
            _reportsEmployeeHandler = reportsEmployeeHandler;
            _payPFTemplateHandler = payPFTemplateHandler;
        }

        [HttpGet]
        [Route("GetPFRoi/{asOnDate}/{brCode}")]
        public async Task<ActionResult> GetPFRateOfInterest (string asOnDate,string brCode)
        {
            Pay_PF_ROITemplate pf = new();
            DateTime.TryParse(asOnDate, out DateTime asOnDateFormatted);
            var result  = await  _payPFRoiTemplateHandler.GetPayPFRoiTemplateByDate(asOnDateFormatted, brCode);
            if (result != null) pf = result;
            return Ok(pf);
        }

        [HttpGet]
        [Route("GetPfRateOfInterest/{FromDate}/{ToDate}/{BrCode}")]
        public async Task<ActionResult<List<RateOfInterestVM>>> GetPFRateOfInterest(string FromDate, string ToDate, string Brcode)
        {
            List<RateOfInterestVM> roiList = new();
            DateTime.TryParse(FromDate , out DateTime FromDateFormatted);
            DateTime.TryParse(ToDate ,out DateTime ToDateDateFormatted);
            var result = await _reportsEmployeeHandler.GetPFRoi(FromDateFormatted, ToDateDateFormatted, Brcode);
            if(result !=null && result.Any())
            {
                roiList = result.ToList();
            }
            return Ok(roiList);
        }

        [HttpPost]
        [Route("Calculate-Pf-Interest")]
        public async Task<ActionResult<DtoVoucher>> CalculatePFInterestCalculation([FromBody] DtoPayPFCalculation pfObject )
        {
            DtoVoucher voucherData = new();
            var result = await _empPFHandler.CalculatePFInterestYearEnd(pfObject.FromDate, pfObject.ToDate, pfObject.UsrId, pfObject.YrId, pfObject.BrCode!);
            if(result != null && result.Voc_Id >0)
            {
                voucherData = result;
            }
            return Ok(voucherData);
            
        }

        [HttpGet]
        [Route("GetPFRoiTemplateList/{brCode}")]
        public async Task<ActionResult<List<Pay_PF_ROITemplate>>> GetPayPFRoiTemplateList(string brCode)
        {
            List<Pay_PF_ROITemplate> pfTemplateList = new();
            var result = await _payPFRoiTemplateHandler.GetPayPFROITemplateListAsync(brCode);
            if(result != null && result.Any()) pfTemplateList = result.ToList();
            return Ok(pfTemplateList);
        }

        [HttpPost]
        [Route("AddPFRoiTemplate")]
        public async Task<ActionResult<List<Pay_PF_ROITemplate>>> AddPFRoiTemplate([FromBody] Pay_PF_ROITemplate pfTemplate)
        {
            List<Pay_PF_ROITemplate> roiList = new();
            var result = await  _payPFRoiTemplateHandler.AddPayPFRoiTemplateAsync(pfTemplate);
            if (result != null && result.Count >0)  roiList = result.ToList();
            return Ok(roiList);
        }

        [HttpPost]
        [Route("EditPFRoiTemplate")]
        public async Task<ActionResult<bool>> EditPfRoiTemplate(Pay_PF_ROITemplate pfTemplate)
        {
            var result = await _payPFRoiTemplateHandler.EditPayPFRoiTemplateAsync(pfTemplate);
            return Ok(result);
        }

        #region Pay PF Template
        [HttpGet]
        [Route("GetPayPFTemplateList/{brCode}")]
        public async Task<ActionResult<List<Pay_PF_Template>>> GetPayPFTemplateList(string brCode)
        {
            List<Pay_PF_Template> list = new List<Pay_PF_Template>();
            var result = await _payPFTemplateHandler.GetPayPFTemplateListAsync(brCode);
            if (result != null) list = result;
            return Ok(list);
        }

        [HttpPost]
        [Route("AddPayPFTemplate")]
        public async Task<ActionResult<List<Pay_PF_Template>>> AddPayPFTemplate([FromBody] Pay_PF_Template pfTemplate)
        {
            List<Pay_PF_Template> list = new List<Pay_PF_Template>();
            var response = await _payPFTemplateHandler.AddPayPFTemplateAsync(pfTemplate);
            if (response != null) list = response;
            return Ok(list);
        }

        [HttpPost]
        [Route("EditPayPFTemplate")]
        public async Task<ActionResult<List<Pay_PF_Template>>> EditPayPFTemplate([FromBody] Pay_PF_Template pfTemplate)
        {
            List<Pay_PF_Template> list = new List<Pay_PF_Template>();
            var response = await _payPFTemplateHandler.EditPayPFTemplateAsync(pfTemplate);
            if (response != null) list = response;
            return Ok(list);
        }
        #endregion 
    }
}
