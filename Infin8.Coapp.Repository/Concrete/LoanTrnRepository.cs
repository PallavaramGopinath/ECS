using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.VisualBasic;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;

namespace Infin8.Coapp.Repository
{
    public class LoanTrnRepository : Repository<Loan_Trn>, ILoanTrnRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LoanTrnRepository(DbContext context) : base(context)
        {
        }

        #region CURD
        public async Task<bool> AddLoanTrnListAsync(List<Loan_Trn> loanTrnList)
        {
            bool result = false;
            decimal maxId = 0;
            int maxSlNo = 0;
            try
            {
                foreach (var loan in loanTrnList)
                {
                    maxId = await CSISContext.Loan_Trn.MaxAsync(x => x.Trn_Id);
                    maxId++;
                    maxSlNo = Get_MaxLoanSlNo(loan.Loan_Id);
                    loan.Trn_Id = maxId;
                    loan.Trn_SlNo = maxSlNo;
                    await AddAsync(loan);
                    CSISContext.SaveChanges();
                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan trn list not saved");
            }
            return result;
        }
        public async Task<bool> AddLoanTrn(Loan_Trn loanTrn)
        {
            bool result = false;
            decimal maxId = 0;
            try
            {
                maxId = await CSISContext.Loan_Trn.MaxAsync(x => x.Trn_Id);
                maxId++;
                loanTrn.Trn_Id = maxId;
                await AddAsync(loanTrn);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan trn list not saved");
            }
            return result;
        }

        public async Task<bool> EditLoanTrnListAsync(List<Loan_Trn> loanTrnList)
        {
            bool result = false;
            decimal maxId = 0;
            try
            {
                foreach (var loan in loanTrnList)
                {
                    loan.TrnTr_Delete = true;
                    await EditAsync(loan);
                }
                foreach (var loan in loanTrnList)
                {
                    maxId = maxId = await CSISContext.Loan_Trn.MaxAsync(x => x.Trn_Id);
                    loan.Trn_Id = maxId;
                    await AddAsync(loan);
                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan trn list not deleted");
            }
            return result;
        }
        #endregion 

        public async Task<List<DropdownItem>> GetLoanHavingOSItemsBySchemeIdAsync(int schemeId)
        {
            List<DropdownItem> result = new List<DropdownItem>();
            try
            {
                var LoanNoList = await (from trn in CSISContext.Loan_Trn
                                        join master in CSISContext.Loan_Master on trn.Loan_Id equals master.Loan_Id
                                        where trn.TrnTr_Delete == false && master.Scheme_Id == schemeId && master.Loan_Delete == false
                                        group new { trn, master } by new { trn.Loan_Id, master.Loan_No } into g
                                        where g.Sum(x => x.trn.Disb_Amt) - g.Sum(x => x.trn.PrlColl_Amt) > 0
                                        orderby g.Key.Loan_No
                                        select new DropdownItem
                                        {
                                            Value = g.Key.Loan_Id.ToString(),
                                            Text = g.Key.Loan_No
                                        }).ToListAsync();
                if (LoanNoList.Count > 0) result = LoanNoList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching loans having outstanding by scheme Id");
            }
            return result;
        }

        public async Task<double> GetJLExistingLoanOutstandingAsync(decimal memId, string brCode)
        {
            double balance = 0;
            try
            {
                double loanOS = await CSISContext.Loan_Master
                .Join(
                    CSISContext.Loan_Trn,
                    master => master.Loan_Id,
                    trn => trn.Loan_Id,
                    (master, trn) => new { Master = master, Trn = trn }
                )
                .Where(x => x.Master.Mem_Id == memId &&
                            x.Master.IsAccountClosed == false &&
                            x.Master.Loan_Delete == false &&
                            x.Master.BrCode == brCode &&
                            x.Trn.TrnTr_Delete == false &&
                            x.Trn.BrCode == brCode &&
                            x.Master.Loan_Type == 2)
                .GroupBy(x => new { x.Master.Mem_Id, x.Master.IsAccountClosed })
                .Select(g => g.Sum(x => x.Trn.Disb_Amt) - g.Sum(x => x.Trn.PrlColl_Amt))
                .FirstOrDefaultAsync();
                if (loanOS != 0) balance = loanOS;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching total existing jewel loan outstanding by member no");
            }
            return balance;
        }

        public async Task<List<DropdownItem>> GetLoanNosAsync(decimal memId, int loanType, string brCode)
        {
            List<DropdownItem> list = new List<DropdownItem>();
            try
            {
                var loanNoList = await CSISContext.Loan_Master
                .Join(
                    CSISContext.Loan_Trn,
                    f => f.Loan_Id,
                    g => g.Loan_Id,
                    (f, g) => new { f, g }
                )
                .Where(x => x.f.Mem_Id == memId &&
                            x.g.TrnTr_Delete == false &&
                            x.f.Loan_Delete == false &&
                            x.f.BrCode == brCode &&
                            x.f.Loan_Type == loanType)
                .GroupBy(x => new { x.f.Loan_Id, x.f.Loan_No })
                .Where(g => g.Sum(x => x.g.Disb_Amt) - g.Sum(x => x.g.PrlColl_Amt) > 0 ||
                            g.Sum(x => x.g.IntCalc_Amt) - g.Sum(x => x.g.IntColl_Amt) > 0)
                .Select(g => new DropdownItem
                {
                    Value = g.Key.Loan_Id.ToString(), // Cast Loan_id to string
                    Text = g.Key.Loan_No
                })
                .ToListAsync();
                if (loanNoList != null && loanNoList.Count > 0) list = loanNoList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching loan nos by member no");
            }
            return list;
        }

        public async Task<List<JewelLoanBalance>> GetJewelLoanNoBalanceAsync(decimal[] loanIdList, DateTime endDate, string brCode)
        {
            List<JewelLoanBalance> list = new List<JewelLoanBalance>();
            try
            {
                // Convert loanIdListString (comma-separated string) to a list of integers
                //var loanIdList = loanIdListString.Split(',').Select(int.Parse).ToList();

                var jlBalanceList = await (
                    from lm in CSISContext.Loan_Master
                    join lt in CSISContext.Loan_Trn on lm.Loan_Id equals lt.Loan_Id
                    join jl in CSISContext.JL_Details on lm.Loan_Id equals jl.Loan_Id
                    join ls in CSISContext.Loan_Schemes on lm.Scheme_Id equals ls.Scheme_Id
                    where lm.Loan_Delete == false &&
                          lt.TrnTr_Delete == false &&
                          jl.JL_Delete == false &&
                          loanIdList.Contains(lm.Loan_Id)
                    group new { lm, lt, jl, ls } by new
                    {
                        lm.Loan_Id,
                        lm.Loan_No,
                        lm.San_Amt,
                        lm.San_Date,
                        lm.Roi,
                        lm.Pi,
                        jl.JL_DueDate,
                        ls.Scheme_Name,
                        ls.PrlLed_Id,
                        ls.IntLed_Id,
                        ls.PILed_Id
                    } into g
                    select new JewelLoanBalance
                    {
                        Loan_Id = g.Key.Loan_Id,
                        Loan_No = g.Key.Loan_No,
                        San_Amt = g.Key.San_Amt,
                        San_Date = g.Key.San_Date,
                        Roi = g.Key.Roi,
                        Pi = g.Key.Pi,
                        JL_DueDate = g.Key.JL_DueDate,
                        Prl_Bal = (double)(g.Key.San_Amt - g.Sum(x => x.lt.PrlColl_Amt)),
                        Int_Bal = (double)(g.Sum(x => x.lt.IntCalc_Amt) - g.Sum(x => x.lt.IntColl_Amt)),
                        IntCalc_Date = g.Max(x => x.lt.IntCalc_Date),
                        PI_Bal = (double)(g.Sum(x => x.lt.PICalc_Amt) - g.Sum(x => x.lt.PIColl_Amt)),
                        PICalc_Date = g.Max(x => x.lt.PICalc_Date),
                        Scheme_Name = g.Key.Scheme_Name,
                        PrlLed_Id = g.Key.PrlLed_Id,
                        IntLed_Id = g.Key.IntLed_Id,
                        PILed_Id = g.Key.PILed_Id
                    }
                ).ToListAsync();
                if (jlBalanceList != null && jlBalanceList.Count > 0) list = jlBalanceList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching jewel loan balance by member no");
            }
            return list;
        }

        public async Task<List<LoanDetailsHL>> GetLoanDetailsListByLoanIdsHSISAsync(decimal[] loanIds, DateTime trnDate, int intCalcType, int societyType)
        {
            DateTime endDate = trnDate;
            DateTime fromDate;
            DateTime _maxDueDate;
            DateTime _maxIntCalcDate;
            DateTime _previourDueDate;
            double _oneMonthIntCalc = 0, _nonODPrl = 0, _loanOS = 0;
            double _prlDemand = 0;
            double _socDisbAmt = 0, _fedDisbAmt = 0;
            double _socOS = 0, _fedOS = 0;
            double _nonODSocAmt = 0, _nonODFedAmt = 0;
            DateTime _intFromDateTmp;
            DateTime prlDueDate = trnDate;
            List<LoanDetailsHL> loanList = new List<LoanDetailsHL>();
            LoanInterestCalculatedItems intCalcItems = new LoanInterestCalculatedItems();
            try
            {
                loanList = await GetLoanDetailsListByLoanIdsAsync(loanIds);
                foreach (var loan in loanList)
                {

                    if (loan.Due_Date != null)
                        _previourDueDate = (DateTime)loan.Due_Date.Value.Date;
                    else
                        if (loan.FirstPrl_DueDate != null)
                            _previourDueDate = ((DateTime)loan.FirstPrl_DueDate).Date;
                        else
                            _previourDueDate = loan.FirstInt_DueDate;
                    switch (intCalcType)
                    {
                        case 1: /// due date
                            endDate = loan.IntCalc_Date != null ? (DateTime)loan.IntCalc_Date : loan.FirstInt_DueDate;
                            break;
                        case 2: /// upto current date
                            endDate = trnDate;
                            if (loan.Due_Date != null)
                            {
                                if ((DateTime)loan.Due_Date > endDate)
                                    endDate = (DateTime)loan.Due_Date;
                            }

                            break;
                        case 3: /// upto end of the month

                            //DateTime prlDueDate;
                            if (loan.Due_Date != null)
                            {
                                prlDueDate = Utilities.AddMonths(Convert.ToDateTime(loan.Due_Date), loan.Dem_Frequency);
                                endDate = prlDueDate;
                                if ((DateTime)loan.Due_Date > trnDate)
                                {
                                    endDate = (DateTime)loan.Due_Date;
                                    prlDueDate = (DateTime)loan.Due_Date;
                                }
                            }
                            else
                            {
                                prlDueDate = new DateTime(trnDate.Year, trnDate.Month, 1);
                                prlDueDate = prlDueDate.AddMonths(1);
                                endDate = prlDueDate;
                                //prlDueDate = loanDetails.FirstInt_DueDate;
                                //endDate = loanDetails.FirstInt_DueDate;
                            }
                            //if (prlDueDate >= trnDate)
                            //    endDate = prlDueDate;
                            //else
                            //    endDate = trnDate;
                            break;
                        case 4: /// raise prl demand for pldb
                            if (loan.Due_Date != null)
                                prlDueDate = Utilities.AddMonths((DateTime)loan.Due_Date, loan.Dem_Frequency);
                            else
                                prlDueDate = loan.FirstInt_DueDate;
                            if (loan.Adv_Prl_Application == 1) /// do not raise advance prl, if prldue date is greater than trn date then, end date is trndate
                            {
                                if (prlDueDate > trnDate)
                                    endDate = trnDate;
                                else
                                    endDate = prlDueDate;
                            }
                            else
                            {
                                if (prlDueDate >= trnDate)
                                    endDate = prlDueDate;
                                else
                                    endDate = trnDate;
                            }
                            break;
                    }

                    if (loan.IntCalc_Date != null)
                        fromDate = (DateTime)loan.IntCalc_Date;
                    else
                        fromDate = loan.Disb_Date;
                    if (loan.Due_Date != null)
                        if (loan.Due_Date.Value.Date >= trnDate.Date)
                            _maxDueDate = (DateTime)loan.Due_Date;
                        else
                            _maxDueDate = Utilities.AddMonths((DateTime)loan.Due_Date, loan.Dem_Frequency);
                    else
                        if (loan.FirstPrl_DueDate != null)
                            _maxDueDate = ((DateTime)loan.FirstPrl_DueDate).Date;
                        else
                            _maxDueDate = loan.FirstInt_DueDate.Date;
                    //_maxDueDate = loanDetails.FirstInt_DueDate;
                    _prlDemand = 0; _nonODPrl = 0; _loanOS = 0; _oneMonthIntCalc = 0;


                    //if (endDate >= _maxDueDate)
                    //{
                    _maxIntCalcDate = loan.IntCalc_Date != null ? (DateTime)loan.IntCalc_Date : loan.Disb_Date;

                    intCalcItems = await GetCalculatedInterestComponentsForLoanAsync(loan.Loan_Id, loan.FirstInt_DueDate, loan.Trn_Date, loan.Due_Date, _maxIntCalcDate, loan.PICalc_Date, endDate, trnDate, loan.Int_Application, loan.PI_Application, loan.IOD_Application, loan.Disb_Amt, loan.PrlColl_Amt, loan.Prl_Sched, loan.Prl_Dem, loan.IntCalc_Amt, loan.IntColl_Amt, "S");

                    loan.IntCalc_AmtCurrent = intCalcItems.InterestCalculatAmt;
                    loan.IntCalc_DateCurrent = Convert.ToDateTime(intCalcItems.InterestCalculateDate);
                    loan.PICalc_AmtCurrent = intCalcItems.PICalculateAmt;
                    loan.PICalc_DateCurrent = intCalcItems.PICalcuateDate;
                    loan.IODCalc_AmtCurrent = intCalcItems.IODCalculateAmt;
                    loan.IODCalc_DateCurrent = intCalcItems.IODCalculateDate;

                    (_socDisbAmt, _fedDisbAmt) = await GetSocDisbAmt_And_FedDisAmt(loan.Loan_Id, endDate);

                    (_nonODSocAmt, _nonODFedAmt, _socOS, _fedOS) = GetNonODAndOutstandingForSocietyAndFederationLoans(_socDisbAmt, _fedDisbAmt, loan.PrlColl_Amt, loan.Prl_Dem);
                    if (endDate >= loan.FirstPrl_DueDate)
                    {
                        _nonODPrl = loan.Disb_Amt - loan.PrlColl_Amt;
                        if (_nonODPrl < 0)
                            _nonODPrl = 0;
                        _loanOS = loan.Disb_Amt - loan.PrlColl_Amt;
                        _loanOS = Math.Round(_loanOS, 2);

                        /// calculate current principal demand
                        if (loan.Inst_Type == 1) /// Fixed principal
                        {
                            if (_loanOS > 0)
                            {
                                _prlDemand = loan.Inst_Amt;
                                if (_prlDemand >= _loanOS)
                                    _prlDemand = _loanOS;
                            }
                            if (loan.Prl_Dem >= loan.Disb_Amt)
                                _prlDemand = 0;
                        }
                        _intFromDateTmp = _maxIntCalcDate;
                        if (loan.Inst_Type == 2) /// equated instalment
                        {
                            double? intcalcDuringDueDate = (from lt in CSISContext.Loan_Trn
                                                            where lt.Loan_Id == loan.Loan_Id
                                                                  && lt.TrnTr_Delete == false
                                                                  && lt.IntCalc_Date > _maxDueDate.AddMonths(-1)
                                                            select lt.IntCalc_Amt).DefaultIfEmpty(0).Sum();

                            //double? intcalcDuringDueDate = CSISContext.Database.SqlQueryRaw<double?>(
                            //    @"select sum(intcalc_amt) as intcalc from loan_trn where loan_id = @loanId and TrnTr_Delete = 0 and IntCalc_Date > @intCalcDate"
                            //    , new NpgsqlParameter("@loanId", loan.Loan_Id)
                            //    , new NpgsqlParameter("@intCalcDate", _maxDueDate.AddMonths(-1))).FirstOrDefault();

                            if (intcalcDuringDueDate == null)
                                intcalcDuringDueDate = 0;
                            _oneMonthIntCalc = CalculateCurrentInterestDemand(loan.Loan_Id, _nonODSocAmt, _nonODFedAmt, prlDueDate.AddMonths(-1), prlDueDate);

                            //_oneMonthIntCalc = Raise_Curr_Int_Demand(loan.Loan_Id, _nonODSocAmt, _nonODFedAmt, prlDueDate.AddMonths(-1), prlDueDate, context, out errorMessage);
                            //if (errorMessage.Length > 0)
                            //{
                            //    goto ErrorHandler;
                            //}

                            _prlDemand = loan.Inst_Amt - _oneMonthIntCalc;
                            if (endDate < loan.FirstPrl_DueDate)
                                _prlDemand = 0;
                            if (_prlDemand >= _loanOS)
                                _prlDemand = _loanOS;
                            if (loan.Prl_Dem >= loan.Disb_Amt)
                                _prlDemand = 0;
                            /// if demand already raised as advance once, then make prl demad = 0
                            if (loan.Due_Date != null)
                            {
                                /// 22-06-2023 correction
                                //if (loanDetails.Due_Date >= _maxDueDate)
                                if (loan.Due_Date >= prlDueDate)
                                {
                                    _prlDemand = 0;
                                }
                            }
                        }

                        /// for pldb do not raise prl demand in advance
                        if (societyType == 3)    /// pldb
                        {
                            prlDueDate = Utilities.AddMonths(loan.Due_Date != null ? (DateTime)loan.Due_Date : (DateTime)loan.FirstPrl_DueDate, loan.Dem_Frequency);
                            /// if trn date is less than prl due date, then do not raise prl demand
                            if (trnDate > prlDueDate) _prlDemand = 0;

                            if (loan.Adv_Prl_Application == 1)   /// do not raise prl demand on adv prl collection
                            {
                                if (loan.PrlColl_Amt - loan.Prl_Sched > 0)
                                {
                                    _prlDemand = _prlDemand - (loan.PrlColl_Amt - loan.Prl_Sched);
                                    if (_prlDemand < 0) _prlDemand = 0;
                                }
                            }
                        }
                    }

                    loan.Prl_DemCurrent = _prlDemand;

                    //loanDetailsList.Add(loan);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching laon balance by member no");
            }
            return loanList;
        }

        public async Task<List<LoanDetailsHL>> GetLoanDetailsListByLoanIdsAsync(decimal[] loanIds)
        {
            List<LoanDetailsHL> list = new List<LoanDetailsHL>();
            try
            {
                var result = await (from lm in CSISContext.Loan_Master
                                    join lt in CSISContext.Loan_Trn on lm.Loan_Id equals lt.Loan_Id
                                    join ls in CSISContext.Loan_Schemes on lm.Scheme_Id equals ls.Scheme_Id
                                    where lt.TrnTr_Delete == false && lm.Loan_Delete == false
                                    group new { lm, lt, ls } by new
                                    {
                                        lm.Loan_Id,
                                        lm.Loan_No,
                                        lm.Mem_Id,
                                        lm.FirstPrl_DueDate,
                                        lm.Prl_Prd,
                                        lm.FirstInt_DueDate,
                                        lm.Inst_Amt,
                                        ls.Inst_Type,
                                        ls.Dem_Frequency,
                                        ls.Int_Frequency,
                                        ls.Int_Application,
                                        ls.PI_Application,
                                        ls.IOD_Application,
                                        ls.Adv_Prl_Application,
                                        ls.PrlLed_Id,
                                        ls.IntLed_Id,
                                        ls.PILed_Id,
                                        ls.IODLed_Id,
                                        ls.Scheme_Name
                                    } into g
                                    where (g.Sum(x => x.lt.Disb_Amt) - g.Sum(x => x.lt.PrlColl_Amt) > 0) ||
                                          (g.Sum(x => x.lt.IntCalc_Amt) - g.Sum(x => x.lt.IntColl_Amt) > 0)
                                    where loanIds.Contains(g.Key.Loan_Id)
                                    select new LoanDetailsHL
                                    {
                                        Loan_Id = g.Key.Loan_Id,
                                        Loan_No = g.Key.Loan_No,
                                        Mem_Id = g.Key.Mem_Id,
                                        FirstPrl_DueDate = g.Key.FirstPrl_DueDate,
                                        Prl_Prd = g.Key.Prl_Prd,
                                        FirstInt_DueDate = Convert.ToDateTime(g.Key.FirstInt_DueDate),
                                        Trn_Date = g.Max(x => x.lt.Trn_Date),
                                        PrlColl_Amt = g.Sum(x => x.lt.PrlColl_Amt),
                                        Prl_Sched = g.Sum(x => x.lt.Prl_Sched),
                                        Prl_Dem = g.Sum(x => x.lt.Prl_Dem),
                                        Disb_Date = Convert.ToDateTime(g.Min(x => x.lt.Disb_Date)),
                                        Disb_Amt = g.Sum(x => x.lt.Disb_Amt),
                                        Due_Date = g.Max(x => x.lt.Due_Date),
                                        IntCalc_Date = g.Max(x => x.lt.IntCalc_Date),
                                        IntCalc_Amt = g.Sum(x => x.lt.IntCalc_Amt),
                                        IntColl_Amt = g.Sum(x => x.lt.IntColl_Amt),
                                        PICalc_Amt = g.Sum(x => x.lt.PICalc_Amt),
                                        PICalc_Date = g.Max(x => x.lt.PICalc_Date),
                                        PIColl_Amt = g.Sum(x => x.lt.PIColl_Amt),
                                        IODCalc_Amt = g.Sum(x => x.lt.IODCalc_Amt),
                                        IODCalc_Date = g.Max(x => x.lt.IODCalc_Date),
                                        IODColl_Amt = g.Sum(x => x.lt.IODColl_Amt),
                                        Inst_Amt = g.Key.Inst_Amt,
                                        Inst_Type = g.Key.Inst_Type,
                                        Dem_Frequency = g.Key.Dem_Frequency,
                                        Int_Frequency = g.Key.Int_Frequency,
                                        Int_Application = g.Key.Int_Application,
                                        PI_Application = g.Key.PI_Application,
                                        IOD_Application = g.Key.IOD_Application,
                                        Adv_Prl_Application = g.Key.Adv_Prl_Application,
                                        PrlLed_Id = g.Key.PrlLed_Id,
                                        IntLed_Id = g.Key.IntLed_Id,
                                        PILed_Id = g.Key.PILed_Id,
                                        IODLed_Id = g.Key.IODLed_Id,
                                        Scheme_Name = g.Key.Scheme_Name
                                    }).ToListAsync();
                if (result.Count > 0) list = result.ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching loan balance by loan Id");
            }
            return list;
        }

        public async Task<List<DropdownItem>> GetLoanNosByMemIdAndLoanTypeAsync(decimal memId, int loanType)
        {
            List<DropdownItem> result = new List<DropdownItem>();
            try
            {
                var loanNoList = await (from f in CSISContext.Loan_Master
                                        join g in CSISContext.Loan_Trn on f.Loan_Id equals g.Loan_Id
                                        where f.Mem_Id == memId
                                              && g.TrnTr_Delete == false
                                              && f.Loan_Delete == false
                                              && f.Loan_Type == loanType
                                        group new { f, g } by new { f.Loan_Id, f.Loan_No } into grouped
                                        let totalDisbAmt = grouped.Sum(x => x.g.Disb_Amt)
                                        let totalPrlCollAmt = grouped.Sum(x => x.g.PrlColl_Amt)
                                        let totalIntCalcAmt = grouped.Sum(x => x.g.IntCalc_Amt)
                                        let totalIntCollAmt = grouped.Sum(x => x.g.IntColl_Amt)
                                        where (totalDisbAmt - totalPrlCollAmt) > 0 || (totalIntCalcAmt - totalIntCollAmt) > 0
                                        select new DropdownItem
                                        {
                                            Value = grouped.Key.Loan_Id.ToString(),
                                            Text = grouped.Key.Loan_No
                                        }).ToListAsync();
                if (loanNoList != null && loanNoList.Count > 0) result = loanNoList.ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching loan no(s) by memId and loan type");
            }
            return result;
        }

        public async Task<LoanInterestCalculatedItems> GetCalculatedInterestComponentsForLoanAsync(decimal loanId, DateTime firstIntDueDate, DateTime maxTrnDate, DateTime? maxDueDate, DateTime intFromDate, DateTime? piFromDate, DateTime toDate, DateTime piToDate, int Int_Application, int PI_Application, int IOD_Application, double DisbAmt, double PrlColl, double PrlSchedule, double PrlDemand, double IntCalcAmt, double IntCollAmt, string DisbAgency)
        {

            LoanInterestCalculatedItems calcLnDues = new LoanInterestCalculatedItems();
            double LoanOS = DisbAmt - PrlColl;
            double NonODPrl = DisbAmt - PrlDemand;
            double PrlOD = PrlDemand - PrlColl;
            double IntOD = IntCalcAmt - IntCollAmt;
            double _nonODSocAmt = 0, _nonODFedAmt = 0, _prlCollOrPrlDem = 0;
            double _socOS = 0, _fedOS = 0;
            double _socDisbAmt = 0, _fedDisbAmt = 0;
            double _intCalc = 0;
            DateTime _intFromDateTmp = intFromDate;
            DateTime _piFromDate = DateTime.Now;
            DateTime _iodFromDate = DateTime.Now;
            DateTime _NextDueDate = DateTime.Now;
            DateTime _intToDate = toDate;
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
                disbList = GetLoanDisbursementList(loanId, intFromDate, toDate);

                (_socDisbAmt, _fedDisbAmt) = await GetSocDisbAmt_And_FedDisAmt(loanId, _iodFromDate);

                (_nonODSocAmt, _nonODFedAmt, _socOS, _fedOS) = Get_NonOD_And_OS_Soc_Fed(_socDisbAmt, _fedDisbAmt, PrlColl, PrlDemand);


                /// calculate Interest
                #region calculate interest

                foreach (Loan_Disb disb in disbList)
                {
                    /// fix to date 

                    if (Int_Application == 1)    /// interest on non-od principal
                    {
                        _intCalc += CalculateCurrentInterestDemand(loanId, _nonODSocAmt, _nonODFedAmt, intFromDate, Convert.ToDateTime(disb.Disb_Date));
                    }
                    if (Int_Application == 2)    /// interest on prl outstanding
                    {
                        _intCalc += CalculateCurrentInterestDemand(loanId, _socOS, _fedOS, intFromDate, Convert.ToDateTime(disb.Disb_Date));
                    }
                    _nonODSocAmt += disb.SocDisb_Amt;
                    _nonODFedAmt += disb.Reim_Amt;
                    _socOS += disb.SocDisb_Amt;
                    _fedOS += disb.Reim_Amt;
                    intFromDate = Convert.ToDateTime(disb.Disb_Date);
                }
                if (Int_Application == 1)    /// interest on on-od principal
                {
                    _intCalc += CalculateCurrentInterestDemand(loanId, _nonODSocAmt, _nonODFedAmt, intFromDate, toDate);
                }
                if (Int_Application == 2)    /// interest on prl outstanding
                {
                    _intCalc += CalculateCurrentInterestDemand(loanId, _socOS, _fedOS, intFromDate, toDate);
                }
                calcLnDues.InterestCalculatAmt = _intCalc;
                calcLnDues.InterestCalculateDate = toDate;
                #endregion

                #region calcualte PI And IOD Calculation

                _piFromDate = maxTrnDate;
                /// CALCULATION OF PI TO BE STUDIED IN DEPTH  2022-04-28
                /// Modified on 2022-04-29
                //_piFromDate = MaxTrn_Date;

                /// if maximum due date is greater than current receipt date
                /// get previous of MaxOfDue_Date Prl and Int balances and calculate PI,IOD from MaxOfPICalc_Date to PIToDate

                if (maxDueDate > piToDate)
                {
                    (_piCalc, _iodCalc) = await GetPIIODForNonDemandLoans(loanId, maxDueDate == null ? firstIntDueDate : Convert.ToDateTime(maxDueDate), piFromDate, piToDate, PI_Application, IOD_Application, PrlOD, IntOD);

                    calcLnDues.PICalculateAmt = _piCalc;
                    calcLnDues.IODCalculateAmt = _iodCalc;
                    calcLnDues.PICalcuateDate = piToDate.Date;
                    calcLnDues.IODCalculateDate = piToDate.Date;
                }
                else
                {
                    if (piFromDate == null)
                        if (maxDueDate != null)
                            piFromDate = (DateTime)maxDueDate;
                    if (piFromDate != null)
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
                                if (maxDueDate >= piFromDate)
                                    calcLnDues.PICalculateAmt = CalculatePIOnVariableRate(PrlOD, loanId, Convert.ToDateTime(maxDueDate), piToDate, "S");
                                //calcLnDues.PICalculateAmt = Calc_PI_OnVariable_ROI(loanId, (DateTime)MaxDueDate, piToDate, PrlOD, "S");
                                else
                                    calcLnDues.PICalculateAmt = CalculatePIOnVariableRate(PrlOD, loanId, (DateTime)piFromDate, piToDate, "S");
                                //calcLnDues.PICalculateAmt = Calc_PI_OnVariable_ROI(loanId, (DateTime)piFromDate, piToDate, PrlOD, "S");
                                if (calcLnDues.PICalculateAmt > 0)
                                    calcLnDues.PICalcuateDate = piToDate.Date;
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
                                if (maxDueDate >= piFromDate)
                                    calcLnDues.PICalculateAmt = CalculatePIOnVariableRate(PrlOD + IntOD, loanId, Convert.ToDateTime(maxDueDate), piToDate, "S");
                                //calcLnDues.PICalculateAmt = Calc_PI_OnVariable_ROI(loanId, (DateTime)MaxDueDate, piToDate, PrlOD + IntOD, "S");
                                else
                                    calcLnDues.PICalculateAmt = CalculatePIOnVariableRate(PrlOD + IntOD, loanId, (DateTime)piFromDate, piToDate, "S");
                                //calcLnDues.PICalculateAmt = Calc_PI_OnVariable_ROI(loanId, (DateTime)piFromDate, piToDate, PrlOD + IntOD, "S");
                                if (calcLnDues.PICalculateAmt > 0)
                                    calcLnDues.PICalcuateDate = piToDate.Date;
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

                        _iodFromDate = maxTrnDate;

                        if (IOD_Application == 1)    /// no iod
                        {
                            calcLnDues.IODCalculateAmt = 0;
                            calcLnDues.IODCalculateDate = null;
                        }
                        else if (IOD_Application == 2) /// iod on prlod
                        {
                            if (PrlOD > 0)
                            {
                                if (maxDueDate >= piFromDate)
                                    calcLnDues.IODCalculateAmt = CalculateInterestOnVariableRate(PrlOD, loanId, (DateTime)maxDueDate, piToDate, "S");
                                //calcLnDues.IODCalculateAmt = Calc_Int_OnVariable_ROI(PrlOD, loanId, (DateTime)maxDueDate, piToDate, "S");
                                else
                                    calcLnDues.IODCalculateAmt = CalculateInterestOnVariableRate(PrlOD, loanId, (DateTime)piFromDate, piToDate, "S");
                                //calcLnDues.IODCalculateAmt = Calc_Int_OnVariable_ROI(PrlOD, loanId, (DateTime)PIFromDate, PiToDate, "S");
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
                                //if (_socDisbAmt > 0)
                                if (maxDueDate >= piFromDate)
                                    calcLnDues.IODCalculateAmt = CalculateInterestOnVariableRate(PrlOD + IntOD, loanId, (DateTime)maxDueDate, piToDate, "S");
                                //calcLnDues.IODCalculateAmt = Calc_Int_OnVariable_ROI(PrlOD + IntOD, loanId, (DateTime)maxDueDate, piToDate, "S");
                                else
                                    calcLnDues.IODCalculateAmt = CalculateInterestOnVariableRate(PrlOD + IntOD, loanId, (DateTime)piFromDate, piToDate, "S");
                                //calcLnDues.IODCalculateAmt = Calc_Int_OnVariable_ROI(PrlOD + IntOD, loanId, (DateTime)PIFromDate, PiToDate, "S");
                            }
                            else
                            {
                                calcLnDues.IODCalculateAmt = 0;
                                calcLnDues.IODCalculateDate = null;
                            }

                        }
                        if (calcLnDues.IODCalculateAmt > 0)
                            calcLnDues.IODCalculateDate = piToDate.Date;
                        else
                            calcLnDues.IODCalculateDate = null;
                        #endregion
                    }
                }
                #endregion
            }
            catch (Exception)
            {
                //errorMessage = ex.Message;
            }
            return calcLnDues;

        }

        public List<Loan_Disb> GetLoanDisbursementList(decimal LoanId, DateTime fromDate, DateTime toDate)
        {
            List<Loan_Disb> disbList = new List<Loan_Disb>();
            disbList = (from ld in CSISContext.Loan_Disb
                        where ld.Loan_Id == LoanId
                              && ld.LoanDisb_Delete == false
                              && (ld.Disb_Date > fromDate && ld.Disb_Date <= toDate)
                        orderby ld.Disb_Date ascending, ld.Disb_SlNo ascending
                        select ld).ToList();
            return disbList;
        }

        public async Task<(double _socDisbAmt, double _fedDisbAmt)> GetSocDisbAmt_And_FedDisAmt(decimal loanId, DateTime disbDateUpto)
        {
            double _socDisbAmt = 0, _fedDisbAmt = 0;
            double? socAmt = await CSISContext.Loan_Disb
            .Where(ld => !ld.LoanDisb_Delete
                   && ld.Loan_Id == loanId
                   && ld.Disb_Date <= disbDateUpto)
            .SumAsync(ld => (double?)ld.SocDisb_Amt);
            double.TryParse(socAmt.ToString(), out _socDisbAmt);

            double? fedAmt = await CSISContext.Loan_Disb
            .Where(ld => !ld.LoanDisb_Delete
                   && ld.Loan_Id == loanId
                   && ld.Disb_Date <= disbDateUpto)
            .SumAsync(ld => (double?)ld.Reim_Amt);
            double.TryParse(fedAmt.ToString(), out _fedDisbAmt);
            return (_socDisbAmt, _fedDisbAmt);
        }

        public (double _nonODSocAmt, double _nonODFedAmt, double _socOS, double _fedOS) Get_NonOD_And_OS_Soc_Fed(double _socDisbAmt, double _fedDisbAmt, double PrlColl, double PrlDemand)
        {
            //, out double _nonODSocAmt, out double _nonODFedAmt, out double _socOS, out double _fedOS, out string errorMessage
            double _nonODSocAmt = 0, _nonODFedAmt = 0, _socOS = 0, _fedOS = 0;
            //errorMessage = "";
            double _prlCollOrPrlDem = 0;

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
            return (_nonODSocAmt, _nonODFedAmt, _socOS, _fedOS);
        }

        public double CalculateCurrentInterestDemand(decimal loanId, double _socAmt, double _fedAmt, DateTime _fromDate, DateTime _toDate)
        {
            /// _socAmt may be nonODSocAmt or socOS
            /// _fedAmt may be nonODFedAmt or fedOS
            double _intCalc = 0;
            DateTime _intFromDateTmp;
            List<Loan_Disb> disbList = new List<Loan_Disb>();

            disbList = GetLoanDisbursementList(loanId, _fromDate, _toDate);

            _intFromDateTmp = _fromDate;
            foreach (var disb in disbList)
            {
                _intCalc += CalculateInterestByVariableRateOfInterest(_socAmt, loanId, _intFromDateTmp, Convert.ToDateTime(disb.Disb_Date), "S");

                _intCalc += CalculateInterestByVariableRateOfInterest(_fedAmt, loanId, _intFromDateTmp, Convert.ToDateTime(disb.Disb_Date), "F");
                if (_intFromDateTmp > Convert.ToDateTime(disb.Disb_Date))
                {
                    _socAmt += disb.SocDisb_Amt;
                    _fedAmt += disb.Reim_Amt;
                }
                _intFromDateTmp = Convert.ToDateTime(disb.Disb_Date);
            }
            _intCalc += CalculateInterestByVariableRateOfInterest(_socAmt, loanId, _intFromDateTmp, _toDate.Date, "S");

            _intCalc += CalculateInterestByVariableRateOfInterest(_fedAmt, loanId, _intFromDateTmp, _toDate.Date, "F");
            return _intCalc;
        }

        public double CalculateInterestOnVariableRate(double amount, decimal loanId, DateTime fromDate, DateTime toDate, string Agency)
        {
            double rateOfInterest = 0;
            DateTime tmpToDate;
            List<RateOfInterestVM> rateOfInterestlist = new List<RateOfInterestVM>();
            double intCalcAmt = 0;
            rateOfInterest = GetLoanRateOfInterest(loanId, fromDate, Agency);
            rateOfInterestlist = GetLoanRateOfInterest(loanId, fromDate, toDate, Agency);
            foreach (var roi in rateOfInterestlist)
            {
                tmpToDate = roi.Roi_Wef;
                intCalcAmt += Utilities.Calculate_Interest(amount, rateOfInterest, (int)(tmpToDate - fromDate).TotalDays); //  Utilities.Calculate_Interest(amt, roi, (int)(tmpToDate - FromDate).TotalDays);
                rateOfInterest = roi.Roi;
                fromDate = roi.Roi_Wef;
            }
            intCalcAmt += Utilities.Calculate_Interest(amount, rateOfInterest, (int)(toDate - fromDate).TotalDays);
            return intCalcAmt;
        }

        public double CalculatePIOnVariableRate(double amount, decimal loanid, DateTime FromDate, DateTime ToDate, string Agency)
        {
            double rateOfInterest = 0;
            DateTime tmpToDate;
            List<RateOfInterestVM> rateOfInterestlist = new List<RateOfInterestVM>();
            double intCalcAmt = 0;
            rateOfInterest = GetLoanRateOfInterest(loanid, FromDate, Agency);
            rateOfInterestlist = GetLoanRateOfInterest(loanid, FromDate, ToDate, Agency);
            foreach (var roi in rateOfInterestlist)
            {
                tmpToDate = roi.Roi_Wef;
                intCalcAmt += Utilities.Calculate_Interest(amount, rateOfInterest, (int)(tmpToDate - FromDate).TotalDays); //  Utilities.Calculate_Interest(amt, roi, (int)(tmpToDate - FromDate).TotalDays);
                rateOfInterest = roi.Pi;
                FromDate = roi.Roi_Wef;
            }
            intCalcAmt += Utilities.Calculate_Interest(amount, rateOfInterest, (int)(ToDate - FromDate).TotalDays);
            return intCalcAmt;
        }

        public (double _nonODSocAmt, double _nonODFedAmt, double _socOS, double _fedOS) GetNonODAndOutstandingForSocietyAndFederationLoans(double _socDisbAmt, double _fedDisbAmt, double PrlColl, double PrlDemand)
        {
            /// , out double _nonODSocAmt, out double _nonODFedAmt, out double _socOS, out double _fedOS
            double _nonODSocAmt = 0, _nonODFedAmt = 0, _socOS = 0, _fedOS = 0;
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
            }
            return (_nonODSocAmt, _nonODFedAmt, _socOS, _fedOS);
        }

