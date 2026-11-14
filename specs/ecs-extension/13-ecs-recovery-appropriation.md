# 13 — ECS Recovery Appropriation (`RecoveryAppropriation`)

> Builds on [12-ecs-demand-calculation.md](./12-ecs-demand-calculation.md), which writes the demand this spec
> recovers, and [11-ecs-loan-recovey-component.md](./11-ecs-loan-recovey-component.md) for the receipt rows.
>
> **Status:** implemented. §13 records the decisions taken; §14 lists where the code departs from TSIS.
>
> **Evidence.** Rules are ported from TSIS (`TSIS/frmTSISRecoveryAppropriation.cs`, `TsisService/DemandService.cs`
> `AppropriationOfRecovery` / `UpdateSubsequentRecoveryInDemand`, `TsisDataAccess/DatabaseDemand.cs` `SaveRecovery` /
> `DeleteRecovery` / `DeleteRecoveryAppropriation`) and checked against what TSIS left in the local `ECS` database
> (branch `11005`). Demand `110050000154` (due 01-10-2026) was appropriated and saved office by office — recovery ids
> 454 (HO, 30-09-2026, voucher `VR/229-AR/201`), 455 (Staff) and 456 (Workmen, both 14-10-2026). Reproducing it is the
> acceptance test in §11.

---

## 1. Purpose and scope

Each month every employer (pay office) deducts the demand from its employees' salaries and remits one amount per
employee. This spec covers what happens next, per pay office of a posted demand:

1. **Appropriate** — split each member's remitted amount across that member's demand lines in the society's
   collection-priority order, writing the `*_collection` columns of `mem_demand` (and new `A` / `B` lines for
   money that has no demand line). No ledger is touched.
2. **Save recovery** — turn the appropriation into receipts: `loan_trn` `R`, `deposit_trn`, `termdeposit_trn` and
   `mem_trn` rows and one voucher whose debit side is the ledger(s) the employer's money arrived in.
3. **Delete** the saved recovery (reverses the receipts) or the appropriation (clears the collections).

**Out of scope:** Recovery Refund (TSIS `frmFactoryRecoveryRefund`, which pays back an excess `B` collection —
a follow-up), nil-recovery appropriation as a separate screen (covered here by saving an office with no amounts,
§6.4), the CTC (`gSocietyId == 211`) variant, Group E reports.

## 2. What exists today

- `Pages/RecoveryAppropriation/RecoveryAppropriation.razor` (`/recovery-appropriation`) is a placeholder with a date
  box and a button that does nothing. The "Recovery Appropriation" `menu_sub` row under "Thrift Society Demand"
  (Admin) already points at that route, so no menu SQL is needed.
- `Mem_CollectionPriority` and a throw-only repository/handler pair exist, but the entity mapped to a table that
  does not exist (`mem_collectionpriority`) and declared `priorityled_id` as `int` (12-digit ids overflow). Fixed:
  `[Table("mem_collection_priority")]`, nullable ints as in the schema, `PriorityLed_Id` `decimal?`.
- `mem_demand` already has every column this spec writes (spec 12 §2 fixed their mappings).

## 3. Data model (existing tables — no schema change)

| Table / column | Written by | Meaning |
|---|---|---|
| `mem_demand_status.is_appropriated` | appropriate / delete appropriation | the office's recovery is appropriated |
| `mem_demand_status.is_recovery_saved`, `voc_id` | save / delete recovery | receipts saved, with this voucher |
| `mem_demand_master.isrecoveryappropriated` | appropriate | every live office appropriated |
| `mem_demand_master.isrecoveryupdated` | save | every live office saved — spec 12 then allows the next demand |
| `mem_demand.*2` (`loan_prl_arrear2` …, `total_demand2`) | appropriate | the **revised** demand the money was split against (§4.1) |
| `mem_demand.*_collection`, `rd_receipt`, `rd_pi_receipt`, `creditor_collection`, `total_collection` | appropriate | the split |
| `mem_demand.recovery_id`, `recovery_date`, `is_appropriated` | appropriate | one recovery id per office run |
| `mem_demand.voc_id`, `is_recovery_saved` | save | voucher of the receipts |

