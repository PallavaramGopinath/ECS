# 09 — ECS Loan Sanction List (`ECSLoanSanctionList`)

> Builds on [06-ecs-loan-sanction.md](./06-ecs-loan-sanction.md) and
> [08-ecs-loan-sanction-modifications.md](./08-ecs-loan-sanction-modifications.md) (this is the
> "sanctioned loans page" promised in spec 08 §6).
>
> **Status:** draft for review. No code has been changed for this spec yet.

---

## 1. Purpose and scope

A list page for ECS loan sanctions that are **awaiting disbursement** (`loansanction_status = 'E'`,
not deleted, current branch). From each row the user can:

| Action | What it does |
|---|---|
| **View** | Shows the sanction's details read-only, with **Modify** and **Print Office Note** buttons. Print is how the user gets the Office Note if they did not print it when the sanction was saved. |
| **Modify** | Opens the sanction on the existing ECS Loan Sanction page, where any detail can be changed and saved again. |
| **Delete** | Deletes the sanction (soft delete) after a confirmation. |

At the top of the page, **Add New ECS Loan Sanction** opens the existing entry page empty.

The menu row `menu_sub.submenu_id = 11005066`, which today opens the entry page
(`/ecs-loan-sanction`), is re-pointed to this list page, so the list becomes the entry point for ECS
loan sanctions.

**Out of scope:** disbursement (separate spec), sanctions in status `S` / `C` (already disbursed),
and any change to the sanction rules in spec 06.

---

## 2. What already exists (reuse, do not rebuild)

All under `api/loansanction` in `Infin8.Coapp.UI/Controllers/LoanSanctionController.cs`:

| Endpoint | Handler | Used here for |
|---|---|---|
| `POST ListSanctions` (`EcsSanctionListFilterVM` → `List<EcsSanctionListItemVM>`) | `EcsLoanSanctionHandler.ListAsync` → `EcsLoanSanctionRepository.GetSanctionListAsync` | The grid. Already filters `status = 'E'`, `!loansanction_delete`, `brcode` on every alias; modes ALL / ID / MEM / DATE; ordered by date desc, id desc; capped at 500 rows. |
| `GET GetSanction/{id}/{brCode}` (`EcsSanctionDetailVM`) | `GetAsync` | View details. |
| `GET OfficeNoteData/{id}` (`OfficeNoteVM`) | `GetOfficeNoteAsync` | Print Office Note (HTML, works on any host — see `CLAUDE.md` › Reports). |
| `POST DeleteSanction/{id}` (`SanctionSaveResultVM`) | `DeleteAsync` | Delete. Already checks: sanction exists in branch, status `E`, no subsequent loan transaction (spec 06 §3.13); soft-deletes header + lines + eligibility in one transaction; ECS gate on the server. |
| `POST SaveSanction` with `LoanSanction_Id > 0` | `SaveAsync` | Modify (on the entry page). Soft-deletes the old sanction and inserts a **new sanction id** (spec 06 §7.2). |

On the client, `Pages/Products/ECSLoanSanction.razor` already has:
- `LoadSanction(decimal id)` — loads a saved sanction into every tab and switches to Modify mode
  (unreachable today since spec 08 removed the Sanctions tab);
- the Office Note modal markup and `printElement` print (`wwwroot/js/pdfViewer.js`).

---

## 3. The page — `Pages/Products/ECSLoanSanctionList.razor`

```razor
@page "/ecs-loan-sanction-list"
@rendermode InteractiveWebAssembly
@attribute [Authorize(Roles = "Admin,Maker,Checker")]
@inject HttpClient _httpClient
@inject AuthenticationStateProvider AuthenticationStateProvider
@inject NavigationManager Navigation
@inject IJSRuntime JSRuntime
```

