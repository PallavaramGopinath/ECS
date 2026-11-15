# 18 — ECS Recurring Deposit Loan Receipt (acc_id 25, "Recurring Deposit Loan Receipt")

> Status: **Draft for review.**
>
> Ported from these TSIS pieces:
>
> - `uc_RDLoanRecovery`;
> - `TermDepositService.GetTDLoanBalanceByLoanId`;
> - `DatabaseTransactions.Save_TransactionsNew` `case 25` → `Save_TDLoanRecovery`.
>
> How this spec is laid out:
>
> - §13 lists TSIS behaviours deliberately not carried over.
> - §14 lists the decisions proposed here.
> - §15 lists the points the reviewer must settle.
> - Text in *italics* describes what TSIS does.
>
> Companion spec: `17-rd-loan-disbursement.md` (acc 14), which opens the loans this account repays. Patterns are reused from:
>
> - `11-ecs-loan-recovey-component.md` (acc 58) — balances and the interest calculator;
> - `16-rd-refund.md` (acc 10).

## 1. Purpose

A Maker uses the **Member Transaction** screen to receive repayment of a member's **loans on RD** (spec 17), by cash or adjustment.

- Interest is worked out to the business day as simple interest on the principal outstanding.
- The receipt goes to interest first, then principal.
- A loan repaid in full is closed and its lien released, so the pledged RDs can be refunded again (spec 16).

Scope:

- ECS only.
- One member per line.
- `Loan_Type 4` loans of the member.

Out of scope:

- Loans of `Loan_Type 1`: acc 58, spec 11.
- Recovering the loan out of an RD refund: §15.6.
- Demand. RD loans are not demanded (spec 17 §2.2).

## 2. Where it lives

| Item | Value |
|---|---|
| Component (Maker input) | `Infin8.Coapp.UI.Client/Components/TermDeposit/RecurringDeposit_LoanReceipt.razor` |
| View component | `Infin8.Coapp.UI.Client/Components/TermDeposit/RecurringDeposit_LoanReceipt_View.razor` |
| Host page | `Pages/Member_Transaction.razor` (`/member-transactions`) |
| Account | `account_transactions.acc_id = 25`, "Recurring Deposit Loan Receipt", **receipt** side (`acc_type 'R'`), `acc_status 'RDLR'` |
| Application date | the business day, `userInfoDto.CurrentDate`, passed in by the host as `TransactionDate` |

The acc 25 slot exists in the code, but every case is empty:

| Place | Line (today) | Needed |
|---|---|---|
| `Member_Transaction.razor` receipt switch | `case 25` sets flags only (920) | also `isComponentLoaded = true`, as `case 2` / `case 17` (903-906) |
| `Member_Transaction.razor` `GetComponentName` | `case 25` (1383) | namespace `Components.TermDeposit`, parameters as §3.1 — copy `case 17` (1310-1333) |
| `Member_Transaction.razor` second switch | `case 25` (1877) | as `case 17` |
| `Member_Transaction.razor` older view switch | `case 25` (2275) | as `case 17` |
| `Member_Transaction.razor` live `GetViewComponentName` | not in the namespace switch (2079-2101) | add 25 to the `Components.TermDeposit` group `case 2 / 10 / 17` (and 14) |
| `Member_Transaction.razor` delete switch | `case 25` empty (2452) | reset the component state, as `case 17` (2438) |
| `Checkerboard.razor` `GetViewComponentName` | only commented (352) | add 25 to the live TermDeposit group (583-587) |
| `TransactionsHandler.SaveTransaction` | `case 25: /// RD Loan Receipt` `break;` (834) | build posting (§11) |

Line numbers are from `main` at `d9e9074`; check them before editing.

### 2.1 Account row — **component names missing**

The local `ECS` database has:

| acc_id | acc_name | acc_type | acc_status | acc_belongsto | component_name | view_component_name |
|---|---|---|---|---|---|---|
| 25 | Recurring Deposit Loan Receipt | R | RDLR | M | *(empty)* | *(empty)* |

Proposed, to be run on every ECS database:

```sql
UPDATE account_transactions
SET component_name = 'RecurringDeposit_LoanReceipt',
    view_component_name = 'RecurringDeposit_LoanReceipt_View'
WHERE acc_id = 25;
```

## 3. Maker–checker flow

1. The Maker sets up the transaction:
   - picks the member;
   - picks **Receipt**;
   - picks account **Recurring Deposit Loan Receipt**;
   - picks Cash or Adjustment.
2. The component lists the member's open RD loans with their balances (§4).
3. The Maker enters the **amount received** and the grid is appropriated (§6). The Maker may instead edit the interest and
   principal per loan (§6.2).
4. **Initiate** adds one `staging_details` row:
   - `Related_Account_Id = 25`;
   - `Related_Account_Data` = the serialised `DtoEcsRdLoanReceipt` (§10);
   - `Receipt_Amount` = total received.
5. The Checker opens the line. The View fetches its JSON through `api/Staging/GetStagingDetailsById`.
6. On approval, `SaveTransaction case 25` calls `EcsRdLoanReceiptHandler.BuildPostingAsync`. It recomputes the balances and
   refuses on any change (§9), then adds rows (§11).

*TSIS has no staging:*

- *`uc_RDLoanRecovery` is opened from `FrmTransactions2.cs:1118`.*
- *Loans are listed by `GetLoanNos(memId, 4)`.*
- *A **Get Loan Balance** button fills the grid.*
- *`UpdateRDLoanRecoveryObject` builds the trn and voucher objects.*
- *`Save_TDLoanRecovery` (`DatabaseTransactions.cs:1670`, `case 25` at 492) writes them.*

### 3.1 Host parameters

The component takes:

- `AccountId`, `CashOrAdjustment`, `MemberId`, `MemberNumber`, `MemberName`, `TransactionDate`;
- `OnRDLoanReceiptComponentClose` (`EventCallback<bool>`);
- `OnRDLoanReceiptInitiate` (`EventCallback<DtoEcsRdLoanReceipt>`).

The host's `RDLoanReceiptInitiate` works like `RDCreateInitiate`.

## 4. Screen

Gate: the `isReady` / "ECS only" pattern.

### 4.1 Loans fetched on load

`GET api/rdloanreceipt/Loans/{memId}` returns the member's loans where:

- `loan_master.Mem_Id = memId`, `Loan_Type = 4`, `Loan_Delete = false`, `IsAccountClosed = false`, `Voc_Status 'V'`;
- `BrCode` = the claims branch on **every** alias (master, trn, lien_trn, termdeposit_master).

The list is ordered by `San_Date`, then `Loan_Id` (oldest first).

No loans → "The member has no open loan on RD".

### 4.2 Loan grid

| Column | Source | *TSIS column* |
|---|---|---|
| Loan No | `Loan_No` | Loan No |
| Disbursed on | first `loan_trn` `I` date | Disb Date |
| Amount | Σ `Disb_Amt` | Disb Amt |
| ROI | `loan_master.Roi` (latest `loan_roi` row ≤ date) | ROI |
| Pledged RDs | `RD No`s from live `lien_trn` → `termdeposit_master` | — |
| Interest to | §5.2 | Int Calc Date |
| Principal outstanding | §5.1 | Prl Bal |
| Interest due | §5.2 | Int Bal |
| Total due | principal + interest | Total |
| Interest received | §6, editable | Int Receipt |
| Principal received | §6, editable | Prl Receipt |
| Total received | interest + principal received | Tot Receipt |
| Closes the loan | yes when everything due is received | — |

### 4.3 Totals and buttons

The screen shows:

- Total due;
- **Amount received** (entered);
- total interest received;
- total principal received.

