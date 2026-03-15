using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class EmpPFHandler : IEmpPFHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public EmpPFHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddEmployeePFAsync(Emp_Pf empPf)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.EmployeePF.AddEmployeePFAsync(empPf);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee PF details not saved");
            }
            return result;
        }

        public async Task<bool> EditEmployeePFAsync(Emp_Pf empPf)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.EmployeePF.EditEmployeePFAsync(empPf);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee PF details not deleted");
            }
            return result;
        }

        public async Task<DtoVoucher> CalculatePFInterestYearEnd(DateTime fromDate, DateTime toDate,
            decimal usrId, decimal Yrid, string brCode)
        {
            DtoVoucher voucherData = new();
            List<DtoVoucherTrn> dtoVoucherTrns = new();

            decimal vocId = 0;
            List<EmployeeMasterDto> empList = new();
            List<DtoEmpPf> pfList = new();
            List<Emp_Pf> pfList2 = new();
            List<Fin_Voucher_Trn> voucherList = new();
            double PFInterestYear = 0;
            double SPFInterestYear = 0;
            double PFBalance = 0, SPFBalance = 0, VPFBalance = 0;
            bool result = false;
            try
            {
                _unitOfWork.BeginTransaction();
                var empListResult = await _unitOfWork.EmployeeMaster.GetEmployeeMasterListAsync(brCode);
                if (empListResult != null && empListResult.Any())
                {
                    empList = empListResult.ToList();
                }

                var result1 = await _unitOfWork.EmployeePF.CalculatePFInterestYearEnd(empList, fromDate, toDate.AddDays(1), usrId, Yrid, brCode);
                if (result1 != null && result1.Any())
                {
                    pfList = result1.ToList();
                    PFInterestYear = pfList.Sum(x => x.epf_Interest); /// PFInterestYear;
                    SPFInterestYear = pfList.Sum(x => x.bpf_Interest);
                }
                DtoTransactionRptPmtNos rptPmtNo = new();
                rptPmtNo = _unitOfWork.TransactionsRepository.GetReceiptAndPaymentNo(0, 0, PFInterestYear, SPFInterestYear, false, Yrid);
                voucherData.Voc_Rpt_No = rptPmtNo.Voc_Rpt_No;
                voucherData.Voc_Pmt_No = rptPmtNo.Voc_Pmt_No;
                voucherData.Voc_Date = toDate;
                voucherData.Voc_Type = 59;
                voucherData.Voc_Narration = "Year end PF Interest Calculation";
                voucherData.brCode = brCode;



                Pay_Template template = new();
                template = await _unitOfWork.PayTemplate.GetPayTemplateAsync(brCode);


                /// Save Emp_Pf list
                var resultFromSave = await _unitOfWork.EmployeePF.SavePFInterestYearEnd(pfList);
                /// Save Fin_Voucher
                Fin_Voucher voc = Utility.GetModalObject.GetFinVoucherObject(vocId, "", 0, toDate, 59, "STRN", PFInterestYear, true,
                                rptPmtNo.Voc_Rpt_SlNo, rptPmtNo.Voc_Rpt_No, rptPmtNo.Voc_Rpt_Mode, rptPmtNo.Voc_Pmt_SlNo, rptPmtNo.Voc_Pmt_No, rptPmtNo.Voc_Pmt_Mode,
                                 usrId, Yrid, brCode, 0);
                (result, vocId) = await _unitOfWork.FinVoucher.AddFinVoucherAsync(voc);

                /// Save Fin_Voucher_Trn
                Fin_Voucher_Trn vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, template.Emp_PF_Led_Id, PFInterestYear + SPFInterestYear, 0, 2, "Year end PF Interest Credited", false, usrId, Yrid, "SPFI", "", 0, brCode, 0, 0, 0);
                voucherList.Add(vocTrn);

                Fin_Voucher_Trn vocTrn2 = Utility.GetModalObject.GetFinVoucherTrObject(vocId, template.Int_On_PF_Led_Id, 0, PFInterestYear, 2, "Year end PF Interest Paid", false, usrId, Yrid, "SPFI", "", 0, brCode, 0, 0, 0);
                voucherList.Add(vocTrn2);
                Fin_Voucher_Trn vocTrn3 = Utility.GetModalObject.GetFinVoucherTrObject(vocId, template.Banks_PF_Led_Id, 0, SPFInterestYear, 2, "Year end Society PF Interest Paid", false, usrId, Yrid, "SPFI", "", 0, brCode, 0, 0, 0);
                voucherList.Add(vocTrn3);
                result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(voucherList);

                foreach (var trn in voucherList)
                {
                    DtoVoucherTrn vTrn = new()
                    {
                        Voc_Trn_Type = trn.Voc_Trn_Type,
                        Led_Id = trn.Led_Id ,
                        Voc_Rpt = trn.Voc_Rpt ,
                        Voc_Pmt = trn.Voc_Pmt ,
                    };
                    dtoVoucherTrns.Add(vTrn);
                }
                voucherData.Transactions = dtoVoucherTrns;

                /// Save year end Emp_PF transactions
                foreach (var emp in empList)
                {
                    PFBalance = pfList
                    .Where(x => x.mem_id == emp.Mem_Id)
                    .OrderByDescending(x => x.PF_Id)
                    .Select(x => x.pf_balance)
                    .FirstOrDefault();
                    VPFBalance = pfList
                    .Where(x => x.mem_id == emp.Mem_Id)
                    .OrderByDescending(x => x.PF_Id)
                    .Select(x => x.vpf_balance)
                    .FirstOrDefault();
                    SPFBalance = pfList
                    .Where(x => x.mem_id == emp.Mem_Id)
                    .OrderByDescending(x => x.PF_Id)
                    .Select(x => x.bpf_balance)
                    .FirstOrDefault();

                    PFInterestYear = pfList.Where(x => x.mem_id == emp.Mem_Id).Sum(x => x.epf_Interest); /// PFInterestYear;
                    SPFInterestYear = pfList.Where(x => x.mem_id == emp.Mem_Id).Sum(x => x.bpf_Interest);
                    Emp_Pf pf = Utility.GetModalObject.GetEmpPFObject(0, emp.Mem_Id, toDate, PFInterestYear, 0, SPFInterestYear, PFInterestYear, SPFInterestYear, PFBalance, VPFBalance, SPFBalance, 0, 0, 0, PFInterestYear, SPFInterestYear, toDate.AddDays(1), false, false, toDate.AddDays(1), vocId, usrId, Yrid, 0, "OB", brCode);
                    pfList2.Add(pf);
                    Emp_Pf pf2 = Utility.GetModalObject.GetEmpPFObject(0, emp.Mem_Id, toDate.AddDays(1), 0, 0, 0, 0, 0, PFBalance, VPFBalance, SPFBalance, 0, 0, 0, 0, 0, toDate.AddDays(1), false, false, toDate.AddDays(1), vocId, usrId, Yrid, 0, "OB", brCode);
                    pfList2.Add(pf2);
                }

                result = await _unitOfWork.EmployeePF.AddEmployeePFListAsync(pfList2);
                _unitOfWork.Complete();
                _unitOfWork.CommitTransaction();

            }
            catch (Exception ex)
            {
                string error = ex.Message;
                result = false;
                voucherData = new();
                _unitOfWork.RollBack();
            }
            return voucherData;
        }
    }
}
