# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A core banking / back-office system for Indian cooperative societies, built for **PCARDB** (Primary
Cooperative Agriculture and Rural Development Banks) and currently being extended to also serve
**ECS** (Cooperative Employees' Thrift and Credit Societies) from the same codebase.

Modules: Members (share capital, dividend, demand/collection), Loans (jewel, on-FD, on-RD, sector
loans), Term Deposits, Accounts/Ledgers, Lockers, Payroll/PF, day-process (day begin/end, financial
year), and ~58 RDLC reports.

Blazor Web App on .NET 10, EF Core 10 + Npgsql against PostgreSQL. See `specs/ecs-extension/` for the
in-flight ECS design decisions.

## Commands

```bash
# Build (solution = 9 projects; does NOT include Infin8.Coapp.API — see Gotchas)
dotnet build Infin8.Coapp.sln

# Run the app -> https://localhost:7073 (or http://localhost:5247)
dotnet run --project Infin8.Coapp.UI
```

Requires a PostgreSQL instance matching `ConnectionStrings:CSISDatabase` in
`Infin8.Coapp.UI/appsettings.json`.

**There are no tests, no test project, and no CI workflow.** Verification is manual: build, run, and
drive the affected screen. Do not claim a change is verified on the strength of a successful build.

**Build a baseline before you start.** HEAD compiles clean, but the working tree usually carries a
large uncommitted feature branch that may not. Run `dotnet build` first so you don't attribute
pre-existing errors to your own change. Expect ~199 warnings at HEAD; that is the normal baseline.

## Architecture

Five layers, strictly one-way:

```
Blazor page (UI.Client)  --HttpClient-->  Controller (UI/Controllers)
   -->  Handler (BusinessLogic)  -->  IUnitOfWork  -->  Repository  -->  CSISContext (EF Core)
```

**`Infin8.Coapp.UI.Client` references only `Dto` and `Utility`** — the `BusinessLogic` project
reference is deliberately commented out in its `.csproj`. Client pages therefore *cannot* inject
handlers; they must go over HTTP to a controller in `Infin8.Coapp.UI/Controllers/`.

| Project | Role |
|---|---|
| `Infin8.Coapp.UI` | Blazor host + all live API controllers + `Reports/*.rdlc` |
| `Infin8.Coapp.UI.Client` | WASM pages and components (where new pages go) |
| `Infin8.Coapp.BusinessLogic` | 121 `*Handler` + `I*Handler` pairs |
| `Infin8.Coapp.Repository` | 124 repositories, `UnitOfWork`, `CSISContext` |
| `Infin8.Coapp.Models` / `.Dto` | 162 EF entities / 365 DTOs and VMs |
| `Infin8.Coapp.Utility` | static helpers, enums, `AppState` |
| `Infin8.Coapp.ReportServices` | one live method: `MicrosoftReport.CreateLocalReport` |

### Adding a feature — the full vertical slice

1. Entity in `Models/` (if new) — see entity conventions below.
2. `DbSet<T>` in `Repository/CSISContext.cs`.
3. Repository in `Repository/Concrete/` + interface in `Repository/Interface/`. Interfaces do **not**
   extend `IRepository<T>`; only hand-written methods are visible to callers.
4. Wire into `UnitOfWork` in **three** places: interface property in `Interface/IUnitOfWork.cs`,
   private backing field, and the lazy `=> _x ??= new XRepository(_context)` property.
5. Handler + interface in `BusinessLogic/`.
6. Register the handler in `Infin8.Coapp.UI/Program.cs` as `AddScoped`.
7. Controller action in `Infin8.Coapp.UI/Controllers/`.
8. Razor page in `Infin8.Coapp.UI.Client/Pages/` with `@rendermode InteractiveWebAssembly`,
   `@attribute [Authorize(Roles = "...")]`, and `@inject HttpClient`.
9. Insert `Menu_Main` / `Menu_Sub` / `Menu_Forms` **database rows** to make it reachable. The nav menu
   is DB-driven (`Voc_Status == "V"`, not soft-deleted), not route-scanned. Two traps:
   - It is filtered by **role only**. `MenuMainRepository` accepts `brCode` and never uses it, despite
     all three tables having a `BrCode` column. `Menu_Main.Role` is a single string, not a list — use
     `'All'` or duplicate the row to expose an item to more than one role.
   - `Menu_Forms.Form_Name` becomes the href via `formName.ToLower().Trim().Replace(" ", "-")`, so it
     must slugify to your `@page` route. Any other punctuation (`/`, `_`, `&`) produces a broken link.

Without step 9 the page is reachable only by typing its URL.

### Repository shape

Every repository opens with this downcast accessor (122 of 124 files) — copy it verbatim:

```csharp
public class LoanTrnRepository : Repository<Loan_Trn>, ILoanTrnRepository
{
    public CSISContext CSISContext => (CSISContext)Context;
    public LoanTrnRepository(DbContext context) : base(context) { }
}
```

### Handler shape

```csharp
readonly IUnitOfWork _unitOfWork;
public LoanTrnHandler(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }
```

Handlers never inject individual repositories — everything is reached via `_unitOfWork.X`. The
contract is: **repositories mutate the change tracker, handlers call `_unitOfWork.CompleteAsync()`.**
Multi-aggregate writes need an explicit `BeginTransaction()` / `Complete()` / `CommitTransaction()` /
`RollBack()`; only 11 of 121 handlers do this today.

### LINQ convention

Query syntax in the **repository** layer (complex joins/groups against `DbSet`); method syntax in the
**business-logic** layer (over already-materialised in-memory collections). No file mixes both in one
expression.

## Entity conventions

Models are attribute-only POCOs. **There are no navigation properties anywhere** — no `virtual`, no
`ICollection<>`, no `[ForeignKey]`. All relationships are manual LINQ `join`s. Do not add navigation
properties expecting lazy/eager loading; there is no FK metadata behind them.

Every new entity needs:

- `[Key]` on the PK. `decimal` for transactional/master tables, `int` for small reference tables.
- `public string? BrCode { get; set; }` — branch/tenant discriminator, on 149 of 162 entities.
- `Usr_Id` / `Yr_Id` (`decimal`) for user-created or year-scoped rows.
- A soft-delete bool with a **module-specific prefix**, not a shared `IsDeleted`: `Loan_Delete`,
  `TrnTr_Delete`, `MemTrn_Delete`, `JL_Delete`, …
- For voucher-participating rows: `Voc_Id` plus `public string? Voc_Status { get; set; } = "V";`
  (`"V"` = valid/verified, the literal used everywhere).

Money is `double`, not `decimal`. IDs are `decimal`.

**IDs are generated application-side via MAX+1**, not by DB sequences — see `Repository/Concrete/MaxId.cs`
and `LoanTrnRepository.cs`. New transactional entities must follow this to match existing data.

**`BrCode` filtering is manual and unenforced.** There are zero `HasQueryFilter` calls in the
solution, so nothing stops a query reading across branches. Write the `BrCode` predicate by hand on
**every alias** in **every** join. Several existing queries omit it — do not copy those.

## Database

**DB-first / externally managed.** There are no EF migrations, no `ModelSnapshot`, no `.sql` files, and
no `dotnet-ef` tooling reference. Adding a column means editing the POCO *and* applying the DDL to
PostgreSQL out of band. PCARDB is live, so schema changes must be additive and backward-compatible.

`CSISContext.OnModelCreating` calls `UseLowerCaseTableAndColumnNames()`, which **lowercases**
identifiers — it does not snake_case them. `Loan_Trn` → `loan_trn`; `IsAccountClosed` →
`isaccountclosed`. Underscores in DB names come from the C# property names already containing them.
Any new fluent configuration must be added *before* that call or it won't be lowercased.

Raw SQL is used in ~24 places via `Database.SqlQueryRaw<T>` / `ExecuteSqlRawAsync` with
`NpgsqlParameter`. Never add double-quoted mixed-case identifiers — unquoted identifiers fold to
lowercase, which is what the schema uses.

Npgsql legacy timestamp behaviour is switched on in the `CSISContext` constructor, so `DateTime` maps
to `timestamp without time zone`. Don't introduce `DateTimeOffset` or UTC-kind assumptions.

Transaction dates come from the **business day** (`Calendar` per `BrCode`, set by Day Begin), not
`DateTime.Now`.

## Auth and session context

Login issues a JWT stored in an **HttpOnly** cookie, so the WASM client cannot read it directly. It
instead calls `api/Auth/me`, and `UI.Client/Providers/CookieAuthStateProvider.cs` rebuilds a
`ClaimsPrincipal` from the response. Every page reads context the same way (~90 call sites):

```csharp
@inject AuthenticationStateProvider AuthenticationStateProvider
...
var userInfoDto = await Utilities.GetUserInfoDtoFromASP(AuthenticationStateProvider);
brCode = userInfoDto.BrCode!;
transaction.Yr_Id = userInfoDto.YrId;
transaction.Transaction_Date = userInfoDto.CurrentDate;   // the business day
```

There is no ambient/cascading context — `BrCode`, `YrId` and `CurrentDate` are threaded explicitly
into every URL and handler call. Read context from claims, never from `AppState`.

`SocietyType` is populated by `api/Auth/me` (from `Gen_Bank_Name.Bank_Type`) and added as a claim by
`CookieAuthStateProvider` — deliberately *not* baked into `JwtService.GenerateAccessToken`, to keep
the live login path and JWT signature untouched.

Two traps here:

- **`userInfoDto.CalendarStatus` is always null.** It is minted into the JWT but dropped at every read
  layer (`ControllerExtension.cs`, `CookieAuthStateProvider`, `GetUserInfoDtoFromASP`). Existing code
  assigns it anyway.
- **`CurrentDate` is frozen in the token.** A Day Begin / Day End performed mid-session is not
  reflected until re-login. Only the `DayProcess` screens query `api/BusinessDay/*` live.

## ECS extension (in flight)

Institution type is **not** a new entity — it reuses `Gen_Bank_Name.Bank_Type` (`1` = PCARDB,
`2` = Cooperative Housing Society, `3` = ECS, `0`/other = Unknown, treated as PCARDB so existing
visibility never regresses). See `specs/ecs-extension/07-loan-type-society-type.md`.

Loans use the existing single-table loan engine and its `Loan_Type` discriminator; values 1–9 are
frozen (live data depends on them). ECS loans are all `Loan_Type 1`, differentiated by `Scheme_Id`;
10–12 retired. Product-specific behaviour comes from rule columns on `loan_schemes`, never from a
scheme's id or name. `Loan_Type 6` is Farm Sector (PCARDB) or Housing (Housing Society) — label it via
`LoanCategoryPolicy.GetDisplayName`, never hard-code it.

- `Utility/LoanCategory.cs` — the single source of truth for `Loan_Type`.
- `Utility/LoanCategoryPolicy.cs` — per-society allow-list behind `IsAllowed(category, institution)`
  (the editable visibility policy) plus `GetDisplayName(category, institution)`.

Gating exists on four pages, all under `Pages/Products/`, all keyed off:

```csharp
institution = userInfoDto.SocietyType.ToInstitutionType();
```

`ECSLoanSanction.razor` (`/ecs-loan-sanction`) is the reference implementation for a whole-page ECS gate — note its
`isReady` guard, which stops the "ECS only" warning flashing before the claim has been read. The same
whole-page gate is copied into `ECSLoanSanctionList.razor` and `ECSLoanDisbursement.razor` (`/ecs-loan-disbursement`).
`Loanproducts.razor` and `LoanProductsCreate.razor` instead filter a category collection through
`LoanCategoryPolicy.IsAllowed`; `LoanProductsView.razor` only uses the institution for labels.

Caveats: gating is **mostly client-side**. Only `LoanSanctionController` checks the institution on the
server (its `IsEcsAsync` looks up `Gen_Bank_Name.Bank_Type` for the claims `BrCode`, since the JWT carries
no `SocietyType`) — this covers sanction save/list/delete, the disbursement endpoints
(`DisbursementPreview`, `Disburse`) and `RecoveryLoans`; other ECS-only endpoints have no server-side check. ECS disbursement
allocates ids with its own in-memory MAX+1 allocator (`EcsLoanDisbursementRepository`) because the shared
add-methods collide when one transaction adds several rows to a table — see spec 10 §13.2. Nav-menu gating is deliberately deferred (no menu rows added), so ECS pages are reachable only
by URL. `specs/ecs-extension/03`–`05` (the three ECS products, all schemes under `Loan_Type 1`) are still stubs.

### ECS loan recovery (acc 58, "Loan Receipt")

Not a page: a component on Member Transaction (`/member-transactions`) for `account_transactions.acc_id = 58`.
Spec: `specs/ecs-extension/11-ecs-loan-recovey-component.md` (sic — "recovey" is the committed file name;
§16 lists where the code departs from the draft).

- **Wiring.** `Components/ECSLoan/ECSLoanRecovery.razor` (Maker input) and `ECSLoanRecovery_View.razor`
  (Checker view). The view's name must match `account_transactions.view_component_name` — the DB row says
  `ECSLoanRecovery_View`, with the underscore. `case 58` sits in four switches in `Member_Transaction.razor`
  (`GetComponentName`, the receipt switch, `GetViewComponentName`, the delete switch) and in
  `Checkerboard.razor`'s `GetViewComponentName`. A view component receives only `stagingId` and
  `relatedAccountId` and fetches its own JSON via `api/Staging/GetStagingDetailsById`.
- **Flow.** `GET api/loansanction/RecoveryLoans/{memId}` → `EcsLoanRecoveryHandler.GetRecoveryLoansAsync`
  (BrCode and date from claims; refuses when the token's `CurrentDate` is no longer the branch's first `N`
  business day). Initiate posts one `staging_details` row with the serialised `DtoEcsLoanRecovery` in
  `Related_Account_Data`. Approval: `TransactionsHandler.SaveTransaction` `case 58` →
  `EcsLoanRecoveryHandler.BuildPostingAsync`, which recomputes everything and adds rows to
  `SaveTransaction`'s own `loanTrnList` / `finVoucherTrns` (no separate allocator: `AddLoanTrnListAsync`
  saves per row, so several loans in one approval do not collide). `TransactionsHandler` now injects
  `IEcsLoanRecoveryHandler` — the one handler-into-handler dependency in the approval path.
