# 14 — ECS Recurring Deposit Receipt (acc_id 2, "RD Receipt")

> Status: **Implemented** (see §17 for where the code departs from this draft). Ported from TSIS `uc_RDReceiptTrn` / `frmViewRDReceipt`.
> Section 13 lists the decisions proposed here, section 14 the points the reviewer must settle, and section 12
> the TSIS behaviours deliberately not carried over. Text in *italics* describes what TSIS does.

## 1. Purpose

Let a Maker take money over the counter (cash or adjustment) against one or more **recurring deposits** of a
member, from the **Member Transaction** screen. The screen works out each RD's pending instalments and penal
interest up to the business day. It splits the amount received across the selected RDs, penal first, then
instalments, and only in whole instalments. The Checker's approval posts it to `termdeposit_trn` and the voucher.

Scope: RDs only, meaning `termdeposit_master` rows whose scheme has `termdeposit_schemes.tdschemetype = 'R'`.
This screen is not the monthly RD demand (spec 12 §4.4) or its recovery through the pay office (spec 13). It is
the counter receipt of a member who pays in person, pays arrears, or pays in advance.

*TSIS: `TSIS/uc_RDReceiptTrn.cs` (input), `TSIS/frmViewRDReceipt.cs` (view of a saved receipt), opened from
`frmTransactions` / `FrmTransactions2` when the receipt account is `Account_Transactions.Acc_Id = 2`.*

## 2. Where it lives

| Item | Value |
|---|---|
| Component | `Infin8.Coapp.UI.Client/Components/TermDeposit/RecurringDeposit_Receipt.razor` |
| View component (Checker / line view) | `Infin8.Coapp.UI.Client/Components/TermDeposit/RecurringDeposit_Receipt_View.razor` |
| Host page | `Pages/Member_Transaction.razor` (`/member-transactions`) |
| Account | `account_transactions.acc_id = 2`, "RD Receipt", receipt side |
| Application date | the business day, `userInfoDto.CurrentDate`, passed in by the host as `TransactionDate` |

The acc_id 2 slot is already reserved in the code, with every switch case empty:

| Place | Line (today) | Needed |
|---|---|---|
| `TransactionsHandler.SaveTransaction` | `case 2: /// RD Receipt` (≈206) | build posting (§10) |
| `Member_Transaction.razor` receipt switch | `case 2` (≈894) | also set `isComponentLoaded = true`, as `case 58` does |
| `Member_Transaction.razor` `GetComponentName` | `case 2` (≈1255) | namespace `Components.TermDeposit` |
| `Member_Transaction.razor` `GetViewComponentName` | no `case 2` | add one → `Components.TermDeposit` |
| `Member_Transaction.razor` delete switch | `case 2` (≈2357) | reversal (§11) |
| `Checkerboard.razor` `GetViewComponentName` | no `case 2` (≈547–600) | add one → `Components.TermDeposit` |

### 2.1 Account row (DDL to apply out of band)

Check the live row first. The repo has no seed for `account_transactions`, because the Access migration excludes
it, so the row exists only in each database.

```sql
-- inspect
SELECT acc_id, acc_name, acc_type, acc_status, acc_belongsto, acc_delete, component_name, view_component_name
FROM account_transactions WHERE acc_id = 2;

-- set the component names (the view name must match the .razor file exactly)
UPDATE account_transactions
SET component_name = 'RecurringDeposit_Receipt', view_component_name = 'RecurringDeposit_Receipt_View'
WHERE acc_id = 2;
```

`acc_type 'R'` and `acc_belongsto 'M'` make it appear in the account dropdown when **Receipt** is chosen. If the
row is missing, insert it with those values, `acc_status` = `'RR'` (the TSIS voucher status) and
`acc_delete = false`.

## 3. Maker–checker flow (same as every other Member Transaction account)

Status lives in `staging_master` / `staging_details.staging_status`: `I` initiated → `M` made → `V` verified.

1. The Maker picks the member, **Receipt**, account **RD Receipt**, and Cash or Adjustment. The component loads.
2. The Maker ticks the RDs, enters the total received and presses **Appropriate** (§7), then **Initiate**. One
   `staging_details` row is added, carrying `Related_Account_Id = 2` and the serialised `DtoEcsRdReceipt`
   (§9) in `Related_Account_Data`. The line amount is the total received.
