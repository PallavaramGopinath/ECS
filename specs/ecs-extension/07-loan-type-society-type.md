# 07 — Loan Type and Society Type Clarification

> **Supersedes** parts of [01-institution-type.md](./01-institution-type.md) (the `Bank_Type` value
> mapping) and [02-loan-module-design.md](./02-loan-module-design.md) (the `Loan_Type` 10 / 11 / 12
> decision). Affects [03](./03-surety-loan.md)–[06](./06-ecs-loan-sanction.md).
>
> **Numbering note:** spec 06 forward-references `07-ecs-loan-disbursement.md`. That spec should now
> be numbered `08`.

---

## 1. Background

This project is one codebase serving three kinds of cooperative credit society. Each lends to a
different set of borrowers, so each offers different loans.

| Society | Lends to | Typical loans |
|---|---|---|
| **Cooperative Primary Agriculture and Rural Development Bank** (PCARDB) | Farmers and people in the bank's area of operation | Long-term loans: farm sector, non-farm sector, rural housing, small road transport operations (SRTO), jewel loans |
| **Cooperative Housing Society** | The general public in the society's area of operation | Construction, purchase and renovation of houses |
| **Cooperative Thrift & Credit Society** (ECS) | Employees of the organisation the society serves | Surety loans, education loans, draught loans, etc. |

Two integer codes classify this:

- **`Gen_Bank_Name.Bank_Type`** says what kind of society a branch is.
- **`Loan_Type`** (on `Loan_Master`, `Loan_Schemes` and `Loan_Schemes_Group`) says which lending
  sector a loan belongs to.

---

## 2. Society type — `Gen_Bank_Name.Bank_Type`

### 2.1 Authoritative mapping

| `Bank_Type` | Society |
|---|---|
| `1` | Cooperative Primary Agriculture and Rural Development Bank (PCARDB) |
| `2` | Cooperative Housing Society |
| `3` | Cooperative Thrift & Credit Society (ECS) |
| `0` / other | Unknown. Treated as PCARDB so that existing PCARDB visibility never regresses. |

`Bank_Type` stays in the `gen_bank_name` table, one row per `BrCode`. The schema does not change.

### 2.2 Change from spec 01

Spec 01 and the code today use **`2` = ECS**. That is wrong. ECS is **`3`**, and `2` is now
Cooperative Housing.

Code impact (`Infin8.Coapp.Utility/InstitutionType.cs`):

```csharp
public enum InstitutionType
{
    Unknown = 0,
    PCARDB  = 1,   // Cooperative Primary Agriculture and Rural Development Bank
    Housing = 2,   // Cooperative Housing Society
    ECS     = 3    // Cooperative Thrift & Credit Society
}
```

`ToInstitutionType()` needs no change. It already maps any undefined value to `Unknown`.

**Data impact:** any ECS society whose `gen_bank_name.bank_type` is currently `2` must be updated to
`3`. Do this **in the same release** as the enum change. If it is skipped, an ECS branch would be
read as a Housing society and lose access to its ECS pages.

```sql
-- Run per ECS database, after confirming the rows really are Thrift & Credit societies.
SELECT brcode, bank_type FROM gen_bank_name;
UPDATE gen_bank_name SET bank_type = 3 WHERE bank_type = 2 AND brcode IN (/* ECS branches */);
```

PCARDB rows (`bank_type = 1`) are not touched.

---

## 3. Loan type — `Loan_Type`

### 3.1 Authoritative mapping

| `Loan_Type` | Loan | Offered by |
|---|---|---|
| `1` | Thrift & Credit Society loans: surety, education, draught, etc. | ECS (`Bank_Type 3`) only |
| `2` | Jewel loan | All three |
| `3` | Loan on fixed deposit | Shared |
| `4` | Loan on recurring deposit | Shared |
| `5` | Loans to staff | Shared |
| `6` | **Housing loans** (Housing Society) **or** **farm sector loans** (PCARDB). See §3.2. | Housing (`2`), PCARDB (`1`) |
| `7` | Non-farm sector loans | PCARDB |
| `8` | Rural housing loans | PCARDB |
| `9` | Small road transport operations (SRTO) loans | PCARDB |

