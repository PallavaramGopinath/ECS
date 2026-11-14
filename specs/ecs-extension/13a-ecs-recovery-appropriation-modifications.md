# 13a — ECS Recovery Appropriation: modifications (Excel upload, recalculation on the recovery date, Maker / Checker)

> Amends [13-ecs-recovery-appropriation.md](./13-ecs-recovery-appropriation.md), which is implemented and verified
> against TSIS demand 154. Section numbers like "13 §4.1" refer to that spec.
>
> **Status:** implemented. §10 records the decisions taken on the draft's open questions; §11 lists where the code
> departs from the draft; §12 is the verification.

## 1. Why

Pay offices deduct the demand from salaries at month end (Nov 2026 salary, 30-11-2026) but send the recovery list
in the first or second week of the next month (01-12-2026 to 14-12-2026, sometimes later). In between, a borrower
may close the demanded loan, or take a new loan that clears the old one, or pay at the counter. The demand sent to
the pay office is then stale. The recovery must be split against the demand **as it stands on the recovery date**,
not as calculated.

Spec 13 already revises the demand into the `*2` columns (13 §4.1), but only partly:

| Head | Spec 13 today | This spec |
|---|---|---|
| Loan principal | as demanded, capped at outstanding | same (no new principal demand) |
| Loan interest / penal | demand less what was collected since; no fresh accrual | recalculated as on the recovery date (§4.2) |
| Deposit (thrift, FWD, SRF …) | **unchanged** | recalculated (§4.3) |
| RD | **unchanged** | recalculated (§4.4) |
| Sundry debtor (due-to) | ledger balance now | same, as on the recovery date (§4.5) |

It also changes how the list arrives (Excel, §3), who does what (Maker / Checker, §6) and the dates (§5).

## 2. Terms

| Term | Meaning |
|---|---|
| **Salary month** | the month whose salary carried the deduction (Nov 2026). |
| **Recovery date** | the 1st of the month after the salary month (01-12-2026). For a monthly demand this is the demand's due date (`mem_demand_master.demanddate`). Every ledger row is dated on it. |
| **Application date** | the business day on which the Checker saves the recovery (e.g. 09-12-2026). The voucher is dated on it. |

## 3. Excel upload

The pay office sends one sheet per office:

| Column | Maps to | Notes |
|---|---|---|
| Employee No | `mem_master.perno` | used when present |
| Member No | `mem_master.memberno` | used when Employee No is blank or unmatched |
| Employee Name | — | shown beside the matched name for checking; not used for matching |
| Amount recovered from salary | the member's recovery | rounded to paise; > 0 |

- The user selects the demand and the **pay office**, then uploads the sheet (`.xlsx`; `.xls` too — see §10 Q5).
  The first row is a header; columns are found by header text, not position. Blank rows are ignored.
- Match per row: Employee No → open member of the branch with that `perno`; else Member No. A row matching nothing,
  matching a member of another office (by **current** `mem_master.office_id`, as 13 §4), or repeating a member
  already listed, is reported. **Nothing is appropriated while any row is in error** (as today).
- A name mismatch (sheet name vs `mem_master` name) is a **warning**, not an error.
- The page shows the sheet total, matched count and total, and the office's demand total before the Maker
  appropriates. The sheet total is what is appropriated.
- Members of the office missing from the sheet recover nothing (as today). Typing amounts and "Recover in full"
  stay available for corrections.
- The CSV upload of spec 13 is replaced by the Excel upload.
- **Specimen sheet.** For each office the page offers a download (Excel or CSV) of the office's members — Employee No,
  Member No, Employee Name, the member's demand, and a blank *Amount recovered from salary* column — to send to the pay
  office. What comes back is uploaded as it is: `.xlsx`, `.xls` or `.csv`; a blank or zero amount means the member
  recovered nothing (the row is skipped, not an error); the *Demand* column is ignored.

## 4. Recalculation as on the recovery date

Done for every live, non-stopped line of the office's members when the Maker appropriates, and again when the
Checker saves (13 §6.1 recheck — any difference refuses the save). Results go to the `*2` columns; the split
(13 §4.2, `mem_collection_priority` order, unchanged) works only on the `*2` columns.

