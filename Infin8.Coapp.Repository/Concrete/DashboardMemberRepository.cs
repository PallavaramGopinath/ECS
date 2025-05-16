using Infin8.Coapp.Dto;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infin8.Coapp.Repository
{
    public class DashboardMemberRepository : Repository<MemberDashBoardAccountsDto>, IDashboardMemberRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public DashboardMemberRepository(CSISContext context) : base(context)
        {

        }
        public List<MemberDashBoardAccountsDto> GetMemberDashBoardAccounts(decimal id, DateTime fromDate, DateTime toDate)
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

        public List<MemberDashBoardAccountsDto> DashboardAccounts(decimal id, DateTime fromDate, DateTime toDate)
        {
            List<MemberDashBoardAccountsDto> membersAccountsList = new List<MemberDashBoardAccountsDto>();
            try
            {
                var db1 =
                (from trn in CSISContext.Mem_Trn
                 join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                 where trn.Mem_Id == id && !trn.MemTrn_Delete && trn.Trn_Date < fromDate
                 group trn by trn.Trn_Type into g
                 where g.Key == 3
                 select new MemberDashBoardAccountsDto
                 {
                     Led_Name = "Share Capital",
                     Trn_Type = g.Key,
                     OB = g.Sum(x => x.Rpt_Amt) - g.Sum(x => x.Pmt_Amt),
                     Rpt_Amt = 0,
                     Pmt_Amt = 0
                 })
                .Union(
                from trn in CSISContext.Mem_Trn
                join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                where trn.Mem_Id == id && !trn.MemTrn_Delete && trn.Trn_Date < fromDate
                group trn by trn.Trn_Type into g
                where g.Key == 7
                select new MemberDashBoardAccountsDto
                {
                    Led_Name = "SB Account",
                    Trn_Type = g.Key,
                    OB = g.Sum(x => x.Rpt_Amt) - g.Sum(x => x.Pmt_Amt),
                    Rpt_Amt = 0,
                    Pmt_Amt = 0
                })
                .Union(
                from trn in CSISContext.Mem_Trn
                join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                where trn.Mem_Id == id && !trn.MemTrn_Delete && trn.Trn_Date < fromDate
                group trn by trn.Trn_Type into g
                where new[] { 2, 6 }.Contains(g.Key)
                select new MemberDashBoardAccountsDto
                {
                    Led_Name = "Suspense Creditor",
                    Trn_Type = g.Key,
                    OB = g.Sum(x => x.Rpt_Amt) - g.Sum(x => x.Pmt_Amt),
                    Rpt_Amt = 0,
                    Pmt_Amt = 0
                })
                .Union(
                from trn in CSISContext.Mem_Trn
                join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                where trn.Mem_Id == id && !trn.MemTrn_Delete && trn.Trn_Date < fromDate
                group trn by trn.Trn_Type into g
                where new[] { 1, 5 }.Contains(g.Key)
                select new MemberDashBoardAccountsDto
                {
                    Led_Name = "Suspense Debtor",
                    Trn_Type = g.Key,
                    OB = g.Sum(x => x.Pmt_Amt) - g.Sum(x => x.Rpt_Amt),
                    Rpt_Amt = 0,
                    Pmt_Amt = 0
                })
                .Union(
                from trn in CSISContext.Mem_Trn
                join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                where trn.Mem_Id == id && !trn.MemTrn_Delete && trn.Trn_Date < fromDate
                group trn by trn.Trn_Type into g
                where new[] { 2, 6 }.Contains(g.Key)
                select new MemberDashBoardAccountsDto
                {
                    Led_Name = "Interest on Suspense Creditor",
                    Trn_Type = g.Key,
                    OB = g.Sum(x => x.Pmt_Amt) - g.Sum(x => x.Rpt_Amt),
                    Rpt_Amt = 0,
                    Pmt_Amt = 0
                })
                .Union(
                from trn in CSISContext.Mem_Trn
                join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                where trn.Mem_Id == id && !trn.MemTrn_Delete && trn.Trn_Date < fromDate
                group trn by trn.Trn_Type into g
                where g.Key == 3
                select new MemberDashBoardAccountsDto
                {
                    Led_Name = "Dividend",
                    Trn_Type = g.Key,
                    OB = g.Sum(x => x.IntCalc_Amt) - g.Sum(x => x.IntPaid_Amt),
                    Rpt_Amt = 0,
                    Pmt_Amt = 0
                })
                .Union(
                 from trn in CSISContext.Mem_Trn
                 join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                 where trn.Mem_Id == id && !trn.MemTrn_Delete && (trn.Trn_Date >= fromDate && trn.Trn_Date <= toDate)
                 group trn by trn.Trn_Type into g
                 where g.Key == 3
                 select new MemberDashBoardAccountsDto
                 {
                     Led_Name = "Share Capital",
                     Trn_Type = g.Key,
                     OB = 0,
                     Rpt_Amt = g.Sum(x => x.Rpt_Amt),
                     Pmt_Amt = g.Sum(x => x.Pmt_Amt)
                 })
                .Union(
                 from trn in CSISContext.Mem_Trn
                 join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                 where trn.Mem_Id == id && !trn.MemTrn_Delete && (trn.Trn_Date >= fromDate && trn.Trn_Date <= toDate)
                 group trn by trn.Trn_Type into g
                 where g.Key == 7
                 select new MemberDashBoardAccountsDto
                 {
                     Led_Name = "SB Account",
                     Trn_Type = g.Key,
                     OB = 0,
                     Rpt_Amt = g.Sum(x => x.Rpt_Amt),
                     Pmt_Amt = g.Sum(x => x.Pmt_Amt)
                 })
                .Union(
                 from trn in CSISContext.Mem_Trn
                 join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                 where trn.Mem_Id == id && !trn.MemTrn_Delete && (trn.Trn_Date >= fromDate && trn.Trn_Date <= toDate)
                 group trn by trn.Trn_Type into g
                 where new[] { 2, 6 }.Contains(g.Key)
                 select new MemberDashBoardAccountsDto
                 {
                     Led_Name = "Suspense Creditor",
                     Trn_Type = g.Key,
                     OB = 0,
                     Rpt_Amt = g.Sum(x => x.Rpt_Amt),
                     Pmt_Amt = g.Sum(x => x.Pmt_Amt)
                 })
                .Union(
                 from trn in CSISContext.Mem_Trn
                 join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                 where trn.Mem_Id == id && !trn.MemTrn_Delete && (trn.Trn_Date >= fromDate && trn.Trn_Date <= toDate)
                 group trn by trn.Trn_Type into g
                 where new[] { 1, 5 }.Contains(g.Key)
                 select new MemberDashBoardAccountsDto
                 {
                     Led_Name = "Suspense Debtor",
                     Trn_Type = g.Key,
                     OB = 0,
                     Rpt_Amt = g.Sum(x => x.Rpt_Amt),
                     Pmt_Amt = g.Sum(x => x.Pmt_Amt)
                 })
                .Union(
                 from trn in CSISContext.Mem_Trn
                 join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                 where trn.Mem_Id == id && !trn.MemTrn_Delete && (trn.Trn_Date >= fromDate && trn.Trn_Date <= toDate)
                 group trn by trn.Trn_Type into g
                 where new[] { 2, 6 }.Contains(g.Key)
                 select new MemberDashBoardAccountsDto
                 {
                     Led_Name = "Interest on Suspense Creditor",
                     Trn_Type = g.Key,
                     OB = 0,
                     Rpt_Amt = g.Sum(x => x.IntCalc_Amt),
                     Pmt_Amt = g.Sum(x => x.IntPaid_Amt)
                 })
                .Union(
                 from trn in CSISContext.Mem_Trn
                 join master in CSISContext.mem_master on trn.Mem_Id equals master.mem_id
                 where trn.Mem_Id == id && !trn.MemTrn_Delete && (trn.Trn_Date >= fromDate && trn.Trn_Date <= toDate)
                 group trn by trn.Trn_Type into g
                 where g.Key == 3
                 select new MemberDashBoardAccountsDto
                 {
                     Led_Name = "Dividend",
                     Trn_Type = g.Key,
                     OB = 0,
                     Rpt_Amt = g.Sum(x => x.IntCalc_Amt),
                     Pmt_Amt = g.Sum(x => x.IntPaid_Amt)
                 }).ToList();

                var result =
                (from record in db1
                 group record by new { record.Led_Name, record.Trn_Type } into g
                 orderby g.Key.Trn_Type descending
                 select new MemberDashBoardAccountsDto
                 {
                     Led_Name = g.Key.Led_Name,
                     Trn_Type = g.Key.Trn_Type,
                     OB = g.Sum(x => x.OB),
                     Rpt_Amt = g.Sum(x => x.Rpt_Amt),
                     Pmt_Amt = g.Sum(x => x.Pmt_Amt)
                 }).ToList();
                if (result != null) membersAccountsList = result;
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
            }
            return membersAccountsList;
        }
    }
}