Values `10`, `11` and `12` are **retired** (see §3.3).

### 3.2 `Loan_Type 6` is shared, and `Bank_Type` decides what it means

`Loan_Type = 6` means two different things:

- In a **Housing Society** (`Bank_Type 2`) it is a housing loan (construction, purchase or
  renovation).
- In a **PCARDB** (`Bank_Type 1`) it is a farm sector loan.

The two never mix inside one database or branch, so the stored value needs no change. Anything that
**displays or reports** `Loan_Type 6` must read the label from the society type and must not
hard-code it:

| Context | Label for `Loan_Type 6` |
|---|---|
| PCARDB | Farm Sector Loans |
| Housing | Housing Loans |

### 3.3 ECS loans all use `Loan_Type 1` and differ by scheme

Every ECS loan product is stored as **`Loan_Type = 1`**. That includes surety, education, draught
and any future ECS product. The products are told apart by **`Scheme_Id`** in `Loan_Schemes`, not by
`Loan_Type`.

This reverses spec 02's decision to add `Loan_Type` values `10` (Surety), `11` (Education) and `12`
(Draught). Those values are retired.

Consequences:

- A new ECS loan product is a **new `Loan_Schemes` row** with `Loan_Type = 1`. No code change and no
  new enum value is needed.
- Rules that apply to only one product (for example, education-loan student details or draught-loan
  limits) cannot key off `Loan_Type`. They are driven by rule columns on the scheme (§3.4).
- Existing code that already assumes ECS loans are `Loan_Type 1` becomes correct, for example
  `ECSDemandCalculationRepository` (`scheme.Loan_Type == 1`) and `LoanTrnRepository` (around
  line 2736).

### 3.4 Product-specific rules come from scheme columns

**Decision (Q3, option a):** whatever makes Surety, Education and Draught behave differently is
stored as **rule columns on the `loan_schemes` row**. Code reads those columns. It never checks which
product a scheme is.

**Rules:**

1. **Never branch on the product's identity.** Code must not test `Scheme_Id`, `Scheme_Name` or
   `Grp_Id` to decide behaviour, as in `if (scheme.Scheme_Name == "Education Loan")`. Scheme ids
   differ per branch and database (they are MAX+1), and names are free text.
2. **Use the existing rule columns first.** `loan_schemes` already carries the rules the ECS sanction
   uses, and `EcsLoanSanctionHandler` already reads them:

   | Column | Rule |
   |---|---|
   | `MaximumLoanAmount` | Upper limit for one loan under this scheme |
   | `MaximumPrincipalPeriod` / `MaximumInterestPeriod` | Maximum repayment period |
   | `Inst_Type` | Instalment method |
   | `AdoptLoanEligibility` | Apply the pay-based eligibility calculation (1 = yes) |
   | `AdoptLoanLimit` | Apply the member's overall loan limit from `Map_General` (1 = yes) |
   | `MatchShareCapital` | Share-capital matching rule (0 / 1 / 2) |
   | `IssurityMemberRequired` | A surety member is mandatory (1 = yes) |
   | `DeductPreviousLoans` | Recover the outstanding on previous loans at sanction |
   | `IsDeductBalances` | Deduct other recoverable balances at sanction |
   | `Is_Member` / `Is_Staff` | Who may borrow under the scheme |

   For example, a Surety Loan scheme sets `IssurityMemberRequired = 1`. An Education or Draught
   scheme sets it to `0` and gets its own `MaximumLoanAmount` and period.
3. **Add a new column only when no existing column covers a rule.** When specs 03–05 identify a rule
   that nothing above expresses, add a column to `loan_schemes`:
   - It must be additive and nullable, or have a default that keeps today's behaviour, so PCARDB
     schemes are unaffected (PCARDB is live).
   - Name it after the **rule**, not the product. For example, use `IsStudentDetailsRequired`, not
     `IsEducationLoan`.
   - Add it to the `Loan_Schemes` POCO and apply the DDL out of band. There are no EF migrations.
   - Expose it on the Loan Products create/edit screen so the society can set it per scheme.
