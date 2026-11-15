# 17 — ECS Loan on Recurring Deposit (acc_id 14, "Loan on Recurring Deposit")

> Status: **Draft for review.** Ported from TSIS `uc_LoanOnTermDeposit` in RD mode,
> `uc_TransactionEntries` `case 14` → `UpdateLoanOnTDObject`, and `DatabaseTransactions.Save_TransactionsNew` `case 14` →
> `Save_TDLoanDisbursement`.
>
> - §13 lists the TSIS behaviours deliberately not carried over.
> - §14 lists the decisions proposed here; §15 the points the reviewer must settle.
> - Text in *italics* describes what TSIS does.
>
> Sibling specs:
>
> - `18-rd-loan-receipt.md` (acc 25) covers repaying the loan.
> - Patterns are reused from `14-rs-receipt-component.md` (acc 2), `15-rd-new-account.md` (acc 17),
>   `16-rd-refund.md` (acc 10) and `10-ecs-loan-disbursement.md` (ECS loan rows and id allocator).

## 1. Purpose

A Maker uses the **Member Transaction** screen to lend a member money against one or more of the member's open recurring
deposits, paid out by cash or adjustment.

- **Limit.** The loan can be at most an eligible percentage of the amount **actually received** on the pledged RDs, less
  any loan already outstanding against them.
- **Rate.** The loan rate is the highest rate among the pledged RDs plus a margin.
- **Checker's approval** opens the loan (`loan_master` and its rows), marks a lien on each pledged RD, and debits the
  *Loan on RD* ledger.

Scope:

- ECS only.
- One member per line.
- Loan scheme with `Loan_Type 4` (`LoanCategory.LoansOnRecurringDeposits`).
- RD schemes only (`termdeposit_schemes.tdschemetype = 'R'`).

Out of scope:

- Repayment of the loan: acc 25, spec 18.
- Recovering the loan out of an RD refund: spec 16 §7 refuses a liened RD; see §15.8.
- Loan on FD (acc 13, the PCARDB component `FixedDeposit_LoanDisbursement`).
- Opening-entry (old loan) data entry: *TSIS `btnSave` → `Save_FDLoanOE`*.

### 1.1 Which TSIS code is live

*`TSIS/uc_LoanOnRecurringDeposit.cs` is **dead code**:*

- *it is listed in the csproj, but nothing instantiates it;*
- *its status `"RDI"` appears nowhere else;*
- *`GetRDLoanModel()` (428) returns an empty `TDLoanBaseVM`.*

*It also bases eligibility on `DepositAmount`, which for an RD is the monthly instalment, so it is wrong.*

*The live path is shared with FD loans:*

- *`FrmTransactions2.cs:1393` `case "14"` loads `uc_LoanOnTermDeposit.Instance`, the same control as acc 13 (1367), in RD mode;*
- *Initiate: `uc_LoanOnTermDeposit.cs` 431-466;*
- *model: `UpdateLoanOnTDObject` 550-684 (called from `uc_TransactionEntries.cs:842`);*
- *save: `TsisDataAccess/DatabaseTransactions.cs:389` `case 14` → `Save_TDLoanDisbursement` 1267-1340;*
- *delete: `DatabaseEmployee.cs:3411-3440` `case "RDL"`.*

This spec ports the live path.

## 2. Where it lives

| Item | Value |
|---|---|
| Component (Maker input) | `Infin8.Coapp.UI.Client/Components/TermDeposit/RecurringDeposit_LoanDisbursement.razor` |
| View component (Checker / line view) | `Infin8.Coapp.UI.Client/Components/TermDeposit/RecurringDeposit_LoanDisbursement_View.razor` |
| Host page | `Pages/Member_Transaction.razor` (`/member-transactions`) |
| Account | `account_transactions.acc_id = 14`, "Loan on Recurring Deposit", **payment** side (`acc_type 'P'`), `acc_status 'RDL'` |
| Application date | the business day, `userInfoDto.CurrentDate`, passed in by the host as `TransactionDate` |

The acc 14 slot exists in the code, but every case is empty:

| Place | Line (today) | Needed |
|---|---|---|
| `Member_Transaction.razor` payment switch | `case 14` sets only `isLedgerSection = false` (972) | also `isComponentLoaded = true`, as `case 10` does (962) |
| `Member_Transaction.razor` `GetComponentName` | `case 14: break;` (1573) | namespace `Components.TermDeposit`, parameters §3.1 — copy `case 10` (1526-1547) |
| `Member_Transaction.razor` second switch | `case 14` (2017) | as `case 10` |
| `Member_Transaction.razor` older view switch | `case 14` (2314) | as `case 10` |
| `Member_Transaction.razor` live `GetViewComponentName` | not in the namespace switch (2079-2101) | add 14 to the `Components.TermDeposit` group `case 2 / 10 / 17` |
| `Member_Transaction.razor` delete switch | `case 14` empty (2490) | reset the component state, as `case 10` (2481) |
| `Checkerboard.razor` `GetViewComponentName` | only in a commented block (391) | add 14 to the live `case 2 / 10 / 17` group (583-587) |
| `TransactionsHandler.SaveTransaction` | `case 14: /// RD Loan Disbursement` `break;` (618) | build posting (§11) |

