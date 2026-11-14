# 12 — ECS Demand Calculation (`ECSDemandCalculation`)

> Builds on [02-loan-module-design.md](./02-loan-module-design.md), [10-ecs-loan-disbursement.md](./10-ecs-loan-disbursement.md)
> and [11-ecs-loan-recovey-component.md](./11-ecs-loan-recovey-component.md) §5–§6, which leave principal demand to
> "the month-end ECS Demand Calculation".
>
> **Status:** implemented. §14 records the decisions taken; §15 lists where the code departs from this draft.
>
> **Evidence.** Rules are ported from TSIS (`TsisService/DemandService.cs`, `TsisDataAccess/DatabaseDemand.cs`) and
> checked against the 155 demands TSIS left in the local `ECS` database (branch `11005`). Demand `110050000154`
> (due 01-10-2026) was calculated **and** posted; demand `110050000155` (due 01-11-2026, calculated 26-10-2026) was
> calculated but **never posted** — it is the acceptance test in §11.

---

## 1. Purpose and scope

Once a month the society tells each employer (pay office) how much to deduct from every member's salary. That
figure — the **demand** — is the instalment and interest due on each loan, the month's deposit subscriptions, and
anything still outstanding from earlier months.

This spec covers three operations, run from one page:

1. **Calculate** a demand for a due date — write the header (`mem_demand_master`), one line per member per
   component (`mem_demand`) and one status row per pay office (`mem_demand_status`). Nothing touches the ledgers.
2. **Post** the calculated demand to the ledgers — the current demand becomes `D` rows in `loan_trn`,
   `deposit_trn` and `termdeposit_trn`, so principal falls overdue, interest is booked, and ECS loan recovery
   (spec 11) sees it.
3. **Delete** — undo a posted demand (if nothing was transacted after it), or delete an unposted calculation, so it
   can be recalculated.

**Out of scope (separate specs):** appropriating the salary recovery each employer remits against the demand and
saving it as receipts (TSIS `frmTSISRecoveryAppropriation`, the third stage — spec 13); the demand reports
(Group E: Demand vertical/horizontal/abstracts, appropriation, loan DCB), which read what this spec writes;
restricted (capped) demand, which TSIS applied only in reports.

## 2. What exists today

- **ECS:** `ECSDemandCalculationHandler` builds demand lines in memory and never saves them; nothing calls it. Its
  deposit and RD calculators always return an empty list. The loan calculator has no `BrCode` filter, raises a bullet
  instalment every month, swaps dates on overdue interest, ignores the stop-demand table and drops a member's loans
  silently on a null first due date. `MemDemand`/`MemDemandStatus`/`MemDemandStop` repositories throw
  `NotImplementedException`. No page, controller or day-end step exists. **This spec replaces all of it.**
- **Model mismatches** that must be fixed before EF can read or write `mem_demand`:
  `Loan_Oustanding` → column `loan_outstanding`; `Deposit_Arrear_Collection` → column
  `deaposit_arrear_collection` (sic); column `restricted_demand` has no property. Map them with `[Column]`, add
  `Restricted_Demand`. *(Done, plus: `rd_instalment` is `double precision` and `recovery_id` `integer`;
  `Mem_Demand_Master.Is_Active` had no column — it is `DemandMaster_Delete`; `Is_Demand_Closed` → `demand_closed`;
  `RecoveryUpdatedVoc_Id` and the `Usr_Id`/`Yr_Id`/`Voc_Id` of `Mem_Demand_Status`/`Mem_Demand_Stop` are 12-digit
  numerics, now `decimal`. All four DbSets already existed.)*
- **Data:** 155 masters (`mem_demand_master`), 452,896 lines, 4 pay offices, 49 stop rows (last used 2016).

## 3. Data model (existing tables — no schema change)

| Table | Role | Key columns used |
|---|---|---|
| `mem_demand_master` | one per demand (due date) | `demand_id`, `demanddate` (due date), `demandcalculationdate`, `nextdemanddate`, `isdemandupdated` (posted), `isrecoveryappropriated`, `isrecoveryupdated`, `demandmaster_delete`, `usr_id`, `yr_id`, `brcode`, `voc_status` |
| `mem_demand` | one per member per component | `demand_id`, `demand_type`, `mem_id`, `calculated_date`, component columns below, `total_demand`, `stop_demand`, `is_active`, `brcode`, `usr_id`, `yr_id` |
| `mem_demand_status` | one per pay office of the demand | `demand_id`, `office_id`, `is_appropriated`, `is_recovery_saved`, `is_active` |
| `mem_demand_stop` | member excluded from a demand date | `mem_id`, `demand_date`, `is_active` |

