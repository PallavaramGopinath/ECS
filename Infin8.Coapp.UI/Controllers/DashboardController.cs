
using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        readonly IDashboardMemberHandler _dashboardHandler;
        public DashboardController(IDashboardMemberHandler dashboardHandler)
        {
            _dashboardHandler = dashboardHandler;
        }

        [HttpGet]
        [Route("get-member-dashboard-acoount/{id:decimal}/{fromDate}/{toDate}/{brCode}")]
        public ActionResult<List<MemberDashBoardAccountsDto>> GetMemberDashBoardAccounts(decimal id, string fromDate, string toDate,string brCode)
        {
            DateTime.TryParse(fromDate, out DateTime fromDateFormated);
            DateTime.TryParse(toDate, out DateTime toDateFormated);
            List<MemberDashBoardAccountsDto> memberDashBoardAccounts = new List<MemberDashBoardAccountsDto>();
            memberDashBoardAccounts = _dashboardHandler.DashboardAccounts(id, fromDateFormated , toDateFormated,brCode );
            if (memberDashBoardAccounts == null)
                return NotFound();
            else
                return Ok(memberDashBoardAccounts);
        }

        [HttpGet]
        [Route("get-member-dashboard-loans/{id:decimal}/{fromDate}/{toDate}/{brCode}")]
        public async Task<ActionResult<List<MemberDashBoardLoans>>> GetMemberDashBoardLoans(decimal id, string fromDate, string toDate, string brCode)
        {
            DateTime.TryParse(fromDate, out DateTime fromDateFormated);
            DateTime.TryParse(toDate, out DateTime toDateFormated);
            List<MemberDashBoardLoans> loans = new ();
            var result  = await  _dashboardHandler.GetMemberDashBoardLoans(id, fromDateFormated, toDateFormated, brCode);
            loans = result.ToList();
            return Ok(loans);
            //if (result != null && result.Any())
            //{
            //    loans = result.ToList();
            //    return Ok(loans);
            //}
            //else
            //    return NotFound();
        }

        [HttpGet]
        [Route("get-member-dashboard-td/{id:decimal}/{fromDate}/{toDate}/{brCode}")]
        public async Task<ActionResult<List<MemberDashBoardTD>>> GetMemberDashBoardTDs(decimal id, string fromDate, string toDate, string brCode)
        {
            DateTime.TryParse(fromDate, out DateTime fromDateFormated);
            DateTime.TryParse(toDate, out DateTime toDateFormated);
            List<MemberDashBoardTD> tds = new();
            var result = await _dashboardHandler.GetMemberDashBoardTDs(id, fromDateFormated, toDateFormated, brCode);
            tds = result.ToList();
            return Ok(tds);
            //if (result != null && result.Any())
            //{
            //    tds = result.ToList();
            //    return Ok(tds);
            //}
            //else
            //    return NotFound();
        }

        [HttpGet]
        [Route("get-member-dashboard/{id:decimal}/{brCode}")]
        public async Task<ActionResult<MemberDetailsVM2>> GetMemberDashBoard(decimal id,  string brCode)
        {
            MemberDetailsVM2 member = new();
            var result = await _dashboardHandler.GetMemberDataDashBoard(id,  brCode);
            if (result != null && result.Mem_Id >0)
            {
                member = result;
                return Ok(member);
            }
            else
                return NotFound();
        }

        [HttpGet]
        [Route("dashboard-accounts-ledger/{memId:decimal}/{trnType:int}/{fromDate}/{toDate}/{brCode}")]
        public async Task<ActionResult<List<MemberLedgerVM>>> GetMemberDashboardAccounts(decimal memId, int trnType,string fromDate, string toDate, string brCode)
        {
            List<MemberLedgerVM> accountsList = new List<MemberLedgerVM>();
            DateTime.TryParse (fromDate , out DateTime fromDateFormated);
            DateTime .TryParse(toDate , out DateTime toDateFormated);
            var result = await _dashboardHandler.GetMemberLedger(memId, trnType, fromDateFormated, toDateFormated, brCode);
            accountsList = result.ToList();
            return Ok(accountsList!);
        }

        [HttpGet]
        [Route("dashboard-loan-ledger/{loanId:decimal}/{fromDate}/{toDate}/{brCode}")]
        public async Task<ActionResult<LoanLedgerVM>> GetMemberDashboardLoans(decimal loanId,  string fromDate, string toDate, string brCode)
        {
            LoanLedgerVM loan = new LoanLedgerVM();
            DateTime.TryParse(fromDate, out DateTime fromDateFormated);
            DateTime.TryParse(toDate, out DateTime toDateFormated);
            var result = await _dashboardHandler.GetLoanLedger(loanId,  fromDateFormated, toDateFormated, brCode);
            loan = result;
            return Ok(loan);
        }

        [HttpGet]
        [Route("dashboard-td-ledger/{tdId:decimal}/{fromDate}/{toDate}/{brCode}")]
        public async Task<ActionResult<TDLedgerVM>> GetMemberDashboardTd(decimal tdId, string fromDate, string toDate, string brCode)
        {
            TDLedgerVM td = new TDLedgerVM();
            DateTime.TryParse(fromDate, out DateTime fromDateFormated);
            DateTime.TryParse(toDate, out DateTime toDateFormated);
            var result = await _dashboardHandler.GetTDLedgerVM(tdId, fromDateFormated, toDateFormated, brCode);
            td = result;
            return Ok(td);
        }
    }
}
