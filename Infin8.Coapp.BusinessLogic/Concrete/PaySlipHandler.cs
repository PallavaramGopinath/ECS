using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Infin8.Coapp.BusinessLogic
{

    public class PaySlipHandler : IPaySlipHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public PaySlipHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddPaySlipAsync(Pay_Slip paySlip)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PaySlip.AddPaySlipAsync(paySlip);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Pay slip not saved");
            }
            return result;
        }
        public async Task<bool> UpdatePaySlipForPayment(decimal empId, decimal payId, decimal vocId, DateTime trnDate)
        {
            return await _unitOfWork.PaySlip.UpdatePaySlipForPayment(empId, payId, vocId, trnDate);
        }
        public async Task<bool> EditPaySlipAsync(Pay_Slip paySlip)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PaySlip.EditPaySlipAsync(paySlip);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Pay slip not deleted");
            }
            return result;
        }

        public async Task<DtoPaySlip> DeletePaySlip(DtoPaySlip paySlip)
        {
            try
            {
                Pay_Slip slip = await _unitOfWork.PaySlip.GetPaySlipByMemId(paySlip.Pay_Id, paySlip.Employee_Id, paySlip.BrCode!);
                if (slip.Pmt == true)
                {
                    paySlip.ErrorMessage = "Pay slip already paid, cannot delete.";
                    paySlip.IsError = true;
                }
                else
                {
                    _unitOfWork.BeginTransaction();
                    slip.Pay_Delete = true;
                    var result = await _unitOfWork.PaySlip.EditPaySlipAsync(slip);
                    if (!result)
                    {
                        paySlip.ErrorMessage = "Error in deleting pay slip.";
                        paySlip.IsError = true;
                    }
                    Pay_Att att = new();
                    var attResult = await _unitOfWork.PayAttance.GetPayAttanceByEmpId(paySlip.Employee_Id, paySlip.Pay_Id, paySlip.BrCode!);
                    if (attResult != null) att = attResult;
                    if (att.Mem_Id == 0)
                    {
                        paySlip.ErrorMessage = "Error in fetching attendance details.";
                        paySlip.IsError = true;
                        return paySlip;
                    }
                    att.Att_Delete = true;
                    var attList = await _unitOfWork.PayAttance.EditPayAttanceAsync(att);
                    if (!attList)
                    {
                        paySlip.ErrorMessage = "Error in deleting attendance details.";
                        paySlip.IsError = true;
                        return paySlip;
                    }
                    List<Pay_Slip_Trn> slipTrnList = await _unitOfWork.PaySlipTrn.GetPaySlipTrnByMemId(paySlip.Pay_Id, paySlip.Employee_Id, paySlip.BrCode!);
                    if (slipTrnList != null && slipTrnList.Any())
                    {
                        foreach (var trn in slipTrnList)
                        {
                            trn.PayTr_Delete = true;
                            var trnResult = await _unitOfWork.PaySlipTrn.EditPaySlipTrnAsync(trn);
                            if (!trnResult)
                            {
                                paySlip.ErrorMessage = "Error in deleting pay slip transactions.";
                                paySlip.IsError = true;
                                return paySlip;
                            }
                        }
                    }
                    _unitOfWork.CommitTransaction();
                    await _unitOfWork.CompleteAsync();
                }
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                paySlip.ErrorMessage = "Error in deleting pay slip.";
                paySlip.IsError = true;
                Console.WriteLine(ex.Message);
            }
            return paySlip;
        }

        public async Task<List<DtoEmployeeLastPayInfo>> GetEmployeeLastPayInfo(string brCode)
        {
            return await _unitOfWork.PaySlip.GetEmployeeLastPayInfo(brCode);
        }

        public async Task<List<DtoPayComponentAssignments>> GetPayComponentAssignmentsByEmployeeId(decimal empId, string brCode)
        {
            return await _unitOfWork.PaySlip.GetPayComponentAssignmentsByEmployeeId(empId, brCode);
        }
        public async Task<bool> Find_PaySlipInit(int payMonth, int payYear, string payDes, string brCode)
        {
            return await _unitOfWork.PaySlip.Find_PaySlipInit(payMonth, payYear, payDes, brCode);
        }
        public async Task<bool> IsPreviousPaySlipInitialised(int payMonth, int payYear, string payDes, string brCode)
        {

            return await _unitOfWork.PaySlip.IsPreviousPaySlipInitialised(payMonth, payYear, payDes, brCode);
        }

        public async Task<bool> IsPaySlipGenerated(decimal payId, decimal empId, string brCode)
        {
            return await _unitOfWork.PaySlip.IsPaySlipGenerated(payId, empId, brCode);
        }

        public async Task<DtoPaySlip> CalculatePaySlip(DtoPaySlip paySlip)
        {
            decimal payId = 0;
            double eligibleBPForDA = 0, PF = 0, DA = 0;
            DateTime wef = new DateTime(paySlip.Pay_Year, paySlip.Pay_Month, 1);
            List<DtoPayComponentAssignments> componentAssignmentsList = new();
            try
            {

                /// Step 1: Assign to componentAssignments;


                /// Step 2: Find Payslip init, if so get PayId
                var payInitResult = await _unitOfWork.PaySlip.Find_PaySlipInit(paySlip.Pay_Month, paySlip.Pay_Year, paySlip.PayDescription!, paySlip.BrCode!);
                if (payInitResult)
                {
                    //paySlip.ErrorMessage = "Pay Slip already initialised for the month of " +
                    //   System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(paySlip.Pay_Month) + " " + paySlip.Pay_Year.ToString();
                    //paySlip.IsError = true;
                    //return paySlip;
                    payId = await _unitOfWork.PaySlip.GetPaySlipId(paySlip.Pay_Month, paySlip.Pay_Year, "P", paySlip.BrCode!);
                    paySlip.Pay_Id = payId;
                }

                /// Step 3: Is Previous Pay Slip Initialised
                var prevPayInitResult = await _unitOfWork.PaySlip.IsPreviousPaySlipInitialised(paySlip.Pay_Month, paySlip.Pay_Year, paySlip.PayDescription!, paySlip.BrCode!);
                if (!prevPayInitResult)
                {
                    paySlip.ErrorMessage += "Previous month pay slip not initialised. Please initialise previous month pay slip first.";
                    paySlip.IsError = true;
                    return paySlip;
                }

                /// Step 4: Get DA Template
                var daTemplateResult = await _unitOfWork.PayDATemplate.GetPayDATemplate(wef, paySlip.PayDescription!, paySlip.BrCode!);
                if (daTemplateResult == null)
                {
                    paySlip.ErrorMessage += "DA Template not found for the month of " +
                        System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(paySlip.Pay_Month) + " " + paySlip.Pay_Year.ToString();
                    return paySlip;
                }
                Pay_DA_Template daTemplate = daTemplateResult;

                /// Step 5: Get Basic pay, pp, grade pay
                eligibleBPForDA = paySlip.ComponentAssignments!.Where(x => x.Is_DA_Applicable == true).Sum(x => x.Current_Value);

                /// Step 6: Get voluntary pf amount
                /// Step 7: Calculate DA
                DA = eligibleBPForDA * (daTemplate.DA_Percent / 100); //  (_dapercentage / 100);
                paySlip.DA_Id = daTemplate.DA_Id;

                var daComponent = paySlip.ComponentAssignments!.FirstOrDefault(x => x.Component_Code == "DA");
                if (daComponent != null)
                {
                    daComponent.Current_Value = DA;
                }
                paySlip.DA_Percentage = daTemplate.DA_Percent;

                /// Step 8: Calculate HRA Amount
                var hraComponent = paySlip.ComponentAssignments!.FirstOrDefault(x => x.Component_Code == "HRA");
                double HRA = 0;
                if ((hraComponent != null))
                {
                    if (hraComponent.Calculation_Method == "percentage")
                    {
                        double hraPercent = hraComponent.Percentage;
                        HRA = (eligibleBPForDA) * hraPercent / 100;
                        HRA = Math.Round(HRA, 2);
                        HRA = (int)(HRA + 0.5);
                        if (HRA > hraComponent.Maximum_Amount && hraComponent.Maximum_Amount > 0)
                        {
                            HRA = hraComponent.Maximum_Amount;
                        }
                        hraComponent.Current_Value = HRA;
                    }
                }

                /// Step 9: Calculate CCA Amount
                var ccaComponent = paySlip.ComponentAssignments!.FirstOrDefault(x => x.Component_Code == "CCA");
                double CCA = 0;
                if ((ccaComponent != null))
                {
                    if (ccaComponent.Calculation_Method == "percentage")
                    {
                        double ccaPercent = ccaComponent.Percentage;
                        CCA = (eligibleBPForDA) * ccaPercent / 100;
                        CCA = Math.Round(CCA, 2);
                        CCA = (int)(CCA + 0.5);
                        ccaComponent.Current_Value = CCA;
                        if (CCA > ccaComponent.Maximum_Amount && ccaComponent.Maximum_Amount > 0)
                        {
                            CCA = ccaComponent.Maximum_Amount;
                        }
                    }
                }

                /// Step 10: Calculate PF Amount
                var pfRoi = await _unitOfWork.PayPFRoiTemplate.GetPayPFRoiTemplateByDate(wef, paySlip.BrCode!);
                if (pfRoi == null)
                {
                    paySlip.ErrorMessage += "PF ROI Template not found for the month of " +
                        System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(paySlip.Pay_Month) + " " + paySlip.Pay_Year.ToString();
                    paySlip.IsError = true;
                    return paySlip;
                }
                PF = (eligibleBPForDA + DA) * pfRoi.Roi / 100;
                PF = Math.Round(PF, 2);
                PF = (int)(PF + 0.5);

                var pfComponent = paySlip.ComponentAssignments!.FirstOrDefault(x => x.Component_Code == "PF");
                if (daComponent != null)
                {
                    pfComponent!.Current_Value = PF;
                }
                paySlip.PF_Percentage = pfRoi.Roi;

                /// Step 11: Get standard deductions ( alreasy added in component assignments)
                /// Step 12: Get Loan deductions
                List<PayLoanBalanceVM> loanList = new();
                var payLoanList = await _unitOfWork.LoanTrn.GetPayLoanBalance(paySlip.Employee_Id, 5, wef, paySlip.BrCode!);
                if (payLoanList != null && payLoanList.Any())
                {
                    loanList = payLoanList.ToList();
                }
                paySlip.LoanList = loanList;

                /// Step 13: Get suspense due to items
                List<MemberTransactionVM> memTrnList = new();
                var memTrnListResult = await _unitOfWork.MemTrn.GetMemberTrnBalanceList(paySlip.Employee_Id, 5, paySlip.BrCode!);
                if (memTrnListResult != null && memTrnListResult.Any()) memTrnList = memTrnListResult.ToList();
                paySlip.SuspeneDueToList = memTrnList;

                /// Step 14: Calcualte LOP, HP, gross pay, total deductions and net pay
                paySlip = Utilities.Calculate_LOP_HP(paySlip);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return paySlip;
        }

        public async Task<DtoPaySlip> GeneratePaySlip(DtoPaySlip paySlip)
        {
            DateTime wef = new DateTime(paySlip.Pay_Year, paySlip.Pay_Month, 1);
            decimal payId = 0;
            double BP = 0, BPEarned = 0, PP = 0, PPEarned = 0, GradePay = 0, GradePayEarned = 0,
                DAPercent = 0, DAEarned = 0, DA = 0, PFAmt = 0, VPF = 0; ///, totalAllowances = 0,
                                                                         /// totalDeductions = 0, NetPay = 0;
            try
            {
                /// Step 1: Is Previous Pay Slip Initialised
                var prevPayInitResult = await _unitOfWork.PaySlip.IsPreviousPaySlipInitialised(paySlip.Pay_Month, paySlip.Pay_Year, paySlip.PayDescription!, paySlip.BrCode!);
                if (!prevPayInitResult)
                {
                    paySlip.ErrorMessage += "Previous month pay slip not initialised. Please initialise previous month pay slip first.";
                    paySlip.IsError = true;
                    return paySlip;
                }
                /// Step 1 : Verify previous pay slip initiated
                var payInitResult = await _unitOfWork.PaySlip.Find_PaySlipInit(paySlip.Pay_Month, paySlip.Pay_Year, paySlip.PayDescription!, paySlip.BrCode!);
                if (payInitResult)
                {
                    var isFind = await _unitOfWork.PaySlip.Find_PaySlipInit(paySlip.Pay_Month, paySlip.Pay_Year, paySlip.PayDescription!, paySlip.BrCode!);
                    if (isFind)
                    {
                        /// Step 2 : Get paySlipId
                        var payIdResult = await _unitOfWork.PaySlip.GetPaySlipId(paySlip.Pay_Month, paySlip.Pay_Year, paySlip.PayDescription!, paySlip.BrCode!);
                        if (payIdResult > 0)
                        {
                            payId = payIdResult;
                        }
                        else
                        {
                            paySlip.ErrorMessage += "Error in fetching Pay Id";
                            paySlip.IsError = true;
                            return paySlip;
                        }
                    }
                }
                /// Step 3 : Fin pay slip init
                var isGenerated = await _unitOfWork.PaySlip.IsPaySlipGenerated(payId, paySlip.Employee_Id, paySlip.BrCode!);
                if (isGenerated)
                {
                    paySlip.ErrorMessage += "Pay slip already generated.";
                    paySlip.IsError = true;
                    return paySlip;
                }

                /// Step 4 : Generate payslip
                Pay_Att payAtt = new Pay_Att()
                {
                    Pay_Id = payId,
                    Att_Id = 0,
                    Mem_Id = paySlip.Employee_Id,
                    Att_HQ = paySlip.HeadQuarters,
                    Att_CAMP = paySlip.Camp,
                    Att_HD = paySlip.Holiday,
                    Att_FH = paySlip.FestivalHoliday,
                    Att_CL = paySlip.CasualLeave,
                    Att_ML = paySlip.MedicalLeave,
                    Att_EL = paySlip.EarnedLeave,
                    Att_LOP = paySlip.LossOfPay,
                    Att_TD = paySlip.TotalDaysInMonth,
                    Att_Delete = false,
                    Usr_Id = paySlip.Usr_Id,
                    Yr_Id = paySlip.Yr_Id,
                    BrCode = paySlip.BrCode,
                };

                List<Pay_Slip_Trn> paySlipTrnList = new();
                foreach (var slip in paySlip.ComponentAssignments!)
                {
                    Pay_Slip_Trn slipTrn = new()
                    {
                        Pay_Id = payId,
                        Mem_Id = paySlip.Employee_Id,
                        All_Id = slip.Component_Type == 1 ? slip.Component_Id : 0,
                        Ded_Id = slip.Component_Type == 2 ? slip.Component_Id : 0,
                        Loan_Id = 0,
                        Led_Id = slip.Led_Id,
                        All_Type = slip.Component_Type == 1 ? 1 : 0,
                        Ded_Type = slip.Component_Type == 2 ? 2 : 0,
                        Pay_Component_Type = slip.Component_Type,
                        All_Ded_Amt = 0,
                        Allowance_Amt = slip.Component_Type == 1 ? slip.Current_Value : 0,
                        Deduction_Amt = slip.Component_Type == 2 ? slip.Current_Value : 0,
                        PayTr_Delete = false,
                        Usr_Id = paySlip.Usr_Id,
                        Yr_Id = paySlip.Yr_Id,
                        BrCode = paySlip.BrCode,
                        Voc_Status = "V",
                        Component_Id = slip.Component_Id
                    };
                    paySlipTrnList.Add(slipTrn);
                }

                List<Pay_Slip_Loan_Trn> loanTrnList = new();
                if (paySlip.LoanList != null && paySlip.LoanList.Any())
                {
                    foreach (var loan in paySlip.LoanList!)
                    {
                        Pay_Slip_Loan_Trn loanTrn = new()
                        {
                            Pay_Id = payId,
                            Loan_Id = loan.Loan_Id,
                            Rpt_Date = wef,
                            Amt_Coll = loan.TotalRecovery,
                            Prl_Schedule = loan.PrlDemand,
                            Prl_Coll = loan.PrlRecovery,
                            Int_Calc_Upto = loan.IntCalcDate,
                            Int_Calc_Amt = loan.IntCalc,
                            Int_Coll = loan.IntRecovery,
                            LoanTr_Delete = false,
                            Usr_Id = paySlip.Usr_Id,
                            Yr_Id = paySlip.Yr_Id,
                            BrCode = paySlip.BrCode,
                            Voc_Status = "V"
                        };
                        loanTrnList.Add(loanTrn);
                        Pay_Slip_Trn slipTrn = new()
                        {
                            Pay_Id = payId,
                            Mem_Id = paySlip.Employee_Id,
                            All_Id = 0,
                            Ded_Id = 0,
                            Loan_Id = loan.Loan_Id,
                            Led_Id = 0,
                            All_Type = 0,
                            Ded_Type = 3,
                            Pay_Component_Type = 3,
                            All_Ded_Amt = 0,
                            Allowance_Amt = 0,
                            Deduction_Amt = loan.TotalRecovery,
                            PayTr_Delete = false,
                            Usr_Id = paySlip.Usr_Id,
                            Yr_Id = paySlip.Yr_Id,
                            BrCode = paySlip.BrCode,
                            Voc_Status = "V"
                        };
                        paySlipTrnList.Add(slipTrn);
                    }
                }

                if (paySlip.SuspeneDueToList != null && paySlip.SuspeneDueToList.Any())
                {
                    foreach (var memTrn in paySlip.SuspeneDueToList)
                    {
                        Pay_Slip_Trn slipTrn = new()
                        {
                            Pay_Id = payId,
                            Mem_Id = paySlip.Employee_Id,
                            All_Id = 0,
                            Ded_Id = 0,
                            Loan_Id = 0,
                            Led_Id = memTrn.LedId,
                            All_Type = 0,
                            Ded_Type = 5,
                            Pay_Component_Type = 5,
                            All_Ded_Amt = 0,
                            Allowance_Amt = 0,
                            Deduction_Amt = memTrn.Balance,
                            PayTr_Delete = false,
                            Usr_Id = paySlip.Usr_Id,
                            Yr_Id = paySlip.Yr_Id,
                            BrCode = paySlip.BrCode,
                            Voc_Status = "V"
                        };
                        paySlipTrnList.Add(slipTrn);
                    }
                }
                var bpComponent = paySlip.ComponentAssignments!.FirstOrDefault(x => x.Component_Code == "BP");
                if (bpComponent != null)
                {
                    BP = bpComponent.Current_Value;
                    BPEarned = bpComponent.Assigned_Value;
                }
                var ppComponent = paySlip.ComponentAssignments!.FirstOrDefault(x => x.Component_Code == "PP");
                if (ppComponent != null)
                {
                    PP = ppComponent.Current_Value;
                    PPEarned = ppComponent.Assigned_Value;
                }
                var gpComponent = paySlip.ComponentAssignments!.FirstOrDefault(x => x.Component_Code == "GP");
                if (gpComponent != null)
                {
                    GradePay = gpComponent.Current_Value;
                    GradePayEarned = gpComponent.Assigned_Value;
                }
                var daComponent = paySlip.ComponentAssignments!.FirstOrDefault(x => x.Component_Code == "DA");
                if (daComponent != null)
                {
                    DAPercent = paySlip.DA_Percentage;
                    DA = daComponent.Current_Value;
                    DAEarned = daComponent.Assigned_Value;
                }
                var pfComponent = paySlip.ComponentAssignments!.FirstOrDefault(x => x.Component_Code == "PF");
                if (pfComponent != null)
                {
                    PFAmt = pfComponent.Current_Value;
                }
                var voluntaryPfComponent = paySlip.ComponentAssignments!.FirstOrDefault(x => x.Component_Code == "VPF");
                if (voluntaryPfComponent != null)
                {
                    VPF = voluntaryPfComponent.Current_Value;
                }
                Pay_Slip pay = new()
                {
                    PaySlip_Id = 0,
                    Pay_Id = payId,
                    Mem_Id = paySlip.Employee_Id,
                    Pay_Basic = BP,
                    Pay_Basic_Earned = BPEarned,
                    Pay_PP = PP,
                    Pay_PP_Earned = PPEarned,
                    Pay_GradePay = GradePay,
                    Pay_GradePay_Earned = GradePayEarned,
                    Pay_DA_Percent = DAPercent,
                    Pay_DA_Earned = DAEarned,
                    Pay_SLS = 0,
                    Pay_ExGratia = 0,
                    Pay_Bonus = 0,
                    Pay_PF = PFAmt,
                    Pay_VPF = VPF,
                    Pay_Tot_Allowance = paySlip.GrossPay,
                    Pay_Tot_Deductions = paySlip.TotalDeductions,
                    Pay_Net = paySlip.NetPay,
                    Usr_Id = paySlip.Usr_Id,
                    Yr_Id = paySlip.Yr_Id,
                    Pmt = false,
                    Voc_Id = 0,
                    Pay_Delete = false,
                    Pay_SLS_Days = 0,
                    BrCode = paySlip.BrCode,
                    Voc_Status = "V"
                };

                Pay_Init payInit = new()
                {
                    Pay_Id = payId,
                    Pay_Month = paySlip.Pay_Month,
                    Pay_Year = paySlip.Pay_Year,
                    Pay_Delete = false,
                    Usr_Id = paySlip.Usr_Id,
                    Yr_Id = paySlip.Yr_Id,
                    Pay_Des = "P",
                    From_Date = null,
                    To_Date = null,
                    DA_Id = paySlip.DA_Id,
                    BrCode = paySlip.BrCode,
                };
                _unitOfWork.BeginTransaction();
                if (payId == 0)
                {
                    var InitResult = await _unitOfWork.PayInit.AddPayInitAsync(payInit);
                    if (InitResult == null)
                    {
                        paySlip.ErrorMessage += "Error in saving Pay Init data.";
                        paySlip.IsError = true;
                        _unitOfWork.RollBack();
                        return paySlip;
                    }
                    else
                        payId = InitResult.Pay_Id;
                }
                if (payAtt != null)
                {
                    payAtt.Pay_Id = payId;
                    var attResult = await _unitOfWork.PayAttance.AddPayAttanceAsync(payAtt);
                    if (!attResult)
                    {
                        paySlip.ErrorMessage += "Error in saving Pay Attendance data.";
                        paySlip.IsError = true;
                        _unitOfWork.RollBack();
                        return paySlip;
                    }
                }

                if (paySlipTrnList != null && paySlipTrnList.Any())
                {
                    foreach (var trn in paySlipTrnList)
                    {
                        trn.Pay_Id = payId;
                        var paySlipTrnResult = await _unitOfWork.PaySlipTrn.AddPaySlipTrnAsync(trn);
                        if (!paySlipTrnResult)
                        {
                            paySlip.ErrorMessage += "Error in saving Pay Slip Transaction data.";
                            paySlip.IsError = true;
                            _unitOfWork.RollBack();
                            return paySlip;
                        }
                    }
                }
                foreach (var loan in loanTrnList)
                {
                    loan.Pay_Id = payId;
                    var loanTrnResult = await _unitOfWork.PaySlipLoanTrn.AddPaySlipLoanTrnAsync(loan);
                    if (!loanTrnResult)
                    {
                        paySlip.ErrorMessage += "Error in saving Pay Loan Transaction data.";
                        paySlip.IsError = true;
                        _unitOfWork.RollBack();
                        return paySlip;
                    }
                }
                pay.Pay_Id = payId;
                var addPaySlipResult = await _unitOfWork.PaySlip.AddPaySlipAsync(pay);
                if (!addPaySlipResult)
                {
                    paySlip.ErrorMessage += "Error in saving Pay Slip data.";
                    paySlip.IsError = true;
                    _unitOfWork.RollBack();
                    return paySlip;
                }
                _unitOfWork.CommitTransaction();
            }
            catch (Exception ex)
            {
                paySlip.ErrorMessage += "Error in saving Pay Slip data.";
                paySlip.IsError = true;
                _unitOfWork.RollBack();
                Console.WriteLine(ex.Message);
            }
            return paySlip;
        }

        public Task<decimal> GetPaySlipId(int payMonth, int payYear, string payDes, string brCode)
        {
            return _unitOfWork.PaySlip.GetPaySlipId(payMonth, payYear, payDes, brCode);
        }

        public Task<DtoPaySlip> GetPaySlipById(decimal payId, decimal memId, string brCode)
        {
            return _unitOfWork.PaySlip.GetPaySlipById(payId, memId, brCode);
        }

        public async Task<List<DropdownItem>> GetPaySlipListForSalaryPayment(string payDescription, string brCode)
        {
            return await _unitOfWork.PaySlip.GetPaySlipListForSalaryPayment(payDescription, brCode);
        }

        public async Task<List<DropdownItem>> GetPaySlipListForSalaryPaymentByEmpId(decimal EmpId, string payDescription, string brCode)
        {
            return await _unitOfWork.PaySlip.GetPaySlipListForSalaryPaymentByEmpId(EmpId, payDescription, brCode);
        }
        public async Task<List<DropdownItem>> GetEmploeeNamesForSalaryPayment(decimal payId, string brCode)
        {
            return await _unitOfWork.PaySlip.GetEmploeeNamesForSalaryPayment(payId, brCode);
        }
        public async Task<DropdownItem> GetEmploeeNameForSalaryPayment(decimal empId, decimal payId, string brCode)
        {
            return await _unitOfWork.PaySlip.GetEmploeeNameForSalaryPayment(empId, payId, brCode);
        }
        public async Task<List<Pay_Slip>> GetPaySlipForPayment(List<decimal> empIdList, decimal payId, string brCode)
        {
            return await _unitOfWork.PaySlip.GetPaySlipForPayment(empIdList, payId, brCode);
        }

        public async Task<DtoPayDAArrears> CalculateDAArrears(DtoPayDAArrears Arrears)
        {
            List<PaySlipDAArrearsVM> daArrearsList = new();
            int NoOfDays = 0;
            decimal PayDAArrearsId = 0;
            decimal PayId = 0;
            double LLP = 0;
            double HP = 0;

            //int DAId = 0;
            double DAArrears = 0;
            double DAArrearsOnSLS = 0;
            double DAArrearsTotal = 0;
            double PFDeductions = 0;

            double BasicPay = 0;
            double TotalBasicPay = 0;
            double PaySLS = 0;
            double TotalPaySLS = 0;
            double PaySLSDays = 0;
            DateTime tmpFromDate;
            string errorMessage = "";
            try
            {
                PayDAArrearsId = await _unitOfWork.PayInit.GetPaySlipForDAArrears(Arrears.DAFrom_Date, Arrears.DATo_Date, "D", Arrears.BrCode!);


                if (PayDAArrearsId > 0)
                {
                    foreach (var emp in Arrears.EmployeeList!)
                    {
                        if (await _unitOfWork.PaySlip.IsPaySlipGenerated(PayDAArrearsId, emp.Mem_Id, Arrears.BrCode!))
                        {
                            daArrearsList = null;
                            errorMessage += "Already DA arreas made for " + emp.MemberName + " ";
                        }
                    }
                    if (errorMessage.Length > 0)
                    {
                        Arrears.IsError = true;
                        Arrears.ErrorMessage = errorMessage;
                        return Arrears;
                    }
                }

                foreach (var emp in Arrears.EmployeeList!)
                {
                    PaySlipDAArrearsVM arrear = new PaySlipDAArrearsVM();
                    tmpFromDate = Arrears.DAFrom_Date;
                    TotalBasicPay = 0; TotalPaySLS = 0; DAArrearsTotal = 0; PFDeductions = 0;
                    do
                    {
                        /// get da arrears for salary
                        PayId = 0;
                        PayId = await GetPaySlipId(tmpFromDate.Month, tmpFromDate.Year, "P", Arrears.BrCode!);
                        if (errorMessage.Length > 0)
                        {
                            daArrearsList = null;
                            Arrears.IsError = true;
                            Arrears.ErrorMessage = errorMessage;
                            return Arrears;
                        }
                        Pay_Slip paySlip = await _unitOfWork.PaySlip.GetPaySlipByMemId(PayId, emp.Mem_Id, Arrears.BrCode!);
                        if (paySlip != null && paySlip.Mem_Id == 0)
                        {
                            daArrearsList = null;
                            Arrears.IsError = true;
                            Arrears.ErrorMessage = "Pay Slip for the month of " + tmpFromDate.Month + " " + tmpFromDate.Year + " not available";
                            return Arrears;
                        }
                        if (paySlip != null && paySlip.Mem_Id > 0)
                        {
                            BasicPay = paySlip.Pay_Basic + paySlip.Pay_PP + paySlip.Pay_GradePay;
                        }
                        Pay_Att payAtt = await _unitOfWork.PayAttance.GetPayAttanceByEmpId(emp.Mem_Id, PayId, Arrears.BrCode!);
                        if (payAtt == null && payAtt!.Mem_Id == 0)
                        {
                            daArrearsList = null;
                            Arrears.IsError = true;
                            Arrears.ErrorMessage = "Attance for the employee for the month of " + tmpFromDate.Month + " " + tmpFromDate.Year + " not available";
                            return Arrears;
                        }
                        if (payAtt != null)
                        {
                            LLP = payAtt.Att_LOP;
                            HP = payAtt.Att_ML;
                        }
                        DAArrears = Math.Round(BasicPay * (Arrears.DARate / 100), 2);
                        DAArrears = (int)(DAArrears + 0.5);
                        /// calculate da arrears for SLS
                        PayId = 0;
                        PayId = await GetPaySlipId(tmpFromDate.Month, tmpFromDate.Year, "S", Arrears.BrCode!);

                        if (PayId > 0)
                        {
                            Pay_Slip SLS = new Pay_Slip();
                            SLS = await _unitOfWork.PaySlip.GetPaySlipByMemId(PayId, emp.Mem_Id, Arrears.BrCode!);

                            if (SLS != null)
                            {
                                BasicPay = SLS.Pay_Basic + SLS.Pay_PP + SLS.Pay_GradePay;
                                PaySLS = BasicPay;
                                PaySLSDays = SLS.Pay_SLS_Days;
                            }

                            //DAArrears = Math.Round(BasicPay * (daRate / 100), 0);
                            if (PaySLS > 0)
                            {
                                DAArrearsOnSLS = PaySLS * (Arrears.DARate / 100);
                                if (PaySLSDays == 15)
                                {
                                    DAArrearsOnSLS = Math.Round(DAArrearsOnSLS / 2, 2);
                                    DAArrearsOnSLS = Convert.ToInt32(DAArrearsOnSLS + 0.5);
                                }
                            }
                            else
                                DAArrearsOnSLS = 0;
                        }
                        else
                        {
                            DAArrearsOnSLS = 0;
                        }

                        NoOfDays = Utilities.GetNoOfDaysInAMonth(tmpFromDate.Month, tmpFromDate.Year);
                        if (NoOfDays == 0)
                        {
                            daArrearsList = null;
                            Arrears.IsError = true;
                            Arrears.ErrorMessage = "Error in obtain no.of days in the month " + tmpFromDate.Month + " " + tmpFromDate.Year;
                            return Arrears;
                        }
                        if (LLP > 0)
                            DAArrears -= ((DAArrears / NoOfDays) * LLP);
                        if (HP > 0)
                            DAArrears -= ((DAArrears / NoOfDays) * HP);
                        DAArrears = Math.Round(DAArrears, 0);
                        DAArrearsOnSLS = Math.Round(DAArrearsOnSLS, 2);
                        DAArrearsOnSLS = (int)(DAArrearsOnSLS + 0.5);

                        var pfRoi = await _unitOfWork.PayPFRoiTemplate.GetPayPFRoiTemplateByDate(tmpFromDate, Arrears.BrCode!);
                        if (pfRoi == null && pfRoi!.Roi_Id == 0)
                        {
                            Arrears.ErrorMessage += "PF ROI Template not found for the month of " +
                                System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(tmpFromDate.Month) + " " + Arrears.Transaction_Date.Year.ToString();
                            Arrears.IsError = true;
                            return Arrears;
                        }
                        PFDeductions += (DAArrears + DAArrearsOnSLS) * pfRoi.Roi / 100;
                        PFDeductions = Math.Round(PFDeductions, 2);
                        PFDeductions = (int)(PFDeductions + 0.5);
                        DAArrearsTotal += DAArrears + DAArrearsOnSLS;
                        TotalBasicPay += BasicPay;
                        TotalPaySLS += PaySLS;

                        arrear.Mem_Id = emp.Mem_Id;
                        arrear.MemberName = emp.MemberName;
                        arrear.FromDate = tmpFromDate;
                        arrear.ToDate = Arrears.DATo_Date;
                        arrear.TotalBasicPay = TotalBasicPay;
                        arrear.SLS = TotalPaySLS;
                        arrear.LLP = LLP;
                        arrear.DAArrears = DAArrearsTotal;
                        arrear.PF = PFDeductions;
                        tmpFromDate = Utilities.AddMonths(tmpFromDate, 1);
                    } while (tmpFromDate <= Arrears.DATo_Date);

                    daArrearsList!.Add(arrear);
                }
                Arrears.DAArrearsList!.Clear();
                Arrears.DAArrearsList.AddRange(daArrearsList!);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Arrears;
        }

        public async Task<DtoPayDAArrears> GenerateDAArrears(DtoPayDAArrears Arrears)
        {
            decimal PayId = 0;
            decimal payId = 0;
            decimal daId = 0;
            Pay_Init payInit = new();
            Pay_DA_Template DATemplate = new Pay_DA_Template();
            List<Pay_Slip> paySlipList = new List<Pay_Slip>();
            try
            {
                PayId = await _unitOfWork.PayInit.GetPaySlipForDAArrears(Arrears.DAFrom_Date, Arrears.DATo_Date, "D", Arrears.BrCode!);
                if (PayId == 0)
                {
                    payInit.Pay_Id = 0;
                    payInit.Pay_Month = Arrears.Transaction_Date.Month;
                    payInit.Pay_Year = Arrears.Transaction_Date.Year;
                    payInit.Pay_Delete = false;
                    payInit.Usr_Id = Arrears.Created_By;
                    payInit.Yr_Id = Arrears.YrId;
                    payInit.From_Date = Arrears.DAFrom_Date.Date;
                    payInit.To_Date = Arrears.DATo_Date.Date;
                    payInit.DA_Id = 0;
                    payInit.Pay_Des = "D";
                    payInit.BrCode = Arrears.BrCode;
                    

                    DATemplate.DA_Id = 0;
                    DATemplate.Wef = Arrears.Transaction_Date.Date;
                    DATemplate.DA_Percent = Arrears.DARate;
                    DATemplate.Usr_Id = Arrears.Created_By;
                    DATemplate.Yr_Id = Arrears.YrId;
                    DATemplate.Arrears_From = Arrears.DAFrom_Date.Date;
                    DATemplate.Arrears_To = Arrears.DATo_Date.Date;
                    DATemplate.Status = "D";
                    DATemplate.BrCode = Arrears.BrCode;
                }
                foreach (var da in Arrears.DAArrearsList!)
                {
                    Pay_Slip slip = new()
                    {
                        PaySlip_Id = 0,
                        Pay_Id = payId,
                        Mem_Id = da.Mem_Id,
                        Pay_Basic = da.TotalBasicPay,
                        Pay_Basic_Earned = da.TotalBasicPay ,
                        Pay_PP = 0,
                        Pay_PP_Earned = 0,
                        Pay_GradePay_Earned = 0,
                        Pay_DA_Percent = Arrears.DARate,
                        Pay_DA_Earned = da.DAArrears,
                        Pay_SLS = 0,
                        Pay_ExGratia = 0,
                        Pay_Bonus = 0,
                        Pay_PF = da.PF,
                        Pay_Tot_Allowance = da.DAArrears,
                        Pay_Tot_Deductions = da.PF,
                        Pay_Net = da.NetDAArrears,
                        Usr_Id = Arrears.Created_By,
                        Yr_Id = Arrears.YrId,
                        Pmt = false,
                        Voc_Id = 0,
                        Pay_Delete = false,
                        Pay_SLS_Days = 0,
                        BrCode = Arrears.BrCode ,
                        Voc_Status = "V"
                    };
                    paySlipList.Add(slip);
                }
                _unitOfWork.BeginTransaction();
                var daResult = await _unitOfWork.PayDATemplate.AddPayDATemplateAsync(DATemplate);
                if (daResult == null)
                {
                    Arrears.ErrorMessage = "Error in adding da rate template";
                    Arrears.IsError = true;
                    return Arrears;
                }
                else
                {
                    daId = daResult.DA_Id;
                }
                if (payId == 0)
                {
                    payInit.DA_Id = daId;
                    var payInitResult = await _unitOfWork.PayInit.AddPayInitAsync(payInit);
                    if (payInitResult != null && payInitResult.Pay_Id > 0)
                    {
                        payId = payInitResult!.Pay_Id;
                    }
                }
                foreach (var slip in paySlipList)
                {
                    slip.Pay_Id = payId;
                    var slipResult = await _unitOfWork.PaySlip.AddPaySlipAsync(slip);
                    if(slipResult == false)
                    {
                        Arrears.IsError = true;
                        Arrears.ErrorMessage = "Error in adding of DA Arrear data";
                        return Arrears;
                    }
                }
                _unitOfWork.CommitTransaction();
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                Console.WriteLine(ex.Message);
                Arrears.IsError = true;
                Arrears.ErrorMessage = ex.Message;
            }
            return Arrears;
        }

        public async Task<List<DtoPayDAArrearsView>> GetPayDAArrearsViews(List<decimal> empIdList, decimal payId, string brCode)
        {
            return await _unitOfWork.PaySlip.GetPayDAArrearsViews(empIdList, payId, brCode);
        }

        public async Task<DtoPayPFData> GetPFBalance(decimal empId, DateTime AsOnDate, string brCode)
        {
            return await _unitOfWork.PaySlip.GetPFBalance(empId, AsOnDate,brCode);
        }

        #region SLS
        public async Task<List<DtoSLSComponent>> GetSLSData(decimal empId, string brCode)
        {
            return await _unitOfWork.PaySlip.GetSLSData(empId,brCode);
        }

        public async Task<bool> IsSLSAlreadyPaid(decimal empId, DateTime fromDate, DateTime toDate, string payDesc, string brCode)
        {
            return await _unitOfWork.PaySlip.IsSLSAlreadyPaid (empId, fromDate, toDate, payDesc,brCode);
        }
        #endregion 
    }
}