- **ECS gate.** Same as `ECSLoanSanction.razor`: read `institution =
  userInfoDto.SocietyType.ToInstitutionType()` and use the `isReady` guard so the "ECS only" warning
  does not flash before the claim is read. Non-ECS users see the warning and no list call is made.
- **Context.** `brCode` and `CurrentDate` from `Utilities.GetUserInfoDtoFromASP`.
- Bootstrap 5.3 + Bootstrap Icons only; no new UI library.

### 3.1 Layout

```
ECS Loan Sanctions (awaiting disbursement)              [+ Add New ECS Loan Sanction]
─────────────────────────────────────────────────────────────────────────────────────
Filter: (•) All  ( ) Sanction Id [____]  ( ) Member No [____]  ( ) Date [from] – [to]   [Search]
─────────────────────────────────────────────────────────────────────────────────────
Sl | Sanction Id | Sanction Date | Member No | Member Name | Scheme | Loan Amount | Actions
 1 |    1045     |  12-11-2026   |  002960   | ...         | Surety |  2,00,000   | [View] [Modify] [Delete]
...
                                                   Total: n sanctions, Σ Loan Amount
```

- **Add New ECS Loan Sanction** → `Navigation.NavigateTo("/ecs-loan-sanction")`.
- **Filter.** Maps directly to `EcsSanctionListFilterVM` (`Mode` = ALL / ID / MEM / DATE,
  `SanctionId`, `MemberNo` — matches member no or PerNo, `From`, `To`). Default on load: **All**.
  Date range defaults to the first of the current month → the business day.
- **Grid.** One row per `EcsSanctionListItemVM`. Amounts right-aligned, Indian grouping, dates
  `dd-MM-yyyy`. Empty state: *"No sanctions awaiting disbursement."* If 500 rows come back, show a
  note that the list is capped and the user should narrow the filter.
- Row actions are icon+text buttons (`bi-eye`, `bi-pencil-square`, `bi-trash`). All are disabled
  while a request is in flight (`busy`).

### 3.2 View

Opens a modal (`modal d-block` + backdrop, the same pattern as the entry page — no new modal
component) titled **"ECS Loan Sanction {id}"**, filled from `GET GetSanction/{id}/{brCode}`.
Everything is read-only.

| Section | Fields (source: `EcsSanctionDetailVM`) |
|---|---|
| Sanction | Sanction Id, Sanction Date, Scheme, Status ("Awaiting disbursement") |
| Member | Member No, PerNo, Name, DOB, DOR, Service months |
| Surety | Surety Member No, Name (or "—" when the scheme needs none) |
| Pay details | Basic Pay, Gross Pay, Society Deduction, Total Deductions, Net Salary |
| Loan terms | Loan Amount, Period, Rate of Interest, Penal Rate, Instalment, First Due Date |
| Recoverables | Grid: Loan No, Status, Ledger, Outstanding, Calc amt, OD amt; **Total Recoverable** (`TotalReceiptAmount`) and **Net Disbursable** (Loan Amount − Total Recoverable) |

Modal footer: **Modify** · **Print Office Note** · **Close**.
- **Modify** → closes the modal and goes to Modify (§3.3).
- **Print Office Note** → `GET OfficeNoteData/{id}` → shows the Office Note modal → **Print**
  (`printElement`) / **Close**. This is the reprint path for a user who skipped the print at save
  time (spec 08 §3).
- If any field above is not on `EcsSanctionDetailVM` today, add it to the VM (additive; filled in
  `EcsLoanSanctionHandler.GetAsync`). No new endpoint.

**Office Note markup is shared, not copied.** Move the Office Note modal body out of
`ECSLoanSanction.razor` into a component (e.g. `UI.Client/Components/EcsOfficeNote.razor`, parameters
`OfficeNoteVM Note`, `decimal SanctionId`, `EventCallback OnClose`) and use it on both pages, so the
printed note cannot drift between them.

### 3.3 Modify