Line numbers are from `main` at `d9e9074`; check them before editing.

### 2.1 Account row

The account row is already set in the local `ECS` database:

| acc_id | acc_name | acc_type | acc_status | acc_belongsto | component_name | view_component_name |
|---|---|---|---|---|---|---|
| 14 | Loan on Recurring Deposit | P | RDL | M | `RecurringDeposit_LoanDisbursement` | `RecurringDeposit_LoanDisbursement_View` |

Run the same on every other ECS database:

```sql
UPDATE account_transactions
SET component_name = 'RecurringDeposit_LoanDisbursement',
    view_component_name = 'RecurringDeposit_LoanDisbursement_View'
WHERE acc_id = 14;
```

`acc_status = 'RDL'` is also the TSIS voucher status for this line (*`uc_LoanOnTermDeposit.cs` Initiate*).

### 2.2 Loan scheme — **must be fixed first**

The local database has one scheme for this product. It cannot be used as it stands:

| scheme_id | scheme_name | loan_type | prlled_id | intled_id | piled_id | loannostartwith |
|---|---|---|---|---|---|---|
| 6 | LOAN ON RECURRING DEPOSIT | **0** | 110050000000 | 110050000000 | 110050000000 | 401 |

The RD-loan ledgers do exist:

- `110050000010` "LOAN ON RD"
- `110050000053` "INTEREST RECEIVED ON RD LOANS"

Today they are wired to scheme **5**, "LOAN ON FIXED DEPOSIT OVERDRAFT" (`loan_type 3`). Scheme 4, "LOAN ON FIXED DEPOSIT ON
DEMAND", uses the FD-loan pair `…009` / `…052`. See §15.1.

Proposed fix, per branch and idempotent:

```sql
-- 1. Scheme 6 becomes the RD-loan scheme.
UPDATE loan_schemes
SET loan_type = 4,
    prlled_id = 110050000010,     -- LOAN ON RD
    intled_id = 110050000053,     -- INTEREST RECEIVED ON RD LOANS
    disb_type = 1,
    int_application = 0,           -- no demand-based application; interest is worked out on receipt (spec 18)
    pi_application = 0             -- no penal
WHERE scheme_id = 6 AND brcode = '11005';

-- 2. Rate margin over the RD rate (§6.2). TSIS hard-codes 2.
INSERT INTO loan_roi_template (roi_id, scheme_id, agency, wef, roi, pi, updt, roitemplate_delete, usr_id, yr_id, brcode, voc_status)
SELECT (SELECT MAX(roi_id) + 1 FROM loan_roi_template WHERE brcode = '11005'),
       6, 'S', DATE '2000-01-01', 2, 0, false, false,
       110050000001, 110050000001, '11005', 'V'
WHERE NOT EXISTS (SELECT 1 FROM loan_roi_template
                  WHERE scheme_id = 6 AND brcode = '11005' AND NOT roitemplate_delete);
```

- Ledger ids embed the branch, so check them per branch.
- The handler looks the scheme up as the single live `Loan_Type 4` scheme of the branch. Zero or several matches →
  "Loan on RD scheme is not set up". *TSIS `GetLoanSchemeId(4)` (`DatabaseLoan.cs:313`) takes the first row.*
- A ledger id ending in `0000000` counts as "not set" and blocks Initiate.

Scheme 6 is `Loan_Type 4`, not `Loan_Type 1`. This is deliberate, and it is an exception to the ECS rule "all loans are type 1,
differentiated by scheme". The reasons:

- Demand (spec 12), appropriation (spec 13) and ECS loan recovery (acc 58, spec 11) all filter `Loan_Type 1`.
- A lien loan must stay **out** of salary demand.
- `LoanCategoryPolicy` already allows `LoansOnRecurringDeposits` for ECS (spec 07 lists type 4 as shared).

## 3. Maker–checker flow

Same as every other Member Transaction account. Status lives in `staging_master` / `staging_details.staging_status`:
`I` initiated → `M` made → `V` verified.

1. The Maker picks:
   - the member;
   - **Payment**;
   - account **Loan on Recurring Deposit**;
   - Cash or Adjustment.
2. The component loads the member's RDs that can be pledged (§4.1).
3. The Maker ticks the RDs. The drawing power, existing loan and eligible amount are shown (§6).
4. The Maker enters the loan amount and presses **Initiate**. One `staging_details` row is added:
   - `Related_Account_Id = 14`;
   - `Related_Account_Data` = the serialised `DtoEcsRdLoan` (§10);
   - `Payment_Amount` = loan amount, split into `CashPayment_Amount` / `AdjustmentPayment_Amount` by `CashOrAdjustment`.
5. The Checker opens the line. `RecurringDeposit_LoanDisbursement_View` receives only `stagingId` and `relatedAccountId`. It
   fetches its own JSON through `api/Staging/GetStagingDetailsById`.
6. On approval, `SaveTransaction case 14` calls `EcsRdLoanHandler.BuildPostingAsync`. It **recomputes every figure** and
   refuses if anything changed (§9). It then adds rows (§11).