Line types added by appropriation (spec 12 §3 left them to this spec). TSIS's own names were `P` and `B`; the
migrated ECS data stores the first as `A`, so ECS keeps the data's letters:

| Type | TSIS | Line |
|---|---|---|
| `A` | `P` (`LPA`) | advance principal on an open loan of the member that has **no** line in this demand; `loan_id`, `loan_prl_current_collection` |
| `B` | `B` (`DBY`) | whatever is left after every head — the member's excess, credited to the `DBY` ledger (SUNDRY CR EXCESS RECOVERY); `creditor_led_id`, `creditor_collection`. Recovery Refund pays it back. |

## 4. Appropriate — per pay office

Allowed for a **posted** demand (`isdemandupdated`) whose office status is live, not appropriated and not saved,
when no later demand is posted, on the open business day of an ECS branch.

Input: one amount per member (the employer's remittance list), identified by member no (or per no). Every member must
be an open member of the branch whose **current** `mem_master.office_id` is the office — anyone else is reported and
nothing is written. A member may appear once. Members of the office who are not listed recover nothing.

### 4.1 Revised demand (`UpdateSubsequentRecoveryInDemand`)

Money can reach an account between posting and appropriation (a counter recovery, a top-up closing a loan). The split
is made against the demand as it stands now, stored in the `*2` columns. For every live, **non-stopped** line of the
office's members, from the account's valid ledger rows (all dates, `voc_status 'V'`, not deleted, branch):

| Line | Revised |
|---|---|
| L principal | arrear2 = arrear, current2 = current; then capped, arrear first, at the principal outstanding Σ `Disb_Amt` − Σ `PrlColl_Amt` |
| L interest | collected since = (int arrear + int current) − (Σ `IntCalc_Amt` − Σ `IntColl_Amt`). Taken off arrear, then current. A negative figure (interest accrued after the demand, e.g. by a counter recovery) **adds** to the arrear. |
| L penal | the same with `PICalc_Amt` / `PIColl_Amt` |
| D | unchanged (TSIS stopped revising deposits in 2021) |
| S (due-to) | arrear2 = 0, current2 = the member's balance on the ledger now (Σ `Pmt_Amt` − Σ `Rpt_Amt`, `Trn_Type 1`), floored at 0 |
| R | unchanged (§14.5) |

Demand 154's `*2` columns show this rule: loan `110050004010` had interest arrear 0 at calculation; a top-up closed it
on 07-10-2026 and accrued ₹429 more interest, so TSIS's `loan_int_arrear2` is 429 and the member's ₹5,478 went
429 + 2,145 to interest and 2,904 to principal.

### 4.2 Split — `mem_collection_priority`, ascending `priorityno`, active rows of the branch

For each member, the amount (rounded to paise) is taken head by head; each head takes at most what it is owed and
the rest moves on. Stopped lines (`stop_demand`) are skipped everywhere.

| Type | Lines | Takes |
|---|---|---|
| `DPI` | the D line of `deposit_id` | penal arrear2, then penal current2 |
| `D` | the D line of `deposit_id` | arrear2, then current2 |
| `LPI` / `LI` / `LP` | L lines of `scheme_id`, oldest sanction (`San_Date`) first | penal / interest / principal: arrear2, then current2 |
| `DTO` | S lines of `priorityled_id` (any due-to ledger when it is the branch zero id) | arrear2, then current2 |
| `RD` | R lines | penal (arrear2 + calc2), then the demand — **whole instalments only** |
| `LPA` | open `Loan_Type 1` loans of the member with principal outstanding and no non-stopped L line in this demand | principal, up to the outstanding; writes an `A` line when > 0 |
| `LP1`–`LP3` | L lines of `scheme_id` | principal paid ahead: up to outstanding − principal already taken on the line; added to `loan_prl_current_collection` |
| `DBY` | — | everything left: a `B` line, `creditor_led_id = priorityled_id` |

Branch `11005` order: DPI(FWD) · D FWD · D Thrift · D SRF · Education PI/Int/Prl · Flood relief PI/Int/Prl · Surety
PI/Int/Prl · DTO · RD · LPA · LP1 Education · LP2 Flood · LP3 Surety · DBY.

If no `DBY` row exists and money is left, the appropriation is refused (the voucher would not balance).

Line totals: `loan_total_collection` = the six loan columns, `deposit_total_collection` = the four deposit columns,
`debtor_total_collection`, `rd_total_receipt`, `creditor_collection`; `total_collection` = all of them.

### 4.3 Writes

One transaction: the `*2` and collection columns; `recovery_id` = MAX(`recovery_id`) + 1 in the branch, one per run;
`recovery_date` = business day and `is_appropriated` on the office's non-stopped lines; the new `A` / `B` lines (MAX+1
ids, `calculated_date` = business day); `mem_demand_status.is_appropriated`; the master's `isrecoveryappropriated`
once every live office is appropriated.

