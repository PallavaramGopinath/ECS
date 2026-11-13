using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;

namespace Infin8.Coapp.Repository
{
    /// <summary>
    /// Queries and change-tracker writes for the ECS Loan Disbursement page (spec 10).
    /// Every query filters BrCode on every alias. Writes never call SaveChanges — the handler does.
    /// </summary>
    public class EcsLoanDisbursementRepository : Repository<Loan_Sanction>, IEcsLoanDisbursementRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;

        public EcsLoanDisbursementRepository(DbContext context) : base(context)
        {
        }

        // =====================================================================
        // Lookups
        // =====================================================================

        public async Task<List<EcsBankLedgerVM>> GetBankLedgersAsync(string brCode)
        {
            return await (from mb in CSISContext.Map_Banks
                          join l in CSISContext.Fin_Ledger on mb.Led_Id equals l.Led_Id
                          where mb.BrCode == brCode && l.BrCode == brCode && !l.Led_Delete
                          orderby l.Led_Name
                          select new EcsBankLedgerVM { LedId = l.Led_Id, LedName = l.Led_Name }).ToListAsync();
        }

        public async Task<DateTime?> GetOpenBusinessDayAsync(string brCode)
        {
            return await (from c in CSISContext.Business_Day
                          where c.Calendar_Status == "N" && !c.Calendar_Delete && c.BrCode == brCode
                          orderby c.Calendar_Id
                          select (DateTime?)c.Calendar_Date).FirstOrDefaultAsync();
        }

        public async Task<List<Loan_Sanction>> GetSanctionsAsync(IEnumerable<decimal> sanctionIds, string brCode)
        {
            var ids = sanctionIds.Distinct().ToList();
            return await (from s in CSISContext.Loan_Sanction
                          where ids.Contains(s.LoanSanction_Id) && !s.LoanSanction_Delete && s.BrCode == brCode
                          select s).ToListAsync();
        }

        public async Task<List<Loan_Sanction_Trn>> GetRecoverableLinesAsync(IEnumerable<decimal> sanctionIds, string brCode)
        {
            var ids = sanctionIds.Distinct().ToList();
            return await (from t in CSISContext.Loan_Sanction_Trn
                          where ids.Contains(t.LoanSanction_Id) && !t.LoanSanctionTr_Delete
                                && t.ReceiptItem > 0 && t.BrCode == brCode
                          orderby t.LoanSanction_Id, t.SlNo
                          select t).ToListAsync();
        }

        public async Task<Dictionary<int, Loan_Schemes>> GetSchemesAsync(IEnumerable<int> schemeIds, string brCode)
        {
            var ids = schemeIds.Distinct().ToList();
            var rows = await (from s in CSISContext.Loan_Schemes
                              where ids.Contains(s.Scheme_Id) && !s.Scheme_Delete && s.BrCode == brCode
                              select s).ToListAsync();
            return rows.GroupBy(r => r.Scheme_Id).ToDictionary(g => g.Key, g => g.First());
        }

        public async Task<Dictionary<decimal, (string? MemberNo, string? PerNo, string? Name)>> GetMembersAsync(
            IEnumerable<decimal> memIds, string brCode)
        {
            var ids = memIds.Where(i => i > 0).Distinct().ToList();
            var rows = await (from m in CSISContext.mem_master
                              where ids.Contains(m.mem_id) && m.brcode == brCode
                              select new { m.mem_id, m.memberno, m.perno, m.membername }).ToListAsync();
            return rows.GroupBy(r => r.mem_id)
                       .ToDictionary(g => g.Key, g => ((string?)g.First().memberno, (string?)g.First().perno, (string?)g.First().membername));
        }

        public async Task<Dictionary<decimal, (string? Name, int SundryType)>> GetLedgersAsync(IEnumerable<decimal> ledIds, string brCode)
        {
            var ids = ledIds.Where(i => i > 0).Distinct().ToList();
            var rows = await (from l in CSISContext.Fin_Ledger
                              where ids.Contains(l.Led_Id) && !l.Led_Delete && l.BrCode == brCode
                              select new { l.Led_Id, l.Led_Name, l.Led_Sundry_Type }).ToListAsync();
            return rows.GroupBy(r => r.Led_Id)
                       .ToDictionary(g => g.Key, g => ((string?)g.First().Led_Name, g.First().Led_Sundry_Type));
        }

