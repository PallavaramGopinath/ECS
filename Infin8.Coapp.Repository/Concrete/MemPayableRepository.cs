using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class MemPayableRepository : Repository<Mem_Payable>, IMemPayableRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public MemPayableRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddMemPayableAsync(Mem_Payable memPayable)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Mem_Payable.MaxAsync(x => x.Pble_Id);
                maxId++;
                memPayable.Pble_Id = maxId;
                await AddAsync(memPayable);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Dividend calculation data not saved");
            }
            return result;
        }

        public async Task<bool> EditMemPayableAsync(Mem_Payable memPayable)
        {
            bool result = false;
            try
            {
                memPayable.Pble_Delete = true;
                await EditAsync(memPayable);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Dividend calculation data not modified");
            }
            return result;
        }

        public async Task<bool> Calculate_Dividend(DtoDividendCalculation dividendCalculate)
        {
            bool result = false;
            int slno = 1;
            int noOfDays = 0;
            DateTime tmpFromDate = dividendCalculate.FromDate;
            DateTime tmpToDate = dividendCalculate.ToDate;
            double intCalc = 0;
            int closingBalance = 0;
            decimal PbleMasterId = 0;
            Mem_Payable_Master master = new();
            List<Mem_Payable> payableList = new();
            Mem_Payable payable = new();
            List<Mem_Trn> memTrnList = new();
            List<mem_master> memberList = new();
            try
            {
                /// Insert Mem Payable Master for Dividend
                PbleMasterId = await  CSISContext.Mem_Payable_Master.MaxAsync(x => x.PbleMaster_Id);
                PbleMasterId++;
                master.PbleMaster_Id = PbleMasterId;
                master.PbleType = dividendCalculate.PbleType;
                master.Led_Id = dividendCalculate.Ledger_Id;
                master.Calculate_Date = dividendCalculate.CalculatedDate;
                master.Transfered_Date = dividendCalculate.TransferedDate;
                master.FromDate = dividendCalculate.FromDate;
                master.ToDate = dividendCalculate.ToDate;
                master.ROI_Pble = dividendCalculate.Roi;
                master.ROI_Trnble = 0;
                master.Calc_YrId = dividendCalculate.YrId;
                master.Master_Delete = false;
                master.Master_Status = "C";
                master.Voc_Id = 0;
                master.Usr_Id = dividendCalculate.CreatedBy;
                master.Yr_Id = dividendCalculate.YrId;

                /// get members list
                var memRresult = CSISContext.mem_master.Where(x => x.membertype == 1 && x.memberdelete == false).OrderBy(x => x.mem_id).ToList();
                if (memRresult != null && memRresult.Any())
                {
                    memberList = memRresult.ToList();
                }
                foreach (var single in memberList)
                {
                    slno = 1;
                    noOfDays = 0;
                    tmpFromDate = dividendCalculate.FromDate;
                    tmpToDate = dividendCalculate.ToDate;
                    if (single.accountcloseddate != null)
                    {
                        if (dividendCalculate.ToDate > single.accountcloseddate)
                            tmpToDate = (DateTime)single.accountcloseddate;
                    }

                    //closingBalance = CSISContext.Mem_Trn
                    //    .Where(t => t.Mem_Id == single.mem_id
                    //             && t.Led_Id == dividendCalculate.Ledger_Id
                    //             && !t.MemTrn_Delete
                    //             && t.Trn_Date < tmpFromDate.Date)
                    //    .Sum(t.Rpt_Amt - t.Pmt_Amt);

                    var query = CSISContext.Mem_Trn
                    .Where(t => t.Mem_Id == single.mem_id
                             && t.Led_Id == dividendCalculate.Ledger_Id
                             && !t.MemTrn_Delete
                             && t.Trn_Date < tmpFromDate.Date);

                    closingBalance = (int)(query.Sum(t => t.Rpt_Amt) - query.Sum(t => t.Pmt_Amt));
                    if (closingBalance > 0)
                    {
                        payable.Pble_Id = 0;
                        payable.PbleMaster_Id = PbleMasterId;
                        payable.Mem_Id = single.mem_id;
                        payable.Trn_Date = dividendCalculate.FromDate;
                        payable.Receipt_Amount = 0;
                        payable.Payment_Amount = 0;
                        payable.Closing_Balance = closingBalance;
                        payable.NoOfDays = noOfDays;
                        payable.Interest_Amount = 0;
                        payable.Pble_SlNo = slno;
                        payable.Pble_Status = "C";
                        payable.Pble_Delete = false;
                        payable.Usr_Id = dividendCalculate.CreatedBy;
                        payable.Yr_Id = dividendCalculate.YrId;
                        payable.St_SlNo = 0;
                        payable.Interest_Paid = 0;
                        payable.Acc_Id = 0;
                        slno++;
                        payableList.Add(payable);
                    }
                    memTrnList = CSISContext.Mem_Trn
                        .Where(x => x.Mem_Id == single.mem_id
                        && x.MemTrn_Delete == false
                        && (x.Trn_Date >= dividendCalculate.FromDate
                        && x.Trn_Date <= dividendCalculate.ToDate))
                        .OrderBy(x => x.Trn_Date)
                        .ThenBy(x => x.Trn_SlNo)
                        .ToList();

                    if (memTrnList != null && memTrnList.Any())
                    {
                        foreach (var trn in memTrnList)
                        {
                            noOfDays = Utilities.GetNoOfDays((DateTime)trn.Trn_Date, tmpFromDate);
                            intCalc = Utilities.Calculate_Interest(closingBalance, dividendCalculate.Roi, noOfDays);
                            closingBalance += (int)trn.Rpt_Amt - (int)trn.Pmt_Amt;

                            if (closingBalance + trn.Rpt_Amt + trn.Pmt_Amt + intCalc > 0)
                            {
                                payable.Pble_Id = 0;
                                payable.PbleMaster_Id = PbleMasterId;
                                payable.Mem_Id = trn.Mem_Id;
                                payable.Trn_Date = trn.Trn_Date;
                                payable.Receipt_Amount = (int)trn.Rpt_Amt;
                                payable.Payment_Amount = (int)trn.Pmt_Amt;
                                payable.Closing_Balance = closingBalance;
                                payable.NoOfDays = noOfDays;
                                payable.Interest_Amount = (int)intCalc;
                                payable.Pble_SlNo = slno;
                                payable.Pble_Status = "C";
                                payable.Pble_Delete = false;
                                payable.Usr_Id = dividendCalculate.CreatedBy;
                                payable.Yr_Id = dividendCalculate.YrId;
                                payable.St_SlNo = 0;
                                payable.Interest_Paid = 0;
                                payable.Acc_Id = 0;
                                slno++;
                                payableList.Add(payable);
                            }
                            tmpFromDate = (DateTime)trn.Trn_Date;
                        }
                    }
                    noOfDays = Utilities.GetNoOfDays(tmpToDate.AddDays(1), tmpFromDate);
                    intCalc = Utilities.Calculate_Interest(closingBalance, dividendCalculate.Roi, noOfDays);
                    if (closingBalance > 0 || intCalc > 0)
                    {
                        payable.Pble_Id = 0;
                        payable.PbleMaster_Id = PbleMasterId;
                        payable.Mem_Id = single.mem_id;
                        payable.Trn_Date = tmpToDate;
                        payable.Receipt_Amount = 0;
                        payable.Payment_Amount = 0;
                        payable.Closing_Balance = closingBalance;
                        payable.NoOfDays = noOfDays;
                        payable.Interest_Amount = (int)intCalc;
                        payable.Pble_SlNo = slno;
                        payable.Pble_Status = "C";
                        payable.Pble_Delete = false;
                        payable.Usr_Id = dividendCalculate.CreatedBy;
                        payable.Yr_Id = dividendCalculate.YrId;
                        payable.St_SlNo = 0;
                        payable.Interest_Paid = 0;
                        payable.Acc_Id = 0;
                        slno++;
                        payableList.Add(payable);
                    }
                    result = true;
                }
            }
            catch (Exception)
            {

                result = false;
            }
            return result;
        }
    }
}