4. **Extra data a product needs** (for example, an education loan's student or course details)
   belongs in a child table keyed by the sanction or loan id. That child table is only filled when
   the scheme's rule column says so. Specs 03–05 define these.

Specs 03–05 should each list their product's rules as a table of `loan_schemes` column → value,
marking any column that has to be added.

---

## 4. Visibility policy

`LoanCategoryPolicy.IsAllowed(category, institution)` becomes a three-way policy.

| `Loan_Type` | PCARDB (`1`) | Housing (`2`) | ECS (`3`) |
|---|:-:|:-:|:-:|
| 1 Thrift & Credit loans | — | — | ✔ |
| 2 Jewel | ✔ | ✔ | ✔ |
| 3 Loan on FD | ✔ | ✔ | ✔ |
| 4 Loan on RD | ✔ | ✔ | ✔ |
| 5 Staff loans | ✔ | ✔ | ✔ |
| 6 Farm sector / Housing | ✔ (Farm) | ✔ (Housing) | — |
| 7 Non-farm sector | ✔ | — | — |
| 8 Rural housing | ✔ | — | — |
| 9 SRTO | ✔ | — | — |

`Unknown` keeps the PCARDB column.

Decisions behind the table (project owner):

- **PCARDB does not offer `Loan_Type 1`** (Q1). Thrift & Credit loans are ECS-only. This is a
  change from today's policy, which shows `Loan_Type 1` to PCARDB. Run the data check in §5.2
  item 4 before release.
- **All three society types offer jewel loans (`Loan_Type 2`)** (Q2), as well as loans on FD (`3`),
  loans on RD (`4`) and staff loans (`5`).

### 4.1 Implementation

`LoanCategoryPolicy` today has two "exclusive" sets and a two-way rule: if the society is ECS, hide
the PCARDB-only loans; otherwise, hide the ECS-only loans. That rule cannot express a third society
type. A Housing branch would fall into "otherwise" and see loan types 7, 8 and 9. Once 10–12 are
retired, the ECS-only set would also be empty.

Replace it with an explicit allow-list per society type. Each **key** is a society type
(`Bank_Type`). Each **value** is the set of `Loan_Type` values that society may offer:

```csharp
private static readonly Dictionary<InstitutionType, HashSet<LoanCategory>> Allowed = new()
{
    // Bank_Type 1: Cooperative Primary Agriculture and Rural Development Bank
    [InstitutionType.PCARDB] = new()
    {
        LoanCategory.JewelLoans,                 // 2
        LoanCategory.LoansOnFixedDeposits,       // 3
        LoanCategory.LoansOnRecurringDeposits,   // 4
        LoanCategory.StaffLoans,                 // 5
        LoanCategory.FarmSectorOrHousingLoans,   // 6 (Farm Sector)
        LoanCategory.NonFarmSectorLoans,         // 7
        LoanCategory.RuralHousing,               // 8
        LoanCategory.SmallRoadTransportLoans     // 9
    },

    // Bank_Type 2: Cooperative Housing Society
    [InstitutionType.Housing] = new()
    {
        LoanCategory.JewelLoans,                 // 2
        LoanCategory.LoansOnFixedDeposits,       // 3
        LoanCategory.LoansOnRecurringDeposits,   // 4
        LoanCategory.StaffLoans,                 // 5
        LoanCategory.FarmSectorOrHousingLoans    // 6 (Housing)
    },

    // Bank_Type 3: Cooperative Thrift & Credit Society
    [InstitutionType.ECS] = new()
    {
        LoanCategory.ThriftCreditLoans,          // 1 (surety, education, draught … by Scheme_Id)
        LoanCategory.JewelLoans,                 // 2
        LoanCategory.LoansOnFixedDeposits,       // 3
        LoanCategory.LoansOnRecurringDeposits,   // 4
        LoanCategory.StaffLoans                  // 5
    }
};

public static bool IsAllowed(LoanCategory category, InstitutionType institution)
{
    // Unknown (bank_type 0 / blank / unrecognised) is treated as PCARDB so existing
    // PCARDB visibility never regresses.
    var key = Allowed.ContainsKey(institution) ? institution : InstitutionType.PCARDB;
    return Allowed[key].Contains(category);
}
```

---

## 5. Required changes

### 5.1 Code

| File | Change |
|---|---|
| `Infin8.Coapp.Utility/InstitutionType.cs` | Add `Housing = 2` and change `ECS = 2` to `ECS = 3`. Update the doc comment. |
| `Infin8.Coapp.Utility/LoanCategory.cs` | Remove `SuretyLoan = 10`, `EducationLoan = 11` and `DraughtLoan = 12` (or mark them `[Obsolete]` until the data migration in §5.2 has run). Rename `EmployeeSocietyLoans = 1` to `ThriftCreditLoans = 1`; the value does not change. Rename `FarmSectorLoans = 6` to a neutral name such as `FarmSectorOrHousingLoans`, or document the double meaning. Update the doc comment. |
| `Infin8.Coapp.Utility/LoanCategoryPolicy.cs` | Replace the two exclusive sets with the per-institution allow-list in §4. |
| `Infin8.Coapp.BusinessLogic/Concrete/EcsLoanSanctionHandler.cs` | `EcsSanctionLoanTypes` becomes `{ (int)LoanCategory.ThriftCreditLoans }` (that is, `1`). `IsEcsScheme` stays as it is in shape. |
| `Infin8.Coapp.UI/Controllers/LoanSanctionController.cs` | Change the summary on `GetEcsLoanSchemes` from *"Schemes with Loan_Type 10 / 11 / 12 that the ECS policy allows"* to *"Schemes with Loan_Type 1 (Thrift & Credit loans; surety, education, draught, etc. differ by Scheme_Id) that the ECS policy allows."* `IsEcsAsync` needs no code change once the enum is fixed, because it compares to `InstitutionType.ECS`. |
| `Infin8.Coapp.UI.Client/Pages/Products/LoanProductsCreate.razor` | Delete the commented-out 10 / 11 / 12 entries in `GetLoanCategories()`. Label `Loan_Type 1` "Thrift & Credit Society Loans". Label `Loan_Type 6` by institution (§3.2). |
| `Infin8.Coapp.UI.Client/Pages/Products/Loanproducts.razor` | Make the `Loan_Type 6` badge and label institution-aware. |
| `Infin8.Coapp.UI.Client/Pages/Products/ECSLoanSanction.razor` | No logic change; its gate compares to `InstitutionType.ECS`. Re-test it after the enum change. |

Also review every remaining hard-coded `Loan_Type == n` check (in `ReportsJewelLoanRepository`,
`LoanTrnRepository`, `ReportsAuditController` and the two Products pages) for assumptions about
`Bank_Type`. Today only `Loan_Type 6` changes meaning between societies.

### 5.2 Data

1. `gen_bank_name.bank_type`: change ECS rows from `2` to `3` (§2.2).
2. Retired loan types: move any rows using `10`, `11` or `12` to `1`.

   ```sql
   -- Check first; expected to be empty or only test data.
   SELECT 'loan_schemes' t, loan_type, count(*) FROM loan_schemes WHERE loan_type IN (10,11,12) GROUP BY loan_type
   UNION ALL SELECT 'loan_schemes_group', loan_type, count(*) FROM loan_schemes_group WHERE loan_type IN (10,11,12) GROUP BY loan_type
   UNION ALL SELECT 'loan_master', loan_type, count(*) FROM loan_master WHERE loan_type IN (10,11,12) GROUP BY loan_type;

   UPDATE loan_schemes       SET loan_type = 1 WHERE loan_type IN (10,11,12);
   UPDATE loan_schemes_group SET loan_type = 1 WHERE loan_type IN (10,11,12);
   UPDATE loan_master        SET loan_type = 1 WHERE loan_type IN (10,11,12);
   ```

   Also check `loan_sanction` and any other table that carries `loan_type`.
3. Each ECS product (Surety, Education, Draught, …) must exist as its own `loan_schemes` row with
   `loan_type = 1`.

4. PCARDB check: confirm no PCARDB database has live `Loan_Type 1` schemes or loans. Once PCARDB
   stops showing `Loan_Type 1` (§4), any such rows would disappear from the Loan Products screens.

   ```sql
   -- Run on each PCARDB database. Expected: no rows.
   SELECT 'loan_schemes' t, count(*) FROM loan_schemes WHERE loan_type = 1 AND NOT scheme_delete
   UNION ALL SELECT 'loan_master', count(*) FROM loan_master WHERE loan_type = 1 AND NOT loan_delete;
   ```

   If rows are found, decide with the project owner how to reclassify them before release.

The data changes affect ECS data only. PCARDB data (`bank_type 1`, `loan_type 2–9`) is unchanged.

### 5.3 Documentation

- **01-institution-type.md**: replace the value-mapping table with §2.1.
- **02-loan-module-design.md**: mark the 10 / 11 / 12 decision as superseded by this spec.
- **03 / 04 / 05**: Surety, Education and Draught are schemes under `Loan_Type 1`, not separate
  loan types. Each spec lists its rules as `loan_schemes` column values (§3.4).
- **06-ecs-loan-sanction.md**: the scheme filter is `Loan_Type = 1`. Rename the forward reference to
  the disbursement spec to `08`.
- **CLAUDE.md**, *ECS extension* section: update the `Bank_Type` mapping and the `Loan_Type`
  paragraph. Its line *"10/11/12 are Surety / Education / Draught, ECS-exclusive"* becomes
  *"ECS loans are all `Loan_Type 1`, differentiated by `Scheme_Id`; 10–12 retired."*

---

## 6. Open questions

| # | Question | Default proposed until answered |
|---|---|---|
| Q1 | Should PCARDB still see `Loan_Type 1`? | **Resolved: no.** `Loan_Type 1` is ECS-only (§4). Check PCARDB data first (§5.2 item 4). |
| Q2 | Do Housing Societies offer jewel loans (`2`)? Are FD (`3`), RD (`4`) and staff (`5`) loans shared by all three society types? | **Resolved: yes.** All three society types offer `2`, `3`, `4` and `5` (§4). |
| Q3 | How does code tell Surety, Education and Draught apart when product-specific rules apply? Options: (a) per-scheme rule columns in `loan_schemes` (eligibility multiple, surety count, max amount, and so on); (b) a new nullable sub-type column on `loan_schemes`; (c) configuration only, with no product-specific code. | **Resolved: (a).** Per-scheme rule columns in `loan_schemes`; no new discriminator (§3.4). |
| Q4 | Are there live Housing Society deployments yet, or is `Bank_Type 2` reserved for the future? | Reserved. Add the enum value and policy now; build no Housing-specific pages yet. |
| Q5 | Which ECS databases currently have `bank_type = 2` and need the update in §2.2? | Check every ECS database before release. |

---

## 7. Verification (manual; there is no test project)

1. Log in to an ECS branch with `bank_type = 3`. `/ecs-loan-sanction` loads with no "ECS only"
   warning. The scheme dropdown lists the `Loan_Type 1` schemes (Surety, Education, Draught).
   Saving a sanction succeeds, and the server-side `IsEcsAsync` gate passes.
2. Log in to an ECS branch still set to `bank_type = 2`. It is treated as Housing, and the ECS pages
   are blocked. This confirms the data update is required.
3. Log in to a PCARDB branch with `bank_type = 1`. Loan Products shows Jewel, Loan on FD, Loan on
   RD, Staff, Farm Sector, Non-Farm Sector, Rural Housing and SRTO (2–9). It does not show Thrift &
   Credit loans (1). `Loan_Type 6` is labelled "Farm Sector Loans".
4. Log in to a branch with `bank_type = 2`. Loan Products shows only the Housing set (Jewel, Loan
   on FD, Loan on RD, Staff, Housing Loans), and `Loan_Type 6` is labelled "Housing Loans".
5. Log in to a branch with `bank_type = 3`. Loan Products shows Thrift & Credit loans, Jewel, Loan
   on FD, Loan on RD and Staff, and nothing from 6–9.
6. After the migration, `SELECT … WHERE loan_type IN (10,11,12)` returns no rows in any table.
