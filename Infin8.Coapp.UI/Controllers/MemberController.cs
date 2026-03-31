using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
namespace API.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        readonly ICRMHandler _memberHandler;
        readonly IDashboardMemberHandler _dashboardHandler;
        readonly IMemTrnHandler _memTrnHandler;
        //private readonly IWebHostEnvironment _webHostEnvironment;
        //IConfiguration _configuration;
        //public MemberController(ICRMHandler memberHandler, IWebHostEnvironment iWebHostEnvironment, IConfiguration configuration ) 
        //{ 
        //    _memberHandler = memberHandler;
        //    _webHostEnvironment = iWebHostEnvironment;  
        //    _configuration = configuration;
        //}


        /// ,IDashboardHandler dashboardHandler

        public MemberController(ICRMHandler memberHandler, IDashboardMemberHandler dashboardHandler, IMemTrnHandler memTrnHandler)
        {
            _memberHandler = memberHandler;
            _dashboardHandler = dashboardHandler;
            _memTrnHandler = memTrnHandler;
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<mem_master>> GetMemberById(int id)
        {
            mem_master mem = new mem_master();
            mem = await _memberHandler.GetMemberById(id);
            if (mem == null)
            {
                return NotFound();
            }
            return Ok(mem);
        }

        [HttpGet]
        [Route("{memType:int}/{memStatus:int}/{isMemNo:bool}/{brCode}")]
        public ActionResult<List<DropdownItem>> GetMemberList(int memType, int memStatus, bool isMemNo, string brCode)
        {
            List<DropdownItem> items = new List<DropdownItem>();
            var item = _memberHandler.GetAllMembers(memType, memStatus, isMemNo, brCode);
            if (items == null)
                return NotFound();
            else
                return Ok(items);
        }

        [HttpGet]
        [Route("GetMemberDetails/{memNo}/{brCode}")]
        public async Task<ActionResult<MemberDetailsVM>> GetMemberDetailsByMemberNo(string memNo, string brCode) 
        { 
            MemberDetailsVM memberDetailsVM = new MemberDetailsVM();
            var mem = await  _memberHandler.GetMemberDetailsByMemNoAsync(memNo, brCode);
            if (mem != null) memberDetailsVM = mem;
            if(memberDetailsVM == null) return NotFound();
            else return Ok(memberDetailsVM);
        }

        [HttpPost]
        //public async Task<HttpResponseMessage> Post([FromBody] MemberRegistration memberRegistration)
        public async Task<ActionResult> Post([FromBody] MemberRegistration memberRegistration)
        {
            try
            {
                //mem_master member = new mem_master();
                var member = new mem_master
                {
                    // Direct matches
                    memberno = memberRegistration.memberno,
                    perno = memberRegistration.memberno,
                    membertype = memberRegistration.membertype,
                    resolutionno = memberRegistration.resolutionno,
                    resolutiondate = memberRegistration.resolutiondate,
                    memberphoto = memberRegistration.memberphoto,
                    memberesignature = memberRegistration.memberesignature,
                    salutation = memberRegistration.salutation,
                    membername = memberRegistration.membername,
                    relation_type = memberRegistration.relation_type,
                    fathername = memberRegistration.fathername,
                    gender = memberRegistration.gender,
                    caste_id = memberRegistration.caste_id,
                    dob = memberRegistration.dob,
                    age = memberRegistration.age,
                    alternative_mobileno = memberRegistration.alternative_mobileno,
                    officephoneno = memberRegistration.officephoneno,
                    mobileno = memberRegistration.mobileno,
                    emailid = memberRegistration.emailid,

                    // Address
                    preadd1 = memberRegistration.preadd1,
                    preadd2 = memberRegistration.preadd2,
                    preadd3 = memberRegistration.preadd3,
                    prepin = memberRegistration.prepin,
                    prearea_id = memberRegistration.prearea_id,
                    peradd1 = memberRegistration.peradd1,
                    peradd2 = memberRegistration.peradd2,
                    peradd3 = memberRegistration.peradd3,
                    perpin = memberRegistration.perpin,
                    perarea_id = memberRegistration.perarea_id,

                    // Membership details
                    isnewmember = memberRegistration.isnewmember,
                    isexistingmember = memberRegistration.isexistingmember,
                    existing_memberno = memberRegistration.existing_memberno,
                    ismember_othersociety = memberRegistration.ismember_othersociety,

                    // Nominee details
                    nomineename = memberRegistration.nomineename,
                    nomineeage = memberRegistration.nomineeage,
                    nomineerelationship = memberRegistration.nomineerelationship,

                    // Bank and identity details
                    sbaccountno = memberRegistration.sbaccountno,
                    bankname = memberRegistration.bankname,
                    ifsccode = memberRegistration.ifsccode,
                    gpf_no = memberRegistration.gpf_no,
                    panno = memberRegistration.panno,
                    aadharno = memberRegistration.aadharno,
                    smartcardno = memberRegistration.smartcardno,
                    aadharcardpath = memberRegistration.aadharcardpath,

                    // Additional fields
                    religion_id = memberRegistration.religion_id,
                    comm_id = memberRegistration.comm_id,
                    occ_id = memberRegistration.occ_id,
                    constituency_id = memberRegistration.constituency_id,
                    admissiondate = memberRegistration.admissiondate,

                    // Default values for required fields not in MemberRegistration
                    mem_id = 0, // This should be handled by your database
                    memberdelete = false,
                    memberstatus = 1, // Set appropriate default
                    isaccountclosed = false,
                    member_oe = false,
                    ismemexpired = false,
                    brcode = memberRegistration.brcode,
                    usr_id = memberRegistration.usr_id,
                    yr_id = memberRegistration.yr_id,
                };

                var result = await _memberHandler.AddMember(member);
                if (result)
                    return Ok(); // Returns 200 OK
                else
                    return StatusCode(500, "Failed to add member"); // Returns 500 with message
                //if(result )
                //    return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                //else
                //    return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
            catch (Exception ex)
            {
                throw new System.Exception(ex.Message);
            }
        }

        [HttpPost]
        [Route("AddEmployeeMaster")]
        public async Task<ActionResult<bool>> AddEmployeeMaster([FromBody]DtoEmpMaster empMaster)
        {
            var result = await _memberHandler.AddEmployeeMaster(empMaster);
            return Ok(result);
        }



        //[HttpGet("{id:decimal}/{fromDate}/{toDate}")]
        //public ActionResult<List<MemberDashBoardAccountsDto>> GetMemberDashBoardAccounts(decimal id, string fromDate, string toDate)
        //{
        //    List<MemberDashBoardAccountsDto> memberDashBoardAccounts = new List<MemberDashBoardAccountsDto>();
        //    DateTime.TryParse(fromDate, out DateTime fromDateFormated);
        //    DateTime.TryParse(toDate, out DateTime toDateFormated);
        //    memberDashBoardAccounts = _dashboardHandler.GetMemberDashBoardAccounts(id, fromDateFormated, toDateFormated);
        //    if (memberDashBoardAccounts == null)
        //        return NotFound();
        //    else
        //        return Ok(memberDashBoardAccounts);
        //}

        [HttpGet()]
        [Route("Dashboard/{id:decimal}/{fromDate}/{toDate}/{brCode}")]
        public ActionResult<List<MemberDashBoardAccountsDto>> GetDashboardAccounts(decimal id, string fromDate, string toDate, string brCode)
        {
            DateTime.TryParse(fromDate, out DateTime fromDateFormated);
            DateTime.TryParse(toDate, out DateTime toDateFormated);
            List<MemberDashBoardAccountsDto> memberDashBoardAccounts = new List<MemberDashBoardAccountsDto>();
            memberDashBoardAccounts = _dashboardHandler.DashboardAccounts(id, fromDateFormated, toDateFormated,brCode );
            if (memberDashBoardAccounts == null)
                return NotFound();
            else
                return Ok(memberDashBoardAccounts);
        }

        [HttpGet]
        [Route("MemTrn/sbAccountBalance/{accId:decimal}/{brCode}")]
        public async Task<ActionResult<double>> GetSBAccountBalanceByAccId(decimal accId, string brCode)
        {
            double balance = 0;
            balance = await _memTrnHandler.GetSBAccountBalanceByAccId(accId, brCode);
            return Ok(balance);
        }
        [HttpGet]
        [Route("MemTrn/SBAccountInterestBalance/{accId:decimal}/{brCode}")]
        public async Task<ActionResult<double>> GetSBAccountInterestBalanceByAccId(decimal accid, string brCode)
        {
            double balance = 0;
            balance = await _memTrnHandler.GetSBAccountInterestBalanceByAccId(accid, brCode);
            return Ok(balance);
        }
        [HttpGet]
        [Route("MemTrn/{memId:decimal}/{ledId:decimal}/{brCode}")]
        public async Task<ActionResult<double>> GetMemTrnByMemIdAndLedId(decimal memId, decimal ledId, string brCode)
        {
            double balance = 0;
            balance = await _memTrnHandler.GetMemTrnByMemIdAndLedIdAsync(memId,ledId, brCode);
            return Ok(balance);
        }
        [HttpGet]
        [Route("MemTrn/Total/{memId:decimal}/{trnType:int}/{brCode}")]
        public async Task<ActionResult<double>> GetMemTrnTotalSuspenseAmount(decimal memId, int trnType, string brCode)
        {
            double balance = 0;
            balance = await _memTrnHandler.GetmemTrnTotalSuspenseAmount(memId, trnType, brCode);
            return Ok(balance);
        }

        #region Dividend
        [HttpGet]
        [Route("MemTrn/DividendPayableByMemId/{memId:decimal}/{asOnDate}/{brCode}")]
        public async Task<ActionResult<List<DividendOrIntOnTDPaymentVM>>> GetDividendPayableByMemId(decimal memId, string asOnDate, string brCode)
        {
            List<DividendOrIntOnTDPaymentVM> divideneList = new();
            DateTime.TryParse(asOnDate, out DateTime asOnDateFormatted);
            var result = await _memTrnHandler.GetDividendPayableListAsync(memId, asOnDateFormatted, brCode);
            if(result != null && result.Any())
            {
                divideneList = result.ToList();
            }
            return Ok(divideneList);
        }
        #endregion 
    }
}
