# 15 — ECS Recurring Deposit New Account (acc_id 17, "Recurring Deposit New Account")

> Status: **Draft for review**. Ported from TSIS `uc_TDCreateTrn` (RD mode) / `FrmViewTDNew` /
> `DatabaseTransactions.Save_TermDepositAccount`. Section 14 lists the decisions proposed here, section 15 the points the
> reviewer must settle, and section 13 the TSIS behaviours deliberately not carried over. Text in *italics* describes
> what TSIS does. Sibling spec: `14-rs-receipt-component.md` (acc 2, RD Receipt), whose patterns this spec reuses.

## 1. Purpose

Let a Maker open a new **recurring deposit** for a member from the **Member Transaction** screen and take the **first
instalment** in the same voucher (cash or adjustment). The screen picks the RD scheme, looks up the rate of interest
and the penal rate, and works out the maturity date and maturity amount. The Checker's approval creates the
`termdeposit_master`, `termdeposit_members` and opening `termdeposit_trn` rows and credits the RD ledger.

Scope: RD schemes only (`termdeposit_schemes.tdschemetype = 'R'`), ECS only. Out of scope: FD opening (acc 16,
`FixedDeposit_Create`), RD receipt (acc 2, spec 14), RD payout/closure, and the TSIS **opening-entry** mode
(`frmTDOpeningEntry`, `TD_OE = true`, no voucher), which migrates old RDs and is not a counter transaction (§15.9).

*TSIS: `TSIS/uc_TDCreateTrn.cs` is one control shared by FD (acc 16) and RD (acc 17), switched by
`ChangeBetweenFDAndRD()`. It is opened from `FrmTransactions2.cs` `case "17"` (1034-1060).*

## 2. Where it lives

| Item | Value |
|---|---|
| Component (Maker input) | `Infin8.Coapp.UI.Client/Components/TermDeposit/RecurringDeposit_Create.razor` |
| View component (Checker / line view) | `Infin8.Coapp.UI.Client/Components/TermDeposit/RecurringDeposit_Create_View.razor` |
| Host page | `Pages/Member_Transaction.razor` (`/member-transactions`) |
| Account | `account_transactions.acc_id = 17`, "Recurring Deposit New Account", receipt side |
| Application date | the business day, `userInfoDto.CurrentDate`, passed in by the host as `TransactionDate` |

The acc 17 slot already exists in the code, but every case is empty or missing:

| Place | Line (today) | Needed |
|---|---|---|
| `TransactionsHandler.SaveTransaction` | `case 17: /// RD New Accounttrns` `break;` (≈679) | build posting (§10) |
| `Member_Transaction.razor` receipt switch | `case 17` sets only `isLedgerSection = false` (≈903) | also `isComponentLoaded = true`, as `case 2` does |
| `Member_Transaction.razor` `GetComponentName` | `case 17: break;` (≈1308) | namespace `Components.TermDeposit`, parameters §3.1 |
| `Member_Transaction.razor` `GetViewComponentName` | no `case 17` (live method ≈2021-2089) | add it in the `Components.TermDeposit` group, next to 16 |
| `Member_Transaction.razor` delete switch | `case 17` empty (≈2390) | reset the component state, as `case 16` does |
| `Checkerboard.razor` `GetViewComponentName` | no `case 17` (only in a commented block ≈342) | add one → `Components.TermDeposit` |

`isComponent()` (≈2595-2623) already returns `true` for 17, so the component name is read from the database.

### 2.1 Account row (already updated by the user)

The local `ECS` database now has:

| acc_id | acc_name | acc_type | acc_status | acc_belongsto | component_name | view_component_name |
|---|---|---|---|---|---|---|
| 17 | Recurring Deposit New Account | R | RDR | M | RecurringDeposit_Create | RecurringDeposit_Create_View |

Run the same update on every other ECS database (the repo has no seed for `account_transactions`):

```sql
SELECT acc_id, acc_name, acc_type, acc_status, acc_belongsto, acc_delete, component_name, view_component_name
FROM account_transactions WHERE acc_id = 17;

UPDATE account_transactions
SET component_name = 'RecurringDeposit_Create', view_component_name = 'RecurringDeposit_Create_View'
WHERE acc_id = 17;
```

`acc_status = 'RDR'` is also the TSIS voucher status for this line (*`uc_TDCreateTrn.cs` 1709: `status = "RDR"`*).

## 3. Maker–checker flow (same as every other Member Transaction account)

Status lives in `staging_master` / `staging_details.staging_status`: `I` initiated → `M` made → `V` verified.

1. The Maker picks the member, **Receipt**, account **Recurring Deposit New Account**, and Cash or Adjustment. The
   component loads with the member pre-filled as first holder.
2. The Maker chooses the scheme, period and instalment, presses **Calculate** (§6), fills nominees, and presses
   **Initiate**. One `staging_details` row is added, carrying `Related_Account_Id = 17` and the serialised
   `DtoEcsRdCreate` (§9) in `Related_Account_Data`. The line amount (`Receipt_Amount`) is **one instalment**, split into
   `CashReceipt_Amount` / `AdjustmentReceipt_Amount` by `CashOrAdjustment`.