- Navigate to **`/ecs-loan-sanction?id={sanctionId}`**.
- On `ECSLoanSanction.razor`, add `[SupplyParameterFromQuery(Name = "id")] public decimal? Id` and,
  after the ECS gate passes in `OnInitializedAsync`, call the existing `LoadSanction(Id)` when
  `Id > 0`. The page then opens in Modify mode with every tab filled; the user can change any detail
  (pay details, scheme, loan terms, recoverables, non-deductable / pending-deposit recoveries) and
  click **Modify Sanction**.
- Behaviour already in place and kept:
  - Member No is locked once the member is loaded (spec 08 §1). **To change the member, delete the
    sanction and add a new one** (see §6 Q1).
  - Server re-validates everything and blocks the save if a subsequent loan transaction exists
    (spec 06 §3.13).
  - Modify **creates a new sanction id**; the old one is soft-deleted (spec 06 §7.2). The success
    message shows the new id and the Office Note opens for it.
- Add a **Back to List** button in the entry page footer (`/ecs-loan-sanction-list`), shown always.
- After a successful Modify, Save stays disabled until Refresh (spec 08 §3). Refresh on a page
  opened with `?id=` must also drop the query string (`NavigateTo("/ecs-loan-sanction")`) so a reload
  does not reload the old sanction.
- If `LoadSanction` fails (not found, deleted, other branch, blocked scheme), show its alert and stay
  in New mode.

### 3.4 Delete

1. Confirm via JS `confirm`: *"Delete ECS loan sanction {id} for {member no} – {name}, Rs.{amount}?
   This cannot be undone from the screen."*
