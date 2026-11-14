# 16 — ECS Recurring Deposit Refund (acc_id 10, "Recurring Deposit Refund")

> Status: **Implemented** (§18 lists the settled decisions and where the code departs from this draft). Ported from TSIS `uc_RDPaymentTrn` ("RECURRING DEPOSIT PAYMENT") /
> `TermDepositService.GetRDDetailsPayable` / `DatabaseTransactions.Save_TransactionsNew` `case 10`. Section 14 lists the
> decisions proposed here, section 15 the points the reviewer must settle, and section 13 the TSIS behaviours
> deliberately not carried over. Text in *italics* describes what TSIS does. Sibling specs: `14-rs-receipt-component.md`
> (acc 2, RD Receipt) and `15-rd-new-account.md` (acc 17, RD New Account), whose patterns this spec reuses.

## 1. Purpose

Let a Maker **close** one or more of a member's recurring deposits from the **Member Transaction** screen and pay out
the deposit received plus the interest earned, by cash or adjustment. Before maturity the interest is worked out at a
**premature rate** (the rate for the period actually run, less a foreclosure rate); on or after maturity the
contracted maturity amount is paid. The Checker's approval writes a closing `termdeposit_trn` row per RD, marks the RD
closed, and debits the RD and RD-interest ledgers.

Scope: RD schemes only (`termdeposit_schemes.tdschemetype = 'R'`), ECS only, one member per line. Out of scope:

- FD refund (acc 8, `FixedDeposit_Payment`), RD receipt (acc 2, spec 14), RD opening (acc 17, spec 15).
- *TSIS `frmRDRefundByECS` / `uc_RDRefundByECSList` / `frmRDRefundByECSDelete`* — bulk RD refund through ECS
  (electronic clearing) from a separate menu. Not a Member Transaction account.
- Loan on RD recovery (acc 14 / 25 are not built for ECS) — see §7.

*TSIS: `TSIS/uc_RDPaymentTrn.cs` (712 lines), opened from `FrmTransactions2.cs` `case "10"` (1312-1340).
`frmMemberTransacation.cs` `case 10` (319) is empty — the older screen never supported it.*

## 2. Where it lives

| Item | Value |
|---|---|
| Component (Maker input) | `Infin8.Coapp.UI.Client/Components/TermDeposit/RecurringDeposit_Refund.razor` |
| View component (Checker / line view) | `Infin8.Coapp.UI.Client/Components/TermDeposit/RecurringDeposit_Refund_View.razor` |
| Host page | `Pages/Member_Transaction.razor` (`/member-transactions`) |
| Account | `account_transactions.acc_id = 10`, "Recurring Deposit Refund", **payment** side (`acc_type 'P'`) |
| Application date | the business day, `userInfoDto.CurrentDate`, passed in by the host as `TransactionDate` |

The acc 10 slot already exists in the code, but every case is empty or missing:

| Place | Line (today) | Needed |
|---|---|---|
| `TransactionsHandler.SaveTransaction` | `case 10: /// RD Refund` `break;` (536) | build posting (§11) |
| `Member_Transaction.razor` payment/receipt switch | `case 10` sets only `isLedgerSection = false` (962) | also `isComponentLoaded = true`, as `case 2` / `case 17` do |
| `Member_Transaction.razor` `GetComponentName` | `case 10: break;` (1525) | namespace `Components.TermDeposit`, parameters §3.1 |
| `Member_Transaction.razor` second switch | `case 10: break;` (1971) | as `case 17` |
| `Member_Transaction.razor` `GetViewComponentName` | `case 10: break;` (2284) | `Components.TermDeposit` group, next to 2 / 17 |
| `Member_Transaction.razor` delete switch | `case 10` empty (2457) | reset the component state, as `case 17` does |
| `Checkerboard.razor` `GetViewComponentName` | only in a commented block (≈385) | add a live `case 10` → `Components.TermDeposit` |

(Line numbers are from `main` at `309bf53`; check before editing.)

### 2.1 Account row — **typo in the local database**

The local `ECS` database has:

| acc_id | acc_name | acc_type | acc_status | acc_belongsto | component_name | view_component_name |
|---|---|---|---|---|---|---|
| 10 | Recurring Deposit Refund | P | RP | M | `RecurringDepsit_Refund` | `RecurringDepsit_Refund_View` |

"Dep**o**sit" is missing its `o`, so the host would look for a component that does not exist. Correct it here and run
the same on every other ECS database:

```sql
SELECT acc_id, acc_name, acc_type, acc_status, acc_belongsto, acc_delete, component_name, view_component_name
FROM account_transactions WHERE acc_id = 10;

UPDATE account_transactions
SET component_name = 'RecurringDeposit_Refund', view_component_name = 'RecurringDeposit_Refund_View'
WHERE acc_id = 10;
```