3. The Checker opens the line. `RecurringDeposit_Create_View` receives only `stagingId` and `relatedAccountId` and
   fetches its own JSON through `api/Staging/GetStagingDetailsById`.
4. On approval, `SaveTransaction case 17` calls `EcsRdCreateHandler.BuildPostingAsync`. It **recomputes every figure**
   and allocates the RD number, refusing if anything changed (§8), then adds rows (§10).

*TSIS has no staging. **Initiate** only validates and adds a row to the voucher grid (`BtnInitiateFDCreate_Click`,
981-1101). On **Save**, `uc_TransactionEntries` `case 17` (866-873) calls `UpdateTDCreateObject`, which builds every
object, and `DatabaseTransactions.Save_TransactionsNew` `case 17` (412-418) → `Save_TermDepositAccount` (1493-1544)
writes them in one EF transaction.*

### 3.1 Host parameters

Same shape as RD Receipt (`Member_Transaction.razor` ≈1256-1279):

`AccountId`, `CashOrAdjustment`, `MemberId`, `MemberNumber`, `MemberName`, `TransactionDate`,
`OnRDCreateComponentClose` (`EventCallback<bool>`), `OnRDCreateInitiate` (`EventCallback<DtoEcsRdCreate>`).

After the POST to `api/Staging/AddStaging`, the component invokes `OnRDCreateInitiate`; the host's handler calls
`UpdateTransactionsFromStaging("I", brCode)` and resets `Account_Id`, exactly like `RDReceiptInitiate` (≈2740-2747).
Do not use the `TransactionState.NotifyIntiate` event path of `FixedDeposit_Create`.

## 4. Screen

Gate: the `isReady` pattern of `ECSLoanSanction.razor` / `RecurringDeposit_Receipt.razor` — spinner until the claim is
read, then "ECS only" for a non-ECS branch.

### 4.1 Account details

| Field | Source / rule | *TSIS* |
|---|---|---|
| RD scheme | `tdschemetype = 'R'`, `TDScheme_Delete = false`, same `BrCode`. Default: the only scheme if there is one. | *`GetTDSchemeIdList("R")`, no delete or branch filter; label "Slect Account"* |
| RD No | shown as **provisional** (§5), read-only | *typed or "Get RD No", editable* |
| Value date | defaults to the business day; rule in §7 | *defaults to the transaction date, free* |
| Instalment (per month) | > 0, whole rupees (§15.7) | *`txtRDAmount`, 2 decimals allowed* |
| Period (months) | dropdown of multiples of 12 within the scheme's `MinimumPeriod`–`MaximumPeriod` that have an ROI row (§6.1) | *12, 24 … 120, editable combo, scheme limits ignored* |
| Rate of interest | looked up (§6.1), read-only | *"Get ROI", editable* |
| Penal rate (per month) | looked up (§6.1), read-only | *"Get ROI" fills it too, editable* |
| Apply quarterly compound | **not offered** for ECS (§14.5, §15.4) | *checkbox, default off* |
| Maturity date | computed (§6.2), read-only | *"Get Matured Amount and Date", editable* |
| Maturity amount | computed (§6.3), read-only | *same, editable, never recomputed at save* |
| Include in demand | 1 "Include in monthly demand" (default) / 2 "Do not include" → master `Status` `'D'` / `'N'` | *"Include in Demnad", "Factory Demand"* |
| Mode of operation | dropdown, default Single (§4.2) | *12-item list* |

### 4.2 Holders

| Column | Notes |
|---|---|
| Mem No, Name, Father/Husband name, Age | first row = the transacting member, pre-filled and not removable |
| Add | Mem No box → `api/Member/GetMemberDetails/{memNo}/{brCode}` (as `FixedDeposit_Create`); duplicates refused |
| Remove | joint holders only |

- Age is computed **as on the business day** (*TSIS uses `DateTime.Now`*).
- **Name(s) on bond** (`TDH_Name`) is built as `"<MemNo> <Name>"` for each holder joined with `", "`, and rebuilt
  after every add or remove. *TSIS rebuilds it with `=` instead of `+=` on remove, leaving only the last holder (707).*
- **Mode of operation** — the TSIS list, stored as its code in `ModeOfOperation`:
  1 Single, 2 Either or Survivor, 3 Any one or Survivor/s, 4 Any two or Survivor/s, 5 All parents jointly,
  6 Any two jointly, 7 Former or Survivor, 8 Latter or Survivor, 9 By Guardian, 10 By Mandate, 11 Power of attorney,
  12 Proprietor. Rule: Single needs exactly one holder; 2–8 need two or more. Put the list in `Utility` so the view
  uses the same labels. `FixedDeposit_Create` uses different codes (1 Single / 2 Joint / 3 Either or Survivor) — see §15.2.

### 4.3 Nominees

