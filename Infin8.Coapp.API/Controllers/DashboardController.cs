
using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("{id:int}/{fromDate:datetime}/{toDate:datetime}")]
        public ActionResult<List<MemberDashBoardAccountsDto>> GetMemberDashBoardAccounts(int id, DateTime fromDate, DateTime toDate)
        {
            List<MemberDashBoardAccountsDto> memberDashBoardAccounts = new List<MemberDashBoardAccountsDto>();
            memberDashBoardAccounts = _dashboardHandler.GetMemberDashBoardAccounts(id, fromDate, toDate);
            if (memberDashBoardAccounts == null)
                return NotFound();
            else
                return Ok(memberDashBoardAccounts);
        }
    }
}
