using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;

namespace Infin8.Coapp.Repository
{
    /// <summary>
    /// Queries and change-tracker writes for ECS Recovery Appropriation (spec 13). Every query filters BrCode on every
    /// alias. Writes never call SaveChanges — EcsRecoveryHandler does.
    /// </summary>
    public class EcsRecoveryRepository : Repository<Mem_Demand>, IEcsRecoveryRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;

        public EcsRecoveryRepository(DbContext context) : base(context)
        {
        }

        // =====================================================================
        // Offices, members, lines
        // =====================================================================

        public async Task<Dictionary<decimal, string>> GetOfficeNamesAsync(string brCode)
        {
            var rows = await (from r in CSISContext.Refer_Data
                              where r.ReferType == 3 && r.BrCode == brCode
                              select new { r.ReferId, r.ReferName }).ToListAsync();
            return rows.GroupBy(r => r.ReferId).ToDictionary(g => g.Key, g => g.First().ReferName ?? $"Office {g.Key}");
        }

        public async Task<List<EcsRecoveryOfficeStatRow>> GetOfficeStatsAsync(decimal demandId, string brCode)
        {
            return await (from d in CSISContext.Mem_Demand
                          join m in CSISContext.mem_master on d.Mem_Id equals m.mem_id
                          where d.Demand_Id == demandId && d.Is_Active && d.BrCode == brCode && m.brcode == brCode
                          group new { d, m } by m.office_id into g
                          select new EcsRecoveryOfficeStatRow
                          {
                              OfficeId = g.Key,
                              Members = g.Select(x => x.d.Mem_Id).Distinct().Count(),
                              Demand = g.Sum(x => x.d.Stop_Demand ? 0 : x.d.Total_Demand),
                              Recovery = g.Sum(x => x.d.Total_Collection),
                              RecoveryId = g.Max(x => x.d.Recovery_Id),
                              RecoveryDate = g.Max(x => x.d.Recovery_Date)
                          }).ToListAsync();
        }

        public async Task<List<EcsRecoveryMemberRow>> GetOfficeMembersAsync(decimal officeId, string brCode)
        {
            return await (from m in CSISContext.mem_master
                          where m.office_id == officeId && !m.memberdelete && !m.isaccountclosed && m.brcode == brCode
                          orderby m.memberno
                          select new EcsRecoveryMemberRow
                          {
                              MemId = m.mem_id,
                              MemberNo = m.memberno,
                              PerNo = m.perno,
                              MemberName = m.membername,
                              OfficeId = m.office_id
                          }).AsNoTracking().ToListAsync();
        }

        public async Task<List<EcsRecoveryMemberRow>> GetBranchMembersAsync(string brCode)
        {
            return await (from m in CSISContext.mem_master
                          where !m.memberdelete && !m.isaccountclosed && m.brcode == brCode
                          select new EcsRecoveryMemberRow
                          {
                              MemId = m.mem_id,
                              MemberNo = m.memberno,
                              PerNo = m.perno,
                              MemberName = m.membername,
                              OfficeId = m.office_id
                          }).AsNoTracking().ToListAsync();
        }

        public async Task<List<Mem_Demand>> GetOfficeLinesAsync(decimal demandId, decimal officeId, string brCode, bool forUpdate)
        {
            var q = from d in CSISContext.Mem_Demand
                    join m in CSISContext.mem_master on d.Mem_Id equals m.mem_id
                    where d.Demand_Id == demandId && d.Is_Active && d.BrCode == brCode
                          && m.office_id == officeId && m.brcode == brCode
                    orderby d.Mem_Id, d.Demand_Type, d.Id
                    select d;
            return forUpdate ? await q.ToListAsync() : await q.AsNoTracking().ToListAsync();
        }

        public async Task<List<Mem_CollectionPriority>> GetPrioritiesAsync(string brCode)
        {
            return await (from p in CSISContext.Mem_CollectionPriority
                          where p.Is_Active && p.BrCode == brCode
                          orderby p.PriorityNo, p.Priority_Id
                          select p).AsNoTracking().ToListAsync();
        }

        // =====================================================================
        // Loans and due-to balances (valid rows, all dates)
        // =====================================================================

        public async Task<Dictionary<decimal, EcsRecoveryLoanRow>> GetLoansAsync(IEnumerable<decimal> loanIds, string brCode)
        {
            var ids = loanIds.Distinct().ToList();
            if (ids.Count == 0) return new();
            var loans = await (from l in CSISContext.Loan_Master
                               where ids.Contains(l.Loan_Id) && l.BrCode == brCode
                               select new EcsRecoveryLoanRow
                               {
                                   LoanId = l.Loan_Id,
                                   MemId = l.Mem_Id,
                                   SchemeId = l.Scheme_Id,
                                   SanDate = l.San_Date
                               }).AsNoTracking().ToListAsync();
            await FillBalancesAsync(loans, brCode);
            return loans.GroupBy(l => l.LoanId).ToDictionary(g => g.Key, g => g.First());
        }

