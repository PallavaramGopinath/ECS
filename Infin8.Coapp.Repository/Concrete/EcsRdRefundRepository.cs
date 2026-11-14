using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;

namespace Infin8.Coapp.Repository
{
    /// <summary>
    /// Queries for the ECS RD Refund component, acc 10 (spec 16). Every query filters BrCode on every alias.
    /// Nothing here calls SaveChanges — TransactionsHandler.SaveTransaction owns the transaction.
    /// </summary>
    public class EcsRdRefundRepository : Repository<TermDeposit_Master>, IEcsRdRefundRepository
    {
        public const int RdRefundAccountId = 10;

        public CSISContext CSISContext => (CSISContext)Context;

        public EcsRdRefundRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<TermDeposit_FcTemplate>> GetRdForeclosureTemplatesAsync(string brCode)
        {
            return await (from f in CSISContext.TermDeposit_FcTemplate
                          where f.TD_Type == 4 && !f.TDfc_Delete && f.BrCode == brCode
                          select f).AsNoTracking().ToListAsync();
        }

        public async Task<HashSet<decimal>> GetLienedTdIdsAsync(IEnumerable<decimal> tdIds, string brCode)
        {
            var ids = tdIds.Distinct().ToList();
            var marked = await (from m in CSISContext.TermDeposit_Master
                                where ids.Contains(m.TD_Id) && m.LienMarked && m.BrCode == brCode
                                select m.TD_Id).ToListAsync();
            var liened = await (from l in CSISContext.Lien_Trn
                                where ids.Contains(l.TD_Id) && !l.LienTr_Delete && !l.LienTr_Closed
                                      && l.Voc_Status == "V" && l.BrCode == brCode
                                select l.TD_Id).ToListAsync();
            return marked.Concat(liened).ToHashSet();
        }

        public async Task<Dictionary<decimal, (decimal LedId, decimal IntLedId, decimal PiLedId)>> GetRdLedgersAsync(IEnumerable<decimal> tdIds, string brCode)
        {
            var ids = tdIds.Distinct().ToList();
            var rows = await (from m in CSISContext.TermDeposit_Master
                              join s in CSISContext.TermDeposit_Schemes on m.TDScheme_Id equals s.TDScheme_Id
                              where ids.Contains(m.TD_Id) && m.BrCode == brCode && s.BrCode == brCode
                              select new { m.TD_Id, s.Led_Id, s.IntLed_Id, s.PiLed_Id }).AsNoTracking().ToListAsync();
            return rows.GroupBy(r => r.TD_Id).ToDictionary(g => g.Key, g => (g.First().Led_Id, g.First().IntLed_Id, g.First().PiLed_Id));
        }

        public async Task<List<string>> GetPendingRefundDataAsync(string brCode, decimal excludeStagingId)
        {
            var pending = new[] { "I", "M" };
            var rows = await (from d in CSISContext.Staging_Details
                              join sm in CSISContext.Staging_Master on d.Staging_Id equals sm.Staging_Id
                              where d.Related_Account_Id == RdRefundAccountId
                                    && pending.Contains(d.Staging_Status!) && pending.Contains(sm.Staging_Status!)
                                    && d.Staging_Id != excludeStagingId
                                    && d.BrCode == brCode && sm.BrCode == brCode
                              select d.Related_Account_Data).AsNoTracking().ToListAsync();
            return rows.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!).ToList();
        }

        public async Task<Dictionary<decimal, DateTime?>> GetUnrecoveredDemandAsync(IEnumerable<decimal> tdIds, string brCode)
        {
            var ids = tdIds.Distinct().ToList();
            var rows = await (from d in CSISContext.Mem_Demand
                              join dm in CSISContext.Mem_Demand_Master on d.Demand_Id equals dm.Demand_Id
                              join m in CSISContext.mem_master on d.Mem_Id equals m.mem_id
                              where ids.Contains(d.TD_Id) && d.Is_Active
                                    && dm.IsDemandUpdated && !dm.DemandMaster_Delete && dm.Voc_Status == "V"
                                    && d.BrCode == brCode && dm.BrCode == brCode && m.brcode == brCode
                                    && !(from s in CSISContext.Mem_Demand_Status
                                         where s.Demand_Id == d.Demand_Id && s.Office_Id == m.office_id
                                               && s.is_Active && s.Is_Recovery_Saved && s.BrCode == brCode
                                         select s.Id).Any()
                              select new { d.TD_Id, dm.DemandDate }).AsNoTracking().ToListAsync();
            return rows.GroupBy(r => r.TD_Id).ToDictionary(g => g.Key, g => g.Max(x => x.DemandDate));
        }

        public async Task MarkClosedAsync(IEnumerable<decimal> tdIds, string brCode)
        {
            var ids = tdIds.Distinct().ToList();
            var masters = await (from m in CSISContext.TermDeposit_Master
                                 where ids.Contains(m.TD_Id) && m.BrCode == brCode
                                 select m).ToListAsync();
            foreach (var m in masters) m.AccountClosed = true;
        }
    }
}
