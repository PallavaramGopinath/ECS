# 06 — ECS Loan Sanction (`ECSLoanSanction`)

> Builds on [01-institution-type.md](./01-institution-type.md) and
> [02-loan-module-design.md](./02-loan-module-design.md). The companion page,
> **ECS Loan Disbursement**, will be specified separately in `08-ecs-loan-disbursement.md`.
>
> **Source being ported:** the TSIS WinForms form `TSISWF/TSIS/frmLoanDisbursement.cs`, with its
> `.Designer.cs` and `.resx`. The `.resx` holds only the form icon, so there are no strings or
> resources to carry over. The form's working *Print Office Note* lives in the sibling form
> `TSISWF/TSIS/frmLoanSanctionTSIS.cs`.

---

## 1. Purpose and scope

The WinForms form does two jobs, chosen from a **Select Action** combo:
- 1 = Loan Sanction
- 2 = Loan Disbursement
- 3 = View/Delete

The Blazor app splits these into two pages.

| Page | Covers | Writes |
|---|---|---|
| **ECSLoanSanction** (this spec) | Member and pay details, loan terms, recoverables, eligibility, save, modify, delete and list of sanctions, and the Office Note | `loan_sanction`, `loan_sanction_trn`, `loan_eligibility`. **No voucher and no `loan_master`.** |
| ECS Loan Disbursement (spec 07) | Pick a sanction (status `E`), receipt and payment numbers, the voucher entry grid, and the disbursement itself | `fin_voucher*`, `loan_master`, `loan_trn`, `loan_disb`, `mem_trn`; sets `loan_sanction.loan_id` and moves the status on from `E` |

### Decisions taken

1. **Evolve, don't duplicate.** The repo already has a working sanction slice:
   - `Infin8.Coapp.UI.Client/Pages/Products/SuretyLoanSanction.razor` (`/loan-sanction`)
   - `Infin8.Coapp.UI/Controllers/LoanSanctionController.cs` (`api/loansanction`)
   - `Infin8.Coapp.BusinessLogic/Concrete/SuretyLoanSanctionHandler.cs`
   - `Infin8.Coapp.BusinessLogic/Concrete/LoanEligibilityHandler.cs`
   - `Infin8.Coapp.Repository/Concrete/SuretyLoanSanctionRepository.cs`

   ECSLoanSanction is built by **extending and renaming this slice**, not by creating a parallel one.
2. **Pay details are typed in by hand** for each sanction, from the member's salary certificate,
   exactly as in WinForms.
3. **Recoverables are fixed at sanction.** These are old-loan outstanding, share-capital shortfall,
   due-to balances, manual deductions and pending deposits. They are stored in `loan_sanction_trn`;
   the disbursement page only posts them.
4. **Schemes** are those with `Loan_Type = 1` (`LoanCategory.ThriftCreditLoans`), filtered through
   `LoanCategoryPolicy`. Surety, education, draught, etc. are separate schemes under that type
   (spec 07 §3.3; this replaces the earlier 10 / 11 / 12 filter).

---

## 2. WinForms source analysis

### 2.1 Tabs (`tabMain`)

| Tab | Side | In ECSLoanSanction? |
|---|---|---|
| Entry (`tabPageEntry`, `uc_Entry1` voucher grid, receipt and payment numbers, *Save Disbursement*) | Disbursement | No, moves to spec 07 |
| Pay Details (`tabPagePayDetails`) | Sanction | Yes |
| Loan Details (`tabPageLoanDetails`): loan terms, member assets, surety, eligibility, recoverables grid | Sanction | Yes |
| Non Deductable Loan OS (`tabPagePreviousLoanOS`) | Sanction | Yes |
| Pending Deposits (`tabPagePendingDeposits`) | Sanction | Yes |
| List Disbursements (`tabPageListDisbursement`) | Disbursement | No, moves to spec 07 |
| List Sanctions (`tabPageListSanction`) | Sanction | Yes |

### 2.2 Field map

**Header**

| WinForms control | Label | Blazor field / DTO property | DB column (`loan_sanction`) |
|---|---|---|---|
| `dtpDisbursementDate` | Disbursement Date | The business day from `userInfoDto.CurrentDate`; read-only | `sanctiondate` |
| `txtMemNo` / `txtMemId` / `txtMemName` | Mem No / Name | `memberNo` → `Mem_Id`, name | `mem_id` |
| `txtSuretyMemNo` / `Id` / `name` | Surety Mem No | `SuretyMem_Id` (from `mem_master.suretymem_id`) | `suretymem_id` |
| `txtSanctionId` | Sanction Id | `LoanSanction_Id` (set when an existing sanction is loaded) | `loansanction_id` |

**Pay Details**

| WinForms control | Label | Blazor field / DTO property | DB column (`loan_sanction`) |
|---|---|---|---|
| `dtpDateOfBirth` / `dtpDateOfRetirement` / `txtServiceMonths` | DOB / DOR / Service in Months | Computed (§3.6) | n/a |
| `txtBasicPay` | Basic Pay | `BasicPay` | `basicpay` |
| `txtGrossPay` | Gross Pay | `GrossPay` | `grosspay` |
| `txtSocietyDeduction` | Society Deduction | `SocietyDeduction` | `societydeduction` |
| `txtTotalDeduction` | Total Deductions | `TotalDeductions` | `totaldeductions` |
| `txtNetSalary` | Net Salary | `NetPay` = Gross − Total | `netpay` |

**Loan Details**

| WinForms control | Label | Blazor field / DTO property | DB column (`loan_sanction`) |
|---|---|---|---|
| `cboSelectLoan` | Select Loan | `Scheme_Id` | `scheme_id`, `inst_type` |
| `txtLoanAmount` | Loan Amount | `LoanAmount` | `loanamount` |
| `txtPeriodOfLoan` | Period of Loan | `PeriodOfLoan` | `periodofloan` |
| `txtROI` / `txtPIRate` | Rate of Interest / Penal Rate | `RateOfInterest` / `PenalRate` | `rateofinterest` / `penalrate` |
| `txtInstalmentAmt` | Instalment Amt | `InstalmentAmount` | `instalmentamount` |
| `dtpFirstDueDate` | First Due Date | `FirstDueDate` | `firstduedate` |
| `txtThisLoanLimit` / `txtMaximumLoanLimit` | This Loan Limit / Maximum Loan Limit | `ThisLoanLimit` / `MemberTotalLimit` | `thisloanlimit` / `membertotallimit` |
| `txtNoOfInstalmentCompleted` / `txtNoOfInstalmentsRecovered` | No. of Instalment Completed / Recovered | same names | same names |

**Assets**

| WinForms control | Label | Blazor field / DTO property | DB column (`loan_sanction`) |
|---|---|---|---|
| `txtMemSC` / `txtSuretySC` | Share Capital / Surety's Share Capital | `MemberShareCapitalAmount` / `SuretyShareCapitalAmount` | same names |
| `txtMembesTD` / `txtMemFWD` | Thrift Deposit / Family Welfare Deposit | `MemberTD` / `FWD` | `membertd` / `fwd` |

**Eligibility**

