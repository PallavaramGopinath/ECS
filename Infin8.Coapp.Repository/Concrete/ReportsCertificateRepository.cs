using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;

namespace Infin8.Coapp.Repository
{
    /// <summary>
    /// Certificate report queries, ported from TSIS DatabaseReports.GetCertificate*. BrCode is filtered on every
    /// alias; only valid ('V'), non-deleted rows count. Read-only.
    /// </summary>
    public class ReportsCertificateRepository : Repository<Loan_Master>, IReportsCertificateRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;

        public ReportsCertificateRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<DropdownItem>> GetMemberLoansAsync(decimal memId, string brCode)
        {
            var loans = await (from lm in CSISContext.Loan_Master
                               join ls in CSISContext.Loan_Schemes on lm.Scheme_Id equals ls.Scheme_Id
                               where lm.Mem_Id == memId && !lm.Loan_Delete && lm.Voc_Status == "V"
                                     && lm.BrCode == brCode && ls.BrCode == brCode
                               orderby lm.Loan_No
                               select new { lm.Loan_Id, lm.Loan_No, ls.Scheme_Name }).AsNoTracking().ToListAsync();
            return loans.Select(l => new DropdownItem { Value = l.Loan_Id.ToString(), Text = $"{l.Loan_No} - {l.Scheme_Name}" }).ToList();
        }

        public async Task<rptCertificateLoan?> GetCertificateLoanAsync(decimal loanId, DateTime fromDate, DateTime toDate, string brCode)
        {
            DateTime periodFrom = fromDate.Date, periodTo = toDate.Date;
            return await (from lm in CSISContext.Loan_Master
                          join ls in CSISContext.Loan_Schemes on lm.Scheme_Id equals ls.Scheme_Id
                          join mm in CSISContext.mem_master on lm.Mem_Id equals mm.mem_id
                          where lm.Loan_Id == loanId && !lm.Loan_Delete
                                && lm.BrCode == brCode && ls.BrCode == brCode && mm.brcode == brCode
                          select new rptCertificateLoan
                          {
                              Mem_Id = mm.mem_id,
                              Loan_Id = lm.Loan_Id,
                              Loan_No = lm.Loan_No,
                              Purpose = ls.Scheme_Name,
                              MemberNo = mm.memberno,
                              MemberName = mm.membername,
                              PrlColl_Amt = CSISContext.Loan_Trn
                                  .Where(t => t.Loan_Id == lm.Loan_Id && !t.TrnTr_Delete && t.Voc_Status == "V" && t.BrCode == brCode
                                              && t.Trn_Date >= periodFrom && t.Trn_Date <= periodTo)
                                  .Sum(t => (double?)t.PrlColl_Amt) ?? 0,
                              IntColl_Amt = CSISContext.Loan_Trn
                                  .Where(t => t.Loan_Id == lm.Loan_Id && !t.TrnTr_Delete && t.Voc_Status == "V" && t.BrCode == brCode
                                              && t.Trn_Date >= periodFrom && t.Trn_Date <= periodTo)
                                  .Sum(t => (double?)t.IntColl_Amt) ?? 0
                          }).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<List<rptCertificateFDLoan>> GetCertificateFDLoanAsync(decimal memId, DateTime fromDate, DateTime toDate, string brCode)
        {
            DateTime periodFrom = fromDate.Date, periodTo = toDate.Date;
            var rows = await (from lm in CSISContext.Loan_Master
                              join mm in CSISContext.mem_master on lm.Mem_Id equals mm.mem_id
                              join t in CSISContext.Loan_Trn on lm.Loan_Id equals t.Loan_Id
                              where lm.Mem_Id == memId && lm.Loan_Type == 3 && !lm.Loan_Delete
                                    && !t.TrnTr_Delete && t.Voc_Status == "V" && t.Trn_Date <= periodTo
                                    && lm.BrCode == brCode && mm.brcode == brCode && t.BrCode == brCode
                              group t by new { lm.Loan_Id, lm.Loan_No, lm.San_Date, mm.mem_id, mm.memberno, mm.perno, mm.membername } into g
                              select new rptCertificateFDLoan
                              {
                                  Mem_Id = g.Key.mem_id,
                                  MemberNo = g.Key.memberno,
                                  PerNo = g.Key.perno,
                                  MemberName = g.Key.membername,
                                  Loan_Id = g.Key.Loan_Id,
                                  Loan_No = g.Key.Loan_No,
                                  San_Date = g.Key.San_Date,
                                  Disb_Amt = g.Sum(x => x.Disb_Amt),
                                  Loan_OS = g.Sum(x => x.Disb_Amt) - g.Sum(x => x.PrlColl_Amt),
                                  PrlColl_Amt = g.Where(x => x.Trn_Date >= periodFrom).Sum(x => x.PrlColl_Amt),
                                  IntCalc_Amt = g.Where(x => x.Trn_Date >= periodFrom).Sum(x => x.IntCalc_Amt),
                                  IntColl_Amt = g.Where(x => x.Trn_Date >= periodFrom).Sum(x => x.IntColl_Amt)
                              }).AsNoTracking().ToListAsync();
            return rows.Where(r => r.PrlColl_Amt > 0 || r.IntColl_Amt > 0).OrderBy(r => r.Loan_No).ToList();
        }

        public async Task<List<rptCertificateFDPayment>> GetCertificateFDPaymentAsync(decimal memId, DateTime fromDate, DateTime toDate, string brCode)
        {
            DateTime periodFrom = fromDate.Date, periodTo = toDate.Date;
            var rows = await (from tm in CSISContext.TermDeposit_Master
                              join ts in CSISContext.TermDeposit_Schemes on tm.TDScheme_Id equals ts.TDScheme_Id
                              join mm in CSISContext.mem_master on tm.Mem_Id equals mm.mem_id
                              join tt in CSISContext.TermDeposit_Trn on tm.TD_Id equals tt.TD_Id
                              where tm.Mem_Id == memId && ts.TDSchemeType == "F" && !tt.TD_Delete && tt.Voc_Status == "V"
                                    && tt.Trn_Date >= periodFrom && tt.Trn_Date <= periodTo
                                    && tm.BrCode == brCode && ts.BrCode == brCode && mm.brcode == brCode && tt.BrCode == brCode
                              group tt by new { tm.Mem_Id, mm.memberno, mm.membername, mm.perno, tm.TD_Id, tm.TD_No, tm.DepositAmount, tm.TDH_Name } into g
                              select new rptCertificateFDPayment
                              {
                                  Mem_Id = g.Key.Mem_Id,
                                  MemberNo = g.Key.memberno,
                                  MemberName = g.Key.membername,
                                  PerNo = g.Key.perno,
                                  TD_Id = g.Key.TD_Id,
                                  TD_No = g.Key.TD_No,
                                  DepositAmount = g.Key.DepositAmount,
                                  TDH_Name = g.Key.TDH_Name,
                                  InterestPaidAmount = g.Sum(x => x.InterestPaidAmount),
                                  DepositPaidAmount = g.Sum(x => x.DepositPaidAmount)
                              }).AsNoTracking().ToListAsync();
            return rows.Where(r => r.InterestPaidAmount > 0 || r.DepositPaidAmount > 0).OrderBy(r => r.TD_No).ToList();
        }
    }
}
