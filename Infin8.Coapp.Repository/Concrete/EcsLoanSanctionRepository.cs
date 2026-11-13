using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.EntityFrameworkCore;

namespace Infin8.Coapp.Repository
{
    /// <summary>
    /// Queries and change-tracker writes for the ECS Loan Sanction page (spec 06 §7.1).
    /// Every query filters BrCode on every join alias. Writes never call SaveChanges.
    /// </summary>
    public class EcsLoanSanctionRepository : Repository<Loan_Sanction>, IEcsLoanSanctionRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;

        public EcsLoanSanctionRepository(DbContext context) : base(context)
        {
        }

        // =====================================================================
        // Lookups
        // =====================================================================

        public async Task<List<DropdownItem>> GetSchemeItemsAsync(IEnumerable<int> loanTypes, string brCode)
        {
            var types = loanTypes.ToList();
            return await (from s in CSISContext.Loan_Schemes
                          where types.Contains(s.Loan_Type) && !s.Scheme_Delete && s.BrCode == brCode
                          orderby s.Scheme_Name
                          select new DropdownItem { Value = s.Scheme_Id.ToString(), Text = s.Scheme_Name })
                         .ToListAsync();
        }

        public async Task<SuretyLoanSanctionDataVM> GetMemberAsync(string memberNo, DateTime asOn, string brCode)
        {
            var data = new SuretyLoanSanctionDataVM();
            var map = await (from g in CSISContext.Map_General where g.BrCode == brCode select g).FirstOrDefaultAsync();

            // Map_General.IsMemNo decides which number the user types; fall back to the other one.
            bool byMemNo = map?.IsMemNo ?? true;
            var member = await FindMemberAsync(memberNo, byMemNo, brCode) ?? await FindMemberAsync(memberNo, !byMemNo, brCode);
            if (member == null)
            {
                data.Found = false;
                data.Message = "Member details not available.";
                return data;
            }
            return await BuildMemberDataAsync(member, map, asOn, brCode);
        }

        public async Task<SuretyLoanSanctionDataVM> GetMemberByIdAsync(decimal memId, DateTime asOn, string brCode)
        {
            var map = await (from g in CSISContext.Map_General where g.BrCode == brCode select g).FirstOrDefaultAsync();
            var member = await (from m in CSISContext.mem_master
                                where m.mem_id == memId && !m.memberdelete && m.brcode == brCode
                                select m).FirstOrDefaultAsync();
            if (member == null)
                return new SuretyLoanSanctionDataVM { Found = false, Message = "Member details not available." };
            return await BuildMemberDataAsync(member, map, asOn, brCode);
        }

        /// <summary>
        /// Fills mem_master.dob for a member whose date of birth is missing. Change tracker only; the
        /// handler commits. Returns false when the member is not found or already has a date of birth.
        /// </summary>
        public async Task<bool> SetMemberDobAsync(decimal memId, DateTime dob, string brCode)
        {
            var member = await (from m in CSISContext.mem_master
                                where m.mem_id == memId && !m.memberdelete && m.brcode == brCode
                                select m).FirstOrDefaultAsync();
            if (member == null || member.dob.HasValue) return false;
            member.dob = dob.Date;
            return true;
        }

