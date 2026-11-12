namespace Infin8.Coapp.Utility.Enums
{
    /// <summary>
    /// Loan product categories (lending sectors). Stored as the integer discriminator <c>Loan_Type</c>
    /// on <c>Loan_Master</c>, <c>Loan_Schemes</c> and <c>Loan_Schemes_Group</c>.
    ///
    /// Values 1-9 MUST NOT change — live data depends on them.
    /// - 1 covers every Thrift &amp; Credit (ECS) product: surety, education, draught, etc. Those
    ///   products are told apart by <c>Scheme_Id</c> and the scheme's rule columns, never by Loan_Type.
    /// - 6 means Farm Sector Loans in a PCARDB and Housing Loans in a Housing Society; always label
    ///   it via <see cref="LoanCategoryPolicy.GetDisplayName"/>, never hard-code the text.
    /// - 10/11/12 (former Surety / Education / Draught) are retired.
    ///
    /// See specs/ecs-extension/07-loan-type-society-type.md.
    /// </summary>
    public enum LoanCategory
    {
        ThriftCreditLoans = 1,
        JewelLoans = 2,
        LoansOnFixedDeposits = 3,
        LoansOnRecurringDeposits = 4,
        StaffLoans = 5,
        FarmSectorOrHousingLoans = 6,
        NonFarmSectorLoans = 7,
        RuralHousing = 8,
        SmallRoadTransportLoans = 9
    }
}