`demand_type`: **L** loan, **D** deposit (`deposit_masters`), **R** recurring deposit, **S** due-to (suspense).
(P/B/C/A rows are created by appropriation — spec 13.)

IDs are MAX+1 per table, allocated in memory for the whole run (the spec 10 §13.2 allocator), with the branch floor
`brcode + "0000000"` — the existing rows follow it (`110050493033`…).

## 4. Calculate — per member

Members: `mem_master` of the branch, not deleted, account not closed. **Loan** lines are calculated for every such
member; **deposit, RD and due-to** lines only for active members (`memberstatus = 1`), as in TSIS.

All `loan_trn` / `deposit_trn` / `termdeposit_trn` / `mem_trn` aggregates count only rows that are not deleted, have
`voc_status = 'V'`, the branch's `brcode`, and `trn_date` **before the due date**. Arrears are recomputed from the
ledgers every month — never carried over from the previous `mem_demand`.

### 4.1 Loans (`L`) — `Loan_Type 1`, open loans with principal or interest outstanding

| Figure | Rule |
|---|---|
| Principal outstanding | Σ `Disb_Amt` − Σ `PrlColl_Amt` |
| Principal arrear | max(0, Σ `Prl_Dem` − Σ `PrlColl_Amt`) |
| Interest arrear | max(0, Σ `IntCalc_Amt` − Σ `IntColl_Amt`) |
| Penal arrear | max(0, Σ `PICalc_Amt` − Σ `PIColl_Amt`) *(TSIS did not clamp — §12)* |
| Interest current | interest from the last `IntCalc_Date` (else the disbursement date) to **due date − 1**, computed with `EcsInterestCalculator` — the same segment-wise rule ECS loan recovery uses (spec 11 §6). |
| Penal current | same, for penal (`PI_Application` 2/3 only; 0/1 = none) |
| Principal current | §4.2 |
| Line total | all six, arrear + current |

A line is written only when its total > 0.

Using the recovery calculator (instead of TSIS's per-segment banker's rounding) keeps demand and recovery
consistent: after posting, `IntCalc_Date` = due date, so the next recovery accrues from there with no gap or overlap.

### 4.2 Principal current, by `loan_schemes.inst_type`

Only on a **due date**: on or after `FirstPrl_DueDate`, and a whole multiple of `Dem_Frequency` months from it
(`Dem_Frequency` 1 = every month; ECS schemes are all 1 today).

| `inst_type` | Principal current |
|---|---|
| 1 Equal instalment | `Loan_Master.Inst_Amt` |
| 2 Equated (EMI) | `Inst_Amt` − interest current, floored at 0 *(TSIS could go negative)* |
| 3 Bullet | none until the last due date |
| 4 No periodic instalment | none |

Then, in order:
1. **Advance paid** (`adv_prl_application = 1`): reduce by the principal paid ahead of demand,
   max(0, Σ `PrlColl_Amt` − Σ `Prl_Dem`). For 2 (the ECS schemes today) principal is raised regardless.
2. **Last instalment:** on or after the last due date (`FirstPrl_DueDate` + (`Prl_Prd` − 1) instalments) the
   current principal is the whole balance not yet demanded. *(TSIS used `+ Prl_Prd`, one month late — §12.)*
3. **Cap:** principal arrear + current ≤ principal outstanding.

### 4.3 Deposits (`D`) — every active `deposit_masters` row

| Figure | Rule |
|---|---|
| Arrear | only if `is_arrear_demand_applicable`: max(0, Σ `Demand_Amount` − Σ `Receipt_Amount`) |
| Penal arrear | max(0, Σ `Penal_Interest_Calculated_Amount` − Σ `Penal_Interest_Received_Amount`) |
| Penal current | one month's penal on the arrear: arrear × rate / 100 / 12, rounded half up; rate from `deposit_roi_templates` (latest `wef` ≤ due date) |
| Current | member's option (`deposit_options`, latest `wef` ≤ due date, active) if any; else `percentage` × basic pay (rounded) if `percentage` > 0; else `fixed_amount`. Annual (`frequency = 'A'`) deposits are due only in `demand_month`. |