        public double CalculateInterestByVariableRateOfInterest(double amount, decimal loanId, DateTime fromDate, DateTime toDate, string agency)
        {
            double rateOfInterest = 0;
            DateTime tmpToDate;
            List<RateOfInterestVM> rateOfInterestlist = new List<RateOfInterestVM>();
            double calculatedInterest = 0;
            rateOfInterest = GetLoanRateOfInterest(loanId, fromDate, agency);
            rateOfInterestlist = GetLoanRateOfInterest(loanId, fromDate, toDate, agency);
            foreach (var roi in rateOfInterestlist)
            {
                tmpToDate = roi.Roi_Wef;
                calculatedInterest += Utilities.Calculate_Interest(amount, rateOfInterest, (int)(tmpToDate - fromDate).TotalDays); ///.TotalDays)  Utilities.Calculate_Interest(amt, roi, (int)(tmpToDate - FromDate).TotalDays);
                rateOfInterest = roi.Roi;
                fromDate = roi.Roi_Wef;
            }
            calculatedInterest += Utilities.Calculate_Interest(amount, rateOfInterest, (int)(toDate - fromDate).TotalDays); ///  Utilities.Calculate_Interest(amt, roi, (int)(ToDate - FromDate).TotalDays);
            return calculatedInterest;
        }

