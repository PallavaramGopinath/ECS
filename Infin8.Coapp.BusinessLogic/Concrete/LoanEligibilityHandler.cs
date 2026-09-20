using Infin8.Coapp.Dto;
using Infin8.Coapp.Repository;

namespace Infin8.Coapp.BusinessLogic
{
    public class LoanEligibilityHandler : ILoanEligibilityHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LoanEligibilityHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<LoanEligibilityResultVM> CalculateAsync(LoanEligibilityInputVM input)
        {
            var map = await _unitOfWork.MapGeneral.GetMapGeneralAsync(input.BrCode ?? "");
            int retirementAgeMonths = map?.TSISRetirementAgeInMonths ?? 0;

            var r = new LoanEligibilityResultVM();

            // --- Demand calculation (Sec 10.2) ---
            // Interest demand on the new loan, monthly (assumption: simple monthly interest on principal).
            double interestDemandOnLoan = input.LoanAmount * (input.RateOfInterest / 100.0) / 12.0;
            r.CurrentLoanDemand = input.InstallmentAmount + interestDemandOnLoan;
            r.CurrentDepositDemand = input.CurrentDepositDemand;
            r.OtherDeductions = input.TotalDeductions - input.SocietyDeduction;
            r.NonDeductableLoanDemand = input.NonDeductableLoanDemand;
            r.TotalDemand = r.CurrentLoanDemand + r.CurrentDepositDemand + r.OtherDeductions + r.NonDeductableLoanDemand;
            r.NewNetPay = input.GrossPay - r.TotalDemand;

            // --- Ceilings (Sec 10.3 / 10.4) ---
            r.FiftyPercentOfGross = input.GrossPay * 0.50;
            r.TwentyFivePercentOfGross = input.GrossPay * 0.25;
            r.TwentyFiveTimesOfGross = input.GrossPay * 25.0;

            // --- Eligibility rules (Sec 10.3) ---
            if (input.AdoptLoanEligibility && (r.TotalDemand + input.ExistingDemand) > r.FiftyPercentOfGross)
            {
                r.FailsDemandCeiling = true;
                r.Reasons.Add("Total demand plus existing demand exceeds 50% of gross pay.");
            }
            if (r.TwentyFivePercentOfGross > input.NetPay)
            {
                r.FailsNetPayFloor = true;
                r.Reasons.Add("25% of gross pay exceeds net pay.");
            }
            // NOTE (assumption to confirm): spec compares "Service in Months" literally to
            // Map_General.TSISRetirementAgeInMonths — interpreted as months of service rendered.
            if (input.ServiceInMonths < retirementAgeMonths)
            {
                r.FailsServiceCheck = true;
                r.Reasons.Add($"Service in months ({input.ServiceInMonths}) is below the configured minimum ({retirementAgeMonths}).");
            }

            r.IsEligible = !r.FailsDemandCeiling && !r.FailsNetPayFloor && !r.FailsServiceCheck;
            if (r.IsEligible) r.Reasons.Add("Eligible.");
            return r;
        }
    }
}