Reuse `EcsLoanSanctionRepository.GetCurrentDepositDemandAsync`'s rule (it already filters `BrCode`), per deposit.
Today's masters: Thrift 1,600 (with options), Family Welfare Fund 100, Surety Relief Fund 20 — monthly.

### 4.4 Recurring deposits (`R`) — open RDs (`termdeposit_schemes.tdschemetype = 'R'`, nothing withdrawn)

Instalments due = months from `ValueDate` to the end of the due month, + 1, capped at `PeriodInMonths`.
Demand = instalments due × instalment − Σ received. Penal on each missed instalment at `PenalRateForRD`
(`termdeposit_roi_template`). *(Branch `11005` has no RD accounts; ported for completeness.)*

### 4.5 Due-to (`S`)

Each due-to ledger the member owes (`mem_trn`, `Trn_Type 1`): Σ `Pmt_Amt` − Σ `Rpt_Amt` > 0, demanded in full.

### 4.6 Stop demand

`stop_demand = true` on the member's lines when the member is inactive (`memberstatus = 2`) **or** has an active
`mem_demand_stop` row for the due date. *(TSIS read the table and then overwrote the answer — §12.)* Stopped lines
are still written and posted (the ledger stays true); appropriation and the demand list skip them.

## 5. Post to the ledgers

Allowed only for a calculated, unposted demand (`isdemandupdated = false`). One transaction. For each line with a
current amount:

| Line | Row | Values |
|---|---|---|
| L | `loan_trn`, `Trn_Status 'D'` | `Trn_Date = Due_Date =` due date; `Prl_Sched = Prl_Dem =` principal current; `IntCalc_Amt =` interest current, `IntCalc_Date =` due date **(always, even when 0)**; `PICalc_Amt / PICalc_Date` likewise when penal applies; `Demand_Id`; next `Trn_SlNo` |
| D | `deposit_trn`, `Status 'D'` | `Trn_Date = Due_Date =` due date; `Demand_Amount =` current; `Penal_Interest_Calculated_Amount / _Date` = penal current; `Demand_Id`; `Is_Active` |
| R | `termdeposit_trn` | `Trn_Date =` due date; `DepositDemandAmount =` demand; `PenalCalculatedAmount / PenalAppliedDate`; `Demand_Id` |

Then `mem_demand_master.isdemandupdated = true`. Arrears are **not** posted again — they are already in the ledgers.
Due-to (`S`) lines post nothing. The voucher id on these rows is the branch zero id (no money moves).

Stamping `IntCalc_Date` even when interest is 0 is what stops ECS recovery accruing the same period twice
(spec 11 §11 does the same).

## 6. Delete

- **Delete posted demand:** refused if any `loan_trn` / `deposit_trn` / `termdeposit_trn` row was added after the
  demand's rows for the same loan or deposit (higher `Trn_SlNo`), or if recovery has been saved
  (`isrecoveryupdated`). Otherwise soft-delete every row with this `Demand_Id` (`TrnTr_Delete`, `Is_Active = false`,
  `TD_Delete`) and set `isdemandupdated = false`.
- **Delete calculated demand:** only when not posted. Soft-delete the master (`demandmaster_delete`), its lines and
  status rows (`is_active = false`).

## 7. Order and guards

- A new demand needs the previous one **posted** (and, once spec 13 exists, its recovery saved).
- One live demand per due date per branch.
- The due date defaults to the previous master's `nextdemanddate` (`demanddate` + 1 month, month-end kept). The
  calculation date is the business day.
- Every action is refused off the open business day, and for a non-ECS branch (server-side, as `LoanSanctionController`).

## 8. The page — `Pages/ECSLoan/ECSDemandCalculation.razor` (`/ecs-demand-calculation`)

Whole-page ECS gate (the `ECSLoanSanction.razor` pattern). One card per state:

1. **Next demand:** due date (prefilled), calculation date (business day), **Calculate** → a preview: totals by
   component and by pay office, member count, stopped members; per-member drill-down. **Save calculation** writes §3.
2. **Calculated, not posted:** the saved totals, **Post to ledgers**, **Delete calculation**.
3. **Posted:** totals, **Delete posted demand** (when §6 allows).
4. **History:** the last 12 demands with their state.

Roles: Admin (as the other ECS processing pages).

## 9. Server design (vertical slice per CLAUDE.md)

