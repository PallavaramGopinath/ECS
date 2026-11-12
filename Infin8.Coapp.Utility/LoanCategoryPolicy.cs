using System.Collections.Generic;
using System.Linq;

namespace Infin8.Coapp.Utility.Enums
{
    /// <summary>
    /// Single source of truth for which <see cref="LoanCategory"/> values each
    /// <see cref="InstitutionType"/> may offer, and how they are labelled.
    /// The allow-list below is the editable policy from specs/ecs-extension/07-loan-type-society-type.md §4.
    ///
    /// Safety bias: Unknown (bank_type 0 / blank / unrecognised) is treated as PCARDB, so existing
    /// PCARDB visibility never regresses if society type cannot be determined.
    /// </summary>
    public static class LoanCategoryPolicy
    {
        private static readonly Dictionary<InstitutionType, HashSet<LoanCategory>> Allowed = new()
        {
            // Bank_Type 1: Cooperative Primary Agriculture and Rural Development Bank
            [InstitutionType.PCARDB] = new()
            {
                LoanCategory.JewelLoans,                 // 2
                LoanCategory.LoansOnFixedDeposits,       // 3
                LoanCategory.LoansOnRecurringDeposits,   // 4
                LoanCategory.StaffLoans,                 // 5
                LoanCategory.FarmSectorOrHousingLoans,   // 6 (Farm Sector)
                LoanCategory.NonFarmSectorLoans,         // 7
                LoanCategory.RuralHousing,               // 8
                LoanCategory.SmallRoadTransportLoans     // 9
            },

            // Bank_Type 2: Cooperative Housing Society
            [InstitutionType.Housing] = new()
            {
                LoanCategory.JewelLoans,                 // 2
                LoanCategory.LoansOnFixedDeposits,       // 3
                LoanCategory.LoansOnRecurringDeposits,   // 4
                LoanCategory.StaffLoans,                 // 5
                LoanCategory.FarmSectorOrHousingLoans    // 6 (Housing)
            },

            // Bank_Type 3: Cooperative Thrift & Credit Society
            [InstitutionType.ECS] = new()
            {
                LoanCategory.ThriftCreditLoans,          // 1 (surety, education, draught … by Scheme_Id)
                LoanCategory.JewelLoans,                 // 2
                LoanCategory.LoansOnFixedDeposits,       // 3
                LoanCategory.LoansOnRecurringDeposits,   // 4
                LoanCategory.StaffLoans                  // 5
            }
        };

        private static InstitutionType PolicyKey(InstitutionType institution) =>
            Allowed.ContainsKey(institution) ? institution : InstitutionType.PCARDB;

        /// <summary>True if <paramref name="institution"/> may offer <paramref name="category"/>.</summary>
        public static bool IsAllowed(LoanCategory category, InstitutionType institution) =>
            Allowed[PolicyKey(institution)].Contains(category);

        /// <summary>All loan categories available to the given institution type.</summary>
        public static IEnumerable<LoanCategory> GetCategoriesFor(InstitutionType institution) =>
            System.Enum.GetValues<LoanCategory>().Where(c => IsAllowed(c, institution));

        /// <summary>
        /// Display label for a loan category. <c>Loan_Type 6</c> depends on the society type
        /// (Housing Loans for a Housing Society, Farm Sector Loans otherwise).
        /// </summary>
        public static string GetDisplayName(LoanCategory category, InstitutionType institution) => category switch
        {
            LoanCategory.ThriftCreditLoans => "Thrift & Credit Society Loans",
            LoanCategory.JewelLoans => "Jewel Loans",
            LoanCategory.LoansOnFixedDeposits => "Loans on Fixed Deposits",
            LoanCategory.LoansOnRecurringDeposits => "Loans on Recurring Deposits",
            LoanCategory.StaffLoans => "Staff Loans",
            LoanCategory.FarmSectorOrHousingLoans =>
                PolicyKey(institution) == InstitutionType.Housing ? "Housing Loans" : "Farm Sector Loans",
            LoanCategory.NonFarmSectorLoans => "Non-Farm Sector Loans",
            LoanCategory.RuralHousing => "Rural Housing Loans",
            LoanCategory.SmallRoadTransportLoans => "Small Road Transport Loans",
            _ => "Not Specified"
        };

        /// <summary><see cref="GetDisplayName(LoanCategory, InstitutionType)"/> for a raw stored <c>Loan_Type</c>.</summary>
        public static string GetDisplayName(int? loanType, InstitutionType institution) =>
            loanType.HasValue && System.Enum.IsDefined(typeof(LoanCategory), loanType.Value)
                ? GetDisplayName((LoanCategory)loanType.Value, institution)
                : "Not Specified";
    }
}
