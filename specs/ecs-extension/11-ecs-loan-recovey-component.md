# 11 — ECS Loan Recovery (acc_id 58, "Loan Receipt")

> Status: **Implemented**. Section 13 lists the decisions taken, section 14 the points still open, and
> section 16 the places where the code departs from the first draft (and why).

## 1. Purpose

Let a Maker collect money against one or more ECS loans of a member from the **Member Transaction** screen.
The screen works out the interest and penal interest due up to the business day. It then splits the amount
received across the selected loans, penal first, then interest, then principal. The Checker's approval posts
it to the loan ledger and the voucher.

Scope is `Loan_Type = 1` (all ECS loans; schemes are told apart by `Scheme_Id`). This screen never raises a
**principal demand**. That happens only in the month-end **ECS Demand Calculation** (section 5).

## 2. Where it lives

| Item | Value |
|---|---|
| Component | `Infin8.Coapp.UI.Client/Components/ECSLoan/ECSLoanRecovery.razor` (new folder `ECSLoan`) |
| View component (Checker / line view) | `Infin8.Coapp.UI.Client/Components/ECSLoan/ECSLoanRecovery_View.razor` |
| Host page | `Pages/Member_Transaction.razor` (`/member-transactions`) |
| Account | `account_transactions.acc_id = 58`, "Loan Receipt", receipt side |
| Application date | the business day, `userInfoDto.CurrentDate`, passed in by the host as `TransactionDate` |

### 2.1 Account row (DDL to apply out of band)

```sql
INSERT INTO account_transactions
  (acc_id, acc_name, acc_type, acc_status, acc_belongsto, acc_delete, component_name, view_component_name)
VALUES
  (58, 'Loan Receipt', 'R', <same value as acc 31>, 'M', false, 'ECSLoanRecovery', 'ECSLoanRecoveryView');
```

The row already exists in the dev database as `(58, ' ECS Loan Recovery', 'R', 'ECSLR', 'M', …,
'ECSLoanRecovery', 'ECSLoanRecovery_View')`; the view component is named to match it. `acc_status` is not
used for the recovery's voucher lines (they carry P / I / PI, section 11).

`acc_type 'R'` and `acc_belongsto 'M'` make it show in the account dropdown when **Receipt** is chosen
(`TransactionsRepository.GetTransactionAccounts`). Copy `acc_status` from the jewel loan recovery row
(acc 31). Check its live value first.

## 3. Maker–checker flow (same as every other Member Transaction account)

Status lives in `staging_master` / `staging_details.staging_status`: `I` initiated → `M` made → `V` verified
(or `R` rejected).

1. **Maker** enters Member No → `handleMemNo` loads the member. They choose Cash/Adjustment, then **Receipt**,
   then the account **Loan Receipt (58)**.
2. `Member_Transaction.razor` → `GetComponentName` needs a new `case 58`. It builds the type
   `Infin8.Coapp.UI.Client.Components.ECSLoan.ECSLoanRecovery` and renders it through the existing
   `DynamicComponent`.
3. The component loads the loans (section 4), and the Maker ticks loans and enters the amount (sections 7–8).
4. **Initiate** → the component posts one `DtoStaging_Details` to `api/Staging/AddStaging`:
   - `Related_Account_Id = 58`
   - `Receipt_Amount` set to the recovery amount, plus the matching `CashReceipt_Amount` or
     `AdjustmentReceipt_Amount` split
   - `Related_Account_Data` holding the serialised `DtoEcsLoanRecovery` (section 10)
   - `Staging_Status = "I"`, `Transaction_Type = "Member Transaction"`

   It then raises `OnECSLoanRecoveryInitiate`. The host reloads the `I` lines and hides the component, the same
   way as `JLRecoveryInitiate`.
5. The Maker can now **add any other account** to the same voucher: SB deposit, share, another receipt, or an
   adjustment payment. This works exactly as today, because each account is its own staging line.
   **Make Transaction** (`HandleValidSubmit` → `api/Staging/MakeStaging`) moves every line to `M`. The existing
   checks that adjustment receipts equal payments and that cash is not negative still apply.
