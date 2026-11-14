using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;

namespace Infin8.Coapp.Repository
{
    /// <summary>
    /// Queries and id allocation for the ECS RD New Account component, acc 17 (spec 15). Every query filters BrCode on
    /// every alias. Nothing here calls SaveChanges — TransactionsHandler.SaveTransaction owns the transaction.
    /// </summary>
    public class EcsRdCreateRepository : Repository<TermDeposit_Master>, IEcsRdCreateRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;

        public EcsRdCreateRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<TermDeposit_Schemes>> GetRdSchemesAsync(string brCode) =>
            await (from s in CSISContext.TermDeposit_Schemes
                   where s.BrCode == brCode && s.TDSchemeType == "R" && !s.TDScheme_Delete
                   orderby s.TDScheme_Name
                   select s).AsNoTracking().ToListAsync();

        public async Task<TermDeposit_Schemes?> GetRdSchemeAsync(int schemeId, string brCode) =>
            await (from s in CSISContext.TermDeposit_Schemes
                   where s.TDScheme_Id == schemeId && s.BrCode == brCode && s.TDSchemeType == "R" && !s.TDScheme_Delete
                   select s).AsNoTracking().FirstOrDefaultAsync();

        public async Task<List<TermDeposit_Roi_Template>> GetRoiRowsAsync(IEnumerable<int> schemeIds, string brCode)
        {
            var ids = schemeIds.Distinct().ToList();
            return await (from r in CSISContext.TermDeposit_Roi_Template
                          where ids.Contains(r.TDScheme_Id) && !r.TDRoi_Delete && r.BrCode == brCode
                          select r).AsNoTracking().ToListAsync();
        }

        public async Task<DtoEcsRdMember?> GetMemberByIdAsync(decimal memId, string brCode) =>
            (await GetMembersByIdAsync(new[] { memId }, brCode)).GetValueOrDefault(memId);

        public async Task<Dictionary<decimal, DtoEcsRdMember>> GetMembersByIdAsync(IEnumerable<decimal> memIds, string brCode)
        {
            var ids = memIds.Distinct().ToList();
            var rows = await (from m in CSISContext.mem_master
                              where ids.Contains(m.mem_id) && m.brcode == brCode
                              select new DtoEcsRdMember
                              {
                                  Mem_Id = m.mem_id,
                                  MemberNo = m.memberno,
                                  MemberName = m.membername,
                                  FatherName = m.fathername,
                                  Dob = m.dob,
                                  MemberStatus = m.memberstatus,
                                  IsExpired = m.ismemexpired,
                                  IsDeleted = m.memberdelete
                              }).AsNoTracking().ToListAsync();
            return rows.GroupBy(r => r.Mem_Id).ToDictionary(g => g.Key, g => g.First());
        }

        public async Task<DtoEcsRdMember?> GetMemberByNoAsync(string memberNo, string brCode)
        {
            var no = memberNo.Trim();
            return await (from m in CSISContext.mem_master
                          where m.memberno != null && m.memberno.Trim() == no && m.brcode == brCode
                          orderby m.memberdelete, m.mem_id descending
                          select new DtoEcsRdMember
                          {
                              Mem_Id = m.mem_id,
                              MemberNo = m.memberno,
                              MemberName = m.membername,
                              FatherName = m.fathername,
                              Dob = m.dob,
                              MemberStatus = m.memberstatus,
                              IsExpired = m.ismemexpired,
                              IsDeleted = m.memberdelete
                          }).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<DtoNominee?> GetLastRdNomineeAsync(decimal memId, string brCode) =>
            await (from m in CSISContext.TermDeposit_Master
                   join s in CSISContext.TermDeposit_Schemes on m.TDScheme_Id equals s.TDScheme_Id
                   where m.Mem_Id == memId && !m.TD_Delete && m.BrCode == brCode
                         && s.TDSchemeType == "R" && s.BrCode == brCode
                   orderby m.TD_Id descending
                   select new DtoNominee
                   {
                       Nominee1Name = m.Nominee1Name,
                       Nominee1Age = m.Nominee1Age,
                       Nominee1Relationship = m.Nominee1Relationship,
                       Nominee2Name = m.Nominee2Name,
                       Nominee2Age = m.Nominee2Age,
                       Nominee2Relationship = m.Nominee2Relationship
                   }).AsNoTracking().FirstOrDefaultAsync();

        public async Task<List<string?>> GetTdNosWithPrefixAsync(string prefix, string brCode)
        {
            var dbNos = await (from m in CSISContext.TermDeposit_Master
                               where m.BrCode == brCode && m.TD_No != null && m.TD_No.StartsWith(prefix)
                               select m.TD_No).AsNoTracking().ToListAsync();
            // Include numbers already allocated in this unit of work but not yet saved.
            dbNos.AddRange(CSISContext.TermDeposit_Master.Local
                .Where(m => m.BrCode == brCode && m.TD_No != null && m.TD_No.StartsWith(prefix))
                .Select(m => m.TD_No));
            return dbNos;
        }

        public async Task<decimal> AddRdAsync(TermDeposit_Master master, List<TermDeposit_Members> members)
        {
            decimal tdId = (await CSISContext.TermDeposit_Master.Select(x => (decimal?)x.TD_Id).MaxAsync() ?? 0);
            if (CSISContext.TermDeposit_Master.Local.Any())
                tdId = Math.Max(tdId, CSISContext.TermDeposit_Master.Local.Max(x => x.TD_Id));
            master.TD_Id = tdId + 1;

            decimal memIdMax = (await CSISContext.TermDeposit_Members.Select(x => (decimal?)x.TDMem_Id).MaxAsync() ?? 0);
            if (CSISContext.TermDeposit_Members.Local.Any())
                memIdMax = Math.Max(memIdMax, CSISContext.TermDeposit_Members.Local.Max(x => x.TDMem_Id));
            foreach (var m in members)
            {
                m.TDMem_Id = ++memIdMax;
                m.TD_Id = master.TD_Id;
            }

            CSISContext.TermDeposit_Master.Add(master);
            CSISContext.TermDeposit_Members.AddRange(members);
            return master.TD_Id;
        }
    }
}