## 5. Delete appropriation

Refused once the office is saved. Soft-deletes the office's `A` / `B` lines (`is_active = false`), zeroes every
collection column, `recovery_date`, `recovery_id`, `is_appropriated` on the office's lines, clears the status flag and
the master's `isrecoveryappropriated`.

## 6. Save recovery — per pay office

Allowed when the office is appropriated and not saved (open business day, ECS branch).

### 6.1 Recheck

The revised demand (§4.1) is recomputed; if any line differs from the stored `*2` figures the save is refused ("the
ledgers changed since the appropriation — delete the appropriation and appropriate again"), so money is never applied
to a balance that no longer exists.

### 6.2 Voucher

| Field | Value |
|---|---|
| `fin_voucher` | `voc_type 2`, `voc_date` = business day, receipt `AR/n` + payment `VR/n` (next numbers in the year and branch), `voc_no = "VR/n-AR/n"`, `voc_amt` = office total, narration `"<office> Factory recovery <Mon yyyy>"` |
| debit lines | the ledger(s) the remittance arrived in, chosen on the page (bank or sundry ledger), amounts must add up to the office total; a bank ledger also writes `fin_voucher_bank` with the cheque / reference |
| credit lines, one per ledger | loan penal / interest / principal → the scheme's `PILed_Id` / `IntLed_Id` / `PrlLed_Id` (L and A lines); deposit → `deposit_masters.led_id`, deposit penal → `pi_led_id`; due-to → the line's ledger; excess (B) → `creditor_led_id`; RD → the scheme's `led_id` |

### 6.3 Ledger rows (only for lines with a collection)

| Line | Row | Values |
|---|---|---|
| L, A | `loan_trn` `R` | `PIColl_Amt`, `IntColl_Amt`, `PrlColl_Amt`; `Trn_Date` = the latest of the due date, the loan's `San_Date` and its last transaction date; `Demand_Id` |
| D | `deposit_trn` | `Receipt_Amount` = arrear + current, `Penal_Interest_Received_Amount` = both penal columns; `Trn_Date` = due date; `Demand_Id` |
| S | `mem_trn` | `Trn_Type 1`, `Rpt_Amt`, due date |
| B | `mem_trn` | `Trn_Type 2`, `Led_Id` = `creditor_led_id`, `Rpt_Amt`, due date |
| R | `termdeposit_trn` | `DepositReceiptAmount`, `PenalReceivedAmount`, instalments, due date, `Demand_Id` |

All carry the voucher id, next `Trn_SlNo` per account, MAX+1 ids (in-memory allocator, spec 10 §13.2).
Then: `voc_id` and `is_recovery_saved` on the office's non-stopped lines, the status row saved with the voucher, and
the master's `isrecoveryupdated` once every live office is saved.

### 6.4 Nil recovery

An office whose appropriation collected nothing is saved without a voucher (status saved, `voc_id` = branch zero id),
so the next demand is not blocked by an employer that remitted nothing.

## 7. Delete saved recovery

Refused when a later demand exists (its arrears assume these receipts) or when any loan or deposit receipt of the
voucher has a later transaction on the same account. Otherwise soft-deletes, by voucher id, the `loan_trn`
(`TrnTr_Delete`), `deposit_trn` (`Is_Active = false`), `mem_trn`, `termdeposit_trn`, `fin_voucher`,
`fin_voucher_trn` and `fin_voucher_bank` rows; resets `voc_id` / `is_recovery_saved` on the lines, the status row and
the master's `isrecoveryupdated`. The appropriation remains.

## 8. Spec 12 changes

- **Calculate** also needs the previous demand's recovery saved for every office (spec 12 §7 deferred this here).
- **Delete posted demand** is also refused once any office of the demand is appropriated.

## 9. The page — `Pages/RecoveryAppropriation/RecoveryAppropriation.razor` (`/recovery-appropriation`)

Admin, whole-page ECS gate (the `ECSLoanSanction.razor` pattern with `isReady`).

1. **Demand** — the posted demands still being recovered (and the last saved one, for deletes), with one row per
   office: members, demand, appropriated / saved, recovery total, voucher.
2. **Appropriate** an office — a grid of the office's members with their demand; the recovery column starts
   empty and can be filled from the demand ("Recover in full"), typed, or loaded from a CSV (`member no, amount`,
   matched by member no or per no). Unknown or other-office members are listed and block the run.
3. **Appropriated** office — per member the split (loan / deposit / due-to / RD / advance / excess) with a drill-down
   per line; **Delete appropriation**; **Save recovery** with the debit ledger(s), amounts and cheque details.
4. **Saved** office — voucher number and totals; **Delete saved recovery**.

## 10. Server design (vertical slice per CLAUDE.md)

| Layer | New / changed |
|---|---|
| Models | `Mem_CollectionPriority` mapping fix (§2) |
| Repository | `EcsRecoveryRepository` (+ interface, `UnitOfWork` in 3 places): masters/status/lines, members by office, priorities, loan info and balances, due-to balances, loans not in demand, ledgers, scheme/deposit ledgers, voucher numbers, later-transaction checks, receipts by voucher, id allocator. Query syntax, `BrCode` on every alias. |
| Handler | `EcsRecoveryHandler`: `GetStateAsync`, `GetOfficeAsync`, `AppropriateAsync`, `DeleteAppropriationAsync`, `SaveAsync`, `DeleteSavedAsync`. Rules in a pure `EcsDemandAppropriator` (revise + split, no DB) beside `EcsDemandCalculator`. |
| Controller | `EcsRecoveryController` (`api/ecsrecovery/...`), ECS gate, `BrCode` / user / year / business day from claims |
| UI | §9 page, replacing the placeholder |

## 11. Verification

1. **Acceptance — reproduce demand 154.** On a copy of the database, put demand 154 back to "posted, not
   appropriated" (soft-delete TSIS's receipts and vouchers `110050009730`, `…734`, `…735`, zero the collection
   columns, clear the flags), feed each office the per-member amounts TSIS recovered (Σ `total_collection`), and
   compare: every line's collection columns; per office the receipts per loan / deposit (amount and date) and the
   voucher's lines per ledger (HO: thrift 1,68,160, FWD 4,000, SRF 800, surety principal 1,11,361, surety interest
   74,061, debit SUNDRY CR MRF 3,58,382). List every difference and explain it.
