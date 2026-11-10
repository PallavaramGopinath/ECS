using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.BusinessLogic
{
    public interface ICRMHandler
    {
        Task<bool> AddMember(mem_master member);
        Task<decimal> AddMemberMaster(mem_master member);
        Task<bool> AddEmployeeMaster(DtoEmpMaster  empMaster);
        Task<mem_master> GetMemberById(decimal mem_id);
        Task<List<DropdownItem>> GetAllMembers(int memType, int memStatus, bool isMemNo, string brCode);
        Task<MemberDetailsVM> GetMemberDetailsByMemIdAsync(decimal memId);
        Task<MemberDetailsVM> GetMemberDetailsByMemNoAsync(string memNo,string brCode);
        Task<List<MemberDetailsVM>> GetMembersForECSDemand(string brCode);
    }
}