        public async Task<List<EcsRecoveryLoanRow>> GetOpenLoansOfMembersAsync(IEnumerable<decimal> memIds, string brCode)
        {
            var ids = memIds.Distinct().ToList();
            if (ids.Count == 0) return new();
            var loans = await (from l in CSISContext.Loan_Master
                               where ids.Contains(l.Mem_Id) && l.Loan_Type == 1 && !l.Loan_Delete && !l.IsAccountClosed
                                     && l.Voc_Status == "V" && l.BrCode == brCode
                               select new EcsRecoveryLoanRow
                               {
                                   LoanId = l.Loan_Id,
                                   MemId = l.Mem_Id,
                                   SchemeId = l.Scheme_Id,
                                   SanDate = l.San_Date
                               }).AsNoTracking().ToListAsync();
            await FillBalancesAsync(loans, brCode);
            return loans.Where(l => l.Disb - l.PrlColl > 0.005).ToList();
        }

        private async Task FillBalancesAsync(List<EcsRecoveryLoanRow> loans, string brCode)
        {
            var ids = loans.Select(l => l.LoanId).Distinct().ToList();
            if (ids.Count == 0) return;
            var sums = await (from t in CSISContext.Loan_Trn
                              where ids.Contains(t.Loan_Id) && !t.TrnTr_Delete && t.Voc_Status == "V" && t.BrCode == brCode
                              group t by t.Loan_Id into g
                              select new
                              {
                                  LoanId = g.Key,
                                  Disb = g.Sum(x => x.Disb_Amt),
                                  PrlColl = g.Sum(x => x.PrlColl_Amt),
                                  IntCalc = g.Sum(x => x.IntCalc_Amt),
                                  IntColl = g.Sum(x => x.IntColl_Amt),
                                  PICalc = g.Sum(x => x.PICalc_Amt),
                                  PIColl = g.Sum(x => x.PIColl_Amt)
                              }).ToListAsync();
            var byId = sums.ToDictionary(s => s.LoanId);
            foreach (var l in loans)
            {
                if (!byId.TryGetValue(l.LoanId, out var s)) continue;
                l.Disb = s.Disb;
                l.PrlColl = s.PrlColl;
                l.IntCalc = s.IntCalc;
                l.IntColl = s.IntColl;
                l.PICalc = s.PICalc;
                l.PIColl = s.PIColl;
            }
        }

        public async Task<Dictionary<(decimal, decimal), double>> GetDueToBalancesAsync(IEnumerable<decimal> memIds, string brCode)
        {
            var ids = memIds.Distinct().ToList();
            if (ids.Count == 0) return new();
            var rows = await (from t in CSISContext.Mem_Trn
                              where ids.Contains(t.Mem_Id) && t.Trn_Type == 1 && !t.MemTrn_Delete && t.Voc_Status == "V"
                                    && t.BrCode == brCode
                              group t by new { t.Mem_Id, t.Led_Id } into g
                              select new { g.Key.Mem_Id, g.Key.Led_Id, Bal = g.Sum(x => x.Pmt_Amt) - g.Sum(x => x.Rpt_Amt) })
                             .ToListAsync();
            return rows.ToDictionary(r => (r.Mem_Id, r.Led_Id), r => r.Bal);
        }

        public async Task<Dictionary<decimal, string>> GetLoanLabelsAsync(IEnumerable<decimal> loanIds, string brCode)
        {
            var ids = loanIds.Distinct().ToList();
            if (ids.Count == 0) return new();
            var rows = await (from l in CSISContext.Loan_Master
                              join s in CSISContext.Loan_Schemes on l.Scheme_Id equals s.Scheme_Id
                              where ids.Contains(l.Loan_Id) && l.BrCode == brCode && s.BrCode == brCode
                              select new { l.Loan_Id, l.Loan_No, s.Scheme_Name }).ToListAsync();
            return rows.GroupBy(r => r.Loan_Id).ToDictionary(g => g.Key, g => $"{g.First().Scheme_Name} {g.First().Loan_No}".Trim());
        }

        // =====================================================================
        // Save inputs
        // =====================================================================

        public async Task<int> GetMaxRecoveryIdAsync(string brCode)
        {
            return await CSISContext.Mem_Demand.Where(d => d.BrCode == brCode).MaxAsync(d => (int?)d.Recovery_Id) ?? 0;
        }

