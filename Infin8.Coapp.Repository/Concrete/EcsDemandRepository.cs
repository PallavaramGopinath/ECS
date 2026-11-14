using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;

namespace Infin8.Coapp.Repository
{
    /// <summary>
    /// Queries and change-tracker writes for ECS Demand Calculation (spec 12). Every query filters BrCode on every
    /// alias. Writes never call SaveChanges — EcsDemandHandler does.
    /// </summary>
    public class EcsDemandRepository : Repository<Mem_Demand_Master>, IEcsDemandRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;

        public EcsDemandRepository(DbContext context) : base(context)
        {
        }

        // =====================================================================
        // Calculation inputs
        // =====================================================================

        public async Task<List<EcsDemandMemberRow>> GetMembersAsync(string brCode)
        {
            return await (from m in CSISContext.mem_master
                          where !m.memberdelete && !m.isaccountclosed && m.brcode == brCode
                          orderby m.memberno
                          select new EcsDemandMemberRow
                          {
                              MemId = m.mem_id,
                              MemberNo = m.memberno,
                              MemberName = m.membername,
                              OfficeId = m.office_id,
                              MemberStatus = m.memberstatus,
                              BasicPay = m.basicpay,
                              IsExpired = m.ismemexpired,
                              ExpiredDate = m.expireddate
                          }).AsNoTracking().ToListAsync();
        }

        public async Task<List<decimal>> GetStoppedMemberIdsAsync(DateTime dueDate, string brCode)
        {
            var due = DateOnly.FromDateTime(dueDate.Date);
            return await (from s in CSISContext.Mem_Demand_Stop
                          where s.Demand_Date == due && s.Is_Active && s.BrCode == brCode
                          select s.Mem_Id).Distinct().ToListAsync();
        }

        public async Task<List<EcsDemandLoanRow>> GetOpenLoansAsync(string brCode)
        {
            return await (from m in CSISContext.Loan_Master
                          join s in CSISContext.Loan_Schemes on m.Scheme_Id equals s.Scheme_Id
                          where m.Loan_Type == 1 && !m.Loan_Delete && !m.IsAccountClosed && m.Voc_Status == "V"
                                && m.BrCode == brCode && s.BrCode == brCode
                          select new EcsDemandLoanRow
                          {
                              LoanId = m.Loan_Id,
                              LoanNo = m.Loan_No,
                              MemId = m.Mem_Id,
                              SchemeId = m.Scheme_Id,
                              SchemeName = s.Scheme_Name,
                              InstType = s.Inst_Type,
                              DemFrequency = s.Dem_Frequency,
                              IntApplication = s.Int_Application,
                              PIApplication = s.PI_Application,
                              AdvPrlApplication = s.Adv_Prl_Application,
                              InstAmt = m.Inst_Amt,
                              PrlPrd = m.Prl_Prd,
                              FirstPrlDueDate = m.FirstPrl_DueDate,
                              Roi = m.Roi,
                              Pi = m.Pi
                          }).AsNoTracking().ToListAsync();
        }

        public async Task<List<EcsDemandLoanSumRow>> GetLoanSumsAsync(string brCode, DateTime dueDate)
        {
            var due = dueDate.Date;
            return await (from t in CSISContext.Loan_Trn
                          where !t.TrnTr_Delete && t.Voc_Status == "V" && t.Trn_Date < due && t.BrCode == brCode
                          group t by t.Loan_Id into g
                          select new EcsDemandLoanSumRow
                          {
                              LoanId = g.Key,
                              Disb = g.Sum(x => x.Disb_Amt),
                              PrlDem = g.Sum(x => x.Prl_Dem),
                              PrlColl = g.Sum(x => x.PrlColl_Amt),
                              IntCalc = g.Sum(x => x.IntCalc_Amt),
                              IntColl = g.Sum(x => x.IntColl_Amt),
                              PICalc = g.Sum(x => x.PICalc_Amt),
                              PIColl = g.Sum(x => x.PIColl_Amt)
                          }).ToListAsync();
        }

