using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;

namespace Infin8.Coapp.Repository
{
    public class ECSDemandCalculationRepository(DbContext context) : Repository<Loan_Trn>(context), IECSDemandCalculationRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;

        public async Task<List<decimal>> GetLoanIdsToCalculateECSDemand(DateTime trnDate, DateTime dueDate, int societyType,string brCode)
        {
            List<decimal> loanIds = new List<decimal>();
            try
            {
                var result = await (from ln in CSISContext.Loan_Master
                                    join lnTrn in CSISContext.Loan_Trn on ln.Loan_Id equals lnTrn.Loan_Id
                                    join scheme in CSISContext.Loan_Schemes on ln.Scheme_Id equals scheme.Scheme_Id
                                    where scheme.Loan_Type == 1
                                       && ln.Loan_Delete == false
                                       && ln.BrCode == brCode
                                       && lnTrn.TrnTr_Delete == false
                                       && lnTrn.BrCode == brCode
                                    group new { lnTrn } by new
                                    { lnTrn.Loan_Id } into g
                                    let disbSum = g.Sum(x => x.lnTrn.Disb_Amt)
                                    let prlCollSum = g.Sum(x => x.lnTrn.PrlColl_Amt)
                                    let intCalcSum = g.Sum(x => x.lnTrn.IntCalc_Amt)
                                    let intCollSum = g.Sum(x => x.lnTrn.IntColl_Amt)
                                    let piCalcSum = g.Sum(x => x.lnTrn.PICalc_Amt)
                                    let piCollSum = g.Sum(x => x.lnTrn.PIColl_Amt)
                                    where (disbSum - prlCollSum > 0)
                                       || (intCalcSum - intCollSum > 0)
                                       || (piCalcSum - piCollSum > 0)
                                    select g.Key).ToListAsync(); // loan_id
                if(result != null && result.Count >0)
                {
                    loanIds = result.Select(x => x.Loan_Id).ToList();
                }   
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " Error in fetching loan id for ecs demand");
            }
            return loanIds;
        }
    }
}