Nominee 1 and 2: name, age, relationship (Husband, Wife, Son, Daughter, Father, Mother; editable). **Copy last
nominee** fills them from the member's latest non-deleted TD (`api/TermDeposit/GetNominee?memId=&tdSchemeType=R&brCode=`).
`IsNomineeProvided` = Nominee 1 name is not blank. Nominee age, when given, is 0–120.

*TSIS "Get Nominee Details" picks the member's highest `TD_Id` of any type **including deleted ones** and throws on a
null nominee age (`GetFDRenewalObjectByMemId`, 957-985). The current `GetNomineeForTermDeposit` fallback has no
`BrCode` filter; the new repository applies it.*

### 4.4 Totals and buttons

Below the form: "Instalment received now" = one instalment, "Cash / Adjustment" as chosen.
Buttons: **Calculate** (re-runs §6 — also triggered when scheme, period, instalment or value date changes),
**Initiate**, **Close**.

## 5. RD number

### 5.1 TSIS

*Numbers are 8 digits starting with 4 (`VerifyNomenclature` "RD", GeneralService 2112-2118). The next number is
`MAX(TD_No) + 1` per scheme (string MAX, deleted rows included), or `40000001` for the first
(`DatabaseGeneral.Get_MaxTDNo`, 285-358). "Get RD No" passes a hard-coded scheme 40001 (897). Uniqueness is checked
among non-deleted rows on Initiate and again at save, outside the save transaction.*

### 5.2 Current app

`NewAccountNoRepository.NewTDNoAsync` (173-203), used by `FixedDeposit_Create` through
`api/NewAccountNo/NewTDNo/{schemeId}/{brCode}`, is not fit for RD:

- The ECS RD scheme id is **40001**, not the `brCode`+3-digit pattern the method assumes, so for the first RD it takes
  the `< brCode+"400"` branch and returns the **FD** prefix: `11005030000001`.
- Migrated numbers are 6-character strings with a leading zero (FD scheme 30001: `030001`–`030011`).
  `long.TryParse("030011") + 1` gives `30012` and drops the zero.
- The scheme filter is `==`, the MAX is a string MAX, and deleted rows are included.

### 5.3 Proposed

- New method in `EcsRdCreateRepository`: next number = numeric MAX over **non-null, numeric** `TD_No` of the scheme in
  the `BrCode` (deleted rows included, so a number is never reused) + 1, **left-padded to the width of the existing
  numbers**. With no RD yet: `"04" + "0001"` = `040001`, matching the migrated FD format `030001` (§15.1).
- The Maker sees it as provisional. The number is **allocated at approval** inside `SaveTransaction`'s transaction and
  stored in the master; the staged number is only a display hint. Two pending RD openings therefore never collide.
- Uniqueness is checked at approval among all rows of the `BrCode` (`TD_Delete` ignored); a clash re-allocates.

## 6. Calculations

All in a pure, DB-free `Infin8.Coapp.Utility/EcsRdCreateCalculator.cs`, shared by the WASM component and the server
recheck (spec 11 / spec 14 pattern).

### 6.1 Rate of interest and penal rate

Rows of `termdeposit_roi_template` with `TDRoi_Delete = false`, same `BrCode`, `TDScheme_Id` = scheme,
`PeriodType = 'M'`, `PeriodBegin ≤ PeriodInMonths ≤ PeriodEnd`, `Wef ≤ ValueDate`; the row with the latest `Wef` wins.
ROI = `Roi`, penal rate = `PenalRateForRD` (a **monthly** rate, settled in spec 14 §17).

- No matching row → "Rate of interest is not defined for <scheme> for <n> months" and Initiate is blocked.
- ROI = 0 → blocked. Penal rate 0 is allowed (no penal will accrue).
- `TDHolderType` is ignored, as in TSIS (§15.6).

This is the same rule as `TermDepositROITemplateRepository.GetROIForTermDepositAsync` and
`EcsRdReceiptHandler.PenalRate`, so spec 14's receipt will see the same penal rate as the opening.

*TSIS `GetROIFor_TermDeposit` (DatabaseTermDepositNew 752-806) and `GetPIFor_RD` (808-853) use the same rule, without
`BrCode`, and return 0 silently on error.*

Live ECS data (local DB, branch 11005, scheme 40001 "Recurring Deposit", `MinimumPeriod 1`, `MaximumPeriod 12`):

| wef | months | ROI | Penal |
|---|---|---|---|
| 01-01-2013 | 1–12 | 8 | 3 |
| 01-01-2013 | 13–24 | 9 | 3 |
| 01-01-2013 | 25–36 | 10 | 3 |

The scheme allows at most 12 months but rates exist up to 36: see §15.5.

### 6.2 Maturity date

```
MaturityDate = AddMonthsKeepEnd(ValueDate, PeriodInMonths)
```

Reuse `EcsRdReceiptAppropriator.AddMonthsKeepEnd` (a month-end value date stays at month end).