| Layer | New / changed |
|---|---|
| Models | `Mem_Demand` column fixes (§2); `Mem_Demand_Stop` DbSet |
| Repository | `EcsDemandRepository` (+ interface, `UnitOfWork` in 3 places): candidate loans with ledger sums, roi rows, deposit masters/options/sums, RD sums, due-to sums, stop rows, masters, the id allocator, add/soft-delete. Query syntax, `BrCode` on every alias. |
| Handler | `EcsDemandHandler`: `PreviewAsync`, `SaveCalculationAsync`, `PostAsync`, `DeleteCalculationAsync`, `DeletePostedAsync`, `GetStateAsync`. Calculation in a pure `EcsDemandCalculator` (no DB) beside `EcsInterestCalculator`, so §4 can be checked by hand. |
| Controller | `EcsDemandController` (`api/ecsdemand/...`), ECS gate, `BrCode`/date/user from claims |
| UI | §8 page |
| Old code | delete `ECSDemandCalculationHandler` / repository and the dead `CalculateDepositDemand` / `CalculateRecurringDepositDemand` paths, or leave them unused (§14) |

## 10. What recovery (spec 11) and the reports gain

- Principal overdue for ECS-originated loans (today always 0, because nothing raises `Prl_Dem`).
- `D` rows dated on the due date, so penal starts and the non-overdue interest base drops on the right day.
- Group E reports and loan DCB get `mem_demand` / `loan_trn` data to read.

## 11. Verification

1. **Acceptance — reproduce TSIS demand 155.** Calculate for due date 01-11-2026 against the current database (nothing
   has posted since TSIS calculated it on 26-10-2026) and compare with TSIS's unposted lines:
   loans 518 lines, principal current 21,44,422, interest current 17,53,650; deposits 2,199 lines, current 14,00,980,
   arrear 5,30,640. Principal must match exactly; interest may differ by the rounding change (§4.1) — list every
   loan that differs by more than ₹1 and explain it.
2. Post it; check `loan_trn` D rows equal the L lines (as demand 154's do: 21,20,384 / 16,85,776).
3. Run spec 11 recovery on a loan after posting: principal overdue appears; interest starts from the due date.
4. Delete posted → rows soft-deleted, master back to calculated; delete calculation → gone; recalculate → same totals.
5. A transaction after posting blocks "delete posted".
6. Hand-check `EcsDemandCalculator` on: equal instalment, EMI with interest > instalment (floored), last instalment,
   advance-paid scheme, bullet, annual deposit outside its month, an option amount, a stopped member.
7. A PCARDB branch gets the gate and a 403.

## 12. Departures from TSIS (bugs not carried over)

1. `mem_demand_stop` honoured (TSIS overwrote it with "member is inactive").
2. Last instalment on `FirstPrl_DueDate + (Prl_Prd − 1)`, not one period later.
3. EMI principal floored at 0.
4. Penal arrear floored at 0.
5. Interest rounded once per head (spec 11 rule) instead of per rate segment with banker's rounding.
6. Overdue-interest (IOD) not calculated — TSIS's call swapped its dates and always produced 0; ECS schemes have
   `iod_application = 1` (none).
7. `Dem_Frequency` honoured (TSIS raised principal every month regardless).
8. The posted-demand delete checks `termdeposit_trn` for RDs (TSIS checked `deposit_trn`).
9. Bullet loans (`inst_type 3`) demand nothing until the last due date (TSIS demanded `Inst_Amt` monthly).

## 13. Not in this spec

Recovery appropriation and saving (spec 13), demand reports (Group E), restricted demand, share-capital / GI due-by
uploads, the day-end hook (§14).

## 14. Decisions

1. **Where it runs:** the page, **and** Day End step 7 (`CalendarHandler.DayEndProcess`): on the last non-holiday
   `business_day` of the month an ECS branch calculates (saves, does not post) the demand falling due next month,
   inside Day End's transaction. It skips silently when the branch is not ECS, the day is not the last business day,
   the previous demand is unposted, or the demand already exists; a failure aborts Day End.
2. **Stopped and inactive members:** their lines are written **and posted** (ledger stays true).
3. **Deposit `maximum_amount`:** caps the current deposit demand when > 0.
4. **Expired members** (`ismemexpired` with `expireddate`): interest and penal accrue only up to `expireddate`.
5. **Old code:** deleted — `ECSDemandCalculationHandler`/repository, the throw-only `MemDemand`/`MemDemandStatus`/
   `MemDemandStop` repositories and handlers, `RecurringDeposit` repository/handler, and the dead
   `CalculateLoanDemand`, `CalculateDepositDemand`, `CalculateDueToDemand`, `GetMembersForECSDemand` methods.
   `CalculateLnDues` stays (loan reports use it).

## 15. Implementation notes and departures

- **Files.** `BusinessLogic/Concrete/EcsDemandCalculator.cs` (pure rules), `EcsDemandHandler.cs`,
  `Repository/Concrete/EcsDemandRepository.cs` (+ `EcsDemandIdAllocator`), `Dto/EcsDemandVM.cs`,
  `UI/Controllers/EcsDemandController.cs` (`api/ecsdemand/State|Preview|Save|Post/{id}|DeleteCalculation/{id}|
  DeletePosted/{id}|Lines/{id}`), `UI.Client/Pages/ECSLoan/ECSDemandCalculation.razor`.
- **Stop Demand page** (`Pages/ECSLoan/ECSStopDemand.razor`, `/ecs-stop-demand`, Admin): add or remove members in
  `mem_demand_stop` for a due date (§4.6). Refused once that due date's demand is calculated (delete the calculation
  first) or posted. Removing clears every active row of the member for the date (legacy data has duplicates).
