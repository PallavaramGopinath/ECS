using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;

namespace Infin8.Coapp.Repository
{
    /// <summary>
    /// Queries for the ECS Loan Recovery component, acc 58 (spec 11). Every query filters BrCode on every
    /// alias. Nothing here calls SaveChanges — TransactionsHandler.SaveTransaction owns the transaction.
    /// </summary>
    public class EcsLoanRecoveryRepository : Repository<Loan_Master>, IEcsLoanRecoveryRepository
    {
        public const int RecoveryAccountId = 58;

        public CSISContext CSISContext => (CSISContext)Context;

        public EcsLoanRecoveryRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<EcsLoanRecoveryLoanVM>> GetCandidateLoansAsync(decimal memId, string brCode)
        {
            return await (from m in CSISContext.Loan_Master
                          join s in CSISContext.Loan_Schemes on m.Scheme_Id equals s.Scheme_Id
                          where m.Mem_Id == memId && m.Loan_Type == 1 && !m.Loan_Delete && !m.IsAccountClosed
                                && m.Voc_Status == "V" && m.BrCode == brCode && s.BrCode == brCode
                          select new EcsLoanRecoveryLoanVM
                          {
                              Loan_Id = m.Loan_Id,
                              Loan_No = m.Loan_No,
                              Scheme_Id = m.Scheme_Id,
                              Scheme_Name = s.Scheme_Name,
                              Roi = m.Roi,
                              Pi = m.Pi,
                              Inst_Type = s.Inst_Type,
                              Int_Application = s.Int_Application,
                              PI_Application = s.PI_Application,
                              PrlLed_Id = s.PrlLed_Id,
                              IntLed_Id = s.IntLed_Id,
                              PILed_Id = s.PILed_Id
                          }).AsNoTracking().ToListAsync();
        }

        public async Task<List<Loan_Trn>> GetLoanTrnRowsAsync(IEnumerable<decimal> loanIds, string brCode, DateTime toDate)
        {
            var ids = loanIds.Distinct().ToList();
            var to = toDate.Date;
            return await (from t in CSISContext.Loan_Trn
                          where ids.Contains(t.Loan_Id) && !t.TrnTr_Delete && t.Voc_Status == "V"
                                && t.Trn_Date <= to && t.BrCode == brCode
                          orderby t.Loan_Id, t.Trn_Date, t.Trn_SlNo
                          select t).AsNoTracking().ToListAsync();
        }

        public async Task<List<Loan_Roi>> GetRoiRowsAsync(IEnumerable<decimal> loanIds, string brCode, DateTime toDate)
        {
            var ids = loanIds.Distinct().ToList();
            var to = toDate.Date;
            return await (from r in CSISContext.Loan_Roi
                          where ids.Contains(r.Loan_Id) && r.Agency == "S" && !r.Loanroi_Delete && r.Voc_Status == "V"
                                && r.Roi_Wef <= to && r.BrCode == brCode
                          orderby r.Loan_Id, r.Roi_Wef
                          select r).AsNoTracking().ToListAsync();
        }

        public async Task<List<string>> GetPendingRecoveryDataAsync(decimal memId, string brCode, decimal excludeStagingId)
        {
            var pending = new[] { "I", "M" };
            var rows = await (from d in CSISContext.Staging_Details
                              join sm in CSISContext.Staging_Master on d.Staging_Id equals sm.Staging_Id
                              where d.Related_Account_Id == RecoveryAccountId && d.Member_Id == memId
                                    && pending.Contains(d.Staging_Status!) && pending.Contains(sm.Staging_Status!)
                                    && d.Staging_Id != excludeStagingId
                                    && d.BrCode == brCode && sm.BrCode == brCode
                              select d.Related_Account_Data).AsNoTracking().ToListAsync();
            return rows.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!).ToList();
        }

        public async Task<List<Loan_Master>> GetLoanMastersForUpdateAsync(IEnumerable<decimal> loanIds, string brCode)
        {
            var ids = loanIds.Distinct().ToList();
            return await (from m in CSISContext.Loan_Master
                          where ids.Contains(m.Loan_Id) && m.BrCode == brCode
                          select m).ToListAsync();
        }
    }
}
