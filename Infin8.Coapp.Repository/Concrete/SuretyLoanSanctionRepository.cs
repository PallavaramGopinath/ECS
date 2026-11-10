using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;

namespace Infin8.Coapp.Repository
{
    /// <summary>
    /// Read-only assembly queries for the ECS Surety Loan sanction screen (Spec 03).
    /// All data is sourced from existing tables; this repository performs no writes.
    /// </summary>
    public class SuretyLoanSanctionRepository : Repository<Loan_Sanction>, ISuretyLoanSanctionRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;

        public SuretyLoanSanctionRepository(DbContext context) : base(context)
        {
        }

        public async Task<SuretyLoanSanctionDataVM> GetSuretyLoanMemberAsync(string memberNo, string brCode)
        {
            var data = new SuretyLoanSanctionDataVM();

            // --- Member (Sec 2) ---
            var member = await CSISContext.mem_master
                .Where(m => m.memberno == memberNo && !m.memberdelete && !m.isaccountclosed && m.brcode == brCode)
                .FirstOrDefaultAsync();

            if (member == null)
            {
                data.Found = false;
                data.Message = "Member details not available.";
                return data;
            }

            data.Found = true;
            data.Mem_Id = member.mem_id;
            data.MemberNo = member.memberno;
            data.PerNo = member.perno;
            data.MemberName = member.membername;
            data.BasicPay = member.basicpay;

            // Dates (Sec 4)
            data.DateOfBirth = member.dob;
            data.DateOfRetirement = member.dor;
            data.ServiceInMonths = GetMonthsToRetirement(member.dor);

            // --- Surety (Sec 2) ---
            data.SuretyMem_Id = member.suretymem_id;
            if (member.suretymem_id > 0)
            {
                try
                {
                    var surety = await CSISContext.mem_master
                    .Where(m => m.mem_id == member.suretymem_id && m.brcode == brCode)
                    .Select(m => new { m.memberno, m.membername })
                    .FirstOrDefaultAsync();
                    if (surety != null)
                    {
                        data.SuretyMemberNo = surety.memberno;
                        data.SuretyMemberName = surety.membername;
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message + " Error while fetching surety member data");
                }
                
            }

            // --- Config ledgers (Sec 6/7 assets) ---
            var mapGeneral = await CSISContext.Map_General
                .Where(g => g.BrCode == brCode)
                .FirstOrDefaultAsync();

            if (mapGeneral != null)
            {
                try
                {
                    data.MemberShareCapital = await GetLedgerBalanceAsync(member.mem_id, mapGeneral.ShareCapital_Led_Id, brCode);
                    // Thrift & family-welfare are deposit-master balances (Deposit_Trn), not ledger balances.
                    data.MemberThriftDeposit = await GetDepositBalanceAsync((decimal)member.mem_id, mapGeneral.ThriftDeposit_DMId, brCode);
                    data.MemberFamilyWelfareDeposit = await GetDepositBalanceAsync((decimal)member.mem_id, (int)mapGeneral.FamilyWelfareDepsitDM_Id, brCode);
                    if (member.suretymem_id > 0)
                    {
                        data.SuretyShareCapital = await GetLedgerBalanceAsync(member.suretymem_id, mapGeneral.ShareCapital_Led_Id, brCode);
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine (ex.Message + " Error while fetching ledger/deposit balances");
                }
               
            }

            // --- Salary (Sec 3) — latest non-deleted payslip for the member ---
            var latestPayslip = await CSISContext.Pay_Slip
                .Where(p => p.Mem_Id == member.mem_id && !p.Pay_Delete && p.BrCode == brCode)
                .OrderByDescending(p => p.Pay_Id)
                .FirstOrDefaultAsync();

            if (latestPayslip != null)
            {
                if (latestPayslip.Pay_Basic > 0) data.BasicPay = latestPayslip.Pay_Basic;
                data.GrossPay = latestPayslip.Pay_Tot_Allowance;
                data.TotalDeductions = latestPayslip.Pay_Tot_Deductions;
                data.NetPay = latestPayslip.Pay_Tot_Allowance - latestPayslip.Pay_Tot_Deductions;
                // Society Deduction = loan-recovery (Ded_Type 3) + society deposit/suspense (Ded_Type 5) lines.
                data.SocietyDeduction = await GetSocietyDeductionAsync(member.mem_id, latestPayslip.Pay_Id, brCode);
            }

            // Surety loan overdue & history (Sec 8) is computed with the deductables/eligibility work in Phase 2.
            return data;
        }

        public async Task<SuretyLoanSchemeDefaultsVM> GetSchemeDefaultsAsync(int schemeId, string brCode)
        {
            var defaults = new SuretyLoanSchemeDefaultsVM { Scheme_Id = schemeId };

            var scheme = await CSISContext.Loan_Schemes
                .Where(s => s.Scheme_Id == schemeId && s.BrCode == brCode && !s.Scheme_Delete)
                .FirstOrDefaultAsync();

            if (scheme != null)
            {
                defaults.Scheme_Name = scheme.Scheme_Name;
                defaults.PeriodOfLoan = scheme.MaximumPrincipalPeriod;
                defaults.ThisLoanLimit = scheme.MaximumLoanAmount;
                defaults.InstType = scheme.Inst_Type;
                defaults.AdoptLoanEligibility = scheme.AdoptLoanEligibility != 0;

                // Loan Scheme Details grid (Caption/Content) — WinForms wording.
                defaults.SchemeDetails = new List<SchemeDetailRowVM>
                {
                    new() { Caption = "Principal Demand Frequency", Content = FreqLabel(scheme.Dem_Frequency) },
                    new() { Caption = "Interest Frequency",         Content = FreqLabel(scheme.Int_Frequency) },
                    new() { Caption = "Penal Interest Applied",     Content = PiLabel(scheme.PI_Application) },
                    new() { Caption = "EMI Interest Applied",       Content = EmiLabel(scheme.IOD_Application) },
                    new() { Caption = "Advance Principal",          Content = AdvPrlLabel(scheme.Adv_Prl_Application) },
                    new() { Caption = "Match Share Capital",        Content = MatchScLabel(scheme.MatchShareCapital) },
                    new() { Caption = "Adopt Maximum Loan",         Content = YesNoLabel(scheme.AdoptLoanLimit) },
                    new() { Caption = "Is Surety Member Required",  Content = SuretyLabel(scheme.IssurityMemberRequired) },
                };
            }

            var mapGeneral = await CSISContext.Map_General
                .Where(g => g.BrCode == brCode)
                .Select(g => new { g.MaxLoanLimit })
                .FirstOrDefaultAsync();
            if (mapGeneral != null) defaults.MaxLoanLimit = mapGeneral.MaxLoanLimit;

            // RateOfInterest / PenalRate come from the ROI template (Loan_Roi_Template); wired in Phase 2.
            // NoOfInstalmentsCompleted / Recovered come from the member's loan history; wired in Phase 2.
            return defaults;
        }

        public async Task<SuretyDeductionsDataVM> GetDeductionsDataAsync(decimal memId, decimal suretyMemId,
            int schemeId, double loanAmount, string brCode)
        {
            var result = new SuretyDeductionsDataVM();

            var mapGeneral = await CSISContext.Map_General.Where(g => g.BrCode == brCode).FirstOrDefaultAsync();
            double scPct = mapGeneral?.ShareCapitalPercentageOnLoanOS ?? 0;

            // Schemes whose loans are deductable from the selected scheme (Sec 9.2).
            var deductableSchemeIds = await CSISContext.Loan_Scheme_Deduction_Rules
                .Where(d => d.Scheme_Id == schemeId && d.Is_Active && d.BrCode == brCode)
                .Select(d => d.Scheme_Id)
                .ToListAsync();

            // Member's outstanding loans (mirrors DashboardMemberRepository.GetMemberDashBoardLoans).
            var outstandingLoans = await (
                from master in CSISContext.Loan_Master
                join trn in CSISContext.Loan_Trn on master.Loan_Id equals trn.Loan_Id
                join scheme in CSISContext.Loan_Schemes on master.Scheme_Id equals scheme.Scheme_Id
                where master.Mem_Id == memId && !master.Loan_Delete && !trn.TrnTr_Delete
                      && master.BrCode == brCode && trn.BrCode == brCode && scheme.BrCode == brCode
                group new { master, trn, scheme } by new
                {
                    master.Loan_Id,
                    master.Loan_No,
                    master.Scheme_Id,
                    scheme.Scheme_Name,
                    scheme.PrlLed_Id,
                    master.San_Amt
                } into g
                select new
                {
                    g.Key.Loan_Id,
                    g.Key.Loan_No,
                    g.Key.Scheme_Id,
                    g.Key.Scheme_Name,
                    g.Key.PrlLed_Id,
                    PrlOS = g.Key.San_Amt - g.Sum(x => x.trn.PrlColl_Amt),
                    PrlOD = (g.Sum(x => x.trn.Prl_Dem) - g.Sum(x => x.trn.PrlColl_Amt)) < 0
                            ? 0 : (g.Sum(x => x.trn.Prl_Dem) - g.Sum(x => x.trn.PrlColl_Amt)),
                    IntOS = g.Sum(x => x.trn.IntCalc_Amt) - g.Sum(x => x.trn.IntColl_Amt),
                    PIBal = g.Sum(x => x.trn.PICalc_Amt) - g.Sum(x => x.trn.PIColl_Amt)
                }).ToListAsync();

            // Only loans with positive principal outstanding are relevant.
            var liveLoans = outstandingLoans.Where(l => l.PrlOS > 0).ToList();

            // Ledger names for the principal ledgers referenced.
            var ledgerIds = liveLoans.Select(l => l.PrlLed_Id).Distinct().ToList();
            var ledgerNames = await CSISContext.Fin_Ledger
                .Where(l => ledgerIds.Contains(l.Led_Id) && l.BrCode == brCode)
                .ToDictionaryAsync(l => l.Led_Id, l => l.Led_Name);

            foreach (var loan in liveLoans)
            {
                bool isDeductable = deductableSchemeIds.Contains(loan.Scheme_Id);
                if (isDeductable)
                {
                    result.DeductableLoans.Add(new DeductableLoanRowVM
                    {
                        Loan_Id = loan.Loan_Id,
                        Loan_No = loan.Loan_No,
                        Mem_Id = memId,
                        Scheme_Id = loan.Scheme_Id,
                        Led_Id = loan.PrlLed_Id,
                        Ledger_Name = ledgerNames.TryGetValue(loan.PrlLed_Id, out var n) ? n : loan.Scheme_Name,
                        Outstanding = loan.PrlOS,
                        ODAmount = loan.PrlOD,
                        InterestBalance = loan.IntOS,
                        LDeduct = loan.PrlOS,   // default recoverable = outstanding (owner to refine 'L Deduct')
                        RowType = "Loan"
                    });
                }
                else
                {
                    result.NonDeductableLoans.Add(new NonDeductableLoanVM
                    {
                        Loan_Id = loan.Loan_Id,
                        Loan_No = loan.Loan_No,
                        Purpose = loan.Scheme_Name,
                        LoanOutstanding = loan.PrlOS,
                        InterestBalance = loan.IntOS,
                        PenalInterestBalance = loan.PIBal
                    });
                    result.NonDeductableLoanDemand += loan.PrlOD + loan.IntOS;
                }
            }

            // Required share-capital check (Sec 10.1). scPct stored as a percentage (e.g. 5 ⇒ 5%).
            result.ShareCapitalPercentageOnLoan = scPct;
            result.RequiredShareCapital = loanAmount * scPct / 100.0;
            if (mapGeneral != null)
            {
                result.MemberShareCapital = await GetLedgerBalanceAsync(memId, mapGeneral.ShareCapital_Led_Id, brCode);
                if (suretyMemId > 0)
                    result.SuretyShareCapital = await GetLedgerBalanceAsync(suretyMemId, mapGeneral.ShareCapital_Led_Id, brCode);
            }
            result.MemberShareCapitalShortfall = Math.Max(0, result.RequiredShareCapital - result.MemberShareCapital);
            result.SuretyShareCapitalShortfall = Math.Max(0, result.RequiredShareCapital - result.SuretyShareCapital);

            if (result.MemberShareCapitalShortfall > 0)
                result.DeductableLoans.Add(new DeductableLoanRowVM
                {
                    Mem_Id = memId,
                    Ledger_Name = "Member Share Capital (shortfall)",
                    Outstanding = result.MemberShareCapitalShortfall,
                    LDeduct = result.MemberShareCapitalShortfall,
                    MatchSC = true,
                    RowType = "ShareCapital"
                });
            if (result.SuretyShareCapitalShortfall > 0)
                result.DeductableLoans.Add(new DeductableLoanRowVM
                {
                    Mem_Id = suretyMemId,
                    Ledger_Name = "Surety Share Capital (shortfall)",
                    Outstanding = result.SuretyShareCapitalShortfall,
                    LDeduct = result.SuretyShareCapitalShortfall,
                    MatchSC = true,
                    RowType = "ShareCapital"
                });

            // Member sundry debtors (Sec 10.1 step 5) — Mem_Trn "due by" (Trn_Type 2), balance = Σrpt − Σpmt.
            var sundryRows = await (
                from mem in CSISContext.Mem_Trn
                join led in CSISContext.Fin_Ledger on mem.Led_Id equals led.Led_Id
                where mem.Mem_Id == memId && mem.Trn_Type == 2 && !mem.MemTrn_Delete && mem.BrCode == brCode
                group new { mem, led } by new { mem.Led_Id, led.Led_Name } into g
                select new
                {
                    g.Key.Led_Id,
                    g.Key.Led_Name,
                    Balance = g.Sum(x => x.mem.Rpt_Amt) - g.Sum(x => x.mem.Pmt_Amt)
                }).ToListAsync();
            foreach (var s in sundryRows.Where(s => s.Balance > 0))
            {
                result.DeductableLoans.Add(new DeductableLoanRowVM
                {
                    Mem_Id = memId,
                    Led_Id = s.Led_Id,
                    Ledger_Name = s.Led_Name,
                    Outstanding = s.Balance,
                    LDeduct = s.Balance,
                    RowType = "Sundry"
                });
            }

            // Pending deposits (Sec 11) + current deposit demand (Sec 10.2).
            if (mapGeneral != null)
            {
                var thrift = await GetDepositPendingAsync((decimal)memId, mapGeneral.ThriftDeposit_DMId, brCode);
                var fwd = await GetDepositPendingAsync((decimal)memId, (int)mapGeneral.FamilyWelfareDepsitDM_Id, brCode);
                result.PendingDeposits.Add(new PendingDepositVM { DepositName = "Thrift Deposit", Balance = thrift.balance, PendingDemand = thrift.pending });
                result.PendingDeposits.Add(new PendingDepositVM { DepositName = "Family Welfare Deposit", Balance = fwd.balance, PendingDemand = fwd.pending });
                result.CurrentDepositDemand = thrift.pending + fwd.pending;
            }

            return result;
        }

        public async Task<(decimal sanctionId, int eligId)> PersistSanctionAsync(
            Loan_Eligibility eligibility, Loan_Sanction sanction, List<Loan_Sanction_Trn> trnList)
        {
            int eligMax = await CSISContext.Loan_Eligibility.AnyAsync()
                ? await CSISContext.Loan_Eligibility.MaxAsync(x => x.LoanElig_Id) : 0;
            eligibility.LoanElig_Id = eligMax + 1;

            decimal sancMax = await CSISContext.Loan_Sanction.AnyAsync()
                ? await CSISContext.Loan_Sanction.MaxAsync(x => x.LoanSanction_Id) : 0;
            sanction.LoanSanction_Id = sancMax + 1;
            sanction.LoanElig_Id = eligibility.LoanElig_Id;

            decimal trnMax = await CSISContext.Loan_Sanction_Trn.AnyAsync()
                ? await CSISContext.Loan_Sanction_Trn.MaxAsync(x => x.LoanSanctionTr_Id) : 0;
            int slNo = 1;
            foreach (var t in trnList)
            {
                trnMax++;
                t.LoanSanctionTr_Id = trnMax;
                t.LoanSanction_Id = sanction.LoanSanction_Id;
                t.LoanElig_Id = eligibility.LoanElig_Id;
                t.SlNo = slNo++;
            }

            await CSISContext.Loan_Eligibility.AddAsync(eligibility);
            await CSISContext.Loan_Sanction.AddAsync(sanction);
            if (trnList.Count > 0) await CSISContext.Loan_Sanction_Trn.AddRangeAsync(trnList);
            // Single SaveChanges => atomic (EF wraps it in a transaction).
            await CSISContext.SaveChangesAsync();

            return (sanction.LoanSanction_Id, eligibility.LoanElig_Id);
        }

        public async Task<DateTime?> GetLastSanctionDateAsync(decimal memId, int schemeId, string brCode)
        {
            return await CSISContext.Loan_Sanction
                .Where(s => s.Mem_Id == memId && s.Scheme_Id == schemeId && !s.LoanSanction_Delete && s.BrCode == brCode)
                .OrderByDescending(s => s.SanctionDate)
                .Select(s => s.SanctionDate)
                .FirstOrDefaultAsync();
        }

        /// <summary>Society deduction = Σ payslip deduction lines with Ded_Type 3 (loan recovery) or 5 (society deposit/suspense).</summary>
        private async Task<double> GetSocietyDeductionAsync(decimal memId, decimal payId, string brCode)
        {
            return await CSISContext.Pay_Slip_Trn
                .Where(t => t.Pay_Id == payId && t.Mem_Id == memId && !t.PayTr_Delete
                            && (t.Ded_Type == 3 || t.Ded_Type == 5))
                .SumAsync(t => (double?)t.Deduction_Amt) ?? 0;
        }

        /// <summary>Deposit balance for a member = Σ receipts − Σ payments for a deposit-master id (Deposit_Trn).</summary>
        private async Task<double> GetDepositBalanceAsync(decimal memId, int dmId, string brCode)
        {
            if (dmId <= 0) return 0;
            var rows = await CSISContext.Deposit_Trn
                .Where(t => t.Mem_Id == memId && t.Deposit_Id == dmId && t.Is_Active && t.BrCode == brCode)
                .Select(t => new { t.Receipt_Amount, t.Paid_Amount })
                .ToListAsync();
            return rows.Sum(r => r.Receipt_Amount) - rows.Sum(r => r.Paid_Amount);
        }

        /// <summary>Deposit balance + pending demand (Σ demand − Σ receipts) for a deposit-master id.</summary>
        private async Task<(double balance, double pending)> GetDepositPendingAsync(decimal memId, int dmId, string brCode)
        {
            if (dmId <= 0) return (0, 0);
            double balanceOutput =0, pendingOutput = 0; 
            try
            {
                var rows = await CSISContext.Deposit_Trn
                .Where(t => t.Mem_Id == memId && t.Deposit_Id == dmId && t.Is_Active && t.BrCode == brCode)
                .Select(t => new { t.Receipt_Amount, t.Paid_Amount, t.Demand_Amount })
                .ToListAsync();
                double balance = rows.Sum(r => r.Receipt_Amount) - rows.Sum(r => r.Paid_Amount);
                double pending = rows.Sum(r => r.Demand_Amount) - rows.Sum(r => r.Receipt_Amount);
                balanceOutput = balance;
                pendingOutput = pending;
            }
            catch (Exception ex)
            {

                Console.WriteLine (ex.Message + " Error in fetching deposit pending for memId: " + memId + ", dmId: " + dmId + ", brCode: " + brCode);
            }
            //return (balance, pending < 0 ? 0 : pending);
            return (balanceOutput, pendingOutput < 0 ? 0 : pendingOutput);
        }

        /// <summary>Member balance for a ledger = Σ receipts − Σ payments (excludes soft-deleted rows).</summary>
        private async Task<double> GetLedgerBalanceAsync(decimal memId, decimal ledId, string brCode)
        {
            if (ledId <= 0) return 0;
            var rows = await CSISContext.Mem_Trn
                .Where(t => t.Mem_Id == memId && t.Led_Id == ledId && t.BrCode == brCode && !t.MemTrn_Delete)
                .Select(t => new { t.Rpt_Amt, t.Pmt_Amt })
                .ToListAsync();
            return rows.Sum(r => r.Rpt_Amt) - rows.Sum(r => r.Pmt_Amt);
        }

        // ----- Loan Scheme Details label maps (WinForms wording) -----
        private static string FreqLabel(int v) => v switch
        {
            1 => "Monthly", 2 => "Quarterly", 3 => "Half-Yearly", 4 => "Yearly", 5 => "On Due Date", _ => "-"
        };

        private static string PiLabel(int v) => v switch
        {
            1 => "No Penal Applied", 2 => "On Principal Overdue", 3 => "On Principal and Interest Overdues", _ => "-"
        };

        private static string EmiLabel(int v) => v switch
        {
            1 => "EMI not Applied", 2 => "On Principal Overdue", 3 => "On Principal and Interest Overdues", _ => "-"
        };

        private static string AdvPrlLabel(int v) => v switch
        {
            1 => "Raise Prl Demand", 2 => "Adjust Advance Principal", _ => "-"
        };

        private static string MatchScLabel(int v) => v switch
        {
            1 => "Member only", 2 => "Surety only", 3 => "Match for both Members", _ => "-"
        };

        private static string YesNoLabel(int v) => v == 1 ? "Yes" : "No";

        private static string SuretyLabel(int v) => v switch
        {
            1 => "Yes", 2 => "No", _ => "-"
        };

        // Service in Months = whole months from today (sanction date) to Date of Retirement.
        private static int GetMonthsToRetirement(DateTime? dor)
        {
            if (dor == null) return 0;
            var d = dor.Value;
            int months = (d.Year - DateTime.Today.Year) * 12 + d.Month - DateTime.Today.Month;
            if (d.Day < DateTime.Today.Day) months--;
            return months < 0 ? 0 : months;
        }
    }
}