| WinForms control | Label | Blazor field / DTO property | DB column |
|---|---|---|---|
| `txt25TimesOfGP` / `txt50PercentOfSalary` / `txt25PercentOfSalary` | 25 × GP / 50% / 25% | `LoanEligibilityResultVM` | `loan_sanction_trn` payment lines 1–3 |
| `txtDemand` / `txtExistingDemand` / `txtOtherDeductions` / `txtNetPay` | Demand / Existing / Other / Net Pay | `LoanEligibilityResultVM` | payment lines 4–6 |

**Recoverables**

| WinForms control | Label | Blazor field / DTO property | DB column |
|---|---|---|---|
| `dgvOS` | Recoverables grid | `List<DeductableLoanRowVM>` | `loan_sanction_trn` receipt side |
| `txtTotalRecoverable` | Total Recoverable | Σ outstanding | `loan_sanction.totalreceiptamount` |
| `txtLoanBalanceAmount` | Loan Balance Amount | Loan − recoverable (the net cash to be disbursed) | derived |
| `cboLedgerDeduction` + `txtDeductionAmt` + `+`/`−` | Manual deduction | Manual row | `loan_sanction_trn` (status from §3.8) |

**Surety and other tabs**

| WinForms control | Label | Blazor field / DTO property | DB column |
|---|---|---|---|
| `dgvSuretyDetails` + `lblSuretyODMessage` | Surety Member's Loan Overdue | Surety OD list | `loan_sanction.issuritybecomedefaulter` |
| `dgvNonDeductableLoanBalances` + *Add to Receipt* | Non Deductable Loan OS | Non-deductable list | Moved into recoverables |
| `dgvDepositPending` | Pending Deposits | Pending-deposit list | Moved into recoverables (status `D`) |
| `dgvLoanSchemeDetails` | Caption / Content | Scheme facts (§3.9) | n/a |
| `chkStopDemand` | Stop Demand | **Not used in code.** Drop it. | n/a |

**Columns of the `dgvOS` grid.** Each column maps to `DeductableLoanRowVM` and `loan_sanction_trn`:
- LoanId → `loan_id`
- LoanNo
- DMId → `dm_id`
- Status → `status`
- MemId → `mem_id`
- LedId → `led_id`
- LedgerName → `receiptdescription`
- Outstanding → `receiptitem`
- CalculatedAmt → `calcamount`
- ODAmt → `odamount`
- SchemeId → `recscheme_id`
- LoanSlNo → `loan_slno`
- LDeduct
- MatchSC

### 2.3 Event flow, in the order WinForms drives it

1. **Form load.** Populates the scheme list (`GetLoanSchemeList(1)`), the deduction ledgers
   (`GetLedgerItemsExceptBankAccounts`) and the sanction-list combos. Sets
   `FirstDueDate = GetFirstDueDate(today)`.
2. **`txtMemNo_Leave`.** Loads the member by `memberNo` or `PerNo` (`Map.IsMemNo`), then:
   - fills DOB and DOR, and runs `GetDateOfRetirementAndServiceMonths()`;
   - loads the surety from `mem_master.SurityMem_Id`;
   - jumps to the Pay Details tab.
3. **`cboSelectLoan_Leave`.**
   1. Checks that this is not a duplicate sanction on this date, with status `E`.
   2. Applies the stipulated-period check, which is a **confirm** and not a hard stop (§3.5).
   3. Checks that the loan is not already saved or disbursed on this date (status `S`/`C`).
   4. Loads the scheme. This sets LoanAmount = MaximumLoanAmount, Period = MaximumPrincipalPeriod
      and ThisLoanLimit, and caches `Inst_Type`, `AdoptLoanEligibility`, `MatchShareCapital`,
      `AdoptLoanLimit` and `IssurityMemberRequired`.
   5. Fills the scheme-details grid.
   6. Reads ROI and PI from `Loan_ROI_Template` (agency `"S"`, latest `wef` ≤ date).
   7. Sets MaximumLoanLimit = `Map.MaxLoanLimit`.
4. **`txtLoanAmount_Leave` / *Get Recoverables* → `GetLoanOutstandingAndEligibleCreteria()`.**
   1. Validates the inputs: loan amount, surety (if the scheme needs one), scheme and gross pay.
   2. Computes the required share capital (§3.1) and the instalment (§3.2).
   3. Reads the member and surety share-capital balances.
   4. Fills the **non-deductable** loans grid and computes `ExistingDemand`.
   5. Fills the **deductable** loans into `dgvOS` as P / I / PI rows, and sets instalments
      completed/recovered for the same scheme.
   6. Adds the member's **due-to** balances as `DTo` rows.
   7. Adds share-capital shortfall rows (`MS`/`SS`) according to `MatchShareCapital`.
   8. Reads the thrift and FWD balances.
   9. Lists the surety's overdue loans, if a surety is required.
   10. Calls `Sum_DueItems()`.
