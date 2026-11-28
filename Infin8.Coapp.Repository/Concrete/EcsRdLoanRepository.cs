using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.EntityFrameworkCore;

namespace Infin8.Coapp.Repository
{
    /// <summary>
    /// Queries for the ECS Loan on Recurring Deposit component, acc 14 (spec 17). Every query filters BrCode on every alias.
    /// Nothing here calls SaveChanges — TransactionsHandler.SaveTransaction owns the transaction.
    /// </summary>
    public class EcsRdLoanRepository : Repository<Loan_Master>, IEcsRdLoanRepository
    {
        public const int RdLoanAccountId = 14;
        public const int RdLoanType = 4;     // LoanCategory.LoansOnRecurringDeposits
        public const int RdEligibilityType = 2;

        public CSISContext CSISContext => (CSISContext)Context;

        public EcsRdLoanRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<Loan_Schemes>> GetRdLoanSchemesAsync(string brCode)
        {
            return await (from s in CSISContext.Loan_Schemes
                          where s.Loan_Type == RdLoanType && !s.Scheme_Delete && s.Voc_Status == "V" && s.BrCode == brCode
                          orderby s.Scheme_Id
                          select s).AsNoTracking().ToListAsync();
        }

        public async Task<double?> GetEligibilityPercentAsync(string brCode, DateTime asOn)
        {
            var day = asOn.Date.AddDays(1);
            return await (from e in CSISContext.TermDeposit_LoanEligibleTemplate
                          where e.TD_Type == RdEligibilityType && !e.TD_Delete && e.BrCode == brCode
                                && e.Wef_Date != null && e.Wef_Date < day
                          orderby e.Wef_Date descending, e.TDLoanelig_Id descending
                          select (double?)e.Elig_Per).FirstOrDefaultAsync();
        }

        public async Task<double?> GetRoiMarginAsync(long schemeId, string brCode, DateTime asOn)
        {
            var day = asOn.Date.AddDays(1);
            return await (from r in CSISContext.Loan_Roi_Template
                          where r.Scheme_Id == schemeId && r.Agency == "S" && !r.RoiTemplate_Delete
                                && r.Wef < day && r.BrCode == brCode
                          orderby r.Wef descending, r.Roi_Id descending
                          select (double?)r.Roi).FirstOrDefaultAsync();
        }

        public async Task<Dictionary<decimal, string?>> GetLienLoanNosAsync(IEnumerable<decimal> tdIds, string brCode)
        {
            var ids = tdIds.Distinct().ToList();
            var result = new Dictionary<decimal, string?>();

            var liened = await (from l in CSISContext.Lien_Trn
                                join m in CSISContext.Loan_Master on l.Loan_Id equals m.Loan_Id
                                where ids.Contains(l.TD_Id) && !l.LienTr_Delete && !l.LienTr_Closed && l.Voc_Status == "V"
                                      && l.BrCode == brCode && m.BrCode == brCode
                                select new { l.TD_Id, m.Loan_No }).AsNoTracking().ToListAsync();
            foreach (var r in liened) result[r.TD_Id] = r.Loan_No?.Trim();

            var marked = await (from t in CSISContext.TermDeposit_Master
                                where ids.Contains(t.TD_Id) && t.LienMarked && t.BrCode == brCode
                                select t.TD_Id).ToListAsync();
            foreach (var id in marked) result.TryAdd(id, null);
            return result;
        }

        public async Task<double> GetExistingOutstandingAsync(IEnumerable<decimal> tdIds, string brCode)
        {
            var ids = tdIds.Distinct().ToList();
            var loanIds = await (from l in CSISContext.Lien_Trn
                                 join m in CSISContext.Loan_Master on l.Loan_Id equals m.Loan_Id
                                 where ids.Contains(l.TD_Id) && !l.LienTr_Delete && !l.LienTr_Closed && l.Voc_Status == "V"
                                       && m.Loan_Type == RdLoanType && !m.Loan_Delete && !m.IsAccountClosed && m.Voc_Status == "V"
                                       && l.BrCode == brCode && m.BrCode == brCode
                                 select m.Loan_Id).Distinct().ToListAsync();
            if (loanIds.Count == 0) return 0;

            var sums = await (from t in CSISContext.Loan_Trn
                              where loanIds.Contains(t.Loan_Id) && !t.TrnTr_Delete && t.Voc_Status == "V" && t.BrCode == brCode
                              group t by t.Loan_Id into g
                              select new { Disb = g.Sum(x => x.Disb_Amt), Coll = g.Sum(x => x.PrlColl_Amt) }).ToListAsync();
            return Math.Round(sums.Sum(s => Math.Max(0, s.Disb - s.Coll)), 2, MidpointRounding.AwayFromZero);
        }

        public async Task<List<string>> GetPendingLoanDataAsync(string brCode, decimal excludeStagingId)
        {
            var pending = new[] { "I", "M" };
            var rows = await (from d in CSISContext.Staging_Details
                              join sm in CSISContext.Staging_Master on d.Staging_Id equals sm.Staging_Id
                              where d.Related_Account_Id == RdLoanAccountId
                                    && pending.Contains(d.Staging_Status!) && pending.Contains(sm.Staging_Status!)
                                    && d.Staging_Id != excludeStagingId
                                    && d.BrCode == brCode && sm.BrCode == brCode
                              select d.Related_Account_Data).AsNoTracking().ToListAsync();
            return rows.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!).ToList();
        }