        public async Task<Dictionary<decimal, string?>> GetLoanNumbersAsync(IEnumerable<decimal> loanIds, string brCode)
        {
            var ids = loanIds.Where(i => i > 0).Distinct().ToList();
            if (ids.Count == 0) return new();
            var rows = await (from l in CSISContext.Loan_Master
                              where ids.Contains(l.Loan_Id) && l.BrCode == brCode
                              select new { l.Loan_Id, l.Loan_No }).ToListAsync();
            return rows.ToDictionary(r => r.Loan_Id, r => (string?)r.Loan_No);
        }

        public async Task<Dictionary<decimal, double>> GetPrincipalOutstandingAsync(IEnumerable<decimal> loanIds, string brCode)
        {
            var ids = loanIds.Where(i => i > 0).Distinct().ToList();
            if (ids.Count == 0) return new();
            // Same formula as the sanction's recoverables (EcsLoanSanctionRepository.GetLoanBalancesAsync):
            // San_Amt − Σ prlcoll_amt.
            var rows = await (from m in CSISContext.Loan_Master
                              join t in CSISContext.Loan_Trn on m.Loan_Id equals t.Loan_Id
                              where ids.Contains(m.Loan_Id) && !t.TrnTr_Delete && m.BrCode == brCode && t.BrCode == brCode
                              group new { m, t } by new { m.Loan_Id, m.San_Amt } into g
                              select new { LoanId = g.Key.Loan_Id, Os = g.Key.San_Amt - g.Sum(x => x.t.PrlColl_Amt) })
                             .ToListAsync();
            return rows.ToDictionary(r => r.LoanId, r => r.Os);
        }

        public async Task<Dictionary<int, decimal>> GetDepositLedgersAsync(string brCode)
        {
            var rows = await (from dm in CSISContext.Deposit_Masters
                              where dm.BrCode == brCode
                              select new { dm.Deposit_Id, dm.Led_Id }).ToListAsync();
            return rows.GroupBy(r => r.Deposit_Id).ToDictionary(g => g.Key, g => g.First().Led_Id);
        }

        public async Task<decimal> GetShareCapitalLedgerAsync(string brCode)
        {
            return await (from g in CSISContext.Map_General
                          where g.BrCode == brCode
                          select (decimal?)g.ShareCapital_Led_Id).FirstOrDefaultAsync() ?? 0;
        }

        public async Task<bool> IsChequeNoUsedAsync(decimal bankLedId, string chequeNo, string brCode)
        {
            var no = chequeNo.Trim();
            return await (from b in CSISContext.Fin_Voucher_Bank
                          join v in CSISContext.Fin_Voucher on b.Voc_Id equals v.Voc_Id
                          where b.Led_Id == bankLedId && b.Fvb_Cheque_No == no && !b.Fvb_Delete
                                && b.Fvb_Bank_Mode == "C" && b.BrCode == brCode
                                && !v.Voc_Delete && v.Voc_Status == "V" && v.BrCode == brCode
                          select b.Fvb_Id).AnyAsync();
        }

        public async Task<(int rptSlNo, int pmtSlNo)> GetNextVoucherSlNosAsync(decimal yrId, string brCode)
        {
            // Adjustment/cheque receipt numbers (AR/n) and payment numbers (VR/n), per year AND branch.
            var rpt = await (from v in CSISContext.Fin_Voucher
                             where v.Yr_Id == yrId && v.BrCode == brCode
                                   && (v.Voc_Rpt_Mode == "AR" || v.Voc_Rpt_Mode == "QR")
                             select (int?)v.Voc_Rpt_SlNo).MaxAsync() ?? 0;
            var pmt = await (from v in CSISContext.Fin_Voucher
                             where v.Yr_Id == yrId && v.BrCode == brCode
                                   && (v.Voc_Pmt_Mode == "VR" || v.Voc_Pmt_Mode == "PQ")
                             select (int?)v.Voc_Pmt_SlNo).MaxAsync() ?? 0;
            return (rpt + 1, pmt + 1);
        }

        public async Task<decimal?> GetOldestUnpaidDepositDemandIdAsync(decimal memId, int depositId, string brCode)
        {
            var rows = await (from t in CSISContext.Deposit_Trn
                              where t.Mem_Id == memId && t.Deposit_Id == depositId && t.Is_Active && t.BrCode == brCode
                              group t by t.Demand_Id into g
                              select new
                              {
                                  DemandId = g.Key,
                                  FirstDate = g.Min(x => x.Trn_Date),
                                  Demand = g.Sum(x => x.Demand_Amount),
                                  Receipt = g.Sum(x => x.Receipt_Amount)
                              }).ToListAsync();
            return rows.Where(r => r.Demand > 0 && r.Demand - r.Receipt > 0.005)
                       .OrderBy(r => r.FirstDate).ThenBy(r => r.DemandId)
                       .Select(r => (decimal?)r.DemandId).FirstOrDefault();
        }