Buttons: **Appropriate**, **Initiate**, **Close**.

## 5. Balances

### 5.1 Principal outstanding

```
Principal outstanding = Σ Disb_Amt − Σ PrlColl_Amt
```

The sum runs over valid `loan_trn` rows of the loan (`TrnTr_Delete = false`, `Voc_Status 'V'`), dated on or before the
business day. This is the spec 11 rule: balances come from `loan_trn`, never from `San_Amt`.

### 5.2 Interest

```
from        = last IntCalc_Date of the loan's valid loan_trn rows (the 'I' row stamps the disbursement date)
to          = min(business day, Last_DueDate)                                   — the maturity cap, §15.2
interest    = Σ IntCalc_Amt − Σ IntColl_Amt   (arrear: interest calculated but not collected)
            + round( Σ principal × Roi × days / 36500 , 0, AwayFromZero )         (current: from → to)
```

- The current interest uses `EcsInterestCalculator.Calculate`. It works segment-wise, so a part principal receipt or a
  rate change in `loan_roi` splits the period, and it rounds **once** per loan.
- `to ≤ from` gives current interest 0.
- `Last_DueDate` is the latest maturity of the pledged RDs (spec 17 §6.3).
- Penal does not apply (`pi_application 0`, `Pi 0`).

*TSIS:*

- *Uses `GetLoanBalanceOnLoanIds` (`DatabaseLoan.cs:1103`) and `Calculate_Interest` (`GeneralService.cs:848`).*
- *The formula is `Round(amt × roi / 100 × days / 365, 2)`, then `(int)(x + 0.5)`.*
- *The end date is capped at `GetMaxMaturityDate` of the liened TDs (`TermDepositService.cs:536-543`).*
- *It finds the TDs through `Lien_Tr`, and refuses "Lien are not availabe" when there are none.*

### 5.3 Worked example

The loan of spec 17 §6.4: 8,000 at 10.5 %, disbursed 20-Jun-2027, last due 14-Nov-2028.

| Date | Days | Interest | Received | → interest | → principal | Principal after |
|---|---|---|---|---|---|---|
| 20-Dec-2027 | 183 | 8,000 × 10.5 × 183 / 36500 = 421.15 → **421** | 3,000 | 421 | 2,579 | 5,421 |
| 20-Jan-2029 (after the cap) | 330 (20-Dec-2027 → 14-Nov-2028) | 5,421 × 10.5 × 330 / 36500 = 514.62 → **515** | 5,936 | 515 | 5,421 | **0 → loan closed, lien released** |

## 6. Appropriation

The rules live in a pure `Infin8.Coapp.Utility/EcsRdLoanReceiptAppropriator.cs`, shared by WASM and the server.

### 6.1 From the amount received

The amount is applied in this order:

1. interest of every listed loan, oldest first;
2. then principal of every loan, oldest first.

These limits apply:

- An amount above the total due is refused: "Amount received exceeds the total due of ₹N".
- Part payment is allowed.

*TSIS `AppropriateRecovery` (`uc_RDLoanRecovery.cs` ≈430) uses the same order: interest across all loans, then principal.*

### 6.2 Manual split

The Maker may overwrite interest or principal received per loan, in whole rupees. These rules apply:

- interest received ≤ interest due;
- principal received ≤ principal outstanding;
- principal received > 0 only when interest received = interest due (interest first, §15.3).

The amount received becomes the sum.

*TSIS clamps each cell to its balance and does not enforce interest first.*

## 7. (reserved)

## 8. Validation (Initiate, and again on approval)