`acc_status = 'RP'` is also the TSIS voucher status for this line (*`uc_RDPaymentTrn.cs` 235, 377: `status = "RP"`*).

## 3. Maker–checker flow (same as every other Member Transaction account)

Status lives in `staging_master` / `staging_details.staging_status`: `I` initiated → `M` made → `V` verified.

1. The Maker picks the member, **Payment**, account **Recurring Deposit Refund**, and Cash or Adjustment.
2. The component lists the member's open RDs (§4.1). The Maker ticks the RDs to close; the grid fills with the payable
   figures (§5); the Maker presses **Initiate**. One `staging_details` row is added with `Related_Account_Id = 10` and
   the serialised `DtoEcsRdRefund` (§10) in `Related_Account_Data`. The line amount (`Payment_Amount`) is the **net
   payable** (§4.4), split into `CashPayment_Amount` / `AdjustmentPayment_Amount` by `CashOrAdjustment`.
3. The Checker opens the line. `RecurringDeposit_Refund_View` receives only `stagingId` and `relatedAccountId` and
   fetches its own JSON through `api/Staging/GetStagingDetailsById`.
4. On approval, `SaveTransaction case 10` calls `EcsRdRefundHandler.BuildPostingAsync`. It **recomputes every figure**,
   refusing if anything changed (§9), then adds rows (§11).

*TSIS has no staging. **Initiate** (`BtnInitiate_Click`, 232-268) only validates and adds a row to the voucher grid with
receipt = loan recovered, payment = deposit + interest. On **Save**, `UpdateRDPaymentObject` (369-694) builds the trn,
loan and voucher objects and `DatabaseTransactions.Save_TransactionsNew` `case 10` (334-362) writes them.*

### 3.1 Host parameters

Same shape as spec 15 §3.1:

`AccountId`, `CashOrAdjustment`, `MemberId`, `MemberNumber`, `MemberName`, `TransactionDate`,
`OnRDRefundComponentClose` (`EventCallback<bool>`), `OnRDRefundInitiate` (`EventCallback<DtoEcsRdRefund>`).

After the POST to `api/Staging/AddStaging`, the component invokes `OnRDRefundInitiate`; the host's handler calls
`UpdateTransactionsFromStaging("I", brCode)` and resets `Account_Id`, exactly like `RDReceiptInitiate` /
`RDCreateInitiate`.

## 4. Screen

Gate: the `isReady` pattern of `ECSLoanSanction.razor` / `RecurringDeposit_Receipt.razor` — spinner until the claim is
read, then "ECS only" for a non-ECS branch.

### 4.1 RDs fetched on load

`GET api/rdrefund/Rds/{memId}` returns the RDs where:

- the member is a holder: `termdeposit_members.Mem_Id = memId`, `TDMem_Delete = false`;
- `termdeposit_master`: `TD_Delete = false`, `AccountClosed = false`, `Voc_Status = 'V'`;
- scheme `TDSchemeType = 'R'`;
- no deposit has been paid out yet: Σ `DepositPaidAmount` over valid trn rows = 0;
- `BrCode` = claims branch on **every** alias (master, members, scheme, trn).

*TSIS loads them on a "Get RD Nos" button (`GetTDNos(memId, "R")`, DatabaseTermDepositNew 226-252) into a checked list;
the payable query (`GetRDDetails(int[])`, 1009-1046) adds `HAVING Sum(DepositPaidAmount) = 0` and an inner join to
`Termdeposit_Trn`, so an RD with no trn row silently disappears. No branch filter.*

No RDs → "The member has no open recurring deposits".

The list loads with the figures (§5) already computed, so the separate TSIS **RD Payable** button is not needed; ticking
an RD adds it to the totals.

### 4.2 RD grid

| Column | Source | *TSIS column* |
|---|---|---|
| Select | checkbox, default unticked | *checked list + Delete link* |
| RD No | `TD_No` | RD No |
| Instalment | `DepositAmount` | RD Amount |
| Value date | `ValueDate` | RD Date |
| Period | `PeriodInMonths` | Period |
| ROI | contract `RateOfInterest` | ROI |
| Months run (n) | §5.2 | — |
| Applied ROI | §5.3 | Appplied ROI |
| Received | Σ `DepositReceiptAmount` | RD Received |
| Interest payable | §5.4 | Interest Payable |
| Penal pending | §6 | — |
| Total payable | received + interest − penal pending | Total Payable |
| Int. calc date | §5.5 | Int Calc Date |
| Maturity date / amount | master | *in the View panel* |
| View | panel with the master fields (RD No, value date, instalment, maturity date / amount, ROI, period) | View |

### 4.3 Loan on RD panel

Shown only if a selected RD has `LienMarked = true` or an active `lien_trn` row, with the message of §7. Initiate is
blocked. *TSIS shows the loans and deducts them (§7).*