3. The Checker opens the line. `RecurringDeposit_Receipt_View` receives only `stagingId` and `relatedAccountId` and fetches its
   own JSON through `api/Staging/GetStagingDetailsById`.
4. On approval, `SaveTransaction case 2` calls `EcsRdReceiptHandler.BuildPostingAsync`. That method **recomputes
   every figure** and refuses if anything changed (§8). It then adds rows to `SaveTransaction`'s own
   `termDepositTrnList` / `finVoucherTrns` (§10).

*TSIS has no staging. **Initiate** only adds a row to the entry-voucher grid, and **Save** builds the
`TermDeposit_Trn` rows (`UpdateRDReceiptObject`) and posts directly in one EF transaction
(`DatabaseTransactions.Save_TransactionsNew`, `case 2`).*

## 4. RDs fetched on load

`GET api/rdreceipt/Rds/{memId}`: BrCode and business day come from the claims. A filter marked † differs from TSIS.

**Which RDs are listed:**
- the member holds them through `termdeposit_members` (joint holders included, `TDMem_Delete = false`);
- `termdeposit_master` is not deleted (`TD_Delete = false`)†, not closed (`AccountClosed = false`), and `Voc_Status = 'V'`†;
- the scheme has `tdschemetype = 'R'`;
- no payout has been made, i.e. Σ `DepositPaidAmount` = 0 on valid rows. A partly paid RD is not listed at all†
  (*TSIS lists it in "Get RD Nos" and then silently drops it from the grid*).
- `BrCode` is applied on **every alias**: master, members, schemes, trn.

**Per RD, summed over valid `termdeposit_trn` rows** (`TD_Delete = false`, `Voc_Status = 'V'`, same `BrCode`):

| Figure | Source |
|---|---|
| D (instalment) | `termdeposit_master.DepositAmount` |
| Received | Σ `DepositReceiptAmount` |
| Instalments received (n) | Σ `NoOfInstalments` |
| Penal calculated | Σ `PenalCalculatedAmount` |
| Penal received | Σ `PenalReceivedAmount` |
| Last penal applied | max `PenalAppliedDate` |
| Last instalment date | max `LastInstalmentDate` |
| Last receipt date | max `Trn_Date` |

*TSIS reads these from `DatabaseTermDepositNew.GetRDDetails` (inline SQL with `HAVING Sum(DepositPaidAmount) = 0`)
and the list from `GetTDNos(memId, "R")`.*

On the page, all eligible RDs are listed with a tick box, ticked by default, in `TD_No` order. This replaces
TSIS's three steps: **Get RD Nos**, then a checked list box, then **RD Receivable**.

## 5. Instalments due and RD pending

Let *B* be the business day.

```
N (instalments due)  = MonthsBetween(ValueDate, B) + 1        while MaturityDate > B
                     = PeriodInMonths                          once MaturityDate <= B
N = min(N, PeriodInMonths)
RD pending           = max(0, N × D − Received)
Advance              = max(0, Received − N × D)               (shown, never collected again)
```

`MonthsBetween` is the pure calendar-month difference `(to.Year − from.Year) × 12 + to.Month − from.Month`,
which ignores the day of month. Reuse `EcsDemandCalculator.MonthsBetween`. The current month's instalment
therefore falls due on the 1st of the month.

*TSIS (`TermDepositService.GetRDDetails`, lines 804-818) does the same with `GetMonthsBetweenFinal(ValueDate,
LastDateOfMonth(B)) + 1`. It assigns a clamped `PendingRD` and then overwrites it with the unclamped value, so an
RD paid in advance shows a negative pending amount. This spec clamps it at 0.*

## 6. Penal interest

### 6.1 Rate

`termdeposit_roi_template.PenalRateForRD` for the RD's scheme. The row is the one with `TDRoi_Delete = false`,
`PeriodType = 'M'`, `PeriodBegin ≤ PeriodInMonths ≤ PeriodEnd` and the latest `wef ≤ ValueDate`. If no row
matches, the rate is 0 and no penal is charged.

*TSIS `GetPIFor_RD` picks the rate as on the RD's **value date**. Specs 12 and 13 pick by period band and latest
`wef` without a date bound, falling back to the latest row of the scheme. See open point §14.3.*

### 6.2 TSIS formula (reference)

```
k  = (N − 1) − n                   overdue instalments; the current month is grace
PI = Σ_{i=1..k} (i × D) × r / 100  = D × r / 100 × k(k+1)/2
```