*TSIS `GetTDMaturityDate` (ExtensionMethods 748-765) means to call its own month-end `AddMonths` (the call is
commented out) but calls `DateTime.AddMonths`, which keeps the day number and only clamps it to the target month's
length.* The two differ only when the value date is the last day of a month shorter than the target month:

| Value date + months | TSIS (`DateTime.AddMonths`) | Proposed (`AddMonthsKeepEnd`) |
|---|---|---|
| 14-Nov-2026 + 12 | 14-Nov-2027 | 14-Nov-2027 |
| 30-Apr-2026 + 1 | 30-May-2026 | 31-May-2026 |
| 28-Feb-2027 + 12 | 28-Feb-2028 | 29-Feb-2028 |
| 31-Jan-2026 + 1 | 28-Feb-2026 | 28-Feb-2026 |

See §15.3.

### 6.3 Maturity amount

TSIS `GetRDMaturityAmount` (ExtensionMethods 767-815) has three branches:

```csharp
// (a) default — every society except gSocietyId 5
for (int i = 1; i <= Prd; i++) {
    Amount += RDAmount;
    intcalc = Math.Truncate((Amount * (ROI / 100) / 12) + 0.5);   // each month rounded half-up to a rupee
    Interest += intcalc;
}
MaturityAmount = RDAmount * Prd + Interest;

// (b) gSocietyId == 5, no compounding
Interest = Math.Round(RDAmount * Prd * (Prd + 1) * ROI / 2400, 2);
Interest = (int)(Interest + 0.5);
MaturityAmount = RDAmount * Prd + Interest;

// (c) quarterly compound (overrides a/b)
double year = Prd / 12;    // integer division
MaturityAmount = Math.Round(RDAmount * (Math.Pow(1 + ROI/400, year * 4) - 1) / (1 - Math.Pow(1 + ROI/400, -4.0 / 12)), 0);
```

**Proposed: (a)**, simple interest on the running balance month by month, each month's interest rounded half-up to a
whole rupee. Compute in `decimal` so a month whose interest is exactly x.5 rounds the same on every machine
(`Math.Round(x, 0, MidpointRounding.AwayFromZero)` is equivalent to `Truncate(x + 0.5)` for positive x).
(b) is the closed form of the same simple interest rounded once; the two agree whenever no month's interest has a
fraction (see §15.4 for which applies to the ECS society).

`Utilities.GetRDMaturityAmount` (Utility/Utilities.cs 307-361) has only branch (b) and the buggy (c); do not reuse it.

### 6.4 Worked example

Scheme 40001, value date = business day 14-Nov-2026, period 12, instalment 500.

| Step | Value |
|---|---|
| ROI / penal | 8 / 3 (band 1–12) |
| Maturity date | 14-Nov-2027 |
| Monthly interest, month i | `round(500 × i × 8 / 1200)` = 3, 7, 10, 13, 17, 20, 23, 27, 30, 33, 37, 40 |
| Interest | 260 |
| Maturity amount | 12 × 500 + 260 = **6,260** |
| Received now | **500** (first instalment) |

(Branch (b) gives 500 × 12 × 13 × 8 / 2400 = 260 as well.)

## 7. Validation (Initiate, and again on approval)

| # | Rule | Message |
|---|---|---|
| 1 | Scheme chosen and is an RD scheme of this branch | "RD scheme is not selected" |
| 2 | At least one holder; first holder = the transacting member | "Holder details are missing" |
| 3 | Every holder is an active member of the branch (not expired / resigned) | "Member No X is not an active member" |
| 4 | Mode of operation agrees with the number of holders (§4.2) | "Mode of operation 'Single' allows only one holder" / "… needs two or more holders" |
| 5 | Instalment > 0 and a whole rupee amount | "Instalment amount is missing" / "Instalment must be in whole rupees" |
| 6 | Period within scheme Min–Max and has an ROI row | "Period must be between A and B months" / §6.1 message |
| 7 | ROI > 0 | §6.1 message |
| 8 | Value date ≤ business day and in the business day's calendar month (§15.8) | "Value date must be in the current month and not after the business day" |
| 9 | Nominee ages 0–120 when given; Nominee 2 only with Nominee 1 | "Nominee age is invalid" |
| 10 | Maturity date / amount equal the recomputed figures (tolerance 0.005) | "Maturity figures are out of date; press Calculate" |
| 11 | Only one RD New Account line per staging voucher (§8) | "Only one new RD account can be opened in a voucher" |
| 12 | ECS branch, business day open (§8) | as spec 14 |

*TSIS messages (`Validate_Input`, 1280-1427) cover only 1, 2, 5 (> 0), the 8-digit/leading-4 number format,
duplicates, and "is missing" for ROI / penal / maturity (blank only — 0 passes). Every message is shown twice
(1401, then 991/1595), and several carry typos ("Recudding", "shoule", "Amout", "interst", "RD No No.").*

## 8. Concurrency, business day and approval recheck

- **Business day.** Load, Initiate and approval refuse unless the token's `CurrentDate` = the branch's open business
  day = the staging date (spec 14 `GetOpenBusinessDayAsync`).
