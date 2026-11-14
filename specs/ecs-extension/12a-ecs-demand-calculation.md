# 12 — ECS Monthly Demand Calculation

> **Status:** Draft for review. No code written. Builds on the existing (unused) `ECSDemandCalculationHandler`
> and the `mem_demand_master` / `mem_demand` tables. Open points are in §14.

## 1. Purpose

Employees' thrift and credit societies (ECS) do not collect over the counter. Each month the society
sends a **demand** to every member's employer. The employer deducts it from salary and remits one
amount to the society, which is then appropriated against the demand (recovery, a separate spec).

One demand covers, per member:

| # | Head | Source | `Mem_Demand.Demand_Type` |
|---|---|---|---|
| 1 | Loan principal (current + arrear) | `loan_master` / `loan_trn` | `L` |
| 2 | Loan interest up to month end | `loan_trn` + `loan_roi` | `L` |
| 3 | Loan penal interest up to month end | `loan_trn` + `loan_roi` | `L` |
| 4 | Thrift Deposit (deposit_id 1), no arrear | `deposit_masters`, `deposit_options`, `deposit_trn` | `D` |
| 5 | Family Welfare Fund (deposit_id 2), current + arrear | same | `D` |
| 6 | *(blank in the brief; see §14 Q1)* | — | — |
| 7 | Surety Relief Fund (deposit_id 3), current + arrear, monthly or annual | same | `D` |
| 8 | Recurring Deposit instalment (`tdscheme_id = 40001`) | `termdeposit_master` / `termdeposit_trn` | `R` |
| 9 | Sundry debtor balance to be recovered | `mem_trn` | `S` |

## 2. The two runs

| | Preliminary demand | Final (month-end) demand |
|---|---|---|
| When | Last week of the month, on user request | Day End of the **last business day of the month** |
| "As on" date for interest/penal | Month end (projected) | Month end (actual) |
| Stored in | `mem_demand_master`, `mem_demand` | Same tables (the preliminary rows are replaced) |
| Posted to transaction tables | No | Yes: `loan_trn`, `deposit_trn`, `termdeposit_trn`, `mem_trn` |
| Output | Employer-wise demand list or report sent to employers | Ledger postings, plus the demand that recovery appropriates against |

The preliminary run is a **projection**: transactions between the run date and month end (part
payments, new disbursements, closures, rate changes, option changes) can change the figures. That is
why the final run deletes and recalculates.

## 3. Demand header — `mem_demand_master`

One row per branch per demand month.

| Column | Value |
|---|---|
| `Demand_Id` | MAX+1 (application side, per CLAUDE.md) |
| `DemandDate` | Month-end date the demand is for (e.g. 2026-11-30) |
| `DemandCalculationDate` | Business day the calculation ran |
| `NextDemandDate` | Next month end |
| `IsDemandUpdated` | `false` after the preliminary run; `true` once the final run has posted to the transaction tables |
| `IsRecoveryAppropriated`, `IsRecoveryUpdated`, `RecoveryUpdatedVoc_Id` | Left for the recovery spec |
| `Is_Demand_Closed` | Set by recovery when the month is closed |
| `SocietyType` | 3 (ECS) |
| `Scheme_Id` | 0 (the demand covers all schemes) |
| `Demand_OE` | `false` (`true` only for migrated opening demands) |
| `BrCode`, `Usr_Id`, `Yr_Id`, `Voc_Status = "V"`, `Is_Active = true` | Standard |

Rule: at most one active `mem_demand_master` per `BrCode` + `DemandDate`. Re-running the preliminary
demand for the same month is allowed only while `IsDemandUpdated = false`, and it replaces the earlier run.

## 4. Demand detail — `mem_demand`

One row **per member per head**: one per loan (`L`), one per deposit type (`D`), one per RD account
(`R`) and one per sundry ledger (`S`). Every row carries `Demand_Id`, `Mem_Id`, `Calculated_Date`,
`Demand_Type`, `Total_Demand`, `BrCode`, `Usr_Id`, `Yr_Id`, `Is_Active`.