6. **Checker** opens `Checkerboard.razor` and views the line through `ECSLoanRecoveryView` (new `case 58` in
   its view mapping). They then approve or reject.
7. **Approve** → `StagingController.ApproveStaging` → `TransactionsHandler.SaveTransaction` needs a new
   `case 58` (section 11). That case:
   - recomputes the figures
   - writes `loan_trn` and `fin_voucher_trn` inside the existing transaction
   - sets the staging rows to `V`

   **Reject** works as today and writes nothing to the loan.

Host page wiring needed for `case 58`:
- `GetComponentName`, the receipt switch, `GetViewComponentName` and the delete switch
- parameters `AccountId, CashOrAdjustment, MemberId, MemberNumber, MemberName, TransactionDate`
- callbacks `OnECSLoanRecoveryComponentClose (bool)` and `OnECSLoanRecoveryInitiate (DtoEcsLoanRecovery)`

## 4. Loans fetched on load

Fetch every loan of the member where:
- `Loan_Master.Loan_Type = 1`, `Loan_Delete = false`, `IsAccountClosed = false`
- `Voc_Status = 'V'` and `BrCode` = the session branch
- **and** at least one of these, all as on the application date (after the accrual in section 6):
  principal outstanding > 0, **or** interest balance > 0, **or** penal interest balance > 0

Every `loan_trn` aggregate counts only rows with `TrnTr_Delete = false`, `Voc_Status = 'V'`,
`Trn_Date <= application date` and the same `BrCode`. Put the `BrCode` predicate on **every alias** in
every join. `EcsLoanSanctionRepository.GetLoanBalancesAsync` is the model to follow, since it already does this.

### 4.1 Balances per loan (from `loan_trn`, no stored balance columns)

| Figure | Formula |
|---|---|
| Total disbursed | Σ `Disb_Amt` |
| Total principal demand | Σ `Prl_Dem` |
| Total principal collection | Σ `PrlColl_Amt` |
| **Principal outstanding** | Σ Disb_Amt − Σ PrlColl_Amt |
| **Principal overdue** | max(0, Σ Prl_Dem − Σ PrlColl_Amt). If it goes negative, that is advance principal collection, so treat it as 0. |
| **Interest overdue** | Σ `IntCalc_Amt` − Σ `IntColl_Amt` (interest booked so far and unpaid) |
| **Penal overdue** | Σ `PICalc_Amt` − Σ `PIColl_Amt` |
| Last interest date | max(`IntCalc_Date`). It is never null for an ECS loan, because disbursement writes `IntCalc_Date = disbursement date` on the `I` row. |
| Last penal date | max(`PICalc_Date`), else the disbursement date (min `Disb_Date`) |
| Disbursement date | min(`Disb_Date`) over `Trn_Status = 'I'` rows |

Do **not** use `San_Amt` for principal outstanding. A loan's disbursed amount is Σ `Disb_Amt`.

## 5. Principal demand (NOT raised here — reference only)

Recovery **never** writes `Prl_Dem`. Principal demand is raised only by the month-end day-end process in the
**ECS Demand Calculation** page, which is out of scope for this spec. Recovery only reads the demand already
raised. The rule it will follow, from `loan_schemes.inst_type`, is:

| inst_type | Meaning | Principal demand per period |
|---|---|---|
| 1 | Equal instalment | `loan_master.inst_amt` |
| 2 | Equated instalment (EMI) | `loan_master.inst_amt` − interest calculated for the period |
| 3 | Bullet payment | none |
| 4 | No periodic instalment (FD/RD-backed, maturity based) | none |

## 6. Interest and penal interest "now calculated"

### 6.1 Period

| | Interest | Penal |
|---|---|---|
| From date | last interest date (4.1). For the first calculation this is the disbursement date. | last penal date (4.1) |
| To date | application date − 1 (inclusive) | application date − 1 (inclusive) |
| Days | application date − from date | application date − from date |