        // =====================================================================
        // Ids
        // =====================================================================

        public async Task<EcsDisbursementIdAllocator> CreateIdAllocatorAsync(string brCode)
        {
            // Ids are branch-prefixed (brcode + 7-digit serial, e.g. 110050006354), so MAX is taken within
            // the branch and never below the branch's zero id (brcode + 0000000).
            decimal zero = decimal.TryParse(brCode + "0000000", out var z) ? z : 0;
            decimal Floor(decimal? v) => Math.Max(v ?? 0, zero);

            return new EcsDisbursementIdAllocator(CSISContext, brCode)
            {
                VocId = Floor(await CSISContext.Fin_Voucher.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Voc_Id)),
                VocTrnId = Floor(await CSISContext.Fin_Voucher_Trn.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Voc_Trn_Id)),
                FvbId = Floor(await CSISContext.Fin_Voucher_Bank.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Fvb_Id)),
                LoanId = Floor(await CSISContext.Loan_Master.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Loan_Id)),
                DisbId = Floor(await CSISContext.Loan_Disb.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Disb_Id)),
                InstId = Floor(await CSISContext.Loan_Inst.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Inst_Id)),
                RoiId = Floor(await CSISContext.Loan_Roi.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Roi_Id)),
                LoanTrnId = Floor(await CSISContext.Loan_Trn.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Trn_Id)),
                MemTrnId = Floor(await CSISContext.Mem_Trn.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Mem_Trn_Id)),
                DepositTrnId = Floor(await CSISContext.Deposit_Trn.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Trn_Id)),
            };
        }

        // =====================================================================
        // Write
        // =====================================================================

        public async Task AddPostingAsync(EcsDisbursementPosting p, string brCode)
        {
            await CSISContext.Fin_Voucher.AddAsync(p.Voucher);
            await CSISContext.Loan_Master.AddRangeAsync(p.Loans);
            await CSISContext.Loan_Disb.AddRangeAsync(p.Disbursements);
            await CSISContext.Loan_Inst.AddRangeAsync(p.Instalments);
            await CSISContext.Loan_Roi.AddRangeAsync(p.Rois);
            await CSISContext.Loan_Trn.AddRangeAsync(p.LoanTrns);
            await CSISContext.Mem_Trn.AddRangeAsync(p.MemTrns);
            await CSISContext.Deposit_Trn.AddRangeAsync(p.DepositTrns);
            await CSISContext.Fin_Voucher_Trn.AddRangeAsync(p.VoucherTrns);
            await CSISContext.Fin_Voucher_Bank.AddRangeAsync(p.BankRows);

            if (p.LoansToClose.Count > 0)
            {
                var close = p.LoansToClose.Distinct().ToList();
                var loans = await (from l in CSISContext.Loan_Master
                                   where close.Contains(l.Loan_Id) && l.BrCode == brCode
                                   select l).ToListAsync();
                foreach (var l in loans)
                {
                    l.IsAccountClosed = true;
                    l.AccountClosedDate = p.Date;
                }
            }

            var sanctionIds = p.SanctionUpdates.Select(u => u.SanctionId).ToList();
            var sanctions = await (from s in CSISContext.Loan_Sanction
                                   where sanctionIds.Contains(s.LoanSanction_Id) && s.BrCode == brCode
                                   select s).ToListAsync();
            var eligIds = p.SanctionUpdates.Select(u => (int)u.LoanEligId).Where(i => i > 0).ToList();
            var eligs = await (from e in CSISContext.Loan_Eligibility
                               where eligIds.Contains(e.LoanElig_Id) && e.BrCode == brCode
                               select e).ToListAsync();

            foreach (var u in p.SanctionUpdates)
            {
                var s = sanctions.FirstOrDefault(x => x.LoanSanction_Id == u.SanctionId);
                if (s == null || s.LoanSanction_Status != "E" || s.LoanSanction_Delete)
                    throw new InvalidOperationException($"Sanction {u.SanctionId} is no longer awaiting disbursement.");
                s.Loan_Id = u.LoanId;
                s.Voc_Id = p.VocId;
                s.LoanSanction_Status = "C";

                var e = eligs.FirstOrDefault(x => x.LoanElig_Id == (int)u.LoanEligId);
                if (e != null)
                {
                    e.Loan_Id = u.LoanId;
                    e.DisbursedAmt = u.LoanAmount;
                }
            }
        }
    }

    /// <summary>
    /// Hands out MAX+1 ids and per-key serials from in-memory counters, so many rows can be added to one
    /// table before a single SaveChanges (the shared repositories re-read MAX and collide on tracked rows).
    /// </summary>
    public class EcsDisbursementIdAllocator
    {
        private readonly CSISContext _ctx;
        private readonly string _brCode;
        private readonly Dictionary<int, long> _loanNo = new();
        private readonly Dictionary<decimal, int> _loanTrnSlNo = new();
        private readonly Dictionary<(decimal, byte, decimal), int> _memTrnSlNo = new();
        private readonly Dictionary<(decimal, int), int> _depositTrnSlNo = new();

        internal EcsDisbursementIdAllocator(CSISContext ctx, string brCode)
        {
            _ctx = ctx;
            _brCode = brCode;
        }

        internal decimal VocId, VocTrnId, FvbId, LoanId, DisbId, InstId, RoiId, LoanTrnId, MemTrnId, DepositTrnId;

        public decimal NextVocId() => ++VocId;
        public decimal NextVocTrnId() => ++VocTrnId;
        public decimal NextFvbId() => ++FvbId;
        public decimal NextLoanId() => ++LoanId;
        public decimal NextDisbId() => ++DisbId;
        public decimal NextInstId() => ++InstId;
        public decimal NextRoiId() => ++RoiId;
        public decimal NextLoanTrnId() => ++LoanTrnId;
        public decimal NextMemTrnId() => ++MemTrnId;
        public decimal NextDepositTrnId() => ++DepositTrnId;

        /// <summary>Same numbering as LoanMasterRepository.GetNewLoanNo (highest numeric loan_no of the scheme + 1,
        /// else brcode + loannostartwith + 0000001), but counted in memory across the run.</summary>
        public async Task<string> NextLoanNoAsync(Loan_Schemes scheme)
        {
            if (!_loanNo.TryGetValue(scheme.Scheme_Id, out var last))
            {
                var nos = await (from l in _ctx.Loan_Master
                                 where l.Scheme_Id == scheme.Scheme_Id && l.BrCode == _brCode
                                 select l.Loan_No).ToListAsync();
                last = nos.Select(n => long.TryParse(n, out var v) ? v : 0).DefaultIfEmpty(0).Max();
                if (last <= 0)
                {
                    if (string.IsNullOrWhiteSpace(scheme.LoanNoStartWith))
                        throw new InvalidOperationException($"Loan scheme {scheme.Scheme_Name} has no 'Loan No Start With'.");
                    var first = (scheme.BrCode ?? _brCode) + scheme.LoanNoStartWith.Trim() + "0000001";
                    if (!long.TryParse(first, out var f))
                        throw new InvalidOperationException($"Loan scheme {scheme.Scheme_Name} has a non-numeric loan no. prefix.");
                    last = f - 1;
                }
            }
            _loanNo[scheme.Scheme_Id] = last + 1;
            return (last + 1).ToString();
        }

        public async Task<int> NextLoanTrnSlNoAsync(decimal loanId)
        {
            if (!_loanTrnSlNo.TryGetValue(loanId, out var last))
                last = await (from t in _ctx.Loan_Trn
                              where t.Loan_Id == loanId && t.BrCode == _brCode
                              select (int?)t.Trn_SlNo).MaxAsync() ?? 0;
            _loanTrnSlNo[loanId] = last + 1;
            return last + 1;
        }

        public async Task<int> NextMemTrnSlNoAsync(decimal memId, byte trnType, decimal ledId)
        {
            var key = (memId, trnType, ledId);
            if (!_memTrnSlNo.TryGetValue(key, out var last))
                last = await (from t in _ctx.Mem_Trn
                              where t.Mem_Id == memId && t.Trn_Type == trnType && t.Led_Id == ledId && t.BrCode == _brCode
                              select (int?)t.Trn_SlNo).MaxAsync() ?? 0;
            _memTrnSlNo[key] = last + 1;
            return last + 1;
        }

        public async Task<int> NextDepositTrnSlNoAsync(decimal memId, int depositId)
        {
            var key = (memId, depositId);
            if (!_depositTrnSlNo.TryGetValue(key, out var last))
                last = await (from t in _ctx.Deposit_Trn
                              where t.Mem_Id == memId && t.Deposit_Id == depositId && t.BrCode == _brCode
                              select (int?)t.Trn_SlNo).MaxAsync() ?? 0;
            _depositTrnSlNo[key] = last + 1;
            return last + 1;
        }
    }
}
