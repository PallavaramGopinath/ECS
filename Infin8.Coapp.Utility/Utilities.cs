using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Infin8.Coapp.Utility
{
    public static class Utilities
    {
        // Regular Expression: Allows letters, numbers, ".", "_", and "-"
        private static readonly Regex UsernameRegex = new(@"^[a-zA-Z0-9._-]{5,30}$", RegexOptions.Compiled);
        // Regular Expression: Enforces strong password rules
        private static readonly Regex PasswordRegex = new(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,64}$", RegexOptions.Compiled); 

        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            return UsernameRegex.IsMatch(username);
        }

        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            //return PasswordRegex.IsMatch(password);
            return true;
        }

        public static int GetAgeBetweenTwoDates(DateTime birthDate, DateTime currentDate)
        {
            /// char gpt method
            //int age = currentDate.Year - birthDate.Year;

            //// Adjust age if the birthday has not occurred yet this year
            //if (currentDate < birthDate.AddYears(age))
            //{
            //    age--;
            //}
            //return age;

            return (currentDate.Year - birthDate.Year - 1) +
                (((currentDate.Month > birthDate.Month) ||
                ((currentDate.Month == birthDate.Month) && (currentDate.Day >= birthDate.Day))) ? 1 : 0);
        }

        public static int GetMonthsBetweenDates(DateTime startDate, DateTime endDate)
        {
            // Ensure startDate is earlier than endDate
            if (startDate > endDate)
            {
                var temp = startDate;
                startDate = endDate;
                endDate = temp;
            }

            int months = (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;

            // Adjust if the end day is earlier than the start day
            if (endDate.Day < startDate.Day)
            {
                months--;
            }

            return months;
        }

        public static int GetNoOfMonths(DateTime fromDate, DateTime toDate)
        {
            int retVal = 0;
            retVal = ((fromDate.Year - toDate.Year) * 12) + fromDate.Month - toDate.Month;
            return retVal;
        }

        public static int GetNoOfDays(DateTime endDate, DateTime startDate)
        {
            TimeSpan ts = endDate.Date - startDate.Date;
            return ts.Days;
        }

        public static int GetNoOfDaysInAMonth(int dMonth, int dYear)
        {
            int noOfDays = 0;
            try
            {
                DateTime selectDate = new DateTime(dYear, dMonth, 1);
                noOfDays = GetNoOfDays(AddMonths(selectDate, 1), selectDate);
            }
            catch (Exception ex)
            {
                noOfDays = 0;
            }
            return noOfDays;
        }
        public static double Calculate_Interest(double loanAmount, double ROI, int noOfDays)
        {
            double interest = 0;
            if (noOfDays <= 0) noOfDays = 0;
            interest = Math.Round((loanAmount * (ROI / 100)) / 365 * noOfDays, 0);
            return interest;
        }

        public static int GetAge(DateTime startDate, DateTime endDate)
        {
            return (endDate.Year - startDate.Year - 1) +
                (((endDate.Month > startDate.Month) ||
                ((endDate.Month == startDate.Month) && (endDate.Day >= startDate.Day))) ? 1 : 0);
        }

        public static DateTime GetTDMaturityDate(DateTime TDDate, int NoOfMonths, int NoOfDays)
        {
            DateTime maturityDate = TDDate;
            try
            {
                if (NoOfMonths > 0)
                    maturityDate = TDDate.AddMonths(NoOfMonths);
                else
                    maturityDate = TDDate.AddDays(NoOfDays);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while calculating  marutiry date");
            }
            return maturityDate;
        }

        public static double GetFDMaturityAmount(double Principal, DateTime FromDate, DateTime ToDate, int PeriodInMonths, int PeriodInDays, double ROI, int IntPayablePrd, bool IsDiscountRate, int CompoundFrequency)
        {
            //FDBalance fdbalance = new FDBalance();
            double MaturityAmount = 0;

            int NoOfCompoundDays;
            int NoOfCompoundPeriod;
            int NoOfMonths;
            int NoOfDays;
            DateTime FromDate2;
            double IntProvision1;
            double IntProvision2;
            //UtilityHandler utilityHandler = new UtilityHandler();
            switch (CompoundFrequency)
            {
                case 0:
                case 13:  /// compound not applicable
                    if (PeriodInMonths > 0)
                    {
                        if (IntPayablePrd == 1) /// monthly
                        {
                            if (IsDiscountRate)
                            {
                                MaturityAmount = Principal * (ROI / 100) / (12 + (ROI / 100));
                                MaturityAmount = (int)(MaturityAmount + 0.5) * PeriodInMonths;
                            }
                            else
                            {
                                MaturityAmount = Principal * (ROI / 100) / 12;
                                MaturityAmount = (int)(MaturityAmount + 0.5) * PeriodInMonths;
                            }
                        }
                        else if (IntPayablePrd == 3)
                        {
                            MaturityAmount = Math.Round((Principal * (ROI / 100)) / 4 * (PeriodInMonths / 3), 2);
                        }
                        else if (IntPayablePrd == 6)
                        {
                            MaturityAmount = Math.Round((Principal * (ROI / 100)) / 2 * (PeriodInMonths / 6), 2);
                        }
                        else if (IntPayablePrd == 12)
                        {
                            MaturityAmount = Math.Round((Principal * (ROI / 100)) * (PeriodInMonths / 12), 2);
                        }
                        else if (IntPayablePrd == 0)
                        {
                            MaturityAmount = Math.Round((Principal * (ROI / 100) / 12) * PeriodInMonths, 2);
                        }
                        MaturityAmount = (int)(MaturityAmount + 0.5);
                    }
                    if (PeriodInDays > 0)
                    {
                        MaturityAmount = Math.Round(Principal * PeriodInDays * (ROI / 36500), 2);
                        MaturityAmount = (int)(MaturityAmount + 0.5);
                    }
                    break;
                case 1: /// monthly
                    NoOfMonths = GetMonthsBetweenDates(FromDate, ToDate);
                    NoOfCompoundPeriod = Convert.ToInt32(NoOfMonths / 12) * 12;
                    NoOfMonths = NoOfCompoundPeriod;
                    FromDate2 = AddMonthsForFD(FromDate, NoOfMonths);
                    NoOfDays = GetNoOfDays(ToDate, FromDate2);
                    IntProvision1 = Math.Round(Principal * (Math.Pow((1 + ROI / 1200), NoOfCompoundPeriod)) - Principal, 2);
                    IntProvision2 = Math.Round((Principal + IntProvision1) * (ROI / 100) / 365 * NoOfDays, 2);
                    MaturityAmount = (int)((IntProvision1 + IntProvision2) + 0.5);
                    break;
                case 3: /// quarterly
                    if (PeriodInMonths > 0)
                    {
                        NoOfMonths = GetMonthsBetweenDates(FromDate, ToDate);
                        NoOfCompoundPeriod = (int)(NoOfMonths / 3);
                        NoOfMonths = NoOfCompoundPeriod * 3;
                        FromDate2 = AddMonthsForFD(FromDate, NoOfMonths);
                        NoOfDays = GetNoOfDays(ToDate, FromDate2);
                        IntProvision1 = Math.Round(Principal * (Math.Pow((1 + ROI / 400), NoOfCompoundPeriod)) - Principal, 2);
                        IntProvision2 = Math.Round((Principal + IntProvision1) * (ROI / 100) / 365 * NoOfDays, 2);
                        MaturityAmount = (int)((IntProvision1 + IntProvision2) + 0.5);
                    }
                    if (PeriodInDays > 0)
                    {
                        NoOfCompoundDays = (PeriodInDays / 365) * 4;
                        IntProvision1 = Math.Round(Principal * (Math.Pow((1 + ROI / 400), NoOfCompoundDays)) - Principal, 2);
                        MaturityAmount = (int)(IntProvision1 + 0.5);
                    }
                    break;
                case 6: /// Half yearly
                    if (PeriodInMonths > 0)
                    {
                        NoOfMonths = GetMonthsBetweenDates(FromDate, ToDate);
                        NoOfCompoundPeriod = (int)(NoOfMonths / 6);
                        NoOfMonths = NoOfCompoundPeriod * 6;
                        FromDate2 = AddMonthsForFD(FromDate, NoOfMonths);
                        NoOfDays = GetNoOfDays(ToDate, FromDate2);
                        IntProvision1 = Math.Round(Principal * (Math.Pow((1 + ROI / 200), NoOfCompoundPeriod)) - Principal, 2);
                        IntProvision2 = Math.Round((Principal + IntProvision1) * (ROI / 100) / 365 * NoOfDays, 2);
                        MaturityAmount = (int)((IntProvision1 + IntProvision2) + 0.5);
                    }
                    if (PeriodInDays > 0)
                    {
                        NoOfCompoundDays = (PeriodInDays / 365) * 2;
                        IntProvision1 = Math.Round(Principal * (Math.Pow((1 + ROI / 200), NoOfCompoundDays)) - Principal, 2);
                        MaturityAmount = (int)(IntProvision1 + 0.5);
                    }
                    break;
                case 12:    /// annual
                    if (PeriodInMonths > 0)
                    {
                        NoOfMonths = GetMonthsBetweenDates(FromDate, ToDate);
                        NoOfCompoundPeriod = (int)(NoOfMonths / 12);
                        NoOfMonths = NoOfCompoundPeriod * 12;
                        FromDate2 = AddMonthsForFD(FromDate, NoOfMonths);
                        NoOfDays = GetNoOfDays(ToDate, FromDate2);
                        IntProvision1 = Math.Round(Principal * (Math.Pow((1 + ROI / 100), NoOfCompoundPeriod)) - Principal, 2);
                        IntProvision2 = Math.Round((Principal + IntProvision1) * (ROI / 100) / 365 * NoOfDays, 2);
                        MaturityAmount = (int)((IntProvision1 + IntProvision2) + 0.5);
                    }
                    if (PeriodInDays > 0)
                    {
                        NoOfCompoundDays = (PeriodInDays / 365);
                        IntProvision1 = Math.Round(Principal * (Math.Pow((1 + ROI / 100), NoOfCompoundDays)) - Principal, 2);
                        MaturityAmount = (int)(IntProvision1 + 0.5);
                    }
                    break;
                default:
                    MaturityAmount = 0;
                    break;
            }
            return MaturityAmount + Principal ;
        }

        public static double GetRDMaturityAmount(double RDAmount, int Prd, double ROI, bool ApplyQuarterlyCompound)
        {
            double MaturityAmount = 0;
            //double Amount = 0;
            double Interest = 0;
            //int NoOfQuarters = 0;
            try
            {
                //if (TSISGlobalVariables.gSocietyId == 5)
                //{
                if (ApplyQuarterlyCompound == false)
                {
                    Interest = Math.Round((RDAmount * Prd * (Prd + 1) * ROI) / 2400, 2);
                    Interest = (int)(Interest + 0.5);
                    MaturityAmount = RDAmount * Prd + Interest;
                }
                if (ApplyQuarterlyCompound == true)
                {
                    /// RD Maturity amount adopted from one web site using javascript
                    double compoundOption = 4;
                    double irate = ROI / compoundOption;
                    double year = Prd / 12;
                    MaturityAmount = RDAmount * (Math.Pow((1 + irate / 100), (year) * compoundOption) - 1) / (1 - Math.Pow((1 + irate / 100), -compoundOption / 12));
                    MaturityAmount = Math.Round(MaturityAmount, 0);
                }
                //}
                //else
                //{
                //    double intcalc = 0;
                //    Interest = 0;
                //    Amount = 0;
                //    for (int i = 1; i <= Prd; i++)
                //    {
                //        Amount += RDAmount;
                //        intcalc = Math.Truncate(((Amount * (ROI / 100) / 12) + 0.5));
                //        Interest += intcalc;
                //    }
                //    MaturityAmount = (RDAmount * Prd) + Interest;
                //}
                //if (ApplyQuarterlyCompound == true)
                //{
                //    /// RD Maturity amount adopted from one web site using javascript
                //    double compoundOption = 4;
                //    double irate = ROI / compoundOption;
                //    double year = Prd / 12;
                //    MaturityAmount = RDAmount * (Math.Pow((1 + irate / 100), (year) * compoundOption) - 1) / (1 - Math.Pow((1 + irate / 100), -compoundOption / 12));
                //    MaturityAmount = Math.Round(MaturityAmount, 0);
                //}
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while calculating  RD maturity amount");
            }
            return MaturityAmount;
        }

        public static DateTime AddMonthsForFD(DateTime date, int noOfMonths)
        {
            if (date.Day != DateTime.DaysInMonth(date.Year, date.Month))    /// if not last day of the month
                return date.AddMonths(noOfMonths);
            else
                return date.AddDays(1).AddMonths(noOfMonths).AddDays(-1);
        }

        public static double CalculateInterestForFixedDeposit(double Amount, double Roi, int Months, bool IsDiscount)
        {
            double interest = 0;
            try
            {
                if (IsDiscount)
                {
                    interest = Math.Round(Amount * (Roi / 100) * (Months / (12 + Roi / 100)), 2);
                    interest = (int)(interest + 0.5);
                }
                else
                {
                    interest = Math.Round((Amount * (Roi / 100) / 12) * Months, 2);
                    interest = (int)(interest + 0.5);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while calculating  interest on fixed deposit");
            }
            double.TryParse(interest.ToString(), out double result);
            return result;

        }

        public static DateTime GetNextMonthForFD(DateTime runningDate, DateTime fdDate, int addMonths)
        {
            DateTime nextMonth = runningDate;
            bool IsDateisEndOfMonth = false;
            int month = fdDate.Month;
            try
            {
                nextMonth = nextMonth.AddMonths(addMonths);
                switch (month)
                {
                    case 1:
                    case 3:
                    case 5:
                    case 7:
                    case 8:
                    case 10:
                    case 12:
                        if (fdDate.Day == 31) IsDateisEndOfMonth = true;
                        break;
                    case 4:
                    case 6:
                    case 9:
                    case 11:
                        if (fdDate.Day == 30) IsDateisEndOfMonth = true;
                        break;
                }
                if (runningDate.Month == 2 && IsDateisEndOfMonth)
                    nextMonth = new DateTime(nextMonth.Year, nextMonth.Month, fdDate.Day);
                if (runningDate.Month != 2 && IsDateisEndOfMonth)
                {
                    switch (nextMonth.Month)
                    {
                        case 1:
                        case 3:
                        case 5:
                        case 7:
                        case 8:
                        case 10:
                        case 12:
                            if (fdDate.Day == 31) nextMonth = new DateTime(nextMonth.Year, nextMonth.Month, 31);
                            break;
                        case 4:
                        case 6:
                        case 9:
                        case 11:
                            if (fdDate.Day == 30) nextMonth = new DateTime(nextMonth.Year, nextMonth.Month, 30);
                            break;
                    }
                }
            }
            catch (Exception)
            {

            }
            return nextMonth;
        }

        public static DateTime GetFirstDueDateForPCARDB(DateTime sanctionedDate)
        {
            DateTime firstDueDate = sanctionedDate;
            try
            {
                switch (firstDueDate.Month)
                {

                    case 1:
                    case 2:
                        firstDueDate = new DateTime(firstDueDate.Year, 2, 1);
                        break;
                    case 3:
                    case 4:
                    case 5:
                        firstDueDate = new DateTime(firstDueDate.Year, 5, 1);
                        break;
                    case 6:
                    case 7:
                    case 8:
                        firstDueDate = new DateTime(firstDueDate.Year, 8, 1);
                        break;
                    case 9:
                    case 10:
                    case 11:
                        firstDueDate = new DateTime(firstDueDate.Year, 11, 1);
                        break;
                    case 12:
                        firstDueDate = new DateTime(firstDueDate.Year + 1, 2, 1);
                        break;
                }
            }
            catch (Exception)
            {

            }
            return firstDueDate;
        }

        public static double GetLoanInstalmentAmount(double loanAmount, int period, double ROI, int instalmentType, int DemFrequency)
        {
            double instAmt = 0;
            int prd = Convert.ToInt32(period / DemFrequency);
            switch (instalmentType)
            {
                case 1: /// fixed principal (monthly) with out int
                case 3: /// fixed principal with out interest
                case 4: /// fixed principal with interest
                    instAmt = Math.Round((loanAmount / prd), 0);
                    break;
                case 2: /// equated instalments (monthly)
                        /// Method 1 (not tallied)
                    //instAmt = PMT(ROI, period, loanAmount);
                    //instAmt = instAmt * DemFrequency;

                    /// Method 2 (not tallied)
                    //double loanAmount = 100000; // replace with your loan amount
                    //double interestRate = ROI / (12 * 100); // replace with your interest rate (e.g., 8% = 0.08)
                    //int loanTenureInMonths = period; // replace with your loan tenure in months

                    //double monthlyInterestRate = interestRate;
                    ////double emi = (loanAmount * monthlyInterestRate * Math.Pow(1 + monthlyInterestRate, loanTenureInMonths)) / (Math.Pow(1 + monthlyInterestRate, loanTenureInMonths) - 1);
                    ////emi = p * r * Math.Pow(1 + r, n) / (Math.Pow(1 + r, n) - 1);
                    //double emi = loanAmount * monthlyInterestRate * Math.Pow(1 + monthlyInterestRate, period) / (Math.Pow(1 + monthlyInterestRate, period) - 1);
                    //instAmt = emi * 12;

                    /// Method 3 (not tallied)
                    //A = R × ((1 - 1 / (1 + i)n) / i)
                    //i = r / t
                    //Where,
                    //A = Loan amount
                    //R = Loan repayment amount per payment period
                    //n = number of terms or payment periods; n = 36 for 3 year loan with monthly repayments
                    //i = periodic interest rate per payment period
                    //r = Interest rate in % per annum
                    //t = 365 for daily payment, 12 for monthly payment, 1 for yearly payment
                    //instAmt = loanAmount /    ((1 - 1 / Math.Pow(1 + ROI,period)) / ROI);

                    /// Method 4 (not tallied)
                    //double A = loanAmount ; // replace with your value of A
                    //double i = ROI/100 ; // replace with your value of i
                    //int n = period ; // replace with your value of n

                    //double numerator = i;
                    //double denominator = 1 - 1 / Math.Pow(1 + i, n);
                    //instAmt = A * (numerator / denominator);

                    /// Method 5 (correct value)
                    /// Annual Payment = (0.09(10000))
                    ///                   ------------
                    ///                   (1-(1+0.09)^-2)
                    ///                   
                    double numerator = ((ROI / 100) * loanAmount);
                    double denominator = (1 - (Math.Pow(1 + ROI / 100, (prd * -1))));
                    instAmt = numerator / denominator;
                    break;
            }
            instAmt = Math.Round(instAmt, 0, MidpointRounding.AwayFromZero);
            return instAmt;
        }

        public static List<LoanRepaymentScheduleVM> GetLoanRepaymentScheduleVMList(DateTime currentDate, double principal, double roi, int prd, int gracePeriod, double instalmentAmount, int instType, int demandFrequency, DateTime firstIntDueDate, DateTime firstPrlDueDate)
        {
            List<LoanRepaymentScheduleVM> repaymentSchedule = new List<LoanRepaymentScheduleVM>();
            DateTime duedate = firstIntDueDate.Date;
            DateTime fromDate = currentDate.Date;
            int slno = 1;
            double intCalc = 0, nonODPrl = 0, prlDem = 0, totIntCalc = 0, totPrlDem = 0, totAmt = 0;
            try
            {
                gracePeriod = GetMonthsBetweenDates(firstIntDueDate, firstPrlDueDate);


                nonODPrl = principal;
                if (gracePeriod > 0)
                {
                    for (int i = 1; i <= gracePeriod; i += demandFrequency)
                    {
                        intCalc = 0;
                        intCalc = Calculate_Interest(nonODPrl, roi, GetNoOfDays(duedate.AddMonths(demandFrequency), duedate));
                        repaymentSchedule.Add(new LoanRepaymentScheduleVM
                        {
                            SlNo = slno,
                            DemandDate = duedate,
                            PrincipalDemand = 0,
                            InterestDemand = intCalc,
                            NonODPrincipal = nonODPrl,
                        });
                        slno++;
                        totIntCalc += intCalc;
                        duedate = duedate.AddMonths(demandFrequency);
                    }
                }
                duedate = firstPrlDueDate.Date;
                for (int i = 1; i <= prd; i += demandFrequency)
                {
                    intCalc = 0;
                    intCalc = Calculate_Interest(nonODPrl, roi, GetNoOfDays(duedate, fromDate));
                    if (i == prd)
                    {
                        prlDem = nonODPrl;
                        nonODPrl = 0;
                    }
                    else
                    {
                        switch (instType)
                        {
                            case 1:
                                prlDem = instalmentAmount;
                                break;
                            case 2:
                                prlDem = instalmentAmount - intCalc;
                                break;
                        }
                        nonODPrl -= prlDem;
                    }
                    totPrlDem += prlDem;
                    if (totPrlDem > principal)
                    {
                        prlDem -= (totPrlDem - principal);
                        totPrlDem = principal;
                    }
                    if (i == prd - demandFrequency + 1)
                        prlDem += (principal - totPrlDem);
                    repaymentSchedule.Add(new LoanRepaymentScheduleVM
                    {
                        SlNo = slno,
                        DemandDate = duedate,
                        PrincipalDemand = prlDem,
                        InterestDemand = intCalc,
                        NonODPrincipal = nonODPrl,
                    });
                    slno++;
                    fromDate = duedate;
                    duedate = duedate.AddMonths(demandFrequency);
                    totIntCalc += intCalc;
                    totAmt += intCalc + prlDem;
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.Message + "\n error in prepare schedule", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return repaymentSchedule;
        }

        public static DateTime AddMonths(DateTime date, int addmonths)
        {
            var newDate = date.AddMonths(addmonths);

            // Check if the original date was the last day of the month
            if (date.Day == DateTime.DaysInMonth(date.Year, date.Month))
            {
                // If so, adjust the new date to the last day of the new month
                newDate = new DateTime(newDate.Year, newDate.Month, DateTime.DaysInMonth(newDate.Year, newDate.Month));
            }

            return newDate;
        }
        public static bool IsEndOfMonth(DateTime date)
        {
            return date.Day == DateTime.DaysInMonth(date.Year, date.Month);
        }

        public static bool IsYearEnd(DateTime date)
        {
            return date.Month == 12 && date.Day == 31;
        }

        public static DtoPaySlip  Calculate_LOP_HP(DtoPaySlip paySlip)
        {
            double bp = 0;
            double allowanceAmt = 0;
            int noOfDays = 0;
            int lopDays = 0;
            int mlDays = 0;
            double hp = 0;

            double grossPay = 0, totalDeductions = 0, netPay = 0, loanDeductions = 0, suspensAccountDeductions = 0;
            try
            {
                bp = paySlip.ComponentAssignments!.Where(x => x.Component_Code == "BP").Select(x => x.Current_Value).FirstOrDefault();
                //da = paySlip.ComponentAssignments!.Where(x => x.Component_Code == "DA").Select(x => x.Current_Value).FirstOrDefault();
                noOfDays = paySlip.TotalDays;
                lopDays = paySlip.LossOfPay;

                if (lopDays > 0)
                {
                    foreach (var list in paySlip.ComponentAssignments!.Where(x=> x.Component_Type == 1))
                    {
                        allowanceAmt = list.Current_Value;
                        allowanceAmt = allowanceAmt / noOfDays * lopDays;
                        allowanceAmt = (int)(allowanceAmt + 0.5);
                        list.Current_Value = allowanceAmt;
                    }
                }
                mlDays = paySlip.MedicalLeave;
                if(mlDays >0)
                {
                    var mlComponent = paySlip.ComponentAssignments!.FirstOrDefault(x => x.Component_Code == "DA");
                    hp = bp * noOfDays / mlDays;
                    hp = Math.Round(hp, 0);
                    mlComponent!.Current_Value = hp;
                }
                grossPay = paySlip.ComponentAssignments!.Where(x => x.Component_Type == 1).Sum(x => x.Current_Value);
                totalDeductions = paySlip.ComponentAssignments!.Where(x => x.Component_Type == 2).Sum(x => x.Current_Value);
                loanDeductions = paySlip.LoanList!.Sum(x=> x.PrlDemand + x.PrlOD + x.IntDemand);
                suspensAccountDeductions = paySlip.SuspeneDueToList!.Sum(x => x.Pmt - x.Rpt);
                totalDeductions += loanDeductions + suspensAccountDeductions;
                paySlip.GrossPay = grossPay;
                paySlip.TotalDeductions = totalDeductions;
                paySlip.NetPay = grossPay - totalDeductions;
            }
            catch (Exception ex)
            {
                Console.WriteLine (ex.Message, "error in calculating Lop/half pay");
            }
            return paySlip;
        }

        public static List<DtoLoanRepaymentSchedule> PrepareLoanRepaymentSchedule(DateTime fromDate,  double prlAmt, double roi, int prd, int gracePeriod, double instAmt, int instType, int demandFrequency,
            DateTime firstIntDueDate, DateTime firstPrlDueDate)
        {
            List<DtoLoanRepaymentSchedule> scheduleList = new();
            DtoLoanRepaymentSchedule schedule = new();
            DateTime duedate = firstIntDueDate;
            try
            {
                gracePeriod = Utilities.GetNoOfCompletedMonthsBetweenTwoDates(firstIntDueDate, firstPrlDueDate);
                //dgvSchedule.Rows.Clear();
                double loanOs = 0, nonODPrl = 0, prlDem = 0, intCalc = 0,  totIntCalc = 0, totPrlDem = 0, totAmt = 0;

                nonODPrl = prlAmt;
                loanOs = prlAmt;
                if (gracePeriod > 0)
                   
                {
                    for (int i = 1; i <= gracePeriod; i += demandFrequency)
                    {
                        schedule = new();
                        intCalc = 0;
                        intCalc = Utilities.Calculate_Interest(nonODPrl, roi, Utilities.GetNoOfDays(duedate.AddMonths(demandFrequency), duedate));
                        schedule.Due_Date = duedate;
                        schedule.Interest_Demand = intCalc;
                        schedule.Principal_Demand = 0;
                        schedule.Total_Demand = intCalc;
                        schedule.Loan_Outstanding = prlAmt;
                        //dgvSchedule.Rows.Add(duedate.ToString("dd-MM-yyyy"), 0, intCalc, intCalc);
                        scheduleList.Add(schedule);
                        totIntCalc += intCalc;
                        duedate = duedate.AddMonths(demandFrequency);
                    }
                }
                duedate = firstPrlDueDate.Date;
                
                for (int i = 1; i <= prd; i += demandFrequency)
                {
                    schedule = new();
                    //intCalc = TermDepositService.CalcIntForFD(nonODPrl, roi, 1, false);
                    intCalc = Utilities.Calculate_Interest(nonODPrl, roi, Utilities.GetNoOfDays(duedate, fromDate));
                    if (i == prd)
                    {
                        prlDem = nonODPrl;
                        nonODPrl = 0;
                    }
                    else
                    {
                        switch (instType)
                        {
                            case 1:
                                prlDem = instAmt;
                                break;
                            case 2:
                                prlDem = instAmt - intCalc;
                                break;
                        }
                        nonODPrl -= prlDem;
                    }
                    totPrlDem += prlDem;
                    if (totPrlDem > prlAmt)
                    {
                        prlDem -= (totPrlDem - prlAmt);
                        totPrlDem = prlAmt;
                    }
                    if (i == prd - demandFrequency + 1)
                        prlDem += (prlAmt - totPrlDem);
                    //dgvSchedule.Rows.Add(duedate.ToString("dd-MM-yyyy"), prlDem, intCalc, prlDem + intCalc);
                    schedule.Due_Date = duedate;
                    schedule.Interest_Demand = intCalc;
                    schedule.Principal_Demand = prlDem;
                    schedule.Total_Demand = intCalc + prlDem;
                    schedule.Loan_Outstanding = nonODPrl;
                    scheduleList.Add(schedule);

                    fromDate = duedate;
                    duedate = duedate.AddMonths(demandFrequency);
                    totIntCalc += intCalc;
                    //totPrlDem += prlDem;
                    totAmt += intCalc + prlDem;
                }
                //dgvSchedule.Rows.Add("Total", totPrlDem, totIntCalc, totAmt);

            }
            catch (Exception ex)
            {
                Console.WriteLine (ex.Message + "\n error in prepare schedule", "error");
            }
            return scheduleList;
        }

        public static int GetNoOfCompletedMonthsBetweenTwoDates(DateTime startDate, DateTime endDate)
        {
            int noOfCompletedMonths = 0;
            noOfCompletedMonths = (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month - (endDate.Day < startDate.Day ? 1 : 0);
            return noOfCompletedMonths;
        }
    }
}
