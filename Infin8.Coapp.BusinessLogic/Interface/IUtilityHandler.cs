using Infin8.Coapp.Dto;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IUtilityHandler
    {
        int GetAgeBetweenTwoDates(DateTime birthDate, DateTime currentDate);
        int GetMonthsBetweenDates(DateTime startDate, DateTime endDate);
        //int GetDaysBetweenDates(DateTime startDate, DateTime endDate);
        int GetNoOfDays(DateTime endDate,DateTime startDate);
        double Calculate_Interest(double loanAmount, double roi, int noOfDays);
        int GetAge(DateTime startDate, DateTime endDate);
        DateTime GetTDMaturityDate(DateTime TDDate, int NoOfMonths, int NoOfDays);
        double GetFDMaturityAmount(double Principal, DateTime FromDate, DateTime ToDate, int PeriodInMonths, int PeriodInDays, double ROI, int IntPayablePrd, bool IsDiscountRate, int CompoundFrequency);
        double GetRDMaturityAmount(double RDAmount, int Prd, double ROI, bool ApplyQuarterlyCompound);
        DateTime AddMonthsForFD(DateTime date, int noOfMonths);
        double CalculateInterestForFixedDeposit(double Amount, double Roi, int Months, bool IsDiscount);
        DateTime GetNextMonthForFD(DateTime runningDate, DateTime fdDate, int addMonths);
        DateTime GetFirstDueDateForPCARDB(DateTime sanctionedDate);
        double GetLoanInstalmentAmount(double loanAmount, int period, double ROI, int instalmentType, int DemFrequency);
        List<LoanRepaymentScheduleVM> GetLoanRepaymentScheduleVMList(DateTime currentDate,double principal, double roi, int prd, int gracePeriod, double instalmentAmount, int instType, int demandFrequency,
            DateTime firstIntDueDate, DateTime firstPrlDueDate);
        DateTime AddMonths(DateTime date, int addmonths);
        string RupeesInWords(double number);
        bool IsEndOfMonth(DateTime date);
        bool IsYearEnd(DateTime date);
    }
}