### 4.4 Totals and buttons

| Total | Value |
|---|---|
| Total RD payment | Σ received of the selected RDs |
| Total interest payment | Σ interest payable |
| Penal recovered | Σ penal pending (§6) |
| **Net payable** | RD + interest − penal = the staging line's `Payment_Amount` |

Buttons: **Initiate**, **Close**.

*TSIS labels: "Total RD Payment", "Total Interest Payment", "Total Payment", "Loan Principle Received", "Loan Interest
Receivedived", "Total Loan Received", "Net Payable" (designer 201-633).*

## 5. Interest payable

All in a pure, DB-free `Infin8.Coapp.Utility/EcsRdRefundCalculator.cs`, shared by the WASM component and the server
recheck (spec 11 / 14 / 15 pattern). The handler feeds it the ROI and foreclosure rows it read.

### 5.1 TSIS rule (reference)

*`TermDepositService.GetRDDetailsPayable` (913-978), per RD:*

```csharp
// last instalment date = max(Max(LastInstalmentDate), Max(Trn_Date)); Trn_Date if no instalment date
revisedMaturityDate = lastInstalmentDate.AddMonths(1) > TrnDate ? MaturityDate : lastInstalmentDate.AddMonths(1);
ForeClosureROI = GetTDForeClosureROI(4, ValueDate);                  // TermDeposit_fcTemplate, TD_Type 4 = RD
NoOfMonths = RDAmountReceived > 0
    ? (LastReceiptDate.Year - ValueDate.Year) * 12 + LastReceiptDate.Month - ValueDate.Month   // Max(Trn_Date)
    : 0;
PreMatureROI = GetROIFor_TermDeposit(ValueDate, TDScheme_Id, NoOfMonths, 0) - ForeClosureROI;

if (RDAmountReceived < DepositAmount * PeriodInMonths) {          // not fully paid
    IntCalcDate    = AddMonths_For_FD(TrnDate, NoOfMonths);         // TrnDate + n months (a future date)
    InterestPayable = NoOfMonths > 0 ? GetRDInterest(DepositAmount, NoOfMonths, PreMatureROI, IsCompound) : 0;
    AppliedROI     = PreMatureROI;
} else if (TrnDate < revisedMaturityDate) {                         // fully paid, before maturity
    InterestPayable = GetRDInterest(DepositAmount, NoOfMonths, PreMatureROI, IsCompound);
    AppliedROI = PreMatureROI;  IntCalcDate = TrnDate;
} else {                                                            // matured
    InterestPayable = MaturityAmount - RDAmountReceived;
    AppliedROI = RateOfInterest; IntCalcDate = MaturityDate;
}

// GetRDInterest (980-1003), no compounding:
RDInt = Convert.ToInt32(RDAmt * Prd * (Prd + 1) * ROI / 2400);    // banker's rounding
```

*The ROI row is picked by `GetROIFor_TermDeposit` (DatabaseTermDepositNew 752-806): latest `wef ≤ ValueDate`,
`PeriodType 'M'`, `PeriodBegin ≤ n ≤ PeriodEnd`. No row → 0. The foreclosure row (200-224): `TD_Type = 4`, latest
`TDfc_wef ≤ ValueDate`, `TDfc_Delete = 0`; no row → 0.*

### 5.2 Months run (n) — proposed

TSIS counts calendar months from the value date to the **date of the last receipt**, whatever was paid. That pays a
member in arrears as if every instalment had come in on time, and pays nothing for the months between the last receipt
and the refund. Proposed:

```
i  = instalments received   = InstalmentCount(Received, Instalment)   (EcsRdReceiptAppropriator, whole instalments)
m  = completed months from ValueDate to the refund date (AddMonthsKeepEnd(ValueDate, m) ≤ refund date)
n  = min(i, m, PeriodInMonths)
```

This is the convention of the maturity formula (spec 15 §6.3): with `n` instalments, the running balances are
D, 2D, … nD for one month each. A member who paid every month gets `n = i`; one in arrears gets credit only for what
was paid; an advance payer gets no credit for months that have not yet run. See §15.2.

### 5.3 Applied rate

```
contractBand = ROI row of the RD's scheme for n months, PeriodType 'M', latest Wef ≤ ValueDate   (spec 15 §6.1 picker)
fc           = termdeposit_fctemplate row, TD_Type = 4, TDfc_Delete = false, same BrCode, latest TDfc_Wef ≤ ValueDate
AppliedROI   = max(0, contractBand.Roi − fc.TDfc_Roi)
```

- `n = 0` → no interest, no rate needed.
- No ROI row for `n` → "Rate of interest is not defined for <scheme> for <n> months"; Initiate blocked.
- No foreclosure row → **0** is deducted, as in TSIS, and the Applied ROI equals the band rate (§15.3 — the local
  database has **no `TD_Type 4` row**; the only row is `TD_Type 3` (FD), 2 %, wef 01-01-2000).