Example: disbursed 01-Oct, application date 15-Nov gives 01-Oct to 14-Nov, which is **45 days**. After posting,
`IntCalc_Date` / `PICalc_Date` = application date, so the next recovery starts from there with no gap and no
overlap.

If the from date ≥ the application date, interest now = 0. This covers a second recovery on the same day.

### 6.2 Diminishing (segment-wise) product method

Split the from-to period into segments. A new segment starts on each date on which either of these happens:

- a `loan_trn` row changes the base. That is a `Disb_Amt`, `Prl_Dem`, `PrlColl_Amt`, `IntCalc_Amt` or
  `IntColl_Amt` row with `Trn_Date` inside the period. The new balance applies **from that date inclusive**.
- a rate changes. That is a `loan_roi` row (Agency `'S'`, same `BrCode`) whose `Roi_Wef` falls inside the
  period. Use the `Roi` / `Pi` in force on each segment's start date. If there is no `loan_roi` row, use
  `Loan_Master.Roi` / `Pi`.

```
interest = Σ over segments ( base_balance × rate × segment_days / 36500 )
```

Round once per loan per head to the nearest rupee, with `MidpointRounding.AwayFromZero`. Do not round per
segment.

### 6.3 Interest base, by `loan_schemes.int_application`

| int_application | Base on each segment |
|---|---|
| 1 — on non-overdue principal | **MIN**( Disbursed − Principal demand raised , Disbursed − Principal collected ), floored at 0 |
| 2 — on principal outstanding | Disbursed − Principal collected |

For Int_Application 1, the first term is the non-overdue principal. The second term takes over only when the
member has paid principal **in advance**. Overdue principal therefore attracts penal interest and not normal
interest. This is the user's ruling on rule 1.2.

### 6.4 Penal base, by `loan_schemes.pi_application`

Penal is charged only when an overdue exists. A segment where the base is ≤ 0 earns no penal.

| pi_application | Base on each segment | Rate |
|---|---|---|
| 1 (or 0 / unset) | No penal. Penal now = 0, and **do not** stamp `PICalc_Date`. Live schemes carry 0. | — |
| 2 | Principal overdue = max(0, Σ Prl_Dem − Σ PrlColl) | `Pi` |
| 3 | Principal overdue + interest overdue (Σ IntCalc − Σ IntColl, booked interest only; the interest now being calculated is excluded) | `Pi` |

Note that `EcsLoanSanctionHandler.cs:215` labels `PI_Application 1` as "On Principal Overdue". That label is
wrong. The meaning above matches the Loan Products screen (`LoanProductsCreate.razor:329`), which is the one to
follow.

### 6.5 Do not reuse these as-is (known bugs found while studying the code)

- `LoanTrnRepository.Calc_Int_OnVariable_ROI` / `Calc_PI_OnVariable_ROI` (:3142/:3197). These keep the starting
  rate only when `Roi > 0 && Pi > 0`, so a loan with Pi = 0 accrues zero interest.
- `CalculatePIOnVariableRate` (:955) is seeded with the ROI instead of the penal rate.
- `loan_roi` / `loan_disb` lookups in `LoanTrnRepository` have no `BrCode` filter, and the method swallows
  exceptions (:779).
- `Utilities.Calculate_Interest` rounds per call with banker's rounding, so do not call it per segment. Write a
  new, small, pure calculator in the handler, such as `EcsInterestCalculator`, that takes balances, rate events
  and dates and returns amount, from date, to date and days. Keep it free of DB access so it can be checked by
  hand.

## 7. Grid

One row per loan from section 4. All columns are read-only except **Select**.

| Select ☐ | Loan No | Scheme | Disb. date | ROI % / PI % | Prl outstanding | Prl overdue | Int overdue | Int now (from–to, days) | PI overdue | PI now (from–to, days) | **Recoverable** | **To close** |

Per loan:
- **Recoverable** = Prl overdue + Int overdue + Int now + PI overdue + PI now
- **To close** = Prl outstanding + Int overdue + Int now + PI overdue + PI now

