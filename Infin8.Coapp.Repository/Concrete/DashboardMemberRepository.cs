using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class DashboardMemberRepository : Repository<MemberDashBoardAccountsDto>, IDashboardMemberRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public DashboardMemberRepository(CSISContext context) : base(context)
        {

        }
        public List<MemberDashBoardAccountsDto> GetMemberDashBoardAccounts_old(decimal id, DateTime fromDate, DateTime toDate)
        {
            List<MemberDashBoardAccountsDto> membersAccountsList = new List<MemberDashBoardAccountsDto>();
            try
            {
                membersAccountsList = CSISContext.Database.SqlQueryRaw<MemberDashBoardAccountsDto>(
                    @"With Db1
                        AS (
						SELECT 'Share Capital' AS Led_Name, Mem_Trn.Trn_Type, Sum(Mem_Trn.Rpt_Amt) - Sum(Mem_Trn.Pmt_Amt) AS OB, 0 AS Rpt_Amt, 0 AS Pmt_Amt
                        FROM Mem_Trn INNER JOIN
                            mem_master ON Mem_Trn.mem_id = mem_master.mem_id 
                        WHERE Mem_Trn.mem_id = @memId AND Mem_Trn.MemTrn_Delete = FALSE  AND Mem_Trn.Trn_Date < @fromDate
                        GROUP BY  Mem_Trn.Trn_Type
						HAVING Mem_Trn.Trn_Type = 3
                        UNION ALL
                        SELECT 'SB Account' AS Led_Name, Mem_Trn.Trn_Type, Sum(Mem_Trn.Rpt_Amt) - Sum(Mem_Trn.Pmt_Amt) AS OB, 0 AS Rpt_Amt, 0 AS Pmt_Amt
                        FROM Mem_Trn INNER JOIN
                            mem_master ON Mem_Trn.mem_id = mem_master.mem_id 
                        WHERE Mem_Trn.mem_id = @memId AND Mem_Trn.MemTrn_Delete = FALSE  AND Mem_Trn.Trn_Date < @fromDate
                        GROUP BY  Mem_Trn.Trn_Type
						HAVING Mem_Trn.Trn_Type = 7
						UNION ALL
						SELECT 'Suspense Creditor' AS Led_Name, Mem_Trn.Trn_Type, Sum(Mem_Trn.Rpt_Amt) - Sum(Mem_Trn.Pmt_Amt) AS OB, 0 AS Rpt_Amt, 0 AS Pmt_Amt
                        FROM Mem_Trn INNER JOIN
                            mem_master ON Mem_Trn.mem_id = mem_master.mem_id 
                        WHERE Mem_Trn.mem_id = @memId AND Mem_Trn.MemTrn_Delete = FALSE AND Mem_Trn.Trn_Date < @fromDate
                        GROUP BY  Mem_Trn.Trn_Type
						HAVING Mem_Trn.Trn_Type in( 2,6)
						UNION ALL
						SELECT 'Suspense Debtor' AS Led_Name, Mem_Trn.Trn_Type,  Sum(Mem_Trn.Pmt_Amt) - Sum(Mem_Trn.Rpt_Amt) AS OB, 0 AS Rpt_Amt, 0 AS Pmt_Amt
                        FROM Mem_Trn INNER JOIN
                            mem_master ON Mem_Trn.mem_id = mem_master.mem_id 
                        WHERE Mem_Trn.mem_id = @memId AND Mem_Trn.MemTrn_Delete = FALSE  AND Mem_Trn.Trn_Date < @fromDate
                        GROUP BY  Mem_Trn.Trn_Type
						HAVING Mem_Trn.Trn_Type in( 1,5)
						UNION ALL
                        SELECT 'Interest on Suspense Creditor' AS Led_Name, Mem_Trn.Trn_Type,  Sum(Mem_Trn.IntCalc_Amt) - Sum(Mem_Trn.IntPaid_Amt) AS OB,0 AS Rpt_Amt, 0 AS Pmt_Amt
                        FROM Mem_Trn INNER JOIN
                            mem_master ON Mem_Trn.mem_id = mem_master.mem_id 
                        WHERE Mem_Trn.mem_id = @memId AND Mem_Trn.MemTrn_Delete = FALSE  AND Mem_Trn.Trn_Date < @fromDate
                        GROUP BY  Mem_Trn.Trn_Type
						HAVING Mem_Trn.Trn_Type in( 2,6)
                        UNION ALL
                        SELECT 'Dividend' AS Led_Name, Mem_Trn.Trn_Type, Sum(Mem_Trn.IntCalc_Amt) - Sum(Mem_Trn.IntPaid_Amt) AS OB,0 AS Rpt_Amt, 0 AS Pmt_Amt
                        FROM Mem_Trn INNER JOIN
                            mem_master ON Mem_Trn.mem_id = mem_master.mem_id 
                        WHERE Mem_Trn.mem_id = @memId AND Mem_Trn.MemTrn_Delete = FALSE AND Mem_Trn.Trn_Date < @fromDate
                        GROUP BY  Mem_Trn.Trn_Type
						HAVING Mem_Trn.Trn_Type = 3
						UNION ALL
						SELECT 'Share Capital' AS Led_Name, Mem_Trn.Trn_Type, 0 AS OB, Sum(Mem_Trn.Rpt_Amt) AS Rpt_Amt, Sum(Mem_Trn.Pmt_Amt) AS Pmt_Amt
                        FROM Mem_Trn INNER JOIN
                            mem_master ON Mem_Trn.mem_id = mem_master.mem_id 
                        WHERE Mem_Trn.mem_id = @memId AND Mem_Trn.MemTrn_Delete = FALSE  AND Mem_Trn.Trn_Date BETWEEN  @fromDate AND @toDate
                        GROUP BY  Mem_Trn.Trn_Type
						HAVING Mem_Trn.Trn_Type = 3
						UNION ALL
                        SELECT 'SB Account' AS Led_Name, Mem_Trn.Trn_Type, 0 AS OB, Sum(Mem_Trn.Rpt_Amt) AS Rpt_Amt, Sum(Mem_Trn.Pmt_Amt) AS Pmt_Amt
                        FROM Mem_Trn INNER JOIN
                            mem_master ON Mem_Trn.mem_id = mem_master.mem_id 
                        WHERE Mem_Trn.mem_id = @memId AND Mem_Trn.MemTrn_Delete = FALSE  AND Mem_Trn.Trn_Date BETWEEN  @fromDate AND @toDate
                        GROUP BY  Mem_Trn.Trn_Type
						HAVING Mem_Trn.Trn_Type = 7
						UNION ALL
						SELECT 'Suspense Creditor' AS Led_Name, Mem_Trn.Trn_Type, 0 AS OB, Sum(Mem_Trn.Rpt_Amt) AS Rpt_Amt, Sum(Mem_Trn.Pmt_Amt) AS Pmt_Amt
                        FROM Mem_Trn INNER JOIN
                            mem_master ON Mem_Trn.mem_id = mem_master.mem_id 
                        WHERE Mem_Trn.mem_id = @memId AND Mem_Trn.MemTrn_Delete = FALSE  AND Mem_Trn.Trn_Date BETWEEN  @fromDate AND @toDate
                        GROUP BY  Mem_Trn.Trn_Type
						HAVING Mem_Trn.Trn_Type in( 2,6)
						UNION ALL
						SELECT 'Suspense Debtor' AS Led_Name, Mem_Trn.Trn_Type, 0 AS OB, Sum(Mem_Trn.Rpt_Amt) AS Rpt_Amt, Sum(Mem_Trn.Pmt_Amt) AS Pmt_Amt
                        FROM Mem_Trn INNER JOIN
                            mem_master ON Mem_Trn.mem_id = mem_master.mem_id 
                        WHERE Mem_Trn.mem_id = @memId AND Mem_Trn.MemTrn_Delete = FALSE  AND Mem_Trn.Trn_Date BETWEEN  @fromDate AND @toDate
                        GROUP BY  Mem_Trn.Trn_Type
						HAVING Mem_Trn.Trn_Type in( 1,5)
						UNION ALL
                        SELECT 'Interest on Suspense Creditor' AS Led_Name, Mem_Trn.Trn_Type,  0 AS OB,Sum(Mem_Trn.IntCalc_Amt) AS Rpt_Amt, Sum(Mem_Trn.IntPaid_Amt) AS Pmt_Amt
                        FROM Mem_Trn INNER JOIN
                            mem_master ON Mem_Trn.mem_id = mem_master.mem_id 
                        WHERE Mem_Trn.mem_id = @memId AND Mem_Trn.MemTrn_Delete = FALSE  AND Mem_Trn.Trn_Date BETWEEN  @fromDate AND @toDate
                        GROUP BY  Mem_Trn.Trn_Type
						HAVING Mem_Trn.Trn_Type in( 2,6)
                        UNION ALL
                        SELECT 'Dividend' AS Led_Name, Mem_Trn.Trn_Type,0 AS OB, Sum(Mem_Trn.IntCalc_Amt) AS Rpt_Amt, Sum(Mem_Trn.IntPaid_Amt) AS Pmt_Amt
                        FROM Mem_Trn INNER JOIN
                            mem_master ON Mem_Trn.mem_id = mem_master.mem_id 
                        WHERE Mem_Trn.mem_id = @memId AND Mem_Trn.MemTrn_Delete = FALSE AND Mem_Trn.Trn_Date BETWEEN  @fromDate AND @toDate
                        GROUP BY  Mem_Trn.Trn_Type
						HAVING Mem_Trn.Trn_Type = 3
                        )
                        SELECT Led_Name, Trn_Type, Sum(OB) AS OB, Sum(Rpt_Amt) AS Rpt_Amt, Sum(Pmt_Amt) AS Pmt_Amt FROM Db1
                        GROUP BY Led_Name, Trn_Type 
						ORDER BY Trn_Type desc"
                        , new NpgsqlParameter("@memId", id)
                        , new NpgsqlParameter("@fromDate", fromDate)
                        , new NpgsqlParameter("@toDate", toDate)).ToList();
            }
            catch (Exception)
            {
                throw;
            }
            return membersAccountsList;
        }

        public List<MemberDashBoardAccountsDto> DashboardAccounts(decimal id, DateTime fromDate, DateTime toDate, string brCode)
        {
            List<MemberDashBoardAccountsDto> membersAccountsList = new List<MemberDashBoardAccountsDto>();
            try
            {
                #region old linq
                //var db1 = 
                //(from trn in CSISContext.Mem_Trn
                // join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                // where trn.Mem_Id == id && !trn.MemTrn_Delete && trn.Trn_Date < fromDate && trn.BrCode == brCode && master.brcode == brCode 
                // group trn by trn.Trn_Type into g
                // where g.Key == 3
                // select new MemberDashBoardAccountsDto
                // {
                //     Led_Name = "Share Capital",
                //     Trn_Type = g.Key,
                //     OB = g.Sum(x => x.Rpt_Amt) - g.Sum(x => x.Pmt_Amt),
                //     Rpt_Amt = 0,
                //     Pmt_Amt = 0
                // })
                //.Union(
                //from trn in CSISContext.Mem_Trn
                //join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                //where trn.Mem_Id == id && !trn.MemTrn_Delete && trn.Trn_Date < fromDate && trn.BrCode == brCode && master.brcode == brCode
                //group trn by trn.Trn_Type into g
                //where g.Key == 7
                //select new MemberDashBoardAccountsDto
                //{
                //    Led_Name = "SB Account",
                //    Trn_Type = g.Key,
                //    OB = g.Sum(x => x.Rpt_Amt) - g.Sum(x => x.Pmt_Amt),
                //    Rpt_Amt = 0,
                //    Pmt_Amt = 0
                //})
                //.Union(
                //from trn in CSISContext.Mem_Trn
                //join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                //where trn.Mem_Id == id && !trn.MemTrn_Delete && trn.Trn_Date < fromDate && trn.BrCode == brCode && master.brcode == brCode
                //group trn by trn.Trn_Type into g
                //where new[] { 2, 6 }.Contains(g.Key)
                //select new MemberDashBoardAccountsDto
                //{
                //    Led_Name = "Suspense Creditor",
                //    Trn_Type = g.Key,
                //    OB = g.Sum(x => x.Rpt_Amt) - g.Sum(x => x.Pmt_Amt),
                //    Rpt_Amt = 0,
                //    Pmt_Amt = 0
                //})
                //.Union(
                //from trn in CSISContext.Mem_Trn
                //join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                //where trn.Mem_Id == id && !trn.MemTrn_Delete && trn.Trn_Date < fromDate && trn.BrCode == brCode && master.brcode == brCode
                //group trn by trn.Trn_Type into g
                //where new[] { 1, 5 }.Contains(g.Key)
                //select new MemberDashBoardAccountsDto
                //{
                //    Led_Name = "Suspense Debtor",
                //    Trn_Type = g.Key,
                //    OB = g.Sum(x => x.Pmt_Amt) - g.Sum(x => x.Rpt_Amt),
                //    Rpt_Amt = 0,
                //    Pmt_Amt = 0
                //})
                //.Union(
                //from trn in CSISContext.Mem_Trn
                //join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                //where trn.Mem_Id == id && !trn.MemTrn_Delete && trn.Trn_Date < fromDate && trn.BrCode == brCode && master.brcode == brCode
                //group trn by trn.Trn_Type into g
                //where new[] { 2, 6 }.Contains(g.Key)
                //select new MemberDashBoardAccountsDto
                //{
                //    Led_Name = "Interest on Suspense Creditor",
                //    Trn_Type = g.Key,
                //    OB = g.Sum(x => x.Pmt_Amt) - g.Sum(x => x.Rpt_Amt),
                //    Rpt_Amt = 0,
                //    Pmt_Amt = 0
                //})
                //.Union(
                //from trn in CSISContext.Mem_Trn
                //join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                //where trn.Mem_Id == id && !trn.MemTrn_Delete && trn.Trn_Date < fromDate && trn.BrCode == brCode && master.brcode == brCode
                //group trn by trn.Trn_Type into g
                //where g.Key == 3
                //select new MemberDashBoardAccountsDto
                //{
                //    Led_Name = "Dividend",
                //    Trn_Type = g.Key,
                //    OB = g.Sum(x => x.IntCalc_Amt) - g.Sum(x => x.IntPaid_Amt),
                //    Rpt_Amt = 0,
                //    Pmt_Amt = 0
                //})
                //.Union(
                // from trn in CSISContext.Mem_Trn
                // join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                // where trn.Mem_Id == id && !trn.MemTrn_Delete && (trn.Trn_Date >= fromDate && trn.Trn_Date <= toDate) && trn.BrCode == brCode && master.brcode == brCode
                // group trn by trn.Trn_Type into g
                // where g.Key == 3
                // select new MemberDashBoardAccountsDto
                // {
                //     Led_Name = "Share Capital",
                //     Trn_Type = g.Key,
                //     OB = 0,
                //     Rpt_Amt = g.Sum(x => x.Rpt_Amt),
                //     Pmt_Amt = g.Sum(x => x.Pmt_Amt)
                // })
                //.Union(
                // from trn in CSISContext.Mem_Trn
                // join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                // where trn.Mem_Id == id && !trn.MemTrn_Delete && (trn.Trn_Date >= fromDate && trn.Trn_Date <= toDate) && trn.BrCode == brCode && master.brcode == brCode
                // group trn by trn.Trn_Type into g
                // where g.Key == 7
                // select new MemberDashBoardAccountsDto
                // {
                //     Led_Name = "SB Account",
                //     Trn_Type = g.Key,
                //     OB = 0,
                //     Rpt_Amt = g.Sum(x => x.Rpt_Amt),
                //     Pmt_Amt = g.Sum(x => x.Pmt_Amt)
                // })
                //.Union(
                // from trn in CSISContext.Mem_Trn
                // join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                // where trn.Mem_Id == id && !trn.MemTrn_Delete && (trn.Trn_Date >= fromDate && trn.Trn_Date <= toDate) && trn.BrCode == brCode && master.brcode == brCode
                // group trn by trn.Trn_Type into g
                // where new[] { 2, 6 }.Contains(g.Key)
                // select new MemberDashBoardAccountsDto
                // {
                //     Led_Name = "Suspense Creditor",
                //     Trn_Type = g.Key,
                //     OB = 0,
                //     Rpt_Amt = g.Sum(x => x.Rpt_Amt),
                //     Pmt_Amt = g.Sum(x => x.Pmt_Amt)
                // })
                //.Union(
                // from trn in CSISContext.Mem_Trn
                // join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                // where trn.Mem_Id == id && !trn.MemTrn_Delete && (trn.Trn_Date >= fromDate && trn.Trn_Date <= toDate) && trn.BrCode == brCode && master.brcode == brCode
                // group trn by trn.Trn_Type into g
                // where new[] { 1, 5 }.Contains(g.Key)
                // select new MemberDashBoardAccountsDto
                // {
                //     Led_Name = "Suspense Debtor",
                //     Trn_Type = g.Key,
                //     OB = 0,
                //     Rpt_Amt = g.Sum(x => x.Rpt_Amt),
                //     Pmt_Amt = g.Sum(x => x.Pmt_Amt)
                // })
                //.Union(
                // from trn in CSISContext.Mem_Trn
                // join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                // where trn.Mem_Id == id && !trn.MemTrn_Delete && (trn.Trn_Date >= fromDate && trn.Trn_Date <= toDate) && trn.BrCode == brCode && master.brcode == brCode
                // group trn by trn.Trn_Type into g
                // where new[] { 2, 6 }.Contains(g.Key)
                // select new MemberDashBoardAccountsDto
                // {
                //     Led_Name = "Interest on Suspense Creditor",
                //     Trn_Type = g.Key,
                //     OB = 0,
                //     Rpt_Amt = g.Sum(x => x.IntCalc_Amt),
                //     Pmt_Amt = g.Sum(x => x.IntPaid_Amt)
                // })
                //.Union(
                // from trn in CSISContext.Mem_Trn
                // join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                // where trn.Mem_Id == id && !trn.MemTrn_Delete && (trn.Trn_Date >= fromDate && trn.Trn_Date <= toDate) && trn.BrCode == brCode && master.brcode == brCode
                // group trn by trn.Trn_Type into g
                // where g.Key == 3
                // select new MemberDashBoardAccountsDto
                // {
                //     Led_Name = "Dividend",
                //     Trn_Type = g.Key,
                //     OB = 0,
                //     Rpt_Amt = g.Sum(x => x.IntCalc_Amt),
                //     Pmt_Amt = g.Sum(x => x.IntPaid_Amt)
                // }).ToList();

                //var result =
                //(from record in db1
                // group record by new { record.Led_Name, record.Trn_Type } into g
                // orderby g.Key.Trn_Type descending
                // select new MemberDashBoardAccountsDto
                // {
                //     Led_Name = g.Key.Led_Name,
                //     Trn_Type = g.Key.Trn_Type,
                //     OB = g.Sum(x => x.OB),
                //     Rpt_Amt = g.Sum(x => x.Rpt_Amt),
                //     Pmt_Amt = g.Sum(x => x.Pmt_Amt)
                // }).ToList();
                //if (result != null && result.Any()) membersAccountsList = result;
                #endregion
                #region new linq
                var memTrns = CSISContext.Mem_Trn
                .Join(CSISContext.mem_master,
                    trn => trn.Mem_Id,
                    master => master.mem_id,
                    (trn, master) => new { trn, master })
                .Where(x => x.trn.Mem_Id == id
                    && !x.trn.MemTrn_Delete
                    && x.trn.BrCode == brCode
                    && x.master.brcode == brCode);

                var before = memTrns.Where(x => x.trn.Trn_Date < fromDate);
                var during = memTrns.Where(x => x.trn.Trn_Date >= fromDate && x.trn.Trn_Date <= toDate);

                var groupedBefore = before
                    .GroupBy(x => x.trn.Trn_Type)
                    .Select(g => new MemberDashBoardAccountsDto
                    {
                        Led_Name = g.Key == 3 ? "Share Capital" :
                                   g.Key == 7 ? "SB Account" :
                                   new[] { 2, 6 }.Contains(g.Key) ? "Suspense Creditor" :
                                   new[] { 1, 5 }.Contains(g.Key) ? "Suspense Debtor" :
                                   g.Key == 3 ? "Dividend" : "Other",
                        Trn_Type = g.Key,
                        OB = g.Key == 3 ? g.Sum(x => x.trn.Rpt_Amt) - g.Sum(x => x.trn.Pmt_Amt) : 0,
                        Rpt_Amt = 0,
                        Pmt_Amt = 0
                    });

                var groupedDuring = during
                    .GroupBy(x => x.trn.Trn_Type)
                    .Select(g => new MemberDashBoardAccountsDto
                    {
                        Led_Name = g.Key == 3 ? "Share Capital" :
                                   g.Key == 7 ? "SB Account" :
                                   new[] { 2, 6 }.Contains(g.Key) ? "Suspense Creditor" :
                                   new[] { 1, 5 }.Contains(g.Key) ? "Suspense Debtor" :
                                   g.Key == 3 ? "Dividend" : "Other",
                        Trn_Type = g.Key,
                        OB = 0,
                        Rpt_Amt = g.Sum(x => x.trn.Rpt_Amt),
                        Pmt_Amt = g.Sum(x => x.trn.Pmt_Amt)
                    });

                var db1 = groupedBefore.Concat(groupedDuring).ToList();

                var result = db1
                    .GroupBy(x => new { x.Led_Name, x.Trn_Type })
                    .Select(g => new MemberDashBoardAccountsDto
                    {
                        Led_Name = g.Key.Led_Name,
                        Trn_Type = g.Key.Trn_Type,
                        OB = g.Sum(x => x.OB),
                        Rpt_Amt = g.Sum(x => x.Rpt_Amt),
                        Pmt_Amt = g.Sum(x => x.Pmt_Amt)
                    })
                    .OrderByDescending(x => x.Trn_Type)
                    .ToList();
                if (result != null && result.Any()) membersAccountsList = result;

                #endregion 
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
            }
            return membersAccountsList;
        }

        public async Task<List<MemberDashBoardLoans>> GetMemberDashBoardLoans(decimal memId, DateTime fromDate, DateTime toDate, string brCode)
        {
            int count = 0;
            List<MemberDashBoardLoans> loanList = new List<MemberDashBoardLoans>();
            try
            {
                #region query
                //var loans = await CSISContext.Database.SqlQueryRaw<MemberDashBoardLoans>(
                //    @"SELECT Loan_Master.Mem_Id, false as  IsLoanClosed,
                //                  Loan_Master.Scheme_Id,Loan_Master.Loan_Id, Loan_Master.Loan_No,  Loan_Schemes.Scheme_Name, Loan_Master.San_amt, Loan_Master.San_date,  Loan_Master.Loan_Type, 
                //               SUM(Loan_Trn.Disb_Amt) AS Disb_Amt, 
                //               Loan_Master.San_amt - SUM(Loan_Trn.PrlColl_Amt) AS PrlOS, 
                //               CASE
                //                WHEN SUM(Loan_Trn.Prl_Dem) -  SUM(Loan_Trn.PrlColl_Amt) < 0
                //                THEN 0
                //                ELSE 
                //                 SUM(Loan_Trn.Prl_Dem) -  SUM(Loan_Trn.PrlColl_Amt)
                //               END  AS PrlOD, 
                //               SUM(Loan_Trn.IntCalc_Amt) - SUM(Loan_Trn.IntColl_Amt) AS IntOD, 
                //                  CASE
                //	WHEN SUM(Loan_Trn.IntCalc_Amt) - SUM(Loan_Trn.IntColl_Amt) IS NULL
                //	THEN 0
                //	ELSE SUM(Loan_Trn.IntCalc_Amt) - SUM(Loan_Trn.IntColl_Amt)
                //END AS IODBal,
                //                  SUM(Loan_Trn.PICalc_Amt) - SUM(Loan_Trn.PIColl_Amt) AS PIBal, 
                //               to_char(CASE
                //                WHEN 
                //                 MAX(Loan_Trn.Due_Date) IS NULL
                //                THEN 
                //                 MAX(Loan_Trn.IntCalc_Date)
                //                ELSE
                //                 MAX(Loan_Trn.Due_Date)
                //               END,'dd-MM-yyyy') AS Due_Date
                //                  FROM Loan_Master INNER JOIN
                //                      Loan_Trn ON Loan_Master.Loan_id = Loan_Trn.Loan_Id INNER JOIN
                //                      Loan_Schemes ON Loan_Master.Scheme_Id = Loan_Schemes.Scheme_Id
                //                  WHERE  (Loan_Master.Mem_Id = @memId) AND (Loan_Master.Loan_Delete = false) AND (Loan_Trn.TrnTr_Delete = false) 
                //                  AND (Loan_Trn.Trn_Date <= @toDate)
                //                  AND (Loan_Trn.Loan_Id in (SELECT DISTINCT Loan_trn.Loan_Id FROM Loan_Trn 
                //INNER JOIN Loan_Master ON Loan_trn.Loan_Id = Loan_Master.Loan_id 
                //WHERE Loan_Master.Mem_Id = @memId AND  Loan_Trn.Trn_Date BETWEEN @fromDate AND @toDate AND Loan_Trn.TrnTr_Delete = false ))
                //                  GROUP BY Loan_Master.Mem_Id,Loan_Master.Loan_Id, Loan_Master.Loan_No,Loan_Master.Scheme_Id,  Loan_Schemes.Scheme_Name, 
                //                  Loan_Master.San_amt, Loan_Master.San_date, Loan_Master.Loan_Type"
                //    , new NpgsqlParameter("@memId", memId)
                //    , new NpgsqlParameter("@fromDate", fromDate)
                //    , new NpgsqlParameter("@toDate", toDate)).ToListAsync();
                //if (loans != null && loans.Any()) loanList = loans.ToList();
                #endregion

                ///AND (Loan_Trn.Trn_Date BETWEEN @fromDate AND @toDate)

                //HAVING SUM(Loan_Trn.Disb_Amt) -SUM(Loan_Trn.PrlColl_Amt) > 0 OR SUM(Loan_Trn.IntCalc_Amt) -SUM(Loan_Trn.IntColl_Amt) > 0 OR

                //    SUM(Loan_Trn.IODCalc_Amt) - SUM(Loan_Trn.IODColl_Amt) > 0 OR SUM(Loan_Trn.PICalc_Amt) -SUM(Loan_Trn.PIColl_Amt) > 0

                // First, get the distinct Loan_Ids from the subquery

                #region old linq less performance
                //var validLoanIds = await (
                //    from trn in CSISContext.Loan_Trn
                //    join master in CSISContext.Loan_Master on trn.Loan_Id equals master.Loan_Id
                //    where master.Mem_Id == memId
                //        && trn.Trn_Date >= fromDate
                //        && trn.Trn_Date <= toDate
                //        && trn.TrnTr_Delete == false
                //        && trn.BrCode == brCode 
                //        && master.BrCode == brCode
                //    select trn.Loan_Id
                //).Distinct().ToListAsync();

                //// Then use the main query
                //var loans = await (
                //    from master in CSISContext.Loan_Master
                //    join trn in CSISContext.Loan_Trn on master.Loan_Id equals trn.Loan_Id
                //    join scheme in CSISContext.Loan_Schemes on master.Scheme_Id equals scheme.Scheme_Id
                //    where master.Mem_Id == memId
                //        && master.Loan_Delete == false
                //        && trn.TrnTr_Delete == false
                //        && trn.Trn_Date <= toDate
                //        && master.BrCode == brCode 
                //        && trn.BrCode == brCode 
                //        && scheme.BrCode == brCode 
                //        && validLoanIds.Contains(trn.Loan_Id)
                //    group new { master, trn, scheme } by new
                //    {
                //        master.Mem_Id,
                //        master.Loan_Id,
                //        master.Loan_No,
                //        master.Scheme_Id,
                //        scheme.Scheme_Name,
                //        master.San_Amt,
                //        master.San_Date,
                //        master.Loan_Type
                //    } into g
                //    let maxDueDate = g.Max(x => x.trn.Due_Date)
                //    let maxIntCalcDate = g.Max(x => x.trn.IntCalc_Date)
                //    select new MemberDashBoardLoans
                //    {

                //        Mem_Id = g.Key.Mem_Id,
                //        Scheme_Id = g.Key.Scheme_Id,
                //        Loan_Id = g.Key.Loan_Id,
                //        Loan_No = g.Key.Loan_No,
                //        Scheme_Name = g.Key.Scheme_Name,
                //        San_Amt = g.Key.San_Amt,
                //        San_Date = g.Key.San_Date,
                //        Loan_Type = g.Key.Loan_Type,
                //        Disb_Amt = g.Sum(x => x.trn.Disb_Amt),
                //        PrlOS = g.Key.San_Amt - g.Sum(x => x.trn.PrlColl_Amt),
                //        PrlOD = (g.Sum(x => x.trn.Prl_Dem) - g.Sum(x => x.trn.PrlColl_Amt)) < 0
                //                ? 0
                //                : (g.Sum(x => x.trn.Prl_Dem) - g.Sum(x => x.trn.PrlColl_Amt)),
                //        IntOD = g.Sum(x => x.trn.IntCalc_Amt) - g.Sum(x => x.trn.IntColl_Amt),
                //        IODBal = (g.Sum(x => x.trn.IODCalc_Amt) - g.Sum(x => x.trn.IODColl_Amt)),
                //        PIBal = (g.Sum(x => x.trn.PICalc_Amt) - g.Sum(x => x.trn.PIColl_Amt)),
                //        Due_Date = (maxDueDate ?? maxIntCalcDate).ToString()
                //    }
                //).ToListAsync();
                #endregion

                #region new linq from perplexity ai
                var loans = await (
                    from master in CSISContext.Loan_Master
                    join trn in CSISContext.Loan_Trn on master.Loan_Id equals trn.Loan_Id
                    join scheme in CSISContext.Loan_Schemes on master.Scheme_Id equals scheme.Scheme_Id
                    // Filter: basic requirements
                    where master.Mem_Id == memId
                        && master.Loan_Delete == false
                        && trn.TrnTr_Delete == false
                        && trn.Trn_Date <= toDate
                        && master.BrCode == brCode
                        && trn.BrCode == brCode
                        && scheme.BrCode == brCode
                        // Extra filter: loan id is in loans having at least one trn in the period
                        && CSISContext.Loan_Trn.Any(trn2 =>
                                 trn2.Loan_Id == trn.Loan_Id
                                && trn2.TrnTr_Delete == false
                                && trn2.Trn_Date >= fromDate
                                && trn2.Trn_Date <= toDate)
                    group new { master, trn, scheme } by new
                    {
                        master.Mem_Id,
                        master.Loan_Id,
                        master.Loan_No,
                        master.Scheme_Id,
                        scheme.Scheme_Name,
                        master.San_Amt,
                        master.San_Date,
                        master.Loan_Type
                    }
                    into g
                    select new MemberDashBoardLoans
                    {
                        Mem_Id = g.Key.Mem_Id,
                        IsLoanClosed = false,
                        Scheme_Id = g.Key.Scheme_Id,
                        Loan_Id = g.Key.Loan_Id,
                        Loan_No = g.Key.Loan_No,
                        Scheme_Name = g.Key.Scheme_Name,
                        San_Amt = g.Key.San_Amt,
                        San_Date = g.Key.San_Date,
                        Loan_Type = g.Key.Loan_Type,
                        Disb_Amt = g.Sum(x => x.trn.Disb_Amt),
                        PrlOS = g.Key.San_Amt - g.Sum(x => x.trn.PrlColl_Amt),
                        PrlOD = (g.Sum(x => x.trn.Prl_Dem) - g.Sum(x => x.trn.PrlColl_Amt)) < 0
                        ? 0
                        : (g.Sum(x => x.trn.Prl_Dem) - g.Sum(x => x.trn.PrlColl_Amt)),
                        IntOD = g.Sum(x => x.trn.IntCalc_Amt) - g.Sum(x => x.trn.IntColl_Amt),
                        IODBal = (g.Sum(x => x.trn.IODCalc_Amt) - g.Sum(x => x.trn.IODColl_Amt)),
                        PIBal = (g.Sum(x => x.trn.PICalc_Amt) - g.Sum(x => x.trn.PIColl_Amt)),
                        // Compute both max values, then choose using ternary logic
                        Due_Date = (
                        (g.Max(x => x.trn.Due_Date) ?? g.Max(x => x.trn.IntCalc_Date)).ToString())
                    }).ToListAsync();
                if (loans != null && loans.Any()) loanList = loans.ToList();
                #endregion 

                foreach (var ln in loanList)
                {
                    count = CSISContext.Loan_Trn
                    .Where(trn => trn.TrnTr_Delete == false && trn.Loan_Id == ln.Loan_Id)
                    .GroupBy(trn => trn.Loan_Id)
                    .Where(g =>
                        (g.Sum(x => x.Disb_Amt) - g.Sum(x => x.PrlColl_Amt) > 0) ||
                        (g.Sum(x => x.IntCalc_Amt) - g.Sum(x => x.IntColl_Amt) > 0) ||
                        (g.Sum(x => x.IODCalc_Amt) - g.Sum(x => x.IODColl_Amt) > 0) ||
                        (g.Sum(x => x.PICalc_Amt) - g.Sum(x => x.PIColl_Amt) > 0)
                    )
                    .Count();
                    int.TryParse(count.ToString(), out int resultCount);
                    if (resultCount > 0)
                        ln.IsLoanClosed = false;
                    else
                        ln.IsLoanClosed = true;

                }
                if (loans != null && loans.Any())
                {
                    loanList = loans.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return loanList;
        }

        public async Task<List<MemberDashBoardTD>> GetMemberDashBoardTDs(decimal memId, DateTime fromDate, DateTime toDate, string brCode)
        {
            List<MemberDashBoardTD> tds = new List<MemberDashBoardTD>();
            try
            {
                #region query
                //              tds = CSISContext.Database.SqlQueryRaw<MemberDashBoardTD>(
                //                  @"SELECT TermDeposit_Master.Mem_Id,TermDeposit_Master.TD_Id,  TermDeposit_Master.TDScheme_Id, 
                //                  TermDeposit_Schemes.TDScheme_Name, TermDeposit_Master.TD_No, TermDeposit_Master.TDH_Name, 
                //               TermDeposit_Master.PeriodInMonths, TermDeposit_Master.PeriodInDays, TermDeposit_Master.RateOfInterest, 
                //                  TermDeposit_Master.AccountClosed,
                //               FORMAT(TermDeposit_Master.ValueDate,'dd-MM-yyyy') AS ValueDate, 
                //               SUM(TermDeposit_Trn.DepositReceiptAmount) AS TDAmt, 
                //               FORMAT(TermDeposit_Master.MaturityDate,'dd-MM-yyyy') AS MaturityDate, 
                //               sum(TermDeposit_Trn.MaturityAmount) AS MaturityAmount, 
                //                  SUM(TermDeposit_Trn.InterestCalculatedAmount) AS IntCalc, 
                //               SUM(TermDeposit_Trn.InterestPaidAmount) AS IntPaid, 
                //               CASE WHEN
                //                MAX(TermDeposit_Trn.InterestAppliedDate) IS NULL
                //               THEN		
                //                NULL
                //               ELSE
                //                FORMAT(MAX(TermDeposit_Trn.InterestAppliedDate),'dd-MM-yyyy')
                //               END AS IntCalcDate
                //                  FROM TermDeposit_Master INNER JOIN
                //                      TermDeposit_Trn ON TermDeposit_Master.TD_Id = TermDeposit_Trn.TD_Id INNER JOIN
                //                      TermDeposit_Schemes ON TermDeposit_Master.TDScheme_Id = TermDeposit_Schemes.TDScheme_Id
                //                  WHERE  (TermDeposit_Master.TD_Delete = 0) AND (TermDeposit_Trn.TD_Delete = 0) 
                //                  AND TermDeposit_Master.TD_Id IN (SELECT DISTINCT TermDeposit_Trn.TD_Id FROM  TermDeposit_Trn 
                //INNER JOIN TermDeposit_Master ON TermDeposit_Trn.TD_Id = TermDeposit_Master.TD_Id 
                //WHERE (Mem_Id = @memId) AND (Trn_Date BETWEEN @fromDate AND @toDate) AND (TermDeposit_Trn.TD_Delete = 0))
                //AND (TermDeposit_Trn.Trn_Date <=@toDate)
                //                  GROUP BY TermDeposit_Master.Mem_Id,TermDeposit_Master.TD_Id, TermDeposit_Master.TDScheme_Id, TermDeposit_Schemes.TDScheme_Name, 
                //                  TermDeposit_Master.TD_No,TermDeposit_Master.TDH_Name, TermDeposit_Master.ValueDate,TermDeposit_Master.PeriodInMonths, 
                //                  TermDeposit_Master.PeriodInDays,TermDeposit_Master.RateOfInterest,TermDeposit_Master.AccountClosed, TermDeposit_Master.MaturityDate
                //                  HAVING (TermDeposit_Master.Mem_Id = @memId)
                //                  ORDER BY TermDeposit_Master.TDScheme_Id"
                //                  , new NpgsqlParameter("@memId", memId)
                //                  , new NpgsqlParameter("@fromDate", fromDate)
                //                  , new NpgsqlParameter("@toDate", toDate)).ToList();
                #endregion

                ///                        AND (TermDeposit_Master.ValueDate  BETWEEN @fromDate AND @toDate)
                // First, get the distinct TD_Ids from the subquery
                var result = await (
                 from master in CSISContext.TermDeposit_Master
                 join trn in CSISContext.TermDeposit_Trn on master.TD_Id equals trn.TD_Id
                 join scheme in CSISContext.TermDeposit_Schemes on master.TDScheme_Id equals scheme.TDScheme_Id
                 where master.TD_Delete == false
                     && trn.TD_Delete == false
                     && trn.Trn_Date <= toDate
                     && (
                         from t in CSISContext.TermDeposit_Trn
                         join m in CSISContext.TermDeposit_Master on t.TD_Id equals m.TD_Id
                         where m.Mem_Id == memId
                             && t.Trn_Date >= fromDate
                             && t.Trn_Date <= toDate
                             && t.TD_Delete == false
                         select t.TD_Id
                     ).Contains(master.TD_Id)
                 group new { master, trn, scheme } by new
                 {
                     master.Mem_Id,
                     master.TD_Id,
                     master.TDScheme_Id,
                     scheme.TDScheme_Name,
                     master.TD_No,
                     master.TDH_Name,
                     master.ValueDate,
                     master.PeriodInMonths,
                     master.PeriodInDays,
                     master.RateOfInterest,
                     master.AccountClosed,
                     master.MaturityDate
                 } into g
                 where g.Key.Mem_Id == memId // Having equivalent
                 orderby g.Key.TDScheme_Id
                 select new MemberDashBoardTD
                 {
                     Mem_Id = g.Key.Mem_Id,
                     TD_Id = g.Key.TD_Id,
                     TDScheme_Id = g.Key.TDScheme_Id,
                     TDScheme_Name = g.Key.TDScheme_Name,
                     TD_No = g.Key.TD_No,
                     TDH_Name = g.Key.TDH_Name,
                     PeriodInMonths = g.Key.PeriodInMonths,
                     PeriodInDays = g.Key.PeriodInDays,
                     RateOfInterest = g.Key.RateOfInterest,
                     AccountClosed = g.Key.AccountClosed,
                     ValueDate = g.Key.ValueDate.ToString("dd-MM-yyyy"),
                     TDAmt = g.Sum(x => x.trn.DepositReceiptAmount),
                     MaturityDate = g.Key.MaturityDate.ToString("dd-MM-yyyy"),
                     MaturityAmount = g.Sum(x => x.trn.MaturityAmount),
                     IntCalc = g.Sum(x => x.trn.InterestCalculatedAmount),
                     IntPaid = g.Sum(x => x.trn.InterestPaidAmount),
                     IntCalcDate = g.Max(x => x.trn.InterestAppliedDate) == null
                         ? null
                         : g.Max(x => x.trn.InterestAppliedDate).ToString()
                 }
                     ).ToListAsync();

                if (result != null && result.Any())
                {
                    tds = result.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return tds;
        }

        public async Task<MemberDetailsVM2> GetMemberDataDashBoard(decimal memId, string brCode)
        {
            MemberDetailsVM2 memData = new();
            try
            {
                var result = await (from master in CSISContext.mem_master
                                    where master.mem_id == memId
                                    join bank in CSISContext.Bank_Master on master.bankname equals bank.Bank_ShortName into bankGroup
                                    from bank in bankGroup.Where(b => !b.Bank_Delete).DefaultIfEmpty()
                                    where master.brcode == brCode
                                    select new MemberDetailsVM2
                                    {
                                        Mem_Id = master.mem_id,
                                        PhotoImage = master.memberphoto,
                                        NomineeName = master.membername,
                                        NomineeAge = master.nomineeage,
                                        NomineeRelationShip = master.nomineerelationship,
                                        SBAccountNo = master.sbaccountno,
                                        BankName = bank != null ? bank.Bank_Name : null,
                                        IFSCCode = master.ifsccode,
                                        PANNo = master.panno,
                                        AadharNo = master.aadharno,
                                        SmartCardNo = master.smartcardno
                                    }).FirstOrDefaultAsync();
                if (result != null && result.Mem_Id > 0) memData = result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return memData;
        }

        public async Task<List<MemberLedgerVM>> GetMemberLedger(decimal MemId, int TrnType, DateTime FromDate, DateTime ToDate, string brCode)
        {
            double OBAmt = 0;
            int IntBal = 0;
            List<MemberLedgerVM> memLedger = new List<MemberLedgerVM>();
            try
            {
                var ledIdList = await CSISContext.Database.SqlQueryRaw<decimal>(
                    @"SELECT DISTINCT Mem_Trn.Led_Id FROM Mem_Trn Where Mem_Trn.Mem_Id = @memId 
                    And Mem_Trn.Trn_Type = @trnType And Mem_Trn.MemTrn_Delete = false AND Mem_Trn.BrCode = @brCode"
                    , new NpgsqlParameter("@memId", MemId)
                    , new NpgsqlParameter("@trnType", TrnType)
                    , new NpgsqlParameter("@brCode", brCode)).ToListAsync();
                /// iterate Led id
                foreach (decimal single in ledIdList)
                {
                    OBAmt = 0;
                    IntBal = 0;
                    /// get ob
                    var OB =await (from trn in CSISContext.Mem_Trn
                              where trn.Mem_Id == MemId && trn.Led_Id == single && trn.MemTrn_Delete == false
                              group trn by new { trn.Mem_Id, trn.Led_Id } into g
                              select new MemberLedgerVM
                              {
                                  Rpt_Amt = g.Sum(x => x.Rpt_Amt),
                                  Pmt_Amt = g.Sum(x => x.Pmt_Amt),
                                  IntBal_Amt = g.Sum(x => x.IntCalc_Amt),
                                  IntPaid_Amt = g.Sum(x => x.IntPaid_Amt)
                              }).FirstOrDefaultAsync();
                    if (OB != null)
                    {
                        int.TryParse(OB.IntCalc_Amt.ToString(), out int _intCalc);
                        int.TryParse(OB.IntPaid_Amt.ToString(), out int _intPaid);
                        double.TryParse(OB.Rpt_Amt.ToString(), out double _rptAmt);
                        double.TryParse(OB.Pmt_Amt.ToString(), out double _pmtAmt);

                        OB.IntBal_Amt = _intCalc - _intPaid;

                        IntBal = OB.IntCalc_Amt != null ? Convert.ToInt32(OB.IntCalc_Amt) : 0 - OB.IntPaid_Amt != null ? Convert.ToInt32(OB.IntPaid_Amt) : 0;
                        IntBal = _intCalc - _intPaid;
                        if (TrnType == 3 || TrnType == 2 || TrnType == 6 || TrnType == 7)    /// share capital or suspense creditor or sb account
                        {
                            OB.Bal_Amt = _rptAmt - _pmtAmt;
                            OBAmt = _rptAmt - _pmtAmt;
                        }
                        else
                        {
                            OB.Bal_Amt = _pmtAmt - _rptAmt;
                            OBAmt = _pmtAmt - _rptAmt;
                        }

                        if (IntBal > 0 || OBAmt > 0)
                        {
                            OB.Led_Name = CSISContext.Fin_Ledger.Where(x => x.Led_Id == single).Select(x => x.Led_Name).FirstOrDefault(); /// dbAccount.GetLedgerName(single) + " OB";
                            OB.Trn_Date = FromDate;
                            OB.Trn_Type = TrnType;
                            memLedger.Add(OB);
                        }
                    }

                    //var Trn = await CSISContext.Database.SqlQueryRaw<MemberLedgerVM>(
                    //    @"SELECT Mem_Trn.Mem_Id, Mem_Trn.Trn_Date, Mem_Trn.Trn_Type, Mem_Trn.Led_Id, Fin_Ledger.Led_Name, Fin_Voucher.Voc_No, Mem_Trn.Rpt_Amt, 
                    //        Mem_Trn.Pmt_Amt, CAST(0 as float) AS Bal_Amt, Mem_Trn.IntCalc_Amt, Mem_Trn.IntPaid_Amt, Mem_Trn.IntCalc_Date, Mem_Trn.Trn_slno, Mem_Trn.MemTrn_Delete,Mem_trn.Voc_Id 
                    //        FROM (Mem_Trn LEFT JOIN Fin_Voucher ON Mem_Trn.Voc_Id = Fin_Voucher.Voc_Id) INNER JOIN Fin_Ledger ON Mem_Trn.Led_Id = Fin_Ledger.Led_Id
                    //        Where Mem_Trn.Led_id = @ledId AND Mem_Trn.Mem_Id = @memId And Mem_Trn.MemTrn_Delete = false AND Mem_Trn.BrCode = @brCode 
                    //        AND (Mem_Trn.Trn_Date BETWEEN @fromDate AND @toDate) AND Fin_Voucher.BrCode = @brCode 
                    //        AND Fin_Ledger.BrCode = @brCode
                    //        ORDER BY Mem_Trn.Led_Id, Mem_Trn.Trn_Date, Mem_Trn.Trn_slno"
                    //    , new NpgsqlParameter("@ledId", single)
                    //    , new NpgsqlParameter("@memId", MemId)
                    //    , new NpgsqlParameter("@fromDate", FromDate)
                    //    , new NpgsqlParameter("@toDate", ToDate)
                    //    , new NpgsqlParameter("@brCode", brCode)).ToListAsync();

                    var Trn = await (
                        from trn in CSISContext.Mem_Trn
                        join ledger in CSISContext.Fin_Ledger on trn.Led_Id equals ledger.Led_Id
                        join voucher in CSISContext.Fin_Voucher on trn.Voc_Id equals voucher.Voc_Id into vocJoin
                        from voucher in vocJoin.DefaultIfEmpty() // Left Join
                        where trn.Led_Id == single
                            && trn.Mem_Id == MemId
                            && trn.MemTrn_Delete == false
                            && trn.BrCode == brCode
                            && trn.Trn_Date >= FromDate
                            && trn.Trn_Date <= ToDate
                            && (voucher == null || voucher.BrCode == brCode) // Handle left join for voucher
                            && ledger.BrCode == brCode
                        orderby trn.Led_Id, trn.Trn_Date, trn.Trn_SlNo
                        select new MemberLedgerVM
                        {
                            Mem_Id = trn.Mem_Id,
                            Trn_Date = trn.Trn_Date,
                            Trn_Type = trn.Trn_Type,
                            Led_Id = trn.Led_Id,
                            Led_Name = ledger.Led_Name,
                            Voc_No = voucher != null ? voucher.Voc_No : null,
                            Rpt_Amt = trn.Rpt_Amt,
                            Pmt_Amt = trn.Pmt_Amt,
                            Bal_Amt = 0, // float literal for compatibility
                            IntCalc_Amt = trn.IntCalc_Amt,
                            IntPaid_Amt = trn.IntPaid_Amt,
                            IntCalc_Date = trn.IntCalc_Date,
                            Trn_SlNo = trn.Trn_SlNo,
                            Voc_Id = trn.Voc_Id,
                            Yr_Id = trn.Yr_Id ,
                            BrCode = trn.BrCode 
                        }
                    ).ToListAsync();
                    if (Trn != null)
                    {
                        foreach (var led in Trn)
                        {
                            int.TryParse(led.IntCalc_Amt.ToString(), out int _intCalc);
                            int.TryParse(led.IntPaid_Amt.ToString(), out int _intPaid);
                            double.TryParse(led.Rpt_Amt.ToString(), out double _rptAmt);
                            double.TryParse(led.Pmt_Amt.ToString(), out double _pmtAmt);
                            IntBal += _intCalc - _intPaid;
                            led.IntBal_Amt = IntBal;

                            if (TrnType == 3 || TrnType == 2 || TrnType == 7)
                            {
                                OBAmt += _rptAmt - _pmtAmt;
                                led.Bal_Amt = OBAmt;
                            }
                            else
                            {
                                OBAmt += _pmtAmt - _rptAmt;
                                led.Bal_Amt = OBAmt;
                            }
                        }
                        memLedger.AddRange(Trn);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return memLedger;
        }

        public async Task<LoanLedgerVM> GetLoanLedger(decimal loanId, DateTime fromDate, DateTime toDate, string brCode)
        {
            double PrlOS = 0;
            double PrlOD = 0;
            double IntBal = 0;
            double IODBal = 0;
            double PIBal = 0;
            LoanLedgerVM ledger = new LoanLedgerVM();
            LoanTransactionVM ob = new LoanTransactionVM();
            List<LoanTransactionVM> loanList = new List<LoanTransactionVM>();
            List<LoanTransactionVM> loanListFinal = new List<LoanTransactionVM>();
            try
            {
                ledger.Header = await GetLoanHeader(loanId, brCode);
                ledger.LoanROI = await GetLoanLedgerROIAndPI(loanId, brCode);
                ledger.LoanInst = await GetLoanLedgerInstalment(loanId, brCode);
                ob = await GetLoanOBOnLoanId(loanId, fromDate, brCode);

                loanList = await GetLoanDetailsBetweenDatesOnLoanId(loanId, fromDate, toDate, brCode);
                if (ob != null)
                    loanListFinal.Add(ob);
                if (loanList != null)
                    loanListFinal.AddRange(loanList);
                ledger.Transactions = loanListFinal;
                foreach (LoanTransactionVM single in loanListFinal)
                {
                    PrlOS += single.Disb_Amt - single.PrlColl_Amt;
                    PrlOD += single.Prl_Dem - single.PrlColl_Amt;
                    IntBal += single.IntCalc_Amt - single.IntColl_Amt;
                    IODBal += single.IODCalc_Amt - single.IODColl_Amt;
                    PIBal += single.PICalc_Amt - single.PIColl_Amt;
                    single.Prl_OS = PrlOS;
                    single.Prl_OD = PrlOD;
                    single.Int_Bal = IntBal;
                    single.IOD_Bal = IODBal;
                    single.PI_Bal = PIBal;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return ledger;
        }

        public async Task<LoanHeaderDetailsVM> GetLoanHeader(decimal LoanId, string brCode)
        {
            LoanHeaderDetailsVM head = new LoanHeaderDetailsVM();
            try
            {
                #region query
                //var result = await CSISContext.Database.SqlQueryRaw<LoanHeaderDetailsVM>(
                //    @"SELECT Master.Scheme_Id  as SchemeId,
                //            Master.Loan_No  as LoanNo,
                //            Master.Mem_Id  as MemId,
                //            Master.Pur_Id  as PurposeId,
                //            Master.San_amt  as LoanAmount,
                //            Master.San_date  as DisbursementDate,
                //            Master.Prl_Prd  as PrlPrd,
                //            Master.Int_Prd  as IntPrd,
                //            Master.roi  as RateOfInterest,
                //            Master.pi  as PenalRate,
                //            Master.Inst_Amt  as InstalmentAmount,
                //            Master.FirstInt_DueDate  as FirstIntDueDate,
                //            Master.FirstPrl_DueDate  as FirstPrlDueDate,
                //            Scheme.Scheme_Name as SchemeName
                //          FROM Loan_Master as Master 
                //          INNER JOIN Loan_Schemes as Scheme on Master.Scheme_Id = Scheme.Scheme_Id 
                //          WHERE Master.Loan_id = @loanId AND Master.Loan_Delete = false 
                //          INNER JOIN Loan_Schemes as Scheme on Master.Scheme_Id = Scheme.Scheme_Id 
                //          AND Loan_Master.brCode = @brCode AND Loan_Schemes.BrCode = @brCode"
                //    , new NpgsqlParameter("@loanId", LoanId)
                //    , new NpgsqlParameter("@brCode", brCode)).FirstOrDefaultAsync();
                #endregion 

                var result = await (from master in CSISContext.Loan_Master
                                    join scheme in CSISContext.Loan_Schemes
                                       on master.Scheme_Id equals scheme.Scheme_Id
                                    where master.Loan_Id == LoanId
                                       && master.Loan_Delete == false
                                       && master.BrCode == brCode
                                       && scheme.BrCode == brCode
                                    select new LoanHeaderDetailsVM
                                    {
                                        SchemeId = master.Scheme_Id,
                                        LoanNo = master.Loan_No,
                                        MemId = master.Mem_Id,
                                        PurposeId = master.Pur_Id,
                                        LoanAmount = master.San_Amt,
                                        DisbursementDate = master.San_Date,
                                        PrlPrd = master.Prl_Prd,
                                        IntPrd = master.Int_Prd,
                                        RateOfInterest = master.Roi,
                                        PenalRate = master.Pi,
                                        InstalmentAmount = master.Inst_Amt,
                                        FirstIntDueDate = master.FirstInt_DueDate,
                                        FirstPrlDueDate = master.FirstPrl_DueDate,
                                        SchemeName = scheme.Scheme_Name
                                    }).FirstOrDefaultAsync();

                if (result != null && result.SchemeId > 0) head = result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return head;
        }

        public async Task<List<Loan_Roi>> GetLoanLedgerROIAndPI(decimal loanid, string brCode)
        {
            List<Loan_Roi> roiList = new List<Loan_Roi>();
            try
            {
                var result = await CSISContext.Database.SqlQueryRaw<Loan_Roi>(
                    @"SELECT * FROM Loan_roi WHERE Loan_id = @loanId AND loanroi_delete = false AND Loan_Roi.BrCode = @brCode"
                            , new NpgsqlParameter("@loanid", loanid)
                            , new NpgsqlParameter("brCode", brCode)).ToListAsync();
                if (result != null && result.Any()) roiList = result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return roiList;
        }

        public async Task<List<Loan_Inst>> GetLoanLedgerInstalment(decimal loanid, string brCode)
        {
            List<Loan_Inst> instList = new List<Loan_Inst>();
            try
            {
                var result = await CSISContext.Database.SqlQueryRaw<Loan_Inst>(
                    @"SELECT * FROM Loan_Inst WHERE Loan_id = @loanId AND inst_delete = false AN Loan_Inst.BrCode = @brCode"
                    , new NpgsqlParameter("@loanId", loanid)
                    , new NpgsqlParameter("@brCode", brCode)).ToListAsync();
                if (result != null && result.Any()) instList = result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return instList;
        }

        public async Task<LoanTransactionVM> GetLoanOBOnLoanId(decimal LoanId, DateTime FromDate, string brCode)
        {
            LoanTransactionVM ob = new LoanTransactionVM();
            try
            {
                #region query
                //var result = await CSISContext.Database.SqlQueryRaw<LoanTransactionVM>(
                //        @"SELECT Trn.Loan_id,
                //            Max(Trn.Trn_Date) as Trn_Date,
                //            Sum(Trn.Disb_Amt) as Disb_Amt,
                //            Max(Trn.Due_Date) as Due_Date,
                //            Max(Trn.Disb_Date) as Disb_Date,
                //            Sum(Trn.Prl_Sched) as Prl_Sched,
                //            Sum(Trn.Prl_Dem) as Prl_Dem,
                //            Sum(Trn.PICalc_Amt) as PICalc_Amt,
                //            Max(Trn.PICalc_Date) as PICalc_Date,
                //            Sum(Trn.IODCalc_Amt) as IODCalc_Amt,
                //            Max(Trn.IODCalc_Date) as IODCalc_Date,
                //            Sum(Trn.IntCalc_Amt) as IntCalc_Amt,
                //            Max(Trn.IntCalc_Date) as IntCalc_Date,
                //            Sum(Trn.PrlColl_Amt) as PrlColl_Amt,
                //            Sum(Trn.IntColl_Amt) as IntColl_Amt,
                //            Sum(Trn.IODColl_Amt) as IODColl_Amt,
                //            Sum(Trn.PIColl_Amt) as PIColl_Amt
                //        FROM 
                //            Loan_Trn as Trn 
                //            WHERE Trn.TrnTr_Delete =  AND Trn.Loan_id = @loanId AND Trn.Trn_Date < @fromDate
                //            AND Loan_Trn.BrCode = @brCode
                //        GROUP BY Trn.Loan_id"
                //    , new NpgsqlParameter("@loanId", LoanId)
                //    , new NpgsqlParameter("@fromDate", FromDate)
                //    , new NpgsqlParameter("@brCode", brCode)).FirstOrDefaultAsync();
                #endregion 
                var result = await CSISContext.Loan_Trn
                        .Where(trn => trn.TrnTr_Delete == false &&
                                      trn.Loan_Id == LoanId &&
                                      trn.Trn_Date < FromDate &&
                                      trn.BrCode == brCode)
                        .GroupBy(trn => trn.Loan_Id)
                        .Select(g => new LoanTransactionVM
                        {
                            Loan_Id = g.Key,
                            Trn_Date = g.Max(trn => trn.Trn_Date),
                            Disb_Amt = g.Sum(trn => trn.Disb_Amt),
                            Due_Date = g.Max(trn => trn.Due_Date),
                            Disb_Date = g.Max(trn => trn.Disb_Date),
                            Prl_Sched = g.Sum(trn => trn.Prl_Sched),
                            Prl_Dem = g.Sum(trn => trn.Prl_Dem),
                            PICalc_Amt = g.Sum(trn => trn.PICalc_Amt),
                            PICalc_Date = g.Max(trn => trn.PICalc_Date),
                            IODCalc_Amt = g.Sum(trn => trn.IODCalc_Amt),
                            IODCalc_Date = g.Max(trn => trn.IODCalc_Date),
                            IntCalc_Amt = g.Sum(trn => trn.IntCalc_Amt),
                            IntCalc_Date = g.Max(trn => trn.IntCalc_Date),
                            PrlColl_Amt = g.Sum(trn => trn.PrlColl_Amt),
                            IntColl_Amt = g.Sum(trn => trn.IntColl_Amt),
                            IODColl_Amt = g.Sum(trn => trn.IODColl_Amt),
                            PIColl_Amt = g.Sum(trn => trn.PIColl_Amt)
                        })
                        .FirstOrDefaultAsync();
                if (result != null && result.Loan_Id > 0)
                {
                    ob = result;
                    ob.Trn_Status = "OB";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return ob;
        }

        public async Task<List<LoanTransactionVM>> GetLoanDetailsBetweenDatesOnLoanId(decimal LoanId, DateTime FromDate, DateTime ToDate, string brCode)
        {
            List<LoanTransactionVM> loanList = new List<LoanTransactionVM>();
            try
            {
                #region query
                //loanList = await CSISContext.Database.SqlQueryRaw<LoanTransactionVM>(
                //        @"SELECT 
                //            Trn.voc_id,
                //            Trn.Loan_id,
                //            Trn.Trn_Status,
                //            Trn.Trn_Date,
                //            Trn.Due_Date,
                //            Trn.Disb_Date,
                //            ISNULL(Trn.Disb_Amt,0) AS Disb_Amt,
                //            ISNULL(Trn.Prl_Sched,0) AS Prl_Sched,
                //            ISNULL(Trn.Prl_Dem,0) AS Prl_Dem,
                //            ISNULL(Trn.PICalc_Amt,0) AS PICalc_Amt,
                //            Trn.PICalc_Date,
                //            ISNULL(Trn.IODCalc_Amt,0) AS IODCalc_Amt,
                //            Trn.IODCalc_Date,
                //            ISNULL(Trn.IntCalc_Amt,0) AS IntCalc_Amt,
                //            Trn.IntCalc_Date,
                //            ISNULL(Trn.PrlColl_Amt,0) AS PrlColl_Amt,
                //            ISNULL(Trn.IntColl_Amt,0) AS IntColl_Amt,
                //            ISNULL(Trn.IODColl_Amt,0) AS IODColl_Amt,
                //            ISNULL(Trn.PIColl_Amt,0) AS PIColl_Amt,
                //            Voc.Voc_No                        
                //        FROM 
                //            Loan_Trn as Trn 
                //            LEFT JOIN Fin_Voucher as Voc ON Trn.Voc_Id = Voc.Voc_Id
                //            WHERE Trn.Loan_id = @loanId AND (CAST(Trn.Trn_Date AS date) 
                //            BETWEEN @fromDate AND @toDate) AND Trn.TrnTr_Delete = false AND Loan_Trn.BrCode = @brCode
                //        ORDER BY Trn.Trn_Date ASC, Trn.trn_slno ASC"
                //    , new NpgsqlParameter("@loanId", LoanId)
                //    , new NpgsqlParameter("@fromDate", FromDate.ToString("yyyy-MM-dd"))
                //    , new NpgsqlParameter("@toDate", ToDate.Date.ToString("yyyy-MM-dd"))
                //    , new NpgsqlParameter("@brCode", brCode)).ToListAsync();
                #endregion 

                var result = await (from trn in CSISContext.Loan_Trn
                                      join voc in CSISContext.Fin_Voucher
                                         on trn.Voc_Id equals voc.Voc_Id into vocJoin
                                      from voc in vocJoin.DefaultIfEmpty()
                                      where trn.Loan_Id == LoanId
                                         && trn.TrnTr_Delete == false
                                         && trn.BrCode == brCode
                                         && trn.Trn_Date.Date >= FromDate.Date
                                         && trn.Trn_Date.Date <= ToDate.Date
                                      orderby trn.Trn_Date ascending, trn.Trn_SlNo ascending
                                      select new LoanTransactionVM
                                      {
                                          Voc_Id = trn.Voc_Id,
                                          Loan_Id = trn.Loan_Id,
                                          Trn_Status = trn.Trn_Status,
                                          Trn_Date = trn.Trn_Date,
                                          Due_Date = trn.Due_Date,
                                          Disb_Date = trn.Disb_Date,
                                          Disb_Amt = trn.Disb_Amt,
                                          Prl_Sched = trn.Prl_Sched,
                                          Prl_Dem = trn.Prl_Dem,
                                          PICalc_Amt = trn.PICalc_Amt,
                                          PICalc_Date = trn.PICalc_Date,
                                          IODCalc_Amt = trn.IODCalc_Amt,
                                          IODCalc_Date = trn.IODCalc_Date,
                                          IntCalc_Amt = trn.IntCalc_Amt,
                                          IntCalc_Date = trn.IntCalc_Date,
                                          PrlColl_Amt = trn.PrlColl_Amt,
                                          IntColl_Amt = trn.IntColl_Amt,
                                          IODColl_Amt = trn.IODColl_Amt,
                                          PIColl_Amt = trn.PIColl_Amt,
                                          Voc_No = voc != null ? voc.Voc_No : null,
                                          Yr_Id = trn.Yr_Id ,
                                          BrCode = trn.BrCode 
                                      }).ToListAsync();

                if(result != null && result.Any())
                {
                    loanList = result.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return loanList;
        }

        public async Task<TDLedgerVM> GetTDLedgerVM(decimal TDId, DateTime FromDate, DateTime ToDate, string brCode)
        {
            TDLedgerVM ledger = new TDLedgerVM();
            TermDeposit_Master master = new TermDeposit_Master();
            List<TDLedgerTrn> tdList = new List<TDLedgerTrn>();
            try
            {
               var singleResult = CSISContext.TermDeposit_Master
    .               FirstOrDefault(td => td.TD_Id == TDId &&
                         td.BrCode == brCode &&
                         td.TD_Delete == false);
                if (singleResult != null && singleResult.TD_Id > 0) master = singleResult;

                var trnOB =await (from trn in CSISContext.TermDeposit_Trn
                            where trn.TD_Id == TDId && trn.Trn_Date < FromDate && trn.BrCode == brCode 
                            group trn by trn.TD_Id into g
                            select new TDLedgerTrn
                            {
                                TD_Id = g.Key,
                                Trn_Date = g.Min(x=> x.Trn_Date),
                                Voc_No = "OB",
                                DepositReceiptAmount = g.Sum(x=> x.DepositReceiptAmount),
                                InterestCalculatedAmount = g.Sum(x=> x.InterestCalculatedAmount ),
                                InterestPaidAmount = g.Sum(x=> x.InterestPaidAmount),
                                PenalCalculatedAmount = g.Sum(x=> x.PenalCalculatedAmount ),
                                PenalAppliedDate = g.Max(x=> x.PenalAppliedDate),
                                PenalReceivedAmount = g.Sum(x=> x.PenalReceivedAmount),
                                DepositPaidAmount = g.Sum(x=> x.DepositPaidAmount),
                                Voc_Id = 0
                            }).FirstOrDefaultAsync();
                if (trnOB != null && trnOB.TD_Id >0) tdList.Add(trnOB); 

                var trnList = await (from tt in CSISContext.TermDeposit_Trn
                       join fv in CSISContext.Fin_Voucher on tt.Voc_Id equals fv.Voc_Id into voucherGroup
                       from voucher in voucherGroup.DefaultIfEmpty()  // LEFT JOIN
                       where tt.TD_Id == TDId
                             && tt.Trn_Date >= FromDate
                             && tt.Trn_Date <= ToDate
                             && tt.TD_Delete == false
                       orderby tt.Trn_Date ascending, tt.Trn_SlNo ascending
                       select new TDLedgerTrn
                       {
                           TD_Id = tt.TD_Id,
                           Trn_Date = tt.Trn_Date,
                           Voc_No = voucher.Voc_No,  // This will be null if no matching voucher
                           DepositReceiptAmount = tt.DepositReceiptAmount,
                           InterestCalculatedAmount = tt.InterestCalculatedAmount,
                           InterestAppliedDate = tt.InterestAppliedDate,
                           InterestPaidAmount = tt.InterestPaidAmount,
                           DepositPaidAmount = tt.DepositPaidAmount,
                           PenalCalculatedAmount = tt.PenalCalculatedAmount,
                           PenalAppliedDate = tt.PenalAppliedDate,
                           PenalReceivedAmount = tt.PenalReceivedAmount,
                           Voc_Id = tt.Voc_Id,
                           Yr_Id = tt.Yr_Id,
                           BrCode = tt.BrCode 
                       }).ToListAsync();
                if(trnList != null && trnList.Any()) tdList.AddRange(trnList);

                ledger.TDMaster = master;
                ledger.TDScheme_Name = CSISContext.TermDeposit_Schemes.Where(x => x.TDScheme_Id == master.TDScheme_Id).Select(x => x.TDScheme_Name).FirstOrDefault(); /// TDSchemeName;
                ledger.TDTrnList = tdList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            if (ledger != null)
            {
                foreach (var single in ledger.TDTrnList!)
                {
                    single.InterestPayableAmount = single.InterestCalculatedAmount - single.InterestPaidAmount;
                }
            }

            return ledger!;
        }
    }
}