- Reuse `TermDepositROITemplateRepository.GetROIForTermDepositAsync` / the spec 15 picker so the three RD screens agree.

### 5.4 Interest

| Case | Rule |
|---|---|
| **Premature**: refund date < `MaturityDate` | `Interest = round(D × n × (n + 1) × AppliedROI / 2400)` |
| **Matured**: refund date ≥ `MaturityDate` and Received = D × `PeriodInMonths` | `Interest = MaturityAmount − Received`, rate = contract ROI |
| **Matured but short**: refund date ≥ `MaturityDate`, Received < D × Period | premature formula with `n = i`, at the **contract** band rate without foreclosure deduction (§15.4) |

Rounding: `Math.Round(x, 0, MidpointRounding.AwayFromZero)` in `decimal` (TSIS `Convert.ToInt32` rounds x.5 to even).
No interest after maturity (TSIS pays none either; §15.5).

### 5.5 Interest calc date

`InterestAppliedDate` = the refund date (business day) for a premature refund, `MaturityDate` for a matured one.
*TSIS stores `TrnDate + n months` for an incompletely paid RD — a date in the future.*

### 5.6 Worked example

Scheme 40001, instalment 500, 12 months, value date 14-Nov-2026, contract ROI 8 (band 1–12), maturity 14-Nov-2027 /
6,260 (spec 15 §6.4). Assume a `TD_Type 4` foreclosure row of 2 %.

| Case | Received | n | Applied ROI | Interest | Payable |
|---|---|---|---|---|---|
| A. Paid 7 on time, refund 20-Jun-2027 | 3,500 | min(7, 7, 12) = 7 | 8 − 2 = 6 | 500×7×8×6/2400 = **70** | 3,570 |
| *A in TSIS (last receipt 14-May-2027)* | *3,500* | *6* | *6* | *52.5 → `Convert.ToInt32` = 52* | *3,552* |
| B. Paid 3 only, refund 20-Jun-2027 | 1,500 | min(3, 7, 12) = 3 | 6 | 500×3×4×6/2400 = **15** | 1,515 |
| *B in TSIS (last receipt 14-Jan-2027)* | *1,500* | *2* | *6* | *500×2×3×6/2400 = 7.5 → 8* | *1,508* |
| C. Paid all 12, refund 15-Nov-2027 | 6,000 | — | 8 | 6,260 − 6,000 = **260** | 6,260 |

(Case B ignores penal; with penal pending P the payable is 1,515 − P, §6.)

## 6. Penal pending

The RD may carry unpaid penal interest (spec 14 §6). *TSIS ignores it on refund: the member is paid in full and the
penal stays on the ledger of a closed RD.*

**Proposed:** compute penal pending exactly as spec 14 (`EcsRdReceiptAppropriator.Compute` / `PenalCurrent`, as on the
refund date) and **deduct** it from the payout: it is collected into the scheme's `PiLed_Id`
(40001 → `110050000059` "PENAL INTEREST RECEIVED ON BELATED RD"). Shown in its own grid column and total. See §15.6.

## 7. Loan on RD

*TSIS (`GetTDLoanDetails`, TermDepositService 458-486; `GetTDLoanBalanceOnTDIds`, DatabaseTermDepositNew 1101-1127)
finds loans through `Lien_Tr.TD_Id`, recomputes interest to the earlier of the transaction date and the latest maturity
of the liened RDs (`Calculate_Interest`, simple, days/365, rounded to a rupee), and recovers principal + interest out of
the refund: a `Loan_Trn` `'R'` row per loan, credit `Map_Fin.RD_Loan_Led_Id` / `RD_Loan_Int_Led_Id` by adjustment, and
the RD debits split into an adjustment part (the loan) and a cash part (the rest). `Save_TDLoanRecovery`
(DatabaseTransactions 1670-1717) closes a fully paid loan and clears `LienMarked`.*

ECS today has no loan on RD:

- loan scheme 6 "LOAN ON RECURRING DEPOSIT" has `loan_type 0` and both ledgers `110050000000` (none);
- `lien` and `lien_trn` are empty; acc 14 (Loan on Recurring Deposit) and acc 25 (RD Loan Receipt) have no components.

**Proposed:** refuse the refund of an RD with `LienMarked = true` or any `lien_trn` row with `LienTr_Delete = false` and
`LienTr_Closed = false`: "RD No X is pledged for a loan; close the loan first". Loan recovery out of a refund is added
with the RD-loan spec. See §15.7.

## 8. Validation (Initiate, and again on approval)