Below the grid, recomputed whenever a box is ticked or cleared:
- **Total amount to be recovered** = Σ Recoverable of the selected loans
- **Total amount to close the selected loan(s)** = Σ To close of the selected loans

If the member has no qualifying loans, show "No ECS loans with dues for this member" and only a Close button.
Show the page only when the institution is ECS: reuse the `isReady` / ECS-only gate from `ECSLoanSanction.razor`.
The server rejects non-ECS calls (section 12).

## 8. Recovery amount and appropriation

There is one text box, **Recovery amount**, with 2 decimals. It is enabled when at least one loan is ticked.

**Validation**
- Must be > 0.
- Must be ≤ **Total amount to close the selected loans**. Anything beyond the penal plus interest plus the full
  principal outstanding of the selected loans is not allowed: "Amount exceeds the closure amount of the selected
  loans (₹x)".
- An amount between *Recoverable* and *To close* is allowed. The excess goes to principal in advance (step 4).

**Order** (across all selected loans; inside each step, oldest loan first, i.e. earliest disbursement date,
then lowest `Loan_Id`)

1. **Penal**: PI overdue + PI now, for every selected loan
2. **Interest**: Int overdue + Int now, for every selected loan
3. **Principal overdue**, for every selected loan
4. **Principal in advance** (remaining outstanding), for every selected loan

Each step fills a loan in full before moving on to the next loan. Show the result live in extra grid columns:
**Penal coll · Interest coll · Principal coll · Prl outstanding after**. Also flag the loans that the amount
would **close**: principal outstanding after = 0 and interest and penal fully paid.

**Initiate** stays disabled until the amount is valid. **Close** discards and raises
`OnECSLoanRecoveryComponentClose`.

## 9. Concurrency and stale figures

- A loan that already sits in another pending (`I` or `M`) staging line for acc 58 is shown greyed out and
  cannot be ticked. Show the message "Pending recovery awaiting approval".
- At approval the handler **recomputes** sections 4 to 8 for the staged loans and amount, using the staging
  date as the application date. If any accrual or appropriated figure differs from the staged JSON, it aborts
  with a clear message, and the Checker rejects so the Maker can re-initiate. Approval must happen on the same
  business day as the staging date.

## 10. DTOs

- `EcsLoanRecoveryLoanVM` (a grid row): `Loan_Id`, `Loan_No`, `Scheme_Id`, `Scheme_Name`, `Disb_Date`,
  `Roi`, `Pi`, the balances from 4.1, `IntNow`, `IntFrom`, `IntTo`, `IntDays`, `PINow`, `PIFrom`, `PITo`,
  `PIDays`, `Recoverable`, `ToClose`, `IsPending`.
- `DtoEcsLoanRecovery` (the `Related_Account_Data` JSON): `MemId`, `MemberNo`, `ApplicationDate`,
  `RecoveryAmount`, `CashOrAdjustment`, and `Lines[]`. Each line holds `Loan_Id`, `Loan_No`, `IntNow`,
  `IntCalcDate`, `PINow`, `PICalcDate`, `PIColl`, `IntColl`, `PrlColl`, `Soc_Roi`, `SocPI_Rate`,
  `ClosesLoan`.
  - The `*CalcDate` fields are the application date, or null for penal when `pi_application = 1`.

## 11. Posting on approval (`TransactionsHandler.SaveTransaction`, `case 58`)

The case runs inside the existing transaction and uses the staging's `Fin_Voucher`. For each line:

**`loan_trn`**: one row, `Trn_Status = 'R'`, `Trn_Date` = application date, `Demand_Id = zeroId`
(`brCode + "0000000"`, as in disbursement). Columns:
- `IntCalc_Amt = IntNow`, `IntCalc_Date` = application date (stamp the date even when IntNow = 0)
- `PICalc_Amt = PINow`, `PICalc_Date` = application date (except when `pi_application = 1`)
- `IntColl_Amt`, `PIColl_Amt`, `PrlColl_Amt` from the appropriation
- `Prl_Dem = 0`, `Soc_Roi`, `SocPI_Rate`, the next `Trn_SlNo`, `Voc_Id`, `Usr_Id`, `Yr_Id`, `BrCode`,
  `Voc_Status 'V'`

