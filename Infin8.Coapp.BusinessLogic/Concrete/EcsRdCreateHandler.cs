using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using Infin8.Coapp.Utility.Enums;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>
    /// ECS RD New Account, acc 17 (spec 15). Loads the Maker screen's setup and, at approval, recomputes the staged
    /// opening with <see cref="EcsRdCreateCalculator"/> and builds the termdeposit_master / termdeposit_members /
    /// termdeposit_trn / fin_voucher_trn rows.
    /// </summary>
    public class EcsRdCreateHandler : IEcsRdCreateHandler
    {
        readonly IUnitOfWork _unitOfWork;

        public EcsRdCreateHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =====================================================================
        // Maker screen (spec 15 §4, §5, §8)
        // =====================================================================

        public async Task<EcsRdCreateSetupVM> GetSetupAsync(decimal memId, string brCode, DateTime currentDate)
        {
            var day = currentDate.Date;
            var result = new EcsRdCreateSetupVM { ApplicationDate = day };

            var openDay = await _unitOfWork.EcsLoanDisbursement.GetOpenBusinessDayAsync(brCode);
            if (openDay?.Date != day)
            {
                result.Message = "The business day has changed since you logged in (open business day: " +
                                 (openDay.HasValue ? openDay.Value.ToString("dd-MM-yyyy") : "none") + "). Log in again.";
                return result;
            }

            var member = await _unitOfWork.EcsRdCreate.GetMemberByIdAsync(memId, brCode);
            var inactive = InactiveMessage(member);
            if (inactive != null)
            {
                result.Message = inactive;
                return result;
            }
            result.FirstHolder = ToHolder(member!, day, 1);

            var schemes = await _unitOfWork.EcsRdCreate.GetRdSchemesAsync(brCode);
            if (schemes.Count == 0)
            {
                result.Message = "No recurring deposit scheme is defined for this branch.";
                return result;
            }
            result.Schemes = schemes.Select(s => new DtoEcsRdScheme
            {
                TDScheme_Id = s.TDScheme_Id,
                TDScheme_Name = s.TDScheme_Name?.Trim(),
                MinimumPeriod = s.MinimumPeriod,
                MaximumPeriod = s.MaximumPeriod
            }).ToList();
            result.RoiRows = await _unitOfWork.EcsRdCreate.GetRoiRowsAsync(schemes.Select(s => s.TDScheme_Id), brCode);

            // Display hint only: the number is allocated at approval (§5.3).
            var first = schemes[0].TDScheme_Id;
            result.ProvisionalTD_No = EcsRdCreateCalculator.NextTdNo(brCode, first,
                await _unitOfWork.EcsRdCreate.GetTdNosWithPrefixAsync(EcsRdCreateCalculator.TdNoPrefix(brCode, first), brCode));

            result.LastNominee = await _unitOfWork.EcsRdCreate.GetLastRdNomineeAsync(memId, brCode);
            return result;
        }

        public async Task<EcsRdHolderLookupVM> GetHolderAsync(string memberNo, string brCode, DateTime businessDay)
        {
            if (string.IsNullOrWhiteSpace(memberNo))
                return new EcsRdHolderLookupVM { Message = "Enter a member number" };
            var member = await _unitOfWork.EcsRdCreate.GetMemberByNoAsync(memberNo, brCode);
            if (member == null)
                return new EcsRdHolderLookupVM { Message = $"Member No {memberNo.Trim()} is not found" };
            var inactive = InactiveMessage(member);
            if (inactive != null) return new EcsRdHolderLookupVM { Message = inactive };
            return new EcsRdHolderLookupVM { Holder = ToHolder(member, businessDay.Date, 0) };
        }

        public async Task<EcsRdCreateCalcVM> CalculateAsync(DtoEcsRdCreate dto, string brCode, DateTime businessDay)
        {
            var result = new EcsRdCreateCalcVM { Dto = dto };
            var scheme = await _unitOfWork.EcsRdCreate.GetRdSchemeAsync(dto.TDScheme_Id, brCode);
            if (scheme == null)
            {
                result.Message = "RD scheme is not selected";
                return result;
            }
            dto.TDScheme_Name = scheme.TDScheme_Name?.Trim();
            var rows = await _unitOfWork.EcsRdCreate.GetRoiRowsAsync(new[] { dto.TDScheme_Id }, brCode);
            var calc = EcsRdCreateCalculator.Recompute(dto, rows);
            result.Message = calc ?? await CheckHoldersAsync(dto, brCode) ?? EcsRdCreateCalculator.Validate(dto, businessDay, rows);
            return result;
        }

        // =====================================================================
        // Approval posting (spec 15 §8, §10)
        // =====================================================================

        public async Task<EcsRdCreatePosting> BuildPostingAsync(DtoEcsRdCreate staged, decimal stagingId, DateTime stagingDate,
            string brCode, decimal yrId, decimal usrId, decimal vocId, int trnType, string status)
        {
            var appDate = staged.ApplicationDate.Date;

            // --- context checks ---
            var society = await _unitOfWork.General.GetSocietyData(brCode);
            if ((society?.Bank_Type ?? 0).ToInstitutionType() != InstitutionType.ECS)
                throw new EcsRecoveryException("RD New Account can be approved only in an ECS society.");

            var openDay = await _unitOfWork.EcsLoanDisbursement.GetOpenBusinessDayAsync(brCode);
            if (appDate != stagingDate.Date || openDay?.Date != appDate)
                throw new EcsRecoveryException(
                    $"RD opening staged on {appDate:dd-MM-yyyy} must be approved on the same business day " +
                    $"(open business day: {(openDay.HasValue ? openDay.Value.ToString("dd-MM-yyyy") : "none")}). Reject it and re-initiate.");

            // --- recheck (§8) ---
            var scheme = await _unitOfWork.EcsRdCreate.GetRdSchemeAsync(staged.TDScheme_Id, brCode)
                ?? throw new EcsRecoveryException($"RD scheme {staged.TDScheme_Name ?? staged.TDScheme_Id.ToString()} no longer exists. Reject it and re-initiate.");
            string schemeName = scheme.TDScheme_Name?.Trim() ?? scheme.TDScheme_Id.ToString();
            if (scheme.Led_Id == 0)
                throw new EcsRecoveryException($"RD scheme {schemeName} has no ledger. Set it on the scheme first.");

            var rows = await _unitOfWork.EcsRdCreate.GetRoiRowsAsync(new[] { scheme.TDScheme_Id }, brCode);
            var holderError = await CheckHoldersAsync(staged, brCode);
            if (holderError != null) throw new EcsRecoveryException(holderError + ". Reject it and re-initiate.");

            var fresh = new DtoEcsRdCreate
            {
                MemId = staged.MemId,
                TDScheme_Id = scheme.TDScheme_Id,
                TDScheme_Name = schemeName,
                ValueDate = staged.ValueDate,
                Instalment = staged.Instalment,
                PeriodInMonths = staged.PeriodInMonths,
                Holders = staged.Holders
            };
            var roiError = EcsRdCreateCalculator.Recompute(fresh, rows);
            if (roiError != null) throw new EcsRecoveryException(roiError + ". Reject it and re-initiate.");

            if (!EcsRdCreateCalculator.Near(fresh.RateOfInterest, staged.RateOfInterest))
                throw Changed(schemeName, $"rate of interest is now {fresh.RateOfInterest:0.##}% (staged {staged.RateOfInterest:0.##}%)");
            if (!EcsRdCreateCalculator.Near(fresh.PenalRate, staged.PenalRate))
                throw Changed(schemeName, $"penal rate is now {fresh.PenalRate:0.##}% (staged {staged.PenalRate:0.##}%)");
            if (fresh.MaturityDate.Date != staged.MaturityDate.Date)
                throw Changed(schemeName, $"maturity date is now {fresh.MaturityDate:dd-MM-yyyy} (staged {staged.MaturityDate:dd-MM-yyyy})");
            if (!EcsRdCreateCalculator.Near(fresh.MaturityAmount, staged.MaturityAmount))
                throw Changed(schemeName, $"maturity amount is now ₹{fresh.MaturityAmount:N2} (staged ₹{staged.MaturityAmount:N2})");

            var invalid = EcsRdCreateCalculator.Validate(staged, appDate, rows);
            if (invalid != null) throw new EcsRecoveryException(invalid + ". Reject it and re-initiate.");

            // --- holders as on the business day ---
            var members = await _unitOfWork.EcsRdCreate.GetMembersByIdAsync(staged.Holders.Select(h => h.Mem_Id), brCode);
            var holders = staged.Holders.Select((h, i) => ToHolder(members[h.Mem_Id], appDate, i + 1)).ToList();
            string nameOnBond = EcsRdCreateCalculator.NameOnBond(holders);

            // --- RD number (§5.3, settled in §17) ---
            var prefix = EcsRdCreateCalculator.TdNoPrefix(brCode, scheme.TDScheme_Id);
            string tdNo = EcsRdCreateCalculator.NextTdNo(brCode, scheme.TDScheme_Id,
                await _unitOfWork.EcsRdCreate.GetTdNosWithPrefixAsync(prefix, brCode));

            // --- rows (§10) ---
            double instalment = Math.Round(staged.Instalment, 0, MidpointRounding.AwayFromZero);
            var master = new TermDeposit_Master
            {
                TD_Id = 0,                                   // allocated by AddRdAsync
                TD_No = tdNo,
                TDScheme_Id = scheme.TDScheme_Id,
                Mem_Id = staged.MemId,
                TDH_Name = nameOnBond,
                TDH_Age = holders[0].Age,
                ModeOfOperation = staged.ModeOfOperation,
                AccountOpenDate = appDate,
                ValueDate = staged.ValueDate.Date,
                DepositAmount = instalment,
                PeriodInMonths = staged.PeriodInMonths,
                PeriodInDays = 0,
                RateOfInterest = fresh.RateOfInterest,
                MaturityDate = fresh.MaturityDate,
                MaturityAmount = fresh.MaturityAmount,
                LeinAmount = 0,
                IsNomineeProvided = !string.IsNullOrWhiteSpace(staged.Nominee1Name),
                InterestPayableFrequency = 0,
                PenalRate = fresh.PenalRate,
                IsDiscountRate = false,
                IsCompoundInterest = false,
                CompoundFrequency = 0,
                Nominee1Name = Clean(staged.Nominee1Name),
                Nominee1Age = string.IsNullOrWhiteSpace(staged.Nominee1Name) ? 0 : staged.Nominee1Age,
                Nominee1Relationship = string.IsNullOrWhiteSpace(staged.Nominee1Name) ? "" : Clean(staged.Nominee1Relationship),
                Nominee2Name = Clean(staged.Nominee2Name),
                Nominee2Age = string.IsNullOrWhiteSpace(staged.Nominee2Name) ? 0 : staged.Nominee2Age,
                Nominee2Relationship = string.IsNullOrWhiteSpace(staged.Nominee2Name) ? "" : Clean(staged.Nominee2Relationship),
                LienMarked = false,
                Loan_Id = 0,
                TD_OE = false,
                TD_Delete = false,
                AccountClosed = false,
                Voc_Id = vocId,
                Usr_Id = usrId,
                Yr_Id = yrId,
                Status = staged.IncludeInDemand ? "D" : "N",
                RenewalTD_Id = 0,
                RenewalTD_No = "",
                BrCode = brCode,
                Voc_Status = "V"
            };

            var posting = new EcsRdCreatePosting { Master = master };
            posting.Members.AddRange(holders.Select(h => new TermDeposit_Members
            {
                TDMem_Id = 0,                                // allocated by AddRdAsync
                TD_Id = 0,
                Mem_Id = h.Mem_Id,
                Mem_SlNo = h.SlNo,
                TDMem_Delete = false,
                Voc_Id = vocId,
                Usr_Id = usrId,
                Yr_Id = yrId,
                BrCode = brCode,
                Voc_Status = "V"
            }));

            decimal tdId = await _unitOfWork.EcsRdCreate.AddRdAsync(master, posting.Members);

            posting.TermDepositTrns.Add(new TermDeposit_Trn
            {
                TDTrn_Id = 0,                                // assigned by AddTermDepositTrnListAsync (MAX+1)
                Trn_Date = appDate,
                TD_Id = tdId,
                DepositDemandAmount = instalment,
                DepositReceiptAmount = instalment,
                MaturityAmount = fresh.MaturityAmount,
                InterestCalculatedAmount = 0,
                InterestAppliedDate = null,
                InterestPaidAmount = 0,
                DepositPaidAmount = 0,
                PenalCalculatedAmount = 0,
                PenalAppliedDate = null,
                PenalReceivedAmount = 0,
                NoOfInstalments = 1,
                InstalmentAmount = (int)instalment,
                LastInstalmentDate = staged.ValueDate.Date,
                LastPaymentDate = null,
                TD_OE = false,
                TD_Delete = false,
                Voc_Id = vocId,
                Usr_Id = usrId,
                Yr_Id = yrId,
                Trn_SlNo = 0,                                // assigned by AddTermDepositTrnListAsync
                Demand_Id = 0,
                TDCalc_Id = 0,
                BrCode = brCode,
                Voc_Status = "V"
            });

            string desc = "RD No " + tdNo;
            string narr = $"M.No {holders[0].MemberNo?.Trim()} {holders[0].MemberName?.Trim()} {desc}";
            posting.VoucherTrns.Add(GetModalObject.GetFinVoucherTrObject(vocId, scheme.Led_Id, instalment, 0, trnType, narr, false,
                usrId, yrId, status, desc, staged.MemId, brCode, 0, 0, 0));
            return posting;
        }

        // =====================================================================
        // Helpers
        // =====================================================================

        /// <summary>§7.2/§7.3: holders exist in the branch and are active (status 1, not expired, not deleted).</summary>
        private async Task<string?> CheckHoldersAsync(DtoEcsRdCreate dto, string brCode)
        {
            if (dto.Holders.Count == 0 || dto.Holders[0].Mem_Id != dto.MemId) return "Holder details are missing";
            var members = await _unitOfWork.EcsRdCreate.GetMembersByIdAsync(dto.Holders.Select(h => h.Mem_Id), brCode);
            foreach (var h in dto.Holders)
            {
                members.TryGetValue(h.Mem_Id, out var m);
                var msg = m == null ? $"Member No {h.MemberNo?.Trim()} is not a member of this branch" : InactiveMessage(m);
                if (msg != null) return msg;
            }
            return null;
        }

        private static string? InactiveMessage(DtoEcsRdMember? m)
        {
            if (m == null) return "Member is not found in this branch";
            if (m.IsDeleted || m.IsExpired || m.MemberStatus != 1)
                return $"Member No {m.MemberNo?.Trim()} is not an active member";
            return null;
        }

        private static DtoEcsRdCreateHolder ToHolder(DtoEcsRdMember m, DateTime day, int slNo) => new()
        {
            Mem_Id = m.Mem_Id,
            MemberNo = m.MemberNo?.Trim(),
            MemberName = m.MemberName?.Trim(),
            FatherName = m.FatherName?.Trim(),
            Age = EcsRdCreateCalculator.AgeOn(m.Dob, day),
            SlNo = slNo
        };

        private static EcsRecoveryException Changed(string scheme, string what) =>
            new($"RD scheme {scheme}: the {what} — it changed after this opening was initiated. Reject it and re-initiate.");

        private static string Clean(string? s) => s?.Trim() ?? "";
    }
}