- **Two pure calculators, deliberately DB-free so they can be checked by hand.**
  `BusinessLogic/Concrete/EcsInterestCalculator.cs` (segment-wise diminishing product, Σ base × rate × days /
  36500, rounded **once** per head with `AwayFromZero`) and `Utility/EcsRecoveryAppropriator.cs` (penal →
  interest → principal overdue → principal in advance, oldest loan first). The appropriator lives in
  `Utility` so the WASM grid and the server's approval recheck run the *same* code; keep it there.
- **Balances come from `loan_trn`, never `San_Amt`.** Principal outstanding = Σ `Disb_Amt` − Σ `PrlColl_Amt`.
  Don't reuse `EcsLoanSanctionRepository.GetLoanBalancesAsync` for this — it uses `San_Amt` and has no
  `Voc_Status` / date filter.
- **`pi_application` 0 means no penal**, the same as 1 (live schemes carry 0); only 2 and 3 accrue penal or
  stamp `PICalc_Date` (`EcsInterestCalculator.PenalApplies`).
- **Approval errors now reach the Checker.** `SaveTransaction`'s catch used to swallow everything. An
  `EcsRecoveryException` (stale figures, wrong business day, closed/pending loan, non-ECS branch) sets
  `DtoVoucher.Message`; `ApproveStaging` returns it as the 400 body and `Checkerboard.razor` alerts it. Every
  other failure still shows the generic text — throw `EcsRecoveryException` only for messages meant for the user.
