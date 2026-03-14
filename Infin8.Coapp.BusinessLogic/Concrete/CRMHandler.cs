using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
namespace Infin8.Coapp.BusinessLogic
{
    public class CRMHandler : ICRMHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public CRMHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddMember(mem_master member)
        {
            bool result = false;
            try
            {
                //_unitOfWork.BeginTransaction();
                await  _unitOfWork.Members.AddMember(member);
                await _unitOfWork.CompleteAsync();
                //_unitOfWork.Complete();
                //_unitOfWork.CommitTransaction();
                result = true;
            }
            catch (Exception ex)
            {
                //_unitOfWork.RollBack();
                string msg = ex.Message;
                result = false;
            }
            return result;
        }

        public async Task<List<DropdownItem>> GetAllMembers(int memType, int memStatus, bool isMemNo,string brCode)
        {
            List<DropdownItem> items = new List<DropdownItem>();
            try
            {
                items = await _unitOfWork.Members.GetAllMembers(memType, memStatus, isMemNo,brCode);
            }
            catch (Exception)
            {
                throw;
            }
            return items;
        }

        public async Task<mem_master> GetMemberById(decimal mem_id)
        {
            mem_master member = new();
            try
            {
                member = await _unitOfWork.Members.GetMemberById(mem_id);
            }
            catch (Exception)
            {
                //throw;
            }
            return member;
        }

        public async Task<MemberDetailsVM> GetMemberDetailsByMemIdAsync(decimal memId)
        {
            return await _unitOfWork.Members.GetMemberDetailsByMemIdAsync(memId);
        }

        public async Task<MemberDetailsVM> GetMemberDetailsByMemNoAsync(string memNo, string brCode)
        {
            return await _unitOfWork.Members.GetMemberDetailsByMemNoAsync(memNo,brCode);
        }

    }
}