2. `POST DeleteSanction/{id}`.
3. **Success:** alert *"Loan sanction {id} deleted."* and reload the grid with the current filter.
   **Failure:** show `SanctionSaveResultVM.Reasons` (e.g. *"Subsequent transaction made in loan
   account …"*, *"Only sanctions awaiting disbursement can be deleted."*); a `403` shows the "ECS
   only" message (reuse `ReadResultAsync` from the entry page).

Delete is a **soft delete** (`loansanction_delete`, `loansanctiontr_delete`, eligibility delete
flag), so the row stays in the database for audit.

---

## 4. Server changes (small; no new endpoints)

| # | Change | Why |
|---|---|---|
| 1 | `ListSanctions`, `GetSanction`, `OfficeNoteData`: add the `IsEcsAsync(ctx.BrCode)` gate that the write endpoints already use. | Today only writes are gated; a non-ECS user could read ECS sanctions by URL. |
| 2 | `GetSanction`: take `brCode` from claims (`UserContext().BrCode`) and ignore the route value (keep the route shape so the entry page is unaffected). | Route `brCode` lets a user read another branch's sanction. |
| 3 | `EcsSanctionDetailVM`: add any View field from §3.2 not already present (e.g. `SchemeName`, surety number/name, DOR / service months). | View needs them in one call. |
| 4 | (Optional) `EcsSanctionListItemVM`: add `SuretyMemberNo`, `TotalReceiptAmount` if the business wants them as grid columns. | §6 Q3. |

No database schema change. No change to the handler's delete / save rules.

---

## 5. Menu

The row `menu_sub.submenu_id = 11005066` currently routes to `/ecs-loan-sanction`. The nav menu
builds the href from `Form_Name` via `ToLower().Trim().Replace(" ", "-")`
(`Infin8.Coapp.UI/Components/Layout/NavMenu.razor`, `ToUrl`), so re-point it by changing
`form_name` only:

```sql
-- Inspect first
SELECT submenu_id, menu_id, submenu_name, form_name, brcode, voc_status, submenu_delete
FROM   menu_sub WHERE submenu_id = 11005066;

-- Re-point to the list page (slug = ecs-loan-sanction-list)
UPDATE menu_sub
SET    form_name = 'ECS Loan Sanction List'
WHERE  submenu_id = 11005066;
```

- `submenu_name` (the label) can stay "ECS Loan Sanction".
- The entry page stays reachable from the list's **Add New** button and by URL; no separate menu
  row is needed for it.
- Apply out of band (no migrations); record the before/after values in the PR description so it
  can be reverted.
- Reminder (`CLAUDE.md`): the menu is filtered by role only, so non-ECS users with the role will see
  the item and then hit the page's ECS gate.

---

## 6. Open questions (need a business decision)

1. **Changing the member on Modify.** Spec 08 §1 locks Member No once loaded. Keep it locked (to
   change the member: delete + add new) — this spec's default — or allow it in Modify?
2. **Who may delete.** Same roles as the page (`Admin, Maker, Checker`), or restrict Delete to
   `Admin` / `Checker`?
3. **Extra grid columns.** Surety No, Net Disbursable, Period — wanted on the list, or only in View?
4. **Deleted / modified history.** Should the page offer a "show deleted" toggle (old ids replaced by
   Modify, and deleted sanctions), or is the database enough for audit?

---

## 7. Files touched (when implemented)

| File | Change |
|---|---|
| `Infin8.Coapp.UI.Client/Pages/Products/ECSLoanSanctionList.razor` | **New** — §3 |
| `Infin8.Coapp.UI.Client/Components/EcsOfficeNote.razor` | **New** — Office Note markup moved out of the entry page (§3.2) |
| `Infin8.Coapp.UI.Client/Pages/Products/ECSLoanSanction.razor` | `?id=` query parameter → `LoadSanction`; Back to List button; Refresh clears the query; use the shared Office Note component (§3.3) |
| `Infin8.Coapp.UI/Controllers/LoanSanctionController.cs` | ECS gate on reads; `GetSanction` uses claims `brCode` (§4) |
| `Infin8.Coapp.Dto/EcsLoanSanctionVM.cs`, `Infin8.Coapp.BusinessLogic/Concrete/EcsLoanSanctionHandler.cs` | Only if View fields are missing (§4 #3, #4) |
| Database: `menu_sub` row 11005066 | `form_name` update (§5) |

---

## 8. Manual test scenarios

No automated tests. `dotnet build Infin8.Coapp.sln`, run, log in to the ECS branch (11005).

| # | Scenario | Expected |
|---|---|---|
| 1 | Click the menu item for submenu 11005066 | Opens `/ecs-loan-sanction-list` |
| 2 | Log in as a PCARDB society, open the URL | "ECS only" warning; no list call. Direct `POST ListSanctions` returns 403. |
| 3 | Page load | All status-`E`, non-deleted sanctions of the branch, newest first; disbursed (`S`/`C`) and deleted ones absent |
| 4 | Filter by Sanction Id / Member No (and PerNo) / Date range | Grid narrows correctly; All restores |
| 5 | **Add New** | Entry page opens empty in New mode |
| 6 | **View** | Modal shows all §3.2 sections matching the saved sanction; totals agree with the Office Note |
| 7 | View → **Print Office Note** → Print | Office Note shows and prints; identical to the one shown after save |
| 8 | **Modify** (row or View) → change loan amount → Get Recoverables → Modify Sanction | Saved under a new id; Office Note opens for it; back on the list the old id is gone and the new id is shown |
| 9 | Modify a sanction whose recovered loan has a later `loan_trn` | Save blocked: "Subsequent transaction made in loan account …" |
| 10 | Open `/ecs-loan-sanction?id=<deleted or other-branch id>` | Alert "Could not load the sanction"; page in New mode |
| 11 | **Delete** → Cancel | Nothing changes |
| 12 | **Delete** → OK | "Loan sanction {id} deleted."; row gone; DB header + lines `_delete = true` |
| 13 | Delete with a subsequent loan transaction | Blocked with the reason; row stays |
| 14 | Refresh on a page opened with `?id=` | URL becomes `/ecs-loan-sanction`; page empty |
