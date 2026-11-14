using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;

namespace Infin8.Coapp.Repository
{
    /// <summary>
    /// Queries for the ECS RD Receipt component, acc 2 (spec 14). Every query filters BrCode on every alias.
    /// Nothing here calls SaveChanges — TransactionsHandler.SaveTransaction owns the transaction.
    /// </summary>
    public class EcsRdReceiptRepository : Repository<TermDeposit_Master>, IEcsRdReceiptRepository
    {
        public const int RdReceiptAccountId = 2;

        public CSISContext CSISContext => (CSISContext)Context;

        public EcsRdReceiptRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<DtoEcsRdReceiptLine>> GetEligibleRdsAsync(decimal memId, string brCode)
        {
            var rds = await (from m in CSISContext.TermDeposit_Master
                             join s in CSISContext.TermDeposit_Schemes on m.TDScheme_Id equals s.TDScheme_Id
                             where m.BrCode == brCode && s.BrCode == brCode
                                   && s.TDSchemeType == "R" && !m.TD_Delete && !m.AccountClosed && m.Voc_Status == "V"
                                   && (m.Mem_Id == memId
                                       || (from j in CSISContext.TermDeposit_Members
                                           where j.TD_Id == m.TD_Id && j.Mem_Id == memId && !j.TDMem_Delete && j.BrCode == brCode
                                           select j.TDMem_Id).Any())
                             orderby m.TD_No
                             select new DtoEcsRdReceiptLine
                             {
                                 TD_Id = m.TD_Id,
                                 TD_No = m.TD_No,
                                 TDScheme_Id = m.TDScheme_Id,
                                 Instalment = m.DepositAmount,
                                 ValueDate = m.ValueDate,
                                 PeriodInMonths = m.PeriodInMonths,
                                 RateOfInterest = m.RateOfInterest,
                                 MaturityDate = m.MaturityDate,
                                 MaturityAmount = m.MaturityAmount
                             }).AsNoTracking().ToListAsync();
            if (rds.Count == 0) return rds;

            var ids = rds.Select(r => r.TD_Id).ToList();
            var sums = await (from t in CSISContext.TermDeposit_Trn
                              where ids.Contains(t.TD_Id) && !t.TD_Delete && t.Voc_Status == "V" && t.BrCode == brCode
                              group t by t.TD_Id into g
                              select new
                              {
                                  TD_Id = g.Key,
                                  Received = g.Sum(x => x.DepositReceiptAmount),
                                  Paid = g.Sum(x => x.DepositPaidAmount),
                                  PenalCalculated = g.Sum(x => x.PenalCalculatedAmount),
                                  PenalReceived = g.Sum(x => x.PenalReceivedAmount),
                                  LastPenalApplied = g.Max(x => x.PenalAppliedDate),
                                  LastInstalment = g.Max(x => x.LastInstalmentDate),
                                  LastReceipt = g.Max(x => (DateTime?)x.Trn_Date)
                              }).AsNoTracking().ToListAsync();
            var byId = sums.ToDictionary(x => x.TD_Id);

            var result = new List<DtoEcsRdReceiptLine>();
            foreach (var rd in rds)
            {
                if (byId.TryGetValue(rd.TD_Id, out var s))
                {
                    if (Math.Abs(s.Paid) > 0.005) continue;   // a payout has been made: not receivable (§4)
                    rd.Received = s.Received;
                    rd.PenalCalculated = s.PenalCalculated;
                    rd.PenalReceived = s.PenalReceived;
                    rd.LastPenalAppliedDate = s.LastPenalApplied;
                    rd.LastInstalmentDate = s.LastInstalment;
                    rd.LastReceiptDate = s.LastReceipt;
                }
                result.Add(rd);
            }
            return result;
        }

        public async Task<List<TermDeposit_Roi_Template>> GetPenalTemplatesAsync(IEnumerable<decimal> schemeIds, string brCode)
        {
            var ids = schemeIds.Select(x => (int)x).Distinct().ToList();
            return await (from r in CSISContext.TermDeposit_Roi_Template
                          where ids.Contains(r.TDScheme_Id) && !r.TDRoi_Delete && r.PeriodType == "M" && r.BrCode == brCode
                          select r).AsNoTracking().ToListAsync();
        }

        public async Task<Dictionary<decimal, (decimal LedId, decimal PiLedId)>> GetRdLedgersAsync(IEnumerable<decimal> tdIds, string brCode)
        {
            var ids = tdIds.Distinct().ToList();
            var rows = await (from m in CSISContext.TermDeposit_Master
                              join s in CSISContext.TermDeposit_Schemes on m.TDScheme_Id equals s.TDScheme_Id
                              where ids.Contains(m.TD_Id) && m.BrCode == brCode && s.BrCode == brCode
                              select new { m.TD_Id, s.Led_Id, s.PiLed_Id }).AsNoTracking().ToListAsync();
            return rows.GroupBy(r => r.TD_Id).ToDictionary(g => g.Key, g => (g.First().Led_Id, g.First().PiLed_Id));
        }

        public async Task<List<string>> GetPendingReceiptDataAsync(string brCode, decimal excludeStagingId)
        {
            var pending = new[] { "I", "M" };
            var rows = await (from d in CSISContext.Staging_Details
                              join sm in CSISContext.Staging_Master on d.Staging_Id equals sm.Staging_Id
                              where d.Related_Account_Id == RdReceiptAccountId
                                    && pending.Contains(d.Staging_Status!) && pending.Contains(sm.Staging_Status!)
                                    && d.Staging_Id != excludeStagingId
                                    && d.BrCode == brCode && sm.BrCode == brCode
                              select d.Related_Account_Data).AsNoTracking().ToListAsync();
            return rows.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!).ToList();
        }

        public async Task<string?> GetRdWithLaterRowsAsync(decimal vocId, string brCode)
        {
            var own = await (from t in CSISContext.TermDeposit_Trn
                             where t.Voc_Id == vocId && t.BrCode == brCode
                             group t by t.TD_Id into g
                             select new { TD_Id = g.Key, MaxSlNo = g.Max(x => x.Trn_SlNo) }).AsNoTracking().ToListAsync();
            foreach (var o in own)
            {
                bool later = await (from t in CSISContext.TermDeposit_Trn
                                    where t.TD_Id == o.TD_Id && t.Trn_SlNo > o.MaxSlNo && t.Voc_Id != vocId
                                          && !t.TD_Delete && t.Voc_Status == "V" && t.BrCode == brCode
                                    select t.TDTrn_Id).AnyAsync();
                if (later)
                {
                    return await (from m in CSISContext.TermDeposit_Master
                                  where m.TD_Id == o.TD_Id && m.BrCode == brCode
                                  select m.TD_No).FirstOrDefaultAsync() ?? o.TD_Id.ToString();
                }
            }
            return null;
        }
    }
}
