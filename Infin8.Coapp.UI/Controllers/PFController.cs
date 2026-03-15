using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Microsoft.AspNetCore.Http;
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
        public PFController(IPayPFRoiTemplateHandler payPFRoiTemplateHandler, IEmpPFHandler empPFHandler,
            IReportsEmployeeHandler reportsEmployeeHandler  )
        {
            _payPFRoiTemplateHandler = payPFRoiTemplateHandler;
            _empPFHandler = empPFHandler;
            _reportsEmployeeHandler = reportsEmployeeHandler;
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
    }
}