        private async Task<SuretyLoanSanctionDataVM> BuildMemberDataAsync(mem_master member, Map_General? map,
            DateTime asOn, string brCode)
        {
            var data = new SuretyLoanSanctionDataVM();
            data.Found = true;
            data.Mem_Id = member.mem_id;
            data.MemberNo = member.memberno;
            data.PerNo = member.perno;
            data.MemberName = member.membername;
            data.DateOfBirth = member.dob;

            // Retirement date and service months (§3.6).
            int? perMember = null;
            try
            {
                perMember = await (from r in CSISContext.Mem_RetirementAge
                                   // Compare as decimal: ECS mem_ids (e.g. 110050001301) exceed Int32, so casting
                                   // member.mem_id to int throws OverflowException while EF evaluates the parameter.
                                   where r.Mem_Id == member.mem_id && !r.RetirementAge_Delete && r.BrCode == brCode
                                   select (int?)r.RetirementAge).FirstOrDefaultAsync();
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P01")
            {
                // mem_retirementage is not present in every database; fall back to Map_General (spec §3.6).
            }
            int ageMonths = perMember ?? map?.TSISRetirementAgeInMonths ?? 0;
            // mem_retirementage.retirementage may be stored in years in some databases; a value below 150
            // cannot be a retirement age in months, so treat it as years.
            if (ageMonths > 0 && ageMonths < 150) ageMonths *= 12;

            if (member.dob.HasValue && ageMonths > 0)
                data.DateOfRetirement = Utilities.GetLastDateOfMonth(member.dob.Value.AddMonths(ageMonths));
            else
                data.DateOfRetirement = member.dor;

            if (data.DateOfRetirement.HasValue)
            {
                int toDor = data.DateOfRetirement.Value < asOn ? 0 : Utilities.GetMonthsBetween(asOn, data.DateOfRetirement.Value);
                data.ServiceInMonths = toDor - (map?.MinInstalmentsForLoan ?? 0);
            }

            // Surety
            data.SuretyMem_Id = member.suretymem_id;
            if (member.suretymem_id > 0)
            {
                var surety = await (from m in CSISContext.mem_master
                                    where m.mem_id == member.suretymem_id && !m.memberdelete && m.brcode == brCode
                                    select new { m.memberno, m.membername }).FirstOrDefaultAsync();
                if (surety != null)
                {
                    data.SuretyMemberNo = surety.memberno;
                    data.SuretyMemberName = surety.membername;
                }
            }

            // Assets
            if (map != null)
            {
                data.MemberShareCapital = await GetLedgerBalanceAsync(member.mem_id, map.ShareCapital_Led_Id, brCode);
                data.MemberThriftDeposit = await GetDepositBalanceAsync(member.mem_id, map.ThriftDeposit_DMId, brCode);
                data.MemberFamilyWelfareDeposit = await GetDepositBalanceAsync(member.mem_id, (int)map.FamilyWelfareDepsitDM_Id, brCode);
                if (member.suretymem_id > 0)
                    data.SuretyShareCapital = await GetLedgerBalanceAsync(member.suretymem_id, map.ShareCapital_Led_Id, brCode);
            }
            return data;
        }

        private async Task<mem_master?> FindMemberAsync(string number, bool byMemNo, string brCode)
        {
            return byMemNo
                ? await (from m in CSISContext.mem_master
                         where m.memberno == number && !m.memberdelete && !m.isaccountclosed && m.brcode == brCode
                         select m).FirstOrDefaultAsync()
                : await (from m in CSISContext.mem_master
                         where m.perno == number && !m.memberdelete && !m.isaccountclosed && m.brcode == brCode
                         select m).FirstOrDefaultAsync();
        }

        public async Task<EcsPayDetailsVM> GetPayDetailsAsync(decimal memId, string brCode)
        {
            var result = new EcsPayDetailsVM();
            var slip = await (from p in CSISContext.Pay_Slip
                              where p.Mem_Id == memId && !p.Pay_Delete && p.BrCode == brCode
                              orderby p.Pay_Id descending
                              select p).FirstOrDefaultAsync();
            if (slip == null) return result;

            result.Found = true;
            result.BasicPay = slip.Pay_Basic;
            result.GrossPay = slip.Pay_Tot_Allowance;
            result.TotalDeductions = slip.Pay_Tot_Deductions;
            // Society deduction = loan recovery (Ded_Type 3) + society deposit / suspense (Ded_Type 5).
            result.SocietyDeduction = await (from t in CSISContext.Pay_Slip_Trn
                                             where t.Pay_Id == slip.Pay_Id && t.Mem_Id == memId && !t.PayTr_Delete
                                                   && (t.Ded_Type == 3 || t.Ded_Type == 5) && t.BrCode == brCode
                                             select (double?)t.Deduction_Amt).SumAsync() ?? 0;
            return result;
        }

        public async Task<Loan_Schemes?> GetSchemeAsync(int schemeId, string brCode)
        {
            return await (from s in CSISContext.Loan_Schemes
                          where s.Scheme_Id == schemeId && !s.Scheme_Delete && s.BrCode == brCode
                          select s).FirstOrDefaultAsync();
        }

        public async Task<(double roi, double pi)?> GetRoiAsync(int schemeId, DateTime asOn, string brCode)
        {
            var row = await (from r in CSISContext.Loan_Roi_Template
                             where r.Scheme_Id == schemeId && r.Agency == "S" && r.Wef <= asOn
                                   && !r.RoiTemplate_Delete && r.BrCode == brCode
                             orderby r.Wef descending, r.Roi_Id descending
                             select new { r.Roi, r.Pi }).FirstOrDefaultAsync();
            return row == null ? null : (row.Roi, row.Pi);
        }

        public async Task<List<int>> GetDeductableSchemeIdsAsync(int schemeId, string brCode)
        {
            return await (from d in CSISContext.Loan_Scheme_Deduction_Rules
                          where d.Scheme_Id == schemeId && d.Is_Active && d.BrCode == brCode
                          select d.Deductable_Scheme_Id).Distinct().ToListAsync();
        }

        // =====================================================================
        // Checks
        // =====================================================================

        public async Task<bool> ExistsSanctionOnDateAsync(decimal memId, int schemeId, DateTime date, string[] statuses,
            decimal excludeSanctionId, string brCode)
        {
            var day = date.Date;
            var next = day.AddDays(1);
            return await (from s in CSISContext.Loan_Sanction
                          where s.Mem_Id == memId && s.Scheme_Id == schemeId
                                && s.SanctionDate >= day && s.SanctionDate < next
                                && statuses.Contains(s.LoanSanction_Status!)
                                && !s.LoanSanction_Delete && s.LoanSanction_Id != excludeSanctionId
                                && s.BrCode == brCode
                          select s.LoanSanction_Id).AnyAsync();
        }

        public async Task<DateTime?> GetLastLoanSanctionDateAsync(decimal memId, int schemeId, string brCode)
        {
            return await (from l in CSISContext.Loan_Master
                          where l.Mem_Id == memId && l.Scheme_Id == schemeId && !l.Loan_Delete && l.BrCode == brCode
                          select (DateTime?)l.San_Date).MaxAsync();
        }

        public async Task<List<string>> GetSubsequentTransactionMessagesAsync(decimal sanctionId, string brCode)
        {
            var snapshots = await (from t in CSISContext.Loan_Sanction_Trn
                                   where t.LoanSanction_Id == sanctionId && t.Loan_Id > 0
                                         && !t.LoanSanctionTr_Delete && t.BrCode == brCode
                                   group t by t.Loan_Id into g
                                   select new { Loan_Id = g.Key, MaxSnap = g.Max(x => x.Loan_SlNo) }).ToListAsync();

            var messages = new List<string>();
            foreach (var s in snapshots)
            {
                int maxNow = await (from lt in CSISContext.Loan_Trn
                                    where lt.Loan_Id == s.Loan_Id && !lt.TrnTr_Delete && lt.BrCode == brCode
                                    select (int?)lt.Trn_SlNo).MaxAsync() ?? 0;
                if (maxNow > s.MaxSnap)
                {
                    var loanNo = await (from l in CSISContext.Loan_Master
                                        where l.Loan_Id == s.Loan_Id && l.BrCode == brCode
                                        select l.Loan_No).FirstOrDefaultAsync();
                    messages.Add($"Subsequent transaction made in loan account {loanNo ?? s.Loan_Id.ToString()}");
                }
            }
            return messages;
        }

        // =====================================================================
        // Recoverable sources
        // =====================================================================

        public async Task<List<EcsLoanBalanceVM>> GetLoanBalancesAsync(decimal memId, string brCode)
        {
            var rows = await (
                from master in CSISContext.Loan_Master
                join trn in CSISContext.Loan_Trn on master.Loan_Id equals trn.Loan_Id
                join scheme in CSISContext.Loan_Schemes on master.Scheme_Id equals scheme.Scheme_Id
                where master.Mem_Id == memId && !master.Loan_Delete && !master.IsAccountClosed && !trn.TrnTr_Delete
                      && master.BrCode == brCode && trn.BrCode == brCode && scheme.BrCode == brCode
                group new { master, trn, scheme } by new
                {
                    master.Loan_Id, master.Loan_No, master.Scheme_Id, scheme.Scheme_Name, scheme.Loan_Type,
                    scheme.PrlLed_Id, scheme.IntLed_Id, scheme.PILed_Id,
                    master.San_Amt, master.Roi, master.Inst_Amt,
                    scheme.Int_Application, scheme.AdoptLoanLimit, scheme.MatchShareCapital
                } into g
                select new EcsLoanBalanceVM
                {
                    Loan_Id = g.Key.Loan_Id,
                    Loan_No = g.Key.Loan_No,
                    Scheme_Id = g.Key.Scheme_Id,
                    Scheme_Name = g.Key.Scheme_Name,
                    Loan_Type = g.Key.Loan_Type,
                    PrlLed_Id = g.Key.PrlLed_Id,
                    IntLed_Id = g.Key.IntLed_Id,
                    PILed_Id = g.Key.PILed_Id,
                    San_Amt = g.Key.San_Amt,
                    Roi = g.Key.Roi,
                    Inst_Amt = g.Key.Inst_Amt,
                    Int_Application = g.Key.Int_Application,
                    AdoptLoanLimit = g.Key.AdoptLoanLimit,
                    MatchShareCapital = g.Key.MatchShareCapital,
                    PrlOS = g.Key.San_Amt - g.Sum(x => x.trn.PrlColl_Amt),
                    PrlOD = g.Sum(x => x.trn.Prl_Dem) - g.Sum(x => x.trn.PrlColl_Amt),
                    IntOS = g.Sum(x => x.trn.IntCalc_Amt) - g.Sum(x => x.trn.IntColl_Amt),
                    PIBal = g.Sum(x => x.trn.PICalc_Amt) - g.Sum(x => x.trn.PIColl_Amt),
                    MaxTrnSlNo = g.Max(x => x.trn.Trn_SlNo),
                    DemandsRaised = g.Count(x => x.trn.Prl_Dem > 0),
                    DemandsRecovered = g.Count(x => x.trn.PrlColl_Amt > 0)
                }).ToListAsync();

            foreach (var r in rows)
            {
                if (r.PrlOS < 0) r.PrlOS = 0;
                if (r.PrlOD < 0) r.PrlOD = 0;
                if (r.IntOS < 0) r.IntOS = 0;
                if (r.PIBal < 0) r.PIBal = 0;
            }
            // Keep loans with anything outstanding (principal OR interest — fixes TSIS §4 #5).
            return rows.Where(r => r.PrlOS > 0 || r.IntOS > 0 || r.PIBal > 0).ToList();
        }

        public async Task<Dictionary<decimal, string>> GetLedgerNamesAsync(IEnumerable<decimal> ledIds, string brCode)
        {
            var ids = ledIds.Where(i => i > 0).Distinct().ToList();
            if (ids.Count == 0) return new();
            var rows = await (from l in CSISContext.Fin_Ledger
                              where ids.Contains(l.Led_Id) && l.BrCode == brCode
                              select new { l.Led_Id, l.Led_Name }).ToListAsync();
            return rows.GroupBy(r => r.Led_Id).ToDictionary(g => g.Key, g => g.First().Led_Name ?? "");
        }

        /// <summary>Member balance for a ledger = Σ receipts − Σ payments (share capital, deposits held).</summary>
        public async Task<double> GetLedgerBalanceAsync(decimal memId, decimal ledId, string brCode)
        {
            if (ledId <= 0) return 0;
            return await (from t in CSISContext.Mem_Trn
                          where t.Mem_Id == memId && t.Led_Id == ledId && !t.MemTrn_Delete && t.BrCode == brCode
                          select (double?)(t.Rpt_Amt - t.Pmt_Amt)).SumAsync() ?? 0;
        }

        public async Task<double> GetDepositBalanceAsync(decimal memId, int dmId, string brCode)
        {
            if (dmId <= 0) return 0;
            return await (from t in CSISContext.Deposit_Trn
                          where t.Mem_Id == memId && t.Deposit_Id == dmId && t.Is_Active && t.BrCode == brCode
                          select (double?)(t.Receipt_Amount - t.Paid_Amount)).SumAsync() ?? 0;
        }

        /// <summary>
        /// Due-to rows (§3.8 "DTo"): balances on Sundry-Debtor ledgers (Fin_Ledger.Led_Sundry_Type = 1) that the
        /// member owes the society = Σ payments − Σ receipts.
        /// </summary>
        public async Task<List<DeductableLoanRowVM>> GetDueToRowsAsync(decimal memId, string brCode)
        {
            var rows = await (from mem in CSISContext.Mem_Trn
                              join led in CSISContext.Fin_Ledger on mem.Led_Id equals led.Led_Id
                              where mem.Mem_Id == memId && !mem.MemTrn_Delete && led.Led_Sundry_Type == 1
                                    && mem.BrCode == brCode && led.BrCode == brCode
                              group new { mem, led } by new { mem.Led_Id, led.Led_Name } into g
                              select new
                              {
                                  g.Key.Led_Id,
                                  g.Key.Led_Name,
                                  Balance = g.Sum(x => x.mem.Pmt_Amt) - g.Sum(x => x.mem.Rpt_Amt)
                              }).ToListAsync();

            return rows.Where(r => r.Balance > 0).Select(r => new DeductableLoanRowVM
            {
                Status = "DTo",
                Mem_Id = memId,
                Led_Id = r.Led_Id,
                Ledger_Name = r.Led_Name,
                Outstanding = Math.Round(r.Balance, 2),
                RowType = "DueTo"
            }).ToList();
        }

        public async Task<List<PendingDepositRowVM>> GetPendingDepositsAsync(decimal memId, DateTime asOn, string brCode)
        {
            var rows = await (from dm in CSISContext.Deposit_Masters
                              join t in CSISContext.Deposit_Trn on dm.Deposit_Id equals t.Deposit_Id
                              join led in CSISContext.Fin_Ledger on (decimal)dm.Led_Id equals led.Led_Id
                              where dm.Is_Active && t.Is_Active && t.Mem_Id == memId && t.Trn_Date <= asOn
                                    && dm.BrCode == brCode && t.BrCode == brCode && led.BrCode == brCode
                              group new { dm, t, led } by new { dm.Deposit_Id, dm.Led_Id, led.Led_Name } into g
                              select new
                              {
                                  g.Key.Deposit_Id,
                                  g.Key.Led_Id,
                                  g.Key.Led_Name,
                                  Demand = g.Sum(x => x.t.Demand_Amount),
                                  Receipt = g.Sum(x => x.t.Receipt_Amount),
                                  Paid = g.Sum(x => x.t.Paid_Amount)
                              }).ToListAsync();

            return rows.Where(r => r.Demand - r.Receipt > 0).Select(r => new PendingDepositRowVM
            {
                DMId = r.Deposit_Id,
                MemId = memId,
                LedId = r.Led_Id,
                LedName = r.Led_Name,
                Balance = Math.Round(r.Receipt - r.Paid, 2),
                PendingAmount = Math.Round(r.Demand - r.Receipt, 2),
                RecoveryAmount = Math.Round(r.Demand - r.Receipt, 2)
            }).ToList();
        }

        public async Task<double> GetCurrentDepositDemandAsync(decimal memId, double basicPay, DateTime calcDate,
            DateTime demandDate, string brCode)
        {
            var masters = await (from dm in CSISContext.Deposit_Masters
                                 where dm.Is_Active && dm.BrCode == brCode
                                 select dm).ToListAsync();
            double total = 0;
            foreach (var dm in masters)
            {
                double d = dm.Percentage > 0
                    ? Math.Round(basicPay * dm.Percentage / 100.0, 0, MidpointRounding.AwayFromZero)
                    : dm.Fixed_Amount;
                if (string.Equals(dm.Frequency, "A", StringComparison.OrdinalIgnoreCase) && dm.Demand_Month != demandDate.Month)
                    d = 0;

                var option = await (from o in CSISContext.Deposit_Options
                                    where o.Mem_Id == memId && o.Deposit_Id == dm.Deposit_Id && o.Is_Active
                                          && o.Wef <= calcDate && o.BrCode == brCode
                                    orderby o.Wef descending, o.Option_Id descending
                                    select (double?)o.Option_Amount).FirstOrDefaultAsync();
                if (option.HasValue && option.Value > 0) d = option.Value;
                total += d;
            }
            return total;
        }

        public async Task<(string status, int dmId, string? ledName)> ResolveDeductionStatusAsync(decimal ledId, string brCode)
        {
            var ledger = await (from l in CSISContext.Fin_Ledger
                                where l.Led_Id == ledId && l.BrCode == brCode
                                select new { l.Led_Name, l.Led_Sundry_Type }).FirstOrDefaultAsync();

            var dmId = await (from dm in CSISContext.Deposit_Masters
                              where (decimal)dm.Led_Id == ledId && dm.Is_Active && dm.BrCode == brCode
                              select (int?)dm.Deposit_Id).FirstOrDefaultAsync();
            if (dmId.HasValue) return ("D", dmId.Value, ledger?.Led_Name);
            if (ledger?.Led_Sundry_Type == 1) return ("DTo", 0, ledger.Led_Name);
            if (ledger?.Led_Sundry_Type == 2) return ("DBy", 0, ledger.Led_Name);

            var scLed = await (from g in CSISContext.Map_General where g.BrCode == brCode select (decimal?)g.ShareCapital_Led_Id)
                              .FirstOrDefaultAsync();
            if (scLed.HasValue && scLed.Value == ledId) return ("MS", 0, ledger?.Led_Name);
            return ("G", 0, ledger?.Led_Name);
        }

        // =====================================================================
        // List / load / delete / report
        // =====================================================================

        public async Task<List<EcsSanctionListItemVM>> GetSanctionListAsync(EcsSanctionListFilterVM filter, string brCode)
        {
            var mode = (filter.Mode ?? "ALL").ToUpperInvariant();
            decimal? sanctionId = mode == "ID" ? filter.SanctionId : null;
            string? memberNo = mode == "MEM" ? filter.MemberNo?.Trim() : null;
            DateTime? fromDate = mode == "DATE" ? filter.From?.Date : null;
            DateTime? toExcl = mode == "DATE" && filter.To.HasValue ? filter.To.Value.Date.AddDays(1) : null;

            var query = from s in CSISContext.Loan_Sanction
                        join sc in CSISContext.Loan_Schemes on s.Scheme_Id equals sc.Scheme_Id
                        join m in CSISContext.mem_master on s.Mem_Id equals m.mem_id
                        where s.LoanSanction_Status == "E" && !s.LoanSanction_Delete
                              && s.BrCode == brCode && sc.BrCode == brCode && m.brcode == brCode
                              && (sanctionId == null || s.LoanSanction_Id == sanctionId)
                              && (memberNo == null || m.memberno == memberNo || m.perno == memberNo)
                              && (fromDate == null || s.SanctionDate >= fromDate)
                              && (toExcl == null || s.SanctionDate < toExcl)
                        orderby s.SanctionDate descending, s.LoanSanction_Id descending
                        select new EcsSanctionListItemVM
                        {
                            LoanSanction_Id = s.LoanSanction_Id,
                            Mem_Id = s.Mem_Id,
                            SuretyMem_Id = s.SuretyMem_Id,
                            MemberNo = m.memberno,
                            PerNo = m.perno,
                            MemberName = m.membername,
                            SanctionDate = s.SanctionDate,
                            LoanAmount = s.LoanAmount,
                            Scheme_Id = s.Scheme_Id,
                            SchemeName = sc.Scheme_Name
                        };
            return await query.Take(500).ToListAsync();
        }

        public async Task<Loan_Sanction?> GetSanctionHeaderAsync(decimal sanctionId, string brCode)
        {
            return await (from s in CSISContext.Loan_Sanction
                          where s.LoanSanction_Id == sanctionId && !s.LoanSanction_Delete && s.BrCode == brCode
                          select s).FirstOrDefaultAsync();
        }

        public async Task<List<Loan_Sanction_Trn>> GetSanctionLinesAsync(decimal sanctionId, string brCode)
        {
            return await (from t in CSISContext.Loan_Sanction_Trn
                          where t.LoanSanction_Id == sanctionId && !t.LoanSanctionTr_Delete && t.BrCode == brCode
                          orderby t.SlNo
                          select t).ToListAsync();
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

        public async Task SoftDeleteSanctionAsync(decimal sanctionId, string brCode)
        {
            var header = await (from s in CSISContext.Loan_Sanction
                                where s.LoanSanction_Id == sanctionId && s.BrCode == brCode
                                select s).FirstOrDefaultAsync();
            if (header == null) return;
            header.LoanSanction_Delete = true;

            var lines = await (from t in CSISContext.Loan_Sanction_Trn
                               where t.LoanSanction_Id == sanctionId && t.BrCode == brCode
                               select t).ToListAsync();
            foreach (var l in lines) l.LoanSanctionTr_Delete = true;

            int eligId = (int)header.LoanElig_Id;
            var elig = await (from e in CSISContext.Loan_Eligibility
                              where e.LoanElig_Id == eligId && e.BrCode == brCode
                              select e).FirstOrDefaultAsync();
            if (elig != null) elig.LoanElig_Delete = true;
        }

        public async Task<List<rptOfficeNote>> GetOfficeNoteAsync(decimal sanctionId, string brCode)
        {
            var header = await (from s in CSISContext.Loan_Sanction
                                join m in CSISContext.mem_master on s.Mem_Id equals m.mem_id
                                where s.LoanSanction_Id == sanctionId && !s.LoanSanction_Delete
                                      && s.BrCode == brCode && m.brcode == brCode
                                select new { s, m.memberno, m.perno, m.membername }).FirstOrDefaultAsync();
            if (header == null) return new();

            var surety = await (from m in CSISContext.mem_master
                                where m.mem_id == header.s.SuretyMem_Id && m.brcode == brCode
                                select new { m.memberno, m.perno, m.membername }).FirstOrDefaultAsync();

            var lines = await GetSanctionLinesAsync(sanctionId, brCode);
            var hdr = header.s;
            return lines.Select(t => new rptOfficeNote
            {
                LoanSanction_Id = hdr.LoanSanction_Id,
                SanctionDate = hdr.SanctionDate ?? DateTime.MinValue,
                LoanAmount = hdr.LoanAmount,
                PeriodOfLoan = hdr.PeriodOfLoan,
                InstalmentAmount = hdr.InstalmentAmount,
                TotalReceiptAmount = hdr.TotalReceiptAmount,
                IsMemberBecomeDefaulter = hdr.IsMemberBecomeDefaulter,
                isSurityBecomeDefaulter = hdr.IsSurityBecomeDefaulter,
                GrossPay = (int)Math.Round(hdr.GrossPay, MidpointRounding.AwayFromZero),
                MemberShareCapitalAmount = hdr.MemberShareCapitalAmount,
                SurityShareCapitalAmount = hdr.SuretyShareCapitalAmount,
                NoOfInstalmentsCompleted = hdr.NoOfInstalmentsCompleted,
                NoOfInstalmentsRecovered = hdr.NoOfInstalmentsRecovered,
                MemberNo = header.memberno,
                PerNo = header.perno,
                MemberName = header.membername,
                SuretymemberNo = surety?.memberno,
                SuretyPerNo = surety?.perno,
                SuretymemberName = surety?.membername,
                SlNo = t.SlNo,
                ReceiptDescription = t.ReceiptDescription,
                ReceiptItem = t.ReceiptItem,
                PaymentDescription = t.PaymentDescription,
                // Net Pay (payment line 6) is shown as 0 when negative.
                PaymentItem = t.Payment_Id == 6 && t.PaymentItem < 0 ? 0 : t.PaymentItem
            }).ToList();
        }

        public async Task<(decimal sanctionId, int eligId)> AddSanctionAsync(
            Loan_Eligibility eligibility, Loan_Sanction sanction, List<Loan_Sanction_Trn> trnList)
        {
            int eligMax = await (from e in CSISContext.Loan_Eligibility select (int?)e.LoanElig_Id).MaxAsync() ?? 0;
            eligibility.LoanElig_Id = eligMax + 1;

            decimal sancMax = await (from s in CSISContext.Loan_Sanction select (decimal?)s.LoanSanction_Id).MaxAsync() ?? 0;
            sanction.LoanSanction_Id = sancMax + 1;
            sanction.LoanElig_Id = eligibility.LoanElig_Id;

            decimal trnMax = await (from t in CSISContext.Loan_Sanction_Trn select (decimal?)t.LoanSanctionTr_Id).MaxAsync() ?? 0;
            foreach (var t in trnList)
            {
                t.LoanSanctionTr_Id = ++trnMax;
                t.LoanSanction_Id = sanction.LoanSanction_Id;
                t.LoanElig_Id = eligibility.LoanElig_Id;
            }

            await CSISContext.Loan_Eligibility.AddAsync(eligibility);
            await CSISContext.Loan_Sanction.AddAsync(sanction);
            if (trnList.Count > 0) await CSISContext.Loan_Sanction_Trn.AddRangeAsync(trnList);
            return (sanction.LoanSanction_Id, eligibility.LoanElig_Id);
        }
    }
}