- A loan already in a pending (I/M) acc 58 staging line is locked: its JSON is deserialised to find the
  `Loan_Id`s. The approving staging excludes itself from that check.

### ECS demand calculation (spec 12)

Page `Pages/ECSLoan/ECSDemandCalculation.razor` (`/ecs-demand-calculation`, Admin), API `api/ecsdemand/*`
(`EcsDemandController`, ECS-gated server-side), `EcsDemandHandler`, `EcsDemandRepository`. Spec:
`specs/ecs-extension/12-ecs-demand-calculation.md` (§14 decisions, §15 departures).

- **Rules are in `BusinessLogic/Concrete/EcsDemandCalculator.cs`**, pure and DB-free like `EcsInterestCalculator`,
  which it calls for interest/penal current. Ledger sums count valid rows dated **before** the due date.
- **Calculate** writes `mem_demand_master` / `mem_demand` / `mem_demand_status` only. **Post** writes `loan_trn` `D`,
  `deposit_trn` and `termdeposit_trn` rows dated on the due date (zero voucher id, `IntCalc_Date` always stamped)
  and first recalculates, refusing if any current figure changed since the calculation.
- **Guards:** previous demand posted, one live demand per due date, token business day = open day, ECS branch.
- **Menu:** unlike the other ECS pages it has a nav entry — `specs/ecs-extension/12-ecs-demand-calculation.sql` revives
  the "Demand Calculation" placeholder under "Thrift Society Demand" (Admin). Per branch, ECS only, idempotent; run it
  on each database.