2. Save with a bank ledger: `fin_voucher_bank` written; voucher balances.
3. Delete saved recovery → rows soft-deleted, office back to appropriated; delete appropriation → collections zero,
   `A` / `B` lines gone; appropriate again → same result.
4. A member of another office / an unknown member no / a duplicate → refused, nothing written.
5. Excess: a member remitting more than the demand → `B` line and `mem_trn` type 2 on SUNDRY CR EXCESS RECOVERY.
6. The next demand cannot be calculated until every office is saved; a posted demand with an appropriated office
   cannot be deleted.
7. A PCARDB branch gets the gate and a 403.

## 12. Not in this spec

Recovery Refund, CTC appropriation, restricted demand, Group E reports, the Excel (`.xls`) upload — the page takes
CSV.

## 13. Decisions

1. **Unit of work:** one pay office at a time, as TSIS — employers remit separately and on different days.
2. **Dates:** receipts are dated as TSIS dated them (due date, or later per §6.3); the voucher and `recovery_date` use
   the business day (TSIS let the user type a recovery date).
3. **Excess** goes to the `DBY` ledger as a `B` line, to be refunded by the Recovery Refund page.
4. **Line flags:** `is_appropriated` / `is_recovery_saved` are kept on `mem_demand` too (TSIS left them false).