Column use per head:

| Head | Columns |
|---|---|
| Loan `L` | `Loan_Id`, `Prl_Schedule`, `Loan_Oustanding` (sic), `Loan_Prl_Arrear`, `Loan_Prl_Current`, `Loan_Int_Arrear`, `Loan_Int_Current`, `Loan_PI_Arrear`, `Loan_PI_Current`, `Loan_Total` |
| Deposit `D` | `Deposit_Id`, `Deposit_Arrear`, `Deposit_Current`, `Deposit_Total` (`Deposit_PI_*` = 0 unless §14 Q6 says otherwise) |
| RD `R` | `TD_Id`, `RD_Instalment`, `RD_NoOf_Instalments`, `RD_Demand`, `RD_PI_Arrear`, `RD_Total` |
| Sundry `S` | `Debtor_Led_Id`, `Debtor_Arrear`, `Debtor_Current`, `Debtor_Total` |

The `*2` columns (`Loan_Total2`, `Total_Demand2`, …) hold the **preliminary** figures. On the final
run the recalculated figures go in the main columns and the preliminary figures are copied to the `*2`
columns, so the society can see what changed between the run sent to employers and the month-end
figures (see §14 Q3).

The employer is `mem_master.office_id` (resolved through `Refer_Data`). Demand lists and reports
are grouped by `office_id`. `mem_demand_status` already has `Office_Id` for per-office tracking.

## 5. Members in scope

- `mem_master.brcode` = branch, not `memberdelete`, not `isaccountclosed`, not `ismemexpired`.
- **Stop demand:** skip members (or single loans or heads) listed in `mem_demand_stop` for the demand
  date, and members with `memberstatus = 2`. Both checks apply; one must not overwrite the other
  (a known bug in the current `CalculateLoanDemand`).
- A member with no head producing a non-zero total gets no rows.

## 6. Loan demand (heads 1–3)

**Loans in scope:** `loan_master` with `BrCode` = branch, `Loan_Type = 1`, `Voc_Status = 'V'`,
not `Loan_Delete`, not `IsAccountClosed`, and with a balance (principal, interest or penal > 0).
`ECSDemandCalculationRepository.GetLoanIdsToCalculateECSDemand` already selects these; add the
missing `BrCode` and closed-loan filters.

**Balances come from `loan_trn`** (`Voc_Status = 'V'`, not `TrnTr_Delete`, `BrCode` on every alias,
`Trn_Date` ≤ demand date), never from `San_Amt`. This is the same rule as spec 11 §4.1.

| Figure | Formula |
|---|---|
| Principal outstanding | Σ `Disb_Amt` − Σ `PrlColl_Amt` |
| Principal arrear | Σ `Prl_Dem` − Σ `PrlColl_Amt` (floor 0) |
| Interest arrear | Σ `IntCalc_Amt` − Σ `IntColl_Amt` |
| Penal arrear | Σ `PICalc_Amt` − Σ `PIColl_Amt` |

### 6.1 Principal — current demand
By `loan_schemes.Inst_Type` (the existing logic in `CalculateLoanDemand`):
- 1, 3 (fixed principal): `Inst_Amt`
- 2 (EMI): `Inst_Amt` − current interest
- 4 (bullet): 0 until `Last_DueDate`

The current demand is capped at (principal outstanding − principal arrear). On or after `Last_DueDate`,
the whole remaining outstanding is demanded. No principal demand is raised before `FirstPrl_DueDate`.

### 6.2 Interest — current demand
Interest from the day after the last interest calculation (`LastIntApplication_Date`, or the last
`IntCalc_Date`) **up to and including the month end**. Use `EcsInterestCalculator.Calculate(EcsAccrualHead.Interest, …)`
with `applicationDate = monthEnd + 1`: segment-wise diminishing product, rates from `loan_roi` by `Roi_Wef`,
rounded once with `AwayFromZero`. Do **not** reuse `CalculateLnDues` (spec 11 §6.5).