- **Day End step 7** (`CalendarHandler`) calls `CalculateForDayEndAsync` on the last business day of the month,
  inside Day End's transaction — the handler must not open its own transaction there.
- The demand entities had wrong column names and `int` ids that overflow; they are fixed. `Deposit_Options.Voc_Id`
  has no column and `Deposit_Roi_Templates.Usr_Id/Yr_Id` overflow `int`: project those tables, don't load them whole.

## Gotchas

- **`Infin8.Coapp.API` is not in the solution** and is not built. It is a stale parallel host: `net9.0`,
  EF Core **SQL Server** 8.0.11, packages from .NET 8, one commit ever. `specs/02` says to register
  services in both `Program.cs` files — that advice is obsolete. **Only `Infin8.Coapp.UI/Program.cs`
  matters.** Its DI list has 101 active registrations vs the API's 42.
- **`Utility/JwtService.cs` and `Utility/RoleConstants.cs` never compile** — they are `<Compile Remove>`d
  in `Infin8.Coapp.Utility.csproj`. The live copies are in `Infin8.Coapp.Repository/` (declared in
  namespace `Infin8.Coapp.Utility`, which is why the illusion holds). Editing the Utility copies does
  nothing.
- **`Infin8.Coapp.Logger` is an empty project** — no `.cs` files, referenced by nothing. There is no
  logging abstraction in this solution; handlers rethrow as `InvalidOperationException` or write to
  `Console`.
