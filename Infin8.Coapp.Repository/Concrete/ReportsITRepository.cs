using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.EntityFrameworkCore;

namespace Infin8.Coapp.Repository
{
    /// <summary>
    /// Income tax reports, ported from TSIS DatabaseReports.GetITReport*. Each report lists the rows that had a
    /// balance before the period or any movement inside it. BrCode is filtered on every alias; only valid ('V'),
    /// non-deleted rows count. Read-only.
    /// </summary>
    public class ReportsITRepository : Repository<mem_master>, IReportsITRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;

        public ReportsITRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<rptITLoan>> GetITLoansAsync(int loanType, DateTime fromDate, DateTime toDate, string brCode)
        {
            DateTime periodFrom = fromDate.Date, periodEnd = toDate.Date.AddDays(1);
            var rows = await (from t in CSISContext.Loan_Trn
                              join lm in CSISContext.Loan_Master on t.Loan_Id equals lm.Loan_Id
                              join ls in CSISContext.Loan_Schemes on lm.Scheme_Id equals ls.Scheme_Id
                              join mm in CSISContext.mem_master on lm.Mem_Id equals mm.mem_id
                              where lm.Loan_Type == loanType && !lm.Loan_Delete && !t.TrnTr_Delete && t.Voc_Status == "V"
                                    && t.Trn_Date < periodEnd
                                    && t.BrCode == brCode && lm.BrCode == brCode && ls.BrCode == brCode && mm.brcode == brCode
                              group t by new { lm.Loan_Id, lm.Loan_No, lm.San_Date, lm.Roi, ls.Scheme_Name, MemId = mm.mem_id } into g
                              select new
                              {
                                  g.Key,
                                  PrlOB = g.Where(x => x.Trn_Date < periodFrom).Sum(x => x.Disb_Amt - x.PrlColl_Amt),
                                  IntOB = g.Where(x => x.Trn_Date < periodFrom).Sum(x => x.IntCalc_Amt - x.IntColl_Amt),
                                  PIOB = g.Where(x => x.Trn_Date < periodFrom).Sum(x => x.PICalc_Amt - x.PIColl_Amt),
                                  IODOB = g.Where(x => x.Trn_Date < periodFrom).Sum(x => x.IODCalc_Amt - x.IODColl_Amt),
                                  InPeriod = g.Count(x => x.Trn_Date >= periodFrom),
                                  Issued = g.Where(x => x.Trn_Date >= periodFrom).Sum(x => x.Disb_Amt),
                                  PrlColl = g.Where(x => x.Trn_Date >= periodFrom).Sum(x => x.PrlColl_Amt),
                                  IntColl = g.Where(x => x.Trn_Date >= periodFrom).Sum(x => x.IntColl_Amt),
                                  PIColl = g.Where(x => x.Trn_Date >= periodFrom).Sum(x => x.PIColl_Amt),
                                  IODColl = g.Where(x => x.Trn_Date >= periodFrom).Sum(x => x.IODColl_Amt)
                              }).AsNoTracking().ToListAsync();

            var members = await MembersAsync(rows.Select(r => r.Key.MemId), brCode);
            return rows
                .Where(r => r.PrlOB > 0 || r.IntOB > 0 || r.PIOB > 0 || r.IODOB > 0 || r.InPeriod > 0)
                .Select(r =>
                {
                    var item = new rptITLoan
                    {
                        Loan_No = r.Key.Loan_No,
                        Scheme_Name = r.Key.Scheme_Name,
                        Disb_Date = r.Key.San_Date,
                        Roi = r.Key.Roi,
                        Loan_OpeningBalance = r.PrlOB,
                        Loan_Issued = r.Issued,
                        Loan_Collected = r.PrlColl,
                        Loan_ClosingBalance = r.PrlOB + r.Issued - r.PrlColl,
                        Int_Collected = r.IntColl,
                        EMI_Collected = r.IODColl,
                        PI_Collected = r.PIColl,
                        Is_OS_MoreThan3Years = Utilities.GetMonthsBetween(r.Key.San_Date, toDate) >= 36 ? "YES" : "NO"
                    };
                    var m = MemberFields(members[r.Key.MemId]);
                    (item.Mem_Id, item.MemberNo, item.PerNo, item.TokenNo, item.MemberName, item.MemberType, item.Address, item.PANNo, item.AadharNo) = m;
                    return item;
                })
                .OrderBy(x => x.MemberNo).ThenBy(x => x.Loan_No)
                .ToList();
        }

