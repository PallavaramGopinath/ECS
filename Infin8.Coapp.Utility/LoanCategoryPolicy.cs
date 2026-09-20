using System.Collections.Generic;
using System.Linq;

namespace Infin8.Coapp.Utility.Enums
{
    /// <summary>
    /// Single source of truth for which <see cref="LoanCategory"/> values apply to which
    /// <see cref="InstitutionType"/>. The exclusive sets below are the editable policy
    /// referenced by specs/ecs-extension/02-loan-module-design.md.
    ///
    /// Safety bias: anything that is NOT explicitly ECS is treated as the incumbent PCARDB,
    /// so existing PCARDB visibility never regresses if society type cannot be determined.
    /// </summary>
    public static class LoanCategoryPolicy
    {
        /// <summary>Loan products only PCARDB (Agriculture &amp; Rural Development) offers.</summary>
        private static readonly HashSet<LoanCategory> PcardbExclusive = new()
        {
            LoanCategory.FarmSectorLoans,
            LoanCategory.NonFarmSectorLoans,
            LoanCategory.RuralHousing,
            LoanCategory.SmallRoadTransportLoans
        };

        /// <summary>Loan products only ECS (Employees' Thrift &amp; Credit) offers.</summary>
        private static readonly HashSet<LoanCategory> EcsExclusive = new()
        {
            LoanCategory.SuretyLoan,
            LoanCategory.EducationLoan,
            LoanCategory.DraughtLoan
        };

        /// <summary>
        /// True if <paramref name="category"/> should be available to <paramref name="institution"/>.
        /// ECS sees the shared core plus ECS-exclusive products; every other institution
        /// (PCARDB or Unknown) sees the shared core plus PCARDB-exclusive products.
        /// </summary>
        public static bool IsAllowed(LoanCategory category, InstitutionType institution) =>
            institution == InstitutionType.ECS
                ? !PcardbExclusive.Contains(category)
                : !EcsExclusive.Contains(category);

        /// <summary>All loan categories available to the given institution type.</summary>
        public static IEnumerable<LoanCategory> GetCategoriesFor(InstitutionType institution) =>
            System.Enum.GetValues<LoanCategory>().Where(c => IsAllowed(c, institution));
    }
}
