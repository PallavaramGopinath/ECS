using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using Infin8.Coapp.Utility.Enums;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>
    /// ECS Loan Sanction (spec 06). Writes loan_sanction, loan_sanction_trn and loan_eligibility with
    /// status "E" (sanctioned, awaiting disbursement). No voucher and no loan_master.
    /// </summary>
    public class EcsLoanSanctionHandler : IEcsLoanSanctionHandler
    {
        // Every ECS product (surety, education, draught, …) is Loan_Type 1, told apart by Scheme_Id (spec 07 §3.3).
        private static readonly int[] EcsSanctionLoanTypes = { (int)LoanCategory.ThriftCreditLoans };
        private const double Tolerance = 0.005;

        readonly IUnitOfWork _unitOfWork;
        readonly ICreateReportsHandler _createReportsHandler;
        readonly IUtilityHandler _utilityHandler;

        public EcsLoanSanctionHandler(IUnitOfWork unitOfWork, ICreateReportsHandler createReportsHandler,
            IUtilityHandler utilityHandler)
        {
            _unitOfWork = unitOfWork;
            _createReportsHandler = createReportsHandler;
            _utilityHandler = utilityHandler;
        }

        private static bool IsEcsScheme(int loanType) =>
            EcsSanctionLoanTypes.Contains(loanType)
            && LoanCategoryPolicy.IsAllowed((LoanCategory)loanType, InstitutionType.ECS);

        // =====================================================================
        // Lookups
        // =====================================================================

        public async Task<List<DropdownItem>> GetEcsSchemesAsync(string brCode)
        {
            var types = EcsSanctionLoanTypes.Where(IsEcsScheme).ToList();
            return await _unitOfWork.EcsLoanSanction.GetSchemeItemsAsync(types, brCode);
        }

        public async Task<SuretyLoanSanctionDataVM> GetMemberAsync(string memberNo, DateTime asOn, string brCode)
        {
            try
            {
                return await _unitOfWork.EcsLoanSanction.GetMemberAsync(memberNo, asOn, brCode);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! Could not load member details for sanction.");
            }
        }

        public Task<EcsPayDetailsVM> GetPayDetailsAsync(decimal memId, string brCode)
            => _unitOfWork.EcsLoanSanction.GetPayDetailsAsync(memId, brCode);

        public async Task<SuretyLoanSanctionDataVM> UpdateMemberDobAsync(EcsMemberDobVM vm, DateTime asOn, string brCode)
        {
            var repo = _unitOfWork.EcsLoanSanction;
            var dob = vm.DateOfBirth.Date;
            string? error = null;
            if (vm.MemId <= 0) error = "Select a member first.";
            else if (dob.Year < 1900) error = "Enter a valid date of birth.";
            else if (dob > asOn.Date) error = "Date of birth cannot be after the business date.";
            else if (dob.AddYears(18) > asOn.Date) error = "Member must be at least 18 years old on the business date.";

            if (error == null)
            {
                try
                {
                    if (!await repo.SetMemberDobAsync(vm.MemId, dob, brCode))
                        error = "Date of birth was not saved: the member was not found or already has a date of birth.";
                    else
                        await _unitOfWork.CompleteAsync();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(ex.Message + " Something went wrong! Could not save the date of birth.");
                }
            }

            var member = await repo.GetMemberByIdAsync(vm.MemId, asOn, brCode);
            member.Message = error;
            return member;
        }

        public async Task<SuretyLoanSchemeDefaultsVM> GetSchemeDefaultsAsync(int schemeId, decimal memId,
            decimal excludeSanctionId, DateTime asOn, string brCode)
        {
            var repo = _unitOfWork.EcsLoanSanction;
            var d = new SuretyLoanSchemeDefaultsVM { Scheme_Id = schemeId, FirstDueDate = Utilities.FirstOfNextMonth(asOn) };

            var scheme = await repo.GetSchemeAsync(schemeId, brCode);
            if (scheme == null)
            {
                d.Message = "Loan scheme not found.";
                return d;
            }
            if (!IsEcsScheme(scheme.Loan_Type))
            {
                d.Message = "This loan scheme is not available for ECS sanction.";
                d.Warnings.Add(new SanctionWarningVM { Code = "NOT_ECS", Blocking = true, Message = d.Message });
                return d;
            }

            d.Found = true;
            d.Scheme_Name = scheme.Scheme_Name;
            d.Loan_Type = scheme.Loan_Type;
            d.LoanAmount = scheme.MaximumLoanAmount;
            d.PeriodOfLoan = scheme.MaximumPrincipalPeriod;
            d.ThisLoanLimit = scheme.MaximumLoanAmount;
            d.InstType = scheme.Inst_Type;
            d.AdoptLoanEligibility = scheme.AdoptLoanEligibility == 1;
            d.MatchShareCapital = scheme.MatchShareCapital;
            d.AdoptLoanLimit = scheme.AdoptLoanLimit == 1;
            d.IsSuretyRequired = scheme.IssurityMemberRequired == 1;
            d.SchemeDetails = BuildSchemeDetails(scheme);

            var map = await _unitOfWork.MapGeneral.GetMapGeneralAsync(brCode);
            d.MaxLoanLimit = map?.MaxLoanLimit ?? 0;

            var roi = await repo.GetRoiAsync(schemeId, asOn, brCode);
            if (roi.HasValue)
            {
                d.RoiTemplateFound = true;
                d.RateOfInterest = roi.Value.roi;
                d.PenalRate = roi.Value.pi;
            }
            else
            {
                d.Warnings.Add(new SanctionWarningVM
                {
                    Code = "ROI_NOT_FOUND",
                    Blocking = false,
                    Message = $"ROI template not found for scheme {scheme.Scheme_Name} as on {asOn:dd/MM/yyyy}. Enter ROI and penal rate."
                });
            }

            if (memId > 0)
            {
                d.Warnings.AddRange(await GetSchemeWarningsAsync(memId, schemeId, excludeSanctionId, asOn, map?.MinInstalmentsForLoan ?? 0, brCode));

                // The loan cannot run past retirement: service in months is mandatory and caps the period.
                var member = await repo.GetMemberByIdAsync(memId, asOn, brCode);
                if (member.ServiceInMonths == null)
                    d.Warnings.Add(new SanctionWarningVM
                    {
                        Code = "NO_SERVICE", Blocking = true,
                        Message = "Service in months is not available for this member. Enter the date of birth first."
                    });
                else if (member.ServiceInMonths <= 0)
                    d.Warnings.Add(new SanctionWarningVM
                    {
                        Code = "NO_SERVICE", Blocking = true,
                        Message = "Member has no remaining service before retirement; the loan cannot be sanctioned."
                    });
                else if (member.ServiceInMonths < d.PeriodOfLoan)
                {
                    d.Warnings.Add(new SanctionWarningVM
                    {
                        Code = "PERIOD_REDUCED", Blocking = false,
                        Message = $"Loan period reduced from {d.PeriodOfLoan} to {member.ServiceInMonths} months (remaining service)."
                    });
                    d.PeriodOfLoan = member.ServiceInMonths.Value;
                }
            }
            return d;
        }

        private async Task<List<SanctionWarningVM>> GetSchemeWarningsAsync(decimal memId, int schemeId,
            decimal excludeSanctionId, DateTime asOn, int minInstalments, string brCode)
        {
            var repo = _unitOfWork.EcsLoanSanction;
            var warnings = new List<SanctionWarningVM>();

            if (await repo.ExistsSanctionOnDateAsync(memId, schemeId, asOn, new[] { "E" }, excludeSanctionId, brCode))
                warnings.Add(new SanctionWarningVM
                {
                    Code = "DUPLICATE", Blocking = true,
                    Message = "This loan is already sanctioned to the member on this date and awaits disbursement."
                });

            if (await repo.ExistsSanctionOnDateAsync(memId, schemeId, asOn, new[] { "S", "C" }, excludeSanctionId, brCode))
                warnings.Add(new SanctionWarningVM
                {
                    Code = "ALREADY_SAVED", Blocking = true,
                    Message = "This loan is already saved / disbursed to the member on this date."
                });

            var last = await repo.GetLastLoanSanctionDateAsync(memId, schemeId, brCode);
            if (last.HasValue && minInstalments > 0)
            {
                int elapsed = Utilities.GetMonthsBetween(last.Value, asOn);
                if (elapsed < minInstalments)
                    warnings.Add(new SanctionWarningVM
                    {
                        Code = "STIPULATED_PERIOD", Blocking = false,
                        Message = $"Member had availed this loan only {elapsed} month(s) ago against the stipulated period of {minInstalments} months. Continue?"
                    });
            }
            return warnings;
        }

        private static List<SchemeDetailRowVM> BuildSchemeDetails(Loan_Schemes s)
        {
            static string Freq(int v) => v switch { 1 => "Monthly", 4 => "Quarterly", 6 => "Half-yearly", 12 => "Annual", _ => v.ToString() };
            static string YesNo(int v) => v == 1 ? "Yes" : "No";
            return new List<SchemeDetailRowVM>
            {
                new() { Caption = "Principal Demand Frequency", Content = Freq(s.Dem_Frequency) },
                new() { Caption = "Interest Frequency",         Content = Freq(s.Int_Frequency) },
                new() { Caption = "Penal Interest Applied",     Content = s.PI_Application == 1 ? "On Principal Overdue" : "On Principal and Interest Overdue" },
                new() { Caption = "EMI Interest Applied",       Content = s.IOD_Application == 1 ? "EMI not Applied" : "EMI Applied" },
                new() { Caption = "Advance Principal",          Content = s.Adv_Prl_Application == 1 ? "Do not raise Principal Demand" : "Raise Principal Demand" },
                new() { Caption = "Match Share Capital",        Content = s.MatchShareCapital switch { 1 => "Member and Surety", 2 => "Member only", 3 => "Not required", _ => "-" } },
                new() { Caption = "Adopt Loan Eligibility",     Content = YesNo(s.AdoptLoanEligibility) },
                new() { Caption = "Adopt Maximum Loan Limit",   Content = YesNo(s.AdoptLoanLimit) },
                new() { Caption = "Is Surety Member Required",  Content = YesNo(s.IssurityMemberRequired) },
            };
        }

        // =====================================================================
        // Recoverables (§2.3 step 4)
        // =====================================================================

        public async Task<EcsRecoverablesVM> GetRecoverablesAsync(EcsRecoverablesRequestVM req, DateTime asOn, string brCode)
        {
            var repo = _unitOfWork.EcsLoanSanction;
            var result = new EcsRecoverablesVM();

            var scheme = await repo.GetSchemeAsync(req.SchemeId, brCode);
            if (scheme == null || !IsEcsScheme(scheme.Loan_Type))
                return new EcsRecoverablesVM { Success = false, Message = "Select a valid ECS loan scheme." };
            if (req.MemId <= 0)
                return new EcsRecoverablesVM { Success = false, Message = "Select a member." };

            var map = await _unitOfWork.MapGeneral.GetMapGeneralAsync(brCode);

            // --- Loans: deductable → P / I / PI rows; others → Non-Deductable tab ---
            var loans = await repo.GetLoanBalancesAsync(req.MemId, brCode);
            var deductableSchemes = await repo.GetDeductableSchemeIdsAsync(req.SchemeId, brCode);
            var ledgerNames = await repo.GetLedgerNamesAsync(
                loans.SelectMany(l => new[] { l.PrlLed_Id, l.IntLed_Id, l.PILed_Id }), brCode);
            string LedName(decimal id, string fallback) => ledgerNames.TryGetValue(id, out var n) && !string.IsNullOrEmpty(n) ? n : fallback;

            foreach (var loan in loans)
            {
                if (loan.Scheme_Id == req.SchemeId)
                {
                    result.NoOfInstalmentsCompleted += loan.DemandsRaised;
                    result.NoOfInstalmentsRecovered += loan.DemandsRecovered;
                }

                if (deductableSchemes.Contains(loan.Scheme_Id))
                {
                    void Add(string status, decimal ledId, string label, double amount, double od)
                    {
                        if (amount <= 0) return;
                        result.Recoverables.Add(new DeductableLoanRowVM
                        {
                            Loan_Id = loan.Loan_Id, Loan_No = loan.Loan_No, Status = status, Mem_Id = req.MemId,
                            Led_Id = ledId, Ledger_Name = LedName(ledId, $"{loan.Scheme_Name} {label}"),
                            Outstanding = Math.Round(amount, 2), ODAmount = Math.Round(od, 2),
                            Scheme_Id = loan.Scheme_Id, Loan_SlNo = loan.MaxTrnSlNo, RowType = "Deductable"
                        });
                    }
                    Add("P", loan.PrlLed_Id, "Principal", loan.PrlOS, loan.PrlOD);
                    Add("I", loan.IntLed_Id, "Interest", loan.IntOS, loan.IntOS);
                    Add("PI", loan.PILed_Id, "Penal Interest", loan.PIBal, loan.PIBal);
                }
                else
                {
                    double demand = loan.Inst_Amt + (loan.Int_Application == 2
                        ? Math.Round(loan.PrlOS * loan.Roi / 1200.0, 0, MidpointRounding.AwayFromZero) : 0);
                    result.NonDeductable.Add(new NonDeductableLoanRowVM
                    {
                        LoanId = loan.Loan_Id, LoanNo = loan.Loan_No, SchemeId = loan.Scheme_Id, Purpose = loan.Scheme_Name,
                        LoanOS = Math.Round(loan.PrlOS, 2), PrlOD = Math.Round(loan.PrlOD, 2),
                        IntBal = Math.Round(loan.IntOS, 2), PIBal = Math.Round(loan.PIBal, 2),
                        PrlLedId = loan.PrlLed_Id, IntLedId = loan.IntLed_Id, PILedId = loan.PILed_Id,
                        PrlLedName = LedName(loan.PrlLed_Id, loan.Scheme_Name + " Principal"),
                        IntLedName = LedName(loan.IntLed_Id, loan.Scheme_Name + " Interest"),
                        PILedName = LedName(loan.PILed_Id, loan.Scheme_Name + " Penal Interest"),
                        LoanSlNo = loan.MaxTrnSlNo, DemandAmount = demand,
                        AdoptLoanLimit = loan.AdoptLoanLimit == 1, MatchSC = loan.MatchShareCapital,
                        PrlRecovery = Math.Round(loan.PrlOS, 2), IntRecovery = Math.Round(loan.IntOS, 2),
                        PIRecovery = Math.Round(loan.PIBal, 2)
                    });
                    result.ExistingDemand += demand;
                    result.NonDeductableOS += loan.PrlOS;
                    if (loan.AdoptLoanLimit == 1) result.NonDeductableOSWithinLimit += loan.PrlOS;
                }
            }

            // --- Due-to (sundry debtor) balances ---
            result.Recoverables.AddRange(await repo.GetDueToRowsAsync(req.MemId, brCode));

            // --- Share capital (§3.1). Member and surety are each compared with the requirement. ---
            double pct = map?.ShareCapitalPercentageOnLoanOS ?? 0;
            result.RequiredSC = (int)(Math.Round(req.LoanAmount * pct / 100.0, 2) + 0.5);
            decimal scLed = map?.ShareCapital_Led_Id ?? 0;
            result.MemberSC = await repo.GetLedgerBalanceAsync(req.MemId, scLed, brCode);
            if (req.SuretyMemId > 0) result.SuretySC = await repo.GetLedgerBalanceAsync(req.SuretyMemId, scLed, brCode);
            var scName = (await repo.GetLedgerNamesAsync(new[] { scLed }, brCode)).GetValueOrDefault(scLed) ?? "Share Capital";

            if (scheme.MatchShareCapital == 1 || scheme.MatchShareCapital == 2)
            {
                double gap = result.RequiredSC - result.MemberSC;
                if (gap > 0)
                    result.Recoverables.Add(new DeductableLoanRowVM
                    {
                        Status = "MS", Mem_Id = req.MemId, Led_Id = scLed, Ledger_Name = scName + " (Member)",
                        Outstanding = Math.Round(gap, 2), MatchSC = true, RowType = "ShareCapital"
                    });
            }
            if (scheme.MatchShareCapital == 1 && req.SuretyMemId > 0)
            {
                double gap = result.RequiredSC - result.SuretySC;
                if (gap > 0)
                    result.Recoverables.Add(new DeductableLoanRowVM
                    {
                        Status = "SS", Mem_Id = req.SuretyMemId, Led_Id = scLed, Ledger_Name = scName + " (Surety)",
                        Outstanding = Math.Round(gap, 2), MatchSC = true, RowType = "ShareCapital"
                    });
            }

            // --- Thrift / FWD, pending deposits, surety overdue ---
            if (map != null)
            {
                result.MemberTD = await repo.GetDepositBalanceAsync(req.MemId, map.ThriftDeposit_DMId, brCode);
                result.MemberFWD = await repo.GetDepositBalanceAsync(req.MemId, (int)map.FamilyWelfareDepsitDM_Id, brCode);
            }
            result.PendingDeposits = await repo.GetPendingDepositsAsync(req.MemId, asOn, brCode);

            if (scheme.IssurityMemberRequired == 1 && req.SuretyMemId > 0)
            {
                var suretyLoans = await repo.GetLoanBalancesAsync(req.SuretyMemId, brCode);
                result.SuretyOverdues = suretyLoans.Where(l => l.PrlOD > 0 || l.IntOS > 0)
                    .Select(l => new SuretyLoanOverdueVM
                    {
                        Loan_Id = l.Loan_Id, LoanNo = l.Loan_No, Purpose = l.Scheme_Name,
                        OverdueAmount = Math.Round(l.PrlOD + l.IntOS + l.PIBal, 2)
                    }).ToList();
            }

            // --- Demand inputs ---
            var demandDate = req.FirstDueDate ?? Utilities.FirstOfNextMonth(asOn);
            result.DepositDemand = await repo.GetCurrentDepositDemandAsync(req.MemId, req.BasicPay, asOn, demandDate, brCode);
            result.InstalmentAmount = CalculateInstalment(req.LoanAmount, req.PeriodOfLoan, req.Roi, scheme.Inst_Type);
            return result;
        }

        /// <summary>§3.2 — LoanService.GetLoanInstalmentAmount.</summary>
        public static int CalculateInstalment(double amount, int period, double roi, int instType)
        {
            if (amount <= 0 || period <= 0) return 0;
            double inst;
            if (instType == 2)
            {
                if (roi <= 0) return 0;
                double r = roi / 100.0 / 12.0;
                inst = (r + r / (Math.Pow(1 + r, period) - 1)) * amount;
            }
            else
            {
                inst = amount / period;
            }
            return (int)Math.Round(inst, 0, MidpointRounding.AwayFromZero);
        }

        public async Task<DeductableLoanRowVM> ResolveDeductionAsync(decimal ledId, string brCode)
        {
            var (status, dmId, ledName) = await _unitOfWork.EcsLoanSanction.ResolveDeductionStatusAsync(ledId, brCode);
            return new DeductableLoanRowVM
            {
                Status = status, DM_Id = dmId, Led_Id = ledId, Ledger_Name = ledName,
                IsManual = true, RowType = "Manual"
            };
        }

        public Task<LoanEligibilityResultVM> CalculateEligibilityAsync(LoanEligibilityInputVM input)
            => Task.FromResult(LoanEligibilityHandler.Calculate(input));

        // =====================================================================
        // Save (create / modify)
        // =====================================================================

        public async Task<SanctionSaveResultVM> SaveAsync(SuretyLoanSanctionSaveVM vm, EcsUserContextVM ctx)
        {
            var repo = _unitOfWork.EcsLoanSanction;
            var brCode = ctx.BrCode;
            var date = ctx.CurrentDate.Date;
            var errors = new List<string>();
            bool isModify = vm.LoanSanction_Id > 0;

            // --- Context and modify pre-checks ---
            if (string.IsNullOrEmpty(brCode)) return Fail("Session has no branch. Please log in again.");
            if (ctx.YrBeginningDate > DateTime.MinValue && ctx.YrEndDate > DateTime.MinValue
                && (date < ctx.YrBeginningDate.Date || date > ctx.YrEndDate.Date))
                errors.Add("Sanction date is outside the current financial year.");

            if (isModify)
            {
                var existing = await repo.GetSanctionHeaderAsync(vm.LoanSanction_Id, brCode);
                if (existing == null) return Fail("The sanction being modified was not found.");
                if (existing.LoanSanction_Status != "E") return Fail("Only sanctions awaiting disbursement can be modified.");
                var subsequent = await repo.GetSubsequentTransactionMessagesAsync(vm.LoanSanction_Id, brCode);
                if (subsequent.Count > 0) return Fail(subsequent);
            }

            // --- Member, scheme, loan terms (§3.11) ---
            var member = await repo.GetMemberByIdAsync(vm.Mem_Id, date, brCode);
            if (!member.Found) return Fail("Member is not selected or not found.");
            if (member.ServiceInMonths == null)
                errors.Add("Service in months is not available for this member. Enter the date of birth.");
            else if (member.ServiceInMonths <= 0)
                errors.Add("Member has no remaining service before retirement.");

            var scheme = await repo.GetSchemeAsync(vm.Scheme_Id, brCode);
            if (scheme == null) return Fail("Select a loan scheme.");
            if (!IsEcsScheme(scheme.Loan_Type)) return Fail("This loan scheme is not available for ECS sanction.");

            var map = await _unitOfWork.MapGeneral.GetMapGeneralAsync(brCode);

            if (vm.GrossPay <= 0) errors.Add("Enter the gross pay.");
            if (vm.LoanAmount <= 0) errors.Add("Loan amount must be greater than zero.");
            if (vm.PeriodOfLoan <= 0) errors.Add("Period of loan must be greater than zero.");
            if (vm.RateOfInterest <= 0) errors.Add("Rate of interest must be greater than zero.");
            if (vm.PenalRate <= 0) errors.Add("Penal rate must be greater than zero.");
            if (vm.InstalmentAmount <= 0) errors.Add("Instalment amount must be greater than zero.");
            if (vm.FirstDueDate == null || vm.FirstDueDate.Value.Day != 1 || vm.FirstDueDate.Value.Date < date)
                errors.Add("First due date must be the 1st of a month, on or after the sanction date.");
            if (scheme.IssurityMemberRequired == 1 && vm.SuretyMem_Id <= 0)
                errors.Add("Surety is required for this loan scheme.");
            if (scheme.MaximumLoanAmount > 0 && vm.LoanAmount > scheme.MaximumLoanAmount)
                errors.Add($"Loan amount exceeds this loan's limit of {scheme.MaximumLoanAmount:0}.");

            // --- Duplicate / already saved / stipulated period (§3.4, §3.5) ---
            var warnings = await GetSchemeWarningsAsync(vm.Mem_Id, vm.Scheme_Id, vm.LoanSanction_Id, date,
                map?.MinInstalmentsForLoan ?? 0, brCode);
            errors.AddRange(warnings.Where(w => w.Blocking).Select(w => w.Message));
            if (!vm.OverrideStipulatedPeriod)
                errors.AddRange(warnings.Where(w => w.Code == "STIPULATED_PERIOD")
                    .Select(w => w.Message.Replace(" Continue?", " Confirm the stipulated-period override to continue.")));

            if (errors.Count > 0) return Fail(errors);

            // --- Recompute recoverables on the server and validate the chosen rows ---
            var recov = await GetRecoverablesAsync(new EcsRecoverablesRequestVM
            {
                MemId = vm.Mem_Id, SuretyMemId = vm.SuretyMem_Id, SchemeId = vm.Scheme_Id, LoanAmount = vm.LoanAmount,
                PeriodOfLoan = vm.PeriodOfLoan, Roi = vm.RateOfInterest, BasicPay = vm.BasicPay, FirstDueDate = vm.FirstDueDate
            }, date, brCode);
            if (!recov.Success) return Fail(recov.Message ?? "Could not compute recoverables.");

            if (vm.InstalmentAmount < recov.InstalmentAmount)
                errors.Add($"Instalment amount cannot be less than the calculated instalment of {recov.InstalmentAmount}.");

            var rows = await ValidateRowsAsync(vm, recov, brCode, errors);

            double totalRecoverable = Math.Round(rows.Sum(r => r.Outstanding), 2);
            if (vm.LoanAmount - totalRecoverable <= 0)
                errors.Add("Member does not have adequate loan balance.");

            // Existing demand excludes non-deductable loans moved into recoverables.
            var recoveredLoans = rows.Where(r => r.Loan_Id > 0).Select(r => r.Loan_Id).ToHashSet();
            double existingDemand = recov.NonDeductable.Where(n => !recoveredLoans.Contains(n.LoanId)).Sum(n => n.DemandAmount);

            // Loan limit (§3.11) — enforced when the scheme adopts it.
            if (scheme.AdoptLoanLimit == 1 && (map?.MaxLoanLimit ?? 0) > 0)
            {
                double withinLimit = recov.NonDeductable.Where(n => n.AdoptLoanLimit).Sum(n =>
                    Math.Max(0, n.LoanOS - rows.Where(r => r.Loan_Id == n.LoanId && r.Status == "P").Sum(r => r.Outstanding)));
                if (vm.LoanAmount + withinLimit > map!.MaxLoanLimit)
                    errors.Add($"Loan amount plus outstanding loans within the limit ({vm.LoanAmount + withinLimit:0}) exceeds the maximum loan limit of {map.MaxLoanLimit:0}.");
            }

            // Eligibility (§3.3), recomputed.
            var elig = LoanEligibilityHandler.Calculate(new LoanEligibilityInputVM
            {
                Mem_Id = vm.Mem_Id, Scheme_Id = vm.Scheme_Id, BrCode = brCode,
                LoanAmount = vm.LoanAmount, PeriodOfLoan = vm.PeriodOfLoan, InstallmentAmount = vm.InstalmentAmount,
                RateOfInterest = vm.RateOfInterest, InstType = scheme.Inst_Type,
                GrossPay = vm.GrossPay, TotalDeductions = vm.TotalDeductions, SocietyDeduction = vm.SocietyDeduction,
                ServiceInMonths = member.ServiceInMonths, DepositDemand = recov.DepositDemand,
                ExistingDemand = existingDemand, AdoptLoanEligibility = scheme.AdoptLoanEligibility == 1
            });
            if (!elig.IsEligible) errors.AddRange(elig.Reasons);

            if (errors.Count > 0) return Fail(errors);

            // --- Build entities (§3.12) ---
            double memberOd = rows.Sum(r => r.ODAmount);
            double suretyOd = recov.SuretyOverdues.Sum(o => o.OverdueAmount);

            var sanction = new Loan_Sanction
            {
                Loan_Id = 0, Voc_Id = 0,
                Mem_Id = vm.Mem_Id, SuretyMem_Id = vm.SuretyMem_Id, Scheme_Id = vm.Scheme_Id, Inst_Type = scheme.Inst_Type,
                SanctionDate = date,
                LoanAmount = vm.LoanAmount, PeriodOfLoan = vm.PeriodOfLoan, RateOfInterest = vm.RateOfInterest,
                PenalRate = vm.PenalRate, InstalmentAmount = vm.InstalmentAmount, FirstDueDate = vm.FirstDueDate!.Value.Date,
                BasicPay = vm.BasicPay, GrossPay = vm.GrossPay, SocietyDeduction = vm.SocietyDeduction,
                TotalDeductions = vm.TotalDeductions, NetPay = vm.GrossPay - vm.TotalDeductions,
                MemberShareCapitalAmount = ToInt(recov.MemberSC), SuretyShareCapitalAmount = ToInt(recov.SuretySC),
                MemberTD = ToInt(recov.MemberTD), FWD = recov.MemberFWD, DA = 0, Lumpsum = 0,
                ThisLoanLimit = ToInt(scheme.MaximumLoanAmount), MemberTotalLimit = ToInt(map?.MaxLoanLimit ?? 0),
                NoOfInstalmentsCompleted = recov.NoOfInstalmentsCompleted, NoOfInstalmentsRecovered = recov.NoOfInstalmentsRecovered,
                TotalReceiptAmount = totalRecoverable,
                IsMemberBecomeDefaulter = memberOd > 0, IsSurityBecomeDefaulter = suretyOd > 0,
                SL_PrlOS = rows.Where(r => r.Status == "P" && r.Scheme_Id == vm.Scheme_Id).Sum(r => r.Outstanding),
                LoanSanction_Status = "E", LoanSanction_Delete = false,
                Usr_Id = ctx.UsrId, Yr_Id = ctx.YrId, BrCode = brCode, Voc_Status = "V"
            };

            var eligibility = new Loan_Eligibility
            {
                LoanElig_Date = date, Loan_Id = 0, Mem_Id = vm.Mem_Id,
                GrossPay = vm.GrossPay, SocietyDeductions = vm.SocietyDeduction, OtherThanSocietyDeductions = elig.OtherDeductions,
                TotalDeductions = vm.TotalDeductions, NetPay = vm.GrossPay - vm.TotalDeductions,
                TotalPrlOS = rows.Where(r => r.Status == "P").Sum(r => r.Outstanding),
                TotalIntOS = rows.Where(r => r.Status == "I" || r.Status == "PI").Sum(r => r.Outstanding),
                ShareCapitalDue = rows.Where(r => r.Status == "MS").Sum(r => r.Outstanding),
                ShareCapitalSurityDue = rows.Where(r => r.Status == "SS").Sum(r => r.Outstanding),
                ThriftDepositDue = rows.Where(r => r.Status == "D" && r.DM_Id == (map?.ThriftDeposit_DMId ?? -1)).Sum(r => r.Outstanding),
                OtherDues = rows.Where(r => r.Status is "DTo" or "DBy" or "G").Sum(r => r.Outstanding),
                TwentyFiveTimesOfGP = elig.TwentyFiveTimesOfGross, FiftyPercentOnGP = elig.FiftyPercentOfGross,
                TwentyFivePercentOnGP = elig.TwentyFivePercentOfGross, DemandAmt = elig.Demand, NetPayAfterDemand = elig.NetPay,
                LoanAppliedAmt = vm.LoanAmount, PeriodOfLoan = vm.PeriodOfLoan,
                IsMemberBecomeDefaulter = memberOd > 0, IsSurityBecomeDefaulter = suretyOd > 0,
                LoanElig_Delete = false, Voc_Id = 0, Usr_Id = (int)ctx.UsrId, Yr_Id = (int)ctx.YrId, BrCode = brCode
            };

            var payments = new (string desc, double amount)[]
            {
                ("25 Times of Gross Pay", elig.TwentyFiveTimesOfGross),
                ("50 Percent of Salary", elig.FiftyPercentOfGross),
                ("25 Percent of Salary", elig.TwentyFivePercentOfGross),
                ("Society Demand", elig.Demand),
                ("Other Deductions", elig.OtherDeductions),
                ("Net Pay", elig.NetPay),
            };

            var lines = new List<Loan_Sanction_Trn>();
            int lineCount = Math.Max(rows.Count, payments.Length);
            for (int i = 0; i < lineCount; i++)
            {
                var line = new Loan_Sanction_Trn
                {
                    SlNo = i + 1, Status = "", Mem_Id = vm.Mem_Id,
                    LoanSanctionTr_Delete = false, Usr_Id = ctx.UsrId, Yr_Id = ctx.YrId, BrCode = brCode, Voc_Status = "V"
                };
                if (i < rows.Count)
                {
                    var r = rows[i];
                    line.Loan_Id = r.Loan_Id;
                    line.RecScheme_Id = r.Scheme_Id;
                    line.DM_Id = r.DM_Id;
                    line.Status = r.Status;
                    line.Mem_Id = r.Mem_Id > 0 ? r.Mem_Id : vm.Mem_Id;
                    line.Led_Id = r.Led_Id;
                    line.ReceiptDescription = r.Ledger_Name;
                    line.ReceiptItem = r.Outstanding;
                    line.CalcAmount = r.CalcAmount;
                    line.ODAmount = r.ODAmount;
                    line.Loan_SlNo = r.Loan_SlNo;
                }
                if (i < payments.Length)
                {
                    line.Payment_Id = i + 1;
                    line.PaymentDescription = payments[i].desc;
                    line.PaymentItem = payments[i].amount;
                }
                lines.Add(line);
            }

            // --- Persist in one transaction. Modify soft-deletes the old sanction and issues a new id. ---
            _unitOfWork.BeginTransaction();
            try
            {
                if (isModify) await repo.SoftDeleteSanctionAsync(vm.LoanSanction_Id, brCode);
                var (newId, eligId) = await repo.AddSanctionAsync(eligibility, sanction, lines);
                await _unitOfWork.CompleteAsync();
                _unitOfWork.CommitTransaction();
                return new SanctionSaveResultVM
                {
                    Success = true, LoanSanction_Id = newId, LoanElig_Id = eligId,
                    Message = isModify
                        ? $"Loan sanction modified. New sanction id {newId} (old id {vm.LoanSanction_Id} cancelled)."
                        : $"Loan sanction saved. Sanction id {newId}."
                };
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw new InvalidOperationException(ex.Message + " Something went wrong! Sanction not saved.");
            }
        }

        /// <summary>
        /// Matches every client row to a server-computed source and takes ledger, loan and snapshot fields
        /// from the server. Only the recovered amount comes from the client, and it is capped at the source.
        /// Manual rows get their status from the ledger (§3.8).
        /// </summary>
        private async Task<List<DeductableLoanRowVM>> ValidateRowsAsync(SuretyLoanSanctionSaveVM vm, EcsRecoverablesVM recov,
            string brCode, List<string> errors)
        {
            var result = new List<DeductableLoanRowVM>();
            foreach (var r in vm.DeductionRows.Where(x => x.Outstanding > 0 || x.CalcAmount > 0))
            {
                double amount = Math.Round(r.Outstanding, 2);
                if (amount < 0) { errors.Add("Recoverable amounts cannot be negative."); continue; }

                if (r.IsManual)
                {
                    if (r.Led_Id <= 0) { errors.Add("A manual deduction has no ledger."); continue; }
                    var resolved = await ResolveDeductionAsync(r.Led_Id, brCode);
                    resolved.Mem_Id = vm.Mem_Id;
                    resolved.Outstanding = amount;
                    result.Add(resolved);
                    continue;
                }

                DeductableLoanRowVM? source = null;
                double cap = 0;
                switch (r.Status)
                {
                    case "P":
                    case "I":
                    case "PI":
                        source = recov.Recoverables.FirstOrDefault(s => s.Loan_Id == r.Loan_Id && s.Status == r.Status);
                        if (source != null) { cap = source.Outstanding; break; }
                        var nd = recov.NonDeductable.FirstOrDefault(n => n.LoanId == r.Loan_Id);
                        if (nd != null)
                        {
                            (decimal led, string? name, cap) = r.Status switch
                            {
                                "P" => (nd.PrlLedId, nd.PrlLedName, nd.LoanOS),
                                "I" => (nd.IntLedId, nd.IntLedName, nd.IntBal),
                                _ => (nd.PILedId, nd.PILedName, nd.PIBal)
                            };
                            source = new DeductableLoanRowVM
                            {
                                Loan_Id = nd.LoanId, Loan_No = nd.LoanNo, Status = r.Status, Mem_Id = vm.Mem_Id,
                                Led_Id = led, Ledger_Name = name, Scheme_Id = nd.SchemeId, Loan_SlNo = nd.LoanSlNo,
                                ODAmount = r.Status == "P" ? nd.PrlOD : 0, RowType = "NonDeductable"
                            };
                        }
                        break;
                    case "DTo":
                        source = recov.Recoverables.FirstOrDefault(s => s.Status == "DTo" && s.Led_Id == r.Led_Id);
                        cap = source?.Outstanding ?? 0;
                        break;
                    case "MS":
                    case "SS":
                        source = recov.Recoverables.FirstOrDefault(s => s.Status == r.Status);
                        cap = source?.Outstanding ?? 0;
                        break;
                    case "D":
                        var pd = recov.PendingDeposits.FirstOrDefault(p => p.DMId == r.DM_Id);
                        if (pd != null)
                        {
                            cap = pd.PendingAmount;
                            source = new DeductableLoanRowVM
                            {
                                Status = "D", DM_Id = pd.DMId, Mem_Id = vm.Mem_Id, Led_Id = pd.LedId,
                                Ledger_Name = pd.LedName, RowType = "Pending"
                            };
                        }
                        break;
                }

                if (source == null)
                {
                    errors.Add($"Recoverable '{r.Ledger_Name}' ({r.Status}) no longer matches the member's balances. Get recoverables again.");
                    continue;
                }
                if (amount > cap + Tolerance)
                {
                    errors.Add($"Recovery for '{source.Ledger_Name}' ({amount:0.00}) exceeds the outstanding {cap:0.00}.");
                    continue;
                }
                result.Add(new DeductableLoanRowVM
                {
                    Loan_Id = source.Loan_Id, Loan_No = source.Loan_No, DM_Id = source.DM_Id, Status = source.Status,
                    Mem_Id = source.Mem_Id, Led_Id = source.Led_Id, Ledger_Name = source.Ledger_Name,
                    Outstanding = amount, ODAmount = Math.Min(source.ODAmount, amount), CalcAmount = source.CalcAmount,
                    Scheme_Id = source.Scheme_Id, Loan_SlNo = source.Loan_SlNo, MatchSC = source.MatchSC,
                    RowType = source.RowType
                });
            }
            return result;
        }

        // =====================================================================
        // Delete / list / get / report
        // =====================================================================

        public async Task<SanctionSaveResultVM> DeleteAsync(decimal sanctionId, EcsUserContextVM ctx)
        {
            var repo = _unitOfWork.EcsLoanSanction;
            var header = await repo.GetSanctionHeaderAsync(sanctionId, ctx.BrCode);
            if (header == null) return Fail("Sanction not found.");
            if (header.LoanSanction_Status != "E") return Fail("Only sanctions awaiting disbursement can be deleted.");
            var subsequent = await repo.GetSubsequentTransactionMessagesAsync(sanctionId, ctx.BrCode);
            if (subsequent.Count > 0) return Fail(subsequent);

            _unitOfWork.BeginTransaction();
            try
            {
                await repo.SoftDeleteSanctionAsync(sanctionId, ctx.BrCode);
                await _unitOfWork.CompleteAsync();
                _unitOfWork.CommitTransaction();
                return new SanctionSaveResultVM { Success = true, LoanSanction_Id = sanctionId, Message = $"Loan sanction {sanctionId} deleted." };
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw new InvalidOperationException(ex.Message + " Something went wrong! Sanction not deleted.");
            }
        }

        public Task<List<EcsSanctionListItemVM>> ListAsync(EcsSanctionListFilterVM filter, string brCode)
            => _unitOfWork.EcsLoanSanction.GetSanctionListAsync(filter, brCode);

        public async Task<EcsSanctionDetailVM?> GetAsync(decimal sanctionId, string brCode)
        {
            var repo = _unitOfWork.EcsLoanSanction;
            var h = await repo.GetSanctionHeaderAsync(sanctionId, brCode);
            if (h == null) return null;
            var lines = await repo.GetSanctionLinesAsync(sanctionId, brCode);
            var loanNos = await repo.GetLoanNumbersAsync(lines.Select(l => l.Loan_Id), brCode);
            var asOn = h.SanctionDate ?? DateTime.Today;
            var member = await repo.GetMemberByIdAsync(h.Mem_Id, asOn, brCode);

            var recoverables = lines.Where(l => l.ReceiptItem > 0 || !string.IsNullOrEmpty(l.Status))
                .Select(l => new DeductableLoanRowVM
                {
                    Loan_Id = l.Loan_Id, Loan_No = loanNos.GetValueOrDefault(l.Loan_Id), DM_Id = l.DM_Id, Status = l.Status,
                    Mem_Id = l.Mem_Id, Led_Id = l.Led_Id, Ledger_Name = l.ReceiptDescription, Outstanding = l.ReceiptItem,
                    CalcAmount = l.CalcAmount, ODAmount = l.ODAmount, Scheme_Id = l.RecScheme_Id, Loan_SlNo = l.Loan_SlNo,
                    MatchSC = l.Status is "MS" or "SS",
                    IsManual = l.Status is "G" or "DBy",
                    RowType = l.Status switch
                    {
                        "P" or "I" or "PI" => "Deductable", "MS" or "SS" => "ShareCapital", "DTo" => "DueTo",
                        "D" => "Pending", _ => "Manual"
                    }
                }).ToList();

            return new EcsSanctionDetailVM
            {
                LoanSanction_Id = h.LoanSanction_Id,
                SanctionDate = h.SanctionDate,
                TotalReceiptAmount = h.TotalReceiptAmount,
                Member = member,
                Recoverables = recoverables,
                Sanction = new SuretyLoanSanctionSaveVM
                {
                    LoanSanction_Id = h.LoanSanction_Id, Mem_Id = h.Mem_Id, SuretyMem_Id = h.SuretyMem_Id,
                    Scheme_Id = h.Scheme_Id, LoanAmount = h.LoanAmount, PeriodOfLoan = h.PeriodOfLoan,
                    RateOfInterest = h.RateOfInterest, PenalRate = h.PenalRate, InstalmentAmount = h.InstalmentAmount,
                    FirstDueDate = h.FirstDueDate, BasicPay = h.BasicPay, GrossPay = h.GrossPay,
                    SocietyDeduction = h.SocietyDeduction, TotalDeductions = h.TotalDeductions,
                    DeductionRows = recoverables
                }
            };
        }

        public async Task<byte[]?> GetOfficeNotePdfAsync(decimal sanctionId, string brCode, Stream reportStream,
            Dictionary<string, string> parameters)
        {
            var rows = await _unitOfWork.EcsLoanSanction.GetOfficeNoteAsync(sanctionId, brCode);
            if (rows.Count == 0) return null;

            var header = await _unitOfWork.EcsLoanSanction.GetSanctionHeaderAsync(sanctionId, brCode);
            var scheme = header == null ? null : await _unitOfWork.EcsLoanSanction.GetSchemeAsync(header.Scheme_Id, brCode);
            var first = rows[0];
            double byCheque = first.LoanAmount - first.TotalReceiptAmount;

            parameters["paramReportHeader"] = "OFFICE NOTE - " + (scheme?.Scheme_Name ?? "");
            parameters["paramNotes"] =
                $"Received Loan of Rs.{first.LoanAmount:0.00}/- {_utilityHandler.RupeesInWords(byCheque)} " +
                $"by way of cheque Rs.{byCheque:0.00}/- and by adjustment Rs.{first.TotalReceiptAmount:0.00}/-";
            return _createReportsHandler.CreateLocalReport("Ds_OfficeNote", reportStream, rows, parameters);
        }

        // =====================================================================

        private static int ToInt(double v) => (int)Math.Round(v, MidpointRounding.AwayFromZero);

        private static SanctionSaveResultVM Fail(string message) =>
            new() { Success = false, Message = message, Reasons = new List<string> { message } };

        private static SanctionSaveResultVM Fail(List<string> messages) =>
            new() { Success = false, Message = string.Join(" ", messages), Reasons = messages };
    }
}