        public async Task<List<rptITShareCapital>> GetITShareCapitalAsync(DateTime fromDate, DateTime toDate, string brCode)
        {
            DateTime periodFrom = fromDate.Date, periodEnd = toDate.Date.AddDays(1);
            const byte shareCapitalTrnType = 3;   // TSIS _trnType = 3
            var rows = await (from mt in CSISContext.Mem_Trn
                              join mm in CSISContext.mem_master on mt.Mem_Id equals mm.mem_id
                              join fl in CSISContext.Fin_Ledger on mt.Led_Id equals fl.Led_Id
                              where mt.Trn_Type == shareCapitalTrnType && !mt.MemTrn_Delete && mt.Voc_Status == "V"
                                    && mt.Trn_Date < periodEnd
                                    && mt.BrCode == brCode && mm.brcode == brCode && fl.BrCode == brCode
                              group mt by new { MemId = mm.mem_id, fl.Led_Id, fl.Led_Name } into g
                              select new
                              {
                                  g.Key,
                                  OB = g.Where(x => x.Trn_Date < periodFrom).Sum(x => x.Rpt_Amt - x.Pmt_Amt),
                                  Rpt = g.Where(x => x.Trn_Date >= periodFrom).Sum(x => x.Rpt_Amt),
                                  Pmt = g.Where(x => x.Trn_Date >= periodFrom).Sum(x => x.Pmt_Amt),
                                  LastTrnDate = g.Max(x => x.Trn_Date)
                              }).AsNoTracking().ToListAsync();

            var members = await MembersAsync(rows.Select(r => r.Key.MemId), brCode);
            return rows
                .Where(r => r.OB > 0 || r.Rpt > 0 || r.Pmt > 0)
                .Select(r =>
                {
                    var item = new rptITShareCapital
                    {
                        Led_Id = r.Key.Led_Id,
                        Led_Name = r.Key.Led_Name,
                        Amt_OB = r.OB,
                        Rpt_Amt = r.Rpt,
                        Pmt_Amt = r.Pmt,
                        Amt_CB = r.OB + r.Rpt - r.Pmt,
                        Trn_Date = r.LastTrnDate
                    };
                    (item.Mem_Id, item.MemberNo, item.PerNo, item.TokenNo, item.MemberName, item.MemberType, item.Address, item.PANNo, item.AadharNo) = MemberFields(members[r.Key.MemId]);
                    return item;
                })
                .OrderBy(x => x.MemberNo).ThenBy(x => x.Led_Name)
                .ToList();
        }

        public async Task<List<rptITMember>> GetITMembersAsync(DateTime asOnDate, string brCode)
        {
            DateTime asOn = asOnDate.Date;
            const int aClassMember = 1;   // TSIS _memberType = 1
            return await (from mm in CSISContext.mem_master
                          where mm.membertype == aClassMember && !mm.memberdelete && !mm.isaccountclosed
                                && (mm.admissiondate == null || mm.admissiondate <= asOn)
                                && mm.brcode == brCode
                          orderby mm.memberno
                          select new rptITMember { MemberNo = mm.memberno, MemberName = mm.membername, MemberType = mm.membertype })
                .AsNoTracking().ToListAsync();
        }

