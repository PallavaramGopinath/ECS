using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class ReportsMemberRepository : Repository<Reports_Master>, IReportsMemberRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public ReportsMemberRepository(CSISContext context) : base(context)
        {
        }

        public async Task<List<rptMemberTrn>> GetRptMemberTrn(DateTime fromDate, DateTime toDate, int trnType, string brCode)
        {
            //List<rptMemberTrn> memTrnList = new List<rptMemberTrn>();
            var memTrnList = new List<rptMemberTrn>();
            try
            {
                #region linq
                // LINQ conversion of the SQL query - revised approach to avoid Union issues

                if (trnType == 2 || trnType == 3 || trnType == 6) // type 2=dueby, type 3=share capital, type 6=staff due by
                {
                    // Create a query to get all required data
                    var query = await (from mt in CSISContext.Mem_Trn
                                       join mm in CSISContext.mem_master on mt.Mem_Id equals mm.mem_id
                                       join fl in CSISContext.Fin_Ledger on mt.Led_Id equals fl.Led_Id
                                       where mt.Trn_Type == trnType && mt.MemTrn_Delete == false
                                       select new { mt, mm, fl }).ToListAsync();

                    // Get opening balances
                    var openingBalances = query
                        .Where(x => x.mt.Trn_Date < fromDate && x.mt.IntCalc_Amt == 0 && x.mt.IntPaid_Amt == 0)
                        .GroupBy(x => new { x.mt.Mem_Id, x.mt.Led_Id, x.mt.Trn_Type, x.mm.memberno, x.mm.perno, x.mm.membername, x.fl.Led_Name })
                        .Select(g => new rptMemberTrn
                        {
                            Mem_Id = g.Key.Mem_Id,
                            Led_Id = g.Key.Led_Id,
                            Trn_Type = (byte)g.Key.Trn_Type,
                            Trn_Date = g.Max(x => x.mt.Trn_Date),
                            MemberNo = g.Key.memberno,
                            PerNo = g.Key.perno,
                            MemberName = g.Key.membername,
                            Led_Name = g.Key.Led_Name,
                            Amt_OB = (double)(g.Sum(x => x.mt.Rpt_Amt) - g.Sum(x => x.mt.Pmt_Amt)),
                            Rpt_Amt = 0,
                            Pmt_Amt = 0,
                            Trn_SlNo = 0
                        })
                        .Where(x => x.Amt_OB > 0)
                        .ToList();

                    // Get transactions within date range
                    var transactions = query
                        .Where(x => x.mt.Trn_Date >= fromDate && x.mt.Trn_Date <= toDate && (x.mt.Rpt_Amt > 0 || x.mt.Pmt_Amt > 0))
                        .Select(x => new rptMemberTrn
                        {
                            Mem_Id = x.mt.Mem_Id,
                            Led_Id = x.mt.Led_Id,
                            Trn_Type = x.mt.Trn_Type,
                            Trn_Date = x.mt.Trn_Date,
                            MemberNo = x.mm.memberno,
                            PerNo = x.mm.perno,
                            MemberName = x.mm.membername,
                            Led_Name = x.fl.Led_Name,
                            Amt_OB = 0,
                            Rpt_Amt = x.mt.Rpt_Amt,
                            Pmt_Amt = x.mt.Pmt_Amt,
                            Trn_SlNo = x.mt.Trn_SlNo
                        })
                        .ToList();

                    // Combine results (avoid using Union)
                    memTrnList = openingBalances.Concat(transactions)
                        .OrderBy(r => r.Led_Id)
                        .ThenBy(r => r.Mem_Id)
                        .ThenBy(r => r.Trn_Date)
                        .ThenBy(r => r.Trn_SlNo)
                        .ToList();
                }
                else if (trnType == 1 || trnType == 5) // type 1=due to, type 5=staff due to
                {
                    // Create a query to get all required data
                    var query = await (from mt in CSISContext.Mem_Trn
                                       join mm in CSISContext.mem_master on mt.Mem_Id equals mm.mem_id
                                       join fl in CSISContext.Fin_Ledger on mt.Led_Id equals fl.Led_Id
                                       where mt.Trn_Type == trnType && mt.MemTrn_Delete == false
                                       && mt.BrCode == brCode && mt.Voc_Status == "V"
                                       && mm.brcode == brCode
                                       && fl.BrCode == brCode 
                                       select new { mt, mm, fl }).ToListAsync();

                    // Get opening balances with different formula
                    var openingBalances = query
                        .Where(x => x.mt.Trn_Date < fromDate)
                        .GroupBy(x => new { x.mt.Mem_Id, x.mt.Led_Id, x.mt.Trn_Type, x.mm.memberno, x.mm.perno, x.mm.membername, x.fl.Led_Name })
                        .Select(g => new rptMemberTrn
                        {
                            Mem_Id = g.Key.Mem_Id,
                            Led_Id = g.Key.Led_Id,
                            Trn_Type = (byte)g.Key.Trn_Type,
                            Trn_Date = g.Max(x => x.mt.Trn_Date),
                            MemberNo = g.Key.memberno,
                            PerNo = g.Key.perno,
                            MemberName = g.Key.membername,
                            Led_Name = g.Key.Led_Name,
                            Amt_OB = (double)(g.Sum(x => x.mt.Pmt_Amt) - g.Sum(x => x.mt.Rpt_Amt)),
                            Rpt_Amt = 0,
                            Pmt_Amt = 0,
                            Trn_SlNo = 0
                        })
                        .Where(x => x.Amt_OB > 0)
                        .ToList();

                    // Get transactions within date range
                    var transactions = query
                        .Where(x => x.mt.Trn_Date >= fromDate && x.mt.Trn_Date <= toDate && (x.mt.Rpt_Amt > 0 || x.mt.Pmt_Amt > 0))
                        .Select(x => new rptMemberTrn
                        {
                            Mem_Id = x.mt.Mem_Id,
                            Led_Id = x.mt.Led_Id,
                            Trn_Type = x.mt.Trn_Type,
                            Trn_Date = x.mt.Trn_Date,
                            MemberNo = x.mm.memberno,
                            PerNo = x.mm.perno,
                            MemberName = x.mm.membername,
                            Led_Name = x.fl.Led_Name,
                            Amt_OB = 0,
                            Rpt_Amt = x.mt.Rpt_Amt,
                            Pmt_Amt = x.mt.Pmt_Amt,
                            Trn_SlNo = x.mt.Trn_SlNo
                        })
                        .ToList();

                    // Combine results (avoid using Union)
                    memTrnList = openingBalances.Concat(transactions)
                        .OrderBy(r => r.Led_Id)
                        .ThenBy(r => r.Mem_Id)
                        .ThenBy(r => r.Trn_Date)
                        .ThenBy(r => r.Trn_SlNo)
                        .ToList();
                }

                #endregion 

                var memTrnTmp = memTrnList.OrderBy(x => x.Led_Id).ThenBy(x => x.Mem_Id).ThenBy(x => x.Trn_Date).ThenBy(x => x.Trn_SlNo).ToList();

                #region commented, this code moved to handler
                //double OB = 0, CB = 0;
                //decimal memId = 0, ledId = 0;
                //foreach (var trn in memTrnTmp)
                //{
                //    if (memId == trn.Mem_Id && ledId != trn.Led_Id)
                //    {
                //        ledId = trn.Led_Id;
                //        OB = trn.Amt_OB;
                //        CB = trn.Amt_OB;
                //    }

                //    if (memId != trn.Mem_Id && ledId != trn.Led_Id)
                //    {
                //        memId = trn.Mem_Id;
                //        ledId = trn.Led_Id;
                //        OB = trn.Amt_OB;
                //        CB = trn.Amt_OB;
                //    }

                //    if (memId != trn.Mem_Id && ledId == trn.Led_Id)
                //    {
                //        memId = trn.Mem_Id;
                //        OB = trn.Amt_OB;
                //        CB = trn.Amt_OB;
                //    }
                //    switch (trn.Trn_Type)
                //    {
                //        case 1:
                //        case 5:
                //            CB += Convert.ToDouble(trn.Pmt_Amt) - Convert.ToDouble(trn.Rpt_Amt);
                //            break;
                //        case 2:
                //        case 3:
                //        case 6:
                //            CB += Convert.ToDouble(trn.Rpt_Amt) - Convert.ToDouble(trn.Pmt_Amt);
                //            break;
                //    }
                //    trn.Amt_CB = CB;
                //}
                #endregion 

                if (memTrnTmp != null)
                {
                    memTrnList = memTrnTmp.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in fetching Member's transaction data " + ex.Message);
                memTrnList = new();
            }
            return memTrnList;
        }

        public async Task<List<rptMemberTrn>> GetRptMemberTrn(DateTime toDate, int trnType, string brCode)
        {
            var memTrnList = new List<rptMemberTrn>();
            try
            {
                #region old linq
                if (trnType == 2 || trnType == 3 || trnType == 6)    // type 2= dueby, type 3= share capital, type 6= staff due by
                {
                    memTrnList = await (from mt in CSISContext.Mem_Trn
                                        join fl in CSISContext.Fin_Ledger on mt.Led_Id equals fl.Led_Id
                                        join mm in CSISContext.mem_master on mt.Mem_Id equals mm.mem_id
                                        where mt.Trn_Type == trnType &&
                                              mt.Trn_Date <= toDate &&
                                              mt.MemTrn_Delete == false &&
                                              mm.membertype <= 4 &&
                                              mt.BrCode == brCode &&
                                              fl.BrCode == brCode &&
                                              mm.brcode == brCode
                                        group new { mt, fl, mm } by new
                                        {
                                            mt.Trn_Type,
                                            mt.Mem_Id,
                                            mt.Led_Id,
                                            fl.Led_Name,
                                            mm.memberno,
                                            mm.perno,
                                            mm.membername
                                        } into g
                                        // let balance = g.Sum(x => x.mt.Rpt_Amt) - g.Sum(x => x.mt.Pmt_Amt)
                                        where g.Sum(x => x.mt.Rpt_Amt) - g.Sum(x => x.mt.Pmt_Amt) > 0
                                        orderby g.Key.Led_Name, g.Key.memberno
                                        select new rptMemberTrn
                                        {
                                            Trn_Type = g.Key.Trn_Type,
                                            Mem_Id = g.Key.Mem_Id,
                                            Led_Id = g.Key.Led_Id,
                                            Amt_CB = g.Sum(x => x.mt.Rpt_Amt) - g.Sum(x => x.mt.Pmt_Amt), ///(double)balance,
                                            Led_Name = g.Key.Led_Name,
                                            MemberNo = g.Key.memberno,
                                            PerNo = g.Key.perno,
                                            MemberName = g.Key.membername
                                        }).ToListAsync();
                }
                else if (trnType == 1 || trnType == 5)   // type 1= due to, type 5= staff due to
                {
                    memTrnList = await (from mt in CSISContext.Mem_Trn
                                        join fl in CSISContext.Fin_Ledger on mt.Led_Id equals fl.Led_Id
                                        join mm in CSISContext.mem_master on mt.Mem_Id equals mm.mem_id
                                        where mt.Trn_Type == trnType &&
                                              mt.Trn_Date <= toDate &&
                                              mt.MemTrn_Delete == false &&
                                              mm.membertype <= 4 &&
                                              mt.BrCode == brCode &&
                                              fl.BrCode == brCode &&
                                              mm.brcode == brCode
                                        group new { mt, fl, mm } by new
                                        {
                                            mt.Trn_Type,
                                            mt.Mem_Id,
                                            mt.Led_Id,
                                            fl.Led_Name,
                                            mm.memberno,
                                            mm.perno,
                                            mm.membername
                                        } into g
                                        //let balance = g.Sum(x => x.mt.Pmt_Amt) - g.Sum(x => x.mt.Rpt_Amt)
                                        where g.Sum(x => x.mt.Pmt_Amt) - g.Sum(x => x.mt.Rpt_Amt) > 0
                                        orderby g.Key.Led_Name, g.Key.memberno
                                        select new rptMemberTrn
                                        {
                                            Trn_Type = g.Key.Trn_Type,
                                            Mem_Id = g.Key.Mem_Id,
                                            Led_Id = g.Key.Led_Id,
                                            Amt_CB = g.Sum(x => x.mt.Pmt_Amt) - g.Sum(x => x.mt.Rpt_Amt), ///(double)balance,
                                            Led_Name = g.Key.Led_Name,
                                            MemberNo = g.Key.memberno,
                                            PerNo = g.Key.perno,
                                            MemberName = g.Key.membername
                                        }).ToListAsync();
                }
                #endregion

                #region linq new
                //    // First part - Opening Balance query
                //    var openingBalanceQuery = GetOpeningBalanceQuery(toDate, trnType);

                //    // Second part - Transaction period query
                //    var transactionPeriodQuery = GetTransactionPeriodQuery(fromDate , toDate, trnType);

                //    // Union the two queries
                //    var unionQuery = openingBalanceQuery.Union(transactionPeriodQuery);

                //    // Execute the query
                //    var rawResults = unionQuery.ToList();

                //    // Group and aggregate the results
                //    memTrnList = (from mem in rawResults
                //                  group mem by new
                //                  {
                //                      mem.Mem_Id,
                //                      mem.memberNo,
                //                      mem.PerNo,
                //                      mem.memberName,
                //                      mem.Trn_Type,
                //                      mem.Led_Id,
                //                      mem.Led_Name
                //                  } into g
                //                  select new rptMemberTrn
                //                  {
                //                      Mem_Id = g.Key.Mem_Id,
                //                      memberNo = g.Key.memberNo,
                //                      PerNo = g.Key.PerNo,
                //                      memberName = g.Key.memberName,
                //                      Trn_Type = g.Key.Trn_Type,
                //                      Led_Id = g.Key.Led_Id,
                //                      Led_Name = g.Key.Led_Name,
                //                      Amt_OB = g.Sum(trn => trn.Amt_OB),
                //                      Rpt_Amt = g.Sum(trn => trn.Rpt_Amt),
                //                      Pmt_Amt = g.Sum(trn => trn.Pmt_Amt)
                //                  })
                //                  .OrderBy(x => x.Trn_Type)
                //                  .ThenBy(x => x.Mem_Id)
                //                  .ThenBy(x => x.Led_Id)
                //                  .ToList();

                //    // Calculate closing balance for each transaction
                //    foreach (var trn in memTrnList)
                //    {
                //        switch (trn.Trn_Type)
                //        {
                //            case 1: /// member due to
                //            case 5: /// staff due to
                //                trn.Amt_CB = trn.Amt_OB + Convert.ToDouble(trn.Pmt_Amt) - Convert.ToDouble(trn.Rpt_Amt);
                //                break;
                //            case 2: /// member due by
                //            case 3: /// share capital
                //            case 6: /// staff due by
                //                trn.Amt_CB = trn.Amt_OB + Convert.ToDouble(trn.Rpt_Amt) - Convert.ToDouble(trn.Pmt_Amt);
                //                break;
                //        }
                //    }
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in fecthing member shedule " + ex.Message);
            }
            return memTrnList;
        }

        private IQueryable<rptMemberTrn> GetOpeningBalanceQuery( DateTime toDate, int trnType)
        {
            switch (trnType)
            {
                case 1: /// member due to
                case 5: /// staff due to
                    return  (from mt in CSISContext.Mem_Trn
                            join mm in CSISContext.mem_master on mt.Mem_Id equals mm.mem_id
                            join fl in CSISContext.Fin_Ledger on mt.Led_Id equals fl.Led_Id
                            where mt.MemTrn_Delete == false &&
                                  mt.Trn_Date < toDate &&
                                  mt.Trn_Type == trnType
                            group new { mt, mm, fl } by new
                            {
                                mt.Mem_Id,
                                mm.memberno,
                                mm.perno,
                                mm.membername,
                                mt.Trn_Type,
                                mt.Led_Id,
                                fl.Led_Name
                            } into g
                            where g.Sum(x => x.mt.Pmt_Amt) - g.Sum(x => x.mt.Rpt_Amt) > 0
                            select new rptMemberTrn
                            {
                                Mem_Id = g.Key.Mem_Id,
                                MemberNo = g.Key.memberno,
                                PerNo = g.Key.perno,
                                MemberName = g.Key.membername,
                                Trn_Type = g.Key.Trn_Type,
                                Led_Id = g.Key.Led_Id,
                                Led_Name = g.Key.Led_Name,
                                Amt_OB = (double)(g.Sum(x => x.mt.Pmt_Amt) - g.Sum(x => x.mt.Rpt_Amt)),
                                Rpt_Amt = 0,
                                Pmt_Amt = 0
                            });

                case 2: /// member due by
                case 3: /// share capital
                case 6: /// staff due by
                    return  (from mt in CSISContext.Mem_Trn
                            join mm in CSISContext.mem_master on mt.Mem_Id equals mm.mem_id
                            join fl in CSISContext.Fin_Ledger on mt.Led_Id equals fl.Led_Id
                            where mt.MemTrn_Delete == false &&
                                  mt.Trn_Date < toDate &&
                                  mt.Trn_Type == trnType
                            group new { mt, mm, fl } by new
                            {
                                mt.Mem_Id,
                                mm.memberno,
                                mm.perno,
                                mm.membername,
                                mt.Trn_Type,
                                mt.Led_Id,
                                fl.Led_Name
                            } into g
                            where g.Sum(x => x.mt.Rpt_Amt) - g.Sum(x => x.mt.Pmt_Amt) > 0
                            select new rptMemberTrn
                            {
                                Mem_Id = g.Key.Mem_Id,
                                MemberNo = g.Key.memberno,
                                PerNo = g.Key.perno,
                                MemberName = g.Key.membername,
                                Trn_Type = g.Key.Trn_Type,
                                Led_Id = g.Key.Led_Id,
                                Led_Name = g.Key.Led_Name,
                                Amt_OB = (double)(g.Sum(x => x.mt.Rpt_Amt) - g.Sum(x => x.mt.Pmt_Amt)),
                                Rpt_Amt = 0,
                                Pmt_Amt = 0
                            });

                default:
                    return CSISContext.Mem_Trn.Where(x => false).Select(x => new rptMemberTrn()); // Empty query
            }
        }

        private IQueryable<rptMemberTrn> GetTransactionPeriodQuery( DateTime fromDate, DateTime toDate, int trnType)
        {
            return  (from mt in CSISContext.Mem_Trn
                    join mm in CSISContext.mem_master on mt.Mem_Id equals mm.mem_id
                    join fl in CSISContext.Fin_Ledger on mt.Led_Id equals fl.Led_Id
                    where mt.MemTrn_Delete == false &&
                          mt.Trn_Date >= fromDate &&
                          mt.Trn_Date <= toDate &&
                          mt.Trn_Type == trnType
                    group new { mt, mm, fl } by new
                    {
                        mt.Mem_Id,
                        mm.memberno,
                        mm.perno,
                        mm.membername,
                        mt.Trn_Type,
                        mt.Led_Id,
                        fl.Led_Name
                    } into g
                    where g.Sum(x => x.mt.Rpt_Amt) > 0 || g.Sum(x => x.mt.Pmt_Amt) > 0
                    select new rptMemberTrn
                    {
                        Mem_Id = g.Key.Mem_Id,
                        MemberNo = g.Key.memberno,
                        PerNo = g.Key.perno,
                        MemberName = g.Key.membername,
                        Trn_Type = g.Key.Trn_Type,
                        Led_Id = g.Key.Led_Id,
                        Led_Name = g.Key.Led_Name,
                        Amt_OB = 0,
                        Rpt_Amt = (double)g.Sum(x => x.mt.Rpt_Amt),
                        Pmt_Amt = (double)g.Sum(x => x.mt.Pmt_Amt)
                    });
        }

        public async Task<List<rptMemberCancellation>> GetMemberCancellation(DateTime fromDate, DateTime toDate, string brCode)
        {
            List<rptMemberCancellation> memTrnList = new List<rptMemberCancellation>();
            try
            {
                decimal cashLedId = CSISContext.Map_General.Select(x => x.Cash_Led_Id).FirstOrDefault();

                var memTrnListTmp = await (from fv in CSISContext.Fin_Voucher
                                           join mm in CSISContext.mem_master on fv.Mem_Id equals mm.mem_id
                                           join gvb in CSISContext.Fin_Voucher_Bank on fv.Voc_Id equals gvb.Voc_Id into gj
                                           from gvb in gj.DefaultIfEmpty()
                                           join fvt in CSISContext.Fin_Voucher_Trn on fv.Voc_Id equals fvt.Voc_Id into fvtj
                                           from fvt in fvtj.DefaultIfEmpty()
                                           join fl in CSISContext.Fin_Ledger on fvt.Led_Id equals fl.Led_Id into flj
                                           from fl in flj.DefaultIfEmpty()
                                           where CSISContext.Fin_Voucher.Where(fvSub => fvSub.Voc_Delete == false &&
                                                                                    CSISContext.Fin_Voucher_Trn.Where(fvtrSub => fvtrSub.Status == "DP")
                                                                                           .Select(fvtrSub => fvtrSub.Voc_Id)
                                                                                           .Contains(fvSub.Voc_Id) &&
                                                                                    fvSub.Voc_Date >= fromDate && fvSub.Voc_Date <= toDate)
                                                                     .Select(fvSub => fvSub.Voc_Id)
                                                                     .Contains(fv.Voc_Id) &&
                                                 !CSISContext.Map_Banks.Select(mb => mb.Led_Id).Contains(fvt.Led_Id) &&
                                                 fvt.Led_Id != cashLedId
                                                 && fv.BrCode == brCode && fv.Voc_Status =="V"
                                                 && gvb.BrCode == brCode && gvb.Voc_Status == "V"
                                                 && fvt.BrCode == brCode && fvt.Voc_Status == "V"
                                           orderby mm.memberno ascending
                                           select new rptMemberCancellation
                                           {
                                               Mem_Id = mm.mem_id, // Assuming Mem_Id is available in Mem_Master
                                               MemberNo = mm.memberno,
                                               PerNo = mm.perno,
                                               MemberName = mm.membername,
                                               Token_PersonNo = mm.token_personno,
                                               Fvb_Cheque_No = gvb.Fvb_Cheque_No,
                                               Fvb_Cheque_Date = gvb.Fvb_Cheque_Date,
                                               Voc_Rpt = fvt.Voc_Rpt,
                                               Voc_Pmt = fvt.Voc_Pmt,
                                               Led_Name = fl.Led_Name
                                           }).ToListAsync();
                if (memTrnListTmp != null) memTrnList = memTrnListTmp;
            }
            catch (Exception)
            {
                throw;
            }
            return memTrnList;
        }

        public async Task<List<rptMemberList>> GetMemberList(DateTime asOnDate, List<int> memberTypeList, List<int> memberStatusList, string brCode)
        {
            List<rptMemberList> memList = new List<rptMemberList>();
            try
            {
                var memListTmp = await (from mm in CSISContext.mem_master
                                        join bm in CSISContext.Bank_Master on mm.bankname equals bm.Bank_Name into bankJoin
                                        from bm in bankJoin.DefaultIfEmpty()
                                        join ra in CSISContext.Refer_Area on mm.prearea_id equals ra.Area_Id into areaJoin
                                        from ra in areaJoin.DefaultIfEmpty()
                                        where mm.memberdelete == false &&
                                              memberTypeList.Contains(mm.membertype) &&
                                              memberStatusList.Contains(mm.memberstatus) &&
                                              mm.isaccountclosed == false &&
                                              (mm.admissiondate == null || mm.admissiondate <= asOnDate)
                                              && mm.brcode == brCode 
                                              && ra.BrCode == brCode 
                                        orderby mm.membertype, mm.memberno
                                        select new rptMemberList
                                        {
                                            Mem_Id = mm.mem_id,
                                            MemberNo = mm.memberno,
                                            MemberType = mm.membertype,
                                            PerNo = mm.perno,
                                            MemberName = mm.membername,
                                            FatherName = mm.fathername,
                                            Gender = mm.gender,
                                            PreAdd1 = mm.preadd1,
                                            PreAdd2 = mm.preadd2,
                                            PreAdd3 = mm.preadd3,
                                            PrePin = mm.prepin,
                                            AreaName = ra.Area_Name, // Corrected this line
                                            SBAccountNo = mm.sbaccountno,
                                            BankName = mm.bankname, // Keep BankName from Mem_Master
                                            IFSCCode = mm.ifsccode,
                                            GPF_No = mm.gpf_no,
                                            PANNo = mm.panno,
                                            AadharNo = mm.aadharno,
                                            Address = mm.preadd1 + " " + mm.preadd2 + " " + mm.preadd3 //Combined Address
                                        }).ToListAsync();
                foreach (var mem in memListTmp)
                {
                    if (mem.Gender == 1)
                        mem.GenderName = "Male";
                    else
                        mem.GenderName = "Female";
                    if (mem.MemberType == 1)
                        mem.MemberTypeName = "A-Class Member";
                    if (mem.MemberType == 2)
                        mem.MemberTypeName = "Associate Member";
                    if (mem.PreAdd1 != null)
                        mem.Address = mem.PreAdd1;
                    if (mem.PreAdd2 != null)
                        if (mem.PreAdd2.Length > 0)
                            mem.Address += ", " + mem.PreAdd2;
                    if (mem.PreAdd3 != null)
                        if (mem.PreAdd3.Length > 0)
                            mem.Address += ", " + mem.PreAdd3;
                    if (mem.PrePin != null)
                        if (mem.PrePin.Length > 0)
                            mem.Address += ", " + mem.PrePin;
                    if (mem.AreaName != null)
                        if (mem.AreaName.Length > 0)
                            mem.Address += ", " + mem.AreaName;
                }
                if (memListTmp != null) memList = memListTmp;
            }
            catch (Exception)
            {
                throw;
            }
            return memList;
        }

        public async Task<List<rptMemberRegister>> GetMemberRegister(int memId)
        {
            List<rptMemberRegister> memTrnList = new List<rptMemberRegister>();
            try
            {
                var memTrnListTmp = await (from mt in CSISContext.Mem_Trn
                                           join mm in CSISContext.mem_master on mt.Mem_Id equals mm.mem_id
                                           join ra in CSISContext.Refer_Area on mm.prearea_id equals ra.Area_Id into ra_join
                                           from ra in ra_join.DefaultIfEmpty()
                                           where mm.mem_id == memId && mt.Trn_Type == 3 && mt.MemTrn_Delete == false
                                           && mt.Voc_Status =="V"
                                           orderby mt.Mem_Id, mt.Trn_Date, mt.Trn_SlNo
                                           select new rptMemberRegister
                                           {
                                               Mem_Id = mt.Mem_Id,
                                               MemberNo = mm.memberno,
                                               PerNo = mm.perno,
                                               MemberName = mm.membername,
                                               FatherName = mm.fathername,
                                               Dob = mm.dob,
                                               Age = mm.age,
                                               PreAdd1 = mm.preadd1,
                                               PreAdd2 = mm.preadd2,
                                               PreAdd3 = mm.preadd3,
                                               Area_Name = ra.Area_Name,
                                               Trn_Date = mt.Trn_Date,
                                               Rpt_Amt = (double)mt.Rpt_Amt,
                                               Pmt_Amt = (double)mt.Pmt_Amt,
                                               Bal_Amt = 0, //  Not present in query. Needs to be calculated if required
                                               IntCalc_Amt = (double)mt.IntCalc_Amt,
                                               IntCalc_Date = mt.IntCalc_Date,
                                               IntPaid_Amt = (double)mt.IntPaid_Amt,
                                               Int_Bal = 0, // Not present in query. Needs to be calculated if required
                                               MobileNo = mm.mobileno,
                                               PANNo = mm.panno,
                                               AadharNo = mm.aadharno,
                                               SmartCardNo = mm.smartcardno,
                                               MemberPhoto = mm.memberphoto
                                           }).ToListAsync();
                double BalAmt = 0, IntBal = 0;
                foreach (var mem in memTrnListTmp)
                {
                    BalAmt += mem.Rpt_Amt - mem.Pmt_Amt;
                    IntBal += mem.IntCalc_Amt - mem.IntPaid_Amt;
                    mem.Bal_Amt = BalAmt;
                    mem.Int_Bal = IntBal;
                }
                if (memTrnListTmp != null) memTrnList = memTrnListTmp;
            }
            catch (Exception)
            {
                throw;
            }
            return memTrnList;
        }

        public async Task<List<rptMemberVoutersList>> GetMemberVoutersList(DateTime asOnDate, List<int> memberTypeList, List<int> memberStatusList, int minimumSCBalance, string brCode)
        {
            List<rptMemberVoutersList> memList = new List<rptMemberVoutersList>();
            try
            {
                var memListTmp = await (from mm in CSISContext.mem_master
                                        join rdCaste in CSISContext.Refer_Data on mm.caste_id equals rdCaste.ReferId into casteJoin
                                        from rdCaste in casteJoin.DefaultIfEmpty()
                                        join rdCommunity in CSISContext.Refer_Data on mm.comm_id equals rdCommunity.ReferId into communityJoin
                                        from rdCommunity in communityJoin.DefaultIfEmpty()
                                        join area in CSISContext.Refer_Area on mm.prearea_id equals area.Area_Id into areaJoin
                                        from area in areaJoin.DefaultIfEmpty()
                                        where mm.memberdelete == false &&
                                              memberTypeList.Contains(mm.membertype) &&
                                              memberStatusList.Contains(mm.memberstatus) &&
                                              CSISContext.Mem_Trn.Where(mt => mt.MemTrn_Delete == false && mt.Trn_Type == 3 && mt.Trn_Date <= asOnDate && mt.Mem_Id == mm.mem_id)
                                                             .GroupBy(mt => mt.Mem_Id)
                                                             .Where(g => g.Sum(mt => mt.Rpt_Amt) - g.Sum(mt => mt.Pmt_Amt) >= minimumSCBalance)
                                                             .Any()
                                            && rdCaste.BrCode == brCode 
                                            && rdCommunity.BrCode == brCode 
                                            && area.BrCode == brCode 
                                        orderby mm.membertype, mm.memberno
                                        select new rptMemberVoutersList
                                        {
                                            Mem_Id = mm.mem_id,
                                            MemberType = mm.membertype,
                                            MemberNo = mm.memberno,
                                            MemberName = mm.membername,
                                            FatherName = mm.fathername,
                                            PerNo = mm.perno,
                                            Gender = mm.gender,
                                            GenderName = mm.gender == 1 ? "Male" : "Female",
                                            Admissiondate = mm.admissiondate,
                                            Dob = mm.dob,
                                            MobileNo = mm.mobileno,
                                            PreAdd1 = mm.preadd1,
                                            PreAdd2 = mm.preadd2,
                                            PreAdd3 = mm.preadd3,
                                            PrePin = mm.prepin,
                                            AreaName = area.Area_Name,
                                            Address = (string.IsNullOrEmpty(mm.preadd1) ? "" : mm.preadd1) +
                                                      (string.IsNullOrEmpty(mm.preadd2) ? "" : "," + mm.preadd2) +
                                                      (string.IsNullOrEmpty(mm.preadd3) ? "" : "," + mm.preadd3) +
                                                      (string.IsNullOrEmpty(mm.prepin) ? "" : "," + mm.prepin),
                                            SBAccountNo = mm.sbaccountno,
                                            BankName = mm.bankname,
                                            IFSCCode = mm.ifsccode,
                                            GPF_No = mm.gpf_no,
                                            PANNo = mm.panno,
                                            SmartCardNo = mm.smartcardno,
                                            AadharNo = mm.aadharno,
                                            Caste = rdCaste.ReferName,
                                            Community = rdCommunity.ReferName,
                                            memberStatus = mm.memberstatus //Added MemberStatus
                                        }).ToListAsync();
                if (memListTmp != null) memList = memListTmp;
            }
            catch (Exception)
            {
                throw;
            }
            return memList;
        }

        public async Task<List<rptMemberVoutersList>> GetMemberAddress(string fromMemNo, string toMemNo, List<int> memberTypeList, List<int> memberStatusList)
        {
            List<rptMemberVoutersList> memList = new List<rptMemberVoutersList>();
            try
            {
                var memListTmp = await (from mm in CSISContext.mem_master
                                        join mt in CSISContext.Mem_Trn on mm.mem_id equals mt.Mem_Id into mt_join
                                        from mt in mt_join.DefaultIfEmpty()
                                        join rdCaste in CSISContext.Refer_Data on mm.caste_id equals rdCaste.ReferId into rdCaste_join
                                        from rdCaste in rdCaste_join.DefaultIfEmpty()
                                        join rdComm in CSISContext.Refer_Data on mm.comm_id equals rdComm.ReferId into rdComm_join
                                        from rdComm in rdComm_join.DefaultIfEmpty()
                                        join area in CSISContext.Refer_Area on mm.prearea_id equals area.Area_Id into area_join
                                        from area in area_join.DefaultIfEmpty()
                                        where mm.membertype == 1 &&
                                              mm.memberdelete == false &&
                                              memberTypeList.Contains(mm.membertype) && // Assuming memberTypeList is List<int>
                                              memberStatusList.Contains(mm.memberstatus) && // Assuming memberStatusList is List<string>
                                              mm.isaccountclosed == false &&
                                              mt.MemTrn_Delete == false &&
                                              mt.Voc_Status=="V"
                                        group new { mm, mt, rdCaste, rdComm, area } by new
                                        {
                                            mm.memberno,
                                            mm.membername,
                                            mm.fathername,
                                            mm.perno,
                                            mm.gender,
                                            mm.admissiondate,
                                            mm.dob,
                                            mm.preadd1,
                                            mm.preadd2,
                                            mm.preadd3,
                                            mm.prepin,
                                            mm.mobileno,
                                            area.Area_Name,
                                            mm.smartcardno,
                                            mm.aadharno,
                                            CasteName = rdCaste.ReferName, // Alias for Caste Name
                                            CommunityName = rdComm.ReferName, // Alias for Community Name
                                            mm.bankname,
                                            mm.ifsccode,
                                            mm.gpf_no,
                                            mm.panno,
                                            mm.membertype
                                        } into g
                                        where (g.Sum(x => x.mt.Rpt_Amt) - g.Sum(x => x.mt.Pmt_Amt)) > 0 &&
                                              g.Key.memberno.CompareTo(fromMemNo) >= 0 &&
                                              g.Key.memberno.CompareTo(toMemNo) <= 0
                                        orderby g.Key.membertype, g.Key.memberno
                                        select new rptMemberVoutersList
                                        {
                                            MemberNo = g.Key.memberno,
                                            MemberName = g.Key.membername,
                                            FatherName = g.Key.fathername,
                                            PerNo = g.Key.perno,
                                            GenderName = g.Key.gender == 1 ? "Male" : "Female",
                                            Admissiondate = g.Key.admissiondate,
                                            Dob = g.Key.dob,
                                            Address = (g.Key.preadd1 != null && g.Key.preadd1.Length > 0 ? g.Key.preadd1 : "") +
                                                      (g.Key.preadd2 != null && g.Key.preadd2.Length > 0 ? "," + g.Key.preadd2 : "") +
                                                      (g.Key.preadd3 != null && g.Key.preadd3.Length > 0 ? "," + g.Key.preadd3 : "") +
                                                      (g.Key.prepin != null && g.Key.prepin.Length > 0 ? "," + g.Key.prepin : ""),
                                            MobileNo = g.Key.mobileno,
                                            AreaName = g.Key.Area_Name,
                                            SmartCardNo = g.Key.smartcardno,
                                            AadharNo = g.Key.aadharno,
                                            Caste = g.Key.CasteName,
                                            Community = g.Key.CommunityName, // Assuming the alias 'commu' maps to ReferName
                                            BankName = g.Key.bankname,
                                            IFSCCode = g.Key.ifsccode,
                                            GPF_No = g.Key.gpf_no,
                                            PANNo = g.Key.panno
                                        }).ToListAsync();
                if (memListTmp != null) memList = memListTmp;
            }
            catch (Exception)
            {
                throw;
            }
            return memList;
        }

        public async Task<List<rptMemberNewAdmission>> GetRptNewMembers(DateTime fromDate, DateTime toDate, string brCode)
        {
            List<rptMemberNewAdmission> memTrnList = new List<rptMemberNewAdmission>();
            try
            {
                var memTrnListTmp = await (from mt in CSISContext.Mem_Trn
                                           join mm in CSISContext.mem_master on mt.Mem_Id equals mm.mem_id
                                           where mt.Trn_Type == 3 &&
                                                 mm.admissiondate >= fromDate &&
                                                 mm.admissiondate <= toDate &&
                                                 mt.MemTrn_Delete == false && mt.BrCode == brCode && mt.Voc_Status =="V" &&
                                                 mm.brcode == brCode 

                                           orderby mm.memberno
                                           select new rptMemberNewAdmission
                                           {
                                               Mem_Id = mm.mem_id,
                                               MemberNo = mm.memberno,
                                               MemberName = mm.membername,
                                               PerNo = mm.perno,
                                               FatherName = mm.fathername,
                                               AdmissionDate = mm.admissiondate,
                                               Trn_Date = mt.Trn_Date,
                                               Rpt_Amt = mt.Rpt_Amt
                                           }).ToListAsync();
                if (memTrnListTmp != null) memTrnList = memTrnListTmp;
            }
            catch (Exception)
            {
                throw;
            }
            return memTrnList;
        }

        public async Task<List<rptMemberKYC>> GetMemberKYC(int memId)
        {
            List<rptMemberKYC> kyc = new List<rptMemberKYC>();
            try
            {
                var kycTmp = await (from mm in CSISContext.mem_master
                                    join presentArea in CSISContext.Refer_Area on mm.prearea_id equals presentArea.Area_Id into presentArea_join
                                    from presentArea in presentArea_join.DefaultIfEmpty()
                                    join permanentArea in CSISContext.Refer_Area on mm.perarea_id equals permanentArea.Area_Id into permanentArea_join
                                    from permanentArea in permanentArea_join.DefaultIfEmpty()
                                    where mm.mem_id == memId && mm.memberdelete == false
                                    select new rptMemberKYC
                                    {
                                        Mem_Id = mm.mem_id,
                                        MemberNo = mm.memberno,
                                        PerNo = mm.perno,
                                        MemberName = mm.membername,
                                        FatherName = mm.fathername,
                                        Dob = mm.dob,
                                        Age = DateTime.Now.Year - mm.dob!.Value.Year, // Approximate age calculation
                                        Gender = mm.gender == 1 ? "MALE" : "FEMALE",
                                        PresentAddress = (mm.preadd1 != null && mm.preadd1!.Length > 0 ? mm.preadd1 : "") +
                                                         (mm.preadd2 != null && mm.preadd2!.Length > 0 ? "," + mm.preadd2 : "") +
                                                         (mm.preadd3 != null && mm.preadd3!.Length > 0 ? "," + mm.preadd3 : "") +
                                                         (mm.prepin != null && mm.prepin!.Length > 0 ? "," + mm.prepin : ""),
                                        PermanentAddress = (mm.peradd1 != null && mm.peradd1!.Length > 0 ? mm.peradd1 : "") +
                                                           (mm.peradd2 != null && mm.peradd2!.Length > 0 ? "," + mm.peradd2 : "") +
                                                           (mm.peradd3 != null && mm.peradd3!.Length > 0 ? "," + mm.peradd3 : "") +
                                                           (mm.perpin != null && mm.perpin!.Length > 0 ? "," + mm.perpin : ""),
                                        Present_area_name = presentArea.Area_Name,
                                        Permenent_area_name = permanentArea.Area_Name,
                                        Present_Pin = mm.prepin,
                                        Permanent_Pin = mm.perpin,
                                        State = "TAMIL NADU",
                                        Country = "INDIA",
                                        MobileNo = mm.mobileno,
                                        EMailId = mm.emailid,
                                        PANNo = mm.panno,
                                        AadharNo = mm.aadharno,
                                        SmartCardNo = mm.smartcardno,
                                        ProofOfIdentity = (mm.panno != null && mm.panno!.Length > 0 ? "PAN" : "") +
                                                          (mm.aadharno != null && mm.aadharno!.Length > 0 ? (mm.panno != null && mm.panno!.Length > 0 ? " / AADHAR CARD" : "AADHAR CARD") : "") +
                                                          (mm.smartcardno != null && mm.smartcardno!.Length > 0 ? ((mm.smartcardno != null && mm.smartcardno!.Length > 0) && (mm.aadharno != null && mm.aadharno!.Length > 0) ? " / SMART CARD NO" : "SMART CARD NO") : ""),
                                        memberphoto = mm.memberphoto // Assuming PhotoImage maps to memberphoto
                                    }).ToListAsync();
                if (kycTmp != null) kyc = kycTmp;
            }
            catch (Exception)
            {
                throw;
            }
            return kyc;
        }
    }
}