- **ECS gate.** `RdCreateController` refuses with 403 unless `Gen_Bank_Name.Bank_Type` = 3 (`IsEcsAsync`, as in
  `RdReceiptController`); `BuildPostingAsync` checks it again.
- **One line per voucher.** `TransactionsHandler` counts `case 17` lines like `rdReceiptLines` and refuses a second
  one. This also sidesteps the id collisions of §10.5. Two members' openings go in two vouchers.
- **Recheck on approval.** `BuildPostingAsync` re-reads the scheme, ROI row, holders and member status, recomputes §6
  and compares ROI, penal rate, maturity date, maturity amount and the instalment with the staged figures (tolerance
  0.005). Any difference, or a deleted scheme, throws `EcsRecoveryException` with a message naming the scheme and the
  figure; the Checker sees it (spec 11 §16) and the Maker deletes and re-initiates.
- **Staged amount.** `Receipt_Amount` of the staging line must equal the instalment.
- No lock is needed: nothing pending can refer to an RD that does not exist yet.

## 9. DTOs

`Infin8.Coapp.Dto/EcsRdCreateVM.cs`:

```csharp
public class DtoEcsRdCreate
{
    public decimal MemId { get; set; }                 // transacting member = first holder
    public string? MemberNo { get; set; }
    public string? MemberName { get; set; }
    public DateTime ApplicationDate { get; set; }      // business day = AccountOpenDate
    public int CashOrAdjustment { get; set; }          // 1 cash, >1 adjustment (as voc_Trn_Type)
    public int TDScheme_Id { get; set; }
    public string? TDScheme_Name { get; set; }
    public string? ProvisionalTD_No { get; set; }      // display only; allocated on approval
    public DateTime ValueDate { get; set; }
    public double Instalment { get; set; }             // DepositAmount = staging line amount
    public int PeriodInMonths { get; set; }
    public double RateOfInterest { get; set; }
    public double PenalRate { get; set; }
    public DateTime MaturityDate { get; set; }
    public double MaturityAmount { get; set; }
    public bool IncludeInDemand { get; set; } = true;  // Status 'D' / 'N'
    public int ModeOfOperation { get; set; } = 1;
    public string? NameOnBond { get; set; }
    public List<DtoEcsRdCreateHolder> Holders { get; set; } = new();
    public string? Nominee1Name { get; set; }
    public int Nominee1Age { get; set; }
    public string? Nominee1Relationship { get; set; }
    public string? Nominee2Name { get; set; }
    public int Nominee2Age { get; set; }
    public string? Nominee2Relationship { get; set; }
}

public class DtoEcsRdCreateHolder
{
    public decimal Mem_Id { get; set; }
    public string? MemberNo { get; set; }
    public string? MemberName { get; set; }
    public string? FatherName { get; set; }
    public int Age { get; set; }                       // as on the business day
    public int SlNo { get; set; }                      // 1 = first holder
}
```

The unused `RecurringDepositCreateDto` / `Json_TermDeposit_Master` are not reused.

## 10. Posting on approval (`TransactionsHandler.SaveTransaction`, `case 17`)

`BuildPostingAsync(dto, stagingId, transactedDate, brCode, yrId, checkedBy, vocId, cashOrAdjustment, status)` returns
`EcsRdCreatePosting { Master, Members, TermDepositTrns, VoucherTrns }`. The handler adds the master and members to the
change tracker through the new repository (ids §10.5); `SaveTransaction` adds the trn to its own `termDepositTrnList`
and the voucher line to `finVoucherTrns`, and its existing `Complete` / `CommitTransaction` saves everything.

### 10.1 `termdeposit_master` — one row

| Column | Value | *TSIS* |
|---|---|---|
| `TD_Id` | allocated (§10.5) | *MAX+1* |
| `TD_No` | allocated (§5.3) | *typed number* |
| `TDScheme_Id` | scheme | same |
| `Mem_Id` | first holder | same |
| `TDH_Name` | name on bond | same |
| `TDH_Age` | first holder's age | same |
| `ModeOfOperation` | 1–12 | same |
| `AccountOpenDate` | business day | *transaction date* |
| `ValueDate` | value date | same |
| `DepositAmount` | instalment | same |
| `PeriodInMonths` / `PeriodInDays` | period / 0 | same |
| `RateOfInterest` | recomputed ROI | *as typed* |
| `MaturityDate` / `MaturityAmount` | recomputed | *as typed* |
| `LeinAmount` | 0 | same |
| `IsNomineeProvided` | Nominee 1 name not blank | same (*case 16 here hard-codes `true`*) |
| `InterestPayableFrequency` | 0 (on maturity) | same |
| `PenalRate` | recomputed penal rate | *as typed* |
| `IsDiscountRate` | false | same |
| `IsCompoundInterest` / `CompoundFrequency` | false / 0 | *true / 3 when compounding ticked* |
| `Nominee1*`, `Nominee2*` | from the form | same |
| `LienMarked` / `Loan_Id` | false / 0 | same |
| `TD_OE` / `TD_Delete` / `AccountClosed` | false | same |
| `Voc_Id` | the voucher | same |
| `Usr_Id` / `Yr_Id` | checker / year from claims | *logged-in user / year* |
| `Status` | `'D'` include in demand, `'N'` otherwise | same |
| `RenewalTD_Id` / `RenewalTD_No` | 0 / `""` | same |
| `BrCode` | from claims | — |
| `Voc_Status` | `'V'` | — |