Every overdue month charges a flat `r %` on the **accumulated** arrear. So the oldest unpaid instalment is
charged for k months, the next for k − 1 months, and so on. Days and receipt dates do not enter. TSIS does not
round.

*Source: `TermDepositService.cs` 832-835 and 883-894. `MaxPICalcDate`, `NoOfMonthForPICalc` and
`CompletedInstalmentAmt` are computed and never used; the guard that would have limited the months is commented
out.*

### 6.3 Penal pending shown and collected

*TSIS shows `PI pending = Σ PenalCalculated − Σ PenalReceived + PICalc`, where `PICalc` is the §6.2 figure
recomputed from scratch over **all** arrears. Penal already booked as `PenalCalculatedAmount` (by an earlier
receipt, or by the monthly demand) is therefore charged a second time for the same months.*

Proposed rule:

```
PI arrear  = max(0, Σ PenalCalculated − Σ PenalReceived)       already booked, not yet paid
PI current = fresh penal for months after the month of the last PenalAppliedDate only
PI pending = PI arrear + PI current
```

For PI current, compute the §6.2 sum one month at a time from the first unpaid instalment month to month *B* − 1,
and keep only the months later than the month of `max(PenalAppliedDate)`. With no earlier penal, this equals the
TSIS figure. Round PI current **once per RD**, to whole rupees, `MidpointRounding.AwayFromZero`.

Whether the monthly rate is `r` (TSIS) or `r / 12` (spec 12 §4.4, `EcsDemandCalculator.Rd`) is open point §14.2.
The two disagree by a factor of 12, and the receipt and the demand must agree.

### 6.4 Worked example (TSIS rate interpretation, r = 2)

RD value date 10-Jan-2026, period 24, D = 500, received 1,500 (n = 3), no penal yet, business day 05-Jun-2026.

| Step | Value |
|---|---|
| N | MonthsBetween(Jan, Jun) + 1 = 6 |
| RD pending | 6 × 500 − 1,500 = **1,500** |
| k | (6 − 1) − 3 = 2 |
| PI current | 500 × 2/100 × (2 × 3 / 2) = **30** |
| PI pending | 0 + 30 = **30** |
| Total pending | **1,530** |

## 7. Grid, receipt amount and appropriation

### 7.1 Grid

| Column | Notes |
|---|---|
| RD No | `TD_No` |
| Instalment | D |
| Value date / Period / ROI | from master |
| Received so far | Σ `DepositReceiptAmount` |
| RD pending | §5 |
| PI arrear / PI current / PI pending | §6.3 |
| Last instalment date | max `LastInstalmentDate` |
| **RD receipt** / **PI receipt** | filled by Appropriate |
| View | popup: RD No, value date, instalment, maturity date, maturity amount, ROI, period (*TSIS `pnlFDDetails`*) |
| Remove | drops the row (equivalent to unticking it) |

Totals beneath the grid: RD pending, PI pending and total pending, then RD received, PI received and total received.
*(TSIS labels: "RD Pending", "Penal Interest Pending", "Total Pending", "RD Received", "Penal Interest Received",
"Total Received".)*

### 7.2 Appropriation (pure, shared by client and server)

Proposed: `Infin8.Coapp.Utility/EcsRdReceiptAppropriator.cs`, DB-free, so that the WASM grid and the approval
recheck run the same code. This follows the spec 11 pattern (`EcsRecoveryAppropriator`).

Input: the total received *T* and the ticked lines in grid order (`TD_No`). For each line in turn:

```
PI receipt = min(rem, PI pending);                         rem −= PI receipt
if rem >= RD pending:  RD receipt = RD pending
else:                  RD receipt = floor(rem / D) × D     whole instalments only
rem −= RD receipt
```

Afterwards:

- **Unallocated** = *rem*. It is shown in red, and Initiate is disabled while it is non-zero. *TSIS rebuilds the
  total from the allocation, so the Maker's figure silently changes and the difference vanishes.*
- *T* may exceed the pending amount, to pay **in advance**, only on the last line and only up to the maturity
  ceiling: `RD receipt ≤ PeriodInMonths × D − Received`, again in whole instalments. Anything beyond that is
  unallocated. (TSIS caps at pending; see open point §14.4.)
- Applying penal before instalments, and whole instalments only, matches spec 13's `EcsDemandAppropriator`
  (lines 258-265).