### 6.3 Penal — current demand
The same calculator with `EcsAccrualHead.Penal`. Penal accrues only when
`EcsInterestCalculator.PenalApplies(pi_application)` (2 or 3; 0 and 1 mean no penal).

### 6.4 Loan total
`Loan_Total = Prl arrear + Prl current + Int arrear + Int current + PI arrear + PI current`.

## 7. Deposit demand (heads 4, 5, 7)

Driven entirely by `deposit_masters` rows (`Is_Active`, `BrCode` = branch). Nothing branches on
`Deposit_Id` in code: Thrift, FWF and SRF are ids 1, 2 and 3 in data only. Any further active deposit row
is picked up the same way. This replaces the hard-coded `Deposit_Id <= 3` in today's code.

### 7.1 Is a demand due this month?
- `Frequency = 'M'`: every month.
- `Frequency = 'A'`: only when the demand month = `Demand_Month` (1–12). In other months the current
  demand is 0, but any arrear is still demanded if arrears are applicable.

### 7.2 Current amount (per member)
1. If `Is_Option_Allowed` and the member has an active `deposit_options` row for this `Deposit_Id`
   with `Wef` ≤ demand date, take the latest such row's `Option_Amount`.
2. Otherwise, if `Percentage > 0`: `basicpay × Percentage / 100`, capped at `Maximum_Amount`
   when `Maximum_Amount > 0`.
3. Otherwise, if `Fixed_Amount > 0`: `Fixed_Amount`.
4. Otherwise 0.

Round to the rupee (`AwayFromZero`). See §14 Q4 on this order of precedence.

### 7.3 Arrear
- `Is_Arrear_Demand_Applicable = false` (Thrift): arrear = 0. An unpaid month is not carried forward.
- `true` (FWF, SRF): arrear = Σ `deposit_trn.Demand_Amount` − Σ `Receipt_Amount` for the member and
  deposit, up to the previous demand (floor 0).

`Deposit_Total = Deposit_Arrear + Deposit_Current`.

The helper `DepositTrnRepository.Calculate_Current_Deposit_Demand` already implements most of §7.1–7.2.
The outer `CalculateDepositDemand` is broken: the master list is never loaded, and the arrear is written
to `Deposit_PI_Arrear`. Fix it rather than copy it.

## 8. Recurring deposit demand (head 8)

**Accounts in scope:** `termdeposit_master` with `BrCode` = branch, `TDScheme_Id = 40001`, `Status = 'Y'`,
not `TD_Delete`, not `AccountClosed`, `Voc_Status = 'V'`, `AccountOpenDate` ≤ demand date and
`MaturityDate` > demand date. See §14 Q5 on the status value and the hard-coded scheme id.

- `RD_Demand` = monthly instalment (`DepositAmount`).
- `RD_PI_Arrear` / arrear: Σ `DepositDemandAmount` − Σ `DepositReceiptAmount` on `termdeposit_trn`
  (see §14 Q6 on whether RD arrears and penal are demanded).
- `RD_Instalment` = instalment number due; `RD_NoOf_Instalments` = `PeriodInMonths`.
- No demand once all instalments have been demanded.

`RecurringDepositRepository.CalculateRecurringDepositDemand` currently always returns an empty list
(`TdIdList` is never filled, and the wrong list is checked) and needs fixing.

## 9. Sundry debtor demand (head 9)

For each sundry-debtor ledger of the member: balance = Σ `mem_trn.Pmt_Amt` − Σ `Rpt_Amt` where
`Trn_Type = 1` (due-to) and the ledger has `Fin_Ledger.Led_Sundry_Type = 1`, with `Voc_Status = 'V'`,
not `MemTrn_Delete`, `BrCode` on every alias. If balance > 0, demand it in full as `Debtor_Current`, with
`Debtor_Arrear` = 0. Use **both** filters; the two existing queries each use only one of them.

