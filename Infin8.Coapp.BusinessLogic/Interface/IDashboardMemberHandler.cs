using Infin8.Coapp.Dto;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IDashboardMemberHandler
    {
        List<MemberDashBoardAccountsDto> GetMemberDashBoardAccounts(decimal id, DateTime fromDate, DateTime toDate);
        List<MemberDashBoardAccountsDto> DashboardAccounts(decimal id, DateTime fromDate, DateTime toDate);
    }
}
