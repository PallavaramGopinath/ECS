# 04 — Education Loan (ECS-exclusive)

> See [02-loan-module-design.md](./02-loan-module-design.md). Per [07-loan-type-society-type.md](./07-loan-type-society-type.md),
> this product is a `Loan_Schemes` row under `Loan_Type 1` (`LoanCategory.ThriftCreditLoans`), not a
> separate loan type; `11` is retired.

**Status:** stub — to be specified.

## Foundation already in place
- Stored as `Loan_Type = 1` (Thrift & Credit loans); identified by its `Scheme_Id`.
- `Loan_Type 1` is visible only to ECS sessions via `LoanCategoryPolicy`.
- Product-specific behaviour comes from rule columns on `loan_schemes` (spec 07 §3.4), never from
  the scheme's id or name. When specifying this product, list its rules as a table of
  `loan_schemes` column → value, marking any column that has to be added.
- Creatable as a `Loan_Schemes` product through the existing LoanProducts admin UI.

## To decide when specifying this loan type
- Engine reuse vs. bespoke (e.g. tranche/semester-wise disbursement, moratorium during study period).
- Eligibility and documentation (course, institution, co-borrower).
- Repayment schedule, ROI rules, interest treatment during moratorium.
- UI fields for `Loan_Type == 11`; new pages vs. reused components.
- MIS/reports.