Ledger sums count valid rows (`voc_status 'V'`, not deleted, branch) **up to the moment of appropriation** — all
dates — so a counter receipt or loan closure made after the due date is seen.

### 4.1 Loan principal — no new principal demand

`prl_arrear2` / `prl_current2` = the demanded principal (arrear + current), capped arrear-first at the principal
outstanding now (Σ `Disb_Amt` − Σ `PrlColl_Amt`). A loan closed since the demand → 0. **No principal is demanded
on a loan that was not in the demand**, even if a new loan was disbursed since; money reaches such a loan only
through the `LPA` / `LP1`–`LP3` advance heads, as today.

### 4.2 Loan interest and penal

Recalculated with `EcsInterestCalculator` (the spec 11 / spec 12 rule, segment-wise diminishing product):

- **Interest arrear2** = max(0, Σ `IntCalc_Amt` − Σ `IntColl_Amt`) over the loan's ledger now, **excluding** the
  demand's own `D` row's `IntCalc_Amt`.
- **Interest current2** = the demand `D` row's `IntCalc_Amt` (accrued to due date − 1) **plus** fresh interest from
  the loan's last `IntCalc_Date` to the recovery date − 1 when that date is later than the `D` row's (it can be,
  when a counter recovery or top-up after the due date moved `IntCalc_Date`). When the recovery date is the due date
  and nothing touched the loan, current2 = the demanded current — the usual case.
- **Penal** the same, with `PICalc_Amt` / `PIColl_Amt` / `PICalc_Date`, only when `pi_application` is 2 or 3
  (`EcsInterestCalculator.PenalApplies`).
- A loan closed since the demand has nothing outstanding → all heads 0; its money moves to the next head.

The fresh accrual (amount and to-date) is kept per line so the save can stamp it (§5.1).

### 4.3 Deposits (`D` lines)

- **arrear2** = (Σ `Demand_Amount` − Σ `Receipt_Amount`) of the deposit before the due date, floored at 0, less
  anything received at the counter since the due date.
- **current2** = the demanded current, less whatever a counter receipt since the due date paid beyond the arrear.
- **penal arrear2 / current2** = the same with `Penal_Interest_Calculated_Amount` / `Penal_Interest_Received_Amount`.
- A deposit closed since the demand → 0 on every head.

### 4.4 Recurring deposits (`R` lines)

Pending instalments recounted as on the recovery date (instalments demanded − instalments received, from
`termdeposit_trn`), penal recalculated on the overdue instalments; an RD closed or matured since → 0. Whole
instalments only, as today. *(Branch `11005` has no RD accounts; the rule cannot be checked against data — §10 Q6.)*

### 4.5 Sundry debtors (`S` lines)

`debtor_current2` = the member's balance on the ledger as on the recovery date (Σ `Pmt_Amt` − Σ `Rpt_Amt`,
`Trn_Type 1`), floored at 0; `debtor_arrear2` = 0 (as today).

### 4.6 Excess

Whatever is left after every head goes to the `DBY` head: a `B` line crediting the member's sundry creditor
(SUNDRY CR EXCESS RECOVERY) account, as today. No `DBY` priority row and money left → refused.

## 5. Posting dates

