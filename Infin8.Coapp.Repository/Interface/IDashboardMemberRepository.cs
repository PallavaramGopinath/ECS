using Infin8.Coapp.Dto;

namespace Infin8.Coapp.Repository
{
    public interface IDashboardMemberRepository
    {
        List<MemberDashBoardAccountsDto> GetMemberDashBoardAccounts(decimal id, DateTime fromDate, DateTime toDate);
        List<MemberDashBoardAccountsDto> DashboardAccounts(decimal id, DateTime fromDate, DateTime toDate);
    }
}