        public async Task<List<Loan_Trn>> GetLoanTrnRowsAsync(IEnumerable<decimal> loanIds, string brCode, DateTime dueDate)
        {
            var ids = loanIds.Distinct().ToList();
            var due = dueDate.Date;
            return await (from t in CSISContext.Loan_Trn
                          where ids.Contains(t.Loan_Id) && !t.TrnTr_Delete && t.Voc_Status == "V"
                                && t.Trn_Date < due && t.BrCode == brCode
                          orderby t.Loan_Id, t.Trn_Date, t.Trn_SlNo
                          select t).AsNoTracking().ToListAsync();
        }

        public async Task<List<Loan_Roi>> GetRoiRowsAsync(IEnumerable<decimal> loanIds, string brCode, DateTime dueDate)
        {
            var ids = loanIds.Distinct().ToList();
            var due = dueDate.Date;
            return await (from r in CSISContext.Loan_Roi
                          where ids.Contains(r.Loan_Id) && r.Agency == "S" && !r.Loanroi_Delete && r.Voc_Status == "V"
                                && r.Roi_Wef < due && r.BrCode == brCode
                          orderby r.Loan_Id, r.Roi_Wef
                          select r).AsNoTracking().ToListAsync();
        }

        public async Task<List<Deposit_Masters>> GetDepositMastersAsync(string brCode)
        {
            return await (from d in CSISContext.Deposit_Masters
                          where d.Is_Active && d.BrCode == brCode
                          orderby d.Deposit_Id
                          select d).AsNoTracking().ToListAsync();
        }

        public async Task<List<Deposit_Options>> GetDepositOptionsAsync(string brCode, DateTime dueDate)
        {
            var due = dueDate.Date;
            return await (from o in CSISContext.Deposit_Options
                          where o.Is_Active && o.Wef <= due && o.BrCode == brCode
                          orderby o.Mem_Id, o.Deposit_Id, o.Wef, o.Option_Id
                          // Projected: the entity's Voc_Id has no column in deposit_options.
                          select new Deposit_Options
                          {
                              Option_Id = o.Option_Id,
                              Deposit_Id = o.Deposit_Id,
                              Mem_Id = o.Mem_Id,
                              Wef = o.Wef,
                              Option_Amount = o.Option_Amount,
                              Is_Active = o.Is_Active,
                              BrCode = o.BrCode
                          }).ToListAsync();
        }

        public async Task<List<Deposit_Roi_Templates>> GetDepositRoiAsync(string brCode)
        {
            return await (from r in CSISContext.Deposit_Roi_Templates
                          where r.is_Active && r.BrCode == brCode
                          // Projected: the entity's int Usr_Id / Yr_Id overflow on the 12-digit ids stored.
                          select new Deposit_Roi_Templates
                          {
                              Roi_Id = r.Roi_Id,
                              Deposit_Id = r.Deposit_Id,
                              Wef = r.Wef,
                              Interest_Rate = r.Interest_Rate,
                              Penal_Interest_Rate = r.Penal_Interest_Rate,
                              is_Active = r.is_Active,
                              BrCode = r.BrCode
                          }).ToListAsync();
        }

        public async Task<List<EcsDemandDepositSumRow>> GetDepositSumsAsync(string brCode, DateTime dueDate)
        {
            var due = dueDate.Date;
            return await (from t in CSISContext.Deposit_Trn
                          where t.Is_Active && t.Trn_Date < due && t.BrCode == brCode
                          group t by new { t.Mem_Id, t.Deposit_Id } into g
                          select new EcsDemandDepositSumRow
                          {
                              MemId = g.Key.Mem_Id,
                              DepositId = g.Key.Deposit_Id,
                              Demand = g.Sum(x => x.Demand_Amount),
                              Receipt = g.Sum(x => x.Receipt_Amount),
                              PiCalc = g.Sum(x => x.Penal_Interest_Calculated_Amount),
                              PiReceived = g.Sum(x => x.Penal_Interest_Received_Amount)
                          }).ToListAsync();
        }

