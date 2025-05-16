using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class ReportsLoanRepository : Repository<Reports_Master>, IReportsLoanRepository
    {
        //private IMemTrnRepository _memTrnRepository;
        public CSISContext CSISContext => (CSISContext)Context;
        public ReportsLoanRepository(CSISContext context ) : base(context)
        {
        }

        public async Task<List<rptLoanLedger>> GetLoanLedger(List<decimal> loanIdList, DateTime fromDate, DateTime toDate)
        {
            List<rptLoanLedger> loanList = new List<rptLoanLedger>();
            try
            {
                var loanListTmp = await GetLoanBalanceByLedIdList(loanIdList, fromDate, toDate);

                #region union
                var loanListUnion = (from x in loanListTmp
                                     from lnMaster in CSISContext.Loan_Master.Where(y => y.Loan_Id == x.Loan_Id).DefaultIfEmpty()
                                     from lnScheme in CSISContext.Loan_Schemes.Where(y => y.Scheme_Id == lnMaster.Scheme_Id).DefaultIfEmpty()
                                     from memMaster in CSISContext.mem_master.Where(y => y.mem_id == lnMaster.Mem_Id).DefaultIfEmpty()
                                     from sro in CSISContext.Refer_Data.Where(y => y.ReferId == lnMaster.Mort_Sro).DefaultIfEmpty()
                                     select new rptLoanLedger
                                     {
                                         Trn_Id = x.Trn_Id,
                                         Mem_Id = lnMaster.Mem_Id,
                                         MemberNo = memMaster.memberno,
                                         PerNo = memMaster.perno,
                                         MemberName = memMaster.membername,
                                         Loan_Id = x.Loan_Id,
                                         Loan_No = lnMaster.Loan_No,
                                         App_No = lnMaster.App_No,
                                         Res_No = lnMaster.Res_No,
                                         Res_Date = lnMaster.Res_Date,
                                         San_Amt = lnMaster.San_Amt,
                                         Prl_Prd = lnMaster.Prl_Prd,
                                         Int_Prd = lnMaster.Int_Prd,
                                         Reim_LNo = lnMaster.Reim_LNo,
                                         Reim_DvNo = lnMaster.Reim_DvNo,
                                         Mort_SlNo = lnMaster.Mort_SlNo,
                                         SRO_Name = sro == null ? null : sro.ReferName,
                                         Roi = lnMaster.Roi,
                                         Pi = lnMaster.Pi,
                                         Inst_Amt = lnMaster.Inst_Amt,
                                         Scheme_Name = lnScheme.Scheme_Name,
                                         Trn_Date = x.Trn_Date,
                                         Disb_Amt = x.Disb_Amt,
                                         Prl_Dem = x.Prl_Dem,
                                         PICalc_Amt = x.PICalc_Amt,
                                         IODCalc_Amt = x.IODCalc_Amt,
                                         IntCalc_Amt = x.IntCalc_Amt,
                                         PIColl_Amt = x.PIColl_Amt,
                                         IODColl_Amt = x.IODColl_Amt,
                                         IntColl_Amt = x.IntColl_Amt,
                                         PrlColl_Amt = x.PrlColl_Amt,
                                         Prl_OS = x.Prl_OS,
                                         Prl_OD = x.Prl_OD,
                                         Int_Bal = x.Int_Bal,
                                         PI_Bal = x.PI_Bal,
                                         IOD_Bal = x.IOD_Bal,
                                         MobileNo = memMaster.mobileno,
                                         Trn_Status = x.Trn_Status,
                                         Address = String.Concat(memMaster.peradd1, " ", memMaster.peradd2, " ", memMaster.peradd3, " ", memMaster.perpin),
                                         PANNo = memMaster.panno,
                                         AadharNo = memMaster.aadharno,
                                         SmartCardNo = memMaster.smartcardno,
                                         MemberPhoto = memMaster.memberphoto
                                     }).ToList();
                #endregion 

                if (loanListUnion != null) loanList = loanListUnion;
            }
            catch (Exception)
            {
                throw;
            }
            return loanList;
        }
        public async Task<List<rptLoanOutstanding>> GetLoanOutstandingWithAgewise(DateTime toDate, int loanType, string brCode)
        {
            int noOfMonths = 0;
            DateTime? minDueDate = null;
            bool minDueDateObtained = false;
            double accruedInt = 0;
            bool isPrlOD = false;
            List<rptLoanOutstanding> OsList = new List<rptLoanOutstanding>();
            try
            {
                OsList = await GetLoanOutstanding(toDate, loanType,brCode);
                foreach (var os in OsList)
                {
                    // LINQ for the first SQL query:
                    var dueDateWiseDemandList = await (from lt in CSISContext.Loan_Trn
                                                       where lt.Prl_Sched > 0 &&
                                                             lt.TrnTr_Delete == false &&
                                                             lt.Loan_Id == os.Loan_Id &&
                                                             lt.Trn_Date.Date <= toDate.Date
                                                             && lt.BrCode == brCode 
                                                             && lt.Voc_Status == "V"
                                                       orderby lt.Loan_Id, lt.Due_Date
                                                       select new rptLoanDemand
                                                       {
                                                           Loan_Id = lt.Loan_Id,
                                                           Trn_Date = lt.Trn_Date,
                                                           Due_Date = lt.Due_Date,
                                                           Prl_Sched = lt.Prl_Sched,
                                                           PrlColl_Amt = 0,
                                                           Prl_OS = 0,
                                                           Prl_OD = 0
                                                       }).ToListAsync();
                    // LINQ for the second SQL query:
                    var prlColl = await (from lt in CSISContext.Loan_Trn
                                         where lt.Loan_Id == os.Loan_Id &&
                                               lt.Trn_Date <= toDate.Date &&
                                               lt.TrnTr_Delete == false
                                         select lt.PrlColl_Amt).SumAsync();
                    minDueDate = null;
                    minDueDateObtained = false;
                    foreach (var dueDateWiseDemand in dueDateWiseDemandList)
                    {
                        if (dueDateWiseDemand.Prl_Sched <= prlColl)
                        {
                            dueDateWiseDemand.PrlColl_Amt = dueDateWiseDemand.Prl_Sched;
                            prlColl -= dueDateWiseDemand.Prl_Sched;
                        }
                        else
                        {
                            dueDateWiseDemand.PrlColl_Amt = prlColl;
                            prlColl = 0;
                        }
                        dueDateWiseDemand.Prl_Bal = dueDateWiseDemand.Prl_Sched - dueDateWiseDemand.PrlColl_Amt;
                        if (minDueDateObtained == false)
                        {
                            if (dueDateWiseDemand.Prl_Bal > 0)
                            {
                                minDueDate = dueDateWiseDemand.Due_Date;
                                minDueDateObtained = true;
                            }
                        }
                    }

                    /// prepare age-wise od
                    if (minDueDate != null)
                    {
                        //noOfMonths = Utilities.GetNoOfMonths(toDate, (DateTime)minDueDate);
                        noOfMonths = Utilities.GetMonthsBetweenDates(toDate, (DateTime)minDueDate);
                        if (noOfMonths == 1) isPrlOD = false;
                        else isPrlOD = true;
                        if (noOfMonths <= 3)
                            os.OD3M = os.Prl_OD;
                        if (noOfMonths > 3 && noOfMonths <= 6)
                            os.OD3M_6M = os.Prl_OD;
                        if (noOfMonths > 6 && noOfMonths <= 12)
                            os.OD7M_12M = os.Prl_OD;
                        if (noOfMonths > 12 && noOfMonths <= 24)
                            os.OD25M_36M = os.Prl_OD;
                        if (noOfMonths > 24 && noOfMonths <= 36)
                            os.OD25M_36M = os.Prl_OD;
                        if (noOfMonths > 36)
                            os.OD37M_Above = os.Prl_OD;
                    }
                    var intCalcDateList = await (from lt in CSISContext.Loan_Trn
                                                 join lm in CSISContext.Loan_Master on lt.Loan_Id equals lm.Loan_Id
                                                 where lt.TrnTr_Delete == false &&
                                                       lt.Trn_Date.Date <= toDate.Date &&
                                                       lt.Loan_Id == os.Loan_Id &&
                                                       lm.Loan_Type == loanType &&
                                                       lt.IntCalc_Amt > 0 &&
                                                       lt.BrCode == brCode  &&
                                                       lt.Voc_Status == "V" &&
                                                       lm.BrCode == brCode && lm.Voc_Status =="V"
                                                 group lt by new { lt.Loan_Id, lt.IntCalc_Date } into g
                                                 orderby g.Key.Loan_Id, g.Key.IntCalc_Date
                                                 select new rptLoanDemand
                                                 {
                                                     Loan_Id = g.Key.Loan_Id,
                                                     IntCalc_Date = g.Key.IntCalc_Date,
                                                     IntCalc_Amt = g.Sum(x => x.IntCalc_Amt)
                                                 }).ToListAsync();
                    var intColl = (from lt in CSISContext.Loan_Trn
                                   where lt.Loan_Id == os.Loan_Id &&
                                         lt.Trn_Date <= toDate &&
                                         lt.TrnTr_Delete == false &&
                                         lt.BrCode == brCode &&
                                         lt.Voc_Status == "V"
                                   select lt.IntColl_Amt).Sum();

                    // If Sum() can return null (if no matching records), handle it:
                    //double intCollValue = intColl ?? 0;
                    foreach (var intCalcDate in intCalcDateList)
                    {
                        if (intCalcDate.IntCalc_Amt <= intColl)
                        {
                            intCalcDate.IntColl_Amt = intCalcDate.IntColl_Amt;
                            intColl -= intCalcDate.IntCalc_Amt;
                        }
                        else
                        {
                            intCalcDate.IntColl_Amt = intColl;
                            intColl = 0;
                        }
                        intCalcDate.Int_Bal = intCalcDate.IntCalc_Amt - intCalcDate.IntColl_Amt;
                    }
                    if (isPrlOD == false)
                    {
                        var lastdueDate = intCalcDateList.LastOrDefault();
                        if (lastdueDate != null)
                            accruedInt = lastdueDate.IntCalc_Amt - lastdueDate.IntColl_Amt;
                        else
                            accruedInt = 0;
                        os.AccruedInt = accruedInt;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return OsList;
        }
        public  async Task<List<rptLoanDCB>> GetLoanDCB(DateTime fromDate, DateTime toDate,string brCode)
        {
            
            List<rptLoanDCB> dcbList = new List<rptLoanDCB>();
            double _advPrl = 0, _advPrlDuring = 0, _advPrlBefore = 0;
            double _prlBal = 0, _intBal = 0, _iodBal = 0, _piBal = 0;
            double _prlDem = 0;
            double _totalDem = 0, _totalColl = 0, _totalBal = 0;
            double _recper = 0;
            double _dueToAmt = 0;
            DateTime demandDate = toDate.Date.AddDays(1);
            try
            {
                #region query
                dcbList = CSISContext.Database.SqlQueryRaw<rptLoanDCB>(
                            @"With Dt1
                        AS ( 
	                        SELECT Loan_Trn.Loan_id, Loan_Master.Mem_Id,Loan_Master.Scheme_Id, Loan_Schemes.Scheme_Name,Loan_Master.Loan_No, 
	                        Mem_Master.memberNo, Mem_Master.memberName, Mem_Master.PreAdd1,Mem_Master.PreAdd2, Mem_Master.PreAdd3,Mem_Master.PrePin, 
	                        Sum(Loan_Trn.Disb_Amt) AS Disb_Amt, 
                            Sum(Loan_Trn.Disb_Amt) - Sum(Loan_Trn.PrlColl_Amt)  AS Loan_OS, 
                            Sum(Loan_Trn.Prl_Sched) - Sum(Loan_Trn.PrlColl_Amt) AS ArrPrlDem, 
	                        Sum(Loan_Trn.IntCalc_Amt) - Sum(Loan_Trn.IntColl_Amt) AS ArrIntDem, Sum(Loan_Trn.IODCalc_Amt) -  Sum(Loan_Trn.IODColl_Amt) AS ArrIODDem, 
	                        Sum(Loan_Trn.PICalc_Amt) - Sum(Loan_Trn.PIColl_Amt) AS ArrPIDem, 
	                        0 AS CurrPrlDem, 0 AS CurrIntDem, 0 AS CurrIODDem, 0 AS CurrPIDem,0 As PrlColl_Amt, 0 AS IntColl_Amt, 0 AS IODColl_Amt, 0 AS PIColl_Amt 
	                        FROM ((Loan_Trn INNER JOIN Loan_Master ON Loan_Trn.Loan_id = Loan_Master.Loan_id) 
	                        INNER JOIN Loan_Schemes ON Loan_Master.Scheme_Id = Loan_Schemes.Scheme_Id) 
	                        INNER JOIN Mem_Master ON Loan_Master.mem_Id = Mem_Master.mem_Id 
	                        Where Loan_Trn.Trn_Date <@fromDate  And Loan_Trn.TrnTr_Delete = false AND Loan_Master.Loan_Delete = false 
                            And Loan_Master.Loan_Type in(1, 6) And Loan_Trn.brCode = @brCode AND Loan_Trn.voc_status = 'V' AND mem_master.brcode = @brCode
	                        GROUP BY Loan_Trn.Loan_id, Loan_Master.Mem_Id,Loan_Master.Scheme_Id,Loan_Schemes.Scheme_Name,Loan_Master.Loan_No,Mem_Master.memberNo, Mem_Master.memberName, 
	                        Mem_Master.PreAdd1 , Mem_Master.PreAdd2, Mem_Master.PreAdd3, Mem_Master.PrePin 
	                        Having Sum(Loan_Trn.Disb_Amt) - Sum(Loan_Trn.PrlColl_Amt) > 0
                         UNION
	                        SELECT Loan_Trn.Loan_id, Loan_Master.Mem_Id,Loan_Master.Scheme_Id, Loan_Schemes.Scheme_Name, Loan_Master.Loan_No, 
	                        Mem_Master.memberNo, Mem_Master.memberName, Mem_Master.PreAdd1, Mem_Master.PreAdd2, Mem_Master.PreAdd3, Mem_Master.PrePin, 
	                        Loan_Trn.Disb_Amt,0 AS Loan_OS, 0 AS ArrPrlDem, 0 AS ArrIntDem, 0 AS ArrIODDem, 0 AS ArrPIDem,   Sum(Loan_Trn.Prl_Sched) AS CurrPrlDem, Sum(Loan_Trn.IntCalc_Amt) AS CurrIntDem, Sum(Loan_Trn.IODCalc_Amt) AS CurrIODDem, Sum(Loan_Trn.PICalc_Amt) AS CurrPIDem, 
	                        0 As PrlColl_Amt, 0 AS IntColl_Amt, 0 AS IODColl_Amt, 0 AS PIColl_Amt
	                        FROM ((Loan_Trn INNER JOIN Loan_Master ON Loan_Trn.Loan_id = Loan_Master.Loan_id) INNER JOIN Loan_Schemes ON Loan_Master.Scheme_Id = Loan_Schemes.Scheme_Id) 
	                        INNER JOIN Mem_Master ON Loan_Master.mem_Id = Mem_Master.mem_Id 
	                        WHERE Loan_Trn.Trn_Date >= @fromDate  AND Loan_Trn.Trn_Date <= @demandDate AND Loan_Trn.TrnTr_Delete = false AND Loan_Master.Loan_Delete = false 
                            AND Loan_Master.Loan_Type In (1,6) And Loan_Trn.brCode = @brCode AND Loan_Trn.voc_status = 'V' AND mem_master.brcode = @brCode
	                        GROUP BY Loan_Trn.Loan_id, Loan_Master.Mem_Id,Loan_Master.Scheme_Id,Loan_Schemes.Scheme_Name,Loan_Master.Loan_No,Mem_Master.memberNo, Mem_Master.memberName, 
	                        Mem_Master.PreAdd1 , Mem_Master.PreAdd2, Mem_Master.PreAdd3, Mem_Master.PrePin, Loan_Trn.Disb_Amt 
                        UNION
	                        SELECT Loan_Trn.Loan_id, Loan_Master.Mem_Id,Loan_Master.Scheme_Id, Loan_Schemes.Scheme_Name, Loan_Master.Loan_No, 
	                        Mem_Master.memberNo, Mem_Master.memberName, Mem_Master.PreAdd1, Mem_Master.PreAdd2, Mem_Master.PreAdd3, Mem_Master.PrePin, 
	                        Loan_Trn.Disb_Amt, Sum(Loan_Trn.Disb_Amt) - Sum(Loan_Trn.PrlColl_Amt)  AS Loan_OS, 
                            0 AS ArrPrlDem, 0 AS ArrIntDem, 0 AS ArrIODDem, 0 AS ArrPIDem,   0 AS CurrPrlDem,  0 AS CurrIntDem, 0 AS CurrIODDem, 0  AS CurrPIDem, 
	                        Sum(Loan_Trn.PrlColl_Amt) AS PrlColl_Amt, Sum(Loan_Trn.IntColl_Amt) AS IntColl_Amt,  Sum(Loan_Trn.IODColl_Amt) AS IODColl_Amt,Sum(Loan_Trn.PIColl_Amt) AS PIColl_Amt 
	                        FROM ((Loan_Trn INNER JOIN Loan_Master ON Loan_Trn.Loan_id = Loan_Master.Loan_id) INNER JOIN Loan_Schemes ON Loan_Master.Scheme_Id = Loan_Schemes.Scheme_Id) 
	                        INNER JOIN Mem_Master ON Loan_Master.mem_Id = Mem_Master.mem_Id 
	                        WHERE Loan_Trn.Trn_Date >= @fromDate  AND Loan_Trn.Trn_Date <= @toDate AND Loan_Trn.TrnTr_Delete = false AND Loan_Master.Loan_Delete = false 
                            AND Loan_Master.Loan_Type  In (1,6)  And Loan_Trn.brCode = @brCode AND Loan_Trn.voc_status = 'V' AND mem_master.brcode = @brCode
	                        GROUP BY Loan_Trn.Loan_id, Loan_Master.Mem_Id,Loan_Master.Scheme_Id,Loan_Schemes.Scheme_Name,Loan_Master.Loan_No,Mem_Master.memberNo, Mem_Master.memberName, 
	                        Mem_Master.PreAdd1 , Mem_Master.PreAdd2, Mem_Master.PreAdd3, Mem_Master.PrePin, Loan_Trn.Disb_Amt 
                        )
                        SELECT Loan_Id , Mem_Id,Scheme_Id, Scheme_Name,Loan_No,MemberNo, MemberName, PreAdd1,PreAdd2, PreAdd3,PrePin, 
	                    Sum(Disb_Amt) AS Disb_Amt, Sum(Loan_OS) AS Loan_OS, Sum(ArrPrlDem) AS ArrPrlDem, Sum(ArrIntDem) AS ArrIntDem, Sum(ArrIODDem) AS ArrIODDem, 
	                    Sum(ArrPIDem) AS ArrPIDem, 	Sum(CurrPrlDem) AS CurrPrlDem, Sum(CurrIntDem) AS CurrIntDem, Sum(CurrIODDem) AS CurrIODDem, Sum(CurrPIDem) AS CurrPIDem,
	                    Sum(PrlColl_Amt) As PrlColl_Amt, Sum(IntColl_Amt) AS IntColl_Amt, Sum(IODColl_Amt) AS IODColl_Amt, Sum(PIColl_Amt) AS PIColl_Amt 
	                    FROM Dt1
	                    GROUP BY Loan_id, Mem_Id, Scheme_Id,Scheme_Name,Loan_No,memberNo, memberName, PreAdd1,PreAdd2, PreAdd3,PrePin
	                    ORDER BY Scheme_Id, Loan_No"
                            , new NpgsqlParameter("@fromDate", fromDate)
                            , new NpgsqlParameter("@toDate", toDate)
                            , new NpgsqlParameter("@demandDate", demandDate)
                            , new NpgsqlParameter ("@brCode",brCode )).ToList();
                #endregion 

                foreach (var dcb in dcbList)
                {
                    _advPrl = 0; _advPrlBefore = 0; _advPrlDuring = 0;
                    if (dcb.ArrPrlDem < 0)
                    {
                        _advPrl = Math.Abs(dcb.ArrPrlDem);
                        //_advPrlDuring = Math.Abs(dcb.ArrPrlDem);
                        dcb.ArrPrlDem = 0;
                    }
                    if (dcb.ArrIntDem < 0) dcb.ArrIntDem = 0;
                    if (dcb.ArrIODDem < 0) dcb.ArrIODDem = 0;
                    if (dcb.ArrPIDem < 0) dcb.ArrPIDem = 0;
                    _prlBal = dcb.ArrPrlDem + dcb.CurrPrlDem - dcb.PrlColl_Amt;
                    _intBal = dcb.ArrIntDem + dcb.CurrIntDem - dcb.IntColl_Amt;
                    _iodBal = dcb.ArrIODDem + dcb.CurrIODDem - dcb.IODColl_Amt;
                    _piBal = dcb.ArrPIDem + dcb.CurrPIDem - dcb.PIColl_Amt;

                    if (_prlBal < 0)
                    {
                        _advPrlDuring = Math.Abs(_prlBal);
                        dcb.PrlColl_Amt -= _advPrlDuring;
                        _prlBal = 0;
                    }
                    _totalColl = dcb.PrlColl_Amt + dcb.IntColl_Amt + dcb.IODColl_Amt + dcb.PIColl_Amt;
                    if (_prlBal > 0 && _advPrl > 0)
                    {
                        if (_advPrl >= _prlBal)
                        {
                            _advPrlBefore = _prlBal;
                            _advPrl -= _advPrlBefore;
                        }
                        else
                        {
                            _advPrlBefore = _advPrl;
                            _advPrl = 0;
                        }
                        _prlBal -= _advPrlBefore;
                    }
                    _totalColl = dcb.PrlColl_Amt + dcb.IntColl_Amt + dcb.IODColl_Amt + dcb.PIColl_Amt;
                    dcb.PrlBal = _prlBal;
                    dcb.IntBal = _intBal;
                    dcb.IODBal = _iodBal;
                    dcb.PIBal = _piBal;
                    _totalBal = _prlBal + _intBal + _iodBal + _piBal;
                    _totalDem = dcb.ArrPIDem + dcb.CurrPIDem + dcb.ArrIntDem + dcb.CurrIntDem + dcb.ArrIODDem + dcb.CurrIODDem + dcb.ArrPrlDem + dcb.CurrPrlDem;
                    if (_totalColl > 0)
                    {
                        _recper = (_totalColl / _totalDem) * 100;
                    }
                    else
                        _recper = 0;

                    /// BEGIN - calculate current demand upto date, then update totals
                    #region Calculate Current Demand
                    DateTime _firstIntDueDate = toDate.Date;
                    DateTime _maxTrnDate = toDate.Date;
                    DateTime _maxDueDate = toDate.Date;
                    DateTime _piFromDate = toDate.Date;

                    DateTime _intfromDate = toDate.Date;
                    //int _intApplication = 0;
                    //DatabaseLoan dbLoan = new DatabaseLoan();
                    LoanInterestCalculatedItems intItems = new LoanInterestCalculatedItems();
                    intItems = await CalculateLnDuesHSIS_ForDCB(dcb.Loan_Id, toDate.Date, toDate.Date);
                    #endregion

                    _prlDem = CalculatePrincipalDemandHSIS_ForDCB(dcb.Loan_Id, toDate.Date);

                    /// update current demand
                    dcb.TotalPrlDem = dcb.ArrPrlDem + dcb.CurrPrlDem + _prlDem;
                    dcb.TotalIntDem = dcb.ArrIntDem + dcb.CurrIntDem + intItems.InterestCalculatAmt;
                    dcb.TotalIODDem = dcb.ArrIODDem + dcb.CurrIODDem + intItems.IODCalculateAmt;
                    dcb.TotalPIDem = dcb.ArrPIDem + dcb.CurrPIDem + intItems.PICalculateAmt;

                    /// update balance with current demand
                    dcb.PrlBal += _prlDem;
                    dcb.IntBal += intItems.InterestCalculatAmt;
                    dcb.IODBal += intItems.IODCalculateAmt;
                    dcb.PIBal += intItems.PICalculateAmt;

                    if (_recper > 100) _recper = 0;
                    dcb.Recper = _recper;
                    dcb.AdvPrlDuring = _advPrlDuring;
                    dcb.AdvPrlBefore = _advPrlBefore;
                    dcb.AdvPrl = _advPrl;
                    //_dueToAmt = 0;
                    //_dueToAmt = await  _memTrnRepository.GetmemTrnTotalSuspenseAmount(dcb.Mem_Id, 1);
                    
                    dcb.Due_To = _dueToAmt;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return dcbList;
        }
        public async Task<List<rptLoanDisbursementHSIS>> GetLoanDisbursement(DateTime fromDate, DateTime toDate, int loanType, string brCode)
        {
            List<rptLoanDisbursementHSIS> loanList = new List<rptLoanDisbursementHSIS>();
            try
            {
                #region sql query
                //        loanList = CSISContext.Database.SqlQueryRaw<rptLoanDisbursementHSIS>(
                //                    @"SELECT Loan_Master.Mem_Id,Loan_Master.Loan_Id,Loan_Master.Loan_No, Mem_Master.memberNo, Mem_Master.PerNo, Mem_Master.memberName, 
                //                        Loan_Master.San_date, Loan_Master.San_amt, Loan_Master.Prl_Prd, 
                //Loan_Master.roi, Loan_Master.pi, Loan_Schemes.Scheme_Name,Loan_Trn.Disb_Date, Loan_Trn.Disb_Amt,  
                //                        Fin_Voucher_Bank.fvb_Cheque_No, Fin_Voucher_Bank.fvb_Cheque_Date, Loan_Trn.voc_id,Fin_Voucher.Voc_No,Fin_Voucher_Bank.fvb_Bank_Name
                //                FROM    Loan_Master INNER JOIN
                //                        Loan_Trn ON Loan_Master.Loan_id = Loan_Trn.Loan_Id INNER JOIN
                //                        Loan_Schemes ON Loan_Master.Scheme_Id = Loan_Schemes.Scheme_Id INNER JOIN
                //                        Fin_Voucher ON Loan_Master.voc_Id = Fin_Voucher.Voc_Id INNER JOIN
                //                        mem_master ON Loan_Master.Mem_Id = Mem_Master.mem_Id LEFT OUTER JOIN
                //                        Fin_Voucher_Bank ON Fin_Voucher.Voc_Id = Fin_Voucher_Bank.Voc_Id
                //                WHERE  (Loan_Master.Loan_Type = @loanType) AND (Loan_Master.Loan_Delete = false) AND (Loan_Trn.TrnTr_Delete = false)
                //                        AND  ( CAST(Loan_Trn.Disb_Date  AS Date) BETWEEN @fromDate AND @toDate)
                //                ORDER BY Loan_Schemes.Scheme_Name, Loan_Master.Loan_No "
                //                    , new NpgsqlParameter("@fromDate", fromDate)
                //                    , new NpgsqlParameter("@toDate", toDate.Date)
                //                    , new NpgsqlParameter("@loanType", loanType)).ToList();
                #endregion

                #region linq
                var loanListTmp =await  (from lm in CSISContext.Loan_Master // Replace YourLoanMasterEntity
                                join lt in CSISContext.Loan_Trn // Replace YourLoanTrnEntity
                                    on lm.Loan_Id equals lt.Loan_Id
                                join ls in CSISContext.Loan_Schemes // Replace YourLoanSchemesEntity
                                    on lm.Scheme_Id equals ls.Scheme_Id
                                join fv in CSISContext.Fin_Voucher // Replace YourFinVoucherEntity
                                    on lm.Voc_Id equals fv.Voc_Id
                                join mm in CSISContext.mem_master // Replace YourMemMasterEntity
                                    on lm.Mem_Id equals mm.mem_id
                                join fvb in CSISContext.Fin_Voucher_Bank // Replace YourFinVoucherBankEntity
                                    on fv.Voc_Id equals fvb.Voc_Id into fvbLeft
                                from fvb in fvbLeft.DefaultIfEmpty()
                                where lm.Loan_Type == loanType &&
                                      lm.Loan_Delete == false &&
                                      lt.TrnTr_Delete == false &&
                                      lt.Disb_Date >= fromDate.Date &&
                                      lt.Disb_Date <= toDate.Date &&
                                      lm.BrCode == brCode && lm.Voc_Status == "V" &&
                                      lt.BrCode == brCode && lt.Voc_Status == "V" &&
                                      fv.BrCode == brCode && fv.Voc_Status == "V" &&
                                      mm.brcode == brCode &&
                                      fvb.BrCode == brCode && fvb.Voc_Status == "V"
                                orderby ls.Scheme_Name, lm.Loan_No
                                select new rptLoanDisbursementHSIS
                                {
                                    Mem_Id = lm.Mem_Id,
                                    MemberNo = mm.memberno,
                                    PerNo = mm.perno,
                                    MemberName = mm.membername,
                                    Loan_No = lm.Loan_No,
                                    San_Date = lm.San_Date,
                                    San_Amt = lm.San_Amt,
                                    Prl_Prd = lm.Prl_Prd,
                                    Roi = lm.Roi,
                                    Pi = lm.Pi,
                                    Scheme_Name = ls.Scheme_Name,
                                    Disb_Date = lt.Disb_Date,
                                    Disb_Amt = lt.Disb_Amt,
                                    Voc_Id = fv.Voc_Id,
                                    Voc_No = fv.Voc_No,
                                    Fvb_Cheque_No = fvb.Fvb_Cheque_No,
                                    Fvb_Cheque_Date = fvb.Fvb_Cheque_Date,
                                    Fvb_Bank_Name = fvb.Fvb_Bank_Name
                                }).ToListAsync();
                #endregion 

                if (loanListTmp != null) loanList = loanListTmp;
            }
            catch (Exception)
            {
                throw;
            }
            return loanList;
        }

        #region dependend methods
        public async Task<List<rptLoanOutstanding>> GetLoanOutstanding(DateTime toDate, int loanType,string brCode)
        {
            List<rptLoanOutstanding> loanList = new List<rptLoanOutstanding>();
            try
            {
                var loanListTmp = await (from t in CSISContext.Loan_Trn
                                         join l in CSISContext.Loan_Master on t.Loan_Id equals l.Loan_Id
                                         join m in CSISContext.mem_master on l.Mem_Id equals m.mem_id
                                         join s in CSISContext.Loan_Schemes on l.Scheme_Id equals s.Scheme_Id
                                         where t.Trn_Date.Date <= toDate.Date &&
                                               t.TrnTr_Delete == false &&
                                               l.Loan_Delete == false &&
                                               l.Loan_Type == loanType &&
                                               t.BrCode == brCode && t.Voc_Status =="V" &&
                                               l.BrCode == brCode && l.Voc_Status =="V" &&
                                               m.brcode == brCode 
                                         group new { t, l, m, s } by new
                                         {
                                             t.Loan_Id,
                                             l.Loan_No,
                                             l.Mem_Id,
                                             m.memberno,
                                             m.perno,
                                             m.membername,
                                             l.Scheme_Id,
                                             s.Scheme_Name,
                                             l.Roi,
                                             l.Prl_Prd,
                                             Disb_Date = l.San_Date // Assuming San_date maps to Disb_Date
                                         } into g
                                         where (g.Sum(x => x.t.Disb_Amt) - g.Sum(x => x.t.PrlColl_Amt)) > 0 ||
                                               (g.Sum(x => x.t.IntCalc_Amt) - g.Sum(x => x.t.IntColl_Amt)) > 0 ||
                                               (g.Sum(x => x.t.IODCalc_Amt) - g.Sum(x => x.t.IODColl_Amt)) > 0 ||
                                               (g.Sum(x => x.t.PICalc_Amt) - g.Sum(x => x.t.PIColl_Amt)) > 0
                                         orderby g.Key.Scheme_Id, g.Key.Loan_No
                                         select new rptLoanOutstanding
                                         {
                                             Loan_Id = g.Key.Loan_Id,
                                             Loan_No = g.Key.Loan_No,
                                             Mem_Id = g.Key.Mem_Id,
                                             MemberNo = g.Key.memberno,
                                             PerNo = g.Key.perno,
                                             MemberName = g.Key.membername,
                                             Scheme_Id = g.Key.Scheme_Id,
                                             Scheme_Name = g.Key.Scheme_Name,
                                             Roi = g.Key.Roi,
                                             Prl_Prd = g.Key.Prl_Prd,
                                             Disb_Date = g.Key.Disb_Date,
                                             Disb_Amt = g.Sum(x => x.t.Disb_Amt),
                                             PrlColl_Amt = g.Sum(x => x.t.PrlColl_Amt),
                                             Prl_Sched = g.Sum(x => x.t.Prl_Sched),
                                             Prl_Dem = g.Sum(x => x.t.Prl_Dem),
                                             IntCalc_Amt = g.Sum(x => x.t.IntCalc_Amt),
                                             IntCalc_Date = g.Max(x => x.t.IntCalc_Date),
                                             IntColl_Amt = g.Sum(x => x.t.IntColl_Amt),
                                             IODCalc_Amt = g.Sum(x => x.t.IODCalc_Amt),
                                             IODColl_Amt = g.Sum(x => x.t.IODColl_Amt),
                                             PICalc_Amt = g.Sum(x => x.t.PICalc_Amt),
                                             PICalc_Date = g.Max(x => x.t.PICalc_Date),
                                             PIColl_Amt = g.Sum(x => x.t.PIColl_Amt),
                                             // Calculate outstanding balances in a separate step if needed
                                             // Prl_OS = g.Sum(x => x.t.Disb_Amt) - g.Sum(x => x.t.PrlColl_Amt),
                                             // Int_Bal = g.Sum(x => x.t.IntCalc_Amt) - g.Sum(x => x.t.IntColl_Amt),
                                             // IOD_Bal = g.Sum(x => x.t.IODCalc_Amt) - g.Sum(x => x.t.IODColl_Amt),
                                             // PI_Bal = g.Sum(x => x.t.PICalc_Amt) - g.Sum(x => x.t.PIColl_Amt),
                                         }).ToListAsync();

                // You can calculate Prl_OS, Int_Bal, IOD_Bal, PI_Bal after fetching the initial data:
                foreach (var item in loanListTmp)
                {
                    item.Prl_OS = item.Disb_Amt - item.PrlColl_Amt;
                    item.Int_Bal = item.IntCalc_Amt - item.IntColl_Amt;
                    item.IOD_Bal = item.IODCalc_Amt - item.IODColl_Amt;
                    item.PI_Bal = item.PICalc_Amt - item.PIColl_Amt;
                }
                if (loanListTmp != null) loanList = loanListTmp;
            }
            catch (Exception)
            {
                throw;
            }
            return loanList;
        }
        public async Task<List<rptLoanLedger>> GetLoanBalanceByLedIdList(List<decimal> loanIdList, DateTime fromDate, DateTime toDate)
        {
            List<rptLoanLedger> loanList = new List<rptLoanLedger>();
            try
            {
                var loanListTmp = await (from lt in CSISContext.Loan_Trn
                                         where lt.TrnTr_Delete == false &&
                                               lt.Trn_Date.Date < fromDate.Date &&
                                               loanIdList.Contains(lt.Loan_Id) && // Assuming resultStr is a comma-separated string of Loan_ids
                                               lt.Voc_Status =="V"
                                         group lt by lt.Loan_Id into g
                                         select new rptLoanLedger
                                         {
                                             Loan_Id = g.Key,
                                             Prl_OS = g.Sum(x => x.Disb_Amt) - g.Sum(x => x.PrlColl_Amt),
                                             Prl_OD = g.Sum(x => x.Prl_Sched) - g.Sum(x => x.PrlColl_Amt),
                                             Int_Bal = g.Sum(x => x.IntCalc_Amt) - g.Sum(x => x.IntColl_Amt),
                                             IOD_Bal = g.Sum(x => x.IODCalc_Amt) - g.Sum(x => x.IODColl_Amt),
                                             PI_Bal = g.Sum(x => x.PICalc_Amt) - g.Sum(x => x.PIColl_Amt),
                                             Disb_Amt = 0,
                                             PrlColl_Amt = 0,
                                             Prl_Dem = 0,
                                             IntCalc_Amt = 0,
                                             IntColl_Amt = 0,
                                             IODCalc_Amt = 0,
                                             IODColl_Amt = 0,
                                             PICalc_Amt = 0,
                                             PIColl_Amt = 0,
                                             Trn_Date = g.Max(x => x.Trn_Date),
                                             Trn_Id = g.Max(x => x.Trn_Id),
                                             // Assuming Trn_SlNo exists in your Loan_Trn entity
                                             // If not, you might need to adjust this based on your actual column name
                                             // or how you determine the maximum serial number.
                                             // Example if the column name is different: TrnSlNo = g.Max(x => x.YourTrnSlNoColumn),
                                             Trn_SlNo = g.Max(x => x.Trn_SlNo),
                                             Trn_Status = "A"
                                         })
                                 .Union(from lt in CSISContext.Loan_Trn
                                        where lt.TrnTr_Delete == false &&
                                              lt.Trn_Date.Date >= fromDate.Date &&
                                              lt.Trn_Date.Date <= toDate.Date &&
                                              lt.Voc_Status =="V" &&
                                              loanIdList.Contains(lt.Loan_Id)
                                        select new rptLoanLedger
                                        {
                                            Loan_Id = lt.Loan_Id,
                                            Prl_OS = 0,
                                            Prl_OD = 0,
                                            Int_Bal = 0,
                                            IOD_Bal = 0,
                                            PI_Bal = 0,
                                            Disb_Amt = lt.Disb_Amt,
                                            PrlColl_Amt = lt.PrlColl_Amt,
                                            Prl_Dem = lt.Prl_Dem,
                                            IntCalc_Amt = lt.IntCalc_Amt,
                                            IntColl_Amt = lt.IntColl_Amt,
                                            IODCalc_Amt = lt.IODCalc_Amt,
                                            IODColl_Amt = lt.IODColl_Amt,
                                            PICalc_Amt = lt.PICalc_Amt,
                                            PIColl_Amt = lt.PIColl_Amt,
                                            Trn_Date = lt.Trn_Date,
                                            Trn_Id = lt.Trn_Id,
                                            Trn_SlNo = lt.Trn_SlNo,
                                            Trn_Status = lt.Trn_Status
                                        })
                                 .OrderBy(x => x.Loan_Id)
                                 .ThenBy(x => x.Trn_Date)
                                 .ThenBy(x => x.Trn_Status)
                                 .ThenBy(x => x.Trn_SlNo)
                                 .ToListAsync();

                double PrlOS = 0, PrlOD = 0, IntBal = 0, IODBal = 0, PIBal = 0;
                decimal loanId = 0;
                //string sql = "";
                foreach (var loan in loanListTmp)
                {
                    if (loanId != loan.Loan_Id)
                    {
                        PrlOS = 0; PrlOD = 0; IntBal = 0; IODBal = 0; PIBal = 0;
                        loanId = loan.Loan_Id;
                    }
                    PrlOS += loan.Prl_OS + loan.Disb_Amt - loan.PrlColl_Amt;
                    PrlOD += loan.Prl_OD + loan.Prl_Dem - loan.PrlColl_Amt;
                    IntBal += loan.Int_Bal + loan.IntCalc_Amt - loan.IntColl_Amt;
                    IODBal += loan.IOD_Bal + loan.IODCalc_Amt - loan.IODColl_Amt;
                    PIBal += loan.PI_Bal + loan.PICalc_Amt - loan.PIColl_Amt;

                    loan.Prl_OS = PrlOS;
                    loan.Prl_OD = PrlOD;
                    loan.Int_Bal = IntBal;
                    loan.IOD_Bal = IODBal;
                    loan.PI_Bal = PIBal;
                }
                if (loanListTmp != null) loanList = loanListTmp;
            }
            catch (Exception)
            {
                throw;
            }
            return loanList;
        }
        public async Task<LoanInterestCalculatedItems> CalculateLnDuesHSIS_ForDCB(decimal loanid, DateTime ToDate, DateTime PiToDate)
        {
            DateTime FirstIntDueDate;
            DateTime MaxTrn_Date;
            DateTime? MaxDueDate;
            DateTime IntFromDate;
            DateTime? PIFromDate;
            //DateTime? IODFromDate;
            int Int_Application = 0;
            int PI_Application = 0;
            int IOD_Application = 0;
            double DisbAmt = 0, PrlColl = 0;
            double PrlDemand = 0, IntCalcAmt = 0, IntCollAmt = 0;
            ///PrlSchedule = 0,
            LoanInterestCalculatedItems calcLnDues = new LoanInterestCalculatedItems();
            //string DisbAgency = "S";
            LoanDetailsHL lnDetails = new LoanDetailsHL();
            lnDetails = GetLoanDetailsHL(loanid);
            if (lnDetails == null)
            {
                return calcLnDues;
            }
            DisbAmt = lnDetails.Disb_Amt;
            PrlDemand = lnDetails.Prl_Dem;
            PrlColl = lnDetails.PrlColl_Amt;
            IntCalcAmt = lnDetails.IntCalc_Amt;
            IntCollAmt = lnDetails.IntColl_Amt;

            MaxTrn_Date = lnDetails.Trn_Date;
            MaxDueDate = lnDetails.Due_Date;
            FirstIntDueDate = lnDetails.FirstInt_DueDate;
            PIFromDate = lnDetails.PICalc_Date;
            Int_Application = lnDetails.Int_Application;
            PI_Application = lnDetails.PI_Application;
            IOD_Application = lnDetails.IOD_Application;
            //LoanInterestCalculatedItems calcLnDues = new LoanInterestCalculatedItems();
            double LoanOS = DisbAmt - PrlColl;
            double NonODPrl = DisbAmt - PrlDemand;
            double PrlOD = PrlDemand - PrlColl;
            double IntOD = IntCalcAmt - IntCollAmt;
            double _nonODSocAmt = 0, _nonODFedAmt = 0, _prlCollOrPrlDem = 0;
            double _socOS = 0, _fedOS = 0;
            double _socDisbAmt = 0, _fedDisbAmt = 0;
            double _intCalc = 0;
            //DateTime _intFromDateTmp = (DateTime)lnDetails.IntCalc_Date; /// IntFromDate;
            DateTime _piFromDate = MaxTrn_Date;  // DateTime.Now;
            DateTime _iodFromDate = MaxTrn_Date; // DateTime.Now;
            //DateTime _NextDueDate = DateTime.Now;
            //DateTime _intToDate = ToDate;
            double _piCalc = 0;
            double _iodCalc = 0;
            try
            {
                if (PrlOD < 0) PrlOD = 0;
                if (PrlColl >= PrlDemand)
                    _prlCollOrPrlDem = PrlColl;
                else
                    _prlCollOrPrlDem = PrlDemand;

                List<Loan_Disb> disbList = new List<Loan_Disb>();

                if (lnDetails.IntCalc_Date != null)
                    IntFromDate = (DateTime)lnDetails.IntCalc_Date;
                else
                    IntFromDate = lnDetails.Disb_Date;
                disbList = await GetLoanDisbursementList(loanid, IntFromDate, ToDate);
                if (disbList == null)
                {
                    return calcLnDues;
                }
                if (!GetSocDisbAmt_And_FedDisAmt(loanid, ToDate, out _socDisbAmt, out _fedDisbAmt))
                {
                    return calcLnDues;
                }


                if (!Get_NonOD_And_OS_Soc_Fed(_socDisbAmt, _fedDisbAmt, PrlColl, PrlDemand, out _nonODSocAmt, out _nonODFedAmt, out _socOS, out _fedOS))
                {
                    return calcLnDues;
                }

                /// calculate Interest
                #region calculate interest

                foreach (Loan_Disb disb in disbList)
                {
                    /// fix to date 

                    if (Int_Application == 1)    /// interest on non-od principal
                    {
                        _intCalc += await Raise_Curr_Int_Demand(loanid, _nonODSocAmt, _nonODFedAmt, IntFromDate, disb.Disb_Date);

                    }
                    if (Int_Application == 2)    /// interest on prl outstanding
                    {
                        _intCalc += await Raise_Curr_Int_Demand(loanid, _socOS, _fedOS, IntFromDate, disb.Disb_Date);

                    }
                    _nonODSocAmt += disb.SocDisb_Amt;
                    _nonODFedAmt += disb.Reim_Amt;
                    _socOS += disb.SocDisb_Amt;
                    _fedOS += disb.Reim_Amt;
                    IntFromDate = disb.Disb_Date;

                    if (Int_Application == 1)    /// interest on on-od principal
                    {
                        _intCalc += await Raise_Curr_Int_Demand(loanid, _nonODSocAmt, _nonODFedAmt, IntFromDate, ToDate);

                    }
                    if (Int_Application == 2)    /// interest on prl outstanding
                    {
                        _intCalc += await Raise_Curr_Int_Demand(loanid, _socOS, _fedOS, IntFromDate, ToDate);

                    }
                    calcLnDues.InterestCalculatAmt = _intCalc;
                    calcLnDues.InterestCalculateDate = ToDate;
                }


                if (Int_Application == 1)    /// interest on non-od principal
                {
                    _intCalc += await Raise_Curr_Int_Demand(loanid, _nonODSocAmt, _nonODFedAmt, IntFromDate, ToDate);

                }
                if (Int_Application == 2)    /// interest on prl outstanding
                {
                    _intCalc += await Raise_Curr_Int_Demand(loanid, _socOS, _fedOS, IntFromDate, ToDate);

                }

                calcLnDues.InterestCalculatAmt += _intCalc;
                calcLnDues.InterestCalculateDate = ToDate;
                #endregion

                /// calculate PI
                #region calcualte PI And IOD Calculation

                _piFromDate = MaxTrn_Date;
                /// CALCULATION OF PI TO BE STUDIED IN DEPTH  2022-04-28
                /// Modified on 2022-04-29
                //_piFromDate = MaxTrn_Date;

                /// if maximum due date is greater than current receipt date
                /// get previous of MaxOfDue_Date Prl and Int balances and calculate PI,IOD from MaxOfPICalc_Date to PIToDate

                if (MaxDueDate > PiToDate)
                {

                    if (!Get_PIIOD_ForNonDemandLoans(loanid, MaxDueDate == null ? FirstIntDueDate : (DateTime)MaxDueDate, PIFromDate, PiToDate, PI_Application, IOD_Application, PrlOD, IntOD, out _piCalc, out _iodCalc))
                    {
                        return calcLnDues;
                    }

                    calcLnDues.PICalculateAmt = _piCalc;
                    calcLnDues.IODCalculateAmt = _iodCalc;
                    calcLnDues.PICalcuateDate = PiToDate.Date;
                    calcLnDues.IODCalculateDate = PiToDate.Date;

                }
                else
                {
                    if (PIFromDate == null)
                        if (MaxDueDate != null)
                            PIFromDate = (DateTime)MaxDueDate;
                    if (PIFromDate != null)
                    {
                        if (PI_Application == 1) /// no pi
                        {
                            calcLnDues.PICalculateAmt = 0;
                            calcLnDues.PICalcuateDate = null;
                        }
                        else if (PI_Application == 2)  /// pi on prl od
                        {
                            if (PrlOD > 0)
                            {
                                if (MaxDueDate >= PIFromDate)
                                    calcLnDues.PICalculateAmt = Calc_PI_OnVariable_ROI(loanid, (DateTime)MaxDueDate, PiToDate, PrlOD, "S");
                                else
                                    calcLnDues.PICalculateAmt = Calc_PI_OnVariable_ROI(loanid, (DateTime)PIFromDate, PiToDate, PrlOD, "S");
                                if (calcLnDues.PICalculateAmt > 0)
                                    calcLnDues.PICalcuateDate = PiToDate.Date;
                            }
                            else
                            {
                                calcLnDues.PICalculateAmt = 0;
                                calcLnDues.PICalcuateDate = null;
                            }
                        }
                        else if (PI_Application == 3) /// pi on prl od + int od
                        {
                            if (PrlOD + IntOD > 0)
                            {
                                if (MaxDueDate >= PIFromDate)
                                    calcLnDues.PICalculateAmt = Calc_PI_OnVariable_ROI(loanid, (DateTime)MaxDueDate, PiToDate, PrlOD + IntOD, "S");
                                else
                                    calcLnDues.PICalculateAmt = Calc_PI_OnVariable_ROI(loanid, (DateTime)PIFromDate, PiToDate, PrlOD + IntOD, "S");
                                if (calcLnDues.PICalculateAmt > 0)
                                    calcLnDues.PICalcuateDate = PiToDate.Date;
                            }
                            else
                            {
                                calcLnDues.PICalculateAmt = 0;
                                calcLnDues.PICalcuateDate = null;
                            }
                        }
                        else
                        {
                            calcLnDues.PICalculateAmt = 0;
                            calcLnDues.PICalcuateDate = null;
                        }

                        #region Calculate IOD

                        _iodFromDate = MaxTrn_Date;

                        if (IOD_Application == 1)    /// no iod
                        {
                            calcLnDues.IODCalculateAmt = 0;
                            calcLnDues.IODCalculateDate = null;
                        }
                        else if (IOD_Application == 2) /// iod on prlod
                        {
                            if (PrlOD > 0)
                            {
                                if (MaxDueDate >= PIFromDate)
                                    calcLnDues.IODCalculateAmt = Calc_Int_OnVariable_ROI(PrlOD, loanid, (DateTime)MaxDueDate, PiToDate, "S");
                                else
                                    calcLnDues.IODCalculateAmt = Calc_Int_OnVariable_ROI(PrlOD, loanid, (DateTime)PIFromDate, PiToDate, "S");
                            }
                            else
                            {
                                calcLnDues.IODCalculateAmt = 0;
                                calcLnDues.IODCalculateDate = null;
                            }
                        }
                        else if (IOD_Application == 3) /// iod on prlod + intod
                        {
                            if (PrlOD + IntOD > 0)
                            {
                                if (MaxDueDate >= PIFromDate)
                                    calcLnDues.IODCalculateAmt = Calc_Int_OnVariable_ROI(PrlOD + IntOD, loanid, (DateTime)MaxDueDate, PiToDate, "S");
                                else
                                    calcLnDues.IODCalculateAmt = Calc_Int_OnVariable_ROI(PrlOD + IntOD, loanid, (DateTime)PIFromDate, PiToDate, "S");
                            }
                            else
                            {
                                calcLnDues.IODCalculateAmt = 0;
                                calcLnDues.IODCalculateDate = null;
                            }
                        }
                        if (calcLnDues.IODCalculateAmt > 0)
                            calcLnDues.IODCalculateDate = PiToDate.Date;
                        else
                            calcLnDues.IODCalculateDate = null;
                        #endregion
                    }
                }
                #endregion
            }

            catch (Exception)
            {
            }
            return calcLnDues;
        }
        public async Task<double> Raise_Curr_Int_Demand(decimal loanId, double _socAmt, double _fedAmt, DateTime _fromDate, DateTime _toDate)
        {
            /// _socAmt may be nonODSocAmt or socOS
            /// _fedAmt may be nonODFedAmt or fedOS
            double _intCalc = 0;
            DateTime _intFromDateTmp;
            List<Loan_Disb> disbList = new List<Loan_Disb>();
            try
            {
                disbList = await GetLoanDisbursementList(loanId, _fromDate, _toDate);

                _intFromDateTmp = _fromDate;
                foreach (var disb in disbList)
                {
                    _intCalc += Calc_Int_OnVariable_ROI(_socAmt, loanId, _intFromDateTmp, disb.Disb_Date, "S");
                    _intCalc += Calc_Int_OnVariable_ROI(_fedAmt, loanId, _intFromDateTmp, disb.Disb_Date, "F");

                    if (_intFromDateTmp > (DateTime)disb.Disb_Date)
                    {
                        _socAmt += disb.SocDisb_Amt;
                        _fedAmt += disb.Reim_Amt;
                    }
                    _intFromDateTmp = (DateTime)disb.Disb_Date;
                }
                _intCalc += Calc_Int_OnVariable_ROI(_socAmt, loanId, _intFromDateTmp, _toDate.Date, "S");
                _intCalc += Calc_Int_OnVariable_ROI(_fedAmt, loanId, _intFromDateTmp, _toDate.Date, "F");
            }
            catch (Exception)
            {
                _intCalc = 0;
            }
            return _intCalc;
        }
        public double CalculatePrincipalDemandHSIS_ForDCB(decimal LoanId, DateTime toDate)
        {
            int Inst_Type = 0, Period = 0;
            double Inst_Amt = 0, DisbAmt = 0, PrlColl = 0, TotalPrlDem = 0, InterestCalculatAmt = 0;
            DateTime? FirstPrl_DueDate;
            DateTime _maxDueDate, _maxIntCalcDate;
            double _loanOS = 0, _prlDemand = 0, _socDisbAmt = 0, _fedDisbAmt = 0, _nonODPrl = 0, _nonODSocAmt = 0, _nonODFedAmt = 0, _socOS = 0, _fedOS = 0, _oneMonthIntCalc = 0;
            DateTime _intFromDateTmp;
            DateTime? LastDueDate;
            LoanDetailsHL lnDetails = new LoanDetailsHL();
            try
            {
                #region Calculate Principal Demand
                lnDetails = GetLoanDetailsHL(LoanId);

                if (lnDetails == null)
                {
                    return 0;
                }
                if (lnDetails.Due_Date >= toDate.Date)
                {
                    return 0;
                }

                Inst_Type = lnDetails.Inst_Type;
                Period = lnDetails.Prl_Prd;
                Inst_Amt = lnDetails.Inst_Amt;
                DisbAmt = lnDetails.Disb_Amt;
                PrlColl = lnDetails.PrlColl_Amt;
                TotalPrlDem = lnDetails.Prl_Dem + lnDetails.Prl_DemCurrent;
                InterestCalculatAmt = lnDetails.IntCalc_AmtCurrent;
                FirstPrl_DueDate = lnDetails.FirstPrl_DueDate;
                if (lnDetails.Due_Date != null)
                    _maxDueDate = (DateTime)lnDetails.Due_Date;
                else
                    _maxDueDate = lnDetails.FirstInt_DueDate;
                if (lnDetails.IntCalc_DateCurrent != null)
                    _maxIntCalcDate = (DateTime)lnDetails.IntCalc_DateCurrent;
                else
                    _maxIntCalcDate = lnDetails.Disb_Date;
                _loanOS = lnDetails.Disb_Amt - lnDetails.PrlColl_Amt;
                if (FirstPrl_DueDate != null)
                    LastDueDate = ((DateTime)FirstPrl_DueDate).AddMonths(Period);
                else
                    LastDueDate = null;

                if (!GetSocDisbAmt_And_FedDisAmt(LoanId, toDate.Date, out _socDisbAmt, out _fedDisbAmt))
                {
                    return 0;
                }


                if (!Get_NonOD_And_OS_Soc_Fed(_socDisbAmt, _fedDisbAmt, PrlColl, TotalPrlDem, out _nonODSocAmt, out _nonODFedAmt, out _socOS, out _fedOS))
                {
                    return 0;
                }

                _nonODPrl = DisbAmt - PrlColl;
                if (_nonODPrl < 0)
                    _nonODPrl = 0;
                _loanOS = DisbAmt - PrlColl;

                /// calculate current principal demand
                if (Inst_Type == 1) /// Fixed principal
                {
                    _prlDemand = Inst_Amt;
                }

                _intFromDateTmp = _maxIntCalcDate;
                if (Inst_Type == 2) /// equated instalment
                {
                    //_oneMonthIntCalc = Calc_Int_OnVariable_ROI(_nonODSocAmt, LoanId, Utilities.AddMonths(_maxDueDate, -1), _maxDueDate, "S");
                    _oneMonthIntCalc = Calc_Int_OnVariable_ROI(_nonODSocAmt, LoanId, Utilities.AddMonths(toDate, -1), toDate, "S");

                    _prlDemand = Inst_Amt - _oneMonthIntCalc;
                    if (InterestCalculatAmt >= _oneMonthIntCalc)
                        _prlDemand = Inst_Amt - InterestCalculatAmt;
                    else
                        _prlDemand = Inst_Amt - _oneMonthIntCalc;
                }

                /// verify for current prl demand for both fixed principal and equated instalement 
                if (FirstPrl_DueDate != null)
                {
                    if (toDate.Date < FirstPrl_DueDate)
                        _prlDemand = 0;
                }
                else
                    _prlDemand = 0;

                /// if due date is greater thanlast due date then entire outstanding  will become demand
                if (LastDueDate != null)
                {
                    if (_maxDueDate >= LastDueDate)
                        _prlDemand = DisbAmt - TotalPrlDem; // lnDetails.Prl_Sched;
                }

                /// if demand is greater than non-od demand then demand will be non-od demand
                if (PrlColl >= _prlDemand) /// lnDetails.Prl_Sched
                {
                    if (_prlDemand >= DisbAmt - PrlColl)
                        _prlDemand = DisbAmt - PrlColl;
                }
                else
                    if (_prlDemand >= DisbAmt - TotalPrlDem)   ///  lnDetails.Prl_Sched
                    _prlDemand = DisbAmt - TotalPrlDem; /// lnDetails.Prl_Sched;

                if (_prlDemand < 0) _prlDemand = 0;

                if (_prlDemand > _loanOS)
                    _prlDemand = _loanOS;
                #endregion
            }
            catch (Exception)
            {
            }
            return _prlDemand;
        }
        public async Task<List<Loan_Disb>> GetLoanDisbursementList(decimal LoanId, DateTime FromDate, DateTime ToDate)
        {
            List<Loan_Disb> disbList = new List<Loan_Disb>();
            try
            {
                disbList = await CSISContext.Loan_Disb
            .Where(x => x.Loan_Id == LoanId &&
                        x.Disb_Date >= FromDate &&
                        x.Disb_Date <= ToDate &&
                        x.LoanDisb_Delete == false)
                .ToListAsync();
            }
            catch (Exception)
            {
            }
            return disbList;
        }
        public LoanDetailsHL GetLoanDetailsHL(decimal loanId)
        {
            LoanDetailsHL loanDetails = new LoanDetailsHL();
            string sql = "";
            try
            {
                sql = @"SELECT Loan_Master.Loan_id, 
                        Loan_Master.Loan_No,
                        Loan_Master.Mem_Id, 
                        Loan_Master.FirstPrl_DueDate,
                        Loan_Master.Prl_Prd,
                        Loan_Master.FirstInt_DueDate, 
                        Max(Loan_Trn.Trn_Date) AS Trn_Date,
                        Sum(Loan_Trn.PrlColl_Amt) AS PrlColl_Amt, 
                        Sum(Loan_Trn.Prl_Sched) AS Prl_Sched, 
                        Sum(Loan_Trn.Prl_Dem) AS Prl_Dem, 
                        Min(Loan_Trn.Disb_Date) AS Disb_Date,
                        Sum(Loan_Trn.Disb_Amt) as Disb_Amt,
                        Max(Loan_Trn.Due_Date) AS Due_Date, 
                        Max(Loan_Trn.IntCalc_Date) AS IntCalc_Date,
                        Sum(Loan_Trn.IntCalc_Amt) AS IntCalc_Amt,
                        Sum(Loan_Trn.IntColl_Amt) AS IntColl_Amt,
                        Sum(Loan_Trn.PICalc_Amt) AS PICalc_Amt,
                        Max(Loan_Trn.PICalc_Date) AS PICalc_Date,
                        Sum(Loan_Trn.PIColl_Amt) AS PIColl_Amt,
                        Sum(Loan_Trn.IODCalc_Amt) AS IODCalc_Amt,
                        Max(Loan_Trn.IODCalc_Date) AS IODCalc_Date,
                        Sum(Loan_Trn.IODColl_Amt) AS IODColl_Amt,
                        Loan_Master.Inst_Amt, 
                        Loan_Schemes.Inst_Type, 
                        Loan_Schemes.Dem_Frequency, 
                        Loan_Schemes.Int_Frequency,
                        Loan_Schemes.Int_Application, 
                        Loan_Schemes.PI_Application, 
                        Loan_Schemes.IOD_Application,
                        Loan_Schemes.Adv_Prl_Application,
                        Loan_Schemes.PrlLed_Id,
                        Loan_Schemes.IntLed_Id,
                        Loan_Schemes.PILed_Id,
                        Loan_Schemes.IODLed_Id,
                        Loan_Schemes.Scheme_Name
                        FROM(Loan_Trn INNER JOIN Loan_Master ON Loan_Trn.Loan_id = Loan_Master.Loan_id) INNER JOIN Loan_Schemes ON Loan_Master.Scheme_Id = Loan_Schemes.Scheme_Id
                        WHERE Loan_Trn.TrnTr_Delete = 0 AND Loan_Master.Loan_Delete = 0
                        GROUP BY Loan_Master.Loan_id, Loan_Master.Loan_No,Loan_Master.Mem_Id, Loan_Master.FirstPrl_DueDate,Loan_Master.Prl_Prd,Loan_Master.FirstInt_DueDate, Loan_Master.Inst_Amt, Loan_Schemes.Inst_Type, 
                        Loan_Schemes.Dem_Frequency, Loan_Schemes.Int_Frequency,Loan_Schemes.Int_Application, Loan_Schemes.PI_Application, Loan_Schemes.IOD_Application,Loan_Schemes.Adv_Prl_Application,
                        Loan_Schemes.PrlLed_Id,Loan_Schemes.IntLed_Id,Loan_Schemes.PILed_Id,Loan_Schemes.IODLed_Id,Loan_Schemes.Scheme_Name
                        Having(Sum([Loan_Trn].[Disb_Amt]) - Sum([Loan_Trn].[PrlColl_Amt]) > 0 OR Sum(Loan_Trn.IntCalc_Amt) -Sum(Loan_Trn.IntColl_Amt) > 0) 
                        AND Loan_Master.Loan_id = @loanId";
                var loanDetailsTmp =  (CSISContext.Database.SqlQueryRaw<LoanDetailsHL>(sql
                    , new NpgsqlParameter("@loanId", loanId))).FirstOrDefault();
                if (loanDetailsTmp != null) loanDetails = loanDetailsTmp;
            }
            catch (Exception)
            {
            }
            return loanDetails;
        }
        public double Calc_Int_OnVariable_ROI(double amt, decimal loanId, DateTime FromDate, DateTime ToDate, string Agency)
        {
            double roi = 0;
            DateTime tmpToDate;
            List<RateOfInterestVM> roilist = new List<RateOfInterestVM>();
            double intCalcAmt = 0;
            roi = CSISContext.Loan_Roi.Where(x => x.Loan_Id == loanId && x.Roi_Wef <= FromDate && x.Agency == Agency).Select(x => x.Roi).FirstOrDefault();
            //roi = GetLoanROI(loanid, FromDate, Agency);
            roilist = CSISContext.Loan_Roi
                     .Where(lr => lr.Loan_Id == loanId
                     && lr.Agency == Agency
                     && lr.Roi_Wef >= FromDate
                     && lr.Roi_Wef <= ToDate
                     && lr.Loanroi_Delete == false)
            .OrderBy(lr => lr.Roi_Wef)
            .Select(lr => new RateOfInterestVM
            {
                Roi = lr.Roi,
                Pi = lr.Pi,
                Roi_Wef = (DateTime)lr.Roi_Wef
            })
            .ToList();
            //roilist = GetLoanROI(loanid, FromDate, ToDate, Agency);
            foreach (RateOfInterestVM single in roilist)
            {
                tmpToDate = single.Roi_Wef;
                intCalcAmt += Utilities.Calculate_Interest(amt, roi, (int)(tmpToDate - FromDate).TotalDays);
                roi = single.Roi;
                FromDate = single.Roi_Wef;
            }
            intCalcAmt += Utilities.Calculate_Interest(amt, roi, (int)(ToDate - FromDate).TotalDays);
            return intCalcAmt;
        }
        public double Calc_PI_OnVariable_ROI(decimal loanId, DateTime FromDate, DateTime ToDate, double Amount, string Agency)
        {
            DateTime tmpToDate;
            double pi = 0;
            List<RateOfInterestVM> roilist = new List<RateOfInterestVM>();
            double intCalcAmt = 0;
            pi = CSISContext.Loan_Roi.Where(x => x.Loan_Id == loanId && x.Roi_Wef <= FromDate && x.Agency == Agency).Select(x => x.Pi).FirstOrDefault();
          
            roilist = CSISContext.Loan_Roi
                     .Where(lr => lr.Loan_Id == loanId
                     && lr.Agency == Agency
                     && lr.Roi_Wef >= FromDate
                     && lr.Roi_Wef <= ToDate
                     && lr.Loanroi_Delete == false)
            .OrderBy(lr => lr.Roi_Wef)
            .Select(lr => new RateOfInterestVM
            {
                Roi = lr.Roi,
                Pi = lr.Pi,
                Roi_Wef = (DateTime)lr.Roi_Wef
            })
            .ToList();

            
            foreach (RateOfInterestVM single in roilist)
            {
                tmpToDate = single.Roi_Wef;
                intCalcAmt += Utilities.Calculate_Interest(Amount, pi, (int)(tmpToDate - FromDate).TotalDays);
                pi = single.Pi;
                FromDate = single.Roi_Wef;
            }
            intCalcAmt += Utilities.Calculate_Interest(Amount, pi, (int)(ToDate - FromDate).TotalDays);
            return intCalcAmt;
        }
        public bool GetSocDisbAmt_And_FedDisAmt(decimal loanId, DateTime disbDateUpto,  out double _socDisbAmt, out double _fedDisAmt)
        {
            bool result = true;
            _socDisbAmt = 0; _fedDisAmt = 0; 
            try
            {
                double? socAmt = CSISContext.Database.SqlQueryRaw<double>(
                    @"Select sum(SocDisb_Amt) as SocDisAmt 
                    from Loan_Disb where LoanDisb_Delete = 0 AND Loan_id = @loanId
                    AND Disb_Date <= @disbDateUpto"
                    , new NpgsqlParameter("@loanId", loanId)
                    , new NpgsqlParameter("@disbDateUpto", disbDateUpto)).FirstOrDefault();
                double.TryParse(socAmt.ToString(), out _socDisbAmt);
                double? fedAmt = CSISContext.Database.SqlQueryRaw<double>(
                    @"Select sum(Reim_Amt) as FedDisAmt 
                    from Loan_Disb where LoanDisb_Delete = 0 AND Loan_id = @loanId
                    AND Disb_Date <= @disbDateUpto"
                    , new NpgsqlParameter("@loanId", loanId)
                    , new NpgsqlParameter("@disbDateUpto", disbDateUpto)).FirstOrDefault();
                double.TryParse(fedAmt.ToString(), out _fedDisAmt);
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }
        public bool Get_NonOD_And_OS_Soc_Fed(double _socDisbAmt, double _fedDisbAmt, double PrlColl, double PrlDemand, out double _nonODSocAmt, out double _nonODFedAmt, out double _socOS, out double _fedOS)
        {
            bool result = true;
            _nonODSocAmt = 0; _nonODFedAmt = 0; _socOS = 0; _fedOS = 0;
            double _prlCollOrPrlDem = 0;
            try
            {
                #region Get _nonODPrl and os for both soc disb and fed disb
                if (PrlColl >= PrlDemand)
                    _prlCollOrPrlDem = PrlColl;
                else
                    _prlCollOrPrlDem = PrlDemand;

                if (_socDisbAmt > 0)
                {
                    if (_prlCollOrPrlDem > _socDisbAmt)
                    {
                        _nonODSocAmt = 0;
                        _prlCollOrPrlDem -= _socDisbAmt;

                    }
                    else
                    {
                        _nonODSocAmt = _socDisbAmt - _prlCollOrPrlDem;
                        _prlCollOrPrlDem = 0;
                    }
                }
                else
                    _nonODSocAmt = 0;

                /// get non-od prl for fed
                if (_fedDisbAmt > 0)
                {
                    if (_prlCollOrPrlDem >= _fedDisbAmt)
                    {
                        _nonODFedAmt = 0;
                        _prlCollOrPrlDem -= _fedDisbAmt;
                    }
                    else
                    {
                        _nonODFedAmt = _fedDisbAmt - _prlCollOrPrlDem;
                        _prlCollOrPrlDem = 0;
                    }
                }
                else
                    _nonODFedAmt = 0;

                /// get os for soc
                _prlCollOrPrlDem = PrlColl;
                if (_socDisbAmt > 0)
                {
                    if (_prlCollOrPrlDem >= _socDisbAmt)
                    {
                        _socOS = 0;
                        _prlCollOrPrlDem -= _socDisbAmt;
                    }
                    else
                    {
                        _socOS = _socDisbAmt - _prlCollOrPrlDem;
                        _prlCollOrPrlDem = 0;
                    }
                }
                else
                    _socOS = 0;
                /// get os for fed
                if (_fedDisbAmt > 0)
                {
                    if (_prlCollOrPrlDem >= _fedDisbAmt)
                    {
                        _fedOS = 0;
                        _prlCollOrPrlDem -= _fedDisbAmt;
                    }
                    else
                    {
                        _fedOS = _fedDisbAmt - _prlCollOrPrlDem;
                        _prlCollOrPrlDem = 0;
                    }
                }
                else
                    _fedOS = 0;
                #endregion
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }
        public bool Get_PIIOD_ForNonDemandLoans(decimal loanId, DateTime dueDate, DateTime? maxPiCalcDate, DateTime piToDate, int piApplication, int iodApplication, double prlOD, double intOD,  out double piCalc, out double iodCalc)
        {
            bool result = true;
            piCalc = 0; iodCalc = 0;
            DateTime dueDateMinusOneMonth;

            try
            {
                if (dueDate > maxPiCalcDate)
                {
                    dueDateMinusOneMonth = dueDate.AddMonths(-1);
                    if (maxPiCalcDate == null) maxPiCalcDate = dueDateMinusOneMonth;
                    LoanDetailsHL lnDues = new LoanDetailsHL();
                    lnDues = CSISContext.Database.SqlQueryRaw<LoanDetailsHL>(
                        @"with Db1
                        AS
                        (
                        SELECT Sum(Prl_Dem) AS PrlDem,
                        0 AS IntCalc,
                        0 AS PrlColl,
                        0 AS IntColl
                        From Loan_Trn WHERE Loan_Id = @loanId AND Due_Date <@dueDate
                        AND TrnTr_Delete = 0
                        UNION ALL
                        SELECT 
                        0 AS PrlDem,
                        Sum(IntCalc_Amt) AS IntCalc,
                        0 AS PrlColl,
                        0 AS IntColl
                        From Loan_Trn WHERE Loan_Id = @loanId AND IntCalc_Date <=@dueDateMinusOneMonth
                        AND TrnTr_Delete = 0
                        UNION ALL
                        SELECT 0 AS PrlDem,
                        0 AS IntCalc,
                        Sum(PrlColl_Amt) AS PrlColl, 
                        Sum(IntColl_Amt) AS IntColl_Amt 
                        From Loan_Trn WHERE Loan_Id = @loanId
                        AND Trn_Date <=@dueDate AND TrnTr_Delete = 0
                        )
                        SELECT Sum(PrlDem) As Prl_Dem, Sum(IntCalc) AS intCalc_Amt, Sum(PrlColl) AS PrlColl_Amt, Sum(IntColl) as IntColl_Amt FROM Db1"
                        , new NpgsqlParameter("@loanId", loanId)
                        , new NpgsqlParameter("@dueDate", dueDate)
                        , new NpgsqlParameter("@dueDateMinusOneMonth", dueDateMinusOneMonth)).First();
                    if (lnDues != null)
                    {
                        prlOD = lnDues.Prl_Dem - lnDues.PrlColl_Amt;
                        intOD = lnDues.IntCalc_Amt - lnDues.IntColl_Amt;
                        if (prlOD < 0) prlOD = 0;
                        if (intOD < 0) intOD = 0;
                    }

                    double _socDisbAmtTmp = CSISContext.Database.SqlQueryRaw<double>(
                        @"select sum(SocDisb_Amt) - Sum(Reim_Amt) AS DisbAmt from Loan_Disb where loan_Id = @loanId AND Disb_Date <=@dueDate and LoanDisb_Delete = 0"
                        , new NpgsqlParameter("@loanId", loanId)
                        , new NpgsqlParameter("@duedate", dueDate)).FirstOrDefault();

                    double.TryParse(_socDisbAmtTmp.ToString(), out double _socDisbAmt);

                    #region Calculation of Penal Interest
                    switch (piApplication)
                    {
                        case 1: /// no pi
                            piCalc = 0;
                            break;
                        case 2:
                            if (prlOD > 0)
                            {
                                if (_socDisbAmt > 0)
                                {
                                    if (maxPiCalcDate >= dueDateMinusOneMonth)
                                        piCalc = Calc_PI_OnVariable_ROI(loanId, (DateTime)maxPiCalcDate, piToDate, prlOD, "S");
                                    else
                                        piCalc = Calc_PI_OnVariable_ROI(loanId, dueDateMinusOneMonth, piToDate, prlOD, "S");
                                }
                                else
                                {
                                    if (maxPiCalcDate >= dueDateMinusOneMonth)
                                        piCalc = Calc_PI_OnVariable_ROI(loanId, (DateTime)maxPiCalcDate, piToDate, prlOD, "F");
                                    else
                                        piCalc = Calc_PI_OnVariable_ROI(loanId, dueDateMinusOneMonth, piToDate, prlOD, "F");
                                }
                            }
                            else
                            {
                                piCalc = 0;
                            }
                            break;
                        case 3:
                            if (prlOD + intOD > 0)
                            {
                                if (_socDisbAmt > 0)
                                {
                                    if (maxPiCalcDate >= dueDateMinusOneMonth)
                                        piCalc = Calc_PI_OnVariable_ROI(loanId, (DateTime)maxPiCalcDate, piToDate, prlOD + intOD, "S");
                                    else
                                        piCalc = Calc_PI_OnVariable_ROI(loanId, dueDateMinusOneMonth, piToDate, prlOD + intOD, "S");
                                }
                                else
                                {
                                    if (maxPiCalcDate >= dueDateMinusOneMonth)
                                        piCalc = Calc_PI_OnVariable_ROI(loanId, (DateTime)maxPiCalcDate, piToDate, prlOD, "F");
                                    else
                                        piCalc = Calc_PI_OnVariable_ROI(loanId, dueDateMinusOneMonth, piToDate, prlOD, "F");
                                }
                            }
                            else
                            {
                                piCalc = 0;
                            }
                            break;
                    }
                    #endregion

                    #region Calculation of IOD
                    switch (iodApplication)
                    {
                        case 1: /// no iod
                            iodCalc = 0;
                            break;
                        case 2: /// on prl od
                            if (prlOD > 0)
                            {
                                if (_socDisbAmt > 0)
                                {
                                    if (maxPiCalcDate >= dueDateMinusOneMonth)
                                        iodCalc = Calc_Int_OnVariable_ROI(prlOD, loanId, (DateTime)maxPiCalcDate, piToDate, "S");
                                    else
                                        iodCalc = Calc_Int_OnVariable_ROI(prlOD, loanId, dueDateMinusOneMonth, piToDate, "S");
                                }
                                else
                                {
                                    if (maxPiCalcDate >= dueDateMinusOneMonth)
                                        iodCalc = Calc_Int_OnVariable_ROI(prlOD, loanId, (DateTime)maxPiCalcDate, piToDate, "F");
                                    else
                                        iodCalc = Calc_Int_OnVariable_ROI(prlOD, loanId, dueDateMinusOneMonth, piToDate, "F");
                                }
                            }
                            else
                            {
                                iodCalc = 0;
                            }
                            break;
                        case 3:  /// on prl od + int od
                            if (prlOD + intOD > 0)
                            {
                                if (_socDisbAmt > 0)
                                {
                                    if (maxPiCalcDate >= dueDateMinusOneMonth)
                                        iodCalc = Calc_Int_OnVariable_ROI(prlOD + intOD, loanId, (DateTime)maxPiCalcDate, piToDate, "S");
                                    else
                                        iodCalc = Calc_Int_OnVariable_ROI(prlOD + intOD, loanId, dueDateMinusOneMonth, piToDate, "S");
                                }
                                else
                                {
                                    if (maxPiCalcDate >= dueDateMinusOneMonth)
                                        iodCalc = Calc_Int_OnVariable_ROI(prlOD + intOD, loanId, (DateTime)maxPiCalcDate, piToDate, "F");
                                    else
                                        iodCalc = Calc_Int_OnVariable_ROI(prlOD + intOD, loanId, dueDateMinusOneMonth, piToDate, "F");
                                }
                            }
                            else
                            {
                                iodCalc = 0;
                            }
                            break;
                    }
                    #endregion
                }
                else
                {
                    piCalc = 0;
                    iodCalc = 0;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }
        #endregion

        
    }
}