        public double GetLoanRateOfInterest(decimal loanId, DateTime fromDate, string agency)
        {
            double result = (from lr in CSISContext.Loan_Roi
                             where lr.Loan_Id == loanId
                                   && lr.Agency == agency
                                   && lr.Roi_Wef <= fromDate
                             orderby lr.Roi_Wef descending
                             select lr.Roi).FirstOrDefault();

            return result;
        }

        public List<RateOfInterestVM> GetLoanRateOfInterest(decimal loanid, DateTime FromDate, DateTime ToDate, string Agency)
        {
            List<RateOfInterestVM> result = (from lr in CSISContext.Loan_Roi
                                             where lr.Loan_Id == loanid
                                                   && lr.Agency == Agency
                                                   && lr.Roi_Wef >= FromDate
                                                   && lr.Roi_Wef <= ToDate
                                                   && lr.Loanroi_Delete == false
                                             orderby lr.Roi_Wef
                                             select new RateOfInterestVM
                                             {
                                                 Roi = lr.Roi,
                                                 Pi = lr.Pi,
                                                 Roi_Wef = Convert.ToDateTime(lr.Roi_Wef)
                                             }).ToList();

            return result;
        }

        public double GetLoanPenalRate(decimal loanId, DateTime fromDate, string agency)
        {
            double result = (from lr in CSISContext.Loan_Roi
                             where lr.Loan_Id == loanId
                                   && lr.Agency == agency
                                   && lr.Roi_Wef <= fromDate
                             orderby lr.Roi_Wef descending
                             select lr.Pi).FirstOrDefault();

            return result;
        }

