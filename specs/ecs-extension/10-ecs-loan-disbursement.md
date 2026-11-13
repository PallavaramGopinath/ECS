# 10 — ECS Loan Disbursement (`ECSLoanDisbursement`)

> Builds on [06-ecs-loan-sanction.md](./06-ecs-loan-sanction.md),
> [08-ecs-loan-sanction-modifications.md](./08-ecs-loan-sanction-modifications.md) and
> [09-ecs-loan-sanction-list.md](./09-ecs-loan-sanction-list.md). This is the "ECS Loan Disbursement
> (separate spec, to follow)" named in spec 06 §1 and §3.14.
>
> **Status:** implemented (see §13 for the decisions taken on §10 and where the code deviates from §6).
>
> **Evidence.** Every table and code value below was read from the local `ECS` database (branch
> `11005`), mainly by tracing how TSIS posted the 2,664 batch disbursements already in it (e.g.
> voucher `110050009736`, 23-10-2026: 13 sanctions, one cheque, one voucher).

---

## 1. Purpose and scope

A page that lists ECS loan sanctions **awaiting disbursement** (`loan_sanction.loansanction_status =
'E'`, not deleted, current branch), lets the user tick **one or more** of them, choose **Cheque** or
**NEFT**, and disburse them in one step. Disbursing:

1. **Opens each new loan** — `loan_master`, `loan_disb`, `loan_inst`, `loan_roi`, `loan_trn` (issue row).
2. **Recovers existing loans** from the loan amount — `loan_trn` (receipt row); closes a loan that is
   repaid in full.
3. **Receives share capital** shortfall of the member and the surety — `mem_trn`.
4. **Receives pending deposits** — `deposit_trn`.
5. **Posts the books** — one `fin_voucher` with its `fin_voucher_trn` lines for every item above and
   every other recoverable (entrance fee, forms, sundry, general ledger…).
6. **Records the bank payment** — `fin_voucher_bank`.
7. **Closes the sanction** — `loan_sanction.loan_id`, `voc_id`, status `E` → `C`.

All of it is one database transaction: either every ticked sanction is disbursed or none is.

**Out of scope:** cash disbursement; partial / multi-tranche disbursement (`loan_disb` gets one, final,
row); reversing a disbursement; printed cheque / NEFT bank file; RDLC reports.

---

## 2. Decisions already taken

| # | Decision | Source |
|---|---|---|
| D1 | **One combined voucher per disbursement run**, however many sanctions are ticked. Sanctions move to status **`C`**. | Business, matches TSIS batch disbursements (`voc_type 14`, status `C`) |
| D2 | **Bank rows (`fin_voucher_bank`) are one per member**, carrying that member's net amount. **Single disbursement:** usually by cheque — the cheque no. is entered for that row. **Multiple disbursements:** one instrument (usually **NEFT**, sometimes one cheque) whose number/reference is shared by every member row. | Business |
| D3 | An existing loan whose **principal outstanding becomes 0** after the recovery is **closed** (`isaccountclosed = true`, `accountcloseddate` = business day). TSIS never did this. | Business |
| D4 | The disbursement date is the **business day** (`CurrentDate` from claims), never `DateTime.Now`. | `CLAUDE.md` |

---

## 3. What the data looks like today (TSIS conventions to keep)

### 3.1 Status and voucher pattern

| `loan_sanction` status | Meaning | Voucher | Count |
|---|---|---|---|
| `E` | Sanctioned, awaiting disbursement | none (`voc_id = 0`, `loan_id = 0`) | 1 live |
| `S` | Disbursed **singly** — one voucher per loan | `voc_type 1`, modes `AR`/`VR` (adjustment) or `QR`/`PQ` (cheque) | 2,589 |
| `C` | Disbursed **in a batch** — one voucher, many loans | `voc_type 14`, modes `QR`/`PQ` | 2,664 sanctions / 237 vouchers |

This page always produces the `C` pattern (D1).

### 3.2 Code values used by the posting

