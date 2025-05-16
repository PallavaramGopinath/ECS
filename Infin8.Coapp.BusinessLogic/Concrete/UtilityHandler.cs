using Infin8.Coapp.Dto;

namespace Infin8.Coapp.BusinessLogic
{
    public class UtilityHandler : IUtilityHandler
    {
        public int GetAgeBetweenTwoDates(DateTime birthDate, DateTime currentDate)
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

        public int GetMonthsBetweenDates(DateTime startDate, DateTime endDate)
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

        //public int GetDaysBetweenDates(DateTime startDate, DateTime endDate)
        //{
        //    return (endDate - startDate).Days;
        //}

        public int GetNoOfDays(DateTime endDate, DateTime startDate)
        {
            TimeSpan ts = endDate.Date - startDate.Date;
            return ts.Days;
        }

        public double Calculate_Interest(double loanAmount, double ROI, int noOfDays)
        {
            double interest = 0;
            if (noOfDays <= 0) noOfDays = 0;
            interest = Math.Round((loanAmount * (ROI / 100)) / 365 * noOfDays, 0);
            return interest;
        }

        public int GetAge(DateTime startDate, DateTime endDate)
        {
            return (endDate.Year - startDate.Year - 1) +
                (((endDate.Month > startDate.Month) ||
                ((endDate.Month == startDate.Month) && (endDate.Day >= startDate.Day))) ? 1 : 0);
        }

        public DateTime GetTDMaturityDate(DateTime TDDate, int NoOfMonths, int NoOfDays)
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

        public double GetFDMaturityAmount(double Principal, DateTime FromDate, DateTime ToDate, int PeriodInMonths, int PeriodInDays, double ROI, int IntPayablePrd, bool IsDiscountRate, int CompoundFrequency)
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
                case 0:  /// compound not applicable
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
            return MaturityAmount;
        }

        public double GetRDMaturityAmount(double RDAmount, int Prd, double ROI, bool ApplyQuarterlyCompound)
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

        public DateTime AddMonthsForFD(DateTime date, int noOfMonths)
        {
            if (date.Day != DateTime.DaysInMonth(date.Year, date.Month))    /// if not last day of the month
                return date.AddMonths(noOfMonths);
            else
                return date.AddDays(1).AddMonths(noOfMonths).AddDays(-1);
        }

        public double CalculateInterestForFixedDeposit(double Amount, double Roi, int Months, bool IsDiscount)
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

        public DateTime GetNextMonthForFD(DateTime runningDate, DateTime fdDate, int addMonths)
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

        public DateTime GetFirstDueDateForPCARDB(DateTime sanctionedDate)
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

        public double GetLoanInstalmentAmount(double loanAmount, int period, double ROI, int instalmentType, int DemFrequency)
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

        public List<LoanRepaymentScheduleVM> GetLoanRepaymentScheduleVMList(DateTime currentDate,double principal, double roi, int prd, int gracePeriod, double instalmentAmount, int instType, int demandFrequency, DateTime firstIntDueDate, DateTime firstPrlDueDate)
        {
            List<LoanRepaymentScheduleVM> repaymentSchedule = new List<LoanRepaymentScheduleVM>();
            DateTime duedate = firstIntDueDate.Date;
            DateTime fromDate = currentDate.Date;
            int slno = 1;
            double intCalc = 0, nonODPrl = 0, prlDem = 0, totIntCalc = 0, totPrlDem = 0, totAmt = 0;
            try
            {
                gracePeriod = GetMonthsBetweenDates(firstIntDueDate , firstPrlDueDate);
                

                nonODPrl = principal;
                if (gracePeriod > 0)
                {
                    for (int i = 1; i <= gracePeriod; i += demandFrequency)
                    {
                        intCalc = 0;
                        intCalc = Calculate_Interest(nonODPrl, roi,GetNoOfDays(duedate.AddMonths(demandFrequency), duedate));
                        repaymentSchedule.Add(new LoanRepaymentScheduleVM
                        {
                            SlNo = slno,
                            DemandDate = duedate,
                            PrincipalDemand =0,
                            InterestDemand = intCalc,
                            NonODPrincipal = nonODPrl,
                        });
                        slno ++;
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

        public DateTime AddMonths(DateTime date, int addmonths)
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

        public string RupeesInWords(double number)
        {
            if (number == 0)
            {
                return "zero";
            }
            if (number < 0)
            {
                return "minus " + RupeesInWords(Math.Abs(number));
            }
            string words = "";

            long longPortion = (long)number;
            double fraction = (number * 100 - longPortion * 100);
            long decPortion = (long)fraction;

            words = NumberToWords(longPortion);

            words = "Rupees " + words;
            if (decPortion > 0)
            {
                words += "; and Paise ";
                words += NumberToWords(decPortion);
            }
            words = words.Trim() + " only";
            return words;
        }

        public string NumberToWords(long number)
        {
            if (number == 0)
                return "Zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 10000000) > 0)
            {
                words += NumberToWords(number / 10000000) + " Crore ";
                number %= 10000000;
            }

            if ((number / 100000) > 0)
            {
                words += NumberToWords(number / 100000) + " Lakh ";
                number %= 100000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += " " + unitsMap[number % 10];
                }
            }

            return words;
        }

        public bool IsEndOfMonth(DateTime date)
        {
            return date.Day == DateTime.DaysInMonth(date.Year, date.Month);
        }

        public bool IsYearEnd(DateTime date)
        {
            return date.Month == 12 && date.Day == 31;
        }
    }
}