        public async Task<List<EcsDemandRdRow>> GetOpenRdsAsync(string brCode, DateTime dueDate)
        {
            var due = dueDate.Date;
            var rds = await (from m in CSISContext.TermDeposit_Master
                             join s in CSISContext.TermDeposit_Schemes on m.TDScheme_Id equals s.TDScheme_Id
                             where s.TDSchemeType == "R" && !m.TD_Delete && !m.AccountClosed && m.Voc_Status == "V"
                                   && m.ValueDate < due && m.BrCode == brCode && s.BrCode == brCode
                             select new EcsDemandRdRow
                             {
                                 TdId = m.TD_Id,
                                 TdNo = m.TD_No,
                                 MemId = m.Mem_Id,
                                 SchemeId = m.TDScheme_Id,
                                 ValueDate = m.ValueDate,
                                 PeriodInMonths = m.PeriodInMonths,
                                 Instalment = m.DepositAmount
                             }).AsNoTracking().ToListAsync();
            if (rds.Count == 0) return rds;

            var ids = rds.Select(r => r.TdId).ToList();
            var sums = await (from t in CSISContext.TermDeposit_Trn
                              where ids.Contains(t.TD_Id) && !t.TD_Delete && t.Voc_Status == "V"
                                    && t.Trn_Date < due && t.BrCode == brCode
                              group t by t.TD_Id into g
                              select new
                              {
                                  TdId = g.Key,
                                  Received = g.Sum(x => x.DepositReceiptAmount),
                                  Paid = g.Sum(x => x.DepositPaidAmount),
                                  PenalCalc = g.Sum(x => x.PenalCalculatedAmount),
                                  PenalReceived = g.Sum(x => x.PenalReceivedAmount)
                              }).ToListAsync();

            var schemeIds = rds.Select(r => r.SchemeId).Distinct().ToList();
            var rates = await (from r in CSISContext.TermDeposit_Roi_Template
                               where schemeIds.Contains(r.TDScheme_Id) && !r.TDRoi_Delete && r.Wef <= due && r.BrCode == brCode
                               select r).AsNoTracking().ToListAsync();

            foreach (var rd in rds)
            {
                var s = sums.FirstOrDefault(x => x.TdId == rd.TdId);
                if (s != null)
                {
                    rd.Received = s.Received;
                    rd.Paid = s.Paid;
                    rd.PenalCalc = s.PenalCalc;
                    rd.PenalReceived = s.PenalReceived;
                }
                var schemeRates = rates.Where(r => r.TDScheme_Id == rd.SchemeId).ToList();
                var rate = schemeRates.Where(r => r.PeriodBegin <= rd.PeriodInMonths && rd.PeriodInMonths <= r.PeriodEnd)
                                      .OrderBy(r => r.Wef).LastOrDefault()
                           ?? schemeRates.OrderBy(r => r.Wef).LastOrDefault();
                rd.PenalRate = rate?.PenalRateForRD ?? 0;
            }
            // Nothing withdrawn: an RD with any payment out is not demanded.
            return rds.Where(r => r.Paid <= 0).ToList();
        }

        public async Task<List<EcsDemandDueToRow>> GetDueToSumsAsync(string brCode, DateTime dueDate)
        {
            var due = dueDate.Date;
            return await (from t in CSISContext.Mem_Trn
                          join l in CSISContext.Fin_Ledger on t.Led_Id equals l.Led_Id
                          where t.Trn_Type == 1 && !t.MemTrn_Delete && t.Voc_Status == "V" && t.Trn_Date < due
                                && t.BrCode == brCode && l.BrCode == brCode
                          group t by new { t.Mem_Id, t.Led_Id, l.Led_Name } into g
                          select new EcsDemandDueToRow
                          {
                              MemId = g.Key.Mem_Id,
                              LedId = g.Key.Led_Id,
                              LedName = g.Key.Led_Name,
                              Payments = g.Sum(x => x.Pmt_Amt),
                              Receipts = g.Sum(x => x.Rpt_Amt)
                          }).ToListAsync();
        }

        // =====================================================================
        // Saved demands
        // =====================================================================

        public async Task<List<Mem_Demand_Master>> GetLiveMastersAsync(string brCode, int take)
        {
            return await (from m in CSISContext.Mem_Demand_Master
                          where !m.DemandMaster_Delete && m.Voc_Status == "V" && m.BrCode == brCode
                          orderby m.DemandDate descending, m.Demand_Id descending
                          select m).AsNoTracking().Take(take).ToListAsync();
        }

        public async Task<Mem_Demand_Master?> GetMasterForUpdateAsync(decimal demandId, string brCode)
        {
            return await (from m in CSISContext.Mem_Demand_Master
                          where m.Demand_Id == demandId && !m.DemandMaster_Delete && m.BrCode == brCode
                          select m).FirstOrDefaultAsync();
        }