5. **`Sum_DueItems()`.** Totals the recoverables, then:
   - computes the loan balance, and **refuses if it is ≤ 0** ("Member does not have adequate loan
     balance");
   - computes 25 × GP, 50 % and 25 %;
   - computes Demand = instalment + current deposit demand;
   - computes Other = Total − Society;
   - computes NetPay = Gross − Demand − Existing − Other.
6. **Optional actions.**
   - *Display Demand* shows the principal, interest and per-deposit demand.
   - *Display Loan Eligibility* shows the 50 %/25 % verdict.
   - *+* / *−* adds or removes manual deduction rows.
   - *Add to Receipt* moves non-deductable recoveries into `dgvOS`.
   - *Get Pending Deposit Amount* fills the pending deposits.
7. **Save.** `btnSaveLoanSanction_Click` → `Validate_Sanction()` → `Save_Sanction()` →
   `LoanService.Save_LoanSanction(header, trList, existingId)`.
8. **List and load.** *List Sanctions* uses filters by Sanction Id, Mem No, Date range or All.
   Clicking a row runs `FillSanctioned_For_Deletion(id)`, which reloads the full sanction. The button
   then reads *Modify Loan Sanction*.
9. **Delete.** `btnDeleteLoanSanction_Click` → `Delete_Sanction(id)`, which soft-deletes.
10. **Print Office Note.** The button exists in this form but **has no click handler**. The working
    code is in `frmLoanSanctionTSIS.Print_OfficeNote()` (§9).

---

## 3. Business rules and formulas

Each rule notes its TSIS source and where it already exists in Blazor.

### 3.1 Required share capital and share-capital matching
```
required = (int)( round(LoanAmount × Map.ShareCapitalPercentageOnLoanOS / 100, 2) + 0.5 )
if suretySC ≥ required: required = suretySC          // TSIS quirk, see §4 #9
MatchShareCapital: 1 = member and surety, 2 = member only, 3 = none
  member shortfall → row Status "MS", Mem_Id = member, Led_Id = Map.ShareCapital_Led_Id
  surety shortfall → row Status "SS", Mem_Id = surety, same ledger
```
- TSIS source: `frmLoanDisbursement.cs` L1252–1360.
- Blazor: `Map_General.ShareCapitalPercentageOnLoanOS` and `ShareCapital_Led_Id` exist.
  `SuretyLoanSanctionRepository.GetDeductionsDataAsync` already produces share-capital rows. Verify
  that it uses the same rounding and the same `MatchShareCapital` cases.

### 3.2 Instalment amount (`LoanService.GetLoanInstalmentAmount`)
```
Inst_Type 1, 3, 4 : round(amount / period, 0)
Inst_Type 2 (EMI) : r = roi/100/12 ;  inst = (r + r/((1+r)^n − 1)) × amount   (0 if roi ≤ 0)
result rounded MidpointRounding.AwayFromZero
```
- The page's existing `EnforceInstallmentFloor` stays.

### 3.3 Demand and eligibility
```
NewLoanDemand     = instalment (+ monthly interest where Inst_Type implies interest-in-demand)
DepositDemand     = Σ over Deposit_Master (DM_Id ≤ 3, not deleted):
                      d = DMPercentage > 0 ? round(BasicPay × pct/100) : DMFixedAmount
                      if DMFrequency = "A" and DemandMonth ≠ firstDueDate.Month → d = 0
                      if latest Depositor_Option (wef ≤ date) has OptionAmount > 0 → d = OptionAmount
Demand            = NewLoanDemand + DepositDemand
ExistingDemand    = Σ instalment (+ calc interest when Int_Application = 2) of NON-deductable loans,
                    minus those moved into recoverables via "Add to Recovery"
OtherDeductions   = TotalDeductions − SocietyDeduction
NetPay (after)    = GrossPay − Demand − ExistingDemand − OtherDeductions
25×GP = GrossPay×25 ; 50% = round(GrossPay×0.5) ; 25% = round(GrossPay×0.25)

if scheme.AdoptLoanEligibility = 1:
   FAIL if Demand + ExistingDemand > 50%      "Demand amount is greater than 50% of gross pay"
   FAIL if 25% > NetPay                       "Net Pay is less than 25% of gross pay"
```
- `LoanEligibilityHandler.CalculateAsync` already implements this. Reconcile three differences:
  - It adds `LoanAmount×ROI/1200` interest to every loan demand. **Apply it only to Inst_Type 3
    (principal + interest)**, matching TSIS `GetLoanDemandAmount` type 3.
  - It applies the 25 % floor even when `AdoptLoanEligibility = 0`. TSIS applies **both** checks only
    when the flag is 1.
  - It compares `ServiceInMonths < Map_General.TSISRetirementAgeInMonths`. That is not a TSIS rule
    (§3.6 explains what service months is in TSIS). **Replace it** with: FAIL if
    `ServiceInMonths < PeriodOfLoan`, meaning the loan cannot run past retirement. Confirm this with
    the business (§13).
- **DepositDemand is not in Blazor yet.** Add `GetCurrentDepositDemandAsync(memId, basicPay,
  calcDate, demandDate, brCode)` (§7.1).

### 3.4 Duplicate and already-saved checks
- **Duplicate sanction.** Look for an existing `loan_sanction` with the same `mem_id`, `scheme_id` and
  `sanctiondate`, status `E`, not deleted, and the same `brcode`. If one exists, block.
- **Already saved or disbursed on the date.** The same check with status `S`/`C`. If found, block.
- These are separate from §3.5.

### 3.5 Stipulated period between loans
```
last = MAX(loan_master.san_date) for mem + scheme, not deleted, brcode
elapsed = MonthsBetween(last, sanctionDate)      // completed months, §3.7
if elapsed < Map_General.MinInstalmentsForLoan →
   "had availed Loan only before {elapsed} months against stipulated period of {min}. Continue?"
```
- **TSIS treats this as a warning that the user can confirm.** The current Blazor handler rejects
  the save outright, and checks against `loan_sanction` instead of `loan_master`.
- Blazor should show a confirm dialog. The save DTO then carries `OverrideStipulatedPeriod = true`,
  and the server records the override. Keep a hard reject only if the business decides so (§13).

### 3.6 Retirement date and service months
```
retirementAgeMonths = mem_retirementage.retirementage (per member, not deleted, brcode)
                      else Map_General.TSISRetirementAgeInMonths
DOR = EndOfMonth(DOB.AddMonths(retirementAgeMonths))
ServiceMonths = MonthsBetween(sanctionDate, DOR) − Map_General.MinInstalmentsForLoan
```
- TSIS has a bug here: it used `dtpDisbListFromDate` instead of the sanction date.
- `Mem_RetirementAge` exists in `Infin8.Coapp.Models`. Confirm that its `RetirementAge` is in months.

### 3.7 `MonthsBetween` (completed months)
```csharp
if (from > to) (from, to) = (to, from);
int m = (to.Year*12 + to.Month) - (from.Year*12 + from.Month);
return (from.AddMonths(m) > to || to.Day < from.Day) ? m - 1 : m;
```
- Put this in `Infin8.Coapp.Utility` as a static helper. `SuretyLoanSanctionHandler` already has a
  private `MonthsBetween`, so check it matches and reuse one copy.

### 3.8 Recoverable row status codes

| Status | Meaning | Source |
|---|---|---|
| `P` / `I` / `PI` | Principal / interest / penal-interest outstanding of a **deductable** loan | `Loan_Scheme_Deduction_Rules` (scheme → deductable scheme) |
| `DTo` | Member's due-to (sundry) balance | `mem_trn`, sundry type 1 |
| `DBy` | Due-by ledger (manual row) | sundry type 2 |
| `MS` / `SS` | Member / surety share-capital shortfall | §3.1 |
| `D` | Deposit (pending deposit or deposit ledger, with `DM_Id`) | `deposit_master.dmled_id` |
| `G` | Any other general ledger (manual row) | default |

- **Manual rows.** Pick a ledger and an amount. The server works out the status in order: deposit
  ledger → `D` (with DM_Id); sundry due-to → `DTo`; due-by → `DBy`; share capital → `MS`; else `G`.
- **Removing rows.** Only manual rows and computed rows with a zero outstanding can be removed. The
  WinForms `−` button has a buggy loop; do not copy it.

### 3.9 Scheme facts shown read-only (`FillLoanSchemeDetailsInGrid`)
- Demand frequency and interest frequency: 1 Monthly, 4 Quarterly, 6 Half-yearly, 12 Annual.
- PI application: 1 = on principal OD, otherwise on P + I OD.
- IOD application: 1 = EMI not applied.
- Advance principal: 1 = do not raise the principal demand.
- Match SC: 1, 2 or 3.
- Adopt loan limit and surety required: Yes / No.

### 3.10 First due date
- Default is the 1st of the month after the sanction date.
- Validate that it falls on the **1st of a month** and is **≥ sanction date**.

### 3.11 Validation on save (from `Validate_Sanction`, server-enforced)
- Sanction date is inside the session's financial year.
- Member is selected. LoanAmount, Period, ROI, PI and Instalment are each > 0.
- Loan balance (Loan − Total recoverable) is > 0.
- First due date follows §3.10. A scheme is selected and allowed for ECS.
- A surety is present when `IssurityMemberRequired = 1`.
- §3.3 eligibility passes when `AdoptLoanEligibility = 1`.
- LoanAmount ≤ ThisLoanLimit (scheme max).
- When `AdoptLoanLimit = 1`: (LoanAmount + non-deductable OS within the limit) ≤ `Map_General.MaxLoanLimit`.
  TSIS displays this but never enforces it. **Enforce it** (§13 confirms).
- **On modify only:** no subsequent transaction exists (§3.13).

### 3.12 What is saved
**Header, `loan_sanction`:**
- All the fields in §2.2.
- `loanelig_id` linked to a new `loan_eligibility` row (existing behaviour).
- `loan_id = 0` and `voc_id = 0`.
- `ismemberbecomedefaulter = Σ ODAmt(recoverables) > 0`.
- `issuritybecomedefaulter = Σ surety OD > 0`.
- `sl_prlos` = Σ outstanding of `P` rows whose scheme is the surety-loan scheme.
- `loansanction_status = 'E'`.
- `loansanction_delete = false`, `voc_status = 'V'`.
- `usr_id`, `yr_id` and `brcode` taken from **server claims**.

**Lines, `loan_sanction_trn`:**
- One line per recoverable row with Outstanding > 0 or CalcAmount > 0. `SlNo` runs 1, 2, … and each
  line also carries `payment_id = SlNo` with the matching payment item.
- Padding lines up to SlNo 6, so that `payment_id` 1–6 always exist:

  | Payment_Id | PaymentDescription | PaymentItem |
  |---|---|---|
  | 1 | 25 Times of Gross Pay | 25×GP |
  | 2 | 50 Percent of Salary | 50 % |
  | 3 | 25 Percent of Salary | 25 % |
  | 4 | Society Demand | Demand |
  | 5 | Other Deductions | Other |
  | 6 | Net Pay | NetPay (the Office Note shows 0 if negative) |

- This layout is what the Office Note (§9) and the disbursement page read. `SuretyLoanSanctionHandler`
  builds something similar (payment lines around L220–230); align it with this table.

### 3.13 Subsequent-transaction check (before modify or delete)
For each non-deleted `loan_sanction_trn` row of the sanction with `loan_id > 0`:
- `maxSnap = MAX(loan_slno)`
- `maxNow = MAX(loan_trn.trn_slno)` where `trntr_delete = false` and the same `brcode`

If `maxNow > maxSnap`, add the message `"Subsequent transaction made in loan account {loan_no}"`.
**Any message blocks the action**, because the recoverable snapshot is stale.

### 3.14 Status lifecycle

| `loansanction_status` | Meaning | Set by |
|---|---|---|
| `E` | Sanctioned, awaiting disbursement | ECSLoanSanction save |
| `S` / `C` | Disbursed | ECS Loan Disbursement (spec 07) |

> **Change from the current code.** `SuretyLoanSanctionHandler` saves `"S"`. Under TSIS semantics
> that means *disbursed*, and it would hide the sanction from the disbursement page. Switch it to
> `"E"`. Check first whether any rows already saved in a test database need fixing.

---

## 4. TSIS defects — do **not** port

1. `GetLoanROIAndPIFromTemplate` (the overload the form uses) can return null. Return a result with
   a clear "ROI template not found for scheme/date" message.
2. `GetLoanDemandAmount` type 2 divides ROI by 100 twice and passes a negative principal.
3. `CalculateLnDues` swaps the from and to dates for IOD application 2. There are also two copies of
   the method that compute `NonODPrl` differently. Use the single calculation the Blazor loan engine
   already has.
4. The deductable path writes IOD to `iodcalculatedamt`, while the other paths write `iodcalcamt`.
5. `GetLoanBalanceOnLoanIds` aliases `disbamt` twice, and its `HAVING San_amt − PrlColl > 0` drops
   loans where only interest is outstanding.
6. The sanction list builds its SQL by **string concatenation**, with unquoted dates. Use LINQ or
   `NpgsqlParameter`.
7. The *Print Office Note* button in `frmLoanDisbursement` has no handler.
8. Reading saved Tr rows, and the Office Note query, never filter `LoanSanctionTr_Delete`. Filter it.
9. `if (suretySC ≥ required) required = suretySC` inflates the member's requirement to match the
   surety's holding. **Keep it only if the business confirms (§13).** Otherwise compare each party
   against `required`.
10. `scPer = 10 / 100` is dead code (integer division). Ignore it.
11. The `btnDeductionRemove` loop removes rows by the wrong index. Ignore it.
12. `Convert.ToInt16(memId)` overflows for ids above 32 767. Use `decimal` IDs throughout.

---

## 5. Gap analysis against today's `SuretyLoanSanction`

| Feature | Today | Action |
|---|---|---|
| ECS gate (`isReady`, `InstitutionType.ECS`) | Have | Keep. Add the **server-side** check (§7.3). |
| Member + surety lookup, DOB / DOR | Have | Add the retirement-age / service-months rule (§3.6) |
| Pay details | Have (prefilled from `Pay_Slip`) | Manual entry is the default. The prefill can stay as an optional *Fetch from payroll* button (§13). |
| Scheme list | Partial: `Loan_Type 1` | Switch to 10/11/12 via `LoanCategoryPolicy` |
| Scheme defaults, ROI / PI | Have | Make sure ROI/PI come from `Loan_Roi_Template` as of the sanction date |
| Scheme facts grid | Have | n/a |
| Deductable recoverables (P/I/PI), SC rows, sundry | Have | Check against §3.1 and §3.8 |
| Manual deduction rows | Have | Status resolution on the server (§3.8) |
| Non-deductable loans + *Add to Recovery* | Partial (tab exists) | Add the move-to-recoverables step and the ExistingDemand adjustment |
| Pending deposits | Partial (tab exists) | Add *Add to Recovery* as status `D` |
| Deposit demand in eligibility | **Missing** | §3.3 / §7.1 |
| Eligibility | Have, with differences | Reconcile per §3.3 |
| Duplicate same-day / already-saved checks | **Missing** | §3.4 |
| Stipulated period | Have, as a hard reject | Change to a confirm plus override (§3.5) |
| Loan-limit check | **Missing** | §3.11 |
| Save | Have (single `SaveChanges`) | Status `E`. Wrap in an explicit transaction. |
| **List sanctions** | **Missing** | §7, §8 |
| **Load for modify / Modify** | **Missing** | Soft delete + reinsert under a new id |
| **Delete** | **Missing** | Soft delete, guarded by §3.13 |
| **Print Office Note** | **Missing** | §9 |
| Nav menu row | **Missing** | §10 (optional) |

---

## 6. Data model

No new tables. Every entity already exists in `Infin8.Coapp.Models`, with a `DbSet` in `CSISContext`.

| Entity | Key | Soft delete | Used for |
|---|---|---|---|
| `Loan_Sanction` | `LoanSanction_Id` (decimal) | `LoanSanction_Delete` | header |
| `Loan_Sanction_Trn` | `LoanSanctionTr_Id` (decimal) | `LoanSanctionTr_Delete` | recoverable and payment lines |
| `Loan_Eligibility` | `LoanElig_Id` (**int**) | `LoanElig_Delete` | eligibility snapshot (existing behaviour) |
| `Loan_Schemes` | `Scheme_Id` (int) | `Scheme_Delete` | product flags (§3.9) |
| `Loan_Scheme_Deduction_Rules` | `Rule_Id` | `Is_Active` | deductable versus non-deductable |
| `Loan_Roi_Template` | n/a | n/a | ROI and PI as of the date |
| `Map_General` | `ID` | n/a | `ShareCapitalPercentageOnLoanOS`, `ShareCapital_Led_Id`, `MaxLoanLimit`, `MinInstalmentsForLoan`, `TSISRetirementAgeInMonths`, `ThriftDeposit_DMId`, `FamilyWelfareDepsitDM_Id`, `IsMemNo` |
| `Mem_RetirementAge` | n/a | `RetirementAge_Delete` | per-member retirement age |
| `mem_master` | `mem_id` | `memberdelete` | member, `suretymem_id`, `dob`, `dor` |
| `Mem_Trn`, `Deposit_Trn`, `Deposit_Masters` | n/a | n/a | SC, sundry and deposit balances; deposit demand |
| `Loan_Master`, `Loan_Trn` | n/a | n/a | read-only: outstanding, last `san_date`, §3.13 |
| `Reports_Master` | `Report_Id` | `ReportDelete` | Office Note row (§9) |

- **Points to check during the build:**
  - `loan_sanction` already has `da`, `lumpsum` and `fwd`. Populate `fwd` from the FWD balance;
    leave `da` and `lumpsum` at 0 unless the business wants them shown.
  - `Map_General` has **no** `RetirementAge` column, only `TSISRetirementAgeInMonths`. Use that.
  - `Map_General` has no `DepositPercentage`; deposit rules come from `Deposit_Masters`.
- **If a column turns out to be missing,** the DDL must be **additive** and applied to PostgreSQL out
  of band. There are no EF migrations.
- **IDs** are MAX+1 on the application side, as in today's `PersistSanctionAsync`, generated inside
  the save transaction.

---

## 7. Server design

The layers are Repository → UnitOfWork → Handler → Controller, following `CLAUDE.md`. Rename the
Surety* slice to ECS* so that the name matches its broader scope.

| Today | Becomes |
|---|---|
| `ISuretyLoanSanctionRepository` / `SuretyLoanSanctionRepository` | `IEcsLoanSanctionRepository` / `EcsLoanSanctionRepository` |
| `IUnitOfWork.SuretyLoanSanction` (IUnitOfWork.cs L50, UnitOfWork.cs L61 and L227) | `EcsLoanSanction`, updated in **all three** places |
| `ISuretyLoanSanctionHandler` / `SuretyLoanSanctionHandler` | `IEcsLoanSanctionHandler` / `EcsLoanSanctionHandler` |
| `Program.cs` L158 `AddScoped<ISuretyLoanSanctionHandler, …>` | `AddScoped<IEcsLoanSanctionHandler, EcsLoanSanctionHandler>()` (only `Infin8.Coapp.UI/Program.cs`) |
| `SuretyLoanSanction*VM` DTOs | Keep the class names, to avoid churn, or rename together. Add the new VMs from §7.4. |

> If the rename looks too noisy for one change, add the new methods to the existing Surety* types
> first and rename in a follow-up commit. The behaviour does not change.

### 7.1 Repository — `EcsLoanSanctionRepository`

Every query below filters **`BrCode` on every join alias** and uses LINQ query syntax. Existing
methods are unchanged unless noted.

```csharp
public interface IEcsLoanSanctionRepository
{
    // existing
    Task<SuretyLoanSanctionDataVM>  GetSuretyLoanMemberAsync(string memberNo, string brCode);
    Task<SuretyLoanSchemeDefaultsVM> GetSchemeDefaultsAsync(int schemeId, DateTime asOn, string brCode); // + ROI/PI as on date
    Task<SuretyDeductionsDataVM>    GetDeductionsDataAsync(decimal memId, decimal suretyMemId, int schemeId,
                                                           double loanAmount, DateTime asOn, string brCode);
    Task<DateTime?> GetLastLoanSanctionDateAsync(decimal memId, int schemeId, string brCode); // from loan_master.san_date (§3.5)

    // new — checks
    Task<bool> ExistsSanctionOnDateAsync(decimal memId, int schemeId, DateTime date, string[] statuses, string brCode); // §3.4
    Task<List<string>> GetSubsequentTransactionMessagesAsync(decimal sanctionId, string brCode);                     // §3.13
    Task<int?> GetMemberRetirementAgeMonthsAsync(decimal memId, string brCode);                                     // §3.6

    // new — recoverable sources
    Task<List<NonDeductableLoanRowVM>> GetNonDeductableLoanBalancesAsync(decimal memId, int schemeId, DateTime asOn, string brCode);
    Task<List<PendingDepositRowVM>>    GetPendingDepositsAsync(decimal memId, DateTime asOn, string brCode);
    Task<List<SuretyOverdueRowVM>>     GetSuretyOverduesAsync(decimal suretyMemId, DateTime asOn, string brCode);
    Task<double> GetCurrentDepositDemandAsync(decimal memId, double basicPay, DateTime calcDate, DateTime demandDate, string brCode); // §3.3
    Task<string> ResolveDeductionStatusAsync(decimal ledId, string brCode);  // returns "D|<dmId>", "DTo", "DBy", "MS", "G" (§3.8)

    // new — list / load / delete / report
    Task<List<EcsSanctionListItemVM>> GetSanctionListAsync(EcsSanctionListFilterVM filter, string brCode);
    Task<EcsSanctionDetailVM?>        GetSanctionByIdAsync(decimal sanctionId, string brCode);  // header + non-deleted trn lines + loan_no
    Task SoftDeleteSanctionAsync(decimal sanctionId, string brCode);  // marks header + lines + eligibility deleted; no SaveChanges
    Task<List<OfficeNoteRowVM>>       GetOfficeNoteAsync(decimal sanctionId, string brCode);

    // changed — no internal SaveChanges; handler owns the transaction
    Task<(decimal sanctionId, int eligId)> AddSanctionAsync(Loan_Eligibility elig, Loan_Sanction header, List<Loan_Sanction_Trn> lines);
}
```

**`GetSanctionListAsync`**
- Join `loan_sanction ⋈ loan_schemes ⋈ mem_master`.
- Filter `loansanction_status = 'E'`, `!loansanction_delete`, `brcode`.
- Optional filters: `SanctionId`, `MemId`, `FromDate..ToDate`.
- Order by `sanctiondate desc, loansanction_id desc`.

**`AddSanctionAsync`**
- Replaces `PersistSanctionAsync`. Assigns MAX+1 to `LoanElig_Id`, `LoanSanction_Id` and
  `LoanSanctionTr_Id`, back-links them, and calls `Add` on all three.
- It **does not** call `SaveChanges`; the handler calls `CompleteAsync()`, per the repository
  contract in `CLAUDE.md`.

**Interest and penal-interest accrual for P / I / PI rows**
- Reuse the loan-dues calculation `GetDeductionsDataAsync` already uses.
- Do not port TSIS `CalculateLnDues` (§4 #3).

### 7.2 Handler — `EcsLoanSanctionHandler`

```csharp
public interface IEcsLoanSanctionHandler
{
    Task<SuretyLoanSanctionDataVM>   GetMemberAsync(string memberNo, DateTime asOn, string brCode);       // + DOR / service months
    Task<SuretyLoanSchemeDefaultsVM> GetSchemeDefaultsAsync(int schemeId, decimal memId, DateTime asOn, string brCode);
        // returns defaults + warnings: Duplicate (block), AlreadySaved (block), StipulatedPeriod (confirm)
    Task<EcsRecoverablesVM>          GetRecoverablesAsync(EcsRecoverablesRequestVM req, string brCode);
        // deductable P/I/PI + DTo + MS/SS + non-deductable list + pending deposits + surety OD + assets + instalment
    Task<string>                     ResolveDeductionStatusAsync(decimal ledId, string brCode);
    Task<LoanEligibilityResultVM>    CalculateEligibilityAsync(LoanEligibilityInputVM input);          // delegates to LoanEligibilityHandler
    Task<SanctionSaveResultVM>       SaveAsync(SuretyLoanSanctionSaveVM vm, UserCtx ctx);               // create or modify
    Task<SanctionSaveResultVM>       DeleteAsync(decimal sanctionId, UserCtx ctx);
    Task<List<EcsSanctionListItemVM>> ListAsync(EcsSanctionListFilterVM filter, string brCode);
    Task<EcsSanctionDetailVM?>        GetAsync(decimal sanctionId, string brCode);
    Task<List<OfficeNoteRowVM>>       GetOfficeNoteAsync(decimal sanctionId, string brCode);
}
```

**`SaveAsync` algorithm** (create when `vm.LoanSanction_Id == 0`, otherwise modify):

1. Re-validate everything in §3.11 **on the server**:
   - the scheme is allowed for ECS (`LoanCategoryPolicy.IsAllowed((LoanCategory)scheme.Loan_Type, ECS)`);
   - the duplicate and already-saved checks (§3.4). On modify, exclude the sanction being modified;
   - the stipulated period (§3.5). Reject unless `vm.OverrideStipulatedPeriod`;
   - eligibility, recomputed (§3.3);
   - on modify, §3.13 must return no messages.
2. Recompute the totals on the server: Σ recoverables, the loan balance (> 0), and the six payment
   lines. **Never trust client totals.**
3. Build the `Loan_Eligibility`, the `Loan_Sanction` (status `E`) and the lines (§3.12).
4. Run the writes in one transaction:
   ```csharp
   _unitOfWork.BeginTransaction();
   try {
       if (isModify) await _unitOfWork.EcsLoanSanction.SoftDeleteSanctionAsync(vm.LoanSanction_Id, brCode);
       var (newId, _) = await _unitOfWork.EcsLoanSanction.AddSanctionAsync(elig, header, lines);
       await _unitOfWork.CompleteAsync();
       _unitOfWork.CommitTransaction();
       return new SanctionSaveResultVM { Success = true, LoanSanction_Id = newId, Message = "Loan sanction saved" };
   } catch { _unitOfWork.RollBack(); throw; }
   ```
   **Modify creates a new sanction id.** The old one stays soft-deleted, which gives the sanction
   history required by the preliminary spec §4.5. The UI shows the new id.

**`DeleteAsync`**
- Requires: the sanction exists, belongs to `brCode`, status is `E`, and §3.13 is clean.
- Then: `SoftDeleteSanctionAsync` → `CompleteAsync`, inside a transaction.

### 7.3 Controller — `LoanSanctionController` (`api/loansanction`)

Keep the existing class (namespace `Infin8.Coapp.API.Controllers`, file
`Infin8.Coapp.UI/Controllers/LoanSanctionController.cs`). Swap the injected handler and add endpoints.

| Verb | Route | Body / returns | Notes |
|---|---|---|---|
| GET | `GetEcsLoanSchemes/{brCode}` | `List<DropdownItem>` | Replaces `GetSuretyLoanSchemes`. Returns schemes with `Loan_Type` ∈ `LoanCategoryPolicy.GetCategoriesFor(ECS)` ∩ {10,11,12}. Keep the old route as an alias until the page is switched. |
| GET | `GetSuretyLoanMember/{memberNo}/{brCode}` | `SuretyLoanSanctionDataVM` | existing; add DOR and service months |
| GET | `GetSchemeDefaults/{schemeId:int}/{memId:decimal}/{brCode}` | `SuretyLoanSchemeDefaultsVM` + `Warnings` | now checks §3.4 and §3.5 |
| GET | `GetLedgers/{brCode}` | `List<DropdownItem>` | existing |
| POST | `GetRecoverables` | `EcsRecoverablesRequestVM` → `EcsRecoverablesVM` | replaces the long-path `GetDeductions` GET |
| GET | `ResolveDeductionStatus/{ledId:decimal}/{brCode}` | `string` | for the manual `+` row |
| POST | `CalculateEligibility` | `LoanEligibilityInputVM` → `LoanEligibilityResultVM` | existing |
| POST | `SaveSanction` | `SuretyLoanSanctionSaveVM` → `SanctionSaveResultVM` | create or modify. Context comes from `this.GetUserInfoDto()`, never the body. |
| POST | `DeleteSanction/{sanctionId:decimal}` | `SanctionSaveResultVM` | |
| POST | `ListSanctions` | `EcsSanctionListFilterVM` → `List<EcsSanctionListItemVM>` | |
| GET | `GetSanction/{sanctionId:decimal}/{brCode}` | `EcsSanctionDetailVM` | load for modify |
| GET | `OfficeNote/{sanctionId:decimal}/{reportId:int}/{brCode}` | `application/pdf` | §9 |

- **Server-side ECS gate.** Add a private guard used by every write endpoint, and by the scheme
  endpoint. It resolves the institution for `brCode` (`Gen_Bank_Name.Bank_Type` via the same lookup
  `api/Auth/me` uses) and returns `403` unless it is `InstitutionType.ECS`. This closes the
  "client-side only" gap noted in `CLAUDE.md`.
- **Use the claims `BrCode` for writes.** For write endpoints, take `brCode` from claims and ignore
  the route value, or reject a mismatch.

### 7.4 DTOs (`Infin8.Coapp.Dto`)

Reuse the following as they are, adding the new properties noted:
- `SuretyLoanSanctionSaveVM`: add `decimal LoanSanction_Id`, `bool OverrideStipulatedPeriod`,
  `double FWD`, `List<PendingDepositRowVM> PendingDepositRecoveries`, `List<NonDeductableLoanRowVM>
  NonDeductableRecoveries`.
- `SanctionSaveResultVM`
- `SuretyLoanSanctionDataVM`: add `DateTime? DateOfRetirement`, `int ServiceMonths`.
- `SuretyLoanSchemeDefaultsVM`: add `List<SanctionWarningVM> Warnings`, `double Roi`, `double Pi`.
- `LoanEligibilityInputVM` / `LoanEligibilityResultVM`: add `double DepositDemand`.
- `DeductableLoanRowVM`

New:
```csharp
public class SanctionWarningVM      { public string Code { get; set; } = "";   // DUPLICATE | ALREADY_SAVED | STIPULATED_PERIOD
                                      public bool Blocking { get; set; } public string Message { get; set; } = ""; }
public class EcsRecoverablesRequestVM { public decimal MemId, SuretyMemId; public int SchemeId; public double LoanAmount;
                                      public int PeriodOfLoan; public double Roi; public int InstType; public double BasicPay;
                                      public DateTime FirstDueDate; }
public class EcsRecoverablesVM      { public List<DeductableLoanRowVM> Recoverables = new();
                                      public List<NonDeductableLoanRowVM> NonDeductable = new();
                                      public List<PendingDepositRowVM> PendingDeposits = new();
                                      public List<SuretyOverdueRowVM> SuretyOverdues = new();
                                      public double MemberSC, SuretySC, MemberTD, MemberFWD, RequiredSC;
                                      public int InstalmentAmount, NoOfInstalmentsCompleted, NoOfInstalmentsRecovered;
                                      public double ExistingDemand, NonDeductableOS, NonDeductableOSWithinLimit, DepositDemand; }
public class NonDeductableLoanRowVM { LoanId, LoanNo, SchemeId, Purpose, LoanOS, IntBal, PIBal, IntCalc, PICalc,
                                      PrlLedId, IntLedId, PILedId, LoanSlNo, DemandAmount, AdoptLoanLimit, MatchSC,
                                      PrlRecovery, IntRecovery, PIRecovery /* editable */ }
public class PendingDepositRowVM    { DMId, MemId, LedId, LedName, PendingAmount, CalcAmount, ODAmount, RecoveryAmount /* editable */ }
public class SuretyOverdueRowVM     { string LoanName; double OverdueAmount; }
public class EcsSanctionListFilterVM { string Mode /* ALL|ID|MEM|DATE */; decimal? SanctionId; decimal? MemId; DateTime? From, To; }
public class EcsSanctionListItemVM  { LoanSanction_Id, Mem_Id, SuretyMem_Id, MemberNo, PerNo, MemberName,
                                      SanctionDate, LoanAmount, Scheme_Id, SchemeName }
public class EcsSanctionDetailVM    { SuretyLoanSanctionSaveVM Sanction; SuretyLoanSanctionDataVM Member;
                                      List<DeductableLoanRowVM> Recoverables; LoanEligibilityResultVM Eligibility;
                                      List<SuretyOverdueRowVM> SuretyOverdues; }
public class OfficeNoteRowVM        { /* mirrors TSIS rptOfficeNote — see §9 */ }
```
The field lists above are shorthand. Write them as normal `{ get; set; }` properties. Money is
`double` and IDs are `decimal`, per `CLAUDE.md`.

---

## 8. UI design — `Pages/Products/ECSLoanSanction.razor`

Rename `SuretyLoanSanction.razor` to `ECSLoanSanction.razor` (`git mv`, so the history is kept).

```razor
@page "/ecs-loan-sanction"
@page "/loan-sanction"            @* back-compat with the existing URL *@
@rendermode InteractiveWebAssembly
@attribute [Authorize(Roles = "Admin,Maker,Checker")]
@inject HttpClient _httpClient
@inject AuthenticationStateProvider AuthenticationStateProvider
@inject IJSRuntime JSRuntime
```

- **Gate.** Keep the existing `isReady` and `institution != InstitutionType.ECS` guard unchanged.
- **Context.** `GetUserInfoDtoFromASP` supplies `brCode`, `yrId` and `CurrentDate`. The sanction date
  is the business day. Show it read-only, not as a date picker; WinForms let the user pick it.
- **Layout** uses Bootstrap 5.3 nav-tabs (no third-party UI library). The header strip shows Member
  No, Name, Surety No/Name, Sanction Id (read-only) and the Sanction Date.

| Tab | Contents | Actions |
|---|---|---|
| 1. Member & Pay | DOB, DOR, Service months; Basic, Gross, Society deduction, Total deductions, Net salary (all typed in) | Optional *Fetch from payroll* (§13) |
| 2. Loan Details & Eligibility | Scheme select, scheme facts table, Loan amount, Period, ROI, PI, Instalment, First due date, This/Max loan limit, Instalments completed/recovered; Member assets (SC, TD, FWD); Surety SC + surety overdue list (red "Surety has overdue" badge); Eligibility panel (25×GP, 50 %, 25 %, Demand, Existing demand, Other deductions, Net pay, verdict + reasons) | *Get Recoverables*, *Display Demand* (modal), *Check Eligibility* |
| 3. Recoverables | Grid: Loan No, Status, Ledger, Outstanding, Calc amt, OD amt, ✕ (manual rows only). Below the grid: ledger select + amount + **Add**. Totals: Total recoverable, **Loan balance (net disbursable)**. | Add / remove manual rows |
| 4. Non-Deductable Loans | Grid with editable Prl/Int/PI recovery per loan; Total OS; OS within loan limit | **Add to Recovery** (moves rows to tab 3 and reduces Existing Demand) |
| 5. Pending Deposits | Grid with editable recovery amount | **Get Pending Deposits**, **Add to Recovery** (status `D`) |
| 6. Sanctions | Filter: All / Sanction Id / Member No / Date range; grid of `EcsSanctionListItemVM` | Row click → `GetSanction` → loads every tab and sets mode = Modify |

**Footer buttons**

| Button | Enabled when | Calls |
|---|---|---|
| **Save Sanction** / **Modify Sanction** (label follows the mode) | Eligibility verdict OK (or the scheme does not adopt eligibility) and loan balance > 0 | `SaveSanction` |
| **Delete Sanction** | Mode = Modify | Confirm dialog → `DeleteSanction` |
| **Print Office Note** | A saved sanction id exists | Opens the PDF from `OfficeNote/...` |
| **New** | Always | Resets the page |

**Behaviour**

- **Scheme change.** Call `GetSchemeDefaults`.
  - A `Blocking` warning shows an alert and clears the scheme.
  - `STIPULATED_PERIOD` shows a JS `confirm`. If the user accepts, set `OverrideStipulatedPeriod = true`.
- **Stale recoverables.** Changing Loan amount, Period or Scheme marks the recoverables stale and
  disables Save until *Get Recoverables* runs again. This replaces the WinForms `_Leave` chaining.
- **Server rejection.** Show `SanctionSaveResultVM.Message` in an alert. On success, show the new
  sanction id and offer *Print Office Note*.
- **Navigation.** The page does not move between tabs on its own, unlike WinForms. Tabs show a
  ✓ once they are complete.
- **Keyboard.** Enter moves focus to the next field, matching WinForms `Ctrl_Up`. This is optional,
  via `wwwroot/js/`.

---

## 9. Report — Office Note

Port `TSISWF/TSIS/Reports/OfficeNote.rdlc` to `Infin8.Coapp.UI/Reports/OfficeNote.rdlc`.

- **DataSet name:** `Ds_OfficeNote`. It must match the name passed in C#.
- **Parameters:**

  | Parameter | Value |
  |---|---|
  | `paramSocietyName` | the society name |
  | `paramReportHeader` | `"OFFICE NOTE - " + SchemeName` |
  | `paramNotes` | `"Received Loan of Rs.{LoanAmount}/- {RupeesInWords(LoanAmount − TotalReceiptAmount)} by way of cheque Rs.{LoanAmount − TotalReceiptAmount}/- and by adjustment Rs.{TotalReceiptAmount}/-"` |
  | `ParamFirstSignature`, `ParamSecondSignature`, `ParamThirdSignature` | from `Reports_Master.FirstSignature` / `SecondSignature` / `ThirdSignature` |

- **`OfficeNoteRowVM`** mirrors TSIS `rptOfficeNote`:
  - From the sanction header: LoanSanction_Id, SanctionDate, LoanAmount, PeriodOfLoan,
    InstalmentAmount, TotalReceiptAmount, IsMemberBecomeDefaulter, IsSurityBecomeDefaulter, GrossPay,
    MemberShareCapitalAmount, SurityShareCapitalAmount, NoOfInstalmentsCompleted,
    NoOfInstalmentsRecovered.
  - From the member: memberNo, PerNo, memberName.
  - From the surety: SuretymemberNo, SuretyPerNo, SuretymemberName.
  - One row per line: SlNo, ReceiptDescription, ReceiptItem, PaymentDescription, PaymentItem.
- **Query.** `loan_sanction ⟕ mem_master (member) ⟕ loan_sanction_trn (non-deleted) ⟕ mem_master
  (surety)`, all filtered by `brcode` and ordered by `slno`. Clamp `PaymentItem` to 0 when SlNo = 6
  and the value is negative.
- **Controller.** Copy the pattern in `ReportsMemberController`: look up `Reports_Master` by
  `reportId`, call `MicrosoftReport.CreateLocalReport`, and return the PDF. Build the path with
  `Path.Combine(ContentRootPath, "Reports", report.ReportFileName)`; the existing controllers use
  `"\\Reports\\"`, which breaks on macOS and Linux.
- **Database row.** Insert into `reports_master`: ReportName `Office Note`, ReportFileName
  `OfficeNote.rdlc`, the signatures, and `brcode`. The id is MAX+1.
- **Rupees in words.** Needs a helper. `Infin8.Coapp.Utility` has none today, so port TSIS
  `Utilities.RupeesInWords` into it.

---

## 10. Navigation menu (optional)

Menu gating for ECS is still deferred under spec 02, so by default the page is reachable by URL
only. To expose it, insert the rows below. `Form_Name` slugifies to `ecs-loan-sanction`:

```sql
-- illustrative; take ids via MAX+1 and match existing Menu_Sub for Loans
INSERT INTO menu_forms (form_id, sub_id, form_name, voc_status, brcode, ...)
VALUES ((SELECT COALESCE(MAX(form_id),0)+1 FROM menu_forms), <loans sub_id>, 'ECS Loan Sanction', 'V', '<brcode>', ...);
```
Remember that the menu filters on role only. PCARDB users will see the item and then hit the ECS
gate.

---

## 11. Build order (checklist)

- [ ] 0. Run `dotnet build Infin8.Coapp.sln` and record the baseline warning count (about 199).
- [ ] 1. Repository: rename the repository to `EcsLoanSanctionRepository`; add the §7.1 methods;
      change `PersistSanctionAsync` to `AddSanctionAsync` (no internal `SaveChanges`).
- [ ] 2. UnitOfWork: update the interface property, the backing field and the lazy property.
- [ ] 3. Utility: add `MonthsBetween` and `FirstOfNextMonth`, plus `RupeesInWords` if it is missing.
- [ ] 4. DTOs: extend the existing VMs and add the new VMs (§7.4).
- [ ] 5. Handler: `EcsLoanSanctionHandler` with save, modify, delete, list and get (§7.2); reconcile
      `LoanEligibilityHandler` (§3.3).
- [ ] 6. DI: update `Infin8.Coapp.UI/Program.cs`.
- [ ] 7. Controller: new and changed endpoints, the server ECS gate, `brCode` from claims (§7.3).
- [ ] 8. Page: rename to `ECSLoanSanction.razor`, then add the tabs, list, modify, delete, print and
      the stale-recoverables guard (§8).
- [ ] 9. Report: `OfficeNote.rdlc`, the `reports_master` row, and the endpoint (§9).
- [ ] 10. (Optional) Menu rows (§10).
- [ ] 11. Build again: no new errors, and no new warnings in the touched files.
- [ ] 12. Run the manual test scenarios (§12).

---

## 12. Manual test scenarios

There are no automated tests. Run `dotnet run --project Infin8.Coapp.UI` and use the page at
`https://localhost:7073/ecs-loan-sanction`.

| # | Scenario | Expected |
|---|---|---|
| 1 | Log in as a PCARDB society | "ECS only" warning; no scheme call. A direct `POST SaveSanction` returns 403. |
| 2 | ECS, valid member, scheme of type 10 | Defaults, ROI and PI filled; scheme facts table shown |
| 3 | Scheme dropdown | Only Loan_Type 10, 11 and 12 schemes are listed |
| 4 | Same member, scheme and date sanctioned twice | Second attempt blocked: "already sanctioned" |
| 5 | Last loan of the scheme within `MinInstalmentsForLoan` months | Confirm dialog. Cancel clears the scheme; OK allows the save, and the override is recorded. |
| 6 | Scheme with `IssurityMemberRequired = 1`, member has no surety | Save blocked: "Surety is required" |
| 7 | SC below the required amount, MatchSC = 1 | `MS` and `SS` rows with the correct shortfall amounts |
| 8 | Old deductable loan outstanding | P / I / PI rows; instalments completed and recovered filled |
| 9 | Demand + existing > 50 %, or net < 25 %, with AdoptLoanEligibility = 1 | Save disabled, reasons shown. With the flag at 0 the save is allowed. |
| 10 | Recoverables ≥ loan amount | "Member does not have adequate loan balance"; save blocked |
| 11 | Non-deductable loan → Add to Recovery | Row moves to recoverables; Existing Demand drops; eligibility is recomputed |
| 12 | Pending deposit → Add to Recovery | `D` row with DM_Id |
| 13 | Manual deduction on a deposit / sundry / share-capital / general ledger | Status `D` / `DTo` / `MS` / `G` respectively |
| 14 | Save | New id shown. Database: header status `E`, lines 1..n plus padding up to 6, all `brcode` set |
| 15 | List → load → change amount → Modify | New id; old header and lines `_delete = true`; the list shows only the new one |
| 16 | Post a `loan_trn` on a recovered loan, then Modify or Delete | Blocked: "Subsequent transaction made in loan account …" |
| 17 | Delete | Header and lines soft-deleted; sanction gone from the list |
| 18 | Print Office Note | PDF with member, surety, payment lines 1–6, notes and signatures |
| 19 | Change the loan amount after Get Recoverables | Save disabled until recoverables are fetched again |

---

## 13. Open questions (need a business decision)

1. **Stipulated period.** Should it be a warning the user can confirm (as in TSIS, and this spec's
   default) or a hard reject (the current Blazor behaviour)?
2. **Service-months rule.** This spec proposes `ServiceMonths ≥ PeriodOfLoan` in place of the current
   comparison against `TSISRetirementAgeInMonths`. Confirm. Also confirm the unit of
   `mem_retirementage.retirementage`.
3. **Surety share capital raising the member's requirement** (§4 #9). Is this intended?
4. **Loan-limit enforcement** (§3.11). Should it be enforced when `AdoptLoanLimit = 1`? TSIS only
   displayed it.
5. **Payroll prefill.** Should the existing `Pay_Slip` prefill stay as an optional button? Default
   is manual entry only.
6. **Sanction authority / approval reference** (preliminary spec §8). Do we need a resolution no. and
   date on the sanction? That would mean additive columns.
7. **Global MAX+1 IDs.** `loan_sanction` IDs are global, not per-branch. Is that acceptable?
   Concurrent saves can collide; the transaction narrows the window but does not remove it.
8. **Existing `"S"` rows.** Do sanctions already saved by the current page with status `"S"` need
   migrating to `"E"`?