*TSIS has no staging. Initiate adds a grid row (payment = loan amount, status `"RDL"`). On **Save**, `UpdateLoanOnTDObject`
builds `TDLoanBaseVM` (`LoanMaster`, `LoanDisbursement`, `LoanTrn`, `LoanRoi`, `LoanLien`, `List<Lien_Tr>`) into
`SaveTrnObject.RDLoanModel`, and `Save_TDLoanDisbursement` writes it.*

### 3.1 Host parameters

Same shape as spec 16 §3.1:

- `AccountId`, `CashOrAdjustment`, `MemberId`, `MemberNumber`, `MemberName`, `TransactionDate`;
- `OnRDLoanComponentClose` (`EventCallback<bool>`);
- `OnRDLoanInitiate` (`EventCallback<DtoEcsRdLoan>`).

After the POST to `api/Staging/AddStaging`, the component invokes `OnRDLoanInitiate`. The host's handler, `RDLoanInitiate`:

- calls `UpdateTransactionsFromStaging("I", brCode)`;
- sets `isComponentLoaded = false` and `isTransactionSection = true`;
- resets `Account_Id`;

exactly like `RDRefundInitiate` (≈2797-2831).

## 4. Screen

Gate: the `isReady` pattern of `ECSLoanSanction.razor` / `RecurringDeposit_Refund.razor`. Show a spinner until the claim is
read, then "ECS only" for a non-ECS branch.

### 4.1 RDs fetched on load

`GET api/rdloan/Setup/{memId}` returns the scheme (§2.2), the eligible percentage (§6.1), the margin (§6.2), and the member's
RDs where:

- the member is a holder: `termdeposit_members.Mem_Id = memId`, `TDMem_Delete = false`;
- in `termdeposit_master`: `TD_Delete = false`, `AccountClosed = false`, `Voc_Status = 'V'`;
- the scheme has `TDSchemeType = 'R'`;
- nothing has been paid out: Σ `DepositPaidAmount` over valid trn rows = 0;
- `BrCode` = the claims branch on **every** alias (master, members, scheme, trn).

This is the RD list of `EcsRdReceiptRepository.GetEligibleRdsAsync` (reused by spec 16). Reuse it.

Each RD is listed with its received amount and its block reason, if any (§8). A blocked RD is shown with its checkbox
disabled, as spec 16 does.

No RDs → "The member has no open recurring deposits".

*TSIS:*

- *`GetTDNos(memId, "R")` (`DatabaseTermDepositNew.cs:226`) fills a checked list, with no branch filter;*
- *the figures come from `GetRDDetailsPayable` → `GetRDDetails` (1009). That query inner-joins `Termdeposit_Trn`, so an RD
  with no receipt silently drops out;*
- *ids are concatenated into the SQL.*

### 4.2 RD grid

| Column | Source | *TSIS* |
|---|---|---|
| Select | checkbox | *checked list* |
| RD No | `TD_No` | RD No |
| Value date | `ValueDate` | |
| Instalment | `DepositAmount` | |
| Period | `PeriodInMonths` | |
| ROI | contract `RateOfInterest` | ROI |
| Maturity date / amount | master | |
| Received | Σ `DepositReceiptAmount`, valid rows | Amount |
| Drawing power | `Received × EligPer / 100` (§6.1) | DrawingPower |
| Block reason | §8 | — |

### 4.3 Existing loans on the selected RDs

This grid is empty unless §15.3 allows top-ups. *TSIS lists the open loans found through `Lien_Tr` (`GetTDLoanIdList`,
`DatabaseTermDepositNew.cs:729`) with:*

- *Loan No;*
- *sanction date / amount;*
- *principal outstanding;*
- *interest due;*
- *total.*

### 4.4 Loan fields and totals

| Field | Value |
|---|---|
| Loan scheme | scheme 6, read-only (§2.2) |
| Loan No | provisional — the next number per §5; the real one is allocated at approval |
| Total RD received | Σ received of the selected RDs |
| Eligible % | §6.1 |
| Borrowing power | §6.1 |
| Existing loan outstanding | §6.1 (0 under the proposed rule) |
| **Eligible amount** | borrowing power − existing |
| Loan ROI | §6.2, read-only |
| Loan due date | §6.3, read-only |
| **Loan amount** | entered; whole rupees; 0 < amount ≤ eligible amount |

Buttons: **Initiate**, **Close**.

*TSIS clamps an amount above the eligible amount back to the eligible amount (`uc_LoanOnTermDeposit.cs` 419-423). Here
Initiate is refused instead, with the message in §8.*

## 5. Loan number

`brCode + LoanNoStartWith + 7-digit running number`, the rule of `LoanMasterRepository.GetNewLoanNo` /
`EcsDisbursementIdAllocator.NextLoanNoAsync` (`EcsLoanDisbursementRepository.cs:288-308`):

- take the highest numeric `Loan_No` of the scheme in the branch, plus 1;
- with none, start at `11005` + `401` + `0000001` = `110054010000001`. That is 15 characters, which fits `loan_master.loan_no
  varchar(15)`.

The number is allocated **at approval**. The number the Maker sees is provisional, and the View shows the real one after
approval.