        public async Task<List<Mem_Demand>> GetLinesAsync(decimal demandId, string brCode, bool forUpdate)
        {
            var q = from d in CSISContext.Mem_Demand
                    where d.Demand_Id == demandId && d.Is_Active && d.BrCode == brCode
                    orderby d.Mem_Id, d.Demand_Type, d.Id
                    select d;
            return forUpdate ? await q.ToListAsync() : await q.AsNoTracking().ToListAsync();
        }

        public async Task<List<Mem_Demand_Status>> GetStatusRowsForUpdateAsync(decimal demandId, string brCode)
        {
            return await (from s in CSISContext.Mem_Demand_Status
                          where s.Demand_Id == demandId && s.is_Active && s.BrCode == brCode
                          select s).ToListAsync();
        }

        public async Task<List<EcsDemandTotalRow>> GetTotalsAsync(IEnumerable<decimal> demandIds, string brCode)
        {
            var ids = demandIds.Distinct().ToList();
            return await (from d in CSISContext.Mem_Demand
                          where ids.Contains(d.Demand_Id) && d.Is_Active && d.BrCode == brCode
                          group d by new { d.Demand_Id, d.Demand_Type } into g
                          select new EcsDemandTotalRow
                          {
                              DemandId = g.Key.Demand_Id,
                              DemandType = g.Key.Demand_Type,
                              Lines = g.Count(),
                              Total = g.Sum(x => x.Total_Demand)
                          }).ToListAsync();
        }

        /// <summary>Display labels keyed "L:loanId", "D:depositId", "R:tdId", "S:ledId".</summary>
        public async Task<Dictionary<string, string>> GetAccountLabelsAsync(IEnumerable<Mem_Demand> lines, string brCode)
        {
            var list = lines.ToList();
            var labels = new Dictionary<string, string>();

            var loanIds = list.Where(l => l.Demand_Type == "L").Select(l => l.Loan_Id).Distinct().ToList();
            if (loanIds.Count > 0)
            {
                var loans = await (from m in CSISContext.Loan_Master
                                   join s in CSISContext.Loan_Schemes on m.Scheme_Id equals s.Scheme_Id
                                   where loanIds.Contains(m.Loan_Id) && m.BrCode == brCode && s.BrCode == brCode
                                   select new { m.Loan_Id, m.Loan_No, s.Scheme_Name }).ToListAsync();
                foreach (var l in loans) labels[$"L:{l.Loan_Id}"] = $"{l.Scheme_Name} {l.Loan_No}".Trim();
            }

            var deposits = await (from d in CSISContext.Deposit_Masters
                                  where d.BrCode == brCode
                                  select new { d.Deposit_Id, d.Deposit_Name }).ToListAsync();
            foreach (var d in deposits) labels[$"D:{d.Deposit_Id}"] = d.Deposit_Name ?? $"Deposit {d.Deposit_Id}";

            var tdIds = list.Where(l => l.Demand_Type == "R").Select(l => l.TD_Id).Distinct().ToList();
            if (tdIds.Count > 0)
            {
                var tds = await (from m in CSISContext.TermDeposit_Master
                                 where tdIds.Contains(m.TD_Id) && m.BrCode == brCode
                                 select new { m.TD_Id, m.TD_No }).ToListAsync();
                foreach (var t in tds) labels[$"R:{t.TD_Id}"] = $"RD {t.TD_No}";
            }

            var ledIds = list.Where(l => l.Demand_Type == "S").Select(l => l.Debtor_Led_Id).Distinct().ToList();
            if (ledIds.Count > 0)
            {
                var leds = await (from l in CSISContext.Fin_Ledger
                                  where ledIds.Contains(l.Led_Id) && l.BrCode == brCode
                                  select new { l.Led_Id, l.Led_Name }).ToListAsync();
                foreach (var l in leds) labels[$"S:{l.Led_Id}"] = l.Led_Name ?? $"Ledger {l.Led_Id}";
            }
            return labels;
        }

        // =====================================================================
        // Posted demands
        // =====================================================================

        public async Task<List<Loan_Trn>> GetPostedLoanTrnsAsync(decimal demandId, string brCode)
        {
            return await (from t in CSISContext.Loan_Trn
                          where t.Demand_Id == demandId && t.Trn_Status == "D" && !t.TrnTr_Delete && t.BrCode == brCode
                          select t).ToListAsync();
        }

