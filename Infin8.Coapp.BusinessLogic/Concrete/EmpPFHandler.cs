using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using System;
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

        public async Task<bool> CalculatePFInterestYearEnd(List<EmployeeMasterDto> empList, DateTime fromDate, DateTime toDate, 
            decimal usrId, decimal Yrid, string brCode)
        {
            decimal vocId = 0;
            List<DtoEmpPf> pfList = new();
            List<Fin_Voucher_Trn> voucherList = new();
            double PFInterestYear = 0;
            double SPFInterestYear = 0;
            bool result = false;
            try
            {
                
                var result1 = await _unitOfWork.EmployeePF.CalculatePFInterestYearEnd(empList, fromDate, toDate, usrId,Yrid,brCode);
                if(result1 != null && result1.Any())
                {
                    pfList = result1.ToList();
                    PFInterestYear = pfList.Sum(x => x.epf_Interest); /// PFInterestYear;
                    SPFInterestYear = pfList.Sum(x=>x.bpf_Interest);
                }
                DtoTransactionRptPmtNos rptPmtNo = new();
                rptPmtNo = _unitOfWork.TransactionsRepository.GetReceiptAndPaymentNo(0, 0, PFInterestYear , SPFInterestYear , false, Yrid);
                Pay_Template template = new();
                template = await _unitOfWork.PayTemplate.GetPayTemplateAsync(brCode);
                
                /// Save Emp_Pf list
                var resultFromSave = await _unitOfWork.EmployeePF.SavePFInterestYearEnd(pfList);
                /// Save Fin_Voucher
                Fin_Voucher voc = Utility.GetModalObject.GetFinVoucherObject(vocId, "", 0, toDate, 59, "STRN", PFInterestYear , true,
                                rptPmtNo.Voc_Rpt_SlNo, rptPmtNo.Voc_Rpt_No, rptPmtNo.Voc_Rpt_Mode, rptPmtNo.Voc_Pmt_SlNo, rptPmtNo.Voc_Pmt_No, rptPmtNo.Voc_Pmt_Mode,
                                 usrId , Yrid , brCode, 0);
                (result, vocId) = await _unitOfWork.FinVoucher.AddFinVoucherAsync(voc);

                /// Save Fin_Voucher_Trn
                
                Fin_Voucher_Trn  vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, template.Emp_PF_Led_Id, PFInterestYear + SPFInterestYear, 0, 2, "Year end PF Interest Credited", false,usrId , Yrid,"SPFI", "",0,brCode , 0, 0, 0);
                voucherList.Add(vocTrn);
                Fin_Voucher_Trn vocTrn2 = Utility.GetModalObject.GetFinVoucherTrObject(vocId, template.Int_On_PF_Led_Id ,0, PFInterestYear, 2, "Year end PF Interest Paid", false, usrId, Yrid, "SPFI", "", 0, brCode, 0, 0, 0);
                voucherList.Add(vocTrn2);
                Fin_Voucher_Trn vocTrn3 = Utility.GetModalObject.GetFinVoucherTrObject(vocId, template.Banks_PF_Led_Id , 0, SPFInterestYear , 2, "Year end Society PF Interest Paid", false, usrId, Yrid, "SPFI", "", 0, brCode, 0, 0, 0);
                voucherList.Add(vocTrn3);

                result = await  _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(voucherList);

                /// Save year end Emp_PF transactions
                
                foreach (var emp in empList )
                {
                    PFInterestYear = pfList.Where(x=> x.mem_id == emp.Mem_Id).Sum(x => x.epf_Interest); /// PFInterestYear;
                    SPFInterestYear = pfList.Where(x => x.mem_id == emp.Mem_Id).Sum(x => x.bpf_Interest);

                }


                //if (pfContribution.PFReceived > 0 || pfContribution.VPFReceived > 0)
                //{
                //    vocTrn = new();
                //    vocTrn = Utility.GetModalObject.GetFinVoucherTrObject(vocId, template.Emp_PF_Led_Id, pfContribution.PFReceived + pfContribution.VPFReceived, 0, trns.Cash_Or_Adjustment, Narration, false, Checked_By, pfContribution.YrId, Status, "", pfContribution.Employee_Id, pfContribution.BrCode!, 0, 0, 0);
                //    finVoucherTrns.Add(vocTrn);
                //}
                //result = await _unitOfWork.EmployeePF.AddEmployeePFAsync(empPF);
               
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                result = false;
            }
            return result;
        }
    }
}