- **Delete Demand for Expired Members page** (`Pages/ECSLoan/ECSExpiredMemberDemand.razor`, `/ecs-expired-demand`,
  Admin), a port of TSIS `frmLoanDemandDeleteForExpiredMembers`. For one loan of a member, it lists `loan_trn` rows from
  the last one on or before the expiry date onwards; a row can be soft-deleted (`TrnTr_Delete` + `ExpiredDeletion`)
  and later restored. Stricter than TSIS: the member must be marked expired (`ismemexpired` + `expireddate`, used as
  the date — TSIS let the user type one), only `D` rows with no money on them can be deleted (TSIS allowed any row
  without money; in practice it was only ever used on `D` rows), and a row is restored only while its demand is
  still posted.
- **Menu rows:** `12-ecs-demand-calculation.sql` points the existing "Demand Calculation", "Stop Demand" and "Delete
  Demand for Expired Members" placeholders under "Thrift Society Demand" (Admin) at the three pages ("Stop Demand"
  pointed at `loan-sanction`), or inserts them where missing. Per branch, ECS only, idempotent.
- **Principal arrear is capped at the principal outstanding.** Migrated loans carry Σ `Prl_Dem` above the amount
  disbursed (opening `I` rows with `prl_dem`); TSIS capped too.
- **Loans with only interest/penal arrears** (principal fully repaid, account not closed) are demanded, per §4.1
  ("principal or interest outstanding"). TSIS skipped them — 15 such loans in branch `11005`, ₹1–₹1,992 each.
- **Post recalculates first.** Posting recomputes the demand and refuses if any current figure differs from the
  saved one (e.g. a recovery moved `IntCalc_Date` after the calculation), so a period is never accrued twice.
  Only lines with a current amount produce a ledger row.
- **Posted-row identification** for delete: `loan_trn` `Trn_Status 'D'`; `deposit_trn` / `termdeposit_trn` rows of
  the demand with no receipt or payment (TSIS stored deposit demand rows with a null status).
- **RD penal** (no RD accounts in `11005`): one month's penal on each instalment unpaid before the due month.
- **Unused id columns** of `mem_demand` (`debtor_led_id`, `td_id`, `dueby_led_id`, `creditor_led_id`, `voc_id`) and
  `recoveryupdatedvoc_id` hold the branch zero id, and `rd_pi_application_date2` holds `1900-01-01`, as TSIS wrote them
  (the last two are NOT NULL).
- **`deposit_options.voc_id` does not exist** and `deposit_roi_templates` ids overflow `int`; the repository projects
  those two queries instead of loading the entities.

**Verified (14-11-2026, local DB and a copy of it).** Preview of 01-11-2026 against TSIS demand 155: deposits identical (2,199
lines); all 517 common loans identical on principal current and arrears, interest current within ₹1. Differences:
the 15 interest-only loans above; loan 1003834 closed and 1004075 opened by an ECS top-up on 23-10-2026 after TSIS
calculated; penal current ₹31 in TSIS vs 0 (ECS schemes have `pi_application` 0/1). Save → post → delete posted →
delete calculation → recalculate (same totals), both refusal guards and the Day End step were exercised on a copy.
The page itself was not driven (needs a login).
