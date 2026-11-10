namespace Infin8.Coapp.Utility.Enums
{
    /// <summary>
    /// Loan product categories. Stored as the integer discriminator <c>Loan_Type</c> on
    /// <c>Loan_Master</c>, <c>Loan_Schemes</c> and <c>Loan_Schemes_Group</c>.
    ///
    /// Values 1-9 are pre-existing and MUST NOT change — live PCARDB data depends on them.
    /// Values 10-12 are the ECS-exclusive products added by the ECS extension.
    ///
    /// This enum was previously duplicated privately inside Loanproducts.razor and
    /// LoanProductsCreate.razor; it is promoted here as the single source of truth.
    /// See specs/ecs-extension/02-loan-module-design.md.
    /// </summary>
    public enum LoanCategory
    {
        EmployeeSocietyLoans = 1,
        JewelLoans = 2,
        LoansOnFixedDeposits = 3,
        LoansOnRecurringDeposits = 4,
        StaffLoans = 5,
        FarmSectorLoans = 6,
        NonFarmSectorLoans = 7,
        RuralHousing = 8,
        SmallRoadTransportLoans = 9,

        // ---- ECS-exclusive loan products (added by the ECS extension) ----
        SuretyLoan = 10,
        EducationLoan = 11,
        DraughtLoan = 12
    }
}