| # | Rule | Message |
|---|---|---|
| 1 | At least one RD selected | "Select the recurring deposit(s) to refund" |
| 2 | Every selected RD is open, an RD of this branch, and the member is a holder (§4.1) | "RD No X is not open for refund" |
| 3 | No lien on a selected RD (§7) | "RD No X is pledged for a loan; close the loan first" |
| 4 | ROI row exists for `n` when `n > 0` (§5.3) | §5.3 message |
| 5 | Net payable > 0 | "Refund amount is nil" |
| 6 | Total payable of each RD ≥ 0 (penal cannot exceed received + interest) | "Penal on RD No X exceeds the amount payable" |
| 7 | RD not in another pending (I/M) acc 2 or acc 10 staging line (§9) | "RD No X is in another pending transaction" |
| 8 | RD not in a posted demand whose office recovery is not yet saved (spec 12/13) | "RD No X is in the demand of <due date>, not yet recovered" |
| 9 | Only one RD Refund line per staging voucher | "Only one RD refund can be made in a voucher" |
| 10 | ECS branch, business day open (§9) | as spec 14 |

*TSIS `Validate_Input` (304-329) checks only that the grid is not empty and the total payment is not 0. A loan bigger
than the payout gives a negative net payable without complaint.*

## 9. Concurrency, business day and approval recheck

- **Business day.** Load, Initiate and approval refuse unless the token's `CurrentDate` = the branch's open business
  day = the staging date (`GetOpenBusinessDayAsync`, as spec 14).
- **ECS gate.** `RdRefundController` refuses with 403 unless `Gen_Bank_Name.Bank_Type` = 3 (`IsEcsAsync`);
  `BuildPostingAsync` checks it again.
- **Locks.** An RD in a pending acc 10 line cannot be selected again, and cannot be received under acc 2; an RD in a
  pending acc 2 line cannot be refunded. Both handlers deserialise the other account's pending JSON to find the
  `TD_Id`s (spec 11 / 14 pattern; `EcsRdReceiptRepository.GetPendingReceiptDataAsync` is extended or mirrored). The
  approving staging excludes itself.
- **One line per voucher.** `TransactionsHandler` counts `case 10` lines like `rdReceiptLines` / `rdCreateLines`.
- **Recheck on approval.** `BuildPostingAsync` re-reads each RD, recomputes §5 and §6 and compares received, n,
  applied ROI, interest, penal and net payable with the staged figures (tolerance 0.005). Any difference, or an RD now
  closed, throws `EcsRecoveryException` naming the RD and figure; the Checker sees it and the Maker re-initiates.
- **Staged amount.** `Payment_Amount` of the staging line must equal the net payable; `Receipt_Amount` must be 0.

## 10. DTOs

`Infin8.Coapp.Dto/EcsRdRefundVM.cs`:

```csharp
public class DtoEcsRdRefund
{
    public decimal MemId { get; set; }
    public string? MemberNo { get; set; }
    public string? MemberName { get; set; }
    public DateTime ApplicationDate { get; set; }      // business day = refund date
    public int CashOrAdjustment { get; set; }          // 1 cash, >1 adjustment
    public List<DtoEcsRdRefundLine> Lines { get; set; } = new();   // selected RDs only
    public double TotalReceived { get; set; }
    public double TotalInterest { get; set; }
    public double TotalPenal { get; set; }
    public double NetPayable { get; set; }             // = staging Payment_Amount
}

public class DtoEcsRdRefundLine
{
    public decimal TD_Id { get; set; }
    public string? TD_No { get; set; }
    public int TDScheme_Id { get; set; }
    public double Instalment { get; set; }
    public DateTime ValueDate { get; set; }
    public int PeriodInMonths { get; set; }
    public double RateOfInterest { get; set; }         // contract
    public DateTime MaturityDate { get; set; }
    public double MaturityAmount { get; set; }
    public double PenalRate { get; set; }

    public double Received { get; set; }               // Σ DepositReceiptAmount
    public int InstalmentsReceived { get; set; }       // i
    public int MonthsRun { get; set; }                 // n (§5.2)
    public bool IsMatured { get; set; }
    public double BandRoi { get; set; }
    public double ForeclosureRoi { get; set; }
    public double AppliedRoi { get; set; }
    public double Interest { get; set; }
    public DateTime InterestAppliedDate { get; set; }
    public double PenalPending { get; set; }           // §6
    public double TotalPayable { get; set; }           // Received + Interest − PenalPending
    public bool Selected { get; set; }
}
```

## 11. Posting on approval (`TransactionsHandler.SaveTransaction`, `case 10`)

`BuildPostingAsync(dto, stagingId, transactedDate, brCode, yrId, checkedBy, vocId, cashOrAdjustment, status)` returns
`EcsRdRefundPosting { TermDepositTrns, VoucherTrns, ClosedTdIds }`. `SaveTransaction` adds the trns to its
`termDepositTrnList` and the voucher lines to `finVoucherTrns`; the master is marked closed through
`TermDepositMaster.UpdateTermDepositMasterAsClosed` (already used by FD refund `case 8`) inside the same transaction.