| # | Rule | Message |
|---|---|---|
| 1 | ECS branch; business day open | as spec 16 |
| 2 | Total received > 0 | "Enter the amount received" |
| 3 | Every loan is open, `Loan_Type 4`, of this branch, and the member's | "Loan No X is not open" |
| 4 | §6 limits | §6 messages |
| 5 | Loan not in another pending (I/M) acc 25 staging line | "Loan No X is in another pending transaction" |
| 6 | Only one acc 25 line per staging voucher | "Only one RD loan receipt can be made in a voucher" |
| 7 | Scheme ledgers `PrlLed_Id` / `IntLed_Id` set (spec 17 §2.2) | "Loan on RD scheme is not set up" |

## 9. Concurrency, business day and approval recheck

- **Business day and ECS gate** — as spec 17 §9. The controller is `RdLoanReceiptController`.
- **Lock.** A loan in a pending acc 25 line cannot be received again. The approving staging excludes itself.
- **Recheck on approval.** The server re-reads each loan and recomputes §5 to the approval date, which must equal the staging
  date. It compares principal outstanding, interest-from date, interest and the split with the staged figures (tolerance
  0.005). A difference throws `EcsRecoveryException`.
- **Staged amount.** `Receipt_Amount` = Σ received; `Payment_Amount` = 0.

## 10. DTOs

`Infin8.Coapp.Dto/EcsRdLoanReceiptVM.cs`:

```csharp
public class DtoEcsRdLoanReceipt
{
    public decimal MemId { get; set; }
    public string? MemberNo { get; set; }
    public string? MemberName { get; set; }
    public DateTime ApplicationDate { get; set; }
    public int CashOrAdjustment { get; set; }
    public List<DtoEcsRdLoanReceiptLine> Lines { get; set; } = new();   // loans with something received
    public double TotalDue { get; set; }
    public double TotalReceived { get; set; }                           // = staging Receipt_Amount
    public double TotalInterest { get; set; }
    public double TotalPrincipal { get; set; }
}

public class DtoEcsRdLoanReceiptLine
{
    public decimal Loan_Id { get; set; }
    public string? Loan_No { get; set; }
    public int Scheme_Id { get; set; }
    public DateTime DisbursedOn { get; set; }
    public double Disbursed { get; set; }
    public double Roi { get; set; }
    public string? PledgedRdNos { get; set; }
    public DateTime LastDueDate { get; set; }
    public double PrincipalOutstanding { get; set; }
    public DateTime InterestFrom { get; set; }
    public DateTime InterestTo { get; set; }
    public double InterestArrear { get; set; }
    public double InterestCurrent { get; set; }          // stamped as IntCalc_Amt
    public double InterestDue { get; set; }
    public double InterestReceived { get; set; }
    public double PrincipalReceived { get; set; }
    public bool ClosesLoan { get; set; }
}
```

## 11. Posting on approval (`TransactionsHandler.SaveTransaction`, `case 25`)

`BuildPostingAsync(...)` returns `EcsRdLoanReceiptPosting { LoanTrns, VoucherTrns, ClosedLoanIds }`.

- `loan_trn` rows go into `SaveTransaction`'s `loanTrnList`. Its `AddLoanTrnListAsync` saves per row, so several loans in one
  approval do not collide (spec 11).
- Voucher lines go into `finVoucherTrns`.
- Closures go through `EcsRdLoanReceiptRepository.CloseLoanAsync` on tracked rows, without saving.

### 11.1 `loan_trn` — one `R` row per loan

| Column | Value | *TSIS* |
|---|---|---|
| `Trn_Status` | `'R'` | same |
| `Trn_Date` | business day | same |
| `IntCalc_Amt` / `IntCalc_Date` | current interest (§5.2) / `InterestTo` | same / *null when 0* |
| `IntColl_Amt` | interest received | same |
| `PrlColl_Amt` | principal received | same |
| `Soc_Roi` | loan ROI | *0* |
| `Demand_Id` | zero id | 0 |
| `Trn_SlNo` | next per loan | *—* |
| `Voc_Id`, `Usr_Id` (checker), `Yr_Id`, `BrCode`, `Voc_Status 'V'` | | |

### 11.2 `fin_voucher_tr` — per loan