| What | Date |
|---|---|
| `loan_trn` `R`, `deposit_trn`, `termdeposit_trn`, `mem_trn` | the **recovery date** (01-12-2026) — always, even if saved on 14-12-2026 |
| `fin_voucher` (`voc_date`) and its `fin_voucher_trn` / `fin_voucher_bank` | the **application date** (business day of the Checker's save) |
| `mem_demand.recovery_date` | the application date |

Change from spec 13 §6.3: a loan receipt was dated the latest of due date, `San_Date` and the loan's last
transaction. It is now the recovery date — see §10 Q3 for a loan that has a transaction dated after it.

### 5.1 `loan_trn` `R` row

| Column | Value |
|---|---|
| `PrlColl_Amt` | principal recovered (arrear + current, plus any `LP1`–`LP3` / `LPA` advance) |
| `IntCalc_Amt`, `IntCalc_Date` | the fresh interest of §4.2 (0 in the usual case) and the date it runs to (recovery date); **stamped even when 0**, as spec 12 does, so the period is never accrued twice |
| `IntColl_Amt` | interest recovered |
| `PICalc_Amt`, `PICalc_Date` | the same for penal, when penal applies |
| `PIColl_Amt` | penal recovered |
| `Trn_Date` | recovery date |

Deposit and RD receipts likewise carry any penal calculated in §4.3 / §4.4 (`Penal_Interest_Calculated_Amount` /
`_Date`, `PenalCalculatedAmount` / `PenalAppliedDate`) beside the received amounts.

## 6. Maker / Checker

| Step | Who | What |
|---|---|---|
| Upload + Appropriate | **Maker** (or Admin) | §3 upload, §4 recalculation, 13 §4.2 split; writes `*2` and `*_collection` columns, `A` / `B` lines, office status *appropriated*. No ledger, no voucher. |
| Delete appropriation | Maker (or Admin) | as 13 §5, while not saved |
| Check | **Checker** (or Admin) | sees per member: sheet amount, revised demand, split per head, `A` / `B` lines; office totals and the sheet total |
| Save recovery | Checker (or Admin) | 13 §6: recheck, then receipts + voucher (dates per §5), debit ledger and cheque details chosen by the Checker |
| Reject | Checker | = delete appropriation, with a reason shown to the Maker |
| Delete saved recovery | Checker (or Admin) | as 13 §7 |

- The Checker must not be the user who appropriated (§10 Q4). The appropriating user and time are recorded on the
  office's `mem_demand_status` row.
- Page and API roles change from `Admin` to `Admin,Maker,Checker`; each endpoint enforces its own role on the
  server, not only the button.

## 7. Page changes (`/recovery-appropriation`)

1. Office row → **Upload sheet** (Maker): file picker, preview grid (sheet row, employee no, member no, sheet
   name, matched name, amount, status), errors and warnings, totals.
2. **Appropriate** (Maker) → office shows "Awaiting check" with the maker's name.
3. Checker opens the office → appropriation detail (as today) plus the sheet-vs-demand comparison; **Save recovery**
   (application date shown, recovery date shown) or **Reject**.
4. Saved → voucher no, both dates.

## 8. Server changes (outline, for the implementation that follows review)

| Layer | Change |
|---|---|
| Upload | `api/ecsrecovery/Upload/{demandId}/{officeId}` (multipart) parses the sheet and returns matched rows; nothing is written. Parser library — §10 Q5. |
| Appropriator | `EcsDemandAppropriator.Revise` recalculates D, R and the loan interest / penal as §4; takes the recovery date and the fresh accrual from `EcsInterestCalculator`. Stays pure. |
| Repository | deposit and RD balances as at a date; loan `IntCalc_Date` / `PICalc_Date`; members by `perno`. |
| Handler | `SaveAsync` dates ledger rows on the recovery date, voucher on the business day; stamps `IntCalc_*` / `PICalc_*` on `R` rows. Maker ≠ Checker check. |
| Controller | role per action (§6). |

## 9. Verification (when built)

1. Demand 154 replay (13 §11.1) with the three offices' amounts supplied as Excel sheets → same collections as
   spec 13, except where §4.3 / §4.5 / §5 deliberately differ; list each difference.
2. A loan closed by a top-up between due date and appropriation → its heads 0, the money flows on to the next head
   or the excess `B` line.
3. A thrift deposit paid at the counter after the due date → deposit current2 reduced by it.
4. Saved on a later business day: ledger rows dated on the due date, voucher dated on the business day; `R` rows
   carry `IntCalc_Date` / `PICalc_Date`.
5. Sheet with an unknown employee no, another office's member, a duplicate → refused, nothing written; name
   mismatch → warning only.
6. Maker appropriates; same user cannot save; a Checker saves; Checker reject returns the office to the Maker.

## 10. Decisions

| # | Question | Decision |
|---|---|---|
| 1 | Recovery date | The demand's due date (`mem_demand_master.demanddate`). |
| 2 | Interest to which date | To the receipt date − 1, so the receipt and the next demand never accrue the same days twice. |
| 3 | Loan with a row after the recovery date | Spec 13's rule stays: a loan receipt is dated no earlier than the loan's last transaction (or its sanction); its fresh accrual runs to that date. Deposit, RD and `mem_trn` rows are always on the due date. |
| 4 | Maker ≠ Checker | The user who saves or rejects must not be the one who appropriated — **Admin is exempt** and may do both. |
| 5 | Excel format | `.xlsx` and `.xls`, parsed on the server with ExcelDataReader (MIT). The first worksheet of one file = one office. CSV upload removed. |
| 6 | RD | §4.4 adopted, via `EcsDemandCalculator.Rd` with the RD's receipts to date (unverified: no RD data in `11005`). |
| 7 | Sheet total vs demand | No tolerance. The upload preview shows sheet total, matched total, office demand and the difference; information only. |
| 8 | Employee No and Member No name different members | **Error.** Every row must match an open member of the branch whose current `office_id` is the chosen office; unmatched, other-office, repeated or conflicting rows are errors and nothing is appropriated. Upload and Appropriate both refuse an office that is already appropriated or saved. A name mismatch is a warning. |
| 9 | Offices listed on the page (societies with 100+ pay offices) | Only offices whose recovery is **not saved** (Pending or Awaiting check); a demand with none left, such as the last fully saved one, is not shown. A *Show saved offices* switch (off by default) brings saved offices back so a saved recovery can still be opened and deleted. A search box narrows the list by office name. Client-side only: `api/ecsrecovery/State` is unchanged. |

## 11. Implementation notes and departures from the draft

- **Specimen.** `GET api/ecsrecovery/Specimen/{demandId}/{officeId}?format=xlsx|csv` (Admin, Maker), built by
  `EcsRecoverySheetWriter` (no package: a minimal one-sheet OOXML zip with ids as text so leading zeros survive; the CSV is
  UTF-8 with a BOM). Rows = the office's open members by current `office_id`, demand = their non-stopped lines'
  `total_demand`. The page links to it directly (the HttpOnly auth cookie goes with a same-origin download).
- **Leading zeros.** An Employee No / Member No with no exact match is compared again with leading zeros stripped on
  both sides (a CSV re-saved in Excel turns `001747` into `1747`); still unique-or-error.
- **Files.** `BusinessLogic/Concrete/EcsRecoverySheetReader.cs` (Excel → rows; header found by text in the first 10 rows,
  so a title row may sit above it; a foot row with only a "Total" label is skipped), `EcsDemandAppropriator.Revise`
  (now takes an `EcsLineRevision`), `EcsRecoveryHandler` (`UploadAsync`, `RejectAsync`, matcher shared by upload and
  appropriate), new `EcsRecoveryRepository` queries (`GetAccrualInputsAsync`, `GetDepositRevisionAsync`,
  `GetRdRevisionAsync`, `GetMembersByIdAsync`, `GetUserNamesAsync`), `EcsRecoveryController` (`Upload/{demandId}/{officeId}`
  multipart, 5 MB cap; `Reject`), the page.
- **Schema.** `13a-ecs-recovery-appropriation.sql` adds `appropriated_by`, `appropriated_on`, `rejected_by`, `rejected_on`,
  `reject_reason` to `mem_demand_status` (additive, idempotent). **Run it on every database before deploying** — the
  entity maps the columns, so every `mem_demand_status` query (spec 12 too) fails without them.
- **Roles (server-enforced per action).** State / Office: Admin, Maker, Checker. Upload, Appropriate, Delete appropriation:
  Admin, Maker. Save (including nil recovery), Reject, Delete saved: Admin, Checker. Recovery Refund (`/ecs-recovery-refund`)
  moved from Admin to Admin, Checker, page and API.
- **Interest heads (§4.2).** Still due = Σ `IntCalc_Amt` − Σ `IntColl_Amt` + fresh, floored at 0; arrear2 = Σ calc less this
  demand's own `D` row(s) less Σ collected, capped at still due; current2 = the rest. A collection beyond the arrear
  therefore comes off the current (the draft's `max(0, …)` would have demanded it twice). Penal the same; fresh penal only
  when `pi_application` is 2 or 3. An expired member's accrual stops at the expiry date, as in spec 12.
- **Closed loan.** Principal goes to 0 (capped at outstanding); interest follows the ledger rather than being forced to
  0 — if a closure left interest unpaid, it is still owed (TSIS's ₹429 on loan `110050004010` is this case).
- **Deposits (§4.3).** The arrear base is the demanded arrear capped at the ledger arrear before the due date (so a
  deposit whose arrear is not applicable stays 0). A deposit counts as closed when the member is no longer open or a
  payment since the due date took its balance to nil. No ledger row at all → the demanded figures are kept.
- **Fresh penal on deposits / RD (§5.1).** The recalculation never demands more penal than the demand did, so there is
  never fresh deposit / RD penal to book; those receipts carry received amounts only.
- **`A` (advance) receipts** carry principal only and do not stamp `IntCalc_Date`: the loan was not demanded, its interest
  has not been accrued to the receipt date, and stamping would lose it.
- **`mem_demand.recovery_date`** is set to the appropriation day on appropriate and overwritten with the application
  date on save. Stopped lines keep it empty.
- **Reject** = delete appropriation plus `rejected_by/on/reason`; the reason shows to the Maker until the next
  appropriation clears it. The Maker withdraws their own appropriation with Delete appropriation, not Reject.

## 12. Verification (14-11-2026, `ECS_spec13a`, a `pg_dump` copy of `ECS`; business day 23-10-2026)

Demand 154 was reset as spec 13 §11.1, the SQL run twice (second run: every column skipped), and each office's TSIS
per-member amounts written to an `.xlsx` sheet (title row, header, rows, a "Total" foot row; Employee No stored as a
number). Driven through `EcsRecoveryHandler` from a scratch console harness, Maker = user 3, Checker = user 2, Admin = 1.

1. **Replay.** Upload previews clean (HO 40 rows ₹3,58,382; Staff 36 ₹2,17,278; Workmen 591 ₹44,46,787). All 2,716
   lines' collection columns identical to TSIS except loan `110050006023` (member id 14): ₹4,735 moves from principal
   current to interest current. Cause: an ECS top-up on 23-10-2026 took ₹5,73,490 principal without accruing interest,
   so the receipt is dated 23-10 (decision 3) and 01-10 → 22-10 interest at 13.5 % is now charged (§4.2) and booked on the
   `R` row. Revised (`*2`) figures identical except that loan and member 433's four lines (TSIS skipped that member —
   spec 13 §15). Vouchers identical to TSIS ledger for ledger except surety principal −4,735 / surety interest +4,735.
2. **Dates.** 458 loan receipts dated 01-10, 1 on 07-10 (loan with a later top-up), 13 on 23-10; 2,001 deposit receipts
   on 01-10; vouchers `VR/238-AR/210` … `VR/240-AR/212` dated 23-10; every `R` row has `IntCalc_Date` (= its date),
   the one with fresh interest carries `IntCalc_Amt` 4,735; lines' `recovery_date` = 23-10.
3. **Counter deposit after the due date.** ₹150 received 10-10 on a deposit demanded arrear 90 / current 100 → arrear2 0,
   current2 40. A ₹500 receipt inserted after appropriation made the Checker's save refuse ("D line of member …").
4. **Bad sheet** (unknown employee no, another office's member, a repeat, employee no / member no of different members)
   → four row errors, `CanAppropriate` false; a different name → warning only. Title row and Total row ignored.
5. **Maker / Checker.** The Maker's own save and own reject refused; Checker saw "Awaiting check by …" and saved; Checker
   reject → office Pending with reason and Checker's name; re-appropriating cleared it; Admin appropriated and saved
   the same office.

6. **Specimen.** HO specimen: 95 open members, demand total ₹4,90,442 (= the office demand); unfilled it is refused ("no
   recovery amounts"). Filled with TSIS's 40 amounts as `.xlsx`, as `.csv`, and as a `.csv` with no employee nos and member
   nos stripped of leading zeros: each uploads clean, 40 matched, ₹3,58,382.

**Not verified:** the page (no test login in the project), a real `.xls` file (no tool here writes BIFF; the reader uses
ExcelDataReader's binary reader with the code-page provider registered), RD lines (none in `11005`), a save on a later
business day than the appropriation (the copy's open day is fixed; the recovery-date / application-date split is shown
by 01-10 rows on a 23-10 voucher).