        public async Task<string> PeekNextLoanNoAsync(Loan_Schemes scheme, string brCode)
        {
            var last = await LastLoanNoAsync(CSISContext, scheme, brCode);
            return (last + 1).ToString();
        }

        /// <summary>
        /// §5, the rule of LoanMasterRepository.GetNewLoanNo / EcsDisbursementIdAllocator.NextLoanNoAsync: the highest numeric
        /// loan_no of the scheme in the branch; with none, brcode + LoanNoStartWith + 0000001 less one.
        /// </summary>
        internal static async Task<long> LastLoanNoAsync(CSISContext ctx, Loan_Schemes scheme, string brCode)
        {
            var nos = await (from l in ctx.Loan_Master
                             where l.Scheme_Id == scheme.Scheme_Id && l.BrCode == brCode
                             select l.Loan_No).ToListAsync();
            long last = nos.Select(n => long.TryParse(n?.Trim(), out var v) ? v : 0).DefaultIfEmpty(0).Max();
            if (last > 0) return last;

            if (string.IsNullOrWhiteSpace(scheme.LoanNoStartWith))
                throw new InvalidOperationException($"Loan scheme {scheme.Scheme_Name} has no 'Loan No Start With'.");
            var first = brCode + scheme.LoanNoStartWith.Trim() + "0000001";
            if (!long.TryParse(first, out var f))
                throw new InvalidOperationException($"Loan scheme {scheme.Scheme_Name} has a non-numeric loan no. prefix.");
            return f - 1;
        }

        public async Task<EcsRdLoanIdAllocator> CreateIdAllocatorAsync(string brCode)
        {
            // Ids are branch-prefixed (brcode + 7-digit serial), so MAX is taken within the branch and never below the
            // branch's zero id (brcode + 0000000) — as EcsLoanDisbursementRepository.CreateIdAllocatorAsync does.
            decimal zero = BranchScopedId.Floor(brCode);
            decimal Floor(decimal? v) => Math.Max(v ?? 0, zero);

            return new EcsRdLoanIdAllocator(CSISContext, brCode)
            {
                LoanId = Floor(await CSISContext.Loan_Master.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Loan_Id)),
                DisbId = Floor(await CSISContext.Loan_Disb.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Disb_Id)),
                RoiId = Floor(await CSISContext.Loan_Roi.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Roi_Id)),
                LienId = Floor(await CSISContext.Lien.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Lien_Id)),
                LienTrId = Floor(await CSISContext.Lien_Trn.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.LienTr_Id)),
            };
        }

        public async Task AddLoanAsync(Loan_Master master, Loan_Disb disb, Loan_Roi roi, Lien lien, IEnumerable<Lien_Trn> lienTrns)
        {
            await CSISContext.Loan_Master.AddAsync(master);
            await CSISContext.Loan_Disb.AddAsync(disb);
            await CSISContext.Loan_Roi.AddAsync(roi);
            await CSISContext.Lien.AddAsync(lien);
            await CSISContext.Lien_Trn.AddRangeAsync(lienTrns);
        }

        public async Task MarkLienAsync(IEnumerable<decimal> tdIds, string brCode)
        {
            var ids = tdIds.Distinct().ToList();
            var masters = await (from m in CSISContext.TermDeposit_Master
                                 where ids.Contains(m.TD_Id) && m.BrCode == brCode
                                 select m).ToListAsync();
            foreach (var m in masters) m.LienMarked = true;
        }
    }

    /// <summary>
    /// MAX+1 ids for one RD loan, counted in memory so several lien_trn rows can be added before a single save (the shared
    /// adders re-read MAX and collide on tracked rows — spec 10 §13.2).
    /// </summary>
    public class EcsRdLoanIdAllocator
    {
        private readonly CSISContext _ctx;
        private readonly string _brCode;
        private readonly Dictionary<long, long> _loanNo = new();

        internal EcsRdLoanIdAllocator(CSISContext ctx, string brCode)
        {
            _ctx = ctx;
            _brCode = brCode;
        }

        internal decimal LoanId, DisbId, RoiId, LienId, LienTrId;

        public decimal NextLoanId() => LoanId = BranchScopedId.Next(_brCode, LoanId);
        public decimal NextDisbId() => DisbId = BranchScopedId.Next(_brCode, DisbId);
        public decimal NextRoiId() => RoiId = BranchScopedId.Next(_brCode, RoiId);
        public decimal NextLienId() => LienId = BranchScopedId.Next(_brCode, LienId);
        public decimal NextLienTrId() => LienTrId = BranchScopedId.Next(_brCode, LienTrId);

        /// <summary>§5: allocated at approval.</summary>
        public async Task<string> NextLoanNoAsync(Loan_Schemes scheme)
        {
            if (!_loanNo.TryGetValue(scheme.Scheme_Id, out var last))
                last = await EcsRdLoanRepository.LastLoanNoAsync(_ctx, scheme, _brCode);
            _loanNo[scheme.Scheme_Id] = last + 1;
            return (last + 1).ToString();
        }
    }
}