### 11.1 `termdeposit_trn` — one row per RD

| Column | Value | *TSIS (`GetTermDepositTrnObject`, 410)* |
|---|---|---|
| `Trn_Date` | business day | same |
| `TD_Id` | the RD | same |
| `DepositDemandAmount`, `DepositReceiptAmount`, `MaturityAmount` | 0 | same |
| `InterestCalculatedAmount` | interest | same |
| `InterestAppliedDate` | §5.5 | *TrnDate + n months when not fully paid* |
| `InterestPaidAmount` | interest | same |
| `DepositPaidAmount` | received | same |
| `PenalCalculatedAmount` / `PenalAppliedDate` | penal current accrued to the refund date / refund date (as spec 14) | *0 / null* |
| `PenalReceivedAmount` | penal pending deducted (§6) | *0* |
| `NoOfInstalments` / `InstalmentAmount` | 0 | same |
| `LastInstalmentDate` | null | same |
| `LastPaymentDate` | business day | *null* |
| `TD_OE` / `TD_Delete` | false | same |
| `Voc_Id`, `Usr_Id` (checker), `Yr_Id`, `BrCode`, `Voc_Status 'V'` | | *logged-in user / year* |
| `Demand_Id` / `TDCalc_Id` | 0 | same |

`TDTrn_Id` / `Trn_SlNo` through the shared `AddTermDepositTrnListAsync`, as spec 14.

### 11.2 `termdeposit_master`

`AccountClosed = true` for each refunded RD. *TSIS does the same (`Update TermDeposit_Master set AccountClosed = 1`,
DatabaseTransactions 349-357).* No closed-date column exists; the closing trn row's date serves. Spec 12's demand
already skips `AccountClosed` RDs (`EcsDemandRepository` ≈181), as does spec 14's receipt list.

### 11.3 `fin_voucher_tr` — per RD

| Line | `led_id` | Dr (`voc_pmt`) | Cr (`voc_rpt`) | `voc_Trn_Type` |
|---|---|---|---|---|
| Deposit | scheme `Led_Id` (40001: `110050000032`) | received | | see below |
| Interest (if > 0) | scheme `IntLed_Id` (40001: `110050000069`) | interest | | see below |
| Penal (if > 0) | scheme `PiLed_Id` (40001: `110050000059`) | | penal | 2 (adjustment) |

- **Cash, no penal:** both debits are type 1; `SaveTransaction`'s normal flow credits cash with the net payable.
- **Cash with penal:** the penal is met by adjustment, as TSIS does for a loan: the deposit debit is split into an
  adjustment part equal to the penal and a cash part for the rest (spilling into the interest line if received <
  penal). The type-1 debits then total the net payable.
- **Adjustment:** every line type 2; the counter-entry comes from the rest of the voucher as for any adjustment line.
- `Status` = the account's `acc_status` (`'RP'`), falling back to `'RP'`.
- `voc_narr` = `"<memberno> <name> RD No <TD_No>"`; `Description` = `"RD No <TD_No>"`; `Mem_Id` = the member.
- Refuse when a needed ledger id is 0. Final check: Σ type-1 debits − Σ type-1 credits = `Payment_Amount` (cash).

*TSIS takes the ledgers from the single-row `Map_Fin` (`RD_Led_Id`, `RD_Int_Led_Id`; DatabaseAccount 209-223). Here
`map_general.rd_led_id` / `rd_int_led_id` / `rd_pi_led_id` are 0, so the scheme ledgers are used, as in specs 14/15.*

## 12. View component and delete

**View (`RecurringDeposit_Refund_View`)** reads the staged `DtoEcsRdRefund` and shows the member, the selected RDs with
every §4.2 column, and the §4.4 totals. No `@rendermode` of its own and no ECS gate, like `RecurringDeposit_Receipt_View`.

**Delete before approval:** the normal staging delete; nothing was posted, the lock (§9) lifts.

**Delete after approval:** none — the app has no delete for an approved Member Transaction voucher (spec 14 §17.1). When
one is built: soft-delete the closing trn rows and voucher by `Voc_Id` and set `AccountClosed = false`, refusing if any
later valid trn exists for the RD.

## 13. TSIS behaviours not carried over