        public async Task<List<EcsRecoveryLedgerRow>> GetLedgersAsync(string brCode)
        {
            var banks = await (from b in CSISContext.Map_Banks
                               where b.BrCode == brCode
                               select b.Led_Id).ToListAsync();
            var bankSet = banks.ToHashSet();
            var ledgers = await (from l in CSISContext.Fin_Ledger
                                 where !l.Led_Delete && l.BrCode == brCode
                                 orderby l.Led_Name
                                 select new EcsRecoveryLedgerRow { LedId = l.Led_Id, LedName = l.Led_Name }).ToListAsync();
            foreach (var l in ledgers) l.IsBank = bankSet.Contains(l.LedId);
            return ledgers;
        }

        public async Task<decimal?> GetLastDebitLedgerAsync(decimal officeId, string brCode)
        {
            return await (from s in CSISContext.Mem_Demand_Status
                          join v in CSISContext.Fin_Voucher on s.Voc_Id equals v.Voc_Id
                          join t in CSISContext.Fin_Voucher_Trn on v.Voc_Id equals t.Voc_Id
                          where s.Office_Id == officeId && s.is_Active && s.Is_Recovery_Saved && s.BrCode == brCode
                                && !v.Voc_Delete && v.Voc_Status == "V" && v.BrCode == brCode
                                && t.Voc_Pmt > 0 && !t.FinVocTr_Delete && t.BrCode == brCode
                          orderby v.Voc_Date descending, v.Voc_Id descending
                          select (decimal?)t.Led_Id).FirstOrDefaultAsync();
        }

        public async Task<Dictionary<int, Loan_Schemes>> GetSchemesAsync(IEnumerable<int> schemeIds, string brCode)
        {
            var ids = schemeIds.Distinct().ToList();
            var rows = await (from s in CSISContext.Loan_Schemes
                              where ids.Contains(s.Scheme_Id) && s.BrCode == brCode
                              select s).AsNoTracking().ToListAsync();
            return rows.GroupBy(s => s.Scheme_Id).ToDictionary(g => g.Key, g => g.First());
        }

        public async Task<Dictionary<int, (decimal LedId, decimal PiLedId)>> GetDepositLedgersAsync(string brCode)
        {
            var rows = await (from d in CSISContext.Deposit_Masters
                              where d.BrCode == brCode
                              select new { d.Deposit_Id, d.Led_Id, d.PI_Led_Id }).ToListAsync();
            return rows.GroupBy(r => r.Deposit_Id).ToDictionary(g => g.Key, g => (g.First().Led_Id, g.First().PI_Led_Id));
        }

        public async Task<Dictionary<decimal, decimal>> GetRdLedgersAsync(IEnumerable<decimal> tdIds, string brCode)
        {
            var ids = tdIds.Distinct().ToList();
            if (ids.Count == 0) return new();
            var rows = await (from m in CSISContext.TermDeposit_Master
                              join s in CSISContext.TermDeposit_Schemes on m.TDScheme_Id equals s.TDScheme_Id
                              where ids.Contains(m.TD_Id) && m.BrCode == brCode && s.BrCode == brCode
                              select new { m.TD_Id, s.Led_Id }).ToListAsync();
            return rows.GroupBy(r => r.TD_Id).ToDictionary(g => g.Key, g => g.First().Led_Id);
        }

        public async Task<Dictionary<decimal, DateTime>> GetLastLoanTrnDatesAsync(IEnumerable<decimal> loanIds, string brCode)
        {
            var ids = loanIds.Distinct().ToList();
            if (ids.Count == 0) return new();
            var rows = await (from t in CSISContext.Loan_Trn
                              where ids.Contains(t.Loan_Id) && !t.TrnTr_Delete && t.Voc_Status == "V" && t.BrCode == brCode
                              group t by t.Loan_Id into g
                              select new { g.Key, Last = g.Max(x => x.Trn_Date) }).ToListAsync();
            return rows.ToDictionary(r => r.Key, r => r.Last);
        }

        public async Task<Dictionary<decimal, string?>> GetVoucherNosAsync(IEnumerable<decimal> vocIds, string brCode)
        {
            var ids = vocIds.Distinct().ToList();
            if (ids.Count == 0) return new();
            var rows = await (from v in CSISContext.Fin_Voucher
                              where ids.Contains(v.Voc_Id) && v.BrCode == brCode
                              select new { v.Voc_Id, v.Voc_No }).ToListAsync();
            return rows.GroupBy(r => r.Voc_Id).ToDictionary(g => g.Key, g => g.First().Voc_No);
        }

        // =====================================================================
        // Writes
        // =====================================================================

