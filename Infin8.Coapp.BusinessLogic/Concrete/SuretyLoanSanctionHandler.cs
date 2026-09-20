using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;

namespace Infin8.Coapp.BusinessLogic
{
    public class SuretyLoanSanctionHandler : ISuretyLoanSanctionHandler
    {
        readonly IUnitOfWork _unitOfWork;
        readonly ILoanEligibilityHandler _eligibilityHandler;
        public SuretyLoanSanctionHandler(IUnitOfWork unitOfWork, ILoanEligibilityHandler eligibilityHandler)
        {
            _unitOfWork = unitOfWork;
            _eligibilityHandler = eligibilityHandler;
        }

        public async Task<SuretyLoanSanctionDataVM> GetSuretyLoanMemberAsync(string memberNo, string brCode)
        {
            try
            {
                return await _unitOfWork.SuretyLoanSanction.GetSuretyLoanMemberAsync(memberNo, brCode);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! Could not load member details for sanction.");
            }
        }

        public async Task<SuretyLoanSchemeDefaultsVM> GetSchemeDefaultsAsync(int schemeId, string brCode)
        {
            try
            {
                return await _unitOfWork.SuretyLoanSanction.GetSchemeDefaultsAsync(schemeId, brCode);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! Could not load scheme defaults.");
            }
        }

        public async Task<SuretyDeductionsDataVM> GetDeductionsDataAsync(decimal memId, decimal suretyMemId,
            int schemeId, double loanAmount, string brCode)
        {
            try
            {
                return await _unitOfWork.SuretyLoanSanction.GetDeductionsDataAsync(memId, suretyMemId, schemeId, loanAmount, brCode);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! Could not load deductions data.");
            }
        }