*TSIS:*

- *`GetMaxLoanNo` (`DatabaseLoan.cs:2236`) is `Max(Loan_No)` of the scheme, with no delete or branch filter;*
- *`VerifyLoanNo` (2307) lets the Maker type a number, provided the first 3 characters equal `LoanNoStartWith`;*
- *the number is fixed at Initiate, so two Makers can collide.*

## 6. Calculations

All of this lives in a pure, DB-free `Infin8.Coapp.Utility/EcsRdLoanCalculator.cs`, shared by the WASM component and the server
recheck (the spec 11 / 14 / 15 / 16 pattern). The handler feeds it the rows it read.

### 6.1 Eligibility

```
received(rd)    = Σ DepositReceiptAmount over valid termdeposit_trn rows of the RD (TD_Delete = false, Voc_Status 'V')
EligPer         = termdeposit_loaneligibletemplate.Elig_Per
                  where TD_Type = 2, TD_Delete = false, BrCode = branch, latest Wef_Date ≤ business day
drawingPower(rd)= received(rd) × EligPer / 100
BorrowingPower  = round( Σ received(rd) × EligPer / 100 , 0, AwayFromZero )
Existing        = Σ principal outstanding of open Loan_Type 4 loans with a live lien_trn on any selected RD
                  (principal = Σ Disb_Amt − Σ PrlColl_Amt from loan_trn, spec 11 rule)
Eligible        = max(0, BorrowingPower − Existing)
```

- The local database has `TD_Type 2` = **85 %**, wef 01-01-2010 (`TD_Type 1`, FD, is also 85 %).
- No row → "Loan eligibility percentage for RD is not defined"; Initiate is blocked.
- Under the proposed one-loan-per-RD rule (§15.3), `Existing` is always 0, because a liened RD cannot be selected. The term
  stays in the formula so that allowing top-ups later is a data decision.

*TSIS `GetTDLoanEligiblePercentage(2)` (`DatabaseTermDepositNew.cs:586`) takes the row with the latest `wef_date` overall. It
ignores `TD_Delete` and the transaction date. `Existing` subtracts principal only (`uc_LoanOnTermDeposit.cs` 397-400).*

Should penal pending on the RD (spec 14) reduce `received`? See §15.4. Proposed: **no**. The penal is collected on the next
receipt or on refund.

### 6.2 Rate of interest

```
RdRoiMax = max contract RateOfInterest of the selected RDs
Margin   = loan_roi_template.Roi where Scheme_Id = scheme, Agency 'S', not deleted, BrCode = branch, latest Wef ≤ business day
LoanRoi  = RdRoiMax + Margin
LoanPi   = 0
```

No margin row → "Rate of interest margin for Loan on RD is not defined"; Initiate is blocked.

*TSIS RD mode uses `max(RD ROI) + 2`, hard-coded (`uc_LoanOnTermDeposit.cs:363`). It still reads
`GetLoanROIAndPIFromTemplate(schemeId, "S", TrnDate)` and aborts if no template row exists, then ignores the value. FD mode adds
the template value (`FrmLoanROITemplate_TD` maintains it for loan types 3 and 4).*

Seeding the margin at 2 (§2.2) gives the TSIS result and makes it editable. See §15.2.

### 6.3 Loan due date

`DueDate = max MaturityDate of the selected RDs`, stored in `loan_master.Last_DueDate`. It is informational, but it also caps
interest in spec 18.

*TSIS stores no due date. `San_date = FirstInt_DueDate = FirstPrl_DueDate = TrnDate`; `Prl_Prd = Int_Prd = 0`. Spec 18 caps
interest at the latest maturity of the liened RDs (`GetMaxMaturityDate`).*

Is there a minimum remaining period to maturity, or a minimum number of instalments paid, before an RD can be pledged? See
§15.5. Proposed: **none**. It is already enforced in effect: an RD with nothing received has drawing power 0.

### 6.4 Worked example

Scheme 40001:

- instalment 500, 12 months, value date 14-Nov-2026;
- contract ROI 8, maturity 14-Nov-2027 (spec 15 §6.4);
- 7 instalments received (3,500);
- second RD: instalment 1,000, 24 months, ROI 8.5, maturity 14-Nov-2028, 7 instalments received (7,000).

Elig 85 %, margin 2. Loan on 20-Jun-2027:

| | RD 1 | RD 2 | Total |
|---|---|---|---|
| Received | 3,500 | 7,000 | 10,500 |
| Drawing power | 2,975 | 5,950 | **8,925** |
| Existing | | | 0 |
| Eligible | | | **8,925** |
| ROI | 8 | 8.5 | 8.5 + 2 = **10.5** |
| Due date | | | **14-Nov-2028** |

A loan of 8,000 → `loan_master.san_amt` 8,000, `securityfacevalue` 10,500, `drawingpower` 8,925, `roi` 10.5.

## 7. (reserved — numbering kept in line with spec 16)

## 8. Validation (on Initiate, and again on approval)