## 10. Preliminary run (last week)

New page **ECS Demand Calculation** (`/ecs-demand-calculation`, gated to ECS as in `ECSLoanSanction.razor`):

1. Shows the demand month (the month end of the current business day) and whether a demand already
   exists and in what state.
2. **Calculate** runs §5–§9 as on the month end and stores the master and detail rows in one transaction.
   If a preliminary demand for the month already exists (`IsDemandUpdated = false`), the user confirms
   that it will be replaced.
3. Shows a summary grid by employer (`office_id`): member count and totals per head. Members can be
   expanded to show their rows.
4. Prints the employer-wise demand list or report and exports it (the employer schedule).

Refused when the month's demand is already final (`IsDemandUpdated = true`).

## 11. Final run at month end (Day End)

Applies when Day End is run on the **last business day of the month** for an ECS branch
(`Bank_Type = 3`). It fills the empty "Step 7 — demand" region in `CalendarHandler.DayEndProcess`.

### 11.1 User confirmation
Before Day End proceeds, `DayEndProcess.razor` asks:

> Month-end demand for November 2026: the preliminary demand calculated on 24-11-2026 (₹ X, N members)
> will be **deleted**, recalculated as on 30-11-2026, and **posted to the loan, deposit, RD and sundry
> ledgers**. Proceed?

- **Yes**: Day End continues with §11.2 inside its existing transaction.
- **No**: Day End is cancelled, because the month cannot close without its demand (see §14 Q7).
- If no preliminary demand exists, the prompt says "will be calculated and posted" instead.

### 11.2 Steps (one transaction, rolled back as a whole on failure)
1. Copy the preliminary figures into memory (for the `*2` columns), then delete the preliminary
   `mem_demand` rows (hard delete, since nothing references them yet).
2. Recalculate §5–§9 as on the month end and insert the rows. Keep the same `Demand_Id` and update
   `DemandCalculationDate`.
3. Post each row to its transaction table, stamped with `Demand_Id`, `Trn_Date` = month end, and
   `Voc_Id` of the month-end voucher:

| Head | Table | Columns written |
|---|---|---|
| Loan principal | `loan_trn` | `Prl_Dem` = current principal demand, `Due_Date` |
| Loan interest | `loan_trn` | `IntCalc_Amt`, `IntCalc_Date` = month end; update `loan_master.LastIntApplication_Date` |
| Loan penal | `loan_trn` | `PICalc_Amt`, `PICalc_Date` = month end; update `LastPIApplication_Date` |
| Deposit | `deposit_trn` | `Demand_Amount` = current, `Due_Date`, `Status`, `Demand_Id` |
| RD | `termdeposit_trn` | `DepositDemandAmount`, `NoOfInstalments`, `InstalmentAmount`, `Demand_Id` |
| Sundry debtor | `mem_trn` | `Demand_Amt` on a demand row for the ledger (no `Pmt`/`Rpt` amounts) |

   Arrears are **not** posted again; they are already in the tables from earlier months.
4. Pass the ledger vouchers (§12).
5. Set `IsDemandUpdated = true`.

IDs use MAX+1. Several rows go into the same table in one transaction, so use an in-memory allocator
as `EcsLoanDisbursementRepository` does (spec 10 §13.2).

## 12. Ledger postings ("transfer to concerned ledgers")

Proposed. Confirm in §14 Q2.

| Head | Debit | Credit |
|---|---|---|
| Loan interest accrued | Interest receivable (per scheme) | Interest income `loan_schemes.IntLed_Id` |
| Loan penal accrued | Penal interest receivable | Penal income `PILed_Id` |
| Loan principal demand | No voucher (memo: demand only) | — |
| Deposit / RD / sundry demand | No voucher (memo: demand only; the money is accounted at recovery) | — |