### 10.2 `termdeposit_members` — one row per holder

`TDMem_Id` allocated (§10.5), `TD_Id`, `Mem_Id`, `Mem_SlNo` = 1..n in grid order, `TDMem_Delete = false`, `Voc_Id`,
`Usr_Id`, `Yr_Id`, `BrCode`, `Voc_Status 'V'`. *TSIS numbers holders 1..n; case 16 here writes 1 for everyone.*

### 10.3 `termdeposit_trn` — the opening instalment

| Column | Value | *TSIS* |
|---|---|---|
| `Trn_Date` | business day | same |
| `TD_Id` | the new RD | same |
| `DepositDemandAmount` | instalment | same |
| `DepositReceiptAmount` | instalment | same |
| `MaturityAmount` | maturity amount | same |
| `InterestCalculatedAmount`, `InterestPaidAmount`, `DepositPaidAmount`, `PenalCalculatedAmount`, `PenalReceivedAmount` | 0 | same |
| `InterestAppliedDate`, `PenalAppliedDate`, `LastPaymentDate` | null | same |
| `NoOfInstalments` | 1 | same |
| `InstalmentAmount` | instalment (`int` column; whole rupees by §7.5) | *`(int)` truncation* |
| `LastInstalmentDate` | value date | same |
| `TD_OE` / `TD_Delete` | false | same |
| `Voc_Id`, `Usr_Id`, `Yr_Id`, `BrCode` | as master | — |
| `Trn_SlNo` | 1 (from `AddTermDepositTrnListAsync`) | same |
| `Demand_Id` / `TDCalc_Id` | 0 | same |
| `Voc_Status` | `'V'` | — |

Spec 14 then reads this RD as Received = D, n = 1, `LastInstalmentDate` = value date. With the value date in the
current month (§7.8), N = 1, so RD pending = 0 and no penal on the opening day.

### 10.4 `fin_voucher_tr` — one credit line

| Field | Value | *TSIS* |
|---|---|---|
| `led_id` | scheme `Led_Id` (40001: `110050000032`); refuse if 0 | *`Map_Fin.RD_Led_Id`* |
| `voc_rpt` / `voc_pmt` | instalment / 0 | same |
| `voc_Trn_Type` | CashOrAdjustment | same |
| `Status` | the account's `acc_status` (`'RDR'`), falling back to `'RDR'` | *"RDR"* |
| `voc_narr` | `"M.No <memberno> <name> RD No <TD_No>"` | *"… FD No …"* |
| `Description` | `"RD No <TD_No>"` | *"FD No …"* |
| `Mem_Id` | first holder | same |

The cash / adjustment counterpart comes from `SaveTransaction`'s existing flow for the staging line. Final check:
`voc_rpt` = `Receipt_Amount`.

`Map_General.RD_Led_Id` / `RD_PI_Led_Id` are 0 in the local database and nothing reads them; spec 14 also uses the
scheme ledgers.

### 10.5 Id allocation

Do **not** use `TermDepositMasterRepository.AddTermDepositMasterAsync` or
`TermDepositMemberRepository.AddTermDepositMemberListAsync`:

- they take MAX+1 from the database without saving, so two adds in one transaction reuse an id (spec 10 §13.2);
- `AddTermDepositMasterAsync` returns an empty `tdNo` (its TD_No lines are commented out) — why case 16's narration
  shows "FD No : " with no number;
- `GetModalObject.GetTermDepositMaster` ignores its `TDId` argument.

Instead `EcsRdCreateRepository` allocates `TD_Id` (global MAX+1), `TDMem_Id` (MAX+1, incremented in memory per holder)
and `TD_No` (§5.3) and adds the entities with ids set. `termdeposit_trn` goes through the shared
`AddTermDepositTrnListAsync`, which allocates `TDTrn_Id` / `Trn_SlNo` safely. Build the entities with object
initialisers rather than the positional `GetModalObject` factories.

## 11. View component (`RecurringDeposit_Create_View`)

Reads the staged `DtoEcsRdCreate` and shows: member, scheme, RD No (marked *provisional* until approved), value date,
instalment, period, ROI, penal rate, maturity date, maturity amount, include in demand, mode of operation (label),
name on bond, holders grid, nominees. No `@rendermode` of its own and no ECS gate, like `RecurringDeposit_Receipt_View`.

*TSIS `FrmViewTDNew` ("Recurring Deposit New Account", from the delete screen, `GetTDNewForView` 2465-2497) shows
members, ROI, maturity and nominees, but not the instalment, penal rate, compounding or demand flag, and has no
`TD_Delete` filter.*