## 14. Departures from TSIS

1. `DTO` with the zero ledger id means every due-to ledger (TSIS compared it literally and never matched); TSIS also
   never took the current due-to amount off the remaining recovery.
2. `LP1`–`LP3` work per loan; TSIS accumulated the advance across the member's loans of the scheme and added the
   running total to each.
3. `LPA` writes an `A` line only when it takes money (TSIS wrote zero lines for every open loan).
4. The master's `isrecoveryappropriated` is set when every office is appropriated (TSIS tested the opposite
   count and so never set it — every migrated master has it false).
5. RD revised demand is the calculated demand (no RD accounts in `11005`); TSIS recalculated pending instalments.
6. Save refuses when the revised demand has changed since the appropriation (§6.1); TSIS saved stale figures.
7. Delete saved recovery checks later transactions and later demands; TSIS's check was commented out.
8. `mem_master.isnewmember` is not cleared on save (nothing in ECS reads it).
9. Heads take at most what is owed — a negative revised figure takes nothing (TSIS could add it back to the
   remaining recovery).

## 15. Implementation notes

- **Files.** `BusinessLogic/Concrete/EcsDemandAppropriator.cs` (pure: `Revise`, `Appropriate`), `EcsRecoveryHandler.cs`,
  `Repository/Concrete/EcsRecoveryRepository.cs` (+ `EcsRecoveryIdAllocator`), `Dto/EcsRecoveryVM.cs`,
  `UI/Controllers/EcsRecoveryController.cs` (`api/ecsrecovery/State|Office/{demandId}/{officeId}|Appropriate|
  DeleteAppropriation|Save|DeleteSaved`), `UI.Client/Pages/RecoveryAppropriation/RecoveryAppropriation.razor`.
- Masters, status rows and the open business day are read through `EcsDemandRepository`; voucher numbers through
  `EcsLoanDisbursementRepository.GetNextVoucherSlNosAsync` (AR/VR per year and branch).
- The page sends amounts keyed by member no; a CSV keyed by per no is mapped to member nos in the browser.
- The debit-ledger list is every live ledger of the branch (bank ledgers from `map_banks` are flagged and need a
  cheque / reference no). It defaults to the debit ledger of the office's last saved recovery.

**Verified (14-11-2026, on `ECS_spec13`, a `pg_dump` copy of `ECS`; business day 23-10-2026).** Demand 154 was put back
to "posted, not appropriated" as §11.1 describes and each office was given the per-member amounts TSIS recovered
(HO 40 members ₹3,58,382; Staff 36, ₹2,17,278; Workmen 591, ₹44,46,787):

- **Appropriation:** every collection column of all 2,716 lines identical to TSIS. Revised (`*2`) figures identical
  except member 433's four lines, which TSIS left at 0 (the member was skipped by TSIS's run — no recovery id; a loan
  closure on 01-10-2026, voucher `…9732`, was later deleted); they recover nothing either way.
- **Save:** the three vouchers identical to TSIS's ledger for ledger (e.g. HO: thrift 1,68,160, FWD 4,000, SRF 800,
  surety principal 1,11,361, surety interest 74,061, Dr SUNDRY CR MRF 3,58,382; Workmen includes FWD penal ₹30).
  Deposit receipts identical (120 / 108 / 1,773 rows). Loan receipts identical in amount (14 / 23 / 435 rows); 13 are
  dated 23-10-2026 instead of 01-10-2026 because those loans were closed by ECS top-ups (vouchers `…9736`, `…9738`) on
  23-10-2026 — after TSIS saved, but before this replay — and §6.3 dates a receipt no earlier than the loan's last
  transaction.