        /// <summary>Demand rows only: receipts/payments that carry the demand id (TSIS appropriation) are left alone.</summary>
        public async Task<List<Deposit_Trn>> GetPostedDepositTrnsAsync(decimal demandId, string brCode)
        {
            return await (from t in CSISContext.Deposit_Trn
                          where t.Demand_Id == demandId && t.Is_Active && t.Receipt_Amount == 0 && t.Paid_Amount == 0
                                && t.BrCode == brCode
                          select t).ToListAsync();
        }

        public async Task<List<TermDeposit_Trn>> GetPostedRdTrnsAsync(decimal demandId, string brCode)
        {
            return await (from t in CSISContext.TermDeposit_Trn
                          where t.Demand_Id == demandId && !t.TD_Delete && t.DepositReceiptAmount == 0
                                && t.DepositPaidAmount == 0 && t.BrCode == brCode
                          select t).ToListAsync();
        }

        public async Task<List<string>> GetLaterTransactionsAsync(decimal demandId, string brCode)
        {
            var loans = await (from d in CSISContext.Loan_Trn
                               join t in CSISContext.Loan_Trn on d.Loan_Id equals t.Loan_Id
                               join m in CSISContext.Loan_Master on d.Loan_Id equals m.Loan_Id
                               where d.Demand_Id == demandId && d.Trn_Status == "D" && !d.TrnTr_Delete && d.BrCode == brCode
                                     && t.Trn_SlNo > d.Trn_SlNo && !t.TrnTr_Delete && t.Voc_Status == "V" && t.BrCode == brCode
                                     && m.BrCode == brCode
                               select "Loan " + m.Loan_No).Distinct().ToListAsync();

            var deposits = await (from d in CSISContext.Deposit_Trn
                                  join t in CSISContext.Deposit_Trn
                                      on new { d.Mem_Id, d.Deposit_Id } equals new { t.Mem_Id, t.Deposit_Id }
                                  join m in CSISContext.mem_master on d.Mem_Id equals m.mem_id
                                  join dm in CSISContext.Deposit_Masters on d.Deposit_Id equals dm.Deposit_Id
                                  where d.Demand_Id == demandId && d.Is_Active && d.Receipt_Amount == 0 && d.Paid_Amount == 0
                                        && d.BrCode == brCode
                                        && t.Trn_SlNo > d.Trn_SlNo && t.Is_Active && t.BrCode == brCode
                                        && m.brcode == brCode && dm.BrCode == brCode
                                  select dm.Deposit_Name + " of member " + m.memberno).Distinct().ToListAsync();

            var rds = await (from d in CSISContext.TermDeposit_Trn
                             join t in CSISContext.TermDeposit_Trn on d.TD_Id equals t.TD_Id
                             join m in CSISContext.TermDeposit_Master on d.TD_Id equals m.TD_Id
                             where d.Demand_Id == demandId && !d.TD_Delete && d.DepositReceiptAmount == 0
                                   && d.DepositPaidAmount == 0 && d.BrCode == brCode
                                   && t.Trn_SlNo > d.Trn_SlNo && !t.TD_Delete && t.Voc_Status == "V" && t.BrCode == brCode
                                   && m.BrCode == brCode
                             select "RD " + m.TD_No).Distinct().ToListAsync();

            return loans.Concat(deposits).Concat(rds).ToList();
        }

        // =====================================================================
        // Business day
        // =====================================================================

        public async Task<DateTime?> GetOpenBusinessDayAsync(string brCode)
        {
            return await (from c in CSISContext.Business_Day
                          where c.Calendar_Status == "N" && !c.Calendar_Delete && c.BrCode == brCode
                          orderby c.Calendar_Id
                          select (DateTime?)c.Calendar_Date).FirstOrDefaultAsync();
        }

        public async Task<bool> IsLastBusinessDayOfMonthAsync(DateTime date, string brCode)
        {
            var day = date.Date;
            var monthEnd = new DateTime(day.Year, day.Month, 1).AddMonths(1);
            bool laterWorkingDay = await (from c in CSISContext.Business_Day
                                          where c.Calendar_Date > day && c.Calendar_Date < monthEnd
                                                && c.Calendar_Status != "H" && !c.Calendar_Delete && c.BrCode == brCode
                                          select c.Calendar_Id).AnyAsync();
            return !laterWorkingDay;
        }

