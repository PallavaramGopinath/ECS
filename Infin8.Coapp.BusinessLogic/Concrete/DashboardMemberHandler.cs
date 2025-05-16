using Infin8.Coapp.Dto;
using Infin8.Coapp.Repository;

namespace Infin8.Coapp.BusinessLogic
{
    public class DashboardMemberHandler : IDashboardMemberHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public DashboardMemberHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public List<MemberDashBoardAccountsDto> GetMemberDashBoardAccounts(decimal id, DateTime fromDate, DateTime toDate)
        {
            List<MemberDashBoardAccountsDto> memberDashBoardAccounts = new();
            try
            {
                memberDashBoardAccounts = _unitOfWork.DashboardMember.GetMemberDashBoardAccounts(id, fromDate, toDate);
            }
            catch (Exception)
            {

                throw;
            }
            return memberDashBoardAccounts;
        }

        public List<MemberDashBoardAccountsDto> DashboardAccounts(decimal id, DateTime fromDate, DateTime toDate)
        {
            List<MemberDashBoardAccountsDto> memberDashBoardAccounts = new();
            try
            {
                memberDashBoardAccounts = _unitOfWork.DashboardMember.DashboardAccounts(id, fromDate, toDate);
            }
            catch (Exception)
            {

                throw;
            }
            return memberDashBoardAccounts;
        }
    }
}
