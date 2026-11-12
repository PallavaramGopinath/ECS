using Infin8.Coapp.Dto;
using Infin8.Coapp.Repository;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>Loan eligibility per spec 06 §3.3 (TSIS Sum_DueItems / Display Loan Eligibility).</summary>
    public class LoanEligibilityHandler : ILoanEligibilityHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LoanEligibilityHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<LoanEligibilityResultVM> CalculateAsync(LoanEligibilityInputVM input)
            => Task.FromResult(Calculate(input));

        public static LoanEligibilityResultVM Calculate(LoanEligibilityInputVM input)
        {
            var r = new LoanEligibilityResultVM();

            // Demand. Monthly interest is part of the demand only for Inst_Type 3 (principal + interest).
            double interest = input.InstType == 3
                ? Math.Round(input.LoanAmount * input.RateOfInterest / 1200.0, 0, MidpointRounding.AwayFromZero)
                : 0;
            r.NewLoanDemand = input.InstallmentAmount + interest;
            r.DepositDemand = input.DepositDemand;
            r.Demand = r.NewLoanDemand + r.DepositDemand;
            r.ExistingDemand = input.ExistingDemand;
            r.OtherDeductions = input.TotalDeductions - input.SocietyDeduction;
            r.NetPay = input.GrossPay - r.Demand - r.ExistingDemand - r.OtherDeductions;

            // Ceilings
            r.TwentyFiveTimesOfGross = input.GrossPay * 25;
            r.FiftyPercentOfGross = Math.Round(input.GrossPay * 0.5, 0, MidpointRounding.AwayFromZero);
            r.TwentyFivePercentOfGross = Math.Round(input.GrossPay * 0.25, 0, MidpointRounding.AwayFromZero);

            // Both salary checks apply only when the scheme adopts loan eligibility.
            if (input.AdoptLoanEligibility)
            {
                if (r.Demand + r.ExistingDemand > r.FiftyPercentOfGross)
                {
                    r.FailsDemandCeiling = true;
                    r.Reasons.Add("Demand amount is greater than 50% of gross pay.");
                }
                if (r.TwentyFivePercentOfGross > r.NetPay)
                {
                    r.FailsNetPayFloor = true;
                    r.Reasons.Add("Net Pay is less than 25% of gross pay.");
                }
            }

            // The loan cannot run past retirement (business decision on spec §13 Q2).
            // Service in months is mandatory: without a date of birth / retirement date the check cannot pass.
            if (!input.ServiceInMonths.HasValue)
            {
                r.FailsServiceCheck = true;
                r.Reasons.Add("Service in months is not available. Enter the member's date of birth.");
            }
            else if (input.ServiceInMonths.Value < input.PeriodOfLoan)
            {
                r.FailsServiceCheck = true;
                r.Reasons.Add($"Remaining service ({input.ServiceInMonths} months) is less than the loan period ({input.PeriodOfLoan} months).");
            }

            r.IsEligible = !r.FailsDemandCeiling && !r.FailsNetPayFloor && !r.FailsServiceCheck;
            if (r.IsEligible) r.Reasons.Add("Eligible.");
            return r;
        }
    }
}