| # | TSIS behaviour | This spec |
|---|---|---|
| 1 | `n` = months from value date to the **last receipt**, whatever was paid — arrears earn as if paid on time | §5.2: `min(instalments, months run, period)` |
| 2 | `IntCalcDate = TrnDate + n months` (a future date) for an incompletely paid RD | §5.5: refund date |
| 3 | "Revised maturity" = last instalment + 1 month: a member who paid all instalments early gets the full maturity interest before the maturity date | §5.4: maturity amount only on/after `MaturityDate` |
| 4 | `Convert.ToInt32` rounds x.5 to even | `AwayFromZero` |
| 5 | Penal pending ignored on refund | §6: deducted (§15.6) |
| 6 | Loans on RD recovered out of the refund | §7: refused while a lien exists (§15.7) |
| 7 | RD with no trn row silently dropped; no `BrCode` filter; ids concatenated into SQL | §4.1 |
| 8 | Ledgers from `Map_Fin`, 0 if unset | §11.3: scheme ledgers, refused when 0 |
| 9 | Negative net payable (loan > payout) accepted | §8.5, §8.6 |
| 10 | `NoOfMonths = Convert.ToInt16(received / instalment)` computed and then overwritten | — |
| 11 | Missing ROI / foreclosure row → 0 silently | §5.3: missing ROI blocks; missing foreclosure shown as 0 |
| 12 | `UpdateRDPaymentObject` shows message boxes and returns half-built lists on error | approval throws, nothing saved |
| 13 | Label typos ("Recording Deposit Receivable", "Receivedived", "Appplied ROI", "Intereset") | corrected |

## 14. Decisions proposed

1. **Maker–checker** through staging with an approval recheck (spec 11/14/15 pattern).
2. **Several RDs of the member in one line**, as in TSIS; one RD Refund line per voucher.
3. **Interest** by §5: premature = closed-form simple interest on `n` months at band ROI − foreclosure rate; matured =
   maturity amount − received.
4. **Penal pending deducted** into `PiLed_Id`.
5. **Refund refused while the RD is under lien**; loan recovery comes with the RD-loan spec.
6. **Scheme ledgers** `Led_Id` / `IntLed_Id` / `PiLed_Id`, refused when 0.
7. **No schema change.** Every column used exists. Data: a `termdeposit_fctemplate` `TD_Type 4` row per branch (§15.3).
8. **ECS only**, gated on the server and the client.

## 15. Open points for the reviewer

1. **Account row typo.** Confirm the §2.1 update (`RecurringDepsit_*` → `RecurringDeposit_*`) on every database.
2. **Months run.** `n = min(instalments received, completed months, period)` (proposed) or TSIS's months to the last
   receipt? (Example A: 70 vs 52; B: 15 vs 8.)
3. **Foreclosure rate.** The local database has only `TD_Type 3` (FD, 2 %). What is the RD pre-closure rate and from
   when? Proposed insert (per branch, ids by MAX+1):
   ```sql
   INSERT INTO termdeposit_fctemplate (tdfc_id, td_type, tdfc_wef, tdfc_roi, tdfc_delete, usr_id, yr_id, brcode)
   SELECT MAX(tdfc_id) + 1, 4, DATE '2000-01-01', 2, false, MAX(usr_id), MAX(yr_id), '11005'
   FROM termdeposit_fctemplate WHERE brcode = '11005';
   ```
   Is the rate taken as on the **value date** (TSIS) or the refund date? Is the band rate for `n` months (TSIS) or the
   contract rate?
4. **Matured but short.** An RD past maturity with missed instalments: contract band rate without foreclosure deduction
   (proposed), or premature rate?
5. **After maturity.** Any interest (e.g. SB rate) for the days between maturity and refund? TSIS pays none.
6. **Penal.** Deduct pending penal (proposed) or ignore it like TSIS?
7. **Loan on RD.** Refuse under lien (proposed) or build loan recovery now? ECS has no working RD-loan scheme today.
8. **Pending demand.** Refuse a refund while the RD is in a posted demand not yet recovered (proposed, §8.8)?
9. **Joint holders.** Any holder may draw the refund (TSIS: any holder in `TermDeposit_Members`), or only the first holder
   / per mode of operation?
10. **Minimum period.** Is there a lock-in (e.g. no interest before 3 or 6 months)? TSIS: none beyond the ROI bands.
11. **Test data.** Branch 11005 has no RD yet; create them with spec 15 (and spec 14 receipts, backdated through Day
    Begin) before testing.

## 16. Server side (vertical slice per CLAUDE.md)