| # | Rule | Message |
|---|---|---|
| 1 | ECS branch; business day open (§9) | as spec 16 |
| 2 | Scheme set up: exactly one live `Loan_Type 4` scheme, ledgers set (§2.2) | "Loan on RD scheme is not set up" |
| 3 | Eligibility % and ROI margin rows exist (§6.1, §6.2) | §6 messages |
| 4 | At least one RD selected | "Select the recurring deposit(s) to pledge" |
| 5 | Every selected RD is open, of this branch, the member is a holder, and nothing has been paid out (§4.1) | "RD No X is not open" |
| 6 | Received > 0 on every selected RD | "RD No X has no amount received" |
| 7 | No lien on a selected RD: `LienMarked = false` and no `lien_trn` row with `LienTr_Delete = false`, `LienTr_Closed = false` (§15.3) | "RD No X is already pledged for loan No Y" |
| 8 | RD not in another pending (I/M) acc 2, acc 10 or acc 14 staging line | "RD No X is in another pending transaction" |
| 9 | RD not in a posted demand whose office recovery is not yet saved (spec 16 §8.8; the demand counts an RD instalment, and the lien would block a refund mid-recovery) | "RD No X is in the demand of <due date>, not yet recovered" |
| 10 | 0 < loan amount ≤ eligible amount; whole rupees | "Loan amount exceeds the eligible amount of ₹N" / "Enter the loan amount" |
| 11 | Member status: `memberstatus = 1`, not expired, not deleted | "Member is not active" |
| 12 | Only one acc 14 line per staging voucher | "Only one loan on RD can be made in a voucher" |

*TSIS checks only:*

- *loan no present;*
- *scheme selected;*
- *grid not empty;*
- *borrowing power present;*
- *ROI present.*

*It never checks the member's status or a pending refund.*

## 9. Concurrency, business day and approval recheck

- **Business day.** Load, Initiate and approval refuse unless the token's `CurrentDate` = the branch's open business day = the
  staging date (`GetOpenBusinessDayAsync`, as spec 14).
- **ECS gate.** `RdLoanController` refuses with 403 unless `Gen_Bank_Name.Bank_Type` = 3 (`IsEcsAsync`). `BuildPostingAsync`
  checks it again.
- **Locks.** An RD in a pending acc 14 line cannot be pledged again, and cannot be refunded under acc 10.
  - `EcsRdRefundHandler` must also deserialise pending acc 14 JSON (today it locks on acc 2 / acc 10 only).
  - An RD in a pending acc 10 line cannot be pledged.
  - A pending acc 2 receipt on the RD also blocks the loan (§8.8), because the received amount would change under it.
  - The approving staging excludes itself.
- **One line per voucher.** `TransactionsHandler` counts `case 14` lines like `rdRefundLines` / `rdCreateLines`.
- **Recheck on approval.** `BuildPostingAsync` re-reads every RD and recomputes §6. It compares received, drawing power,
  borrowing power, existing, eligible, ROI and due date with the staged figures (tolerance 0.005).
  - Any difference, a new lien, or a closed RD throws `EcsRecoveryException` naming the RD and the figure.
  - The Checker sees the message, and the Maker re-initiates.
  - The loan amount is unchanged and must still be ≤ eligible.
- **Staged amount.** `Payment_Amount` of the staging line must equal `LoanAmount`; `Receipt_Amount` must be 0.

## 10. DTOs

`Infin8.Coapp.Dto/EcsRdLoanVM.cs`:

```csharp
public class DtoEcsRdLoan
{
    public decimal MemId { get; set; }
    public string? MemberNo { get; set; }
    public string? MemberName { get; set; }
    public DateTime ApplicationDate { get; set; }      // business day = sanction / disbursement date
    public int CashOrAdjustment { get; set; }          // 1 cash, >1 adjustment

    public int Scheme_Id { get; set; }
    public string? SchemeName { get; set; }
    public string? ProvisionalLoanNo { get; set; }     // display only; allocated at approval
    public string? LoanNo { get; set; }                // filled by the View after approval, if read back

    public List<DtoEcsRdLoanLine> Lines { get; set; } = new();   // selected RDs only
    public double EligPer { get; set; }
    public double TotalReceived { get; set; }
    public double BorrowingPower { get; set; }
    public double ExistingOutstanding { get; set; }
    public double EligibleAmount { get; set; }
    public double RdRoiMax { get; set; }
    public double Margin { get; set; }
    public double LoanRoi { get; set; }
    public DateTime DueDate { get; set; }
    public double LoanAmount { get; set; }             // = staging Payment_Amount
}

public class DtoEcsRdLoanLine
{
    public decimal TD_Id { get; set; }
    public string? TD_No { get; set; }
    public int TDScheme_Id { get; set; }
    public double Instalment { get; set; }
    public DateTime ValueDate { get; set; }
    public int PeriodInMonths { get; set; }
    public double RateOfInterest { get; set; }
    public DateTime MaturityDate { get; set; }
    public double MaturityAmount { get; set; }
    public double Received { get; set; }
    public double DrawingPower { get; set; }
    public string? BlockReason { get; set; }           // null = selectable
    public bool Selected { get; set; }
}
```

## 11. Posting on approval (`TransactionsHandler.SaveTransaction`, `case 14`)

