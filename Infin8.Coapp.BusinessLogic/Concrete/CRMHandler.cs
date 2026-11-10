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

        public async Task<bool> AddEmployeeMaster(DtoEmpMaster empMaster)
        {
            bool result = false;
            try
            {
                Emp_Master emp = new();
                mem_master mem = new();
                List<Emp_Qualification> qualifications = new();
                emp = empMaster.Employee!;
                mem = empMaster.MemberMaster! ;
                qualifications.AddRange(empMaster.Qualifications!);
                _unitOfWork.BeginTransaction();
                var memMaxId = await _unitOfWork.Members.AddMemberMaster(mem);
                emp.Mem_Id = memMaxId;
                if (empMaster.Qualifications != null && empMaster.Qualifications.Count > 0)
                {
                    foreach (var qua in qualifications)
                    {
                        qua.Mem_Id = memMaxId;
                    }
                }
                var empResult = await _unitOfWork.EmployeeMaster.AddEmployeeMasterAsync(emp);
               
                var quaResult = await _unitOfWork.EmployeeQualification.AddEmployeeQualificationListAsync(qualifications);
                _unitOfWork.CommitTransaction();

                result = true;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                result = false;
                Console.Write(ex.Message);
            }
            return result;
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

        public async Task<decimal> AddMemberMaster(mem_master member)
        {
            return await _unitOfWork.Members.AddMemberMaster(member);
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

        public async Task<List<MemberDetailsVM>> GetMembersForECSDemand(string brCode)
        {
           return await _unitOfWork.Members.GetMembersForECSDemand(brCode);
        }
    }
}

