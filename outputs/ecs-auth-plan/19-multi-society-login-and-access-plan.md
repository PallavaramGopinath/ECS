# ECS multi-society login and access plan

**Status:** Draft for review. No application or database change is authorised by this document.

## 1. Purpose and scope

Run the ECS application for many Tamil Nadu cooperative thrift and credit societies using the **one existing PostgreSQL ECS database**. Each society may have any number of named employees. Society staff work only with their own society's data. Admin, Maker, and Checker are permissions within a society. Regional and circle MIS access is a separate, later access type.

**Recommended architecture:** one application, one ECS database, one server-managed database connection pool, and a stable `BrCode` identifying each society's operational data. Login selects the user's authorised society scope; it does **not** select a PostgreSQL database or connection string. Browser clients never receive database credentials.

## 2. Identifiers and society directory

| Identifier | Purpose | Rule |
| --- | --- | --- |
| `SocietyCode` | Statewide MIS identity from the reviewed society register | Numeric, unique, permanent; do not reuse. |
| `RegionCode`, `CircleCode` | MIS hierarchy | Store as relationships; changes need effective dates for historical reports. |
| Proposed combined code `RR-CCC-SSSS` | Human-readable MIS reference | Display/reporting value derived from the three numeric codes. It is not the login tenant key. |
| `BrCode` | Existing application tenant/branch key | Keep as a distinct, unique string and map it one-to-one to the society. Do not replace existing values without a full account-number and data migration review. |
| Official registration number | Legal/source identity | Store separately from the above codes. |

The reviewed spreadsheet's `BRCode` column was blank. Before user onboarding, establish a controlled society directory or additive mapping table linking each active society to its existing or approved `BrCode`, `SocietyCode`, circle, region, official registration number, and status. Verify that `Gen_Bank_Name.BrCode` identifies exactly one active society and `Bank_Type` is the ECS type. Do not infer `BrCode` from `S.No` or the proposed combined MIS code: existing account-number generation concatenates `BrCode` into account numbers.

Existing transactional rows retain their current `BrCode`. Resolve duplicate, null, unknown, or conflicting codes before activating a society. Keep a crosswalk of old and new identifiers during migration. A society moving to another circle keeps its `SocietyCode` and `BrCode`; its MIS assignment changes with an effective date.

## 3. Employee identity and roles

Each employee gets an individual `Users` record, a globally unique login name, and one active society assignment (`Users.BrCode`) for the first release. There is no shared account and no limit of two or three users per society. A user can be disabled without deleting historical transactions. If one person must work for multiple societies later, design an explicit assignment and society-switch workflow; never let the browser supply an arbitrary `BrCode`.

| Society role | Intended actions |
| --- | --- |
| Admin | Manage users and society configuration within that society; view society reports. No automatic power to approve their own work. |
| Maker | Create and submit transactions for the assigned society; view their status. |
| Checker | Review, approve, or reject pending transactions for the assigned society. |

Prefer one operational role per user for a clear separation of duties. If multiple roles are permitted, **the server must still prohibit a user from approving or rejecting their own initiated transaction**, including an Admin. Define a controlled emergency override separately if operations require one, with a reason and audit record. Keep platform support and regional/circle MIS identities separate from society Admin.

## 4. User provisioning and lifecycle

1. A platform operator activates the society only after its directory entry, `BrCode`, business-day setup, financial year, and required master data are verified.
2. A platform operator creates the first society Admin and binds their `BrCode` on the **server**. The Admin can create, disable, reset, and assign Maker/Checker users only within that same `BrCode`.
3. Registration is invitation or administrator driven. The request may contain personal details and a requested role, but the server checks the operator's permission and assigns the society from the operator's verified scope. An ordinary user cannot choose their own role or society.
4. Enforce unique login names, permitted role values, active society status, password/reset policy, and individual accountability. Do not put passwords or reset tokens in URLs or logs.
5. Disabling or transferring a user revokes their refresh sessions and prevents new access. Retain their user ID on historical Maker/Checker and audit records. A transfer to another society requires an explicit, logged reassignment and session termination.

## 5. Login and request flow