`BuildPostingAsync(dto, stagingId, transactedDate, brCode, yrId, checkedBy, vocId, cashOrAdjustment, status)` returns
`EcsRdLoanPosting { LoanMaster, LoanTrn, LoanDisb, LoanRoi, Lien, LienTrns, VoucherTrns, LienTdIds }`.

- The loan, lien and RD-flag rows are added through `EcsRdLoanRepository.AddLoanAsync`. It has its own in-memory MAX+1 for
  `Loan_Id`, `LoanTrn_Id`/`Trn_SlNo`, `Disb_Id`, `Roi_Id`, `Lien_Id` and `LienTr_Id`, floored at `brcode + "0000000"` as
  `EcsLoanDisbursementRepository.CreateIdAllocatorAsync` does (174-196). It does not save.
- The voucher line goes into `SaveTransaction`'s `finVoucherTrns`.
- The shared adders (`AddLoanMasterAsync`, `AddLienAsync`, …) are **not** used. They save per row and collide when one
  transaction adds several rows to a table (spec 10 §13.2).
- Entities are built with object initialisers, not `GetModalObject` (spec 10 §13.2).

### 11.1 `loan_master`

| Column | Value | *TSIS (`UpdateLoanOnTDObject`)* |
|---|---|---|
| `Scheme_Id` / `Loan_Type` | scheme 6 / 4 | same |
| `Loan_No` | §5, allocated here | *typed / Max+1 at Initiate* |
| `Mem_Id` | member | same |
| `San_Amt` | loan amount | same |
| `San_Date`, `Res_Date` | business day / null | *same / null* |
| `Loan_Status` | 1 | same |
| `Pur_Id`, `SubGrp_Id`, `Grp_Id`, `Ags_Id` | 1, 0, 0, 0 (as spec 10) | *0* |
| `Prl_Prd`, `Int_Prd`, `Diff_Prd`, `Grace_Prd`, `Inst_Amt`, `Inst_Date` | 0 | same |
| `FirstInt_DueDate`, `FirstPrl_DueDate` | business day | same |
| `Last_DueDate` | §6.3 | *null* |
| `Roi` / `Pi` | §6.2 / 0 | *max RD ROI + 2 / 0* |
| `LastIntApplication_Date` | business day | *null* |
| `SecurityFaceValue` | total received | same |
| `DrawingPower` | borrowing power | same |
| `SecurityType` | 0 (§15.6) | *0* |
| `Loan_OE`, `Loan_Delete`, `IsAccountClosed` | false | same |
| `Voc_Id`, `Usr_Id` (checker), `Yr_Id`, `BrCode`, `Voc_Status 'V'` | | |

No `loan_members` row, as in spec 10 and TSIS.

### 11.2 `loan_trn`, `loan_disb`, `loan_roi`

| Table | Values |
|---|---|
| `loan_trn` | `Trn_Status 'I'`; `Trn_Date` = `Disb_Date` = business day; `Disb_Amt` = loan amount; `IntCalc_Date` = business day, `IntCalc_Amt` 0; `Soc_Roi` = ROI; `SocPI_Rate` 0; `Demand_Id` = zero id; `Trn_SlNo` 1; all else 0 |
| `loan_disb` | `SocDisb_No` 1; `SocDisb_Amt` = amount; `Disb_Date` = `SocDisb_Date` = business day; `Is_FinalDisbursement` true; `Disb_SlNo` 1 |
| `loan_roi` | `Agency 'S'`; `Roi_Wef` = business day; `Roi` = §6.2; `Pi` 0; `Prd` 0 |

### 11.3 `lien` and `lien_trn`

| Table | Column | Value | *TSIS* |
|---|---|---|---|
| `lien` (one per loan) | `Loan_Id` | the new loan | same |
| | `TD_Id` | the **first** selected RD | ***0*** |
| | `TD_Amount` | total received | same |
| | `Lien_Amount` | loan amount | same |
| | `Lien_Recovered` | 0 | same |
| | `SecurityDescription` | `"RD Nos <no> <no> …"`, cut to 100 characters | same, uncut |
| | `Lien_Closed`, `Lien_OE`, `Lien_Delete` | false | same |
| `lien_trn` (one per RD) | `Lien_Id`, `Loan_Id`, `TD_Id` | | same |
| | `TDTr_Amount` | received on the RD | same |
| | `DrawingPower` | the RD's drawing power | same |
| | `LienTr_Amount` | loan amount × this RD's drawing power ÷ borrowing power, rounded; the last RD takes the remainder | ***0*** |
| | `LienTr_Recovered` | 0 | same |
| | `LienTr_Closed`, `LienTr_OE`, `LienTr_Delete` | false | same |

Both tables also get `Voc_Id`, `Usr_Id`, `Yr_Id`, `BrCode` and `Voc_Status 'V'`.

`lien_id` / `lientr_id` have DB sequences, but they are allocated by MAX+1 to match the rest of the data (CLAUDE.md).

### 11.4 `termdeposit_master`

For each pledged RD, set `LienMarked = true` on the tracked row (BrCode-filtered) without saving, as
`EcsRdRefundRepository.MarkClosedAsync` does. `Loan_Id` is left unchanged (§15.7).