        // =====================================================================
        // Writes
        // =====================================================================

        public async Task<EcsDemandIdAllocator> CreateIdAllocatorAsync(string brCode)
        {
            // Ids are branch-prefixed (brcode + 7-digit serial), so MAX is taken within the branch and never
            // below the branch's zero id (brcode + 0000000).
            decimal zero = decimal.TryParse(brCode + "0000000", out var z) ? z : 0;
            decimal Floor(decimal? v) => Math.Max(v ?? 0, zero);

            return new EcsDemandIdAllocator(CSISContext, brCode)
            {
                DemandId = Floor(await CSISContext.Mem_Demand_Master.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Demand_Id)),
                LineId = Floor(await CSISContext.Mem_Demand.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Id)),
                StatusId = Floor(await CSISContext.Mem_Demand_Status.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Id)),
                LoanTrnId = Floor(await CSISContext.Loan_Trn.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Trn_Id)),
                DepositTrnId = Floor(await CSISContext.Deposit_Trn.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.Trn_Id)),
                RdTrnId = Floor(await CSISContext.TermDeposit_Trn.Where(x => x.BrCode == brCode).MaxAsync(x => (decimal?)x.TDTrn_Id)),
            };
        }

        public async Task AddCalculationAsync(Mem_Demand_Master master, IEnumerable<Mem_Demand> lines, IEnumerable<Mem_Demand_Status> statuses)
        {
            await CSISContext.Mem_Demand_Master.AddAsync(master);
            await CSISContext.Mem_Demand.AddRangeAsync(lines);
            await CSISContext.Mem_Demand_Status.AddRangeAsync(statuses);
        }

        public async Task AddPostingAsync(IEnumerable<Loan_Trn> loanTrns, IEnumerable<Deposit_Trn> depositTrns, IEnumerable<TermDeposit_Trn> rdTrns)
        {
            await CSISContext.Loan_Trn.AddRangeAsync(loanTrns);
            await CSISContext.Deposit_Trn.AddRangeAsync(depositTrns);
            await CSISContext.TermDeposit_Trn.AddRangeAsync(rdTrns);
        }
    }

    /// <summary>
    /// MAX+1 ids and per-account Trn_SlNo counters held in memory for one demand run, so thousands of rows can be
    /// added before a single SaveChanges (spec 10 §13.2). Serials are primed in bulk to avoid a query per account.
    /// </summary>
    public class EcsDemandIdAllocator
    {
        private readonly CSISContext _ctx;
        private readonly string _brCode;
        private readonly Dictionary<decimal, int> _loanSlNo = new();
        private readonly Dictionary<(decimal, int), int> _depositSlNo = new();
        private readonly Dictionary<decimal, int> _rdSlNo = new();

        internal EcsDemandIdAllocator(CSISContext ctx, string brCode)
        {
            _ctx = ctx;
            _brCode = brCode;
        }

        internal decimal DemandId, LineId, StatusId, LoanTrnId, DepositTrnId, RdTrnId;

        public decimal ZeroId => decimal.TryParse(_brCode + "0000000", out var z) ? z : 0;
        public decimal NextDemandId() => ++DemandId;
        public decimal NextLineId() => ++LineId;
        public decimal NextStatusId() => ++StatusId;
        public decimal NextLoanTrnId() => ++LoanTrnId;
        public decimal NextDepositTrnId() => ++DepositTrnId;
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
                var rows = await (from t in _ctx.Deposit_Trn
                                  where mIds.Contains(t.Mem_Id) && t.BrCode == _brCode
                                  group t by new { t.Mem_Id, t.Deposit_Id } into g
                                  select new { g.Key.Mem_Id, g.Key.Deposit_Id, Max = g.Max(x => x.Trn_SlNo) }).ToListAsync();
                foreach (var r in rows) _depositSlNo[(r.Mem_Id, r.Deposit_Id)] = r.Max;
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
        public int NextRdTrnSlNo(decimal tdId) => _rdSlNo[tdId] = _rdSlNo.GetValueOrDefault(tdId) + 1;
    }
}