The amount box is re-appropriated on blur and on **Appropriate**. A blank or 0 amount clears all receipts and
shows "Total RD receipt is missing" (*TSIS message*).

### 7.3 Initiate validation

- At least one line, and total received > 0.
- Unallocated = 0.
- Every RD receipt is a multiple of D.
- No ticked RD is locked (§8).
- Only one RD Receipt line per member per staging voucher. *TSIS's duplicate-account check is commented out, so two
  RD Receipt rows in one voucher post everything twice.*

## 8. Concurrency and stale figures

- **Lock.** An RD named in a pending (`I`/`M`) acc 2 staging line, on master or detail and in the same `BrCode`,
  is locked: it is shown greyed out with "pending approval" and cannot be ticked. Find these RDs by deserialising
  `Related_Account_Data` and skip malformed rows. The approving staging excludes itself. This is the same
  mechanism as `EcsLoanRecoveryRepository.GetPendingRecoveryDataAsync`.
- **Business day.** Load and approval both refuse unless the token's `CurrentDate` = the branch's open business
  day = the staging date.
- **Recheck on approval.** Recompute §4–§7 from the database and compare every staged figure (RD pending, PI
  arrear, PI current, RD receipt, PI receipt, total) with a 0.005 tolerance. On any difference, a closed or
  withdrawn RD, or a now-locked RD, throw `EcsRecoveryException` with a message naming the RD No. The Checker sees
  that message (spec 11 §16) and the Maker must delete and re-initiate.
- **ECS gate.** The server refuses the call when `Gen_Bank_Name.Bank_Type` ≠ 3 (`IsEcsAsync`).

## 9. DTOs

`Infin8.Coapp.Dto/EcsRdReceiptVM.cs`:

```csharp
public class DtoEcsRdReceipt
{
    public decimal MemId { get; set; }
    public string? MemberNo { get; set; }
    public string? MemberName { get; set; }
    public DateTime ApplicationDate { get; set; }   // business day
    public double ReceiptAmount { get; set; }       // total received = staging line amount
    public int CashOrAdjustment { get; set; }       // 1 cash, >1 adjustment (as voc_Trn_Type)
    public List<DtoEcsRdReceiptLine> Lines { get; set; } = new();
}

public class DtoEcsRdReceiptLine
{
    public decimal TD_Id { get; set; }
    public string? TD_No { get; set; }
    public decimal TDScheme_Id { get; set; }
    public double Instalment { get; set; }
    public DateTime ValueDate { get; set; }
    public int PeriodInMonths { get; set; }
    public double RateOfInterest { get; set; }
    public DateTime? MaturityDate { get; set; }
    public double MaturityAmount { get; set; }
    public double PenalRate { get; set; }
    public int InstalmentsDue { get; set; }          // N
    public double Received { get; set; }
    public int InstalmentsReceived { get; set; }     // n
    public DateTime? LastInstalmentDate { get; set; }
    public double RdPending { get; set; }
    public double PiArrear { get; set; }
    public double PiCurrent { get; set; }
    public double PiPending { get; set; }
    public double RdReceipt { get; set; }
    public double PiReceipt { get; set; }
    public bool Locked { get; set; }                 // display only
}
```

## 10. Posting on approval (`TransactionsHandler.SaveTransaction`, `case 2`)

`BuildPostingAsync` only adds rows to the lists that `SaveTransaction` owns, and never saves or commits. Ids come
from `TermDepositTrnRepository.AddTermDepositTrnListAsync` (MAX+1 `TDTrn_Id`, MAX+1 `Trn_SlNo` per `TD_Id`).

### 10.1 `termdeposit_trn` — one row per RD with RD receipt > 0 or PI receipt > 0