## 12. Delete / reversal

**Before approval.** The normal staging delete; nothing was posted.

**After approval.** None — the app has no delete for an approved Member Transaction voucher (spec 14 §17.1). For the
record, *TSIS (`EmployeeService.Delete_SalaryPayment` case "RDR", 1612-1618) refuses when any later valid
`TermDeposit_Trn` row exists for the RD (its message query is typed `"Sselect"`, so the user gets an SQL error), then
soft-deletes master, trn and members by `Voc_Id`, and the voucher. It does not check loans or liens against the RD.*
When a voucher delete is built, the same rule plus a check of `LienMarked` / `Loan_Id` applies.

## 13. TSIS behaviours not carried over

| # | TSIS behaviour | This spec |
|---|---|---|
| 1 | Maturity amount/date, ROI and penal rate are editable and never recomputed at save | §6: computed, read-only, recomputed on approval |
| 2 | "Get RD No" uses hard-coded scheme 40001; numbers by string MAX | §5.3: allocated at approval per scheme |
| 3 | Duplicate-number check at save runs outside the transaction with concatenated SQL | §5.3: inside the approval transaction |
| 4 | Scheme list has no `TDScheme_Delete` / branch filter; scheme min/max period ignored | §4.1, §7.6 |
| 5 | Narration and description say "FD No" for an RD | §10.4: "RD No" |
| 6 | Name on bond keeps only the last holder after a remove | §4.2 |
| 7 | Maturity date uses `DateTime.AddMonths` by accident | §6.2: `AddMonthsKeepEnd` (§15.3) |
| 8 | Quarterly compound uses `Prd / 12` integer division | §14.5: not offered |
| 9 | `InstalmentAmount = (int)TDAmount` truncates | §7.5: whole rupees only |
| 10 | Opening-entry flag on the singleton leaks into normal mode (no voucher line) and shows the OE panel | opening entry out of scope |
| 11 | Nominee copy includes deleted TDs and crashes on null ages | §4.3 |
| 12 | Ledger from single-row `Map_Fin` (0 if not set) | §10.4: scheme `Led_Id`, refused when 0 |
| 13 | Age measured against today; `Yr_Id` is the logged-in year | business day; claims year |
| 14 | Every validation message shown twice; typos | §7 |
| 15 | No checks on value date, member status, mode of operation vs holders | §7.3, §7.4, §7.8 |

## 14. Decisions proposed

1. **Maker–checker** through staging with an approval recheck (spec 11/14 pattern), not TSIS's direct save.
2. **First instalment collected at opening**, as in TSIS: staging line amount = one instalment.
3. **RD number allocated at approval** (§5.3), format `04nnnn`.
4. **Figures computed, not typed**: ROI and penal from the template as on the value date, maturity by §6.3 (a).
5. **No quarterly compounding** for ECS RDs (no live RD uses it; the TSIS formula is buggy).
6. **Scheme ledger** `Led_Id`, refused when 0.
7. **One new RD per voucher.**
8. **No schema change.** Every column used exists.
9. **ECS only**, gated on the server and the client.

## 15. Open points for the reviewer

1. **RD number format.** `040001` (6 chars, like the migrated FD `030001`), or TSIS's 8-digit `40000001`, or the
   current app's `brCode + "040000001"`? Is numbering per scheme or per branch across RD schemes?
2. **Mode of operation codes.** TSIS list 1–12 (2 = Either or Survivor) vs `FixedDeposit_Create` 1–3 (2 = Joint).
   All live TDs are 1. Use the TSIS list for RD, and align FD later?
3. **Month-end maturity date.** `AddMonthsKeepEnd` (proposed) or TSIS's actual `DateTime.AddMonths`?
4. **Maturity formula.** Which TSIS branch did the ECS society use — default (a) or `gSocietyId == 5` (b)? They differ
   by at most a few rupees when monthly interest has fractions. Is ROI per annum (assumed)?
5. **Scheme period.** Scheme 40001 has `MaximumPeriod = 12`, but ROI rows go up to 36 months and TSIS offered 12–120.
   Correct the scheme row, or ignore Min/Max as TSIS did? Must the period be a multiple of 12?
6. **Holder type rates.** `TDHolderType` is ignored (TSIS too). Any senior-citizen or staff rate for RD?
7. **Instalment.** Minimum amount? Multiple of 10 or 100? (Draft: > 0, whole rupees.)
8. **Value date.** May it be backdated (TSIS: freely)? The draft allows only the current month, so the opening never
   creates arrears or penal. A future value date?
9. **Opening entry.** TSIS `frmTDOpeningEntry` (`TD_OE = true`, no voucher, sets instalments completed and penal pending)
   is excluded. Is a migration screen needed for old RDs?
10. **Include in demand.** TSIS stores `Status 'D'/'N'` and its demand selects on it, but spec 12's
    `EcsDemandRepository` (≈181) ignores master `Status`, so "Do not include" has no effect today. Should spec 12 filter
    on `Status = 'D'`?