        public async Task<List<rptITFixedDeposit>> GetITFixedDepositsAsync(DateTime fromDate, DateTime toDate, string brCode)
        {
            DateTime periodFrom = fromDate.Date, periodEnd = toDate.Date.AddDays(1);
            var rows = await (from tt in CSISContext.TermDeposit_Trn
                              join tm in CSISContext.TermDeposit_Master on tt.TD_Id equals tm.TD_Id
                              join ts in CSISContext.TermDeposit_Schemes on tm.TDScheme_Id equals ts.TDScheme_Id
                              join mm in CSISContext.mem_master on tm.Mem_Id equals mm.mem_id
                              where ts.TDSchemeType == "F" && !tm.TD_Delete && !tt.TD_Delete && tt.Voc_Status == "V"
                                    && tt.Trn_Date < periodEnd
                                    && tt.BrCode == brCode && tm.BrCode == brCode && ts.BrCode == brCode && mm.brcode == brCode
                              group tt by new { tm.TD_Id, tm.TD_No, ts.TDScheme_Name, tm.RateOfInterest, MemId = mm.mem_id } into g
                              select new
                              {
                                  g.Key,
                                  OB = g.Where(x => x.Trn_Date < periodFrom).Sum(x => x.DepositReceiptAmount - x.DepositPaidAmount),
                                  Received = g.Where(x => x.Trn_Date >= periodFrom).Sum(x => x.DepositReceiptAmount),
                                  Paid = g.Where(x => x.Trn_Date >= periodFrom).Sum(x => x.DepositPaidAmount),
                                  Interest = g.Where(x => x.Trn_Date >= periodFrom).Sum(x => x.InterestPaidAmount)
                              }).AsNoTracking().ToListAsync();

            var members = await MembersAsync(rows.Select(r => r.Key.MemId), brCode);
            return rows
                .Where(r => r.OB > 0 || r.Received > 0 || r.Paid > 0 || r.Interest > 0)
                .Select(r =>
                {
                    var item = new rptITFixedDeposit
                    {
                        TD_Id = r.Key.TD_Id,
                        TD_No = r.Key.TD_No,
                        TDScheme_Name = r.Key.TDScheme_Name,
                        RateOfInterest = r.Key.RateOfInterest,
                        Deposit_OB = r.OB,
                        Deposit_Received = r.Received,
                        Deposit_Paid = r.Paid,
                        Deposit_CB = r.OB + r.Received - r.Paid,
                        Interest_Paid = r.Interest
                    };
                    (item.Mem_Id, item.MemberNo, item.PerNo, item.TokenNo, item.MemberName, item.MemberType, item.Address, item.PANNo, item.AadharNo) = MemberFields(members[r.Key.MemId]);
                    return item;
                })
                .OrderBy(x => x.MemberNo).ThenBy(x => x.TD_No)
                .ToList();
        }

        private async Task<Dictionary<decimal, mem_master>> MembersAsync(IEnumerable<decimal> memIds, string brCode)
        {
            var ids = memIds.Distinct().ToList();
            return await (from mm in CSISContext.mem_master
                          where ids.Contains(mm.mem_id) && mm.brcode == brCode
                          select mm).AsNoTracking().ToDictionaryAsync(m => m.mem_id);
        }

        /// <summary>
        /// Member columns shared by the IT reports. TSIS's Address expression had an operator-precedence bug that
        /// returned only the first line; this joins all four parts.
        /// </summary>
        private static (decimal, string?, string?, string?, string?, string, string, string?, string?) MemberFields(mem_master m)
        {
            string address = string.Join(", ", new[] { m.peradd1, m.peradd2, m.peradd3, m.perpin }
                .Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!.Trim()));
            return (m.mem_id, m.memberno, m.perno, string.IsNullOrWhiteSpace(m.tickettokengangno) ? m.perno : m.tickettokengangno,
                    m.membername, m.membertype == 1 ? "A- Class Member" : "Associate Member", address, m.panno, m.aadharno);
        }
    }
}