| Column | Value | TSIS |
|---|---|---|
| `Trn_Date` | business day | same |
| `TD_Id` | the RD | same |
| `DepositDemandAmount` | 0 | same |
| `DepositReceiptAmount` | RD receipt | same |
| `MaturityAmount`, `InterestCalculatedAmount`, `InterestPaidAmount`, `DepositPaidAmount` | 0 | same |
| `InterestAppliedDate`, `LastPaymentDate` | null | same |
| `PenalCalculatedAmount` | PI current (§6.3) | *fresh PICalc over all arrears* |
| `PenalAppliedDate` | business day when PI current > 0, else null | same (*the UI sets it; the service's `PICalcDate` is always null*) |
| `PenalReceivedAmount` | PI receipt | same |
| `NoOfInstalments` | RD receipt / D (integer) | same |
| `InstalmentAmount` | D when RD receipt > 0, else 0 (`int` column) | same |
| `LastInstalmentDate` | `AddMonths(prev, NoOfInstalments)` when RD receipt > 0, else null, where prev = max `LastInstalmentDate` ?? `ValueDate` | same |
| `TD_OE`, `TD_Delete` | false | same |
| `Voc_Id` | the voucher | same |
| `Usr_Id`, `Yr_Id`, `BrCode` | from claims | — |
| `Demand_Id`, `TDCalc_Id` | 0 | same |
| `Voc_Status` | `'V'` | — |

`AddMonths` keeps month-end: if the date is the last day of its month, return
`date.AddDays(1).AddMonths(n).AddDays(-1)`, else `date.AddMonths(n)`. This is *TSIS `ExtensionMethods.AddMonths`*.
Put it in `Utility` next to the appropriator.

Edge case: if a row with PI receipt only arrives when PI current is 0, all instalment columns are 0 and
`LastInstalmentDate` is null.

### 10.2 `fin_voucher_tr` — per RD

| Line | Ledger | `voc_rpt` | Written when |
|---|---|---|---|
| RD instalments | RD principal ledger (§13.1) | RD receipt | > 0 |
| RD penal | RD penal ledger (§13.1) | PI receipt | > 0 |

Common to both lines:
- `voc_pmt` = 0 and `voc_Trn_Type` = CashOrAdjustment.
- `Status` = `'RR'`.
- `voc_narr` = `"M.No <memberno> <name> RD No <TD_No>"`.
- `Description` = `"RD No <TD_No>"` and `Mem_Id` = member.

The cash (or adjustment) counterpart comes from `SaveTransaction`'s existing flow for the staging line, as for
every receipt account.

Final check: Σ `voc_rpt` of these lines equals `ReceiptAmount`. If not, throw `EcsRecoveryException`.

### 10.3 `termdeposit_master`

Not updated. *TSIS doesn't update it either: no status and no last-instalment date. `AccountClosed` is set only by
RD payment/closure.*

## 11. Delete / reversal

**Before approval.** Deleting the staging line is the normal staging delete and posts nothing. This releases the
lock in §8.

**After approval** (`Member_Transaction.razor` delete switch `case 2`, and the server reversal):

1. **Refuse** if any valid `termdeposit_trn` row for the same `TD_Id` has a `Trn_SlNo` greater than this voucher's
   max `Trn_SlNo`. That covers a later receipt, a later demand posting (spec 12) or a recovery (spec 13). The
   message names the RD No: "RD No X has later transactions; delete them first."
   *TSIS `Find_SubsequentTransactionInTermDeposit` does the same, but its message query is typed `"Sselect ..."`,
   so the user gets a SQL error instead of the RD No.*
2. Otherwise, soft-delete the voucher's `termdeposit_trn` rows (`TD_Delete = true`, by `Voc_Id` and `BrCode`). The
   voucher lines and voucher go through the existing voucher delete.

The view for a saved receipt lists, per `Voc_Id`: RD No, instalment, value date, period, ROI, RD received, PI
received and penal applied date, with totals. This is *TSIS `frmViewRDReceipt` / `GetRDReceiptForView`*.

## 12. TSIS behaviours not carried over (bugs found while studying the code)

| # | TSIS behaviour | This spec |
|---|---|---|
| 1 | PI pending adds a fresh PI over **all** arrears on top of the PI already booked, so it is double-counted | §6.3: fresh PI only for months after the last penal applied |
| 2 | The leftover amount after appropriation is silently dropped (the total is rebuilt) | §7.2: unallocated is shown and blocks Initiate |
| 3 | `PendingRD` is overwritten unclamped and goes negative on an advance | §5: clamped at 0 |
| 4 | `GetTDNos` lists a partly paid RD that `GetRDDetails` then drops | §4: not listed |
| 5 | `GetRDDetails` does not check `Master.TD_Delete` | §4: checked |
| 6 | The service's `PICalcDate` is always null (assigned and then reset) | §10.1: the UI behaviour is kept explicitly |
| 7 | Two RD Receipt rows in one voucher post twice | §7.3: refused |
| 8 | The delete check's message query typo (`Sselect`) | §11: proper message |
| 9 | `ex.InnerException.ToString()` in the save catch throws when there is no inner exception | not applicable (`EcsRecoveryException` path) |
| 10 | Ledgers read from the single-row `Map_Fin`, which returns 0 if the setting is missing | §13.1: scheme ledgers, refused when 0 |
| 11 | Demand recovery writes `LastInstalmentDate = null`, so instalments recovered through the pay office never advance it | noted only: spec 13 has the same gap (§14.6) |

## 13. Decisions proposed

1. **Ledgers.** Use the RD's scheme: `termdeposit_schemes.Led_Id` for instalments and `PiLed_Id` for penal.
   Refuse with `EcsRecoveryException` if either is 0.
   - *TSIS reads `Map_Fin.RD_Led_Id` / `RD_PI_Led_Id`.* The current app's `Map_General` has `RD_Led_Id` /
     `RD_PI_Led_Id`, but nothing reads them; specs 12 and 13 use the scheme columns.
   - Spec 13 credits RD penal to `Led_Id` as well (`EcsRecoveryHandler.cs` ≈655). That is inconsistent with this
     choice; see §14.1.
2. **Maker–checker** through staging, with an approval recheck (spec 11 pattern), not TSIS's direct posting.
3. **Whole instalments only** for the RD receipt. Penal is taken before instalments, oldest RD (`TD_No`) first.
4. **Rounding** penal once per RD, to whole rupees, `AwayFromZero`.
5. **No schema change.** Every column used already exists on `termdeposit_trn`, `termdeposit_master`,
   `termdeposit_schemes` and `termdeposit_roi_template`.
6. **ECS only**, gated on the server (`IsEcsAsync`) and on the client (the `isReady` pattern of `ECSLoanSanction.razor`).

## 14. Open points for the reviewer

1. **Penal ledger in spec 13.** Should spec 13's recovery also split penal to `PiLed_Id`, so that the counter
   receipt and the pay-office recovery hit the same ledgers?
2. **Monthly penal rate.** Is `PenalRateForRD` a **monthly** rate (TSIS: `D × r/100` per overdue month) or an
   annual one (spec 12: `/ 12`)? Check the live `termdeposit_roi_template` values: a figure like 1–2 suggests
   monthly, 12–15 annual. Spec 12 must then match.
3. **Rate date.** Should the penal rate be taken as on the value date (TSIS), the receipt date, or the latest
   `wef` (specs 12/13)?
4. **Advance and post-maturity receipts.** May a member pay instalments in advance beyond the current month? This
   draft says yes, up to maturity. May a matured RD that is not yet closed still take a receipt? TSIS allows both.
5. **Grace.** Is the current month's instalment free of penal for the whole month, as in TSIS and spec 12, or only
   up to a cut-off day?
6. **`LastInstalmentDate`.** Spec 13's recovery leaves it null, so `max(LastInstalmentDate)` lags after
   pay-office recoveries. Should spec 13 set it too? RD payout/closure (TSIS `GetRDDetailsPayable`) uses it to
   derive a revised maturity date.
7. **Test data.** Branch `11005` has no RD accounts (spec 12, 13a Q6). A test RD must be created before this can
   be verified.

## 15. Server side (vertical slice per CLAUDE.md)

| Layer | Item |
|---|---|
| Utility | `EcsRdReceiptAppropriator.cs` (pure: §5, §6, §7.2, `AddMonths`) |
| Dto | `EcsRdReceiptVM.cs` (§9) |
| Repository | `EcsRdReceiptRepository` + interface: eligible RDs and sums (§4), penal rate (§6.1), scheme ledgers, pending acc 2 staging JSON (§8), later-row check (§11). Query syntax, `BrCode` on every alias. |
| UnitOfWork | interface property, backing field, lazy property |
| BusinessLogic | `EcsRdReceiptHandler` + interface: `GetRdsAsync(memId, brCode, date)`, `BuildPostingAsync(dto, stagingId, …, termDepositTrnList, finVoucherTrns)`, `CanDeleteAsync(vocId)` |
| Program.cs | `AddScoped<IEcsRdReceiptHandler, EcsRdReceiptHandler>()` |
| TransactionsHandler | inject `IEcsRdReceiptHandler`, fill `case 2` |
| Controller | `api/rdreceipt/Rds/{memId}` (BrCode and date from claims, ECS-gated) |
| Client | `Components/TermDeposit/RecurringDeposit_Receipt.razor`, `RecurringDeposit_Receipt_View.razor`; switch cases in §2 |

## 16. Verification (manual — the project has no tests)

Create test RDs in an ECS branch first (§14.7). For each case, initiate, approve, then check
`termdeposit_trn` / `fin_voucher_tr` against a hand calculation.

| # | Case | Expect |
|---|---|---|
| 1 | RD fully up to date, pay this month's instalment | RD receipt = D, PI 0, `NoOfInstalments` 1, `LastInstalmentDate` + 1 month |
| 2 | 2 instalments overdue (§6.4 example) | RD pending 1,500, PI 30 (r = 2, TSIS rate), receipt 1,530 → two lines on the voucher |
| 3 | Partial amount 1,200 on case 2 | PI 30, RD 1,000 (2 instalments); 170 unallocated → Initiate blocked until the amount is 1,030 |
| 4 | Two RDs, amount covers the first fully and part of the second | oldest `TD_No` first, whole instalments on the second |
| 5 | Advance of 3 instalments beyond the current month | allowed up to maturity; `LastInstalmentDate` advances 3 months |
| 6 | Second initiate on the same RD while the first is pending | RD greyed out "pending approval" |
| 7 | Post a demand (spec 12) between initiate and approve | approval refused with a message naming the RD |
| 8 | Day End between initiate and approve | refused: business day |
| 9 | Delete the approved receipt, then delete it again after a later receipt | first succeeds; second refused naming the RD |
| 10 | PCARDB branch | API refuses; the page shows "ECS only" |

## 17. As built — decisions and departures

Reviewer answers to §14 that the code follows:

| Point | Decision |
|---|---|
| Names | `RecurringDeposit_Receipt` / `RecurringDeposit_Receipt_View` in `Components/TermDeposit/` (not `RDReceipt`). `account_transactions` acc 2 must carry these names (§2.1). |
| §14.2 | `PenalRateForRD` is a **monthly** rate (TSIS). Spec 12's `EcsDemandCalculator.Rd` still divides by 12, so the monthly demand and the counter receipt disagree until spec 12 is changed. |
| §14.3 | Rate as on the **value date**: `PeriodType = 'M'`, period band, latest `Wef ≤ ValueDate`; none → 0. |
| §14.4 | Advance allowed on the last ticked RD up to maturity; a matured RD that is not closed is still listed. |
| §14.1, §14.5, §14.6 | Unchanged: spec 13 still credits RD penal to `Led_Id`; the current month is grace for the whole month; spec 13 still leaves `LastInstalmentDate` null. |

Departures from the draft:

1. **No post-approval delete (§11).** The app has no delete for any approved Member Transaction voucher, so there is no
   place to hook one. Before approval the normal staging delete applies and releases the lock. The later-rows check is
   built (`EcsRdReceiptRepository.GetRdWithLaterRowsAsync`, `IEcsRdReceiptHandler.GetRdBlockingDeleteAsync`) but unused.
2. **n (instalments received) is `floor(Received / D)`**, not Σ `NoOfInstalments`, so demand-posted or migrated rows
   without an instalment count still give the right arrear. `RD pending` uses `Received` as drafted.
3. **Penal months.** Month *t* (counted from the value date's month, *t* = N − 1 being the business day's month) is
   charged `(t − n) × D × r / 100` for *t* = n + 1 … N − 1; months up to the month of `max(PenalAppliedDate)` are skipped.
   This equals the §6.2 total with no earlier penal.
4. **RD receipt that is not whole instalments** is accepted only when it equals RD pending (an RD whose `Received` is not
   a multiple of D can then still be brought up to date).
5. **Lock is branch-wide by `TD_Id`**, not per member, so a joint holder's pending receipt also locks the RD.
6. **One RD Receipt line per voucher** is enforced at approval (`TransactionsHandler` `case 2`), and the staged amount
   must equal the staging line's `Receipt_Amount`.
7. **Voucher `Status`** is the account's `acc_status` (expected `'RR'`), falling back to `'RR'` when the row has none.
8. **Ids** come from the shared `AddTermDepositTrnListAsync` (global MAX+1 `TDTrn_Id`, MAX+1 `Trn_SlNo` per `TD_Id`).
9. **API.** `GET api/rdreceipt/Rds/{memId}` (`RdReceiptController`), ECS-gated, BrCode and business day from claims.