One consolidated `Fin_Voucher` per branch for the month end, with `Voc_Status = 'V'`.

## 13. Server side (vertical slice per CLAUDE.md)

- **Handler:** complete `ECSDemandCalculationHandler` (already registered). Split it into
  `CalculateAsync` (pure, returns rows), `SavePreliminaryAsync` and `FinaliseAsync` (called from
  `CalendarHandler.DayEndProcess`).
- **Repositories:** fix `LoanTrnRepository.CalculateLoanDemand` (or replace it with a calculator-based
  path), `DepositTrnRepository.CalculateDepositDemand`,
  `RecurringDepositRepository.CalculateRecurringDepositDemand` and `MemTrnRepository.CalculateDueToDemand`.
  Add `DeleteByDemandIdAsync` to `MemDemandRepository`.
- **Controller:** new `EcsDemandController` with `GET Status/{month}`, `POST Calculate`, `GET Summary/{demandId}`
  and `GET MemberRows/{demandId}/{memId}`, server-side ECS check as in `LoanSanctionController.IsEcsAsync`.
  Plus `GET api/BusinessDay/MonthEndDemandInfo` for the Day End prompt.
- **Calculation is pure where possible**, like `EcsInterestCalculator`: the deposit-amount rule (§7.2) and
  the principal rule (§6.1) as static methods, so they can be checked by hand.
- **No schema change expected.** All columns already exist.

## 14. Open points for the reviewer

1. **Item 6 is blank in the brief.** Was a head intended here (share capital, a due-by, insurance)?
2. **Ledger postings (§12).** Should interest and penal be accrued (receivable/income) at month end, or
   only memo-demanded and taken to income at recovery? Should the principal or deposit demand produce a
   voucher at all?
3. **Preliminary vs final differences.** The employer has already been sent the preliminary figures.
   If the final differs, which figure does recovery appropriate against? (The draft keeps both: final in
   the main columns, preliminary in `*2`.)
4. **Deposit precedence (§7.2).** The brief lists fixed, then percentage, then option. The draft uses
   option > percentage > fixed. Which applies if both `Fixed_Amount` and `Percentage` are set? Does
   `Maximum_Amount` also cap an option amount?
5. **RD selection.** The brief says `Status = 'Y'`, but existing code treats `'D'` as active. Which is it?
   Also, `TDScheme_Id = 40001` is a hard-coded id, while elsewhere ECS uses rule columns. Should a
   "demand-based RD" flag on the TD scheme be added instead?
6. **Arrears and penal on RD and deposits.** Is unpaid RD carried forward as arrear, and is penal raised
   on it? Is there penal on FWF/SRF arrears (`Deposit_Roi_Templates` has a penal rate)?
7. **Day End when the user says No.** Should Day End be cancelled, or allowed to continue without the
   demand?
8. **"Last week".** Fixed day (e.g. from the 24th), or any day the user chooses?
9. **Interest "as on end of month" in the preliminary run.** This is a projection on today's balance.
   Confirm that is intended.
10. **Members who retire or close mid-month.** Should they get a final demand of the full outstanding?

## 15. Verification (manual; there are no tests)

- Pick 3–4 members covering: two loans (one EMI, one fixed principal) with arrears; Thrift by percentage
  capped at maximum; an FWF option amount; SRF annual with the demand month and a non-demand month; one
  RD; one sundry debtor.
- Compute each figure by hand from `loan_trn` / `deposit_trn` / `termdeposit_trn` / `mem_trn` and
  compare with the `mem_demand` rows after the preliminary run.
- Add a part payment after the preliminary run, then run month-end Day End. Check that the preliminary
  rows are gone, the new figures reflect the payment, the `*2` columns hold the old figures, the
  transaction-table rows carry `Demand_Id`, and the voucher balances.
- Run Day End and answer **No**: nothing changes.
- Log in as a PCARDB branch: the page shows "ECS only", and Day End shows no prompt.
