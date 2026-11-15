using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;

namespace Infin8.Coapp.Repository
{
    /// <summary>
    /// Queries for the ECS Recurring Deposit Loan Receipt component, acc 25 (spec 18). Every query filters BrCode on every
    /// alias. Nothing here calls SaveChanges — TransactionsHandler.SaveTransaction owns the transaction.
    /// </summary>
    public class EcsRdLoanReceiptRepository : Repository<Loan_Master>, IEcsRdLoanReceiptRepository
    {
        public const int RdLoanReceiptAccountId = 25;
        public const int RdLoanType = 4;     // LoanCategory.LoansOnRecurringDeposits

        public CSISContext CSISContext => (CSISContext)Context;

        public EcsRdLoanReceiptRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<Loan_Master>> GetOpenRdLoansAsync(decimal memId, string brCode)
        {
            return await (from m in CSISContext.Loan_Master
                          where m.Mem_Id == memId && m.Loan_Type == RdLoanType && !m.Loan_Delete && !m.IsAccountClosed
                                && m.Voc_Status == "V" && m.BrCode == brCode
                          orderby m.San_Date, m.Loan_Id
                          select m).AsNoTracking().ToListAsync();
        }

        public async Task<Dictionary<decimal, List<string>>> GetPledgedRdNosAsync(IEnumerable<decimal> loanIds, string brCode)
        {
            var ids = loanIds.Distinct().ToList();
            var rows = await (from l in CSISContext.Lien_Trn
                              join t in CSISContext.TermDeposit_Master on l.TD_Id equals t.TD_Id
                              where ids.Contains(l.Loan_Id) && !l.LienTr_Delete && !l.LienTr_Closed && l.Voc_Status == "V"
                                    && l.BrCode == brCode && t.BrCode == brCode
                              orderby l.Loan_Id, l.LienTr_Id
                              select new { l.Loan_Id, t.TD_No }).AsNoTracking().ToListAsync();
            return rows.GroupBy(r => r.Loan_Id)
                       .ToDictionary(g => g.Key, g => g.Select(r => r.TD_No?.Trim() ?? "").Where(n => n.Length > 0).ToList());
        }

        public async Task<List<string>> GetPendingReceiptDataAsync(string brCode, decimal excludeStagingId)
        {
            var pending = new[] { "I", "M" };
            var rows = await (from d in CSISContext.Staging_Details
                              join sm in CSISContext.Staging_Master on d.Staging_Id equals sm.Staging_Id
                              where d.Related_Account_Id == RdLoanReceiptAccountId
                                    && pending.Contains(d.Staging_Status!) && pending.Contains(sm.Staging_Status!)
                                    && d.Staging_Id != excludeStagingId
                                    && d.BrCode == brCode && sm.BrCode == brCode
                              select d.Related_Account_Data).AsNoTracking().ToListAsync();
            return rows.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!).ToList();
        }

        public async Task CloseLoanAsync(decimal loanId, string brCode, DateTime closedOn)
        {
            var day = closedOn.Date;

            var master = await (from m in CSISContext.Loan_Master
                                where m.Loan_Id == loanId && m.BrCode == brCode
                                select m).FirstOrDefaultAsync();
            if (master != null)
            {
                master.IsAccountClosed = true;
                master.AccountClosedDate = day;
            }

            var liens = await (from l in CSISContext.Lien
                               where l.Loan_Id == loanId && !l.Lien_Delete && !l.Lien_Closed && l.BrCode == brCode
                               select l).ToListAsync();
            foreach (var l in liens)
            {
                l.Lien_Closed = true;
                l.Lien_Recovered = l.Lien_Amount;
            }

            var lienTrns = await (from l in CSISContext.Lien_Trn
                                  where l.Loan_Id == loanId && !l.LienTr_Delete && !l.LienTr_Closed && l.BrCode == brCode
                                  select l).ToListAsync();
            foreach (var l in lienTrns)
            {
                l.LienTr_Closed = true;
                l.LienTr_Recovered = l.LienTr_Amount;
            }

            // Release each pledged RD unless another open loan's live lien still holds it (spec 17 allows one loan per RD,
            // but this is checked anyway).
            var tdIds = lienTrns.Select(l => l.TD_Id).Distinct().ToList();
            if (tdIds.Count == 0) return;

            var stillHeld = await (from l in CSISContext.Lien_Trn
                                   join m in CSISContext.Loan_Master on l.Loan_Id equals m.Loan_Id
                                   where tdIds.Contains(l.TD_Id) && l.Loan_Id != loanId
                                         && !l.LienTr_Delete && !l.LienTr_Closed && l.Voc_Status == "V"
                                         && !m.Loan_Delete && !m.IsAccountClosed
                                         && l.BrCode == brCode && m.BrCode == brCode
                                   select l.TD_Id).Distinct().ToListAsync();

            var release = tdIds.Except(stillHeld).ToList();
            var masters = await (from t in CSISContext.TermDeposit_Master
                                 where release.Contains(t.TD_Id) && t.BrCode == brCode
                                 select t).ToListAsync();
            foreach (var t in masters) t.LienMarked = false;
        }
    }
}
