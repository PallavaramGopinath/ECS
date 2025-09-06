using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;

namespace Infin8.Coapp.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayController : ControllerBase
    {
        readonly IPaySlipHandler _paySlipHandler;
        readonly IEmpMasterHandler _empMasterHandler;
        public PayController( IPaySlipHandler paySlipHandler, IEmpMasterHandler empMasterHandler)
        {
            _paySlipHandler = paySlipHandler;
            _empMasterHandler = empMasterHandler;
        }

        [HttpGet]
        [Route("GetEmployeeList/{brCode}")]
        public async Task<ActionResult<List<EmployeeMasterDto>>> GetEmployeeList(string brCode)
        {
            List<EmployeeMasterDto> list = new List<EmployeeMasterDto>();
            var result = await _empMasterHandler.GetEmployeeMasterListAsync(brCode);
            if (result != null) 
            { 
                list = result; 
                return Ok(list);
            }
            else
            {
                return NotFound("No Data Found");
            }
        }

        [HttpGet]
        [Route("GetLastPayInfo")]
        public async Task<ActionResult<List<DtoEmployeeLastPayInfo>>> GetLastPayInfo()
        {
            List<DtoEmployeeLastPayInfo> list = new List<DtoEmployeeLastPayInfo>();
            var result = await _paySlipHandler.GetEmployeeLastPayInfo();
            if(result != null) 
            { 
                list = result; 
                return Ok(list);
            }
            else
            {
                return NotFound("No Data Found");
            }
        }

        [HttpGet]
        [Route("GetPayComponentAssignmentsByEmployeeId/{empId:decimal}/{brCode}")]
        public async Task<ActionResult<List<DtoPayComponentAssignments>>> GetPayComponentAssignmentsByEmployeeId(decimal empId, string brCode)
        {
            List<DtoPayComponentAssignments> list = new List<DtoPayComponentAssignments>();
            var result = await _paySlipHandler.GetPayComponentAssignmentsByEmployeeId(empId, brCode);
            if (result != null)
            {
                list = result;
                return Ok(list);
            }
            else
            {
                return NotFound("No Data Found");
            }
        }

        [HttpPost]
        [Route("CalculatePaySlip")]
        public async Task<IActionResult> CalculatePaySlip([FromBody] DtoPaySlip paySlip)
        {
            try
            {
                if (paySlip == null)
                {
                    return BadRequest("PaySlip data is required");
                }

                DtoPaySlip paySlipCalc = new DtoPaySlip();
                var result = await _paySlipHandler.CalculatePaySlip(paySlip);
                if (result != null)
                {
                    paySlipCalc = result;
                    return Ok(paySlipCalc);
                }
                else
                {
                    return NotFound("No Data found");
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in CalculatePaySlip: {ex.Message}");
                return StatusCode(500, "Internal server error occurred");
            }
            
        }

        [HttpPost]
        [Route("GeneratePaySlip")]
        public async Task<IActionResult> GeneratePaySlip([FromBody] DtoPaySlip paySlip)
        {
            DtoPaySlip slip = new DtoPaySlip();
            try
            {
                if (paySlip == null)
                {
                    return BadRequest("PaySlip data is required");
                }
                var result = await _paySlipHandler.GeneratePaySlip(paySlip);
                if (result != null)
                {
                    slip = result;
                    return Ok(slip);
                }
                else
                {
                    return StatusCode(500, "Failed to generate PaySlip");
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GeneratePaySlip: {ex.Message}");
                return StatusCode(500, "Internal server error occurred");
            }
        }

        [HttpGet]
        [Route("GetPaySlipById/{payId:decimal}/{memId:decimal}/{brCode}")]
        public async Task<ActionResult<DtoPaySlip>> GetPaySlipById(decimal payId, decimal memId, string brCode)
        {
            DtoPaySlip slip = new DtoPaySlip();
            var result = await _paySlipHandler.GetPaySlipById(payId, memId, brCode);
            if (result != null)
            {
                slip = result;
                return Ok(slip);
            }
            else
            {
                return NotFound("No Data Found");
            }
        }

        [HttpPost]
        [Route("DeletePaySlip")]
        public async Task<IActionResult> DeletePaySlip([FromBody] DtoPaySlip paySlip)
        {
            try
            {
                if (paySlip == null)
                {
                    return BadRequest("PaySlip data is required");
                }
                var result = await _paySlipHandler.DeletePaySlip(paySlip);
                if (!result.IsError )
                {
                    return Ok("PaySlip deleted successfully");
                }
                else
                {
                    return StatusCode(500, "Failed to delete PaySlip, " + result.ErrorMessage );
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in DeletePaySlip: {ex.Message}");
                return StatusCode(500, "Internal server error occurred");
            }
        }

        [HttpGet]
        [Route("GetPaySlipListForSalaryPayment/{payDescription}/{brCode}")]
        public async Task<ActionResult<List<DropdownItem>>> GetPaySlipListForSalaryPayment(string payDescription, string brCode)
        {
            List<DropdownItem> list = new List<DropdownItem>();
            var result = await _paySlipHandler.GetPaySlipListForSalaryPayment(payDescription, brCode);
            if (result != null)
            {
                list = result;
                return Ok(list);
            }
            else
            {
                return NotFound("No Data Found");
            }
        }


        [HttpGet]
        [Route("GetEmployeeNamesForSalaryPayment/{payId:decimal}/{brCode}")]
        public async Task<ActionResult<List<DropdownItem>>> GetEmploeeNamesForSalaryPayment(decimal payId,string brCode)
        {
            List<DropdownItem> list = new List<DropdownItem>();
            var result = await _paySlipHandler.GetEmploeeNamesForSalaryPayment(payId, brCode);
            if (result != null)
            {
                list = result;
                return Ok(list);
            }
            else
            {
                return NotFound("No Data Found");
            }
        }

        [HttpGet]
        [Route("GetEmployeeNameForSalaryPayment/{empId:decimal}/{payId:decimal}/{brCode}")]
        public async Task<ActionResult<DropdownItem>> GetEmployeeNameForSalaryPayment(decimal empId, decimal payId, string brCode)
        {
            DropdownItem item = new DropdownItem();
            var result = await _paySlipHandler.GetEmploeeNameForSalaryPayment(empId, payId, brCode);
            if (result != null)
            {
                item = result;
                return Ok(item);
            }
            else
            {
                return NotFound("No Data Found");
            }
        }


        [HttpGet]
        [Route("GetPaySlipForPayment")]
        public async Task<ActionResult<List<Pay_Slip>>> GetPaySlipForPayment([FromQuery] List<decimal> empIdList, [FromQuery] decimal payId, [FromQuery] string brCode)
        {
            List<Pay_Slip> list = new List<Pay_Slip>();
            var result = await _paySlipHandler.GetPaySlipForPayment(empIdList, payId, brCode);
            if (result != null)
            {
                list = result;
                return Ok(list);
            }
            else
            {
                return NotFound("No Data Found");
            }
        }

    }
}
