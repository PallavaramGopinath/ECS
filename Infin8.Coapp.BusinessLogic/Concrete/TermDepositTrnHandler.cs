using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class TermDepositTrnHandler : ITermDepositTrnHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public TermDepositTrnHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddTermDepositTrnAsync(TermDeposit_Trn termDepositTrn)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.TermDepositTrn.AddTermDepositTrnAsync(termDepositTrn);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Term deposit transaction not saved");
            }
            return result;
        }
        public async Task<bool> AddTermDepositTrnListAsync(List<TermDeposit_Trn> termDepositTrnList)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.TermDepositTrn.AddTermDepositTrnListAsync(termDepositTrnList);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Term deposit transaction not saved");
            }
            return result;
        }
        public async Task<bool> EditTermDepositTrnAsync(TermDeposit_Trn termDepositTrn)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.TermDepositTrn.EditTermDepositTrnAsync(termDepositTrn);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while modifying the Term deposit transaction");
            }
            return result;
        }
        public async Task<List<FDDetailsVM>> GetFDPayableByTDIdsAsync(decimal[] fdNos, DateTime toDate, int accountId, string brCode)
        {
            bool isMonthEndCalc = false;
            double IntCalc = 0;
            double TotalIntCalc = 0;
            double FcRate = 0;
            double PreROI = 0;
            int NoOfMonths = 0;
            int NoOfDays = 0;
            DateTime IntToDate;
            DateTime IntNextDate;
            DateTime intCalcDate;
            List<FDDetailsVM> fdDetails = new List<FDDetailsVM>();
            UtilityHandler utilityHandler = new UtilityHandler();
            Map_General mapGeneral = new Map_General();
            try
            {
                Map_General map_General = new Map_General();
                map_General = await _unitOfWork.MapGeneral.GetMapGeneralAsync(brCode);
                isMonthEndCalc = map_General.IsFDIntCalcOnMonthBasis;
                fdDetails = await _unitOfWork.TermDepositTrn.GetFDPayableByTDIdsAsync(fdNos);
                mapGeneral = await _unitOfWork.MapGeneral.GetMapGeneralAsync(brCode);
                if (fdDetails.Count > 0)
                {
                    foreach (var single in fdDetails)
                    {
                        IntCalc = 0;
                        TotalIntCalc = 0;
                        single.FDAmountRefund = 0;
                        if (single.FDIntAlreadyCalculatedDate == null)
                        {
                            intCalcDate = single.FDValueDate;
                            IntToDate = single.FDValueDate;
                        }
                        else
                        {
                            intCalcDate = (DateTime)single.FDIntAlreadyCalculatedDate;
                            IntToDate = (DateTime)single.FDIntAlreadyCalculatedDate;
                        }
                        IntNextDate = intCalcDate;
                        if (single.FDMaturityDate <= toDate)
                            IntToDate = single.FDMaturityDate;
                        else
                            IntToDate = toDate;
                        NoOfMonths = utilityHandler.GetMonthsBetweenDates(IntNextDate.Date, IntToDate.Date);
                        //NoOfMonths = GeneralService.GetNoOfCompletedMonthsBetweenTwoDates(IntNextDate.Date, IntToDate.Date);
                        if (NoOfMonths < 0) NoOfMonths = 0;
                        //if (errorMessage.Length > 0)
                        //{
                        //    TotalIntCalc = 0;
                        //    goto FinalOutPut;
                        //}
                        NoOfDays = utilityHandler.GetNoOfDays(IntToDate, IntNextDate);
                        //NoOfDays = GeneralService.GetNoOfDays(IntToDate, IntNextDate);
                        if (accountId == 7) /// int payment
                        {
                            if (single.FDIntPayableFrequency == 0)  /// on maturity
                            {
                                if (toDate.Date >= single.FDMaturityDate.Date)    /// toDate is greater than Maturity Date
                                {
                                    IntCalc = single.FDMaturityAmount - single.FDAmount - single.FDIntAlreadyCalculated;
                                    TotalIntCalc = IntCalc;
                                }
                                else
                                {
                                    //MessageBox.Show("Interest due not fall in FD No " + single.FDNo, "Information");
                                    IntCalc = 0;
                                    TotalIntCalc = 0;
                                }
                                IntNextDate = single.FDMaturityDate;
                            }
                            if (single.FDIntPayableFrequency >= 1)  /// other than maturity
                            {
                                if (IntToDate >= single.FDMaturityDate)    /// toDate is greater than Maturity Date
                                {
                                    IntCalc = single.FDMaturityAmount - single.FDAmount - single.FDIntAlreadyCalculated;
                                    TotalIntCalc = IntCalc;
                                    IntNextDate = single.FDMaturityDate;
                                }
                                else
                                {
                                    if (isMonthEndCalc)     /// end of month calculation to be verified
                                    {
                                        int tmpNoOfDays = 0;
                                        int tmpNoOfMonths = 0;
                                        DateTime tmpNextDate;
                                        tmpNoOfMonths = utilityHandler.GetMonthsBetweenDates(IntNextDate.Date, IntToDate.Date);
                                        //tmpNoOfMonths = GeneralService.GetNoOfCompletedMonthsBetweenTwoDates(IntNextDate.Date, IntToDate.Date);
                                        tmpNextDate = utilityHandler.GetNextMonthForFD(intCalcDate, single.FDValueDate, tmpNoOfMonths);
                                        //tmpNextDate = GeneralService.Get_Next_Month_For_FD(intCalcDate, single.FDValueDate, tmpNoOfMonths);
                                        tmpNoOfDays = utilityHandler.GetNoOfDays(IntToDate, tmpNextDate);
                                        if (tmpNoOfDays > 0)
                                        {
                                            IntCalc = utilityHandler.Calculate_Interest(single.FDAmount, single.FDROI, tmpNoOfDays);
                                            //IntCalc = Utilities.Calculate_Interest(single.FDAmount, single.FDROI, tmpNoOfDays);
                                            TotalIntCalc += IntCalc;
                                        }

                                        for (int i = 1; i <= tmpNoOfMonths / single.FDIntPayableFrequency; i++)
                                        {
                                            IntCalc = utilityHandler.CalculateInterestForFixedDeposit(single.FDAmount, single.FDROI, single.FDIntPayableFrequency, single.FDIsDiscountRate);
                                            //IntCalc = CalcIntForFD(single.FDAmount, single.FDROI, single.FDIntPayableFrequency, single.FDIsDiscountRate);
                                            TotalIntCalc += IntCalc;
                                        }
                                        IntNextDate = IntToDate;
                                    }
                                    else
                                    {
                                        for (int i = 1; i <= NoOfMonths / single.FDIntPayableFrequency; i++)
                                        {
                                            if (toDate >= IntNextDate)
                                            {
                                                IntCalc = 0;
                                                IntCalc = utilityHandler.CalculateInterestForFixedDeposit(single.FDAmount, single.FDROI, single.FDIntPayableFrequency, single.FDIsDiscountRate);
                                                //IntCalc = CalcIntForFD(single.FDAmount, single.FDROI, single.FDIntPayableFrequency, single.FDIsDiscountRate);
                                                TotalIntCalc += IntCalc;
                                                IntNextDate = utilityHandler.GetNextMonthForFD(IntNextDate, single.FDValueDate, single.FDIntPayableFrequency);
                                            }
                                            //IntNextDate = TsisService.GeneralService.Get_Next_Month_For_FD(IntNextDate, single.FDValueDate, single.FDIntPayableFrequency);
                                        }
                                        //IntNextDate = IntToDate;
                                    }
                                }
                            }
                        }
                        else if (accountId == 8) /// refund 
                        {
                            FcRate = await _unitOfWork.TermDepositFCTemplate.GetTDForeClosureROIAsync(3, toDate);
                            //FcRate = TsisDataAccess.DatabaseTermDeposit.GetTDForeClosureROI(3, toDate);
                            NoOfDays = utilityHandler.GetNoOfDays(IntToDate, single.FDValueDate);
                            //NoOfDays = GeneralService.GetNoOfDays(IntToDate, single.FDValueDate);
                            NoOfMonths = utilityHandler.GetMonthsBetweenDates(single.FDValueDate.Date, IntToDate.Date);
                            //NoOfMonths = GeneralService.GetNoOfCompletedMonthsBetweenTwoDates(single.FDValueDate.Date, IntToDate.Date);
                            if (utilityHandler.GetNoOfDays(toDate.Date, single.FDMaturityDate.Date) < 0)  /// before maturity date
                            {
                                if (NoOfDays < 365)
                                {
                                    PreROI = await _unitOfWork.TermDepositROITemplate.GetROIForTermDepositAsync(single.FDValueDate, single.FDSchemeId, 0, NoOfDays, brCode);
                                    //PreROI = TsisDataAccess.DatabaseTermDeposit.GetROIFor_TermDeposit(single.FDValueDate, single.FDSchemeId, 0, NoOfDays, out errorMessage);
                                    //if (errorMessage.Length > 0)
                                    //{
                                    //    goto ErrorOuptPut;
                                    //}
                                    if (PreROI == 0)
                                    {
                                        goto ErrorOuptPut;
                                    }
                                    PreROI -= FcRate;
                                    IntCalc = utilityHandler.GetFDMaturityAmount(single.FDAmount, single.FDValueDate, IntToDate, 0, NoOfDays, PreROI, single.FDIntPayableFrequency, single.FDIsDiscountRate, 0);
                                    //IntCalc = GetFDMaturityAmount(single.FDAmount, single.FDValueDate, IntToDate, 0, NoOfDays, PreROI, single.FDIntPayableFrequency, single.FDIsDiscountRate, 0);
                                    if (IntCalc < 0) IntCalc = 0;
                                    IntNextDate = IntToDate;
                                    TotalIntCalc += IntCalc;
                                }
                                else   /// if Noofdays >=365
                                {
                                    PreROI = await _unitOfWork.TermDepositROITemplate.GetROIForTermDepositAsync(single.FDValueDate, single.FDSchemeId, NoOfMonths, 0, brCode);
                                    //PreROI = TsisDataAccess.DatabaseTermDeposit.GetROIFor_TermDeposit(single.FDValueDate, single.FDSchemeId, NoOfMonths, 0, out errorMessage);
                                    //if (errorMessage.Length > 0)
                                    if (PreROI == 0)
                                    {
                                        goto ErrorOuptPut;
                                    }
                                    PreROI -= FcRate;
                                    NoOfMonths = utilityHandler.GetMonthsBetweenDates(single.FDValueDate.Date, IntToDate.Date);
                                    //NoOfMonths = GeneralService.GetNoOfCompletedMonthsBetweenTwoDates(single.FDValueDate.Date, IntToDate.Date);
                                    IntNextDate = utilityHandler.AddMonthsForFD(single.FDValueDate.Date, NoOfMonths);
                                    //IntNextDate = GeneralService.AddMonths_For_FD(single.FDValueDate, NoOfMonths);

                                    NoOfDays = utilityHandler.GetNoOfDays(IntToDate, IntNextDate);
                                    //NoOfDays = GeneralService.GetNoOfDays(IntToDate, IntNextDate);
                                    IntCalc = utilityHandler.GetFDMaturityAmount(single.FDAmount, single.FDValueDate, IntToDate, NoOfMonths, 0, PreROI, single.FDIntPayableFrequency, single.FDIsDiscountRate, 0);
                                    //IntCalc = GetFDMaturityAmount(single.FDAmount, single.FDValueDate, IntToDate, NoOfMonths, 0, PreROI, single.FDIntPayableFrequency, single.FDIsDiscountRate, 0);
                                    TotalIntCalc += IntCalc;
                                    IntCalc = 0;
                                    IntCalc = utilityHandler.GetFDMaturityAmount(single.FDAmount, single.FDValueDate, IntToDate, 0, NoOfDays, PreROI, single.FDIntPayableFrequency, single.FDIsDiscountRate, single.FDCompoundFrequency);
                                    //IntCalc = GetFDMaturityAmount(single.FDAmount, single.FDValueDate, IntToDate, 0, NoOfDays, PreROI, single.FDIntPayableFrequency, single.FDIsDiscountRate, single.FDCompoundFrequency);
                                    TotalIntCalc += IntCalc;
                                }

                            }
                            else  /// after maturity date
                            {
                                PreROI = single.FDROI;
                                IntCalc = single.FDMaturityAmount - single.FDAmount - single.FDIntAlreadyCalculated;
                                TotalIntCalc = IntCalc;
                                IntNextDate = single.FDMaturityDate;
                            }
                            single.FDROIApplied = PreROI;
                            single.FDAmountRefund = single.FDAmount;
                        }
                        else if (accountId == 9) /// Renewal
                        {
                            if (utilityHandler.GetNoOfDays(toDate.Date, single.FDMaturityDate.Date) < 0)
                            //if (GeneralService.GetNoOfDays(toDate.Date, single.FDMaturityDate.Date) < 0)  /// before maturity date
                            {
                                IntCalc = 0;
                                TotalIntCalc = 0;
                                single.FDAmountRefund = 0;
                                //errorMessage = "Fixed Deposit " + single.FDNo + " not matured as on " + IntToDate + Environment.NewLine;
                            }
                            else
                            {
                                TotalIntCalc = single.FDMaturityAmount - single.FDAmount - single.FDIntAlreadyCalculated;
                                single.FDAmountRefund = single.FDAmount;
                                IntNextDate = single.FDMaturityDate;
                            }
                            single.FDROIApplied = single.FDROI;
                        }

                        goto FinalOutPut;

                    ErrorOuptPut:
                        single.FDIntCalculatedNow = 0;
                        single.FDIntCalculatedDateNow = null;

                    FinalOutPut:
                        TotalIntCalc = Math.Round(TotalIntCalc, 0, MidpointRounding.AwayFromZero);
                        single.FDIntCalculatedNow = TotalIntCalc;
                        single.FDIntCalculatedDateNow = IntNextDate;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching the Term deposit transaction details");
            }
            return fdDetails;
        }
        public async Task<List<DropdownItem>> GetTDNosByMemIdAsync(decimal memId, string tdSchemeType, string brCode)
        {
            return await _unitOfWork.TermDepositTrn.GetTDNosByMemIdAsync(memId, tdSchemeType, brCode);
        }

        public async Task<List<decimal>> GetFDIdListForDayEndCalculation(int day, string brCode)
        {
            return await _unitOfWork.TermDepositTrn.GetFDIdListForDayEndCalculation(day, brCode);
        }
        public async Task<List<DropdownItem>> GetTDNosByMemIdForRenewal(decimal memId, string tdSchemeType, DateTime trnDate, string brCode)
        {
            return await _unitOfWork.TermDepositTrn.GetTDNosByMemIdForRenewal(memId, tdSchemeType, trnDate, brCode);
        }
        public async Task<DtoNominee> GetNomineeForTermDeposit(decimal memId, string tdSchemeType, string brCode)
        {
            return await _unitOfWork.TermDepositTrn.GetNomineeForTermDeposit(memId, tdSchemeType, brCode);
        }

        public async Task<FDDetailsVM> GetFDDataByTDId(decimal tdId, string brCode)
        {
            return await _unitOfWork.TermDepositTrn.GetFDDataByTDId(tdId, brCode);
        }

        public async Task<DtoSecurityDepositData> GetSecurityDepositData(decimal empId, string brCode)
        {
            return await _unitOfWork.TermDepositTrn.GetSecurityDepositData(empId, brCode);
        }

        public async Task<SecurityDepositVM> CalculateSecurityDepositInterest(decimal empId, int schemeId, DateTime toDate, string brCode)
        {
            SecurityDepositVM securityDeposit = new();
            try
            {
                var Roi = await _unitOfWork.TermDepositROITemplate.GetSecurityDepositRoi(schemeId, toDate, brCode);
                var response = await _unitOfWork.TermDepositTrn.CalculateSecurityDepositInterest(empId,schemeId, brCode);
                if (response != null && response.Mem_Id > 0)
                {
                    securityDeposit = response;
                    securityDeposit.RateOfInterest = Roi;
                    //if (securityDeposit.InterestAppliedDate == null)
                    //{
                    //    securityDeposit.InterestAppliedDate = securityDeposit.ValueDate;
                    //}
                    int NoOfDays = (toDate - (DateTime)securityDeposit.InterestAppliedDate).Days;
                    if (NoOfDays > 0)
                    {
                        double intCalcAmount = Math.Round((securityDeposit.DepositAmount * securityDeposit.RateOfInterest * NoOfDays) / 36500, 2, MidpointRounding.AwayFromZero);
                        double intPayableAmount = intCalcAmount + securityDeposit.InterestCalculatedAmount - securityDeposit.InterestPaidAmount;
                        //securityDeposit.IntCalcAmount = Math.Round((securityDeposit.DepositAmount * securityDeposit.RateOfInterest * NoOfDays) / 36500, 2, MidpointRounding.AwayFromZero);
                        securityDeposit.CurrentIntCalcAmount = intCalcAmount;
                        securityDeposit.IntPayableAmount = intPayableAmount;
                        securityDeposit.CurrentIntCalcDate = toDate;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " Something went wrong! An error occurred while calculating the security deposit interest");
            }
            return securityDeposit;
        }
    }
}