| Line | `led_id` | Dr | Cr (`voc_rpt`) | `voc_Trn_Type` |
|---|---|---|---|---|
| Interest (if > 0) | scheme `IntLed_Id` (`110050000053`) | | interest received | `Cash_Or_Adjustment` |
| Principal (if > 0) | scheme `PrlLed_Id` (`110050000010`) | | principal received | `Cash_Or_Adjustment` |

- `Status` = `'RDLR'`.
- `loan_id` = the loan.
- `voc_narr` = `"<memberno> <name> Loan No <Loan_No>"`; `Description` = `"Loan No <Loan_No>"`.
- In cash mode, `SaveTransaction` debits cash with the total.

*TSIS takes `Map_Fin.RD_Loan_Int_Led_Id` / `RD_Loan_Led_Id`. Its voucher `Loan_Id` is 0.*

### 11.3 Closing a fully repaid loan

A loan closes when principal outstanding − principal received = 0 **and** interest due − interest received = 0. Closing it:

- sets `loan_master.IsAccountClosed = true` and `AccountClosedDate` = business day;
- sets `lien.Lien_Closed = true` and `Lien_Recovered = Lien_Amount`;
- sets `lien_trn.LienTr_Closed = true` and `LienTr_Recovered = LienTr_Amount` on every live row;
- sets `termdeposit_master.LienMarked = false` on each pledged RD, **unless** another open lien still holds it (not possible
  under spec 17's one-loan-per-RD rule, but checked).

For a part payment, `Lien_Recovered` / `LienTr_Recovered` are left alone; they record closure only (§15.5).

*TSIS `Save_TDLoanRecovery`:*

- *Closes the loan and clears `LienMarked` through `Lien_Tr`.*
- ***Never** closes the `Lien` / `Lien_Tr` rows.*
- *Returns true even when `Insert_LoanTr` fails.*

## 12. View component and delete

**View.** It shows the member, the loans with the §4.2 columns, the totals, and "closes the loan" flags. It has no rendermode
and no gate.

**Delete before approval.** The normal staging delete.

**Delete after approval.** None (spec 14 §17.1). When one is built, it must:

- soft-delete the `R` rows and the voucher by `Voc_Id`;
- reopen the loan, its lien rows and `LienMarked`;
- refuse when a later valid `loan_trn` exists.

## 13. TSIS behaviours not carried over

| # | TSIS behaviour | This spec |
|---|---|---|
| 1 | `(int)(x + 0.5)` after rounding to 2 places | `AwayFromZero` once per loan, via `EcsInterestCalculator` |
| 2 | Interest single-period on the whole outstanding | segment-wise (§5.2) |
| 3 | Lien rows never closed | §11.3 |
| 4 | Ledgers from `Map_Fin`; voucher `Loan_Id` 0 | scheme ledgers; `loan_id` set |
| 5 | Per-cell clamp, interest-first not enforced on manual entry | §6.2 |
| 6 | Save failure reported as success | the approval throws, nothing is saved |
| 7 | `accountId == 24` test picks the RD or FD list (`uc_RDLoanRecovery.cs` ≈400), always passed 24 | — |
| 8 | No `BrCode` filter; ids concatenated into SQL | §4.1 |

## 14. Decisions proposed

1. Maker–checker with an approval recheck.
2. Several loans of the member per line; one acc 25 line per voucher.
3. Interest is simple, actual/365, segment-wise, rounded once. It is capped at `Last_DueDate`.
4. Interest first, then principal, oldest loan first.
5. Full repayment closes the loan, its lien rows and `LienMarked`.
6. Scheme ledgers `IntLed_Id` / `PrlLed_Id` are used.
7. No schema change. Data: the §2.1 account row and spec 17 §2.2 on every ECS database.
8. ECS only.

## 15. Open points for the reviewer

1. **Account row.** Confirm the component names in §2.1.
2. **Maturity cap.** TSIS charges no interest after the latest RD maturity, so a member who does not repay pays nothing after
   that date. Keep the cap (proposed, as TSIS), or charge to the business day — possibly at a higher overdue rate?
3. **Interest first.** Enforce it on a manual split (proposed), or allow principal first like TSIS?
4. **Matured RDs.** Once the pledged RDs mature, should the loan be recovered from the maturity amount automatically (a
   Checker-run adjustment)? Proposed: no — the member repays through acc 25, or §15.6.
5. **Part-payment lien.** Should `Lien_Recovered` / `LienTr_Recovered` track part payments, and should a part payment free an
   RD whose share is repaid? Proposed: no; the lien is released only on closure.
6. **Recovery out of an RD refund.** Spec 16 refuses a liened RD. Alternatives:
   - **(a)** keep that; the member puts acc 25 (by adjustment) and acc 10 in the same voucher. This does **not** work today,
     because acc 10 checks the lien at Initiate, before acc 25 is approved.
   - **(b)** build TSIS's deduction: `uc_RDPaymentTrn` 106-160, 360, ≈496-531. The `R` rows are credited to the RD-loan ledgers
     and the refund's net payable falls.

   Proposed: (b) as a follow-up change to spec 16, reusing this spec's §5 and §11.
7. **Demand.** RD loans stay out of salary demand (spec 17 §2.2). Confirm that members repay only at the counter or by
   refund.

## 16. Server side (vertical slice per CLAUDE.md)

| Layer | Item |
|---|---|
| Utility | `EcsRdLoanReceiptAppropriator.cs` (pure: §6, the closure test) |
| BusinessLogic | `EcsInterestCalculator` reused for §5.2 |
| Dto | `EcsRdLoanReceiptVM.cs` (§10) |
| Repository | `EcsRdLoanReceiptRepository` + interface: open type-4 loans with trn sums, roi events, pledged RDs, scheme ledgers, pending acc 25 JSON, `CloseLoanAsync`. Query syntax, `BrCode` on every alias. |
| UnitOfWork | three-place wiring |
| BusinessLogic | `EcsRdLoanReceiptHandler` + interface: `GetLoansAsync`, `BuildPostingAsync` |
| Program.cs | `AddScoped<IEcsRdLoanReceiptHandler, EcsRdLoanReceiptHandler>()` |
| TransactionsHandler | inject the handler; fill `case 25`; one-line-per-voucher counter |
| Controller | `RdLoanReceiptController`: `GET api/rdloanreceipt/Loans/{memId}`, ECS-gated |
| Client | `Components/TermDeposit/RecurringDeposit_LoanReceipt.razor`, `_View.razor`; the switch cases in §2; the host callbacks; a live `case 25` in `Checkerboard.razor` |

## 17. Verification (manual)

These cases need a spec 17 loan approved first.

| # | Case | Expect |
|---|---|---|
| 1 | §5.3 row 1, cash 3,000 | `R` row: IntCalc 421 / IntColl 421 / PrlColl 2,579; Cr `…053` 421, Cr `…010` 2,579, cash Dr 3,000; loan open |
| 2 | §5.3 row 2, 5,936 after the cap | interest 515 to 14-Nov-2028; loan closed; `lien` / `lien_trn` closed; `LienMarked` false on both RDs |
| 3 | Then RD Refund (acc 10) on those RDs | allowed |
| 4 | Amount above total due | refused |
| 5 | Manual split: principal with interest short | refused |
| 6 | Same loan in two pending acc 25 lines | second refused |
| 7 | Another receipt approved between initiate and approve | refused: figures changed |
| 8 | Adjustment | lines typed `Cash_Or_Adjustment`, no cash line |
| 9 | PCARDB branch | 403 / "ECS only" |
| 10 | Checkerboard view | `RecurringDeposit_LoanReceipt_View` shows the loans and the split |
