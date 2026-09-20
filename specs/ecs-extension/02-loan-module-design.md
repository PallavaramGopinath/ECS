# 02 — Loan Module Design (ECS Extension)

> Builds on [01-institution-type.md](./01-institution-type.md). Each loan type has its own spec:
> [03-surety-loan.md](./03-surety-loan.md), [04-education-loan.md](./04-education-loan.md),
> [05-draught-loan.md](./05-draught-loan.md).

## Decision: extend the existing `Loan_Type` discriminator ("Option A")

The codebase already models all loans in a **single `Loan_Master` table** distinguished by an integer
`Loan_Type`, with type-specific data attached via FK (`JL_Details`, `Lien`, …) and loan products
configured as rows in `Loan_Schemes` + `Loan_Schemes_Group` (also keyed by `Loan_Type`). PCARDB
already layered sector products (FarmSector=6 … SmallRoadTransport=9) onto this same engine.

Therefore the three ECS products are added as **new `Loan_Type` discriminator values**, not a new
table hierarchy. This is:

- **Consistent** with how loan types already work (no new abstraction to maintain).
- **Additive / backward-compatible** — values 1–9 are untouched; live PCARDB data and queries are
  unaffected. (Rejected "Option B" — base-`Loan` abstraction / table-per-type — would have meant
  rewriting the live loan engine for no functional gain.)

## Reserved loan-type values

| Value | `LoanCategory` | Scope |
|---|---|---|
| 1 | EmploheeSocietyLoans | shared |
| 2 | JewelLoans | shared |
| 3 | LoansOnFixedDeposits | shared |
| 4 | LoansOnRecurringDeposits | shared |
| 5 | StaffLoans | shared |
| 6 | FarmSectorLoans | **PCARDB-exclusive** |
| 7 | NonFarmSectorLoans | **PCARDB-exclusive** |
| 8 | RuralHousing | **PCARDB-exclusive** |
| 9 | SmallRoadTransportLoans | **PCARDB-exclusive** |
| **10** | **SuretyLoan** | **ECS-exclusive (new)** |
| **11** | **EducationLoan** | **ECS-exclusive (new)** |
| **12** | **DraughtLoan** | **ECS-exclusive (new)** |

## Shared enum + policy (this extension's foundation)

- `Infin8.Coapp.Utility/LoanCategory.cs` (namespace `Infin8.Coapp.Utility.Enums`) — the single source of
  truth, **promoted** out of the previously-duplicated private enums in `LoanProductsCreate.razor` and
  `Loanproducts.razor`.
- `Infin8.Coapp.Utility/LoanCategoryPolicy.cs` — `IsAllowed(category, institution)` /
  `GetCategoriesFor(institution)`. The `PcardbExclusive` / `EcsExclusive` sets here are the **editable
  visibility policy**. Bias: anything not explicitly ECS is treated as PCARDB, so PCARDB never
  regresses.

## Visibility — current state vs. deferred

**Implemented now (loan-products admin screens):** the category list/tabs in `Loanproducts.razor`
and the category dropdown in `LoanProductsCreate.razor` are filtered through `LoanCategoryPolicy`
using the current session's `InstitutionType`. PCARDB admins never see Surety/Education/Draught;
ECS admins never see Farm/Non-Farm/Rural/SRTO.

**Deferred (decide later):** the main navigation menu (`NavMenu.razor`, driven by DB rows in
`Menu_Main / Menu_Sub / Menu_Forms`, filtered by role + `BrCode`). Two options on the table:

- **A — SocietyType-only gating:** filter menu rendering by `InstitutionType` in the UI only.
- **B — SocietyType + DB menu rows:** add ECS loan menu entries as data and filter so only ECS
  societies receive them (matches the existing DB-driven menu architecture).

No menu rows were added in the foundation phase.

## Migration safety

- New `Loan_Type` values are just ints; **no schema migration** in the foundation phase.
- New loan **products** are created as data rows via the existing `LoanProducts` admin UI
  (`Loan_Schemes` / `Loan_Schemes_Group`) per ECS society.
- Any `Loan_Schemes_Group` seed rows for categories 10–12 are additive **data** and are handled in the
  per-loan-type specs.

## Conventions for new ECS work

Namespaces `Infin8.Coapp.<Layer>`; PascalCase entities → lowercase PG tables; soft-delete `*_Delete`;
audit fields `Voc_Id/Usr_Id/Yr_Id/BrCode/Voc_Status`; handler + repository + UnitOfWork pattern;
register services in **both** `Infin8.Coapp.UI/Program.cs` and `Infin8.Coapp.API/Program.cs`.

## Deferred per-type decisions (handled in 03/04/05)

Each loan type will decide whether it reuses the existing engine (`Loan_Master` / `Loan_Trn` /
`Loan_Repayment_Schedule` / interest calc) as-is or needs bespoke logic — e.g. Surety guarantor/salary
checks, Education tranche disbursement, Draught moratorium/interest treatment — plus its UI fields,
`Loan_Schemes` defaults, and reports.