- **`Utility/DtoToModel.cs` is an empty stub.** Entity construction goes through
  `Utility/GetModalObject.cs` — static factories with very long *positional* parameter lists (one takes
  ~90). Trailing unlabelled `0, 0, 0` arguments are the most error-prone construct in the codebase;
  count carefully.
- `Repository.cs` base class is inconsistent: `DeleteAsync` and `EditAsync` call `SaveChangesAsync`
  themselves, breaking the unit-of-work boundary. `Add`/`Edit`/`Delete` do not.
- The `Task<bool>` write convention is largely meaningless — the common handler template overwrites the
  repository's result with `result = true` before returning.
- `AppState` is registered `AddSingleton` on the server. Nothing injects it (the only `@inject AppState`
  is commented out), but do not start — a singleton holding `UserId`/`BrCode` leaks across users.
  `TransactionStateService` *is* a server singleton that ~20 components subscribe to.
- `mem_master` is the one entity with an all-lowercase class name and all-lowercase properties
  (`mem_id`, `memberno`, `brcode`). Member queries read `x.mem_id`, not `x.Mem_Id`.
- The 34 controllers in `Infin8.Coapp.UI/Controllers/` are declared in namespace
  `Infin8.Coapp.API.Controllers` — they were copy-pasted from the dead API project. Don't let the
  namespace fool you into editing the wrong file.
- `<RedirectToLogin />` in `Components/Routes.razor` **does not exist** as a component. Razor emits
  only a warning, so unauthenticated users get an inert element instead of a redirect.
- `NavMenu.razor` is the single component that injects a handler directly (`IMenuMainHandler`) and the
  only one running `@rendermode InteractiveServer` — the sidebar lives on a SignalR circuit while page
  bodies run in WASM. Do not treat it as a template for new pages.
- Several components have abandoned variants still in the build (`*_old`, `*_Renewal2/3`, `*ChatGpt`,
  `*_GPT`), and there are three overlapping modal implementations plus two autocompletes. Check which
  variant is actually routed before editing.

## Reports

RDLC via `ReportViewerCore.NETCore`, PDF only (`LocalReport.Render("PDF")`); the Excel/Word calls are
commented out.