        public async Task<(double piCalc, double iodCalc)> GetPIIODForNonDemandLoans(decimal loanId, DateTime dueDate, DateTime? maxPiCalcDate, DateTime piToDate, int piApplication, int iodApplication, double prlOD, double intOD)
        {
            double piCalc = 0, iodCalc = 0;
            DateTime dueDateMinusOneMonth;

            try
            {
                if (dueDate > maxPiCalcDate)
                {
                    dueDateMinusOneMonth = dueDate.AddMonths(-1);
                    if (maxPiCalcDate == null) maxPiCalcDate = dueDateMinusOneMonth;
                    LoanDetailsHL lnDues = new LoanDetailsHL();

                    #region linq from deep seek
                    //var prlDem = (from lt in CSISContext.Loan_Trn
                    //              where lt.Loan_Id == loanId
                    //                    && lt.Due_Date < dueDate
                    //                    && lt.TrnTr_Delete == false
                    //              select lt.Prl_Dem).DefaultIfEmpty(0).Sum();

                    //var intCalc = (from lt in CSISContext.Loan_Trn
                    //               where lt.Loan_Id == loanId
                    //                     && lt.IntCalc_Date <= dueDateMinusOneMonth
                    //                     && lt.TrnTr_Delete == false
                    //               select lt.IntCalc_Amt).DefaultIfEmpty(0).Sum();

                    //var prlColl = (from lt in CSISContext.Loan_Trn
                    //               where lt.Loan_Id == loanId
                    //                     && lt.Trn_Date <= dueDate
                    //                     && lt.TrnTr_Delete == false
                    //               select lt.PrlColl_Amt).DefaultIfEmpty(0).Sum();

                    //var intColl = (from lt in CSISContext.Loan_Trn
                    //               where lt.Loan_Id == loanId
                    //                     && lt.Trn_Date <= dueDate
                    //                     && lt.TrnTr_Delete == false
                    //               select lt.IntColl_Amt).DefaultIfEmpty(0).Sum();

                    //lnDues = new LoanDetailsHL
                    //{
                    //    Prl_Dem = prlDem,
                    //    IntCalc_Amt = intCalc,
                    //    PrlColl_Amt = prlColl,
                    //    IntColl_Amt = intColl
                    //};

                    //lnDues = result1;
                    #endregion from deep seek

                    #region linq from chatgpt
                    //lnDues = (from lt in context.Loan_Trn
                    //          where lt.Loan_Id == loanId && lt.Due_Date < dueDate && lt.TrnTr_Delete == 0
                    //          group lt by 1 into g
                    //          select new
                    //          {
                    //              PrlDem = g.Sum(x => x.Prl_Dem),
                    //              IntCalc = 0,
                    //              PrlColl = 0,
                    //              IntColl = 0
                    //          })

                    //  .Concat(from lt in context.Loan_Trn
                    //          where lt.Loan_Id == loanId && lt.IntCalc_Date <= dueDateMinusOneMonth && lt.TrnTr_Delete == 0
                    //          group lt by 1 into g
                    //          select new
                    //          {
                    //              PrlDem = 0,
                    //              IntCalc = g.Sum(x => x.IntCalc_Amt),
                    //              PrlColl = 0,
                    //              IntColl = 0
                    //          })

                    //  .Concat(from lt in context.Loan_Trn
                    //          where lt.Loan_Id == loanId && lt.Trn_Date <= dueDate && lt.TrnTr_Delete == 0
                    //          group lt by 1 into g
                    //          select new
                    //          {
                    //              PrlDem = 0,
                    //              IntCalc = 0,
                    //              PrlColl = g.Sum(x => x.PrlColl_Amt),
                    //              IntColl = g.Sum(x => x.IntColl_Amt)
                    //          })

                    //  .GroupBy(x => 1)
                    //  .Select(g => new LoanDetailsHL
                    //  {
                    //      Prl_Dem = g.Sum(x => x.PrlDem),
                    //      IntCalc_Amt = g.Sum(x => x.IntCalc),
                    //      PrlColl_Amt = g.Sum(x => x.PrlColl),
                    //      IntColl_Amt = g.Sum(x => x.IntColl)
                    //  })
                    //  .FirstOrDefault();

                    #endregion from chatgpt

                    #region linq query from claud ai in usage
                    var prlDemQuery = CSISContext.Loan_Trn
                                    .Where(lt => lt.Loan_Id == loanId
                                           && lt.Due_Date < dueDate
                                           && !lt.TrnTr_Delete)
                                    .GroupBy(lt => 1) // Group all records together
                                    .Select(g => new
                                    {
                                        PrlDem = g.Sum(lt => lt.Prl_Dem),
                                        IntCalc = 0.0,
                                        PrlColl = 0.0,
                                        IntColl = 0.0
                                    });

                    var intCalcQuery = CSISContext.Loan_Trn
                                    .Where(lt => lt.Loan_Id == loanId
                                           && lt.IntCalc_Date <= dueDateMinusOneMonth
                                           && !lt.TrnTr_Delete)
                                    .GroupBy(lt => 1)
                                    .Select(g => new
                                    {
                                        PrlDem = 0.0,
                                        IntCalc = g.Sum(lt => lt.IntCalc_Amt),
                                        PrlColl = 0.0,
                                        IntColl = 0.0
                                    });

                    var collectionQuery = CSISContext.Loan_Trn
                                    .Where(lt => lt.Loan_Id == loanId
                                           && lt.Trn_Date <= dueDate
                                           && !lt.TrnTr_Delete)
                                    .GroupBy(lt => 1)
                                    .Select(g => new
                                    {
                                        PrlDem = 0.0,
                                        IntCalc = 0.0,
                                        PrlColl = g.Sum(lt => lt.PrlColl_Amt),
                                        IntColl = g.Sum(lt => lt.IntColl_Amt)
                                    });

                    // Combine all queries using Union
                    var combinedQuery = prlDemQuery
                        .Union(intCalcQuery)
                        .Union(collectionQuery);

                    // Final aggregation
                    //lnDues = combinedQuery
                    //    .GroupBy(x => 1)
                    //    .Select(g => new LoanDetailsHL
                    //    {
                    //        Prl_Dem = g.Sum(x => x.PrlDem),
                    //        IntCalc_Amt = g.Sum(x => x.IntCalc),
                    //        PrlColl_Amt = g.Sum(x => x.PrlColl),
                    //        IntColl_Amt = g.Sum(x => x.IntColl)
                    //    })
                    //    .FirstOrDefault();

                    // If using EF Core 3.0 or later, use async version:

                    var result = await combinedQuery
                        .GroupBy(x => 1)
                        .Select(g => new LoanDetailsHL
                        {
                            Prl_Dem = g.Sum(x => x.PrlDem),
                            IntCalc_Amt = g.Sum(x => x.IntCalc),
                            PrlColl_Amt = g.Sum(x => x.PrlColl),
                            IntColl_Amt = g.Sum(x => x.IntColl)
                        })
                        .FirstOrDefaultAsync();
                    if (result != null) lnDues = result;
                    #endregion linq query

                    if (lnDues != null)
                    {
                        prlOD = lnDues.Prl_Dem - lnDues.PrlColl_Amt;
                        intOD = lnDues.IntCalc_Amt - lnDues.IntColl_Amt;
                        if (prlOD < 0) prlOD = 0;
                        if (intOD < 0) intOD = 0;
                    }
                    double _socDisbAmtTmp = CSISContext.Loan_Disb
                    .Where(ld => ld.Loan_Id == loanId
                                 && ld.Disb_Date <= dueDate
                                 && ld.LoanDisb_Delete == false)
                    .Sum(ld => (double?)ld.SocDisb_Amt - (double?)ld.Reim_Amt) ?? 0;
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
                                        piCalc = CalculatePIOnVariableRate(prlOD, loanId, (DateTime)maxPiCalcDate, piToDate, "S");
                                    else
                                        piCalc = CalculatePIOnVariableRate(prlOD, loanId, dueDateMinusOneMonth, piToDate, "S");
                                }
                                else
                                {
                                    if (maxPiCalcDate >= dueDateMinusOneMonth)
                                        piCalc = CalculatePIOnVariableRate(prlOD, loanId, (DateTime)maxPiCalcDate, piToDate, "F");
                                    else
                                        piCalc = CalculatePIOnVariableRate(prlOD, loanId, dueDateMinusOneMonth, piToDate, "F");
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
                                        piCalc = CalculatePIOnVariableRate(prlOD + intOD, loanId, (DateTime)maxPiCalcDate, piToDate, "S");
                                    else
                                        piCalc = CalculatePIOnVariableRate(prlOD + intOD, loanId, dueDateMinusOneMonth, piToDate, "S");
                                }
                                else
                                {
                                    if (maxPiCalcDate >= dueDateMinusOneMonth)
                                        piCalc = CalculatePIOnVariableRate(prlOD, loanId, (DateTime)maxPiCalcDate, piToDate, "F");
                                    else
                                        piCalc = CalculatePIOnVariableRate(prlOD, loanId, dueDateMinusOneMonth, piToDate, "F");
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
                                        iodCalc = CalculateInterestOnVariableRate(prlOD, loanId, (DateTime)maxPiCalcDate, piToDate, "S");
                                    //iodCalc = Calc_Int_OnVariable_ROI(prlOD, loanId, (DateTime)maxPiCalcDate, piToDate, "S");
                                    else
                                        iodCalc = CalculateInterestOnVariableRate(prlOD, loanId, dueDateMinusOneMonth, piToDate, "S");
                                    //iodCalc = Calc_Int_OnVariable_ROI(prlOD, loanId, dueDateMinusOneMonth, piToDate, "S");
                                }
                                else
                                {
                                    //if (maxPiCalcDate <= dueDate)
                                    //if (dueDate <= maxPiCalcDate)
                                    if (maxPiCalcDate >= dueDateMinusOneMonth)
                                        iodCalc = CalculateInterestOnVariableRate(prlOD, loanId, (DateTime)maxPiCalcDate, piToDate, "F");
                                    //iodCalc = Calc_Int_OnVariable_ROI(prlOD, loanId, (DateTime)maxPiCalcDate, piToDate, "F");
                                    else
                                        iodCalc = CalculateInterestOnVariableRate(prlOD, loanId, dueDateMinusOneMonth, piToDate, "F");
                                    //iodCalc = Calc_Int_OnVariable_ROI(prlOD, loanId, dueDateMinusOneMonth, piToDate, "F");
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
                                    //if (maxPiCalcDate <= dueDate)
                                    //if (dueDate <= maxPiCalcDate)
                                    if (maxPiCalcDate >= dueDateMinusOneMonth)
                                        iodCalc = CalculateInterestOnVariableRate(prlOD + intOD, loanId, (DateTime)maxPiCalcDate, piToDate, "S");
                                    //iodCalc = Calc_Int_OnVariable_ROI(prlOD + intOD, loanId, (DateTime)maxPiCalcDate, piToDate, "S");
                                    else
                                        iodCalc = CalculateInterestOnVariableRate(prlOD + intOD, loanId, dueDateMinusOneMonth, piToDate, "S");
                                    //iodCalc = Calc_Int_OnVariable_ROI(prlOD + intOD, loanId, dueDateMinusOneMonth, piToDate, "S");
                                }
                                else
                                {
                                    //if (maxPiCalcDate <= dueDate)
                                    //if (dueDate <= maxPiCalcDate)
                                    if (maxPiCalcDate >= dueDateMinusOneMonth)
                                        iodCalc = CalculateInterestOnVariableRate(prlOD + intOD, loanId, (DateTime)maxPiCalcDate, piToDate, "F");
                                    //iodCalc = Calc_Int_OnVariable_ROI(prlOD + intOD, loanId, (DateTime)maxPiCalcDate, piToDate, "F");
                                    else
                                        iodCalc = CalculateInterestOnVariableRate(prlOD + intOD, loanId, dueDateMinusOneMonth, piToDate, "F");
                                    //iodCalc = Calc_Int_OnVariable_ROI(prlOD + intOD, loanId, dueDateMinusOneMonth, piToDate, "F");
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

            }
            return (piCalc, iodCalc);
        }

        public async Task<List<LoanDetailsVM>> GetLoanDetailsList2ByLoanIdsAsync(decimal[] loanIds)
        {
            List<LoanDetailsVM> list = new List<LoanDetailsVM>();
            try
            {
                // Assuming result is a string of comma-separated loan IDs, convert it to a list of integers
                //var loanIds = result.Split(',').Select(int.Parse).ToList();

                var LoanList = await (from master in CSISContext.Loan_Master
                                      join scheme in CSISContext.Loan_Schemes
                                          on master.Scheme_Id equals scheme.Scheme_Id
                                      join trn in CSISContext.Loan_Trn
                                          on master.Loan_Id equals trn.Loan_Id
                                      where !trn.TrnTr_Delete && !master.Loan_Delete
                                      group new { master, scheme, trn } by new
                                      {
                                          master.Loan_Id,
                                          master.Scheme_Id,
                                          master.Loan_No,
                                          master.Roi,
                                          master.Inst_Amt,
                                          master.San_Amt,
                                          master.San_Date,
                                          scheme.Scheme_Name,
                                          scheme.PrlLed_Id,
                                          scheme.IntLed_Id,
                                          scheme.IODLed_Id,
                                          scheme.PILed_Id,
                                          scheme.Inst_Type,
                                          scheme.Int_Application,
                                          scheme.PI_Application,
                                          scheme.IOD_Application,
                                          scheme.MatchShareCapital,
                                          scheme.AdoptLoanLimit,
                                          scheme.StaffLoan_Int_Type,
                                          master.SecurityFaceValue,
                                          master.FirstInt_DueDate,
                                          master.FirstPrl_DueDate
                                      } into g
                                      where loanIds.Contains(g.Key.Loan_Id) &&
                                            g.Key.San_Amt - g.Sum(x => x.trn.PrlColl_Amt) > 0
                                      select new LoanDetailsVM
                                      {
                                          loanid = g.Key.Loan_Id,
                                          schemeid = g.Key.Scheme_Id,
                                          loanno = g.Key.Loan_No,
                                          roi = g.Key.Roi,
                                          instalmentamt = g.Key.Inst_Amt,
                                          disbamt = g.Sum(x => x.trn.Disb_Amt),
                                          disbursementdate = g.Key.San_Date,
                                          schemename = g.Key.Scheme_Name,
                                          demanddate = g.Max(x => (DateTime?)x.trn.Due_Date),
                                          prlledid = g.Key.PrlLed_Id,
                                          intledid = g.Key.IntLed_Id,
                                          iodledid = g.Key.IODLed_Id,
                                          piledid = g.Key.PILed_Id,
                                          instalType = g.Key.Inst_Type,
                                          intapplication = g.Key.Int_Application,
                                          piapplication = g.Key.PI_Application,
                                          iodapplication = g.Key.IOD_Application,
                                          matchShareCapital = g.Key.MatchShareCapital,
                                          adoptLoanLimit = g.Key.AdoptLoanLimit,
                                          StaffLoan_Int_Type = g.Key.StaffLoan_Int_Type,
                                          sanctionamt = g.Key.San_Amt,
                                          sanctiondate = g.Key.San_Date,
                                          prlschedule = g.Sum(x => x.trn.Prl_Sched),
                                          prldemand = g.Sum(x => x.trn.Prl_Dem),
                                          prlcoll = g.Sum(x => x.trn.PrlColl_Amt),
                                          intcalulatedamt = g.Sum(x => x.trn.IntCalc_Amt),
                                          maxintcalcdate = g.Max(x => (DateTime?)x.trn.IntCalc_Date),
                                          intcollamt = g.Sum(x => x.trn.IntColl_Amt),
                                          picalulatedamt = g.Sum(x => x.trn.PICalc_Amt),
                                          maxpicalcdate = g.Max(x => (DateTime?)x.trn.PICalc_Date),
                                          picollamt = g.Sum(x => x.trn.PIColl_Amt),
                                          iodcalculatedamt = g.Sum(x => x.trn.IODCalc_Amt),
                                          iodcollamt = g.Sum(x => x.trn.IODColl_Amt),
                                          trndate = g.Min(x => x.trn.Trn_Date),
                                          securityfacevalue = g.Key.SecurityFaceValue,
                                          maxtrnslno = g.Max(x => x.trn.Trn_SlNo),
                                          firstintduedate = g.Key.FirstInt_DueDate,
                                          firstprlduedate = g.Key.FirstPrl_DueDate
                                      }).ToListAsync();
                if (LoanList.Count > 0) list = LoanList.ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching loan balance by loan Id");
            }
            return list;
        }

        public async Task<List<LoanDetailsHL>> GetLoanDetailsListByLoanIdLTAsync(decimal[] loanIds, DateTime trnDate)
        {
            double intDemand = 0, prlDemand = 0, piDemand = 0, emiDemand = 0;
            double intCalcAmt = 0, intCollAmt = 0;
            List<Loan_Disb> disbList = new List<Loan_Disb>();
            //DateTime maxRoiWef;
            //double _roi = 0;
            List<Loan_Roi> roiList = new List<Loan_Roi>();
            DateTime endDate = trnDate;
            DateTime fromDate;
            DateTime _maxDueDate;
            DateTime _maxIntCalcDate;
            DateTime _maxPICalcDate;
            DateTime _previourDueDate;
            double _oneMonthIntCalc = 0;
            double prlCurrendDemand = 0;
            //double _prlDemand = 0;
            double _prlCurrentDemand = 0;
            double _prlTotalCurrentDemand = 0;
            double _nonODPrl = 0;
            double _loanOS = 0;
            double _disbAmt = 0;
            LoanInterestCalculatedItems intCalcItems = new LoanInterestCalculatedItems();
            List<LoanDetailsHL> loanDetailsList = new List<LoanDetailsHL>();
            //LoanDetailsHL loanDetails = new LoanDetailsHL();
            List<int> loanIdList = new List<int>();

            loanDetailsList = await GetLoanDetailsListByLoanIdsAsync(loanIds);

            foreach (var loan in loanDetailsList)
            {
                //loanDetails = await  GetLoanDetailsListByLoanIdsAsync(loanIds); /// GetLoanDetailsHL(loan, out errorMessage);
                if (loan.Due_Date != null)
                    _previourDueDate = (DateTime)loan.Due_Date.Value.Date;
                else
                    _previourDueDate = loan.FirstInt_DueDate.Date;

                if (loan.IntCalc_Date != null)
                    fromDate = (DateTime)loan.IntCalc_Date;
                else
                    fromDate = loan.Disb_Date;
                if (loan.Due_Date != null)
                    if (loan.Due_Date.Value.Date >= trnDate.Date)
                        _maxDueDate = (DateTime)loan.Due_Date;
                    else
                    {
                        _maxDueDate = Utilities.AddMonths(Convert.ToDateTime(loan.Due_Date), loan.Dem_Frequency); /// Utilities.AddMonths((DateTime)loan.Due_Date, loan.Dem_Frequency);
                        if (_maxDueDate.Date > trnDate.Date)
                            _maxDueDate = loan.Due_Date.Value.Date;
                    }
                else
                    _maxDueDate = loan.FirstInt_DueDate;

                prlDemand = 0; _nonODPrl = 0; _prlCurrentDemand = 0;
                _loanOS = 0;
                intDemand = 0; prlDemand = 0; piDemand = 0; emiDemand = 0; prlCurrendDemand = 0;
                _prlTotalCurrentDemand = 0;
                _disbAmt = loan.Disb_Amt;
                _loanOS = loan.Disb_Amt - loan.PrlColl_Amt;
                _nonODPrl = loan.Disb_Amt - loan.Prl_Dem;
                _maxIntCalcDate = loan.IntCalc_Date != null ? (DateTime)loan.IntCalc_Date : loan.Disb_Date;
                _maxPICalcDate = loan.PICalc_Date != null ? (DateTime)loan.PICalc_Date : loan.FirstInt_DueDate;
                int noOfMonths = Utilities.GetNoOfMonths(trnDate.Date, fromDate.Date); ///  Utility.GetNoOfMonths(trnDate.Date, fromDate.Date);
                prlDemand = loan.Prl_Dem;
                intCalcAmt = loan.IntCalc_Amt;
                intCollAmt = loan.IntColl_Amt;

                #region calculation of demand based on loan repayment schedule
                List<Loan_Repayment_Schedule> repaymentScheduleList = new List<Loan_Repayment_Schedule>();

                repaymentScheduleList = CSISContext.Loan_Repayment_Schedule
                    .Where(x => x.Loan_Id == loan.Loan_Id && (x.Due_Date > fromDate.Date && x.Due_Date <= trnDate.Date))
                    .OrderBy(x => x.Due_Date).ToList();
                /// loop upto last due date

                foreach (var schedule in repaymentScheduleList)
                {
                    (intCalcItems, _disbAmt) = await CalculateLoanDuesLT(loan.Loan_Id, loan.FirstInt_DueDate, Convert.ToDateTime(schedule.Due_Date), schedule.Due_Date, _maxIntCalcDate, _maxPICalcDate,
                        _maxPICalcDate, Convert.ToDateTime(schedule.Due_Date), Convert.ToDateTime(schedule.Due_Date), loan.Int_Application, loan.PI_Application, loan.IOD_Application,
                         loan.Disb_Amt, loan.PrlColl_Amt, prlDemand, prlDemand, intCalcAmt, intCollAmt,
                         "S");

                    //intCalcItems = CalculateLnDuesPLDB(loanDetails.Loan_Id, loanDetails.FirstInt_DueDate, (DateTime)schedule.Due_Date, schedule.Due_Date, _maxIntCalcDate, _maxPICalcDate,
                    //    _maxPICalcDate, (DateTime)schedule.Due_Date, (DateTime)schedule.Due_Date, loanDetails.Int_Application, loanDetails.PI_Application, loanDetails.IOD_Application,
                    //     loanDetails.Disb_Amt, loanDetails.PrlColl_Amt, prlDemand, prlDemand, intCalcAmt, intCollAmt,
                    //     "S", context, out _disbAmt, out errorMessage);

                    intCalcAmt += intCalcItems.InterestCalculatAmt;
                    intDemand += intCalcItems.InterestCalculatAmt;
                    piDemand += intCalcItems.PICalculateAmt;
                    emiDemand += intCalcItems.IODCalculateAmt;
                    loan.Disb_Amt += _disbAmt;
                    _maxDueDate = Convert.ToDateTime(schedule.Due_Date);
                    _maxIntCalcDate = Convert.ToDateTime(schedule.Due_Date);
                    _maxPICalcDate = Convert.ToDateTime(schedule.Due_Date);
                    //loanDetails.IntCalc_DateCurrent = (DateTime)schedule.Due_Date;
                    loan.Due_Date = Convert.ToDateTime(schedule.Due_Date);
                    /// raise principal demand
                    if (loan.Inst_Type == 1) /// Fixed principal
                        _prlCurrentDemand = loan.Inst_Amt;
                    if (loan.Inst_Type == 2) /// Equated Instalment principal
                    {
                        _oneMonthIntCalc = CalculateCurrentInterestDemand(loan.Loan_Id, _nonODPrl, 0, _maxDueDate.AddMonths(-1), _maxDueDate);
                        //_oneMonthIntCalc = Raise_Curr_Int_Demand(loanDetails.Loan_Id, _nonODPrl, 0, _maxDueDate.AddMonths(-1), _maxDueDate, context, out errorMessage);
                        //if (errorMessage.Length > 0)
                        //{
                        //    goto ErrorHandler;
                        //}
                        _prlCurrentDemand = loan.Inst_Amt - _oneMonthIntCalc;
                    }
                    /// validate current principal demand with os,first due date
                    if (_loanOS > 0)
                    {
                        if (_prlCurrentDemand >= _loanOS)
                            _prlCurrentDemand = _loanOS;
                    }
                    if (loan.Prl_Dem >= loan.Disb_Amt)
                        _prlCurrentDemand = 0;
                    if (endDate < loan.FirstPrl_DueDate)
                        _prlCurrentDemand = 0;

                    /// if demand already raised as advance once, then make prl demad = 0
                    if (loan.Adv_Prl_Application == 1) /// do not raise advance principal demand
                    {
                        double advPrlColl = loan.PrlColl_Amt - (loan.Prl_Dem + prlDemand);
                        if (advPrlColl < 0) advPrlColl = 0;
                        if (advPrlColl >= _prlCurrentDemand)
                        {
                            _prlCurrentDemand = 0;
                            advPrlColl -= _prlCurrentDemand;
                        }
                        else
                        {
                            _prlCurrentDemand -= advPrlColl;
                            advPrlColl = 0;
                        }
                    }
                    if (_prlCurrentDemand >= loan.Disb_Amt - prlDemand)
                    {
                        _prlCurrentDemand = loan.Disb_Amt - prlDemand;
                        if (_prlCurrentDemand < 0) _prlCurrentDemand = 0;
                    }
                    prlDemand += _prlCurrentDemand;
                    _prlTotalCurrentDemand += _prlCurrentDemand;
                    prlCurrendDemand += _prlCurrentDemand;
                }
                #endregion

                /// calculate from last due date upto receipt date
                #region loop from last due date from repayment schedule to receipt date
                int noOfPeriods = 0;
                noOfPeriods = Utilities.GetAgeBetweenTwoDates(_maxDueDate, trnDate.Date); /// Utilities.GetNoOfCompletedMonthsBetweenTwoDates(_maxDueDate, trnDate.Date);
                noOfPeriods = noOfPeriods / loan.Dem_Frequency;
                for (int i = 1; i <= noOfPeriods; i++)
                {
                    (intCalcItems, _disbAmt) = await CalculateLoanDuesLT(loan.Loan_Id, loan.FirstInt_DueDate, _maxDueDate.AddMonths(loan.Dem_Frequency), _maxDueDate.AddMonths(loan.Dem_Frequency), _maxIntCalcDate, _maxPICalcDate,
                                _maxPICalcDate, _maxDueDate.AddMonths(loan.Dem_Frequency), _maxPICalcDate.AddMonths(loan.Dem_Frequency), loan.Int_Application, loan.PI_Application, loan.IOD_Application,
                                 loan.Disb_Amt, loan.PrlColl_Amt, prlDemand, prlDemand, intCalcAmt, intCollAmt,
                                 "S");
                    //intCalcItems = CalculateLnDuesPLDB(loanDetails.Loan_Id, loanDetails.FirstInt_DueDate, _maxDueDate.AddMonths(loanDetails.Dem_Frequency), _maxDueDate.AddMonths(loanDetails.Dem_Frequency), _maxIntCalcDate, _maxPICalcDate,
                    //            _maxPICalcDate, _maxDueDate.AddMonths(loanDetails.Dem_Frequency), _maxPICalcDate.AddMonths(loanDetails.Dem_Frequency), loanDetails.Int_Application, loanDetails.PI_Application, loanDetails.IOD_Application,
                    //             loanDetails.Disb_Amt, loanDetails.PrlColl_Amt, prlDemand, prlDemand, intCalcAmt, intCollAmt,
                    //             "S", context, out _disbAmt, out errorMessage);
                    intCalcAmt += intCalcItems.InterestCalculatAmt;
                    intDemand += intCalcItems.InterestCalculatAmt;
                    piDemand += intCalcItems.PICalculateAmt;
                    emiDemand += intCalcItems.IODCalculateAmt;
                    loan.Disb_Amt += _disbAmt;

                    if (loan.Inst_Type == 1) /// Fixed principal
                        _prlCurrentDemand = loan.Inst_Amt;
                    if (loan.Inst_Type == 2) /// Equated Instalment principal
                    {
                        _oneMonthIntCalc = CalculateCurrentInterestDemand(loan.Loan_Id, _nonODPrl, 0, _maxDueDate.AddMonths(loan.Dem_Frequency * -1), _maxDueDate);
                        //_oneMonthIntCalc = Raise_Curr_Int_Demand(loanDetails.Loan_Id, _nonODPrl, 0, _maxDueDate.AddMonths(loanDetails.Dem_Frequency * -1), _maxDueDate, context, out errorMessage);
                        _prlCurrentDemand = loan.Inst_Amt - _oneMonthIntCalc;
                    }
                    /// validate current principal demand with os,first due date
                    if (_loanOS > 0)
                    {
                        if (_prlCurrentDemand >= _loanOS)
                            _prlCurrentDemand = _loanOS;
                    }
                    if (prlDemand >= loan.Disb_Amt)
                        _prlCurrentDemand = 0;
                    if (trnDate.Date < loan.FirstPrl_DueDate)
                        _prlCurrentDemand = 0;
                    /// if demand already raised as advance once, then make prl demad = 0
                    if (loan.Adv_Prl_Application == 1) /// do not raise advance principal demand
                    {
                        double advPrlColl = loan.PrlColl_Amt - (loan.Prl_Dem + prlDemand);
                        if (advPrlColl < 0) advPrlColl = 0;
                        if (advPrlColl >= _prlCurrentDemand)
                        {
                            _prlCurrentDemand = 0;
                            advPrlColl -= _prlCurrentDemand;
                        }
                        else
                        {
                            _prlCurrentDemand -= advPrlColl;
                            advPrlColl = 0;
                        }
                    }
                    if (_prlCurrentDemand >= loan.Disb_Amt - prlDemand)
                    {
                        _prlCurrentDemand = loan.Disb_Amt - prlDemand;
                        if (_prlCurrentDemand < 0) _prlCurrentDemand = 0;
                    }
                    prlDemand += _prlCurrentDemand;
                    _prlTotalCurrentDemand += _prlCurrentDemand;
                    _nonODPrl -= _prlCurrentDemand;
                    _maxDueDate = _maxPICalcDate.AddMonths(loan.Dem_Frequency).Date;
                    _maxIntCalcDate = _maxPICalcDate.AddMonths(loan.Dem_Frequency).Date;
                    _maxPICalcDate = _maxPICalcDate.AddMonths(loan.Dem_Frequency).Date;
                }

                (intCalcItems, _disbAmt) = await CalculateLoanDuesLT(loan.Loan_Id, loan.FirstInt_DueDate, trnDate, _maxDueDate, _maxIntCalcDate, _maxPICalcDate,
                        _maxPICalcDate, trnDate.Date, trnDate.Date, loan.Int_Application, loan.PI_Application, loan.IOD_Application,
                         loan.Disb_Amt, loan.PrlColl_Amt, loan.Prl_Sched, prlDemand, intCalcAmt, intCollAmt,
                         "S");

                //intCalcItems = CalculateLnDuesPLDB(loanDetails.Loan_Id, loanDetails.FirstInt_DueDate, trnDate, _maxDueDate, _maxIntCalcDate, _maxPICalcDate,
                //        _maxPICalcDate, trnDate.Date, trnDate.Date, loanDetails.Int_Application, loanDetails.PI_Application, loanDetails.IOD_Application,
                //         loanDetails.Disb_Amt, loanDetails.PrlColl_Amt, loanDetails.Prl_Sched, prlDemand, intCalcAmt, intCollAmt,
                //         "S", context, out _disbAmt, out errorMessage);
                intCalcAmt += intCalcItems.InterestCalculatAmt;
                intDemand += intCalcItems.InterestCalculatAmt;
                piDemand += intCalcItems.PICalculateAmt;
                emiDemand += intCalcItems.IODCalculateAmt;


                #endregion

                loan.IntCalc_AmtCurrent = intDemand;
                loan.IntCalc_DateCurrent = trnDate.Date;
                loan.PICalc_AmtCurrent = piDemand;
                loan.PICalc_DateCurrent = trnDate.Date;
                loan.IODCalc_AmtCurrent = emiDemand;
                loan.IODCalc_DateCurrent = trnDate.Date;
                loan.Prl_DemCurrent = _prlTotalCurrentDemand;
                //loanDetailsList.Add(loanDetails);
            }
            //    goto NoError;
            //NoError:
            return loanDetailsList;
            //ErrorHandler:
            //    loanDetailsList.Clear();
            //    return loanDetailsList;
        }

        public async Task<(LoanInterestCalculatedItems calcLnDues, double _disbDuringPeriod)> CalculateLoanDuesLT(decimal loanid, DateTime FirstIntDueDate, DateTime MaxTrn_Date, DateTime? MaxDueDate, DateTime IntFromDate, DateTime? PIFromDate, DateTime? IODFromDate, DateTime ToDate, DateTime PiToDate, int Int_Application, int PI_Application, int IOD_Application, double DisbAmt, double PrlColl, double PrlSchedule, double PrlDemand, double IntCalcAmt, double IntCollAmt, string DisbAgency)
        {
            LoanInterestCalculatedItems calcLnDues = new LoanInterestCalculatedItems();
            double LoanOS = DisbAmt - PrlColl;
            double NonODPrl = DisbAmt - PrlDemand;
            double PrlOD = PrlDemand - PrlColl;
            double IntOD = IntCalcAmt - IntCollAmt;
            double _nonODSocAmt = 0, _nonODFedAmt = 0, _prlCollOrPrlDem = 0;
            double _socOS = 0, _fedOS = 0;
            double _socDisbAmt = 0;
            double _disbDuringPeriod = 0;
            double _fedDisbAmt = 0;
            double _intCalc = 0;
            DateTime _intFromDateTmp = IntFromDate;
            DateTime _piFromDate = DateTime.Now;
            DateTime _iodFromDate = DateTime.Now;
            DateTime _NextDueDate = DateTime.Now;
            DateTime _intToDate = ToDate;
            double _piCalc = 0;
            double _iodCalc = 0;
            try
            {
                if (PrlOD < 0) PrlOD = 0;
                if (PrlColl >= PrlDemand)
                    _prlCollOrPrlDem = PrlColl;
                else
                    _prlCollOrPrlDem = PrlDemand;
                //_socDisbAmt = DisbAmt;
                List<Loan_Disb> disbList = new List<Loan_Disb>();

                disbList = GetLoanDisbursementList(loanid, IntFromDate, ToDate); /// dbLoan.GetLoanDisbursementList(loanid, IntFromDate, ToDate, context, out errorMessage);
                (_socDisbAmt, _fedDisbAmt) = await GetSocDisbAmt_And_FedDisAmt(loanid, IntFromDate);

                //if (!GetSocDisbAmt_And_FedDisAmt(loanid, IntFromDate, context, out _socDisbAmt, out _fedDisbAmt, out errorMessage))
                //{
                //    errorMessage += "\n Error in obtain society disbursement and Federation disbursement data";
                //    return calcLnDues;
                //}

                (_nonODSocAmt, _nonODFedAmt, _socOS, _fedOS) = GetNonODAndOutstandingForSocietyAndFederationLoans(_socDisbAmt, _fedDisbAmt, PrlColl, PrlDemand);

                //if (!Get_NonOD_And_OS_Soc_Fed(_socDisbAmt, _fedDisbAmt, PrlColl, PrlDemand, out _nonODSocAmt, out _nonODFedAmt, out _socOS, out _fedOS, out errorMessage))
                //{
                //    errorMessage += "\n Error in obtain society society non-od prl,federation non-od prl, society os and federation os data";
                //    return calcLnDues;
                //}

                /// calculate Interest
                #region calculate interest

                foreach (Loan_Disb disb in disbList)
                {
                    /// fix to date 

                    if (Int_Application == 1)    /// interest on non-od principal
                    {
                        _intCalc += CalculateCurrentInterestDemand(loanid, _nonODSocAmt, _nonODFedAmt, IntFromDate, Convert.ToDateTime(disb.Disb_Date));
                        //_intCalc += Raise_Curr_Int_Demand(loanid, _nonODSocAmt, _nonODFedAmt, IntFromDate, (DateTime)disb.Disb_Date, context, out errorMessage);
                        //if (errorMessage.Length > 0)
                        //    return calcLnDues;
                    }
                    if (Int_Application == 2)    /// interest on prl outstanding
                    {
                        _intCalc += CalculateCurrentInterestDemand(loanid, _socOS, _fedOS, IntFromDate, Convert.ToDateTime(disb.Disb_Date));
                        //_intCalc += Raise_Curr_Int_Demand(loanid, _socOS, _fedOS, IntFromDate, (DateTime)disb.Disb_Date, context, out errorMessage);
                        //if (errorMessage.Length > 0)
                        //    return calcLnDues;
                    }
                    _nonODSocAmt += disb.SocDisb_Amt;
                    _nonODFedAmt += disb.Reim_Amt;
                    _socOS += disb.SocDisb_Amt;
                    _fedOS += disb.Reim_Amt;
                    _disbDuringPeriod += disb.SocDisb_Amt;
                    IntFromDate = Convert.ToDateTime(disb.Disb_Date);
                }
                if (Int_Application == 1)    /// interest on on-od principal
                {
                    _intCalc += CalculateCurrentInterestDemand(loanid, _nonODSocAmt, _nonODFedAmt, IntFromDate, ToDate);
                    //_intCalc += Raise_Curr_Int_Demand(loanid, _nonODSocAmt, _nonODFedAmt, IntFromDate, ToDate, context, out errorMessage);
                    //if (errorMessage.Length > 0)
                    //    return calcLnDues;
                }
                if (Int_Application == 2)    /// interest on prl outstanding
                {
                    _intCalc += CalculateCurrentInterestDemand(loanid, _socOS, _fedOS, IntFromDate, ToDate);
                    //_intCalc += Raise_Curr_Int_Demand(loanid, _socOS, _fedOS, IntFromDate, ToDate, context, out errorMessage);
                    //if (errorMessage.Length > 0)
                    //    return calcLnDues;
                }
                calcLnDues.InterestCalculatAmt = _intCalc;
                calcLnDues.InterestCalculateDate = ToDate;
                #endregion

                /// calculate PI
                #region calcualte PI And IOD Calculation
                _piFromDate = MaxTrn_Date;
                if (MaxDueDate > PiToDate)
                {
                    (_piCalc, _iodCalc) = await GetPIIODForNonDemandLoans(loanid, MaxDueDate == null ? FirstIntDueDate : (DateTime)MaxDueDate, PIFromDate, PiToDate, PI_Application, IOD_Application, PrlOD, IntOD);
                    //if (!Get_PIIOD_ForNonDemandLoans(loanid, MaxDueDate == null ? FirstIntDueDate : (DateTime)MaxDueDate, PIFromDate, PiToDate, PI_Application, IOD_Application, PrlOD, IntOD, context, out _piCalc, out _iodCalc, out errorMessage))
                    //{
                    //    errorMessage += "\n Error in obtain Loan overdue data";
                    //    return calcLnDues;
                    //}
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
                                calcLnDues.PICalculateAmt = CalculatePIOnVariableRate(PrlOD, loanid, Convert.ToDateTime(PIFromDate), PiToDate, "S");
                                //calcLnDues.PICalculateAmt = Calc_PI_OnVariable_ROI(loanid, (DateTime)PIFromDate, PiToDate, PrlOD, "S");
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
                                calcLnDues.PICalculateAmt = CalculatePIOnVariableRate(PrlOD + IntOD, loanid, Convert.ToDateTime(PIFromDate), PiToDate, "S");
                                //calcLnDues.PICalculateAmt = Calc_PI_OnVariable_ROI(loanid, (DateTime)PIFromDate, PiToDate, PrlOD + IntOD, "S");
                                //if (calcLnDues.PICalculateAmt > 0)
                                //    calcLnDues.PICalcuateDate = PiToDate.Date;
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
                                calcLnDues.IODCalculateAmt = CalculateInterestOnVariableRate(PrlOD, loanid, Convert.ToDateTime(PIFromDate), PiToDate, "S");
                            //calcLnDues.IODCalculateAmt = Calc_Int_OnVariable_ROI(PrlOD, loanid, (DateTime)PIFromDate, PiToDate, "S");
                            else
                                calcLnDues.IODCalculateAmt = 0;
                        }
                        else if (IOD_Application == 3) /// iod on prlod + intod
                        {
                            if (PrlOD + IntOD > 0)
                                calcLnDues.IODCalculateAmt = CalculateInterestOnVariableRate(PrlOD + IntOD, loanid, Convert.ToDateTime(PIFromDate), PiToDate, "S");
                            //calcLnDues.IODCalculateAmt = Calc_Int_OnVariable_ROI(PrlOD + IntOD, loanid, (DateTime)PIFromDate, PiToDate, "S");
                            else
                                calcLnDues.IODCalculateAmt = 0;
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
                //errorMessage = ex.Message;
            }
            return (calcLnDues, _disbDuringPeriod);
        }

        public async Task<(double appraisalFee, double bankCharges, double serviceCharges)> GetJewelLoanAppraisalFees(double loanAmount)
        {
            double appraisalFee = 0, bankCharges = 0, serviceCharges = 0;
            try
            {
                var result = await (from pf in CSISContext.JL_ProcessingFees
                                    where loanAmount > pf.From_Value
                                          && loanAmount <= pf.To_Value
                                          && pf.Version_Id == (from pf2 in CSISContext.JL_ProcessingFees
                                                               select pf2.Version_Id).Max()
                                    select new
                                    {
                                        appraisalFee = pf.Appraisal_Fee,
                                        bankCharges = pf.Bank_Charges,
                                        serviceCharges = pf.Service_Charges,
                                        pf
                                    }).FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return (appraisalFee, bankCharges, serviceCharges);
        }

        #region staff loan
        public async Task<List<PayLoanBalanceVM>> GetPayLoanBalance(decimal empId, int loanType, DateTime toDate, string brCode)
        {
            double intCalc = 0;
            double intBalIncludingCurrentIntCalc = 0;
            DateTime intCalcUpto;
            int prdElapsed = 0;
            double intInstalment = 0;
            double intDemand = 0;
            double prlDemand = 0;
            int presentIntPrd = 0;
            List<PayLoanBalanceVM> loanList = new();
            try
            {
                #region get record set
                var result = await (from lm in CSISContext.Loan_Master
                                    join mm in CSISContext.mem_master on lm.Mem_Id equals mm.mem_id
                                    join ls in CSISContext.Loan_Schemes on lm.Scheme_Id equals ls.Scheme_Id
                                    join lt in CSISContext.Loan_Trn on lm.Loan_Id equals lt.Loan_Id
                                    where lm.Mem_Id == empId
                                          && !lt.TrnTr_Delete
                                          && !lm.Loan_Delete
                                    group new { lm, ls, lt } by new
                                    {
                                        lm.Loan_Id,
                                        lm.Loan_No,
                                        lm.San_Amt,
                                        lm.FirstPrl_DueDate,
                                        lm.FirstInt_DueDate,
                                        lm.San_Date,
                                        lm.Prl_Prd,
                                        lm.Int_Prd,
                                        lm.Inst_Amt,
                                        lm.Roi,
                                        ls.Scheme_Name,
                                        ls.Scheme_Id,
                                        ls.Loan_Type,
                                        ls.PrlLed_Id,
                                        ls.IntLed_Id,
                                        ls.StaffLoan_Int_Type
                                    } into g
                                    where (g.Sum(x => x.lt.Disb_Amt) - g.Sum(x => x.lt.PrlColl_Amt) > 0) ||
                                          (g.Sum(x => x.lt.IntCalc_Amt) - g.Sum(x => x.lt.IntColl_Amt) > 0)
                                    where g.Key.Loan_Type == 5
                                    select new PayLoanBalanceVM
                                    {
                                        Loan_Id = g.Key.Loan_Id,
                                        Loan_No = g.Key.Loan_No,
                                        San_Amt = g.Key.San_Amt,
                                        FirstPrl_DueDate = (DateTime)g.Key.FirstPrl_DueDate!,
                                        FirstInt_DueDate = (DateTime)g.Key.FirstInt_DueDate!,
                                        San_Date = g.Key.San_Date,
                                        Prl_Prd = g.Key.Prl_Prd,
                                        Int_Prd = g.Key.Int_Prd,
                                        Inst_Amt = g.Key.Inst_Amt,
                                        Roi = g.Key.Roi,
                                        Scheme_Name = g.Key.Scheme_Name,
                                        Scheme_Id = g.Key.Scheme_Id,
                                        Loan_Type = g.Key.Loan_Type,
                                        PrlLed_Id = g.Key.PrlLed_Id,
                                        IntLed_Id = g.Key.IntLed_Id,
                                        StaffLoan_Int_Type = g.Key.StaffLoan_Int_Type,
                                        MaxTrn_Date = g.Max(x => x.lt.Trn_Date),
                                        SumDisb_Amt = g.Sum(x => x.lt.Disb_Amt),
                                        SumPrl_Sched = g.Sum(x => x.lt.Prl_Sched),
                                        SumPrl_Dem = g.Sum(x => x.lt.Prl_Dem),
                                        SumPrlColl_Amt = g.Sum(x => x.lt.PrlColl_Amt),
                                        SumIntCalc_Amt = g.Sum(x => x.lt.IntCalc_Amt),
                                        MaxIntCalc_Date = g.Max(x => x.lt.IntCalc_Date),
                                        SumIntColl_Amt = g.Sum(x => x.lt.IntColl_Amt)
                                    }).ToListAsync();
                #endregion 

                if (result != null && result.Any())
                {
                    loanList = result.ToList();

                    foreach (var loan in loanList)
                    {
                        loan.PrlOS = loan.SumDisb_Amt - loan.SumPrlColl_Amt;
                        loan.PrlOD = loan.SumPrl_Dem - loan.SumPrlColl_Amt;
                        if (loan.PrlOD < 0)
                            loan.PrlOD = 0;
                        loan.IntBal = loan.SumIntCalc_Amt - loan.SumIntColl_Amt;
                        if (loan.IntBal < 0)
                            loan.IntBal = 0;
                        if (loan.MaxIntCalc_Date != null)
                            intCalcUpto = ((DateTime)loan.MaxIntCalc_Date).Date;
                        else
                            intCalcUpto = loan.San_Date.Date;
                        intCalc = Utilities.Calculate_Interest(loan.PrlOS, loan.Roi, Utilities.GetNoOfDays(toDate, intCalcUpto));
                        loan.IntCalc = intCalc;
                        if (intCalc > 0)
                        {
                            loan.IntCalcDate = toDate.Date;
                        }
                        intBalIncludingCurrentIntCalc = loan.IntBal + intCalc;
                        //prdElapsed = GeneralService.GetNoOfMonths(toDate, loan.FirstPrl_DueDate);
                        prdElapsed = Utilities.GetMonthsBetweenDates(loan.FirstPrl_DueDate, toDate);
                        switch (loan.StaffLoan_Int_Type)
                        {
                            case 1: /// int after prl
                                if (prdElapsed > loan.Prl_Prd)
                                {
                                    //intInstalment = Math.Round(intBalIncludingCurrentIntCalc / loan.Int_Prd, 0);
                                    intInstalment = Math.Round(loan.SumIntCalc_Amt / loan.Int_Prd, 0);
                                    presentIntPrd = loan.Int_Prd - (prdElapsed - loan.Prl_Prd);
                                }
                                else
                                    presentIntPrd = 0;
                                if (loan.PrlOS == 0)
                                {
                                    //intInstalment = Math.Round(intBalIncludingCurrentIntCalc / loan.Int_Prd, 0);
                                    intInstalment = Math.Round(loan.SumIntCalc_Amt / loan.Int_Prd, 0);
                                    presentIntPrd = loan.Int_Prd - (prdElapsed - loan.Prl_Prd);
                                }
                                if (presentIntPrd > 0)
                                {
                                    if (intBalIncludingCurrentIntCalc >= intInstalment)
                                        intDemand = intInstalment;
                                    else
                                        intDemand = intBalIncludingCurrentIntCalc;
                                }
                                else
                                    intDemand = 0;
                                break;
                            case 2: /// int along with prl
                                intDemand = Math.Round(intBalIncludingCurrentIntCalc, 0);
                                break;
                            case 3: /// no interest
                                intDemand = 0;
                                break;
                            case 4: /// fixed principal
                                intDemand = Math.Round(intBalIncludingCurrentIntCalc, 0);
                                break;
                        }
                        loan.IntDemand = intDemand;
                        switch (loan.StaffLoan_Int_Type)
                        {
                            case 1: /// int paid after fixed prl
                            case 3: /// no int fixed prl
                            case 4: /// int paid with fixed prl
                                if (loan.PrlOS > loan.Inst_Amt)
                                    prlDemand = loan.Inst_Amt;
                                else
                                    prlDemand = loan.PrlOS;

                                if (loan.FirstPrl_DueDate > toDate.AddDays(-1))
                                    prlDemand = 0;
                                break;
                            case 2: /// int paid with prl (emi)
                                prlDemand = loan.Inst_Amt - intCalc;
                                if (prlDemand > loan.PrlOS) prlDemand = loan.PrlOS;
                                if (loan.FirstPrl_DueDate > toDate.AddDays(-1)) prlDemand = 0;
                                break;
                        }
                        if (loan.SumPrl_Dem >= loan.SumDisb_Amt)
                            prlDemand = 0;
                        //else if (loan.FirstPrl_DueDate > toDate)
                        //    prlDemand = loan.PrlOS;
                        loan.PrlDemand = prlDemand;
                        loan.PrlRecovery = loan.PrlOD + prlDemand;
                        loan.IntRecovery = intDemand;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return loanList;
        }

        public async Task<List<PayLoanBalanceVM>> GetPayLoanBalance(decimal[] loanIdList, DateTime toDate, string brCode)
        {
            double intCalc = 0;
            double intBalIncludingCurrentIntCalc = 0;
            DateTime intCalcUpto;
            int prdElapsed = 0;
            double intInstalment = 0;
            double intDemand = 0;
            double prlDemand = 0;
            int presentIntPrd = 0;
            List<PayLoanBalanceVM> loanList = new();
            try
            {
                #region get record set
                var result = await (from lm in CSISContext.Loan_Master
                                    join mm in CSISContext.mem_master on lm.Mem_Id equals mm.mem_id
                                    join ls in CSISContext.Loan_Schemes on lm.Scheme_Id equals ls.Scheme_Id
                                    join lt in CSISContext.Loan_Trn on lm.Loan_Id equals lt.Loan_Id
                                    where loanIdList.Contains(lm.Loan_Id)
                                          && !lt.TrnTr_Delete
                                          && !lm.Loan_Delete
                                          && lm.BrCode == brCode
                                          && mm.brcode == brCode
                                          && ls.BrCode == brCode
                                          && lt.BrCode == brCode
                                    group new { lm, ls, lt } by new
                                    {
                                        lm.Loan_Id,
                                        lm.Loan_No,
                                        lm.San_Amt,
                                        lm.FirstPrl_DueDate,
                                        lm.FirstInt_DueDate,
                                        lm.San_Date,
                                        lm.Prl_Prd,
                                        lm.Int_Prd,
                                        lm.Inst_Amt,
                                        lm.Roi,
                                        ls.Scheme_Name,
                                        ls.Scheme_Id,
                                        ls.Loan_Type,
                                        ls.PrlLed_Id,
                                        ls.IntLed_Id,
                                        ls.StaffLoan_Int_Type
                                    } into g
                                    where (g.Sum(x => x.lt.Disb_Amt) - g.Sum(x => x.lt.PrlColl_Amt) > 0) ||
                                          (g.Sum(x => x.lt.IntCalc_Amt) - g.Sum(x => x.lt.IntColl_Amt) > 0)
                                    where g.Key.Loan_Type == 5
                                    select new PayLoanBalanceVM
                                    {
                                        Loan_Id = g.Key.Loan_Id,
                                        Loan_No = g.Key.Loan_No,
                                        San_Amt = g.Key.San_Amt,
                                        FirstPrl_DueDate = (DateTime)g.Key.FirstPrl_DueDate!,
                                        FirstInt_DueDate = (DateTime)g.Key.FirstInt_DueDate!,
                                        San_Date = g.Key.San_Date,
                                        Prl_Prd = g.Key.Prl_Prd,
                                        Int_Prd = g.Key.Int_Prd,
                                        Inst_Amt = g.Key.Inst_Amt,
                                        Roi = g.Key.Roi,
                                        Scheme_Name = g.Key.Scheme_Name,
                                        Scheme_Id = g.Key.Scheme_Id,
                                        Loan_Type = g.Key.Loan_Type,
                                        PrlLed_Id = g.Key.PrlLed_Id,
                                        IntLed_Id = g.Key.IntLed_Id,
                                        StaffLoan_Int_Type = g.Key.StaffLoan_Int_Type,
                                        MaxTrn_Date = g.Max(x => x.lt.Trn_Date),
                                        SumDisb_Amt = g.Sum(x => x.lt.Disb_Amt),
                                        SumPrl_Sched = g.Sum(x => x.lt.Prl_Sched),
                                        SumPrl_Dem = g.Sum(x => x.lt.Prl_Dem),
                                        SumPrlColl_Amt = g.Sum(x => x.lt.PrlColl_Amt),
                                        SumIntCalc_Amt = g.Sum(x => x.lt.IntCalc_Amt),
                                        MaxIntCalc_Date = g.Max(x => x.lt.IntCalc_Date),
                                        SumIntColl_Amt = g.Sum(x => x.lt.IntColl_Amt)
                                    }).ToListAsync();
                #endregion 

                if (result != null && result.Any())
                {
                    loanList = result.ToList();

                    foreach (var loan in loanList)
                    {
                        loan.PrlOS = loan.SumDisb_Amt - loan.SumPrlColl_Amt;
                        loan.PrlOD = loan.SumPrl_Dem - loan.SumPrlColl_Amt;
                        if (loan.PrlOD < 0)
                            loan.PrlOD = 0;
                        loan.IntBal = loan.SumIntCalc_Amt - loan.SumIntColl_Amt;
                        if (loan.IntBal < 0)
                            loan.IntBal = 0;
                        if (loan.MaxIntCalc_Date != null)
                            intCalcUpto = ((DateTime)loan.MaxIntCalc_Date).Date;
                        else
                            intCalcUpto = loan.San_Date.Date;
                        intCalc = Utilities.Calculate_Interest(loan.PrlOS, loan.Roi, Utilities.GetNoOfDays(toDate, intCalcUpto));
                        loan.IntCalc = intCalc;
                        if (intCalc > 0)
                        {
                            loan.IntCalcDate = toDate.Date;
                        }
                        intBalIncludingCurrentIntCalc = loan.IntBal + intCalc;
                        //prdElapsed = GeneralService.GetNoOfMonths(toDate, loan.FirstPrl_DueDate);
                        prdElapsed = Utilities.GetMonthsBetweenDates(loan.FirstPrl_DueDate, toDate);
                        switch (loan.StaffLoan_Int_Type)
                        {
                            case 1: /// int after prl
                                if (prdElapsed > loan.Prl_Prd)
                                {
                                    //intInstalment = Math.Round(intBalIncludingCurrentIntCalc / loan.Int_Prd, 0);
                                    intInstalment = Math.Round(loan.SumIntCalc_Amt / loan.Int_Prd, 0);
                                    presentIntPrd = loan.Int_Prd - (prdElapsed - loan.Prl_Prd);
                                }
                                else
                                    presentIntPrd = 0;
                                if (loan.PrlOS == 0)
                                {
                                    //intInstalment = Math.Round(intBalIncludingCurrentIntCalc / loan.Int_Prd, 0);
                                    intInstalment = Math.Round(loan.SumIntCalc_Amt / loan.Int_Prd, 0);
                                    presentIntPrd = loan.Int_Prd - (prdElapsed - loan.Prl_Prd);
                                }
                                if (presentIntPrd > 0)
                                {
                                    if (intBalIncludingCurrentIntCalc >= intInstalment)
                                        intDemand = intInstalment;
                                    else
                                        intDemand = intBalIncludingCurrentIntCalc;
                                }
                                else
                                    intDemand = 0;
                                break;
                            case 2: /// int along with prl
                                intDemand = Math.Round(intBalIncludingCurrentIntCalc, 0);
                                break;
                            case 3: /// no interest
                                intDemand = 0;
                                break;
                            case 4: /// fixed principal
                                intDemand = Math.Round(intBalIncludingCurrentIntCalc, 0);
                                break;
                        }
                        loan.IntDemand = intDemand;
                        switch (loan.StaffLoan_Int_Type)
                        {
                            case 1: /// int paid after fixed prl
                            case 3: /// no int fixed prl
                            case 4: /// int paid with fixed prl
                                if (loan.PrlOS > loan.Inst_Amt)
                                    prlDemand = loan.Inst_Amt;
                                else
                                    prlDemand = loan.PrlOS;

                                if (loan.FirstPrl_DueDate > toDate.AddDays(-1))
                                    prlDemand = 0;
                                break;
                            case 2: /// int paid with prl (emi)
                                prlDemand = loan.Inst_Amt - intCalc;
                                if (prlDemand > loan.PrlOS) prlDemand = loan.PrlOS;
                                if (loan.FirstPrl_DueDate > toDate.AddDays(-1)) prlDemand = 0;
                                break;
                        }
                        if (loan.SumPrl_Dem >= loan.SumDisb_Amt)
                            prlDemand = 0;
                        //else if (loan.FirstPrl_DueDate > toDate)
                        //    prlDemand = loan.PrlOS;
                        loan.PrlDemand = prlDemand;
                        loan.PrlRecovery = loan.PrlOD + prlDemand;
                        loan.IntRecovery = intDemand;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return loanList;
        }

        public async Task<DtoLoanDisbursementStaff> GetStaffLoanDisbursement(decimal vocId, string brCode)
        {
            DtoLoanDisbursementStaff staffLoan = new();
            try
            {
                var result = await (from master in CSISContext.Loan_Master
                                    join schemes in CSISContext.Loan_Schemes on master.Scheme_Id equals schemes.Scheme_Id
                                    where master.Voc_Id == vocId
                                    && master.BrCode == brCode
                                    select new DtoLoanDisbursementStaff
                                    {
                                        Transaction_Date = master.San_Date,
                                        Loan_No = master.Loan_No,
                                        Scheme_Id = master.Scheme_Id,
                                        Scheme_Name = schemes.Scheme_Name,
                                        ResolutionNo = master.Res_No,
                                        ResolutionDate = master.Res_Date,
                                        DisbursementAmount = master.San_Amt,
                                        Principal_Period = master.Prl_Prd,
                                        Interest_Period = master.Int_Prd,
                                        Rate_Of_Interest = master.Roi,
                                        InstalmentStart_Date = (DateTime)master.FirstPrl_DueDate!,
                                        InstalmentAmount = master.Inst_Amt,
                                    }).FirstOrDefaultAsync();
                if (result != null && result.Scheme_Id > 0) staffLoan = result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return staffLoan;
        }
        #endregion 

        #region term deposit loans
        public async Task<List<decimal>> GetLoanIdListByTdIdListAsync(decimal[] tdIds, string brCode)
        {
            List<decimal> loanIds = new List<decimal>();
            try
            {
                var list = await (from lien in CSISContext.Lien_Trn
                                  where lien.LienTr_Delete == false && lien.BrCode == brCode && tdIds.Contains(lien.TD_Id)
                                  select lien.Loan_Id)
                   .Distinct()
                   .ToListAsync();
                if (list.Count > 0) loanIds = list;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching loan id list by tdid list");
            }
            return loanIds;
        }

        public async Task<List<TDLoanData>> GetTDLoanDetailsByTDIds(decimal[] tdIds, string brCode)
        {
            List<TDLoanData> loanList = new List<TDLoanData>();
            try
            {
                #region old linq
                //var query  =
                //    await  (from  lien in CSISContext.Lien_Trn
                //    join loan in CSISContext.Loan_Master on lien.Loan_Id equals loan.Loan_Id
                //    join trn in CSISContext.Loan_Trn on loan.Loan_Id equals trn.Loan_Id
                //    where tdIds.Contains(lien.TD_Id)
                //          && lien.LienTr_Delete == false
                //          && loan.Loan_Delete == false
                //          && trn.TrnTr_Delete == false
                //    group trn by new
                //    {
                //        lien.Loan_Id,
                //        loan.Loan_No,
                //        loan.San_Date,
                //        loan.San_Amt
                //    } into g
                //    let disbAmt = g.Sum(x => x.Disb_Amt)
                //    let prlCollAmt = g.Sum(x => x.PrlColl_Amt)
                //    let intCalcAmt = g.Sum(x => x.IntCalc_Amt)
                //    let intCollAmt = g.Sum(x => x.IntColl_Amt)
                //    where (disbAmt - prlCollAmt) > 0
                //    select new TDLoanDetailsVM 
                //    {
                //        Loan_Id = g.Key.Loan_Id,
                //        Loan_No = g.Key.Loan_No,
                //        Loan_Date = g.Key.San_Date,
                //        Loan_Amount = g.Key.San_Amt,
                //        Loan_Outstanding = disbAmt - prlCollAmt,
                //        Interest_Balance = intCalcAmt - intCollAmt,
                //        Interest_Calculated = intCalcAmt,
                //        Interest_Applied_Date = g.Max(x => x.IntCalc_Date),
                //    }).ToListAsync();
                #endregion 

                var query =
                   await (from lien in CSISContext.Lien_Trn
                          join loan in CSISContext.Loan_Master on lien.Loan_Id equals loan.Loan_Id
                          join trn in CSISContext.Loan_Trn on loan.Loan_Id equals trn.Loan_Id
                          where tdIds.Contains(lien.TD_Id)
                                && lien.LienTr_Delete == false
                                && loan.Loan_Delete == false
                                && trn.TrnTr_Delete == false
                                && lien.BrCode == brCode
                                && loan.BrCode == brCode
                                && trn.BrCode == brCode
                          select new
                          {
                              lien.Loan_Id,
                              loan.Loan_No,
                              loan.San_Date,
                              loan.San_Amt,
                              trn.Disb_Amt,
                              trn.PrlColl_Amt,
                              trn.IntCalc_Amt,
                              trn.IntColl_Amt,
                              trn.IntCalc_Date
                          })
                    .GroupBy(x => new
                    {
                        x.Loan_Id,
                        x.Loan_No,
                        x.San_Date,
                        x.San_Amt
                    })
                    .Where(g => g.Sum(x => x.Disb_Amt) - g.Sum(x => x.PrlColl_Amt) > 0)
                    .Select(g => new TDLoanData
                    {
                        Loan_Id = g.Key.Loan_Id,
                        Loan_No = g.Key.Loan_No,
                        Loan_Date = g.Key.San_Date,
                        Loan_Amount = g.Key.San_Amt,
                        Loan_Outstanding = g.Sum(x => x.Disb_Amt) - g.Sum(x => x.PrlColl_Amt),
                        Interest_Calculated = g.Sum(x => x.IntCalc_Amt),
                        Interest_Applied_Date = g.Max(x => x.IntCalc_Date),
                        Interest_Balance = g.Sum(x => x.IntCalc_Amt) - g.Sum(x => x.IntColl_Amt)
                    }).ToListAsync();
                if (query != null && query.Count > 0) loanList = query.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return loanList;
        }

        public async Task<List<DtoTermDepositLoan>> GetTDLoanDataByTDIds(List<decimal> tdIdList, string brCode)
        {
            List<DtoTermDepositLoan> loanList = new List<DtoTermDepositLoan>();
            try
            {
                var query = await (from lien in CSISContext.Lien_Trn
                                   where lien.BrCode == brCode && tdIdList.Contains(lien.TD_Id)
                                   join loan in CSISContext.Loan_Master on lien.Loan_Id equals loan.Loan_Id
                                   join trn in CSISContext.Loan_Trn on loan.Loan_Id equals trn.Loan_Id
                                   group new { lien, loan, trn } by new
                                   {
                                       lien.TD_Id,
                                       lien.Loan_Id,
                                       loan.Loan_No,
                                       loan.San_Date,
                                       loan.San_Amt,
                                       loan.Roi
                                   } into g
                                   select new DtoTermDepositLoan
                                   {
                                       TD_Id = g.Key.TD_Id,
                                       Loan_Id = g.Key.Loan_Id,
                                       Loan_No = g.Key.Loan_No,
                                       Loan_Date = g.Key.San_Date,
                                       Loan_Amount = g.Key.San_Amt,
                                       Rate_Of_Interest = g.Key.Roi,
                                       Interest_Overdue = g.Sum(x => x.trn.IntCalc_Amt) - g.Sum(x => x.trn.IntColl_Amt),
                                       Principal_Balance = g.Key.San_Amt - g.Sum(x => x.trn.PrlColl_Amt),
                                       Fixed_Deposit_Face_Value = g.Sum(x => x.lien.LienTr_Amount),
                                       IntCalc_Date = g.Max(x => x.trn.IntCalc_Date)
                                   }).ToListAsync();

                if (query != null) loanList = query.ToList();
            }
            catch (Exception)
            {
                throw;
            }
            return loanList;
        }

        public async Task<List<DtoTermDepositLoanBalance>> GetTDLoanBalanceByTDIds(List<decimal> loanIdList, DateTime toDate, string brCode)
        {
            List<DtoTermDepositLoanBalance> loanList = new List<DtoTermDepositLoanBalance>();
            DateTime IntCalcDate;
            double intCalc = 0;
            try
            {
                //var result = await (from master in CSISContext.Loan_Master
                //                    join scheme in CSISContext.Loan_Schemes on master.Scheme_Id equals scheme.Scheme_Id
                //                    join trn in CSISContext.Loan_Trn on master.Loan_Id equals trn.Loan_Id
                //                    where !trn.TrnTr_Delete && !master.Loan_Delete
                //                    && master.BrCode == brCode && trn.BrCode == brCode && scheme.BrCode == brCode
                //                    group trn by new
                //                    {
                //                        master.Loan_Id,
                //                        master.Loan_No,
                //                        master.San_Date,
                //                        master.San_Amt,
                //                        master.Roi,
                //                        scheme.PrlLed_Id,
                //                        scheme.IntLed_Id
                //                    } into g
                //                    let principalBalance = g.Key.San_Amt - g.Sum(x => x.PrlColl_Amt)
                //                    where loanIdList.Contains(g.Key.Loan_Id) && principalBalance > 0

                //                    select new DtoTermDepositLoanBalance
                //                    {
                //                        Loan_Id = g.Key.Loan_Id,
                //                        Loan_No = g.Key.Loan_No,
                //                        Loan_Date = g.Key.San_Date,
                //                        Loan_Amount = g.Key.San_Amt,
                //                        Rate_Of_Interest = g.Key.Roi,
                //                        Interest_Balance = g.Sum(x => x.IntColl_Amt) - g.Sum(x => x.IntColl_Amt), // always 0
                //                        Current_Interest = 0.0f,
                //                        IntCalc_Date = g.Max(x => x.IntCalc_Date),
                //                        Principal_Balance = principalBalance,
                //                        Total_Collection = 0.0f,
                //                        Interest_Collection = 0.0f,
                //                        Principal_Collection = 0.0f,
                //                        Total_Balance = 0.0f,
                //                        PrlLed_Id = g.Key.PrlLed_Id,
                //                        IntLed_Id = g.Key.IntLed_Id
                //                    }).ToListAsync();

                //var result = await (from master in CSISContext.Loan_Master
                //                    join scheme in CSISContext.Loan_Schemes on master.Scheme_Id equals scheme.Scheme_Id
                //                    join trn in CSISContext.Loan_Trn on master.Loan_Id equals trn.Loan_Id
                //                    where !trn.TrnTr_Delete && !master.Loan_Delete
                //                          && master.BrCode == brCode && trn.BrCode == brCode && scheme.BrCode == brCode
                //                    group trn by new
                //                    {
                //                        master.Loan_Id,
                //                        master.Loan_No,
                //                        master.San_Date,
                //                        master.San_Amt,
                //                        master.Roi,
                //                        scheme.PrlLed_Id,
                //                        scheme.IntLed_Id
                //                    } into g
                //                    select new
                //                    {
                //                        g,
                //                        principalBalance = g.Key.San_Amt - g.Sum(x => x.PrlColl_Amt)
                //                    })
                //    .AsEnumerable() // 👈 move to client-side processing
                //    .Where(x => loanIdList.Contains(x.g.Key.Loan_Id) && x.principalBalance > 0)
                //    .Select(x => new DtoTermDepositLoanBalance
                //    {
                //        Loan_Id = x.g.Key.Loan_Id,
                //        Loan_No = x.g.Key.Loan_No,
                //        Loan_Date = x.g.Key.San_Date,
                //        Loan_Amount = x.g.Key.San_Amt,
                //        Rate_Of_Interest = x.g.Key.Roi,
                //        Interest_Balance = 0.0f,
                //        Current_Interest = 0.0f,
                //        IntCalc_Date = x.g.Max(t => t.IntCalc_Date),
                //        Principal_Balance = x.principalBalance,
                //        Total_Collection = 0.0f,
                //        Interest_Collection = 0.0f,
                //        Principal_Collection = 0.0f,
                //        Total_Balance = 0.0f,
                //        PrlLed_Id = x.g.Key.PrlLed_Id,
                //        IntLed_Id = x.g.Key.IntLed_Id
                //    }).ToListAsync();

                var result = await (from master in CSISContext.Loan_Master
                                    join scheme in CSISContext.Loan_Schemes on master.Scheme_Id equals scheme.Scheme_Id
                                    join trn in CSISContext.Loan_Trn on master.Loan_Id equals trn.Loan_Id
                                    where !trn.TrnTr_Delete && !master.Loan_Delete
                                    && master.BrCode == brCode && trn.BrCode == brCode && scheme.BrCode == brCode
                                    group trn by new
                                    {
                                        master.Loan_Id,
                                        master.Loan_No,
                                        master.San_Date,
                                        master.San_Amt,
                                        master.Roi,
                                        scheme.PrlLed_Id,
                                        scheme.IntLed_Id
                                    } into g
                                    where loanIdList.Contains(g.Key.Loan_Id) && g.Key.San_Amt - g.Sum(x => x.PrlColl_Amt) > 0

                                    select new DtoTermDepositLoanBalance
                                    {
                                        Loan_Id = g.Key.Loan_Id,
                                        Loan_No = g.Key.Loan_No,
                                        Loan_Date = g.Key.San_Date,
                                        Loan_Amount = g.Key.San_Amt,
                                        Rate_Of_Interest = g.Key.Roi,
                                        Interest_Balance = g.Sum(x => x.IntColl_Amt) - g.Sum(x => x.IntColl_Amt), // always 0
                                        Current_Interest = 0.0f,
                                        IntCalc_Date = g.Max(x => x.IntCalc_Date),
                                        Principal_Balance = g.Key.San_Amt - g.Sum(x => x.PrlColl_Amt),
                                        Total_Collection = 0.0f,
                                        Interest_Collection = 0.0f,
                                        Principal_Collection = 0.0f,
                                        Total_Balance = 0.0f,
                                        PrlLed_Id = g.Key.PrlLed_Id,
                                        IntLed_Id = g.Key.IntLed_Id
                                    }).ToListAsync();

                if (result != null)
                {

                    var maxMaturityDate = (from lien in CSISContext.Lien_Trn
                                           join td in CSISContext.TermDeposit_Master
                                           on lien.TD_Id equals td.TD_Id
                                           where loanIdList.Contains(lien.Loan_Id)
                                           select td.MaturityDate).Max();
                    //if (toDate > maxMaturityDate)
                    //{
                    //    toDate = maxMaturityDate;
                    if (toDate > maxMaturityDate) toDate = maxMaturityDate;
                    loanList = result.ToList();
                    foreach (var loan in loanList)
                    {
                        if (loan.IntCalc_Date != null)
                            IntCalcDate = Convert.ToDateTime(loan.IntCalc_Date);
                        else
                            IntCalcDate = loan.Loan_Date;
                        intCalc = 0;
                        intCalc = Utilities.Calculate_Interest(loan.Principal_Balance, loan.Rate_Of_Interest, Utilities.GetNoOfDays(toDate, IntCalcDate));
                        loan.Current_Interest = intCalc;
                        loan.Interest_Outstanding = loan.Interest_Balance + intCalc;
                        loan.Total_Balance = loan.Principal_Balance + loan.Interest_Balance + intCalc;
                        loan.IntCalc_Date = IntCalcDate;
                        loan.Interest_Applied_Date = toDate;
                    }
                    //}
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            return loanList;
        }

        #endregion

        public int Get_MaxLoanSlNo(decimal LoanId)
        {
            int MaxSlNo = 0;
            try
            {
                MaxSlNo = (CSISContext.Loan_Trn
                  .Where(t => t.Loan_Id == LoanId)
                  .Select(t => (int?)t.Trn_SlNo) // Cast to nullable int to handle empty sequences
                  .Max() ?? 0) + 1;

            }
            catch
            {
                MaxSlNo = 1;
            }
            return MaxSlNo;
        }

        public Task<List<LoanDetailsVM>> GetLoanDetailsList2ByLoanIdsAsync(decimal[] loanIds, string brCode)
        {
            throw new NotImplementedException();
        }

        #region ECS Demand Calculation
        public async Task<List<Mem_Demand>> CalculateLoanDemand(decimal memid, int memberStatus, DateTime demandCalcDate, DateTime demandDate)
        {
            DateTime? expiryDate = null;
            bool IsStopDemand = false;
            DateTime FromDate;
            DateTime LastDueDate;
            double prlDemand = 0;
            double loanOS = 0;
            double prlOD = 0;
            List<LoanDetailsVM> loanBalanceList = new List<LoanDetailsVM>();
            List<Mem_Demand> loandemand = new List<Mem_Demand>();

            var loanList = await (from master in CSISContext.Loan_Master
                                  join scheme in CSISContext.Loan_Schemes on master.Scheme_Id equals scheme.Scheme_Id
                                  join trn in CSISContext.Loan_Trn on master.Loan_Id equals trn.Loan_Id
                                  where trn.TrnTr_Delete == false && master.Loan_Delete == false && master.Loan_Type == 1
                                  group new { master, scheme, trn } by new
                                  {
                                      master.Mem_Id,
                                      master.Loan_Id,
                                      master.Scheme_Id,
                                      master.Loan_No,
                                      master.Roi,
                                      master.Prl_Prd,
                                      master.Int_Prd,
                                      master.Inst_Amt,
                                      master.San_Amt,
                                      master.San_Date,
                                      scheme.Scheme_Name,
                                      scheme.PrlLed_Id,
                                      scheme.IntLed_Id,
                                      scheme.IODLed_Id,
                                      scheme.PILed_Id,
                                      scheme.Inst_Type,
                                      scheme.Int_Application,
                                      scheme.PI_Application,
                                      scheme.IOD_Application,
                                      scheme.MatchShareCapital,
                                      scheme.AdoptLoanLimit,
                                      master.SecurityFaceValue,
                                      master.FirstInt_DueDate,
                                      master.FirstPrl_DueDate
                                  } into g
                                  let disbAmt = g.Sum(x => x.trn.Disb_Amt)
                                  let prlColl = g.Sum(x => x.trn.PrlColl_Amt)
                                  let intCalc = g.Sum(x => x.trn.IntCalc_Amt)
                                  let intColl = g.Sum(x => x.trn.IntColl_Amt)
                                  where g.Key.Mem_Id == memid
                                        && (disbAmt - prlColl > 0 || intCalc - intColl > 0)
                                        && disbAmt - prlColl > 0
                                  select new LoanDetailsVM
                                  {
                                      memid = g.Key.Mem_Id,
                                      loanid = g.Key.Loan_Id,
                                      schemeid = g.Key.Scheme_Id,
                                      loanno = g.Key.Loan_No,
                                      roi = g.Key.Roi,
                                      Prl_Prd = g.Key.Prl_Prd,
                                      Int_Prd = g.Key.Int_Prd,
                                      instalmentamt = g.Key.Inst_Amt,
                                      disbursementdate = g.Key.San_Date,
                                      schemename = g.Key.Scheme_Name,
                                      prlledid = g.Key.PrlLed_Id,
                                      intledid = g.Key.IntLed_Id,
                                      iodledid = g.Key.IODLed_Id,
                                      piledid = g.Key.PILed_Id,
                                      instalType = g.Key.Inst_Type,
                                      intapplication = g.Key.Int_Application,
                                      piapplication = g.Key.PI_Application,
                                      iodapplication = g.Key.IOD_Application,
                                      matchShareCapital = g.Key.MatchShareCapital,
                                      adoptLoanLimit = g.Key.AdoptLoanLimit,
                                      disbamt = disbAmt,
                                      sanctionamt = g.Key.San_Amt,
                                      sanctiondate = g.Key.San_Date,
                                      prlschedule = g.Sum(x => x.trn.Prl_Sched),
                                      prldemand = g.Sum(x => x.trn.Prl_Dem),
                                      prlcoll = prlColl,
                                      intcalulatedamt = intCalc,
                                      maxintcalcdate = g.Max(x => x.trn.IntCalc_Date),
                                      intcollamt = intColl,
                                      picalulatedamt = g.Sum(x => x.trn.PICalc_Amt),
                                      maxpicalcdate = g.Max(x => x.trn.PICalc_Date),
                                      picollamt = g.Sum(x => x.trn.PIColl_Amt),
                                      iodcalculatedamt = g.Sum(x => x.trn.IODCalc_Amt),
                                      iodcalcamt = g.Sum(x => x.trn.IODColl_Amt),
                                      trndate = g.Min(x => x.trn.Trn_Date),
                                      securityfacevalue = g.Key.SecurityFaceValue,
                                      maxtrnslno = g.Max(x => x.trn.Trn_SlNo),
                                      firstintduedate = g.Key.FirstInt_DueDate,
                                      firstprlduedate = g.Key.FirstPrl_DueDate
                                  }).ToListAsync();
            if (loanList != null && loanList.Count > 0)
            {
                loanBalanceList = loanList.ToList();
            }

            var count = CSISContext.Mem_Demand_Stop
            .Where(x => x.Is_Active == true
                        && x.Mem_Id == memid
                        && x.Demand_Date == DateOnly.FromDateTime(demandDate))
            .Count();
            if (count > 0) IsStopDemand = true; else IsStopDemand = false;

            if (memberStatus == 2)
                IsStopDemand = true;
            else
                IsStopDemand = false;

            LoanInterestCalculatedItems calcItems = [];
            try
            {
                if (loanBalanceList != null && loanBalanceList.Count > 0)
                {
                    foreach (LoanDetailsVM bal in loanBalanceList)
                    {
                        expiryDate = null;
                        LastDueDate = Utilities.AddMonths((DateTime)bal.firstprlduedate!, bal.Prl_Prd);
                        if (bal.maxintcalcdate == null) FromDate = bal.disbursementdate.Date;
                        else FromDate = (DateTime)bal.maxintcalcdate.Value.Date;
                        bal.prlcoll = Math.Round(bal.prlcoll, 2);



                        calcItems = CalculateLnDues(bal.loanid, FromDate, demandDate, bal.intapplication, bal.piapplication, bal.iodapplication, bal.disbamt, bal.prlcoll, bal.prlschedule, bal.prldemand, bal.intcalulatedamt, bal.intcollamt, "S");

                        bal.intcalcamt = calcItems.InterestCalculatAmt;
                        bal.intcalcdate = calcItems.InterestCalculateDate;
                        bal.picalcamt = calcItems.PICalculateAmt;
                        bal.picalcdate = calcItems.PICalcuateDate;
                        bal.iodcalcamt = calcItems.IODCalculateAmt;

                        expiryDate = CSISContext.mem_master.Where(x => x.mem_id == memid).Select(x => x.expireddate).FirstOrDefault();
                        //expiryDate = dbMember.GetMemberExpiryDate(memid, out errorMessage);
                        //if (errorMessage.Length > 0)
                        //{

                        //}
                        if (expiryDate != null)
                        {
                            if (demandDate > (DateTime)expiryDate.Value)
                            {
                                bal.intcalcamt = 0;
                                bal.picalcamt = 0;
                                bal.iodcalcamt = 0;
                            }
                        }

                        loanOS = bal.disbamt - bal.prlcoll;
                        prlOD = bal.prldemand - bal.prlcoll;

                        Mem_Demand single = new Mem_Demand();

                        single.Id = 0;
                        single.Loan_Id = bal.loanid;
                        single.Demand_Id = 0;
                        single.Calculated_Date = DateOnly.FromDateTime( demandCalcDate);
                        single.Recovery_Date = null;
                        single.Demand_Type = "L";
                        single.Mem_Id = memid;
                        single.Loan_PI_Arrear = bal.picalulatedamt - bal.picollamt;
                        single.Loan_PI_Current = bal.picalcamt;
                        if (bal.intcalulatedamt - bal.intcollamt < 0)
                            single.Loan_Int_Arrear = 0;
                        else
                            single.Loan_Int_Arrear = bal.intcalulatedamt - bal.intcollamt;
                        single.Loan_Int_Current  = bal.intcalcamt;

                        single.Loan_Prl_Arrear  = bal.prldemand - bal.prlcoll;
                        if (bal.prldemand - bal.prlcoll < 0)
                            single.Loan_Prl_Arrear  = 0;
                        single.Loan_Oustanding = bal.disbamt - bal.prlcoll;

                        /// Get principal demand
                        if (bal.firstprlduedate <= demandDate)
                        {
                            switch (bal.instalType)
                            {
                                case 1:
                                    prlDemand = bal.instalmentamt;
                                    break;
                                case 2:
                                    prlDemand = bal.instalmentamt - bal.intcalcamt;
                                    break;
                                case 3:
                                    prlDemand = bal.instalmentamt;
                                    break;
                                case 4:
                                    prlDemand = 0;
                                    break;
                            }
                        }
                        else
                        {
                            prlDemand = 0;
                        }


                        //prlDemand = Utilities.GetLoanPrincipalDemand(bal.firstprlduedate, demandDate, bal.instalType, bal.instalmentamt, single.IntCurrentDemand.Value, out errorMessage);

                        //if (bal.firstprlduedate > demandDate)
                        //{
                            //if (TSISGlobalVariables.gSocietyId == 5) /// tvs society
                            //{
                            //    IsStopDemand = true;
                            //}
                        //}
                        if (prlDemand > loanOS)
                            prlDemand = loanOS;
                        if (demandDate > LastDueDate)
                        {
                            prlDemand = loanOS;
                        }
                        if (prlDemand + prlOD > loanOS)
                        {
                            prlDemand = prlDemand - ((prlOD + prlDemand) - loanOS);
                        }
                       
                        single.Loan_Prl_Current  = prlDemand;
                        single.Loan_Total = single.Loan_PI_Arrear + single.Loan_PI_Current + single.Loan_Int_Arrear + single.Loan_Int_Current + single.Loan_Prl_Arrear + single.Loan_Prl_Current;

                        //single.PITotalDemand = single.PIArrearDemand + single.PICurrentDemand;
                        //single.IntTotalDemand = single.IntArrearDemand + single.IntCurrentDemand;
                        //single.PrlTotalDemand = single.PrlArrearDemand + single.PrlCurrentDemand;
                        single.Prl_Schedule  = single.Loan_Prl_Current;
                        single.Non_OD_Prl = bal.disbamt - bal.prldemand;
                        single.Deposit_Id = 0;

                        //single.ArrearDepositAmount = 0;
                        //single.CurrentDepositAmount = 0;
                        //single.TotalDepositAmount = 0;
                        //single.ArrearPIOnDeposit = 0;
                        //single.CurrentPIOnDeposit = 0;
                        //single.TotalPIOnDeposit = 0;
                        //single.SuspenseLed_Id = 0;
                        //single.ArrearSuspenseAmount = 0;
                        //single.CurrentSuspenseAmount = 0;
                        //single.TotalSuspenseAmount = 0;
                        //single.TotalDemandAmount = single.PIArrearDemand + single.PICurrentDemand + single.IntArrearDemand + single.IntCurrentDemand + single.PrlArrearDemand + single.PrlCurrentDemand;
                        //single.PIArrearCollection = 0;
                        //single.IntArrearCollection = 0;
                        //single.PrlArrearCollection = 0;
                        //single.PICurrentCollection = 0;
                        //single.IntCurrentCollection = 0;
                        //single.PrlCurrentCollection = 0;
                        //single.PITotalCollection = 0;
                        //single.IntTotalCollection = 0;
                        //single.PrlTotalCollection = 0;
                        single.Stop_Demand = IsStopDemand;
                        //single.usr_Id = usrId;
                        //single.yr_id = yrId;
                        //single.ReferId = 0;
                        //single.ArrearDepositCollection = 0;
                        //single.CurrentDepositCollection = 0;
                        //single.TotalDepositCollection = 0;
                        //single.ArrearPIOnDepositCollection = 0;
                        //single.CurrentPIOnDepositCollection = 0;
                        //single.TotalPIOnDepositCollection = 0;
                        //single.ArrearSuspenseCollection = 0;
                        //single.CurrentSuspenseCollection = 0;
                        //single.TotalSuspenseCollection = 0;
                        //single.TotalCollectionAmount = 0;
                        //single.SuspenseDueByLed_Id = 0;
                        //single.TotalDueByCollection = 0;
                        //single.voc_id = 0;
                        //single.PenalInterestCalculatedAmount = 0;
                        //single.InterestCalculatedAmount = 0;
                        //single.Prl_SchedForStopDemand = 0;
                        //single.IntCalcForStopDemand = 0;
                        //single.Recovery_Id = 0;
                        //single.PIArrearDemand2 = 0;
                        //single.IntArrearDemand2 = 0;
                        //single.PrlArrearDemand2 = 0;
                        //single.PICurrentDemand2 = 0;
                        //single.IntCurrentDemand2 = 0;
                        //single.PrlCurrentDemand2 = 0;
                        //single.PITotalDemand2 = 0;
                        //single.IntTotalDemand2 = 0;
                        //single.PrlTotalDemand2 = 0;
                        //single.ArrearDepositAmount2 = 0;
                        //single.CurrentDepositAmount2 = 0;
                        //single.TotalDepositAmount2 = 0;
                        //single.ArrearPIOnDeposit2 = 0;
                        //single.CurrentPIOnDeposit2 = 0;
                        //single.TotalPIOnDeposit2 = 0;
                        //single.ArrearSuspenseAmount2 = 0;
                        //single.CurrentSuspenseAmount2 = 0;
                        //single.TotalSuspenseAmount2 = 0;
                        //single.TotalDemandAmount2 = 0;
                        //single.TD_Id = 0;
                        //single.RDInstalmentAmount = 0;
                        //single.RDNoOfInstalments = 0;
                        //single.RDDemandAmount = 0;
                        //single.PIOnRDArrearAmount = 0;
                        //single.PIOnRDCalcAmount = 0;
                        //single.PIOnRDTotalAmount = 0;
                        //single.RDTotalDemandAmount = 0;
                        //single.RDReceiptAmount = 0;
                        //single.PIOnRDReceiptAmount = 0;
                        //single.RDTotalReceiptAmount = 0;
                        //single.RDNoOfInstalments2 = 0;
                        //single.RDDemandAmount2 = 0;
                        //single.PIOnRDArrearAmount2 = 0;
                        //single.PIOnRDCalcAmount2 = 0;
                        //single.PIOnRDTotalAmount2 = 0;
                        //single.RDTotalDemandAmount2 = 0;
                        //single.RDReceiptAmount2 = 0;
                        //single.PIOnRDReceiptAmount2 = 0;
                        //single.RestrictedDemand = 0;
                        //single.TotalDemand = 0;
                        //single.TotalLoanArrearDemand = 0;
                        //single.TotalLoanCurrentDemand = 0;
                        //single.TotalRDDemand = 0;
                        //single.TotalDepositDemand = 0;
                        //single.DueByDemandLed_Id = 0;
                        //single.ArrearDueByDemandAmount = 0;
                        //single.CurrentDueByDemandAmount = 0;
                        //single.TotalDueByDemandAmount = 0;
                        //single.ArrearDueByCollectionOnDemand = 0;
                        //single.CurrentDueByCollectionOnDemand = 0;
                        //single.TotalDueByCollectionOnDemand = 0;
                        if (single.Loan_Total > 0)
                        {
                            //demId += 1;
                            loandemand.Add(single);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return loandemand;
        }

        public LoanInterestCalculatedItems CalculateLnDues(decimal loanid, DateTime FromDate, DateTime ToDate, int InterestApplication, int PIApplication, int IODApplication, double DisbAmt, double PrlColl, double PrlSchedule, double PrlDemand, double IntCalcAmt, double IntCollAmt, string DisbAgency)
        {
            LoanInterestCalculatedItems calcLnDues = [];
            double LoanOS = DisbAmt - PrlColl;
            double NonODPrl = DisbAmt - PrlDemand;
            double PrlOD = PrlDemand - PrlColl;
            double IntOD = IntCalcAmt - IntCollAmt;

            if (NonODPrl < 0)
            {
                NonODPrl = 0;
            }

            //double roi = 0;

            /// calculate Interest
            if (InterestApplication == 1) /// int on non od prl
            {

                calcLnDues.InterestCalculatAmt = Calc_Int_OnVariable_ROI(NonODPrl, loanid, FromDate, ToDate, DisbAgency);
                if (calcLnDues.InterestCalculatAmt > 0)
                    calcLnDues.InterestCalculateDate = ToDate;
            }
            else if (InterestApplication == 2)    // int on prl os
            {
                calcLnDues.InterestCalculatAmt = Calc_Int_OnVariable_ROI(LoanOS, loanid, FromDate, ToDate, DisbAgency);
                if (calcLnDues.InterestCalculatAmt > 0)
                    calcLnDues.InterestCalculateDate = ToDate;
            }

            /// calculate PI
            if (PIApplication == 1)   /// no pi
            {
                calcLnDues.PICalculateAmt = 0;
                calcLnDues.PICalcuateDate = null;
            }
            if (PIApplication == 2)  /// pi on prl od
            {
                if (PrlOD > 0)
                {
                    calcLnDues.PICalculateAmt = Calc_PI_OnVariable_ROI(PrlOD, loanid, FromDate, ToDate, DisbAgency);
                    if (calcLnDues.PICalculateAmt > 0)
                        calcLnDues.PICalcuateDate = ToDate;
                }
                else
                {
                    calcLnDues.PICalculateAmt = 0;
                    calcLnDues.PICalcuateDate = null;
                }
            }
            else if (PIApplication == 3) /// pi on prl od + int od
            {
                if (PrlOD + IntOD > 0)
                {
                    calcLnDues.PICalculateAmt = Calc_PI_OnVariable_ROI(PrlOD + IntOD, loanid, FromDate, ToDate, DisbAgency);
                    if (calcLnDues.PICalculateAmt > 0)
                        calcLnDues.PICalcuateDate = ToDate;
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
            /// calculate IOD
            if (IODApplication == 1)    /// no iod
            {
                calcLnDues.IODCalculateAmt = 0;
            }
            else if (IODApplication == 2) /// iod on prlod
            {
                calcLnDues.IODCalculateAmt = Calc_Int_OnVariable_ROI(PrlOD, loanid, ToDate, FromDate, DisbAgency);

            }
            else if (IODApplication == 3) /// iod on prlod + intod 
            {
                calcLnDues.IODCalculateAmt = Calc_Int_OnVariable_ROI(PrlOD + IntOD, loanid, FromDate, ToDate, DisbAgency);
            }
            return calcLnDues;
        }

        public double Calc_Int_OnVariable_ROI(double amt, decimal loanid, DateTime FromDate, DateTime ToDate, string Agency)
        {
            double roi = 0;
            DateTime tmpToDate;
            RateOfInterestVM roiVM = new RateOfInterestVM();

            List<RateOfInterestVM> roilist = [];
            double intCalcAmt = 0;

            var tempRoi = CSISContext.Loan_Roi
            .Where(x => x.Loan_Id == loanid
                        && x.Agency == Agency
                        && x.Roi_Wef <= FromDate)
            .OrderByDescending(x => x.Roi_Wef)
            .Select(x => new RateOfInterestVM
            {
                Roi = x.Roi,
                Pi = x.Pi,
                Roi_Wef = x.Roi_Wef
            }).FirstOrDefault();

            if (tempRoi != null && tempRoi.Roi > 0 && tempRoi.Pi > 0)
            {
                roiVM = tempRoi;
            }
            roi = roiVM.Roi;
            var tempRoiList = CSISContext.Loan_Roi
                    .Where(x => x.Loan_Id == loanid
                                && x.Agency == Agency
                                && x.Roi_Wef >= FromDate
                                && x.Roi_Wef <= ToDate
                                && !x.Loanroi_Delete)
                    .OrderBy(x => x.Roi_Wef)
                    .Select(x => new RateOfInterestVM
                    {
                        Roi = x.Roi,
                        Pi = x.Pi,
                        Roi_Wef = x.Roi_Wef
                    }).ToList();

            if (tempRoiList != null && tempRoiList.Count > 0)
            {
                roilist = tempRoiList.ToList();
            }
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

        public double Calc_PI_OnVariable_ROI(double amt, decimal loanid, DateTime FromDate, DateTime ToDate, string Agency)
        {
            double roi = 0;
            DateTime tmpToDate;
            RateOfInterestVM roiVM = new RateOfInterestVM();

            List<RateOfInterestVM> roilist = [];
            double intCalcAmt = 0;

            var tempRoi = CSISContext.Loan_Roi
            .Where(x => x.Loan_Id == loanid
                        && x.Agency == Agency
                        && x.Roi_Wef <= FromDate)
            .OrderByDescending(x => x.Roi_Wef)
            .Select(x => new RateOfInterestVM
            {
                Roi = x.Roi,
                Pi = x.Pi,
                Roi_Wef = x.Roi_Wef
            }).FirstOrDefault();

            if (tempRoi != null && tempRoi.Roi > 0 && tempRoi.Pi > 0)
            {
                roiVM = tempRoi;
            }
            roi = roiVM.Pi;
            var tempRoiList = CSISContext.Loan_Roi
                    .Where(x => x.Loan_Id == loanid
                                && x.Agency == Agency
                                && x.Roi_Wef >= FromDate
                                && x.Roi_Wef <= ToDate
                                && !x.Loanroi_Delete)
                    .OrderBy(x => x.Roi_Wef)
                    .Select(x => new RateOfInterestVM
                    {
                        Roi = x.Roi,
                        Pi = x.Pi,
                        Roi_Wef = x.Roi_Wef
                    }).ToList();

            if (tempRoiList != null && tempRoiList.Count > 0)
            {
                roilist = tempRoiList.ToList();
            }

            foreach (RateOfInterestVM single in roilist)
            {
                tmpToDate = single.Roi_Wef;
                intCalcAmt += Utilities.Calculate_Interest(amt, roi, (int)(tmpToDate - FromDate).TotalDays);
                roi = single.Pi;
                FromDate = single.Roi_Wef;
            }
            intCalcAmt += Utilities.Calculate_Interest(amt, roi, (int)(ToDate - FromDate).TotalDays);
            return intCalcAmt;
        }
        #endregion 

    }
}
