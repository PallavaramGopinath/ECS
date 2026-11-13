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

            // Demand. The first month's interest is part of the demand for every scheme except
            // Inst_Type 2 (EMI), whose instalment already includes the interest.
            double interest = input.InstType == 2
                ? 0
                : Math.Round(input.LoanAmount * input.RateOfInterest / 1200.0, 0, MidpointRounding.AwayFromZero);
            r.InterestDemand = interest;
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
                    r.Reasons.Add($"Demand amount ({r.Demand + r.ExistingDemand:N2}) is greater than 50% of gross pay ({r.FiftyPercentOfGross:N2}).");
                }
                if (r.TwentyFivePercentOfGross > r.NetPay)
                {
                    r.FailsNetPayFloor = true;
                    r.Reasons.Add($"Carry home salary (net pay {r.NetPay:N2}) is less than 25% of gross pay ({r.TwentyFivePercentOfGross:N2}).");
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
            if (r.IsEligible) r.Reasons.Add($"Loan applied amount ({input.LoanAmount:N2}) is eligible.");
            return r;
        }
    }
}