**PDF rendering works only on a Windows host.** On macOS/Linux, `Render("PDF")` throws
`PlatformNotSupportedException`: `System.Drawing.Common` is Windows-only from .NET 7 on, and the
renderer also needs Uniscribe. Installing `libgdiplus` does not fix it. `MicrosoftReport.CreateLocalReport`
catches the exception, writes it to the console and returns an **empty byte array**, so on a Mac dev
machine every report endpoint returns `200` with no bytes and the page shows a blank or "empty" report.
Don't debug the RDLC for that symptom on a Mac. To check field binding on a Mac anyway, render to `"CSV"` instead of `"PDF"`: the CSV
renderer needs no GDI, and an unbound field comes out as an empty column (binding is **case-sensitive** —
`memberNo` does not bind `MemberNo`, a common fault in reports copied from TSIS). When a document must render on any host, draw it as HTML
from a data endpoint instead; the ECS Office Note does this (`api/loansanction/OfficeNoteData`, printed
with `printElement` in `UI.Client/wwwroot/js/pdfViewer.js`). Definitions live in `Infin8.Coapp.UI/Reports/*.rdlc` and are resolved at runtime as
`{ContentRootPath}/Reports/{ReportFileName}`.

Every controller builds that path with `Path.Combine(ContentRootPath, "Reports", ReportFileName)`. Older code
used `$"{ContentRootPath}\\Reports\\"` with Windows backslashes, which throws `FileNotFoundException` on a Mac
before the renderer is reached; don't reintroduce it.

About 40 `reports_master` rows came over in the Access migration from the old TSIS WinForms system and have
**no ECS code behind them** — no controller builds their dataset. Their `.rdlc` files were copied unchanged
from TSIS (e.g. `LoanLedger.rdlc`, `Demand_Vertical.rdlc`, the `Dividend*` and `IT*` reports), so they will not
render until someone writes the controller and dataset; most ECS reports were adapted from their TSIS
originals, so expect to edit the `.rdlc` too (its `<DataSet Name>` must match the C# side). Seven files exist
nowhere and are still missing: five Crystal `.rpt` files (`DepositFWDFWS`, `JLMemberWiseOutstanding`,
`JLMembewiseOutstanding`, `JLVerificationStatement_ornaments`, `MembersDividendIntOnTDPendingList`),
`PersonalRegisterSociety.rdlc` and `RecurringDepositRegister_New.rdlc`.

Report pages pick a report by `Report_Id` in a hard-coded `switch`, not by name, and load their dropdown by
`ReportGrp_Id`. Report pages are linked as `menu_sub` rows under the "Reports" menu, with `form_name` = the route.
The four pages added for the TSIS port — `/reports-certificate` (group 15), `/reports-income-tax` (group 14),
`/reports-personal-register` (group 11) and `/utility-emi-schedule` (report 126) — get their rows from
`specs/reports/02-report-menu-rows.sql` (per branch, idempotent); run it on each database.

Reports are **discovered from the `Reports_Master` database table**, not the filesystem — adding a
report requires a DB row, not just a file. The `.rdlc` `<DataSet Name="...">` must match the dataset
name string passed in C#.

`ReportViewer/` at the repo root is an untracked local copy of the ReportViewerCore *source*, for
reference only — it has its own solution and nothing in the build depends on it. The dependency is the
NuGet package.

Note: `Infin8.Coapp.UI.csproj` has no `CopyToOutputDirectory` entry for `Reports/**/*.rdlc`, so reports
work in dev (ContentRootPath = project folder) but the `.rdlc` files are not carried into a publish
output.

## Conventions

Namespaces `Infin8.Coapp.<Layer>`. Handler + interface + repository + interface + UnitOfWork wiring for
every new data path. Commit messages are plain-English feature status (`"X commenced"` / `"X completed"`).

**There is no global render mode** — `App.razor` and `Routes.razor` are static SSR, so every
interactive component must declare its own `@rendermode`. A page that omits it silently renders
static and will not respond to events (six routable pages currently have this bug).

**No third-party Blazor UI library** (no MudBlazor/Radzen/Syncfusion/Telerik). Styling is hand-written
Bootstrap 5.3 with Bootstrap Icons; JS interop lives in `UI.Client/wwwroot/js/`. Bootstrap's JS and
the icon font are loaded from CDN at versions that don't match the local/libman pins.
