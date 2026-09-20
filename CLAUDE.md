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
`2` = ECS, `0`/other = Unknown, treated as PCARDB so existing visibility never regresses).

New loan products are **new `Loan_Type` discriminator values** on the existing single-table loan
engine, not a new table hierarchy. Values 1–9 are frozen (live PCARDB data depends on them); 10/11/12
are Surety / Education / Draught, ECS-exclusive.

- `Utility/LoanCategory.cs` — the single source of truth for `Loan_Type`.
- `Utility/LoanCategoryPolicy.cs` — `IsAllowed(category, institution)`; the editable visibility policy.

Gating exists on exactly three pages, all under `Pages/Products/`, all keyed off:

```csharp
institution = userInfoDto.SocietyType.ToInstitutionType();
```

`SuretyLoanSanction.razor` is the reference implementation for a whole-page ECS gate — note its
`isReady` guard, which stops the "ECS only" warning flashing before the claim has been read.
`Loanproducts.razor` and `LoanProductsCreate.razor` instead filter a category collection through
`LoanCategoryPolicy.IsAllowed`.

Caveats: **gating is client-side only** — the ECS-only endpoints have no server-side institution
check. Nav-menu gating is deliberately deferred (no menu rows added), so ECS pages are reachable only
by URL. `specs/ecs-extension/03`–`05` (the three ECS loan types) are still stubs.

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
commented out. Definitions live in `Infin8.Coapp.UI/Reports/*.rdlc` and are resolved at runtime as
`{ContentRootPath}/Reports/{ReportFileName}`.

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
