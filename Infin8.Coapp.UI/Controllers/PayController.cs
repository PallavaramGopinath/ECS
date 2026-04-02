using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.BusinessLogic.Interface;
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
        readonly IPayInitHandler _payInitHandler;
        readonly IPayComponentHandler _payComponentHandler;
        readonly IPayComponentAssignmentsHandler _payComponentAssignmentsHandler;
        readonly IPayGenInfoHandler _payGenInfoHandler;
        readonly IPayTemplateHandler _payTemplateHandler;

        public PayController(IPaySlipHandler paySlipHandler, IEmpMasterHandler empMasterHandler,
            IPayInitHandler payInitHandler, IPayComponentHandler payComponentHandler,
            IPayComponentAssignmentsHandler payComponentAssignmentsHandler,
            IPayGenInfoHandler payGenInfoHandler,
            IPayTemplateHandler payTemplateHandler)
        {
            _paySlipHandler = paySlipHandler;
            _empMasterHandler = empMasterHandler;
            _payInitHandler = payInitHandler;
            _payComponentHandler = payComponentHandler;
            _payComponentAssignmentsHandler = payComponentAssignmentsHandler;
            _payGenInfoHandler = payGenInfoHandler;
            _payTemplateHandler = payTemplateHandler;
        }

        #region Pay Component
        [HttpPost]
        [Route("AddPayComponent")]
        public async Task<ActionResult<List<Pay_Components>>> AddPayComponent([FromBody] Pay_Components component)
        {
            List<Pay_Components> list = new();
            var result = await _payComponentHandler.AddPayComponent(component);
            if (result != null && result.Count > 0) list = result.ToList();
            if (result != null) list = result;
            return Ok(list);
        }

        [HttpPost]
        [Route("AddPayComponentAssignments")]
        public async Task<ActionResult<List<Pay_Component_Assignments>>> AddPayComponentAssignments([FromBody] Pay_Component_Assignments component)
        {
            List<Pay_Component_Assignments> list = new();
            var result = await _payComponentAssignmentsHandler.AddPayComponentAssignments(component);
            if (result != null && result.Count > 0) list = result.ToList();
            if (result != null) list = result;
            return Ok(list);
        }


        [HttpPost]
        [Route("NewPayComponentAssignments")]
        public async Task<ActionResult<List<Pay_Component_Assignments>>> NewPayComponentAssignments([FromBody] List<Pay_Component_Assignments> components)
        {
            List<Pay_Component_Assignments> list = new();
            var result = await _payComponentAssignmentsHandler.AddPayComponentAssignments(components);
            if (result != null && result.Count > 0) list = result.ToList();
            if (result != null) list = result;
            return Ok(list);
        }

        [HttpGet]
        [Route("GetPayComponents/{brCode}")]
        public async Task<ActionResult<List<Pay_Components>>> GetPayComponents(string brCode)
        {
            List<Pay_Components> list = new();
            var result = await _payComponentHandler.GetPayComponents(brCode);
            if (result != null && result.Count > 0) list = result.ToList();
            if (result != null) list = result;
            return Ok(list);
        }

        [HttpGet]
        [Route("GetPayEmployeesComponents/{brCode}")]
        public async Task<ActionResult<DtoPayComponents>> GetPayEmployeesComponents(string brCode)
        {
            DtoPayComponents comp = new();
            List<Pay_Components> componentList = new();
            var result = await _payComponentHandler.GetPayComponents(brCode);
            if (result != null && result.Count > 0) componentList = result.ToList();
            List<Pay_Component_Assignments> assignmentList = new();
            var query = await _payComponentAssignmentsHandler.GetPayComponentAssignments(brCode);
            if (query != null) assignmentList = query.ToList();
            List<EmployeeMasterDto> employees = new();
            var empList = await _empMasterHandler.GetEmployeeMasterListAsync(brCode);
            if (employees != null && employees.Count > 0) empList = employees.ToList();
            comp.Employees = empList;
            comp.AllComponents = componentList;
            comp.AllEmployeesAssignments = assignmentList;
            return Ok(comp);
        }

        [HttpGet]
        [Route("GetPayComponentAssignments/{brCode}")]
        public async Task<ActionResult<List<Pay_Component_Assignments>>> GetPayComponentAssignments(string brCode)
        {
            List<Pay_Component_Assignments> list = new();
            var result = await _payComponentAssignmentsHandler.GetPayComponentAssignments(brCode);
            if (result != null && result.Count > 0) list = result.ToList();
            if (result != null) list = result;
            return Ok(list);
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
        #endregion

        #region Employee
        [HttpGet]
        [Route("GetEmployeeList/{brCode}")]
        public async Task<ActionResult<List<EmployeeMasterDto>>> GetEmployeeList(string brCode)
        {
            List<EmployeeMasterDto> list = new List<EmployeeMasterDto>();
            var result = await _empMasterHandler.GetEmployeeMasterListAsync(brCode);
            if (result != null) list = result;
            return Ok(list);
        }
        [HttpGet]
        [Route("GetEmployee/{empId:decimal}/{brCode}")]
        public async Task<ActionResult<EmployeeMasterDto>> GetEmployee(decimal empId, string brCode)
        {
            EmployeeMasterDto emp = new EmployeeMasterDto();
            var result = await _empMasterHandler.GetEmployeeMasterById(empId, brCode);
            if (result != null)
            {
                emp = result;
                return Ok(emp);
            }
            else
            {
                return NotFound("No Data Found");
            }
        }

        [HttpGet]
        [Route("GetEmployeeListForExit/{brCode}")]
        public async Task<ActionResult<DtoEmployeeExit>> GetEmployeeListForExit(string brCode)
        {
            List<DtoEmployeeExit> empList = new();
            var result = await _empMasterHandler.GetEmployeeExitListAsync(brCode);
            if (result != null && result.Count >0)   empList = result.ToList ();
            return Ok(empList);
        }


        [HttpPost]
        [Route("AddSeparationEmployee")]
        public async Task<ActionResult<List<DtoEmployeeExit>>> AddSeparationEmployee([FromBody] DtoEmployeeExit emp)
        {
            List<DtoEmployeeExit> list = new();
            var result = await _empMasterHandler.AddSeparationEmployeeAsync(emp);
            if (result != null) list = result.ToList ();
            return Ok(list);
        }


        [HttpPost]
        [Route("RevertSeparationEmployee")]
        public async Task<ActionResult<List<DtoEmployeeExit>>> RevertSeparationEmployee([FromBody] DtoEmployeeExit emp)
        {
            List<DtoEmployeeExit> list = new();
            var result = await _empMasterHandler.RevertSeparationEmployeeAsync(emp);
            if (result != null) list = result.ToList();
            return Ok(list);
        }
        #endregion

        #region Pay Calculation
        [HttpGet]
        [Route("GetLastPayInfo/{brCode}")]
        public async Task<ActionResult<List<DtoEmployeeLastPayInfo>>> GetLastPayInfo(string brCode)
        {
            List<DtoEmployeeLastPayInfo> list = new List<DtoEmployeeLastPayInfo>();
            var result = await _paySlipHandler.GetEmployeeLastPayInfo(brCode);
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
                if (!result.IsError)
                {
                    return Ok("PaySlip deleted successfully");
                }
                else
                {
                    return StatusCode(500, "Failed to delete PaySlip, " + result.ErrorMessage);
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
        [Route("GetPaySlipListForSalaryPaymentByEmpId/{empId:decimal}/{payDescription}/{brCode}")]
        public async Task<ActionResult<List<DropdownItem>>> GetPaySlipListForSalaryPaymentByEmpId(decimal empId, string payDescription, string brCode)
        {
            List<DropdownItem> list = new List<DropdownItem>();
            var result = await _paySlipHandler.GetPaySlipListForSalaryPaymentByEmpId(empId, payDescription, brCode);
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
        public async Task<ActionResult<List<DropdownItem>>> GetEmploeeNamesForSalaryPayment(decimal payId, string brCode)
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
            if (result != null && !string.IsNullOrWhiteSpace(result.Value))
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

        [HttpGet]
        [Route("GetPayIdForDAArrears/{fromDate}/{toDate}/{description}/{brCode}")]
        public async Task<ActionResult<decimal>> GetPayIdForDAArrears(string fromDate, string toDate, string description, string brCode)
        {
            decimal payId = 0;
            DateTime.TryParse(fromDate, out DateTime fromDateFormated);
            DateTime.TryParse(toDate, out DateTime toDateFormated);
            payId = await _payInitHandler.GetPaySlipForDAArrears(fromDateFormated, toDateFormated, description, brCode);
            return Ok(payId);
        }

        [HttpGet]
        [Route("IsPaySlipGenerated/{payId:decimal}/{empId:decimal}/{brCode}")]
        public async Task<ActionResult<bool>> IsPaySlipGenerated(decimal payId, decimal empId, string brCode)
        {
            bool result = false;
            result = await _paySlipHandler.IsPaySlipGenerated(payId, empId, brCode);
            return Ok(result);
        }

        [HttpPost]
        [Route("CalculateDAArrears")]
        public async Task<IActionResult> CalculateDAArrears([FromBody] DtoPayDAArrears arrears)
        {
            try
            {
                if (arrears == null)
                {
                    return BadRequest("Employee  data is required to calculate DA Arrears");
                }

                //DtoPayDAArrears daCalc = new ();
                var result = await _paySlipHandler.CalculateDAArrears(arrears);
                if (result != null)
                {
                    //daCalc = result;
                    return Ok(result);
                }
                else
                {
                    return NotFound("No Data found");
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in Calculate DA Arrears: {ex.Message}");
                return StatusCode(500, "Internal server error occurred");
            }

        }

        [HttpPost]
        [Route("GenerateDAArrears")]
        public async Task<IActionResult> GenerateDAArrears([FromBody] DtoPayDAArrears arrears)
        {
            try
            {
                if (arrears == null)
                {
                    return BadRequest("Employee  data is required to calculate DA Arrears");
                }
                //DtoPayDAArrears daCalc = new ();
                var result = await _paySlipHandler.GenerateDAArrears(arrears);
                if (result != null)
                {
                    //daCalc = result;
                    return Ok(result);
                }
                else
                {
                    return NotFound("No Data found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Generate DA Arrears: {ex.Message}");
                return StatusCode(500, "Internal server error occurred");
            }
        }

        [HttpGet]
        [Route("GetPayDAArrearsView")]
        public async Task<ActionResult<List<DtoPayDAArrearsView>>> GetPayDAArrearsView([FromQuery] List<decimal> empIdList, [FromQuery] decimal payId, [FromQuery] string brCode)
        {
            List<DtoPayDAArrearsView> list = new List<DtoPayDAArrearsView>();
            var result = await _paySlipHandler.GetPayDAArrearsViews(empIdList, payId, brCode);
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
        [Route("GetPFBalance/{empId:decimal}/{asOnDate}/{brCode}")]
        public async Task<ActionResult<DtoPayPFData>> GetPayPFData(decimal empId, string asOnDate, string brCode)
        {
            DtoPayPFData pfData = new();
            DateTime.TryParse(asOnDate, out DateTime asOnDataFormatted);
            var result = await _paySlipHandler.GetPFBalance(empId, asOnDataFormatted, brCode);
            if (result != null)
            {
                pfData = result;
                return Ok(pfData);
            }
            else
            {
                return NotFound("No Data Found");
            }
        }

        [HttpGet]
        [Route("GetSLSData/{empId:decimal}/{brCode}")]
        public async Task<ActionResult<List<DtoSLSComponent>>> GetSLSData(decimal empId, string brCode)
        {
            List<DtoSLSComponent> slsList = new();
            var result = await _paySlipHandler.GetSLSData(empId, brCode);
            if (result != null)
            {
                slsList = result.ToList();
                return Ok(slsList);
            }
            else
            {
                return NotFound("No Data Found");
            }
        }

        [HttpGet]
        [Route("IsSLSPaid/{empId:decimal}/{fromDate}/{toDate}/{payDesc}/{brCode}")]
        public async Task<ActionResult<bool>> IsSLSAlreadyPaid(decimal empId, string fromDate, string toDate, string payDesc, string brCode)
        {
            DateTime.TryParse(fromDate, out DateTime fromDateFormatted);
            DateTime.TryParse(toDate, out DateTime toDateFormatted);
            var response = await _paySlipHandler.IsSLSAlreadyPaid(empId, fromDateFormatted, toDateFormatted, payDesc, brCode);
            if (response == true)
            {
                return Ok(true);
            }
            else
            {
                return Ok(false);
            }
        }

        [HttpGet]
        [Route("GetPayIdList/{yrId:decimal}/{payDes}/{brCode}")]
        public async Task<ActionResult<List<DropdownItem>>> GetPayIdList(decimal yrId, string payDes, string brCode)
        {
            List<DropdownItem> list = new List<DropdownItem>();
            var result = await _payInitHandler.GetPayIdList(yrId, payDes, brCode);
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
        #endregion 

        #region Pay Info
        [HttpGet]
        [Route("GetGeneralInfoList/{brCode}")]
        public async Task<ActionResult<List<Pay_Gen_Info>>> GetGeneralInfoList(string brCode)
        {
            List<Pay_Gen_Info> list = new();
            var result = await _payGenInfoHandler.GetPayGenInfoListAsync(brCode);
            if (result != null) list = result;
            return Ok(list);
        }

        [HttpPost]
        [Route("AddGeneralInfo")]
        public async Task<ActionResult<List<Pay_Gen_Info>>> AddGeneralInfo([FromBody] Pay_Gen_Info info)
        {
            List<Pay_Gen_Info> list = new();
            var result = await _payGenInfoHandler.AddPayGenInfoAsync(info);
            if (result != null) list = result;
            return Ok(list);
        }

        [HttpPost]
        [Route("EditGeneralInfo")]
        public async Task<ActionResult<List<Pay_Gen_Info>>> EditGeneralInfo([FromBody] Pay_Gen_Info info)
        {
            List<Pay_Gen_Info> list = new();
            var result = await _payGenInfoHandler.EditPayGenInfoAsync(info);
            if (result != null) list = result;
            return Ok(list);
        }
        #endregion

        #region Pay Template
        [HttpGet]
        [Route("GetPayTemplate/{brCode}")]
        public async Task<ActionResult<Pay_Template>> GetPayTemplate(string brCode)
        {
            Pay_Template template = new Pay_Template();
            var result = await _payTemplateHandler.GetPayTemplateAsync(brCode);
            if (result != null) template = result;
            return Ok(template);
        }

        [HttpPost]
        [Route("CreateOrUpdatePayTemplate")]
        public async Task<ActionResult<bool>> CreateOrUpdatePayTemplate([FromBody] Pay_Template template)
        {
            bool result = false;
            if (template != null && template.PayTemplate_Id > 0)
            {
                result = await _payTemplateHandler.EditPayTemplateAsync(template);
            }
            else if (template != null)
            {
                result = await _payTemplateHandler.AddPayTemplateAsync(template);
            }
            else
            {
                return BadRequest("Invalid Pay Template data");
            }
            return Ok(result);
        }
        #endregion


    }
}