**`loan_master`**: set `LastIntApplication_Date` (and `LastPIApplication_Date`) to the application date. If
`ClosesLoan`, also set `IsAccountClosed = true` and `AccountClosedDate` to the application date. The closure
tolerance is 0.005, the same as `EcsLoanDisbursementHandler.Tolerance`.

**`fin_voucher_trn`**: one credit line per non-zero collection head, each carrying `Loan_Id`:
- principal → scheme `PrlLed_Id`, status `P`
- interest → `IntLed_Id`, status `I`
- penal → `PILed_Id`, status `PI`

The cash or adjustment debit side is already produced by the existing cash/adjustment logic in
`SaveTransaction`. Model the line building on `JsonbObject.GetVoucherTrnForJewelLoanRecovery`. Accrual
(`IntCalc_Amt`) is a loan-ledger memo only, with no GL entry, which matches the jewel loan recovery.

**IDs**: the rows go into `SaveTransaction`'s own `loanTrnList` / `finVoucherTrns`, like every other account
in the voucher. `LoanTrnRepository.AddLoanTrnListAsync` assigns `Trn_Id` = MAX+1 and `Trn_SlNo` = MAX per loan
+ 1 and saves after each row, so several loans in one approval do not collide; `FinVoucherTrnRepository`
does the same for `Voc_Trn_Id`. No separate allocator is used.

## 12. Server side (vertical slice per CLAUDE.md)