| Column | Value | Meaning |
|---|---|---|
| `loan_trn.trn_status` | `I` | Issue (disbursement) — `disb_amt`, `disb_date`, `soc_roi`, `socpi_rate` |
| | `R` | Receipt (recovery) — `prlcoll_amt`, `intcoll_amt`, `picoll_amt` |
| | `D` | Demand (raised by month-end demand, not by this page) |
| `loan_master.loan_status` | `1` | Always 1; closure is `isaccountclosed` / `accountcloseddate` |
| `loan_roi.agency` | `S` | Society rate |
| `mem_trn.trn_type` | `3` | Share capital (ledger `SHARE CAPITAL`, `110050000024` = `map_general.sharecapital_led_id`) |
| `fin_voucher.voc_type` | `14` | ECS batch loan disbursement |
| `fin_voucher.voc_rpt_mode` / `voc_pmt_mode` | `QR` / `PQ` | Cheque |
| | `AR` / `VR` | Adjustment — used for NEFT (as TSIS NEFT vouchers, e.g. `voc_type 71`) |
| `fin_voucher.voc_rpt_no` / `voc_pmt_no` | `AR/n` / `VR/n` | Next number per `yr_id` (and `brcode`, see §8.3) |
| `fin_voucher.voc_no` | `VR/n-AR/m` | Payment no., hyphen, receipt no. — the format in the data |
| `fin_voucher_trn.voc_trn_type` | `2` | Adjustment / transfer (every line; no cash) |
| `fin_voucher_trn.status` | `LA` | Loan advanced (the new loan's debit line) |
| | `P` / `I` / `PI` | Principal / interest / penal recovered |
| | `MS` / `SS` | Share capital — member / surety |
| | `D` | Deposit receipt |
| | `F`, `G`, `DTo`, `DBy` | Fee, general ledger, sundry due-to, due-by |
| | `Q` / `NEFT` | Bank credit line — cheque / NEFT |
| `fin_voucher_bank.fvb_bank_mode` | `C` | Cheque |
| | `O` | Online (NEFT / RTGS) |

- `voc_rpt` is the **credit** side and `voc_pmt` the **debit** side. A loan disbursed is `voc_pmt`;
  everything recovered and the bank credit are `voc_rpt`. Σ`voc_rpt` = Σ`voc_pmt` = `fin_voucher.voc_amt`
  = Σ loan amounts.
- **"Zero" ids.** Unused id columns hold `brcode + 0000000` (e.g. `110050000000`), not `0`
  (`fin_voucher_trn.loan_id`, `loan_trn.demand_id`, `fin_voucher_bank.fvb_return_voc_id`). Follow it.
- Narrations: `"{PerNo} {Name} Loan No {loan_no}"` on loan lines, `"{PerNo} {Name}"` on others.

### 3.3 The source: what a sanction gives us

`loan_sanction` (header) — `loansanction_id`, `mem_id`, `suretymem_id`, `scheme_id`, `inst_type`,
`sanctiondate`, `loanamount`, `periodofloan`, `rateofinterest`, `penalrate`, `instalmentamount`,
`firstduedate`, `totalreceiptamount`, `membersharecapitalamount`, `suretysharecapitalamount`,
`loanelig_id`, `loan_id` (0 until disbursed), `voc_id` (0 until disbursed).

`loan_sanction_trn` (lines) — one per recoverable with `receiptitem > 0`: `status`, `loan_id`,
`recscheme_id`, `dm_id`, `mem_id`, `led_id`, `receiptdescription`, `receiptitem` (amount),
`loan_slno` (snapshot of the loan's last `trn_slno`). Lines with `receiptitem = 0` are the payment
padding rows of spec 06 §3.12 and are ignored here.

**Net disbursable** = `loanamount − totalreceiptamount`.

---

## 4. The page — `Pages/Products/ECSLoanDisbursement.razor`

```razor
@page "/ecs-loan-disbursement"
@rendermode InteractiveWebAssembly
@attribute [Authorize(Roles = "Admin,Maker,Checker")]
@inject HttpClient _httpClient
@inject AuthenticationStateProvider AuthenticationStateProvider
@inject IJSRuntime JSRuntime
```

- **ECS gate** with the `isReady` guard, exactly as `ECSLoanSanction.razor`.
- `brCode`, `YrId`, `CurrentDate` from `Utilities.GetUserInfoDtoFromASP`. Bootstrap 5.3 + Icons only.

### 4.1 Layout

```
ECS Loan Disbursement                                     Business day: 23-11-2026
──────────────────────────────────────────────────────────────────────────────────────────────
Filter: (•) All ( ) Sanction Id [__] ( ) Member No [__] ( ) Date [from]–[to]          [Search]
──────────────────────────────────────────────────────────────────────────────────────────────
[☐] | Sl | Sanction Id | Date | Member No | Name | Surety | Scheme | Loan Amt | Recoverable | Net    | [View]
[☑] |  1 | 1045 | 23-10-2026 | 002960 | …   | 002313 | Surety | 12,00,000 | 6,73,490 | 5,26,510 | [View]
──────────────────────────────────────────────────────────────────────────────────────────────
Selected: 3 sanctions   Loan Amt Σ   Recoverable Σ   Net payable Σ
──────────────────────────────────────────────────────────────────────────────────────────────
Payment   Mode (•) Cheque ( ) NEFT     Bank ledger [CURRENT ACCOUNT WITH KCCB ▼]
          Cheque No / NEFT Ref [______]   Date [23-11-2026]   Bank name (NEFT) [____]
          Narration [ECS Loan Disbursement ______________________]
                                                           [Preview Voucher]  [Disburse]
```

- **Grid.** Reuse `POST api/loansanction/ListSanctions` (status `E` filter already in it). Header
  checkbox = select all visible. Amounts right-aligned, Indian grouping, `dd-MM-yyyy`.
- **View** opens the read-only sanction modal from spec 09 §3.2 (shared component).
- **Payment.**
  - *Mode* defaults to **Cheque** when one row is ticked and **NEFT** when more than one (D2); the
    user can change it.
  - *Bank ledger*: bank ledgers of the branch (`_unitOfWork.Accounts.GetBankLedgerIds`), default the
    first (today `CURRENT ACCOUNT WITH KCCB`).
  - *Cheque No / NEFT Ref*: required. One value for the run, copied to every member's bank row.
  - *Date*: cheque date / NEFT value date, default business day, not after the business day.
- **Preview Voucher** calls `POST Preview` (§6) and shows a modal with the voucher lines (Ledger,
  Member, Narration, Debit, Credit, totals, "balanced ✓"), the bank rows (member, net amount,
  instrument) and any blocking messages. Nothing is saved.
- **Disburse** is enabled only after a clean preview of the *same* selection and payment fields.
  Confirm: *"Disburse {n} loans, Σ Rs.{loan} (net Rs.{net}) by {mode} {ref}? This cannot be undone
  from the screen."* Then `POST Disburse`.
- **Result.** Alert with the voucher no. (`VR/n-AR/m`) and a table Sanction Id → Loan No → Net paid.
  The grid reloads (disbursed rows drop out). **Print Voucher** prints the preview markup as HTML via
  `printElement` (`wwwroot/js/pdfViewer.js`) — no RDLC, so it works on any host.
- All buttons disabled while a request is in flight.

---

## 5. Posting rules (per run)

Let *run* = the ticked sanctions, *date* = business day, *V* = the new `voc_id`.

### 5.1 Voucher header — `fin_voucher` (one row)

| Column | Value |
|---|---|
| `voc_id` | MAX+1 |
| `voc_date` | date |
| `voc_type` | `14` |
| `voc_rpt_slno` / `voc_rpt_no` / `voc_rpt_mode` | next receipt no. → `AR/n`; mode `QR` (cheque) or `AR` (NEFT) |
| `voc_pmt_slno` / `voc_pmt_no` / `voc_pmt_mode` | next payment no. → `VR/n`; mode `PQ` (cheque) or `VR` (NEFT) |
| `voc_no` | `voc_pmt_no + "-" + voc_rpt_no` |
| `voc_slno` | 0 |
| `voc_amt` | Σ `loanamount` of the run |
| `voc_narration` | page narration |
| `mem_id` | the member when the run has one sanction, else the zero id |
| `voc_bus` | true |
| `voc_delete`, `voc_status`, `usr_id`, `yr_id`, `brcode` | false, `V`, claims |

### 5.2 Per sanction — the new loan

| Table | Row | Key values |
|---|---|---|
| `loan_master` | 1 | `loan_id` MAX+1; `loan_no` = `GetNewLoanNo(scheme_id)` (scheme's highest + 1, first = `loannostartwith`…); `scheme_id`; `mem_id`; `loan_type` = scheme's (1 for ECS); `loan_status 1`; `san_amt` = `loanamount`; `san_date` = `res_date` = `sanctiondate`; `prl_prd` = `periodofloan`; `int_prd 0`; `firstprl_duedate` = `firstint_duedate` = `firstduedate`; `roi` / `pi` = sanction rates; `inst_amt` = `instalmentamount`; `lastintapplication_date` = date; `loan_oe/loan_delete/isaccountclosed` false; `voc_id` V |
| `loan_disb` | 1 | `disb_id` MAX+1; `disb_date` = `socdisb_date` = date; `socdisb_no 1`; `socdisb_amt` = `loanamount`; `socdisbcheque_no` / `socdisbcheque_date` = instrument no. / date; `disb_slno 1`; `is_finaldisbursement true`; `reim_amt 0`; `voc_id` V |
| `loan_inst` | 1 | `inst_id` MAX+1; `inst_wef` = date; `inst_amt` = `instalmentamount`; `voc_id` V |
| `loan_roi` | 1 | `roi_id` MAX+1; `agency 'S'`; `roi_wef` = date; `roi` / `pi` = sanction rates; `prd 0`; `voc_id` V |
| `loan_trn` | 1 (`I`) | `trn_id` MAX+1; `trn_status 'I'`; `trn_date` = `disb_date` = date; `disb_amt` = `loanamount`; `soc_roi` / `socpi_rate`; `trn_slno 1`; `demand_id` zero id; `voc_id` V |
| `fin_voucher_trn` | 1 | scheme `prlled_id`; `voc_pmt` = `loanamount`; status `LA`; `mem_id`; `loan_id` = new loan |

Every row also gets `usr_id`, `yr_id`, `brcode` from claims and `voc_status 'V'`.

### 5.3 Per sanction — each recoverable line (`loan_sanction_trn`, `receiptitem > 0`)

| Line status | Detail table | `fin_voucher_trn` (credit, `voc_rpt` = amount) |
|---|---|---|
| `P` / `I` / `PI` | **`loan_trn`**, one `R` row **per recovered loan** (P, I and PI of the same `loan_id` merged): `prlcoll_amt` = P, `intcoll_amt` = I, `picoll_amt` = PI, `trn_date` = date, `trn_slno` = that loan's MAX+1, `voc_id` V. Then **close** the loan if principal outstanding is now 0 (D3). | line's `led_id` (scheme `prlled_id` / `intled_id` / `piled_id`), status `P`/`I`/`PI`, `mem_id`, `loan_id` = recovered loan, narration with its loan no. |
| `MS` / `SS` | **`mem_trn`**: `trn_type 3`, `mem_id` = line's `mem_id` (member for MS, **surety** for SS), `led_id` = share-capital ledger, `rpt_amt` = amount, `trn_date` = date, `trn_slno` = MAX+1 for (mem, type, ledger), `voc_id` V | share-capital ledger, status `MS`/`SS`, `mem_id` = member / surety |
| `D` | **`deposit_trn`**: `deposit_id` = line's `dm_id`, `mem_id`, `receipt_amount` = amount, `trn_date` = date, `demand_id` = the oldest unpaid demand of that deposit (zero id if none), `trn_slno` = MAX+1 for (mem, deposit), `is_active true`, `is_opening_entry false`, `voc_id` V | deposit's `led_id` (`deposit_masters.led_id`), status `D` |
| `DTo` / `DBy` | **`mem_trn`** (Q3): `trn_type` 1 (DTo) / 2 (DBy), `led_id` = line's sundry ledger, `rpt_amt` = amount, `trn_slno` = MAX+1 for (mem, type, ledger), `voc_id` V | line's `led_id` and status |
| `F`, `G`, any other | none | line's `led_id` and status |

### 5.4 Per run — the bank

| Table | Rows | Values |
|---|---|---|
| `fin_voucher_trn` | **1** bank credit line | bank ledger; `voc_rpt` = Σ net of the run; status `Q` (cheque) or `NEFT`; narration `"ECS Loan Disbursement Cheque No {no}"` / `"… NEFT Ref {ref}"` |
| `fin_voucher_bank` | **1 per sanction** (member) | `fvb_id` MAX+1; `fvb_date` = date; `mem_id`; `fvb_bank_mode` `C` (cheque) / `O` (NEFT); `fvb_bank_name` = bank ledger name (cheque) or the bank name typed (NEFT); `led_id` = bank ledger; `fvb_amount` = that member's net; `fvb_cheque_no` = instrument no.; `fvb_cheque_date` = instrument date; `fvb_real_amount 0`; `fvb_cheque_return false`; `fvb_return_voc_id` zero id; `voc_id` V |

### 5.5 Per sanction — close the sanction

`loan_sanction`: `loan_id` = new loan, `voc_id` = V, `loansanction_status = 'C'`.
`loan_eligibility` (by `loanelig_id`): `loan_id` = new loan, `disbursedamt` = `loanamount`.

### 5.6 Worked example (one sanction, cheque)

Sanction 1,200,000; recoverables: old loan P 573,490 + I 4,250, member share 70,000, surety share 25,000,
thrift deposit pending 750 → Σ 673,490; net 526,510.

| Ledger | Status | Debit (`voc_pmt`) | Credit (`voc_rpt`) |
|---|---|---|---|
| MEMBER SURETY LOAN (new loan) | LA | 12,00,000 | |
| MEMBER SURETY LOAN (old loan) | P | | 5,73,490 |
| INTEREST RECEIVED ON SURETY LOAN | I | | 4,250 |
| SHARE CAPITAL (member) | MS | | 70,000 |
| SHARE CAPITAL (surety) | SS | | 25,000 |
| THRIFT DEPOSIT | D | | 750 |
| CURRENT ACCOUNT WITH KCCB | Q | | 5,26,510 |
| **Total** | | **12,00,000** | **12,00,000** |

Detail rows: `loan_master`/`loan_disb`/`loan_inst`/`loan_roi`/`loan_trn I` for the new loan; `loan_trn R`
for the old loan (closed if P outstanding is now 0); 2 × `mem_trn`; 1 × `deposit_trn`; 1 ×
`fin_voucher_bank` (526,510, cheque no.); sanction → `C`.

---

## 6. Server design

New handler `EcsLoanDisbursementHandler` / `IEcsLoanDisbursementHandler`
(`Infin8.Coapp.BusinessLogic`), registered `AddScoped` in `Infin8.Coapp.UI/Program.cs`. Actions on
`LoanSanctionController` (route `api/loansanction`) so they share its `IsEcsAsync` gate and
`UserContext()`; `brCode`, `usr_id`, `yr_id`, business day are taken **from claims only**.

| Method | Endpoint | In → Out |
|---|---|---|
| `PreviewAsync` | `POST DisbursementPreview` | `EcsDisbursementRequestVM` → `EcsDisbursementPreviewVM` (voucher lines, bank rows, totals, `Messages`, `IsValid`) |
| `DisburseAsync` | `POST Disburse` | `EcsDisbursementRequestVM` → `EcsDisbursementResultVM` (`Success`, `VocId`, `VocNo`, per sanction `LoanId`/`LoanNo`/`Net`, `Reasons`) |

```csharp
public class EcsDisbursementRequestVM {
    public List<decimal> SanctionIds { get; set; } = new();
    public string Mode { get; set; } = "C";          // "C" cheque, "N" NEFT
    public decimal BankLedId { get; set; }
    public string InstrumentNo { get; set; } = "";
    public DateTime InstrumentDate { get; set; }
    public string? BankName { get; set; }            // NEFT
    public string? Narration { get; set; }
}
```

`DisburseAsync` = `PreviewAsync`'s build step + writes, in one transaction:

1. `BeginTransaction()`.
2. Re-load each sanction **inside the transaction**; build the posting (§5) in memory.
3. Validate (§7). Any message → `RollBack()`, return reasons.
4. Insert in this order (later MAX+1 reads see earlier rows): `fin_voucher` → `loan_master` →
   `loan_disb` / `loan_inst` / `loan_roi` → `loan_trn` (via `AddLoanTrnListAsync`, which sets
   `trn_slno` correctly) → `mem_trn` (`AddMemTrnListAsync`) → `deposit_trn` → `fin_voucher_trn` →
   `fin_voucher_bank` → old-loan closures → `loan_sanction` / `loan_eligibility` updates.
5. `CompleteAsync()`; `CommitTransaction()`. On exception `RollBack()` and rethrow
   `InvalidOperationException` (the `EcsLoanSanctionHandler` model — **not** `TransactionsHandler`,
   which swallows errors and returns an empty voucher).

### 6.1 Reuse

| Need | Existing code |
|---|---|
| List of `E` sanctions | `EcsLoanSanctionRepository.GetSanctionListAsync` / `ListSanctions` |
| Stale-snapshot check | `EcsLoanSanctionRepository.GetSubsequentTransactionMessagesAsync` (spec 06 §3.13) |
| New loan no. | `LoanMasterRepository.GetNewLoanNo(schemeId)`, `AddLoanMasterAsync` |
| Entity factories | `GetModalObject.GetLoanMasterObject`, `GetLoanTrnObject`, `GetLoanDisbursementObject`, `GetLoanROIObject`, `GetMemTrnObject`, `GetFinVoucherObject`, `GetFinVoucherTrObject`, `GetFinVocBankObject` (count positional args carefully) |
| Receipt / payment nos. | `TransactionsRepository.GetReceiptAndPaymentNo` / `GetTransactionNo` |
| Bank ledgers | `_unitOfWork.Accounts.GetBankLedgerIds` |
| Share-capital ledger | `map_general.sharecapital_led_id` |
| Reference write order | `TransactionsHandler.SaveTransaction` (lines ~1701–1728), `SaveLongTermDisbursement` (~1879) |
| Unused TSIS-port VMs | `LoanDisbursementByECSVM`, `LoanDisbursementByECSReturnVM`, `RecoverablesForLoanDisbursemetVM` in `Infin8.Coapp.Dto` — reuse if they fit, else delete |

### 6.2 Gaps to fill

| Gap | Fix |
|---|---|
| `DepositTrnRepository.AddDepositTrnAsync` throws `NotImplementedException` | Implement `AddDepositTrnListAsync` (MAX+1 `trn_id`, `trn_slno` per mem+deposit) and a "oldest unpaid demand" query |
| `GetLoanTrnObject` ignores its `TrnSlNo` argument | Use `AddLoanTrnListAsync` (sets it) for every `loan_trn` row |
| `AddMemTrnListAsync` takes the `trn_slno` key from the first row only | Compute `trn_slno` per (mem, type, ledger) row — needed because MS and SS rows are different members |
| `GetTransactionNo` filters by `yr_id` only | Add the `brcode` predicate (§8.3) |
| `loan_master` closure | Small repository method: set `isaccountclosed`/`accountcloseddate` when Σ`prlcoll` ≥ disbursed − (other principal receipts) i.e. principal outstanding = 0 |
| `loan_eligibility` update | Repository method by `loanelig_id` + `brcode` |

No new tables or columns. `loan_sanction.loan_id` / `voc_id` already exist.

---

## 7. Validation (server-enforced, in `PreviewAsync` and again in `DisburseAsync`)

1. At least one sanction ticked; every id exists in the claims `brcode`, is **status `E`**, not deleted
   (re-read inside the transaction, so two users cannot disburse the same sanction).
2. Business day is open (Day Begin done) and inside the session's financial year.
3. `sanctiondate` ≤ business day.
4. No subsequent transaction on any recovered loan since the sanction (spec 06 §3.13). Message:
   *"Subsequent transaction made in loan account {loan_no} — modify the sanction first."*
5. Net disbursable > 0 for each sanction; Σ`receiptitem` of its lines = `totalreceiptamount`.
6. Mode is `C` or `N`; bank ledger is a bank ledger of the branch; instrument no. not blank; instrument
   date ≤ business day. For a cheque, the cheque no. is not already used on another live voucher of the
   same bank ledger (warning, see Q5).
7. Scheme still allowed for ECS (`LoanCategoryPolicy.IsAllowed`); scheme has `prlled_id`; each
   recoverable line has a `led_id` (non-zero).
8. The built voucher balances: Σ`voc_pmt` = Σ`voc_rpt` = `voc_amt`.

---

## 8. Things to watch

1. **Ids are MAX+1** (`loan_id`, `loan_no`, `disb_id`, `inst_id`, `roi_id`, `trn_id`, `mem_trn_id`,
   deposit `trn_id`, `voc_id`, `voc_trn_id`, `fvb_id`). They are generated inside the open transaction,
   so a large batch is serialised; two users disbursing at the same moment can collide — the
   transaction must fail cleanly (unique-key error → rollback → "try again") rather than half-post.
2. **`BrCode` on every alias** of every query (no `HasQueryFilter` in the solution).
3. **Voucher numbering per branch.** `GetTransactionNo` reads `MAX(voc_rpt_slno / voc_pmt_slno)` per
   `yr_id` only; add `brcode` for this page.
4. **`CurrentDate` is frozen in the token.** If Day End/Begin happened mid-session the date is stale;
   check the `calendar` row for the branch on the server (§7.2).
5. **Narrations / names** come from `mem_master` (`memberno`, `perno`, name — lowercase property names).
6. Dates are `timestamp without time zone`; money is `double` — round to rupees as the sanction does.

---

## 9. Menu

No menu row exists for this page. Following spec 09 §5, add a `menu_forms` / `menu_sub` row whose
`form_name` slugifies to the route — `'ECS Loan Disbursement'` → `ecs-loan-disbursement` — under the
same menu as submenu `11005066`, `Role 'All'` or duplicated per role, `voc_status 'V'`. Apply out of
band and record the inserted ids. The menu is role-filtered only; non-ECS users hit the page gate.

---

## 10. Open questions

1. **Single-sanction runs.** D1 makes every run `voc_type 14` / status `C`. TSIS used `voc_type 1` /
   status `S` for a single loan. Keep `C` always (this spec), or use `S` when one sanction is ticked?
2. **Bank credit line.** One bank line for the whole run (this spec, as TSIS batch vouchers) or one per
   member to mirror the `fin_voucher_bank` rows?
3. **`DTo` / `DBy` recoveries.** Post only to the books (this spec), or also write a `mem_trn` sundry row
   (`trn_type 1/2`) so the member's sundry balance (which the sanction reads from `mem_trn`) is reduced?
4. **NEFT details.** Is one batch reference (UTR) enough, or must each member row carry the member's
   own account no. / IFSC / UTR (`mem_master` bank fields, `society_neft`)? Is an NEFT upload file
   needed later?
5. **Duplicate cheque no.** Block or only warn?
6. **Who may disburse.** All of `Admin, Maker, Checker`, or `Checker`/`Admin` only (maker–checker)?
7. **Interest up to disbursement date.** The sanction's `I` amount was computed on the sanction date. If
   disbursement is later, recompute interest to the business day, or post the sanctioned figure (this
   spec)?

---

## 11. Files touched (when implemented)

| File | Change |
|---|---|
| `Infin8.Coapp.UI.Client/Pages/Products/ECSLoanDisbursement.razor` | **New** — §4 |
| `Infin8.Coapp.Dto/EcsLoanDisbursementVM.cs` | **New** — request / preview / result VMs |
| `Infin8.Coapp.BusinessLogic/Interface/IEcsLoanDisbursementHandler.cs`, `Concrete/EcsLoanDisbursementHandler.cs` | **New** — §5–§7 |
| `Infin8.Coapp.UI/Program.cs` | Register the handler |
| `Infin8.Coapp.UI/Controllers/LoanSanctionController.cs` | `DisbursementPreview`, `Disburse` actions |
| `Infin8.Coapp.Repository/Concrete/DepositTrnRepository.cs` (+ interface) | Implement receipt insert, oldest-demand query |
| `Infin8.Coapp.Repository/Concrete/MemTrnRepository.cs` | Per-row `trn_slno` |
| `Infin8.Coapp.Repository/Concrete/TransactionsRepository.cs` | `brcode` in voucher numbering |
| `Infin8.Coapp.Repository/Concrete/LoanMasterRepository.cs`, `EcsLoanSanctionRepository.cs` | Close loan; update sanction + eligibility |
| Database: `menu_sub` / `menu_forms` | New row (§9) |

---

## 12. Manual test scenarios

No automated tests. `dotnet build Infin8.Coapp.sln`, run, log in to branch `11005`, Day Begin done.

| # | Scenario | Expected |
|---|---|---|
| 1 | PCARDB user opens `/ecs-loan-disbursement` | "ECS only"; `POST Disburse` returns 403 |
| 2 | Page load | Only status-`E`, non-deleted sanctions of the branch |
| 3 | Tick 1 row | Mode defaults to Cheque; totals show the row |
| 4 | Tick 3 rows | Mode defaults to NEFT; totals are the sums |
| 5 | Preview | Voucher balances; one `LA` line per loan; P/I/PI/MS/SS/D/F lines per sanction; one bank line = Σ net; one bank row per member |
| 6 | Disburse 1 sanction by cheque | 1 `fin_voucher` (`voc_type 14`, `QR`/`PQ`, `VR/n-AR/m`); new loan rows in all five loan tables; `loan_trn R` on the old loan; `mem_trn` MS/SS; `deposit_trn` for D; `fin_voucher_bank` mode `C` with the cheque no.; sanction `C` with `loan_id`/`voc_id` |
| 7 | Disburse 3 sanctions by NEFT | One voucher; 3 new loans; 3 `fin_voucher_bank` rows mode `O`, same reference, each = that member's net |
| 8 | Old loan fully recovered | `isaccountclosed = true`, `accountcloseddate` = business day |
| 9 | Old loan partly recovered | Loan stays open; outstanding reduced |
| 10 | Surety share capital | `mem_trn` row on the **surety's** `mem_id` |
| 11 | A recovered loan had a receipt after sanction | Blocked with "Subsequent transaction…"; nothing written for any ticked sanction |
| 12 | Two users disburse the same sanction | Second gets "not awaiting disbursement"; no duplicate loan |
| 13 | Force an error mid-run (e.g. bad ledger) | Full rollback: no voucher, loan, `mem_trn`, `deposit_trn`, bank row; sanctions still `E` |
| 14 | Run trial balance / day book for the date | Voucher appears balanced; bank ledger credited by Σ net |
| 15 | Sanction list (spec 09) after disbursement | Disbursed sanctions gone; Modify/Delete of them impossible |

---

## 13. Implementation notes (decisions and deviations)

### 13.1 Answers to §10

| Q | Decision |
|---|---|
| 1 | Always `voc_type 14`, sanction status `C`, even for one sanction. |
| 2 | One bank credit line per run (status `Q` / `NEFT`), one `fin_voucher_bank` row per member. |
| 3 | `DTo` / `DBy` **also write `mem_trn`** (`trn_type` 1 / 2, `rpt_amt`), so the sundry balance that the sanction reads (Σ`pmt_amt` − Σ`rpt_amt`) drops. |
| 4 | One shared NEFT reference; optional beneficiary bank name. No per-member account/IFSC/UTR, no upload file. |
| 5 | Duplicate cheque no. on the same bank ledger is a **warning** in the preview, not a block. |
| 6 | `Admin, Maker, Checker`. |
| 7 | **Block**: a sanction that recovers loans (`P`/`I`/`PI` lines) and is dated before the business day is refused — *"interest is stale — modify the sanction (it re-dates to today) and disburse."* |

### 13.2 Where the code differs from §6

- **Ids are allocated in memory, not by the shared repositories** (`EcsLoanDisbursementRepository`
  + `EcsDisbursementIdAllocator`). Inside the transaction it reads each table's MAX once — **within the
  branch** and never below the branch's zero id (ids are `brcode` + 7-digit serial) — and hands out ids,
  loan nos. (per scheme, same format as `GetNewLoanNo`) and `trn_slno`s (per loan; per mem+type+ledger;
  per mem+deposit) from counters. Every row is added with `AddRange` and saved by one `CompleteAsync()`.
  Reason: the shared add-methods re-read MAX without seeing tracked rows (duplicate keys in a batch),
  `GetNewLoanNo` would give two sanctions of one scheme the same number, `AddMemTrnListAsync` keys
  `trn_slno` off the first row, `DepositTrnRepository` is unimplemented. Those shared repositories are
  **unchanged** (live PCARDB paths).
- **Voucher numbers** come from the new repository, per `yr_id` **and** `brcode`: receipt = MAX
  `voc_rpt_slno` of modes `AR`/`QR` + 1, payment = MAX `voc_pmt_slno` of modes `VR`/`PQ` + 1.
  `TransactionsRepository.GetTransactionNo` was **not** changed (it would alter numbering on every live
  voucher path). Follow-up: it still filters by `yr_id` only and does not count `PQ` payment modes.
- Entities are built with object initialisers, not the positional `GetModalObject` factories
  (`GetLoanTrnObject` discards `TrnSlNo`).
- Principal outstanding for closure (D3) uses the sanction's formula: `san_amt − Σ prlcoll_amt`.
- Deposit receipts (`D`) take `demand_id` = the `demand_id` of the member's oldest deposit demand
  (`deposit_trn` rows with `demand_amount > 0`) not yet fully received; zero id when none.
- `fin_voucher_trn.loan_id` is the new loan on the `LA` line and the recovered loan on `P`/`I`/`PI` lines
  (TSIS left the zero id); zero id on the other lines.
- Server re-checks the branch's open `business_day` against the claims `CurrentDate` and refuses with
  "log out and log in again" when they differ (§8.4). The same recovered loan in two ticked sanctions is refused.
- The three unused TSIS-port VMs were deleted. The read-only View modal was extracted to
  `Components/EcsSanctionDetailModal.razor` and is shared with the sanction list.
- `ListSanctions` rows now also carry `TotalReceiptAmount` and `SuretyMemberNo`.
- Menu row: `10-ecs-loan-disbursement.sql` (not applied by the code).

