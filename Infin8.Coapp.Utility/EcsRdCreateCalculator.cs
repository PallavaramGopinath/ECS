using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.Utility
{
    /// <summary>
    /// Pure rules of the ECS RD New Account, acc 17 (spec 15 §4.2, §5.3, §6, §7). Shared by the WASM component and the
    /// server's recheck at approval, so both sides always agree. No DB access.
    /// </summary>
    public static class EcsRdCreateCalculator
    {
        public const double Tolerance = 0.005;
        public const int MaxPeriodMonths = 120;

        /// <summary>RD number layout (spec 15 §17): brCode + one RD digit + 7-digit running number.</summary>
        public const int RunningNoWidth = 7;

        // =====================================================================
        // Mode of operation (§4.2) — the TSIS list, stored as its code.
        // =====================================================================

        public static readonly IReadOnlyList<(int Code, string Name)> ModesOfOperation = new List<(int, string)>
        {
            (1, "Single"),
            (2, "Either or Survivor"),
            (3, "Any one or Survivor/s"),
            (4, "Any two or Survivor/s"),
            (5, "All parents jointly"),
            (6, "Any two jointly"),
            (7, "Former or Survivor"),
            (8, "Latter or Survivor"),
            (9, "By Guardian"),
            (10, "By Mandate"),
            (11, "Power of attorney"),
            (12, "Proprietor")
        };

        public static string ModeName(int code) =>
            ModesOfOperation.FirstOrDefault(m => m.Code == code).Name ?? code.ToString();

        /// <summary>Single needs exactly one holder; 2–8 need two or more; 9–12 any number.</summary>
        public static string? ValidateMode(int code, int holderCount)
        {
            if (!ModesOfOperation.Any(m => m.Code == code)) return "Mode of operation is not selected";
            if (code == 1 && holderCount != 1) return "Mode of operation 'Single' allows only one holder";
            if (code >= 2 && code <= 8 && holderCount < 2) return $"Mode of operation '{ModeName(code)}' needs two or more holders";
            return null;
        }

        public static readonly IReadOnlyList<string> Relationships = new[] { "Husband", "Wife", "Son", "Daughter", "Father", "Mother" };

        // =====================================================================
        // Holders (§4.2)
        // =====================================================================

        /// <summary>"&lt;MemNo&gt; &lt;Name&gt;" for each holder, joined with ", ".</summary>
        public static string NameOnBond(IEnumerable<DtoEcsRdCreateHolder> holders) =>
            string.Join(", ", holders.Select(h => $"{h.MemberNo?.Trim()} {h.MemberName?.Trim()}".Trim()));

        /// <summary>Completed years on <paramref name="onDate"/>; 0 without a date of birth.</summary>
        public static int AgeOn(DateTime? dob, DateTime onDate)
        {
            if (dob == null || dob.Value.Date > onDate.Date) return 0;
            var d = dob.Value.Date;
            int age = onDate.Year - d.Year;
            if (onDate.Date < d.AddYears(age)) age--;
            return Math.Max(age, 0);
        }

        // =====================================================================
        // Rate of interest (§6.1)
        // =====================================================================

        /// <summary>
        /// The scheme's monthly row whose band holds <paramref name="months"/>, latest Wef on or before the value date.
        /// TDHolderType is ignored (§15.6).
        /// </summary>
        public static TermDeposit_Roi_Template? PickRoi(IEnumerable<TermDeposit_Roi_Template> rows, int schemeId, int months, DateTime valueDate) =>
            rows.Where(r => !r.TDRoi_Delete && r.TDScheme_Id == schemeId
                            && string.Equals(r.PeriodType?.Trim(), "M", StringComparison.OrdinalIgnoreCase)
                            && r.PeriodBegin <= months && months <= r.PeriodEnd
                            && r.Wef.Date <= valueDate.Date)
                .OrderBy(r => r.Wef)
                .ThenBy(r => r.Roi_Id)
                .LastOrDefault();

        /// <summary>
        /// Periods offered (spec 15 §17): multiples of 12 up to 120 that have an ROI row with ROI &gt; 0. The scheme's
        /// Min/Max period is ignored, as TSIS did.
        /// </summary>
        public static List<int> AllowedPeriods(IEnumerable<TermDeposit_Roi_Template> rows, int schemeId, DateTime valueDate)
        {
            var list = rows.ToList();
            var result = new List<int>();
            for (int m = 12; m <= MaxPeriodMonths; m += 12)
            {
                var r = PickRoi(list, schemeId, m, valueDate);
                if (r != null && r.Roi > 0) result.Add(m);
            }
            return result;
        }

        // =====================================================================
        // Maturity (§6.2, §6.3)
        // =====================================================================

        public static DateTime MaturityDate(DateTime valueDate, int months) =>
            EcsRdReceiptAppropriator.AddMonthsKeepEnd(valueDate.Date, months);

        /// <summary>
        /// TSIS branch (a): simple interest on the running balance month by month, each month's interest rounded
        /// half-up to a whole rupee. Computed in decimal so x.5 rounds the same everywhere.
        /// </summary>
        public static double MaturityAmount(double instalment, int months, double roi)
        {
            if (instalment <= 0 || months <= 0) return 0;
            decimal d = (decimal)instalment;
            decimal rate = (decimal)roi;
            decimal balance = 0, interest = 0;
            for (int i = 1; i <= months; i++)
            {
                balance += d;
                interest += Math.Round(balance * rate / 1200m, 0, MidpointRounding.AwayFromZero);
            }
            return (double)(d * months + interest);
        }

        /// <summary>
        /// Fills ROI, penal rate, maturity date/amount and name on bond of <paramref name="dto"/> from the rows. Returns
        /// the §6.1 message when no usable ROI row exists (figures then zero).
        /// </summary>
        public static string? Recompute(DtoEcsRdCreate dto, IEnumerable<TermDeposit_Roi_Template> rows)
        {
            dto.NameOnBond = NameOnBond(dto.Holders);
            dto.RateOfInterest = 0;
            dto.PenalRate = 0;
            dto.MaturityAmount = 0;
            dto.MaturityDate = dto.PeriodInMonths > 0 ? MaturityDate(dto.ValueDate, dto.PeriodInMonths) : dto.ValueDate.Date;
            if (dto.TDScheme_Id <= 0) return "RD scheme is not selected";
            if (dto.PeriodInMonths <= 0) return "Period is not selected";

            var row = PickRoi(rows, dto.TDScheme_Id, dto.PeriodInMonths, dto.ValueDate);
            if (row == null || row.Roi <= 0)
                return $"Rate of interest is not defined for {dto.TDScheme_Name?.Trim() ?? dto.TDScheme_Id.ToString()} for {dto.PeriodInMonths} months";

            dto.RateOfInterest = row.Roi;
            dto.PenalRate = row.PenalRateForRD;
            dto.MaturityAmount = MaturityAmount(dto.Instalment, dto.PeriodInMonths, row.Roi);
            return null;
        }

        // =====================================================================
        // Validation (§7) — every rule except member status (server) and one-line-per-voucher (approval).
        // =====================================================================

        /// <summary>
        /// Checks <paramref name="dto"/> as staged. Returns the first message, or null when valid. Does not modify the
        /// dto: the maturity figures are compared with a fresh recomputation (§7.10).
        /// </summary>
        public static string? Validate(DtoEcsRdCreate dto, DateTime businessDay, IEnumerable<TermDeposit_Roi_Template> rows)
        {
            var rowList = rows.ToList();
            if (dto.TDScheme_Id <= 0) return "RD scheme is not selected";

            if (dto.Holders.Count == 0 || dto.Holders[0].Mem_Id != dto.MemId) return "Holder details are missing";
            if (dto.Holders.Select(h => h.Mem_Id).Distinct().Count() != dto.Holders.Count) return "A member is listed twice as a holder";

            var mode = ValidateMode(dto.ModeOfOperation, dto.Holders.Count);
            if (mode != null) return mode;

            if (dto.Instalment <= 0) return "Instalment amount is missing";
            if (Math.Abs(dto.Instalment - Math.Round(dto.Instalment)) > Tolerance) return "Instalment must be in whole rupees";

            if (dto.PeriodInMonths <= 0 || dto.PeriodInMonths % 12 != 0 || dto.PeriodInMonths > MaxPeriodMonths)
                return "Period must be a multiple of 12 months";

            var day = businessDay.Date;
            if (dto.ValueDate.Date > day || dto.ValueDate.Year != day.Year || dto.ValueDate.Month != day.Month)
                return "Value date must be in the current month and not after the business day";

            var fresh = Clone(dto);
            var roi = Recompute(fresh, rowList);
            if (roi != null) return roi;

            if (!string.IsNullOrWhiteSpace(dto.Nominee1Name) && (dto.Nominee1Age < 0 || dto.Nominee1Age > 120)) return "Nominee age is invalid";
            if (!string.IsNullOrWhiteSpace(dto.Nominee2Name))
            {
                if (string.IsNullOrWhiteSpace(dto.Nominee1Name)) return "Enter Nominee 1 before Nominee 2";
                if (dto.Nominee2Age < 0 || dto.Nominee2Age > 120) return "Nominee age is invalid";
            }

            if (!Near(fresh.RateOfInterest, dto.RateOfInterest) || !Near(fresh.PenalRate, dto.PenalRate)
                || fresh.MaturityDate.Date != dto.MaturityDate.Date || !Near(fresh.MaturityAmount, dto.MaturityAmount))
                return "Maturity figures are out of date; press Calculate";
            return null;
        }

        // =====================================================================
        // RD number (§5.3 as settled in §17)
        // =====================================================================

        /// <summary>brCode + the scheme's first digit (4 for RD scheme 40001).</summary>
        public static string TdNoPrefix(string brCode, int schemeId) =>
            brCode.Trim() + schemeId.ToString()[0];

        /// <summary>
        /// Next number in the series: numeric MAX of the existing 13-digit numbers with the prefix + 1, or prefix +
        /// 0000001 for the first.
        /// </summary>
        public static string NextTdNo(string brCode, int schemeId, IEnumerable<string?> existing)
        {
            var prefix = TdNoPrefix(brCode, schemeId);
            long max = 0;
            foreach (var raw in existing)
            {
                var s = raw?.Trim();
                if (s == null || s.Length != prefix.Length + RunningNoWidth || !s.StartsWith(prefix, StringComparison.Ordinal)) continue;
                if (!s.All(char.IsDigit)) continue;
                long n = long.Parse(s.Substring(prefix.Length));
                if (n > max) max = n;
            }
            return prefix + (max + 1).ToString("D" + RunningNoWidth);
        }

        public static bool Near(double x, double y) => Math.Abs(x - y) <= Tolerance;

        private static DtoEcsRdCreate Clone(DtoEcsRdCreate d) => new()
        {
            MemId = d.MemId,
            TDScheme_Id = d.TDScheme_Id,
            TDScheme_Name = d.TDScheme_Name,
            ValueDate = d.ValueDate,
            Instalment = d.Instalment,
            PeriodInMonths = d.PeriodInMonths,
            Holders = d.Holders
        };
    }
}