| Layer | New / changed |
|---|---|
| Repository | `EcsLoanRecoveryRepository` + `IEcsLoanRecoveryRepository`, wired in `IUnitOfWork` / `UnitOfWork` (3 places). Methods: `GetRecoverableLoansAsync(memId, brCode, date)`, `GetLoanTrnEventsAsync(loanIds, brCode, toDate)`, `GetRoiEventsAsync(loanIds, brCode, toDate)`, `GetPendingRecoveryLoanIdsAsync(memId, brCode)`. Query syntax, `BrCode` on every alias. |
| Handler | `EcsLoanRecoveryHandler` + interface, registered `AddScoped` in `Infin8.Coapp.UI/Program.cs`. Methods: `GetRecoveryLoansAsync` / `GetRecoveryPreviewAsync` (sections 4, 6 and 7) and `BuildPostingAsync` (sections 9 and 11), used by `TransactionsHandler`. The accrual is the pure `EcsInterestCalculator` (BusinessLogic); the appropriation is the pure `EcsRecoveryAppropriator` (Utility). |
| Controller | `GET api/loansanction/RecoveryLoans/{memId}` on `LoanSanctionController` (reuses `IsEcsAsync`; `BrCode` and the date from claims; refuses when the token's business day is no longer the open one). Appropriation has no endpoint: see section 16. |
| Approval | `TransactionsHandler.SaveTransaction` `case 58` → calls `EcsLoanRecoveryHandler` to recompute, check (section 9) and build the rows. |
| UI | `Components/ECSLoan/ECSLoanRecovery.razor`, `ECSLoanRecovery_View.razor` (`@rendermode InteractiveWebAssembly`, `@inject HttpClient`); `case 58` in `Member_Transaction.razor` (4 switches) and `Checkerboard.razor`. |
| DB | The section 2.1 row only (already present in dev). No schema change. |

## 13. Decisions taken

1. Int_Application 1 uses the **smaller** of (Disb − Demand) and (Disb − Collection).
2. Partial buckets are filled **oldest loan first** (disbursement date, then `Loan_Id`).
3. "Diminishing days" is a **segment-wise product** that splits at balance and rate changes, on a 365-day
   basis, rounded once per loan per head.
4. This spec includes the acc_id 58 `account_transactions` row.
5. Principal is appropriated overdue first, then in advance, up to the closure amount.
6. Recovery never raises principal demand.

## 14. Open points for the reviewer

- Should **Int now** be charged if the member pays only part of it? The current spec books the full accrual
  (`IntCalc_Amt`) and collects what the amount covers; the rest becomes interest overdue.
- Penal from date on the **first** penal calculation is the disbursement date. The segment method yields 0
  until the first demand creates an overdue, so no penal is over-charged. Confirm.
- Is `IOD_Application` (interest on interest overdue) in scope? This spec ignores it.
- ECS Demand Calculation page: `ECSDemandCalculationHandler` does not save and has no page or controller today.
  Recovery figures for principal overdue depend on demand being raised there. That is a separate spec.

## 15. Verification (manual — the project has no tests)

Run each scenario on an ECS branch, Maker then Checker:

1. **Worked example.** Disbursed ₹1,00,000 on 01-Oct, ROI 12, PI 2, inst_type 1, int_application 1,
   pi_application 2. On 31-Oct a demand of ₹5,000 was raised. Application date 15-Nov, no collections.
   - Int now = 100000×12×30/36500 + 95000×12×15/36500 = 986.30 + 468.49 = **₹1,455**
   - PI now = 5000×2×15/36500 = **₹4**
   - Recoverable = 5000 + 1455 + 4 = **₹6,459**; To close = 100000 + 1455 + 4 = **₹1,01,459**
   - Recovery ₹3,000 goes as PI 4, Int 1,455, Prl 1,541. After approval: loan_trn `R` row with those values,
     IntCalc/PICalc dates = 15-Nov, principal overdue 3,459.
2. With two loans ticked and an amount below the total penal, only the older loan's penal is paid.
3. An amount above To close is rejected. An amount between Recoverable and To close goes to principal in
   advance, so principal overdue shows 0 rather than negative.
4. A full closure amount sets `IsAccountClosed` after approval, and the loan no longer appears.
5. A second recovery on the same day gives Int now = 0. A loan pending in another staging cannot be ticked.
6. Adding an SB deposit line alongside the recovery and making the transaction produces one voucher, with both
   lines posted at approval.
7. Rejecting the transaction leaves `loan_trn` unchanged.
8. Logging in to a PCARDB branch shows the ECS-only notice, and the endpoint returns 403.

## 16. Implementation notes (departures from the first draft)

1. **Appropriation runs on both sides from one class.** `EcsRecoveryAppropriator` lives in
   `Infin8.Coapp.Utility`, which the WASM client references, so the grid's Penal / Interest / Principal coll
   columns update live without a server round trip, and approval reruns exactly the same code. The draft's
   `POST RecoveryAppropriate` endpoint was therefore not built.
2. **IDs** come from the existing `SaveTransaction` insert path, not an in-memory allocator (section 11).
3. **Closure tolerance** is 0.005, not ₹1: no ₹1 rule exists in the disbursement code the draft cited.
4. **`EcsLoanSanctionRepository.GetLoanBalancesAsync` was not reused.** It derives principal outstanding from
   `San_Amt` and has no `Voc_Status` / date filter. The recovery repository reads raw `loan_trn` rows (BrCode on
   every alias, `TrnTr_Delete = false`, `Voc_Status 'V'`, `Trn_Date <= application date`) and aggregates in the handler.
5. **Approval errors reach the Checker.** `SaveTransaction` used to swallow every exception. An
   `EcsRecoveryException` (stale figures, wrong business day, closed or pending loan, non-ECS branch) now sets
   `DtoVoucher.Message`; `ApproveStaging` returns it as the 400 body and `Checkerboard.razor` shows it. Other
   failures keep the old generic text.
6. **Per-loan pending lock.** `staging_details` rows for acc 58 in status I/M (master and detail) are
   deserialised and their `Loan_Id`s are locked; the staging being approved is excluded.
7. **`pi_application = 0`** (present on live schemes) is treated as 1: no penal and no `PICalc_Date` stamp.
8. **Business day.** `RecoveryLoans` and approval both compare against the first `N` business day by
   `Calendar_Id` (the same rule login uses for `CurrentDate`), so a stale token or a next-day approval is refused.