        public async Task<SanctionSaveResultVM> SaveSanctionAsync(SuretyLoanSanctionSaveVM vm,
            decimal usrId, decimal yrId, string brCode, DateTime sanctionDate)
        {
            // --- Integrity rule 1: duplicate-sanction guard (Sec 12.2) ---
            var map = await _unitOfWork.MapGeneral.GetMapGeneralAsync(brCode);
            int minInst = map?.MinInstalmentsForLoan ?? 0;
            var lastSanctionDate = await _unitOfWork.SuretyLoanSanction.GetLastSanctionDateAsync(vm.Mem_Id, vm.Scheme_Id, brCode);
            if (lastSanctionDate.HasValue && minInst > 0)
            {
                int gap = MonthsBetween(lastSanctionDate.Value, sanctionDate);
                if (gap < minInst)
                    return new SanctionSaveResultVM
                    {
                        Success = false,
                        Message = $"A sanction for this member and scheme exists within the minimum gap " +
                                  $"({minInst} months); only {gap} month(s) have elapsed. Save rejected."
                    };
            }

            // --- Integrity rule 2: re-verify eligibility server-side (Sec 10) ---
            LoanEligibilityResultVM? elig = null;
            if (vm.EligibilityInput != null)
            {
                vm.EligibilityInput.BrCode = brCode;
                elig = await _eligibilityHandler.CalculateAsync(vm.EligibilityInput);
                if (!elig.IsEligible)
                    return new SanctionSaveResultVM
                    {
                        Success = false,
                        Message = "Not eligible: " + string.Join(" ", elig.Reasons)
                    };
            }

            // --- Build Loan_Eligibility row ---
            var eligibility = new Loan_Eligibility
            {
                LoanElig_Date = sanctionDate,
                Loan_Id = 0,
                Mem_Id = (int)vm.Mem_Id,
                GrossPay = vm.GrossPay,
                SocietyDeductions = vm.SocietyDeduction,
                OtherThanSocietyDeductions = elig?.OtherDeductions ?? (vm.TotalDeductions - vm.SocietyDeduction),
                TotalDeductions = vm.TotalDeductions,
                NetPay = vm.NetPay,
                TwentyFiveTimesOfGP = elig?.TwentyFiveTimesOfGross ?? 0,
                FiftyPercentOnGP = elig?.FiftyPercentOfGross ?? 0,
                TwentyFivePercentOnGP = elig?.TwentyFivePercentOfGross ?? 0,
                DemandAmt = elig?.TotalDemand ?? 0,
                NetPayAfterDemand = elig?.NewNetPay ?? 0,
                LoanAppliedAmt = vm.LoanAmount,
                PeriodOfLoan = vm.PeriodOfLoan,
                IsMemberBecomeDefaulter = false,
                IsSurityBecomeDefaulter = false,
                Voc_Id = 0,
                Usr_Id = (int)usrId,
                Yr_Id = (int)yrId,
                BrCode = brCode
            };

            // --- Build Loan_Sanction header (approval-only: Loan_Id = 0; non-financial: Voc_Id = 0) ---
            double totalReceipt = vm.DeductionRows.Sum(r => r.LDeduct);
            var sanction = new Loan_Sanction
            {
                Loan_Id = 0,
                Mem_Id = vm.Mem_Id,
                SuretyMem_Id = vm.SuretyMem_Id,
                Scheme_Id = vm.Scheme_Id,
                Inst_Type = vm.Inst_Type,
                SanctionDate = sanctionDate,
                LoanAmount = vm.LoanAmount,
                PeriodOfLoan = vm.PeriodOfLoan,
                RateOfInterest = vm.RateOfInterest,
                PenalRate = vm.PenalRate,
                InstalmentAmount = vm.InstalmentAmount,
                FirstDueDate = vm.FirstDueDate,
                BasicPay = vm.BasicPay,
                GrossPay = vm.GrossPay,
                SocietyDeduction = vm.SocietyDeduction,
                TotalDeductions = vm.TotalDeductions,
                NetPay = vm.NetPay,
                MemberShareCapitalAmount = vm.MemberShareCapitalAmount,
                SuretyShareCapitalAmount = vm.SuretyShareCapitalAmount,
                MemberTD = vm.MemberTD,
                ThisLoanLimit = vm.ThisLoanLimit,
                MemberTotalLimit = vm.MemberTotalLimit,
                NoOfInstalmentsCompleted = vm.NoOfInstalmentsCompleted,
                NoOfInstalmentsRecovered = vm.NoOfInstalmentsRecovered,
                TotalReceiptAmount = totalReceipt,
                IsMemberBecomeDefaulter = false,
                IsSurityBecomeDefaulter = false,
                LoanSanction_Status = "S",
                Voc_Id = 0,
                Usr_Id = usrId,
                Yr_Id = yrId,
                BrCode = brCode,
                Voc_Status = "V"
            };

            // --- Build Loan_Sanction_Trn lines ---
            var trnList = new List<Loan_Sanction_Trn>();
            foreach (var row in vm.DeductionRows)
            {
                if (row.RowType == "Loan")
                {
                    // Principal recovery line (Status "P")
                    trnList.Add(NewTrn(vm, usrId, yrId, brCode, status: "P", loanId: row.Loan_Id, ledId: row.Led_Id,
                        memId: row.Mem_Id != 0 ? row.Mem_Id : vm.Mem_Id, recSchemeId: row.Scheme_Id,
                        receiptDesc: row.Ledger_Name, receiptItem: row.LDeduct, odAmount: row.ODAmount));
                    // Interest recovery line (Status "I")
                    if (row.InterestBalance > 0)
                        trnList.Add(NewTrn(vm, usrId, yrId, brCode, status: "I", loanId: row.Loan_Id, ledId: row.Led_Id,
                            memId: row.Mem_Id != 0 ? row.Mem_Id : vm.Mem_Id, recSchemeId: row.Scheme_Id,
                            receiptDesc: (row.Ledger_Name ?? "") + " Interest", receiptItem: row.InterestBalance,
                            calcAmount: row.InterestBalance));
                }
                else
                {
                    // Share-capital / sundry / manual deduction line (blank Status)
                    trnList.Add(NewTrn(vm, usrId, yrId, brCode, status: "", loanId: row.Loan_Id, ledId: row.Led_Id,
                        memId: row.Mem_Id != 0 ? row.Mem_Id : vm.Mem_Id, recSchemeId: row.Scheme_Id,
                        receiptDesc: row.Ledger_Name, receiptItem: row.LDeduct));
                }
            }

            // Eligibility-breakdown lines (blank Status; figures on the payment side) — Sec 13.2.
            if (elig != null)
            {
                AddBreakdown(trnList, vm, usrId, yrId, brCode, "25 Times of Gross Pay", elig.TwentyFiveTimesOfGross);
                AddBreakdown(trnList, vm, usrId, yrId, brCode, "50 Percent of Salary", elig.FiftyPercentOfGross);
                AddBreakdown(trnList, vm, usrId, yrId, brCode, "25 Percent of Salary", elig.TwentyFivePercentOfGross);
                AddBreakdown(trnList, vm, usrId, yrId, brCode, "Society Demand", vm.SocietyDeduction);
                AddBreakdown(trnList, vm, usrId, yrId, brCode, "Other Deductions", elig.OtherDeductions);
                AddBreakdown(trnList, vm, usrId, yrId, brCode, "Net Pay", elig.NewNetPay);
            }

            try
            {
                var (sanctionId, eligId) = await _unitOfWork.SuretyLoanSanction.PersistSanctionAsync(eligibility, sanction, trnList);
                return new SanctionSaveResultVM { Success = true, LoanSanction_Id = sanctionId, LoanElig_Id = eligId, Message = "Sanction saved." };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! Sanction not saved.");
            }
        }

        private static Loan_Sanction_Trn NewTrn(SuretyLoanSanctionSaveVM vm, decimal usrId, decimal yrId, string brCode,
            string status, decimal loanId, decimal ledId, decimal memId, int recSchemeId, string? receiptDesc,
            double receiptItem, double odAmount = 0, double calcAmount = 0)
            => new Loan_Sanction_Trn
            {
                Loan_Id = loanId,
                Mem_Id = memId,
                Led_Id = ledId,
                RecScheme_Id = recSchemeId,
                Status = status,
                ReceiptDescription = receiptDesc,
                ReceiptItem = receiptItem,
                CalcAmount = calcAmount,
                ODAmount = odAmount,
                Usr_Id = usrId,
                Yr_Id = yrId,
                BrCode = brCode,
                Voc_Status = "V"
            };

        private static void AddBreakdown(List<Loan_Sanction_Trn> list, SuretyLoanSanctionSaveVM vm,
            decimal usrId, decimal yrId, string brCode, string description, double amount)
            => list.Add(new Loan_Sanction_Trn
            {
                Loan_Id = 0,
                Mem_Id = vm.Mem_Id,
                Status = "",
                PaymentDescription = description,
                PaymentItem = amount,
                Usr_Id = usrId,
                Yr_Id = yrId,
                BrCode = brCode,
                Voc_Status = "V"
            });

        private static int MonthsBetween(DateTime from, DateTime to)
        {
            if (from > to) (from, to) = (to, from);
            int months = (to.Year - from.Year) * 12 + to.Month - from.Month;
            if (to.Day < from.Day) months--;
            return months < 0 ? 0 : months;
        }
    }
}
