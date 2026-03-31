using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;

namespace Infin8.Coapp.Repository
{
    public class CRMRepository : Repository<mem_master>, ICRMRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;

        public CRMRepository(CSISContext context) : base(context)
        {
        }

        public async Task<bool> AddMember(mem_master member)
        {
            decimal maxId = await CSISContext.mem_master.MaxAsync(x => x.mem_id);
            maxId++;
            member.mem_id = maxId;
            Add(member);
            return true;
        }
        public async Task<decimal> AddMemberMaster(mem_master member)
        {
            decimal maxId = await CSISContext.mem_master.MaxAsync(x => x.mem_id);
            maxId++;
            member.mem_id = maxId;
            await AddAsync  (member);
            CSISContext.SaveChanges();
            return maxId;
        }
        public async Task<mem_master> GetMemberById(decimal mem_id)
        {
            mem_master mem = new();
            try
            {
                mem = await CSISContext.mem_master.Where(x => x.mem_id == mem_id).FirstAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return mem;
        }

        public async Task< List<DropdownItem>> GetAllMembers(int memType, int memStatus, bool isMemNo, string brCode)
        {
            int[] MemType;
            int[] MemStatus;
            List<DropdownItem> items = new List<DropdownItem>();
            try
            {
                if (memType == 0)
                    MemType = new int[] { 1, 2 };
                else
                    MemType = new int[] { memType };
                if (memStatus == 0)
                    MemStatus = new int[] { 1, 2, 3 };
                else
                    MemStatus = new int[] { memStatus };

                var data = await  (from r in CSISContext.mem_master
                           where MemType.Contains(r.membertype) && MemStatus.Contains(r.memberstatus)
                           && r.brcode == brCode
                           && r.memberdelete == false && r.isaccountclosed == false
                           select new DropdownItem
                           {
                               Value = r.mem_id.ToString(),
                               Text = isMemNo ? r.memberno : r.perno
                           }).ToListAsync();

                if (data != null)
                {
                    items = data.ToList();
                }
            }
            catch (Exception ex)
            {

                string err = ex.Message;
            }
            return items;
        }

        public async Task<MemberDetailsVM> GetMemberDetailsByMemIdAsync(decimal memId)
        {
            MemberDetailsVM memberDetails = new();
            try
            {
                var memDetails = await (from f in CSISContext.mem_master
                                  where f.mem_id == memId && f.memberdelete == false && f.isaccountclosed == false
                                  select new MemberDetailsVM
                                  {
                                      Mem_Id = f.mem_id,
                                      MemberNo = f.memberno,
                                      MemberType = (int)f.membertype,
                                      PerNo = f.perno,
                                      MemberName = f.membername,
                                      FatherName = f.fathername,
                                      Dob = f.dob,
                                      Age = (int)f.age,
                                      DOR = f.dor,
                                      IsMemExpired = f.ismemexpired,
                                      ExpiredDate = f.expireddate,
                                      MemberStatus = f.memberstatus,
                                      GPF_No = f.gpf_no,
                                      TicketTokenGangNo = f.tickettokengangno,
                                      DOJ = f.doj,
                                      BasicPay = f.basicpay,
                                      AdmissionDate = f.admissiondate,
                                      Address = (f.peradd1 ?? "") +
                                                (!string.IsNullOrEmpty(f.peradd2) ? ", " + f.peradd2 : "") +
                                                (!string.IsNullOrEmpty(f.peradd3) ? ", " + f.peradd3 : "") +
                                                (!string.IsNullOrEmpty(f.prepin) ? ", " + f.prepin : "")
                                  }).FirstOrDefaultAsync();
                if(memDetails != null) memberDetails = memDetails;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching member details by member id");
            }
            return memberDetails;
        }

        public async Task<MemberDetailsVM> GetMemberDetailsByMemNoAsync(string memNo, string brCode)
        {
            MemberDetailsVM memberDetails = new();
            try
            {
                var memDetails = await (from f in CSISContext.mem_master
                                  where f.memberno == memNo && f.memberdelete == false && f.isaccountclosed == false && f.brcode == brCode
                                  select new MemberDetailsVM
                                  {
                                      Mem_Id = f.mem_id,
                                      MemberNo = f.memberno,
                                      MemberType = (int)f.membertype,
                                      PerNo = f.perno,
                                      MemberName = f.membername,
                                      FatherName = f.fathername,
                                      Dob = f.dob,
                                      Age = (int)f.age,
                                      DOR = f.dor,
                                      IsMemExpired = f.ismemexpired,
                                      ExpiredDate = f.expireddate,
                                      MemberStatus = f.memberstatus,
                                      GPF_No = f.gpf_no,
                                      TicketTokenGangNo = f.tickettokengangno,
                                      DOJ = f.doj,
                                      BasicPay = f.basicpay,
                                      AdmissionDate = f.admissiondate,
                                      BrCode = f.brcode,
                                      Address = (f.peradd1 ?? "") +
                                                (!string.IsNullOrEmpty(f.peradd2) ? ", " + f.peradd2 : "") +
                                                (!string.IsNullOrEmpty(f.peradd3) ? ", " + f.peradd3 : "") +
                                                (!string.IsNullOrEmpty(f.prepin) ? ", " + f.prepin : "")
                                  }).FirstOrDefaultAsync();
                if (memDetails != null) memberDetails = memDetails;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching member details by member No");
            }
            return memberDetails;
        }

    }

}