*The live TSIS path **never sets `LienMarked`**. Only the opening-entry path (`DatabaseLoan.cs:6762`) does. Spec 16 refuses a
refund on either `LienMarked` or an open `lien_trn`, so both are kept in step here.*

### 11.5 `fin_voucher_tr`

| Line | `led_id` | Dr (`voc_pmt`) | Cr (`voc_rpt`) | `voc_Trn_Type` |
|---|---|---|---|---|
| Loan | scheme `PrlLed_Id` (`110050000010` LOAN ON RD) | loan amount | | `Cash_Or_Adjustment` |

- `Status` = the account's `acc_status` (`'RDL'`), falling back to `'RDL'`.
- `loan_id` = the new loan. *TSIS leaves it 0.*
- `voc_narr` = `"<memberno> <name> Loan No <Loan_No>"`; `Description` = `"Loan No <Loan_No>"`; `Mem_Id` = the member.
- In cash mode, `SaveTransaction`'s normal flow credits cash with the line's cash payment. In adjustment mode, the
  counter-entry comes from the rest of the voucher.

*TSIS takes the ledger from `Map_Fin.RD_Loan_Led_Id` (`AccountsService.GetRDLoanLedId`, `DatabaseAccount.cs:279`). ECS has no
`Map_Fin`, and `map_general` has only `rd_led_id` / `rd_int_led_id` / `rd_pi_led_id` (deposit side), so the scheme's ledgers
are used.*

## 12. View component and delete

**View (`RecurringDeposit_LoanDisbursement_View`)** reads the staged `DtoEcsRdLoan`. It shows:

- the member;
- the pledged RDs with every §4.2 column;
- the §4.4 figures.

It has no `@rendermode` of its own and no ECS gate, like `RecurringDeposit_Refund_View`.

**Delete before approval:** the normal staging delete. Nothing was posted, and the lock (§9) lifts.

**Delete after approval:** none. The app has no delete for an approved Member Transaction voucher (spec 14 §17.1). When one is
built, mirror *TSIS `case "RDL"` (`DatabaseEmployee.cs:3411-3440`)*:

- soft-delete `loan_master`, `loan_disb`, `loan_trn`, `loan_roi`, `lien` and `lien_trn` by `Voc_Id`;
- set `LienMarked = false`;
- refuse if any later valid `loan_trn` exists for the loan.

## 13. TSIS behaviours not carried over

| # | TSIS behaviour | This spec |
|---|---|---|
| 1 | Two controls, one of them dead (`uc_LoanOnRecurringDeposit`, eligibility on the instalment) | the live `uc_LoanOnTermDeposit` RD path is ported |
| 2 | ROI = max RD ROI + 2, hard-coded; the template is read and ignored | §6.2: margin from `loan_roi_template` |
| 3 | Eligibility % = latest row overall, `TD_Delete` ignored | §6.1: latest `wef ≤` business day, not deleted, per branch |
| 4 | Loan No typed or fixed at Initiate; `Max(Loan_No)` without delete/branch filter | §5: allocated at approval |
| 5 | Amount above eligible silently clamped | §8.10: refused |
| 6 | `LienMarked` not set on disbursement | §11.4: set |
| 7 | `Lien.TD_Id = 0`, `LienTr_Amount = 0` | §11.3: first RD / pro-rata share |
| 8 | Voucher `Loan_Id = 0` | §11.5: the loan |
| 9 | Ledger from `Map_Fin`, 0 if unset | §11.5: scheme `PrlLed_Id`, refused when unset |
| 10 | RDs with no receipt silently dropped; no `BrCode` filter; ids concatenated into SQL | §4.1 |
| 11 | Further loans on already-liened RDs allowed (existing principal deducted) | §8.7: refused (§15.3) |
| 12 | No member-status, pending-refund or demand check | §8 |
| 13 | Message box shows `errorMessage` instead of `combinedErrorMessage` (`uc_LoanOnTermDeposit.cs:352`) | — |
| 14 | `Lien_Tr.LienTr_Id = LienId` in `GetLienTrObject` (`TsisShared/Utilities.cs:1296`) | ids by allocator |

## 14. Decisions proposed

1. **Maker–checker** through staging with an approval recheck (spec 11/14/15/16 pattern).
2. **Scheme 6 becomes the RD-loan scheme**: `Loan_Type 4`, ledgers `…010` / `…053`, no penal (§2.2). It is kept out of salary
   demand.
3. **Eligibility** = 85 % (template `TD_Type 2`) of the amount received, rounded to a rupee.
4. **ROI** = highest contract ROI of the pledged RDs + template margin (seeded at 2).
5. **One open loan per RD**: a liened RD cannot be pledged again.
6. **Several RDs per loan, one loan per line**, one acc 14 line per voucher.
7. **Lien rows plus `LienMarked`** on every pledged RD.
8. **Own id allocator** for the loan and lien rows; loan number at approval.
9. **No schema change.** Data: the §2.2 scheme update and margin row on every ECS database.
10. **ECS only**, gated on the server and the client.

## 15. Open points for the reviewer