        public async Task<EcsRecoveryIdAllocator> CreateIdAllocatorAsync(string brCode)
        {
            // Ids are branch-prefixed (brcode + 7-digit serial): MAX within the branch, never below the zero id.
            decimal zero = decimal.TryParse(brCode + "0000000", out var z) ? z : 0;
            decimal Floor(decimal? v) => Math.Max(v ?? 0, zero);

            return new EcsRecoveryIdAllocator(CSISContext, brCode)
            {
                LineId = Floor(await CSISContext.Mem_Demand.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Id)),
                VocId = Floor(await CSISContext.Fin_Voucher.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Voc_Id)),
                VocTrnId = Floor(await CSISContext.Fin_Voucher_Trn.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Voc_Trn_Id)),
                FvbId = Floor(await CSISContext.Fin_Voucher_Bank.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Fvb_Id)),
                LoanTrnId = Floor(await CSISContext.Loan_Trn.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Trn_Id)),
                DepositTrnId = Floor(await CSISContext.Deposit_Trn.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Trn_Id)),
                MemTrnId = Floor(await CSISContext.Mem_Trn.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Mem_Trn_Id)),
                RdTrnId = Floor(await CSISContext.TermDeposit_Trn.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.TDTrn_Id)),
            };
        }

        public async Task AddLinesAsync(IEnumerable<Mem_Demand> lines)
        {
            await CSISContext.Mem_Demand.AddRangeAsync(lines);
        }

        public async Task AddSaveAsync(EcsRecoverySavePosting p)
        {
            if (p.Voucher != null) await CSISContext.Fin_Voucher.AddAsync(p.Voucher);
            await CSISContext.Fin_Voucher_Trn.AddRangeAsync(p.VoucherTrns);
            await CSISContext.Fin_Voucher_Bank.AddRangeAsync(p.BankRows);
            await CSISContext.Loan_Trn.AddRangeAsync(p.LoanTrns);
            await CSISContext.Deposit_Trn.AddRangeAsync(p.DepositTrns);
            await CSISContext.Mem_Trn.AddRangeAsync(p.MemTrns);
            await CSISContext.TermDeposit_Trn.AddRangeAsync(p.RdTrns);
        }

        // =====================================================================
        // Delete saved recovery
        // =====================================================================

        public async Task<EcsRecoveryReceipts> GetReceiptsForUpdateAsync(decimal vocId, string brCode)
        {
            return new EcsRecoveryReceipts
            {
                Vouchers = await (from v in CSISContext.Fin_Voucher
                                  where v.Voc_Id == vocId && !v.Voc_Delete && v.BrCode == brCode select v).ToListAsync(),
                VoucherTrns = await (from t in CSISContext.Fin_Voucher_Trn
                                     where t.Voc_Id == vocId && !t.FinVocTr_Delete && t.BrCode == brCode select t).ToListAsync(),
                BankRows = await (from b in CSISContext.Fin_Voucher_Bank
                                  where b.Voc_Id == vocId && !b.Fvb_Delete && b.BrCode == brCode select b).ToListAsync(),
                LoanTrns = await (from t in CSISContext.Loan_Trn
                                  where t.Voc_Id == vocId && !t.TrnTr_Delete && t.BrCode == brCode select t).ToListAsync(),
                DepositTrns = await (from t in CSISContext.Deposit_Trn
                                     where t.Voc_Id == vocId && t.Is_Active && t.BrCode == brCode select t).ToListAsync(),
                MemTrns = await (from t in CSISContext.Mem_Trn
                                 where t.Voc_Id == vocId && !t.MemTrn_Delete && t.BrCode == brCode select t).ToListAsync(),
                RdTrns = await (from t in CSISContext.TermDeposit_Trn
                                where t.Voc_Id == vocId && !t.TD_Delete && t.BrCode == brCode select t).ToListAsync(),
            };
        }

        public async Task<List<string>> GetLaterTransactionsAsync(decimal vocId, string brCode)
        {
            var loans = await (from r in CSISContext.Loan_Trn
                               join t in CSISContext.Loan_Trn on r.Loan_Id equals t.Loan_Id
                               join m in CSISContext.Loan_Master on r.Loan_Id equals m.Loan_Id
                               where r.Voc_Id == vocId && !r.TrnTr_Delete && r.BrCode == brCode
                                     && t.Trn_SlNo > r.Trn_SlNo && !t.TrnTr_Delete && t.Voc_Status == "V" && t.BrCode == brCode
                                     && m.BrCode == brCode
                               select "Loan " + m.Loan_No).Distinct().ToListAsync();

            var deposits = await (from r in CSISContext.Deposit_Trn
                                  join t in CSISContext.Deposit_Trn
                                      on new { r.Mem_Id, r.Deposit_Id } equals new { t.Mem_Id, t.Deposit_Id }
                                  join m in CSISContext.mem_master on r.Mem_Id equals m.mem_id
                                  join dm in CSISContext.Deposit_Masters on r.Deposit_Id equals dm.Deposit_Id
                                  where r.Voc_Id == vocId && r.Is_Active && r.BrCode == brCode
                                        && t.Trn_SlNo > r.Trn_SlNo && t.Is_Active && t.BrCode == brCode
                                        && m.brcode == brCode && dm.BrCode == brCode
                                  select dm.Deposit_Name + " of member " + m.memberno).Distinct().ToListAsync();

            return loans.Concat(deposits).ToList();
        }

        // =====================================================================
        // Recovery refund (spec 13 §16)
        // =====================================================================

        public async Task<List<Mem_Demand>> GetMemberLinesAsync(decimal demandId, decimal memId, string brCode, bool forUpdate)
        {
            var q = from d in CSISContext.Mem_Demand
                    where d.Demand_Id == demandId && d.Mem_Id == memId && d.Is_Active && d.BrCode == brCode
                    orderby d.Demand_Type, d.Id
                    select d;
            return forUpdate ? await q.ToListAsync() : await q.AsNoTracking().ToListAsync();
        }

        public async Task<EcsRecoveryReceipts> GetMemberReceiptsForUpdateAsync(decimal vocId, decimal memId, string brCode)
        {
            return new EcsRecoveryReceipts
            {
                Vouchers = await (from v in CSISContext.Fin_Voucher
                                  where v.Voc_Id == vocId && !v.Voc_Delete && v.BrCode == brCode select v).ToListAsync(),
                VoucherTrns = await (from t in CSISContext.Fin_Voucher_Trn
                                     where t.Voc_Id == vocId && !t.FinVocTr_Delete && t.BrCode == brCode
                                     orderby t.Voc_Trn_Id select t).ToListAsync(),
                LoanTrns = await (from t in CSISContext.Loan_Trn
                                  join l in CSISContext.Loan_Master on t.Loan_Id equals l.Loan_Id
                                  where t.Voc_Id == vocId && !t.TrnTr_Delete && t.Trn_Status == "R" && t.BrCode == brCode
                                        && l.Mem_Id == memId && l.BrCode == brCode
                                  orderby t.Loan_Id select t).ToListAsync(),
                DepositTrns = await (from t in CSISContext.Deposit_Trn
                                     where t.Voc_Id == vocId && t.Mem_Id == memId && t.Is_Active && t.BrCode == brCode
                                     orderby t.Deposit_Id select t).ToListAsync(),
                RdTrns = await (from t in CSISContext.TermDeposit_Trn
                                join m in CSISContext.TermDeposit_Master on t.TD_Id equals m.TD_Id
                                where t.Voc_Id == vocId && !t.TD_Delete && t.BrCode == brCode
                                      && m.Mem_Id == memId && m.BrCode == brCode
                                orderby t.TD_Id select t).ToListAsync(),
            };
        }

        public async Task<List<string>> GetLaterMemberTransactionsAsync(decimal vocId, decimal memId, string brCode)
        {
            var loans = await (from r in CSISContext.Loan_Trn
                               join m in CSISContext.Loan_Master on r.Loan_Id equals m.Loan_Id
                               join t in CSISContext.Loan_Trn on r.Loan_Id equals t.Loan_Id
                               where r.Voc_Id == vocId && !r.TrnTr_Delete && r.BrCode == brCode
                                     && m.Mem_Id == memId && m.BrCode == brCode
                                     && t.Trn_SlNo > r.Trn_SlNo && !t.TrnTr_Delete && t.Voc_Status == "V" && t.BrCode == brCode
                               select "Loan " + m.Loan_No).Distinct().ToListAsync();

            var deposits = await (from r in CSISContext.Deposit_Trn
                                  join t in CSISContext.Deposit_Trn
                                      on new { r.Mem_Id, r.Deposit_Id } equals new { t.Mem_Id, t.Deposit_Id }
                                  join dm in CSISContext.Deposit_Masters on r.Deposit_Id equals dm.Deposit_Id
                                  where r.Voc_Id == vocId && r.Mem_Id == memId && r.Is_Active && r.BrCode == brCode
                                        && t.Trn_SlNo > r.Trn_SlNo && t.Is_Active && t.BrCode == brCode && dm.BrCode == brCode
                                  select "" + dm.Deposit_Name).Distinct().ToListAsync();

            var rds = await (from r in CSISContext.TermDeposit_Trn
                             join m in CSISContext.TermDeposit_Master on r.TD_Id equals m.TD_Id
                             join t in CSISContext.TermDeposit_Trn on r.TD_Id equals t.TD_Id
                             where r.Voc_Id == vocId && !r.TD_Delete && r.BrCode == brCode && m.Mem_Id == memId && m.BrCode == brCode
                                   && t.Trn_SlNo > r.Trn_SlNo && !t.TD_Delete && t.Voc_Status == "V" && t.BrCode == brCode
                             select "RD " + m.TD_No).Distinct().ToListAsync();

            return loans.Concat(deposits).Concat(rds).ToList();
        }

        public async Task<Dictionary<int, string>> GetDepositNamesAsync(string brCode)
        {
            var rows = await (from d in CSISContext.Deposit_Masters
                              where d.BrCode == brCode
                              select new { d.Deposit_Id, d.Deposit_Name }).ToListAsync();
            return rows.GroupBy(r => r.Deposit_Id).ToDictionary(g => g.Key, g => g.First().Deposit_Name ?? $"Deposit {g.Key}");
        }

        public async Task<Dictionary<decimal, string>> GetRdNosAsync(IEnumerable<decimal> tdIds, string brCode)
        {
            var ids = tdIds.Distinct().ToList();
            if (ids.Count == 0) return new();
            var rows = await (from m in CSISContext.TermDeposit_Master
                              where ids.Contains(m.TD_Id) && m.BrCode == brCode
                              select new { m.TD_Id, m.TD_No }).ToListAsync();
            return rows.GroupBy(r => r.TD_Id).ToDictionary(g => g.Key, g => $"RD {g.First().TD_No}");
        }

        // =====================================================================
        // Spec 13a — recalculation as on the recovery date, Maker / Checker
        // =====================================================================

        public async Task<Dictionary<decimal, string>> GetUserNamesAsync(IEnumerable<decimal> userIds, string brCode)
        {
            var ids = userIds.Distinct().ToList();
            if (ids.Count == 0) return new();
            var rows = await (from u in CSISContext.Users
                              where ids.Contains(u.Id) && u.BrCode == brCode
                              select new { u.Id, u.Username, u.First_Name, u.Last_Name }).ToListAsync();
            return rows.GroupBy(r => r.Id).ToDictionary(g => g.Key, g =>
            {
                var u = g.First();
                var full = $"{u.First_Name} {u.Last_Name}".Trim();
                return full.Length > 0 ? full : u.Username ?? $"User {u.Id}";
            });
        }

        public async Task<Dictionary<decimal, EcsRecoveryAccrualLoan>> GetAccrualInputsAsync(IEnumerable<decimal> loanIds, string brCode)
        {
            var ids = loanIds.Distinct().ToList();
            if (ids.Count == 0) return new();
            var loans = await (from l in CSISContext.Loan_Master
                               join s in CSISContext.Loan_Schemes on l.Scheme_Id equals s.Scheme_Id
                               where ids.Contains(l.Loan_Id) && l.BrCode == brCode && s.BrCode == brCode
                               select new EcsRecoveryAccrualLoan
                               {
                                   LoanId = l.Loan_Id,
                                   IntApplication = s.Int_Application,
                                   PIApplication = s.PI_Application,
                                   Roi = l.Roi,
                                   Pi = l.Pi
                               }).AsNoTracking().ToListAsync();
            var trns = await (from t in CSISContext.Loan_Trn
                              where ids.Contains(t.Loan_Id) && !t.TrnTr_Delete && t.Voc_Status == "V" && t.BrCode == brCode
                              orderby t.Loan_Id, t.Trn_Date, t.Trn_SlNo
                              select t).AsNoTracking().ToListAsync();
            var rates = await (from r in CSISContext.Loan_Roi
                               where ids.Contains(r.Loan_Id) && r.Agency == "S" && !r.Loanroi_Delete && r.Voc_Status == "V"
                                     && r.BrCode == brCode
                               orderby r.Loan_Id, r.Roi_Wef
                               select r).AsNoTracking().ToListAsync();
            var trnsBy = trns.ToLookup(t => t.Loan_Id);
            var ratesBy = rates.ToLookup(r => r.Loan_Id);
            var result = new Dictionary<decimal, EcsRecoveryAccrualLoan>();
            foreach (var l in loans)
            {
                if (result.ContainsKey(l.LoanId)) continue;
                l.Trns = trnsBy[l.LoanId].ToList();
                l.Rates = ratesBy[l.LoanId].ToList();
                result[l.LoanId] = l;
            }
            return result;
        }

        public async Task<Dictionary<(decimal MemId, int DepositId), EcsRecoveryDepositRow>> GetDepositRevisionAsync(
            IEnumerable<decimal> memIds, DateTime dueDate, string brCode)
        {
            var ids = memIds.Distinct().ToList();
            if (ids.Count == 0) return new();
            var due = dueDate.Date;
            var rows = await (from t in CSISContext.Deposit_Trn
                              where ids.Contains(t.Mem_Id) && t.Is_Active && t.BrCode == brCode
                              group t by new { t.Mem_Id, t.Deposit_Id } into g
                              select new EcsRecoveryDepositRow
                              {
                                  MemId = g.Key.Mem_Id,
                                  DepositId = g.Key.Deposit_Id,
                                  BeforeDemand = g.Sum(x => x.Trn_Date < due ? x.Demand_Amount : 0),
                                  BeforeReceipt = g.Sum(x => x.Trn_Date < due ? x.Receipt_Amount : 0),
                                  BeforePiCalc = g.Sum(x => x.Trn_Date < due ? x.Penal_Interest_Calculated_Amount : 0),
                                  BeforePiReceived = g.Sum(x => x.Trn_Date < due ? x.Penal_Interest_Received_Amount : 0),
                                  SinceReceipt = g.Sum(x => x.Trn_Date >= due ? x.Receipt_Amount : 0),
                                  SincePiReceived = g.Sum(x => x.Trn_Date >= due ? x.Penal_Interest_Received_Amount : 0),
                                  SincePaid = g.Sum(x => x.Trn_Date >= due ? x.Paid_Amount : 0),
                                  AllReceipt = g.Sum(x => x.Receipt_Amount),
                                  AllPaid = g.Sum(x => x.Paid_Amount)
                              }).ToListAsync();
            return rows.ToDictionary(r => (r.MemId, r.DepositId));
        }

        public async Task<Dictionary<decimal, EcsRecoveryRdRow>> GetRdRevisionAsync(IEnumerable<decimal> tdIds, DateTime dueDate, string brCode)
        {
            var ids = tdIds.Distinct().ToList();
            if (ids.Count == 0) return new();
            var due = dueDate.Date;
            var rds = await (from m in CSISContext.TermDeposit_Master
                             where ids.Contains(m.TD_Id) && m.BrCode == brCode
                             select new EcsRecoveryRdRow
                             {
                                 TdId = m.TD_Id,
                                 SchemeId = m.TDScheme_Id,
                                 ValueDate = m.ValueDate,
                                 PeriodInMonths = m.PeriodInMonths,
                                 Instalment = m.DepositAmount,
                                 Closed = m.TD_Delete || m.AccountClosed || m.Voc_Status != "V"
                             }).AsNoTracking().ToListAsync();
            var sums = await (from t in CSISContext.TermDeposit_Trn
                              where ids.Contains(t.TD_Id) && !t.TD_Delete && t.Voc_Status == "V" && t.BrCode == brCode
                              group t by t.TD_Id into g
                              select new
                              {
                                  TdId = g.Key,
                                  Received = g.Sum(x => x.DepositReceiptAmount),
                                  Paid = g.Sum(x => x.DepositPaidAmount),
                                  PenalCalcBefore = g.Sum(x => x.Trn_Date < due ? x.PenalCalculatedAmount : 0),
                                  PenalReceived = g.Sum(x => x.PenalReceivedAmount)
                              }).ToListAsync();
            var schemeIds = rds.Select(r => r.SchemeId).Distinct().ToList();
            var rates = await (from r in CSISContext.TermDeposit_Roi_Template
                               where schemeIds.Contains(r.TDScheme_Id) && !r.TDRoi_Delete && r.Wef <= due && r.BrCode == brCode
                               select r).AsNoTracking().ToListAsync();
            var result = new Dictionary<decimal, EcsRecoveryRdRow>();
            foreach (var rd in rds)
            {
                if (result.ContainsKey(rd.TdId)) continue;
                var s = sums.FirstOrDefault(x => x.TdId == rd.TdId);
                if (s != null)
                {
                    rd.Received = s.Received;
                    rd.Paid = s.Paid;
                    rd.PenalCalcBeforeDue = s.PenalCalcBefore;
                    rd.PenalReceived = s.PenalReceived;
                }
                // Same rate pick as spec 12's GetOpenRdsAsync.
                var schemeRates = rates.Where(r => r.TDScheme_Id == rd.SchemeId).ToList();
                var rate = schemeRates.Where(r => r.PeriodBegin <= rd.PeriodInMonths && rd.PeriodInMonths <= r.PeriodEnd)
                                      .OrderBy(r => r.Wef).LastOrDefault()
                           ?? schemeRates.OrderBy(r => r.Wef).LastOrDefault();
                rd.PenalRate = rate?.PenalRateForRD ?? 0;
                result[rd.TdId] = rd;
            }
            return result;
        }

        public async Task<List<EcsRecoveryMemberRow>> GetMembersByIdAsync(IEnumerable<decimal> memIds, string brCode)
        {
            var ids = memIds.Distinct().ToList();
            if (ids.Count == 0) return new();
            return await (from m in CSISContext.mem_master
                          where ids.Contains(m.mem_id) && m.brcode == brCode
                          select new EcsRecoveryMemberRow
                          {
                              MemId = m.mem_id,
                              MemberNo = m.memberno,
                              PerNo = m.perno,
                              MemberName = m.membername,
                              OfficeId = m.office_id,
                              IsOpen = !m.memberdelete && !m.isaccountclosed,
                              IsExpired = m.ismemexpired,
                              ExpiredDate = m.expireddate
                          }).AsNoTracking().ToListAsync();
        }
    }

    /// <summary>
    /// MAX+1 ids and per-account Trn_SlNo counters held in memory for one appropriation or save, so many rows can be
    /// added to one table before a single SaveChanges (spec 10 §13.2).
    /// </summary>
    public class EcsRecoveryIdAllocator
    {
        private readonly CSISContext _ctx;
        private readonly string _brCode;
        private readonly Dictionary<decimal, int> _loanSlNo = new();
        private readonly Dictionary<(decimal, int), int> _depositSlNo = new();
        private readonly Dictionary<(decimal, byte, decimal), int> _memTrnSlNo = new();
        private readonly Dictionary<decimal, int> _rdSlNo = new();

        internal EcsRecoveryIdAllocator(CSISContext ctx, string brCode)
        {
            _ctx = ctx;
            _brCode = brCode;
        }

        internal decimal LineId, VocId, VocTrnId, FvbId, LoanTrnId, DepositTrnId, MemTrnId, RdTrnId;

        public decimal ZeroId => decimal.TryParse(_brCode + "0000000", out var z) ? z : 0;
        public decimal NextLineId() => ++LineId;
        public decimal NextVocId() => ++VocId;
        public decimal NextVocTrnId() => ++VocTrnId;
        public decimal NextFvbId() => ++FvbId;
        public decimal NextLoanTrnId() => ++LoanTrnId;
        public decimal NextDepositTrnId() => ++DepositTrnId;
        public decimal NextMemTrnId() => ++MemTrnId;
        public decimal NextRdTrnId() => ++RdTrnId;

        public async Task PrimeSerialsAsync(IEnumerable<decimal> loanIds, IEnumerable<decimal> memIds, IEnumerable<decimal> tdIds)
        {
            var lIds = loanIds.Distinct().ToList();
            if (lIds.Count > 0)
            {
                var rows = await (from t in _ctx.Loan_Trn
                                  where lIds.Contains(t.Loan_Id) && t.BrCode == _brCode
                                  group t by t.Loan_Id into g
                                  select new { g.Key, Max = g.Max(x => x.Trn_SlNo) }).ToListAsync();
                foreach (var r in rows) _loanSlNo[r.Key] = r.Max;
            }

            var mIds = memIds.Distinct().ToList();
            if (mIds.Count > 0)
            {
                var dep = await (from t in _ctx.Deposit_Trn
                                 where mIds.Contains(t.Mem_Id) && t.BrCode == _brCode
                                 group t by new { t.Mem_Id, t.Deposit_Id } into g
                                 select new { g.Key.Mem_Id, g.Key.Deposit_Id, Max = g.Max(x => x.Trn_SlNo) }).ToListAsync();
                foreach (var r in dep) _depositSlNo[(r.Mem_Id, r.Deposit_Id)] = r.Max;

                var mem = await (from t in _ctx.Mem_Trn
                                 where mIds.Contains(t.Mem_Id) && t.BrCode == _brCode
                                 group t by new { t.Mem_Id, t.Trn_Type, t.Led_Id } into g
                                 select new { g.Key.Mem_Id, g.Key.Trn_Type, g.Key.Led_Id, Max = g.Max(x => x.Trn_SlNo) }).ToListAsync();
                foreach (var r in mem) _memTrnSlNo[(r.Mem_Id, r.Trn_Type, r.Led_Id)] = r.Max;
            }

            var tIds = tdIds.Distinct().ToList();
            if (tIds.Count > 0)
            {
                var rows = await (from t in _ctx.TermDeposit_Trn
                                  where tIds.Contains(t.TD_Id) && t.BrCode == _brCode
                                  group t by t.TD_Id into g
                                  select new { g.Key, Max = g.Max(x => x.Trn_SlNo) }).ToListAsync();
                foreach (var r in rows) _rdSlNo[r.Key] = r.Max;
            }
        }

        public int NextLoanTrnSlNo(decimal loanId) => _loanSlNo[loanId] = _loanSlNo.GetValueOrDefault(loanId) + 1;
        public int NextDepositTrnSlNo(decimal memId, int depositId) =>
            _depositSlNo[(memId, depositId)] = _depositSlNo.GetValueOrDefault((memId, depositId)) + 1;
        public int NextMemTrnSlNo(decimal memId, byte trnType, decimal ledId) =>
            _memTrnSlNo[(memId, trnType, ledId)] = _memTrnSlNo.GetValueOrDefault((memId, trnType, ledId)) + 1;
        public int NextRdTrnSlNo(decimal tdId) => _rdSlNo[tdId] = _rdSlNo.GetValueOrDefault(tdId) + 1;
    }
}
