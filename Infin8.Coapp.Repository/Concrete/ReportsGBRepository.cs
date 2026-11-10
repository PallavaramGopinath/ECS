using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
//using Infin8.Coapp.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class ReportsGBRepository : Repository<Reports_Master>, IReportsGBRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;

        public ReportsGBRepository(CSISContext context) : base(context)
        {
        }

        public async Task<List<rptDividendTDFWDWorking>> GetDividendWorkingSheet(decimal pbleMasterId, string brCode)
        {
            List<rptDividendTDFWDWorking> rptList = [];
            try
            {
                var result = await  (from rpt in CSISContext.Mem_Payable
                                     join mem in CSISContext.mem_master on rpt.Mem_Id equals mem.mem_id 
                              where rpt.PbleMaster_Id == pbleMasterId && rpt.Pble_Delete == false
                              select new rptDividendTDFWDWorking
                              {
                                  PbleMaster_Id = rpt.PbleMaster_Id,
                                  Mem_Id = rpt.Mem_Id,
                                  MemberNo = mem.memberno,
                                  MemberName = mem.membername ,
                                  PerNo = mem.perno,
                                  TokenNo = mem.token_personno ,
                                  Trn_Date = (DateTime )rpt.Trn_Date! ,
                                  Receipt_Amount = rpt.Receipt_Amount ,
                                  Payment_Amount = rpt.Payment_Amount , 
                                  Closing_Balance = rpt.Closing_Balance ,
                                  NoOfDays = rpt.NoOfDays ,
                                  Interest_Amount = rpt.Interest_Amount 
                              })
                              .OrderBy(x=> x.MemberNo).ThenBy(x=> x.Trn_Date) .ToListAsync();
                if (result != null && result.Count != 0)
                    rptList = [.. result];
            }
            catch (Exception)
            {
                throw;
            }
            return rptList;
        }

        public async Task<List<rptDividendIntOnTDPendingList>> GetDividendPendingList(DateTime asOnDate, string brCode)
        {
            List<rptDividendIntOnTDPendingList> rptList = [];
            try
            {
                var query = await  (from trn in CSISContext.Mem_Trn
                            join mem in CSISContext.mem_master on trn.Mem_Id equals mem.mem_id
                            join pble in CSISContext.Mem_Payable_Master on trn.PbleMaster_Id equals pble.PbleMaster_Id
                            where trn.Trn_Type == 3
                               && !trn.MemTrn_Delete
                               && trn.Trn_Date <= asOnDate 
                            group new { trn, mem, pble } by new
                            {
                                mem.memberno,
                                mem.membername,
                                mem.perno,
                                mem.tickettokengangno ,
                                trn.PbleMaster_Id,
                                trn.Mem_Id,
                                pble.FromDate,
                                pble.ToDate
                            } into g
                            where g.Sum(x => x.trn.IntCalc_Amt) - g.Sum(x => x.trn.IntPaid_Amt) > 0
                            orderby g.Key.Mem_Id, g.Key.PbleMaster_Id
                            select new rptDividendIntOnTDPendingList
                            {
                                MemberNo = g.Key.memberno,
                                MemberName = g.Key.membername,
                                PerNo = g.Key.perno,
                                TicketTokenGangNo =g.Key.tickettokengangno,
                                 PbleMaster_Id = g.Key.PbleMaster_Id,
                                 Mem_Id =   g.Key.Mem_Id,
                                 PendingAmt =  (double)g.Sum(x => x.trn.IntCalc_Amt) - (double)g.Sum(x => x.trn.IntPaid_Amt),
                                 PendingYear = g.Key.FromDate.ToString("dd-MM-yyyy") + " to " + g.Key.ToDate.ToString(g.Key.ToDate.ToString("dd-MM-yyyy")
                            )}).ToListAsync ();

                if (query != null && query.Count >0)
                    rptList = [.. query];
            }
            catch (Exception)
            {
                throw;
            }
            return rptList;
        }

        public async Task<List<rptDividendPaid >> GetDividendPaidList(DateTime fromDate, DateTime toDate, string brCode)
        {
            List<rptDividendPaid> rptList = [];
            try
            {
                var query = await (from trn in CSISContext.Mem_Trn
                                   join mem in CSISContext.mem_master on trn.Mem_Id equals mem.mem_id
                                   join pble in CSISContext.Mem_Payable_Master on trn.PbleMaster_Id equals pble.PbleMaster_Id
                                   where trn.Trn_Type == 3
                                      && !trn.MemTrn_Delete
                                      && (trn.Trn_Date >= fromDate && trn.Trn_Date <= toDate) 
                                   group new { trn, mem, pble } by new
                                   {
                                       mem.memberno,
                                       mem.membername,
                                       mem.perno,
                                       mem.tickettokengangno,
                                       trn.PbleMaster_Id,
                                       trn.Mem_Id,
                                       pble.FromDate,
                                       pble.ToDate
                                   } into g
                                   where g.Sum(x => x.trn.IntPaid_Amt) > 0
                                   orderby g.Key.Mem_Id, g.Key.PbleMaster_Id
                                   select new rptDividendPaid 
                                   {
                                       Mem_Id = g.Key.Mem_Id,
                                       MemberNo = g.Key.memberno,
                                       MemberName = g.Key.membername,
                                       PbleMaster_Id = g.Key.PbleMaster_Id,
                                       FromDate = g.Key.FromDate ,
                                       ToDate = g.Key.ToDate,
                                       DividendPaid = g.Sum(x => x.trn.IntPaid_Amt
                                   )
                                   }).ToListAsync();

                if (query != null && query.Count > 0)
                    rptList = [.. query];
            }
            catch (Exception)
            {
                throw;
            }
            return rptList;
        }
    }
}