1. **Scheme.** Fix scheme 6 as §2.2? Scheme 5 "LOAN ON FIXED DEPOSIT OVERDRAFT" (`loan_type 3`) also points at `…010` / `…053`.
   Should it be corrected to its own FD-overdraft ledgers, or deleted? Two type-3 schemes also break `GetLoanSchemeByType(3)` for
   acc 13.
2. **Margin.** Is 2 % over the highest RD rate right? Over the highest, or a weighted rate of the pledged RDs?
3. **Top-up.** One open loan per RD (proposed), or allow a further loan up to the remaining drawing power, as TSIS does?
4. **Penal pending.** Should unpaid RD penal reduce the received amount used for eligibility? Proposed: no.
5. **Minimum.** Is there a minimum number of instalments, a minimum loan amount, or a minimum time to maturity (e.g. no loan
   in the last 3 months)? Proposed: none.
6. **`SecurityType`.** Is there a code for "deposit lien"? Proposed: 0.
7. **`termdeposit_master.Loan_Id`.** Set it to the loan (one loan per RD makes that possible) or leave it 0 as TSIS? Proposed:
   leave 0, because the lien rows are the link.
8. **Refund of a pledged RD.** Spec 16 refuses it. Keep that (the member repays through acc 25 first, possibly in the same
   voucher by adjustment), or build loan recovery into the refund as TSIS does?
9. **Joint holders.** Any holder may borrow (proposed, same RD list as spec 14/16), or only the first holder?
10. **Test data.** Branch 11005 has no RD yet. Create them with spec 15, with receipts under spec 14 (backdated through Day
    Begin), before testing.

## 16. Server side (vertical slice per CLAUDE.md)

| Layer | Item |
|---|---|
| Utility | `EcsRdLoanCalculator.cs` (pure: §6.1–§6.3, the lien share §11.3, the §8 amount rules) |
| Dto | `EcsRdLoanVM.cs` (§10) |
| Repository | `EcsRdLoanRepository` + interface. Reads: scheme by `Loan_Type 4`, eligibility row, margin row, the RD list (reuse `EcsRdReceiptRepository.GetEligibleRdsAsync`), lien check, existing outstanding, pending acc 2/10/14 staging JSON, posted-demand check, member status. Writes: `AddLoanAsync` (allocator, no save) and `MarkLienAsync`. Query syntax, `BrCode` on every alias. |
| UnitOfWork | interface property, backing field, lazy property |
| BusinessLogic | `EcsRdLoanHandler` + interface: `GetSetupAsync(memId, brCode, date)`, `BuildPostingAsync(...)`. `EcsRdRefundHandler` also locks RDs in pending acc 14 lines. |
| Program.cs | `AddScoped<IEcsRdLoanHandler, EcsRdLoanHandler>()` |
| TransactionsHandler | inject `IEcsRdLoanHandler`; fill `case 14`; one-line-per-voucher counter |
| Controller | `RdLoanController`: `GET api/rdloan/Setup/{memId}`, BrCode and date from claims, ECS-gated |
| Client | `Components/TermDeposit/RecurringDeposit_LoanDisbursement.razor`, `RecurringDeposit_LoanDisbursement_View.razor`; the switch cases in §2; `RDLoanInitiate` / `CloseRDLoanComponent` in the host; live `case 14` in `Checkerboard.razor` |

## 17. Verification (manual — the project has no tests)

Run this in ECS branch 11005:

- first run the §2.2 SQL;
- open RDs with spec 15 and receive instalments with spec 14;
- initiate and approve;
- then check `loan_master`, `loan_trn`, `loan_disb`, `loan_roi`, `lien`, `lien_trn`, `termdeposit_master.lienmarked` and
  `fin_voucher_tr` against a hand calculation.

| # | Case | Expect |
|---|---|---|
| 1 | §6.4, loan 8,000, cash | loan type 4, scheme 6, no. `110054010000001`, roi 10.5, drawing power 8,925, face value 10,500, last due 14-Nov-2028; trn `I` 8,000; one lien, two lien_trn (2,667 / 5,333); both RDs `lienmarked`; Dr `110050000010` 8,000, cash Cr 8,000 |
| 2 | Loan 9,000 | refused: exceeds eligible 8,925 |
| 3 | Adjustment | loan line type = `Cash_Or_Adjustment`; no cash line |
| 4 | RD already liened | checkbox disabled, with the loan no. |
| 5 | RD in a pending acc 2 / acc 10 line | not selectable; approval refused |
| 6 | Pending acc 14 on the RD, then try acc 10 | refund refused (lock) |
| 7 | An RD receipt (acc 2) approved between initiate and approve | approval refused: figures changed |
| 8 | Day End between initiate and approve | refused: business day |
| 9 | Scheme 6 not yet fixed / no margin row | "not set up" / "not defined"; Initiate blocked |
| 10 | Two acc 14 lines in a voucher | second refused |
| 11 | PCARDB branch | API 403; component shows "ECS only" |
| 12 | After approval: RD Refund (acc 10) on a pledged RD | refused (spec 16 §8.3) |
| 13 | After approval: demand calculation | the loan does not appear (type 4) |
| 14 | Checkerboard and Member Transaction line view | the View shows the RDs and figures |