```text
Username + password
  -> server looks up Users and verifies password, account status, and society assignment
  -> server loads the society's current business day and financial year
  -> server issues a short-lived authentication session for UserId + Role + BrCode
  -> client calls api/Auth/me to display society and role context
  -> every API request authorises the action and enforces the same BrCode on the server
```

The server obtains `BrCode`, `UserId`, and role from the authenticated principal. A `brCode` supplied in a URL, query, body, or staged record is **untrusted**: reject a mismatch or replace it with the authorised value before any read or write. Resolve a record by both its ID and `BrCode`; an ID alone is not sufficient. Set `Created_By`, `Checked_By`, `BrCode`, business date, and financial year on the server. Never use a client-supplied `Checked_By`, role, date, or year as authority.

At login and refresh, reject inactive, closed, locked, or unassigned users and inactive/unknown societies. Revocation or role/assignment changes must take effect for existing sessions through session invalidation or a short token lifetime plus server-side revalidation. Refresh-token rotation remains, but logout, reset, disable, and transfer must revoke relevant tokens. Keep cookie settings suitable for the deployed origin and enforce CSRF protection on cookie-authenticated state-changing endpoints.

## 6. Tenant isolation in the single database

Create one central server-side tenant-context service that validates the authenticated user's `BrCode` and provides it to handlers and repositories. Audit every endpoint, repository query, join, update, delete, background process, report, export, and file path. For society-owned data, every read and write must be scoped to the current `BrCode`, including **all aliases in joins** and ownership checks on records referenced by ID. Reject null or unknown `BrCode` for tenant-owned rows. Classify genuinely shared reference tables explicitly; do not assume every table has identical tenancy rules.

The navigation menu and page-level role attributes are usability controls, not a security boundary. Apply role and tenant checks at the API/business-operation boundary. Use database constraints and `(BrCode, relevant ID/status/date)` indexes where appropriate. Evaluate PostgreSQL row-level security as a second enforcement layer after confirming how connection pooling and per-request tenant context will be managed; policies must fail closed when no tenant context is present. In particular, the runtime DB account must not bypass row-level security as a superuser, table owner, or role with `BYPASSRLS`. It does not replace endpoint authorisation or Maker/Checker rules.

No society user should receive direct PostgreSQL access. Use a dedicated application DB account with the minimum required privileges. Support and maintenance access must be separate, logged, and controlled.

## 7. Maker/Checker transaction controls

- A Maker submits a staging record under their `BrCode`; the server records their user ID.
- A Checker sees only pending records of the same `BrCode`. Approval/rejection re-reads the staging master and details under that `BrCode`, verifies status, and refuses a self-approval.
- Approval recomputes amounts and business rules as the existing ECS flows already intend. It posts the voucher and changes staging status atomically, with the authenticated Checker ID and society code.
- Use concurrency control so two Checkers cannot approve the same staging item twice. Audit submit, approve, reject, reversal, and any exceptional override.
- Apply these rules to all Member, Staff, Account, ECS loan, deposit, recovery, and day-process paths, not just the new ECS endpoints.

## 8. MIS access for registrars

Society employees see reports for their own `BrCode`. A later **Regional MIS Viewer** may read societies assigned to that region, and a **Circle MIS Viewer** may read societies assigned to that circle. Derive the authorised society-code set from server-side assignments and the effective-dated hierarchy, then aggregate only those societies. These roles are read-only and must not inherit society Admin, Maker, or Checker rights. Do not implement regional access by giving an official a society user account or by accepting arbitrary society codes from the browser.

Define each report's reporting date, as-of hierarchy, completeness rules, and treatment of inactive societies. Reconcile circle totals to their societies and region totals to their circles. Log report access and exports.

## 9. Current-code findings to address before rollout

These are observations from the current repository, not changes made by this plan:

1. `Infin8.Coapp.UI/Program.cs` configures one `CSISDatabase` connection. `JwtAuthenticationHandler`, `JwtService`, `api/Auth/me`, and `CookieAuthStateProvider` already carry `BrCode` and roles through login. Keep this path and harden it.
2. `AuthController.Register` and `UserController.UserRegister` have no authorisation guard, accept a caller-supplied role, and assign hard-coded `BrCode = "11001"`. They must be replaced or restricted before multi-society registration. `UserRegister.razor` currently posts to the former.
3. `UserController.get-appstate/{userName}/{password}` places a password in a URL, while its lookup method ignores that password. Remove this path before rollout. `AuthController.GetJWt` also contains a hard-coded demonstration credential and signing key; remove or disable it.
4. The live authentication handler verifies the password but does not check `Is_Active`, `Is_Account_Closed`, or `Is_Locked`; refresh has the same status gap. Implement these checks and session revocation.
5. `StagingController` has no class-level `[Authorize]`. Approval uses a supplied staging ID and does not compare the staging `BrCode` or Maker ID with the authenticated Checker. `TransactionsRepository.GetStagingDataByStagingId` likewise filters by ID and status without `BrCode`. These are release blockers for shared-database tenancy.
6. Other controllers have missing or commented-out authorisation attributes, and many APIs accept `brCode` from the client. Audit all routes, including reporting and download routes. `MenuMainRepository.GetMenuStructureAsync` accepts `brCode` but filters menus by role only.
7. The client may keep using `api/Auth/me` to show the society context, but UI claims and menu visibility cannot establish server authority.

## 10. Delivery sequence and acceptance gates

**Phase 1 — identity and directory.** Approve the society-to-`BrCode` crosswalk and hierarchy; inventory current `BrCode` values and shared tables; define user roles and onboarding authority; prepare additive schema/constraints and a rollback plan.

**Phase 2 — close login and provisioning gaps.** Restrict registration, remove insecure alternate endpoints, validate user/society status, make roles and society assignment server-owned, implement revocation and audited user administration.

**Phase 3 — enforce isolation.** Add central tenant context and server-side authorisation. Review every API and data path, especially staging, approval, reports, exports, and day processing. Add database constraints/indexes and assess row-level security.

**Phase 4 — pilot.** Use at least two societies with distinct `BrCode` values and at least three named employees per society. Test Admin, Maker, Checker, inactive and transferred users, concurrent approvals, and one society trying another society's IDs, URLs, request bodies, reports, and exports. Confirm the other society's data cannot be read or changed. Check existing PCARDB flows for regression if this deployment shares code.

**Phase 5 — staged onboarding and MIS.** Onboard societies in batches with reconciliation of directory, user, and financial master data. Add circle/region read-only MIS access only after society isolation and as-of hierarchy reporting pass review.

### Minimum acceptance tests

- A user of society A cannot see or mutate society B's data even by changing a URL, body, staged ID, report parameter, or exported file request.
- A society Admin cannot create a user for another `BrCode` or grant a role outside the approved set.
- An inactive/locked/closed user cannot log in, refresh, or continue using a session after revocation takes effect.
- A Maker cannot approve their own transaction; only an authorised Checker of the same society can approve it once.
- All relevant writes and audit records carry the authenticated user and correct `BrCode`; business date and year come from the society's server-side state.
- Regional/circle viewers can read only their assigned hierarchy and cannot initiate or approve transactions.

## 11. Decisions requested before implementation

1. Which current `BrCode` values already exist in the ECS database, and which of the 1,637 societies already have records? Approve the permanent crosswalk before importing users.
2. Who may create the first Admin for each society, and may that Admin create further Admin users? Recommended default: platform operator creates the first Admin; society Admin can create Maker and Checker, while additional Admin requires platform approval.
3. Should an employee hold more than one role? Recommended default: one operational role per user, with self-approval always prohibited.
4. What is the authoritative region/circle/society register and effective date for regional MIS assignments? Confirm these before enabling registrar logins.
5. What hosting origin and browser login arrangement will be used? Confirm cookie, CSRF, session, and password-reset behavior against that deployment.

## References for implementation review

- Microsoft, [Policy-based authorization in ASP.NET Core 10](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-10.0).
- Microsoft, [Prevent Cross-Site Request Forgery attacks in ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0).
- PostgreSQL, [Row Security Policies](https://www.postgresql.org/docs/18/ddl-rowsecurity.html) and [SET](https://www.postgresql.org/docs/18/sql-set.html), for reviewing per-transaction tenant context with pooled connections.