- Also exercised: master flags; delete saved refused while demand 155 exists, then allowed (rows soft-deleted, office
  back to appropriated); spec 12 calculate refused and delete-posted refused while an office is appropriated; delete
  appropriation (collections zeroed, `A` line removed); bad list (unknown member, other office, duplicate) refused with
  every reason; overpayment by a member with another open loan → `A` advance line; by a member without loans → `B`
  line, `mem_trn` type 2 and a credit to SUNDRY CR EXCESS RECOVERY; bank debit → `fin_voucher_bank`, missing cheque no
  and unequal remittance refused.

The handler was driven from a scratch console harness against the copy. **The page itself was not driven** (it needs a
login); it builds, and uses the same endpoints.

## 16. Recovery Refund (`/ecs-recovery-refund`)

Ported from TSIS `frmFactoryRecoveryRefund` / `DatabaseDemand.Save_FactoryRecoveryRefund`. A refund does **not** pay money
out: it takes part of one member's saved recovery off their loan, deposit or RD receipts and credits it to the excess
recovery ledger (the `DBY` priority ledger, SUNDRY CR EXCESS RECOVERY), from where it is paid back to the member — the
same place an over-remittance lands (§4.2 `B`).

**Allowed** for a member whose pay office's recovery of the demand is saved with a voucher, when no later demand
exists and none of the member's refunded accounts has a transaction after its receipt (higher `Trn_SlNo`). Each amount
is at most what that receipt recovered.

**Writes** (one transaction, editing the saved recovery in place, as TSIS did):

| What | Change |
|---|---|
| `loan_trn` `R` receipt | `PrlColl_Amt` / `IntColl_Amt` / `PIColl_Amt` reduced |
| `deposit_trn` receipt | `Receipt_Amount` / `Penal_Interest_Received_Amount` reduced |
| `termdeposit_trn` receipt | `DepositReceiptAmount` / `PenalReceivedAmount` reduced, instalments recounted |
| `fin_voucher_trn` | the credit lines of those ledgers reduced; the excess ledger's credit line increased (added when missing). The debit side and `voc_amt` are unchanged, so the voucher still balances. |
| `mem_demand` | the line's collections reduced (current first, then arrear); a new `B` line with `creditor_collection` = `total_collection` = the refund, `voc_id`, saved |
| `mem_trn` | `Trn_Type 2`, `Rpt_Amt` = the refund, on the excess ledger, dated the due date |

Because everything stays on the recovery voucher, **Delete saved recovery** (§7) reverses a refund with the rest, and
**Delete appropriation** removes its `B` line.

**Menu:** `12-ecs-demand-calculation.sql` re-points the "Recovery Refund" placeholder under "Thrift Society Demand"
(was `under-development`) at `/ecs-recovery-refund`, or inserts the row — same per-branch, idempotent pattern.

**Departures from TSIS:** the refund `B` line carries the amount in `total_collection` too (TSIS left it 0, so office
totals no longer matched the voucher — 34 legacy `B` rows have that shape); collections are unwound current-first
(TSIS took arrear first); SRF refunds are included in the excess total (TSIS left them out, unbalancing the voucher);
the SRF voucher line is reduced by the SRF refund (TSIS used the FWD amount); RD voucher lines are found by ledger (TSIS
passed the RD transaction id as the ledger id); refunds are refused after later transactions or a later demand (TSIS
had no check). Only one demand was selectable in TSIS (the latest saved); ECS offers every demand the Recovery
Appropriation page lists.

**Verified (14-11-2026, `ECS_spec13`).** HO member 003214 on demand 154: ₹1,000 principal + ₹500 interest of surety loan
1003146 and ₹2,000 thrift refunded → loan receipt 3,167 / 3,245, thrift receipt 0, voucher still 3,59,082 on both
sides with surety principal −1,000, surety interest −500, thrift −2,000 and the excess ledger +3,500, `B` line and
`mem_trn` type 2 of 3,500, office total unchanged. Over-refund refused; a later loan row (inserted on the copy) blocked
the refund; delete saved recovery reversed the refund's `mem_trn` with the rest. The menu SQL was run twice on the copy
(one row, re-pointed). The page was not driven (needs a login).