| Layer | Item |
|---|---|
| Utility | `EcsRdRefundCalculator.cs` (pure: §5.2–§5.5, total payable, §8 rules); reuses `EcsRdReceiptAppropriator.AddMonthsKeepEnd` / `InstalmentCount` / `PenalCurrent` |
| Dto | `EcsRdRefundVM.cs` (§10) |
| Repository | `EcsRdRefundRepository` + interface: open RDs of a member with trn sums (§4.1), ROI rows, foreclosure rows, scheme ledgers, lien check, pending acc 2/10 staging JSON, posted-demand check. Query syntax, `BrCode` on every alias. |
| UnitOfWork | interface property, backing field, lazy property |
| BusinessLogic | `EcsRdRefundHandler` + interface: `GetRdsAsync(memId, brCode, date)`, `BuildPostingAsync(...)`; `EcsRdReceiptHandler` also locks RDs in pending acc 10 lines |
| Program.cs | `AddScoped<IEcsRdRefundHandler, EcsRdRefundHandler>()` |
| TransactionsHandler | inject `IEcsRdRefundHandler`; fill `case 10`; one-line-per-voucher counter |
| Controller | `RdRefundController`: `GET api/rdrefund/Rds/{memId}` — BrCode and date from claims, ECS-gated |
| Client | `Components/TermDeposit/RecurringDeposit_Refund.razor`, `RecurringDeposit_Refund_View.razor`; switch cases in §2; `RDRefundInitiate` handler in the host |

## 17. Verification (manual — the project has no tests)

In ECS branch 11005, with RDs opened under spec 15 and instalments received under spec 14, initiate and approve, then
check `termdeposit_trn`, `termdeposit_master.accountclosed` and `fin_voucher_tr` against a hand calculation.

| # | Case | Expect |
|---|---|---|
| 1 | §5.6 A, cash | n 7, applied ROI 6, interest 70, net 3,570; trn paid 3,500 / int 70; Dr `110050000032` 3,500, Dr `110050000069` 70, cash Cr 3,570; RD closed |
| 2 | §5.6 B (arrears, penal pending P) | n 3, interest 15; penal P deducted and credited to `110050000059`; net 1,515 − P |
| 3 | §5.6 C (matured) | interest 260, rate 8, `InterestAppliedDate` = maturity date |
| 4 | Two RDs selected | two trn rows, two sets of voucher lines, both closed |
| 5 | Adjustment | all lines type 2 |
| 6 | RD in a pending acc 2 line | not selectable / approval refused |
| 7 | Same RD in two acc 10 lines | second refused |
| 8 | RD with `LienMarked` | refused (§7) |
| 9 | A receipt (acc 2) approved between initiate and approve | approval refused: figures changed |
| 10 | Day End between initiate and approve | refused: business day |
| 11 | PCARDB branch | API 403; component shows "ECS only" |
| 12 | After approval: RD Receipt and next demand | the RD no longer appears |
| 13 | Checkerboard and Member Transaction line view | `RecurringDeposit_Refund_View` shows the RDs and totals |

## 18. Settled decisions and departures (implementation)

Every §15 point was settled with the §14 proposal:

1. **Account row.** `specs/ecs-extension/16-rd-refund.sql` carries the §2.1 update; run it on every ECS database (done on the
   local `ECS` database).
2. **Months run** `n = min(instalments received, completed months, period)`.
3. **Foreclosure rate** — `TD_Type 4`, as on the **value date**, deducted from the band rate for `n` months. No row → 0
   deducted (the local database has none, so the full band rate applies until one is added; the insert is commented in the
   `.sql` file).
4. **Matured but short** — `n = i` at the contract band rate, no foreclosure deduction.
5. **After maturity** — no interest.
6. **Penal** — pending penal (arrear + current, spec 14) deducted and credited to `PiLed_Id`.
7. **Loan on RD** — refused while `LienMarked` or an open `lien_trn` row exists.
8. **Pending demand** — refused while the RD is in a posted demand whose recovery for the member's **current** pay office is
   not saved.
9. **Joint holders** — any holder may draw (same RD list as spec 14).
10. **Minimum period** — none beyond the ROI bands.

Where the code departs from the draft:

- **RD list.** `EcsRdRefundHandler` reuses `EcsRdReceiptRepository.GetEligibleRdsAsync` and `GetPenalTemplatesAsync` (same §4.1
  rule; the ROI rows also give the penal rate). An RD with no trn row is listed with nothing payable, and Initiate refuses it as
  "Refund amount is nil".
- **Blocked RDs are listed, not hidden.** Lien, a pending acc 2 / acc 10 line, an unrecovered demand and a missing ROI row are
  shown under the RD No and its checkbox is disabled (§4.3's separate loan panel is not built). The same text is the approval
  refusal.
- **Closing the master.** `EcsRdRefundRepository.MarkClosedAsync` sets `AccountClosed` on the tracked rows (BrCode-filtered) and
  leaves the save to the approval, instead of `TermDepositMaster.UpdateTermDepositMasterAsClosed`, which has no BrCode filter,
  saves mid-transaction and swallows errors.
- **Voucher type in adjustment mode** is the line's `Cash_Or_Adjustment` value (as specs 14/15), not a literal 2.
- **Narration** is `"<memberno> <name> RD No <TD_No>"`.
- **Locks both ways.** `EcsRdReceiptHandler` also locks RDs in pending acc 10 lines; its approval message now reads "another
  pending RD receipt or refund".