11. **Joint holder demand.** The monthly demand goes to the first holder (`Mem_Id`) only — confirm.
12. **Test data.** Branch 11005 has no RD accounts yet; this screen will create the first one, so spec 14 can be
    verified after it.

## 16. Server side (vertical slice per CLAUDE.md)

| Layer | Item |
|---|---|
| Utility | `EcsRdCreateCalculator.cs` (pure: §6.1 picker over a list, §6.2, §6.3, §7 rules); mode-of-operation list |
| Dto | `EcsRdCreateVM.cs` (§9) |
| Repository | `EcsRdCreateRepository` + interface: RD schemes, ROI rows for a scheme, scheme ledger, next `TD_No` (§5.3), `TD_Id` / `TDMem_Id` allocation and add, member details and status, last nominee. Query syntax, `BrCode` on every alias. |
| UnitOfWork | interface property, backing field, lazy property |
| BusinessLogic | `EcsRdCreateHandler` + interface: `GetSetupAsync(memId, brCode, date)` (schemes, ROI rows, provisional number, nominee), `CalculateAsync(dto)`, `BuildPostingAsync(...)` |
| Program.cs | `AddScoped<IEcsRdCreateHandler, EcsRdCreateHandler>()` |
| TransactionsHandler | inject `IEcsRdCreateHandler`; fill `case 17`; one-line-per-voucher counter |
| Controller | `RdCreateController`: `GET api/rdcreate/Setup/{memId}`, `GET api/rdcreate/Member/{memNo}`, `POST api/rdcreate/Calculate` — BrCode and date from claims, ECS-gated |
| Client | `Components/TermDeposit/RecurringDeposit_Create.razor`, `RecurringDeposit_Create_View.razor`; switch cases in §2; `RDCreateInitiate` handler in the host |

## 17. Verification (manual — the project has no tests)

In ECS branch 11005, initiate and approve, then check `termdeposit_master`, `termdeposit_members`, `termdeposit_trn`,
`fin_voucher_tr` against a hand calculation.

| # | Case | Expect |
|---|---|---|
| 1 | Single holder, 500 × 12, value date = business day, cash | §6.4: ROI 8, penal 3, maturity 6,260 / +12 months; RD No `040001`; trn receipt 500, `NoOfInstalments` 1; voucher Cr `110050000032` 500 "RD No 040001" + cash Dr |
| 2 | Same, adjustment | no cash line; adjustment rules of the voucher apply |
| 3 | Joint holders, mode "Either or Survivor" | two `termdeposit_members` rows, `Mem_SlNo` 1, 2; name on bond lists both |
| 4 | Mode "Single" with two holders | Initiate refused |
| 5 | "Do not include in demand" | master `Status = 'N'` |
| 6 | Second RD opening, approved before the first | numbers allocated in approval order, no clash |
| 7 | Two acc 17 lines in one voucher | approval refused |
| 8 | Change the ROI row's rate between initiate and approve | approval refused naming the figure |
| 9 | Day End between initiate and approve | refused: business day |
| 10 | Value date last month | Initiate refused (§15.8) |
| 11 | PCARDB branch | API 403; page shows "ECS only" |
| 12 | Open RD Receipt (spec 14) for the member the same day | the new RD is listed, received 500, RD pending 0, PI 0 |
| 13 | Checkerboard and Member Transaction line view | `RecurringDeposit_Create_View` shows every §11 field |

## 18. Decisions settled (review, 14-Nov-2026)

| § | Point | Decision |
|---|---|---|
| 15.1 | RD number | `brCode` (5 digits) + one RD digit (the first digit of the scheme id, `4` for 40001) + a 7-digit running number: `1100540000001`. Next = numeric MAX of the branch's 13-digit numbers with that prefix (deleted rows included) + 1, allocated at approval. The migrated FD numbers are wrong and will be corrected separately. |
| 15.3 | Month-end maturity | `AddMonthsKeepEnd`, as proposed. |
| 15.4 | Maturity formula | Branch (a), each month's interest rounded half-up to a rupee. |
| 15.5 | Period | Multiples of 12 (up to 120) that have an ROI row with ROI > 0, so 12 / 24 / 36 today. The scheme's Min/Max is ignored, as in TSIS. |
| 7.3 | Active member | `memberstatus = 1`, `ismemexpired = false`, `memberdelete = false`, same branch. |

Kept as drafted: value date in the business day's month and not after it; instalment > 0 in whole rupees; TSIS mode-of-operation
list; `TDHolderType` ignored; "Include in demand" stored as `Status 'D'/'N'` only (§15.10 still open); one RD opening per voucher.

Implementation notes:
- **Calculate endpoint.** `POST api/rdcreate/Calculate` re-runs §6/§7 on the server, including member status, just before
  Initiate.
- **Mode of operation.** Adding a joint holder switches the mode from Single to "Either or Survivor". Removing back to one holder
  switches a 2–8 mode back to Single.
